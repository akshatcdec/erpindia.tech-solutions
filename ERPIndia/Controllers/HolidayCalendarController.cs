using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class Holiday
    {
        public Guid HolidayID { get; set; }
        public int SortOrder { get; set; }
        public DateTime HolidayDate { get; set; }
        public string HolidayName { get; set; }
        public string HolidayType { get; set; }
        public Guid SessionID { get; set; }
        public Guid TenantID { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class HolidayCalendarController : BaseController
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        public ActionResult ManageHolidays()
        {
            // Retrieve values from session
            Guid sessionId = CurrentSessionID;
            int sessionYear = CurrentSessionYear;

            // Pass to ViewBag for use in the view
            ViewBag.SessionId = sessionId;
            ViewBag.SessionYear = sessionYear;

            return View();
        }

        [HttpPost]
        public JsonResult GetAllHolidays(string sessionId, int sessionYear, bool checkDuplicate = false, string holidayName = null, string holidayDate = null)
        {
            try
            {
                // Get tenant info
                Guid parsedSessionId;

                if (!Guid.TryParse(sessionId, out parsedSessionId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid session ID",
                        draw = "0",
                        recordsFiltered = 0,
                        recordsTotal = 0,
                        data = new List<Holiday>()
                    });
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(holidayName))
                    {
                        string sql1 = @"SELECT * FROM HolidayCalendar 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND HolidayName = @HolidayName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            HolidayName = holidayName
                        };

                        connection.Open();
                        var holidays = connection.Query<Holiday>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = holidays });
                    }

                    // For DataTables server-side processing
                    var request = HttpContext.Request;
                    var draw = request.Form["draw"];
                    var start = Convert.ToInt32(request.Form["start"] ?? "0");
                    var length = Convert.ToInt32(request.Form["length"] ?? "0");
                    var sortColumnIndex = request.Form["order[0][column]"];
                    var sortColumn = request.Form[$"columns[{sortColumnIndex}][name]"];
                    var sortColumnDirection = request.Form["order[0][dir]"];
                    var searchValue = request.Form["search[value]"];

                    // Base query
                    var sql = new StringBuilder();
                    sql.Append("WITH HolidayData AS ( ");
                    sql.Append("SELECT *, ROW_NUMBER() OVER (");

                    // Handle sorting
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        sql.Append($"ORDER BY {sortColumn} {sortColumnDirection}");
                    }
                    else
                    {
                        // Default sorting by HolidayDate
                        sql.Append("ORDER BY HolidayDate");
                    }

                    sql.Append(") AS RowNum ");
                    sql.Append("FROM HolidayCalendar WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (HolidayName LIKE @Search OR HolidayType LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM HolidayData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM HolidayCalendar 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (HolidayName LIKE @Search OR HolidayType LIKE @Search)");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@TenantCode", CurrentTenantCode);
                    parameters.Add("@SessionID", parsedSessionId);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Execute queries
                    var rawData = connection.Query<Holiday>(sql.ToString(), parameters).ToList();

                    // Format dates for proper JSON serialization
                    var data = rawData.Select(h => new {
                        HolidayID = h.HolidayID,
                        SortOrder = h.SortOrder,
                        HolidayDate = h.HolidayDate.ToString("yyyy-MM-dd"), // ISO format for consistent parsing
                        HolidayName = h.HolidayName,
                        HolidayType = h.HolidayType,
                        SessionID = h.SessionID,
                        TenantID = h.TenantID,
                        TenantCode = h.TenantCode,
                        IsActive = h.IsActive,
                        IsDeleted = h.IsDeleted,
                        CreatedBy = h.CreatedBy,
                        CreatedDate = h.CreatedDate,
                        ModifiedBy = h.ModifiedBy,
                        ModifiedDate = h.ModifiedDate
                    }).ToList();

                    var recordsTotal = connection.ExecuteScalar<int>(countSql, parameters);
                    var recordsFiltered = connection.ExecuteScalar<int>(countFilteredSql.ToString(), parameters);

                    var jsonData = new
                    {
                        draw = draw,
                        recordsFiltered = recordsFiltered,
                        recordsTotal = recordsTotal,
                        data = data
                    };

                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    draw = "0",
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = new List<Holiday>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetHolidayById(string id)
        {
            try
            {
                Guid holidayId;

                if (!Guid.TryParse(id, out holidayId))
                {
                    return Json(new { success = false, message = "Invalid holiday ID" }, JsonRequestBehavior.AllowGet);
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM HolidayCalendar 
                                  WHERE HolidayID = @HolidayID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        HolidayID = holidayId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var holidayData = connection.QueryFirstOrDefault<Holiday>(sql, parameters);

                    if (holidayData != null)
                    {
                        var result = new
                        {
                            success = true,
                            data = new
                            {
                                HolidayID = holidayData.HolidayID,
                                HolidayName = holidayData.HolidayName,
                                HolidayDate = holidayData.HolidayDate.ToString("yyyy-MM-dd"),
                                HolidayType = holidayData.HolidayType,
                                SortOrder = holidayData.SortOrder,
                                SessionID = holidayData.SessionID,
                                TenantID = holidayData.TenantID,
                                TenantCode = holidayData.TenantCode,
                                IsActive = holidayData.IsActive,
                                IsDeleted = holidayData.IsDeleted,
                                CreatedBy = holidayData.CreatedBy,
                                CreatedDate = holidayData.CreatedDate,
                                ModifiedBy = holidayData.ModifiedBy,
                                ModifiedDate = holidayData.ModifiedDate
                            }
                        };

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Holiday not found" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetNextSortOrder(string sessionId)
        {
            try
            {
                Guid parsedSessionId;

                if (!Guid.TryParse(sessionId, out parsedSessionId))
                {
                    return Json(new { success = false, message = "Invalid session ID" }, JsonRequestBehavior.AllowGet);
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT ISNULL(MAX(SortOrder), 0) + 1 
                                  FROM HolidayCalendar 
                                  WHERE TenantID = @TenantID 
                                  AND SessionID = @SessionID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = parsedSessionId
                    };

                    connection.Open();
                    int nextSortOrder = connection.ExecuteScalar<int>(sql, parameters);

                    return Json(new { success = true, nextSortOrder = nextSortOrder }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private bool IsHolidayNameDuplicate(SqlConnection connection, Holiday holidayData)
        {
            string sql = @"SELECT COUNT(*) FROM HolidayCalendar 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND HolidayName = @HolidayName 
                          AND IsDeleted = 0
                          AND HolidayID != @HolidayID";

            var parameters = new
            {
                holidayData.TenantID,
                holidayData.SessionID,
                holidayData.HolidayName,
                HolidayID = holidayData.HolidayID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        private bool IsHolidayDateDuplicate(SqlConnection connection, Holiday holidayData)
        {
            string sql = @"SELECT COUNT(*) FROM HolidayCalendar 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND HolidayDate = @HolidayDate 
                          AND IsDeleted = 0
                          AND HolidayID != @HolidayID";

            var parameters = new
            {
                holidayData.TenantID,
                holidayData.SessionID,
                HolidayDate = holidayData.HolidayDate.Date,
                HolidayID = holidayData.HolidayID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        private bool IsAttendanceExistsForDate(SqlConnection connection, DateTime holidayDate, Guid sessionId)
        {
            string sql = @"SELECT COUNT(*) FROM StudentAttendance 
                          WHERE AttendanceDate = @AttendanceDate 
                          AND SessionID = @SessionID
                          AND TenantID = @TenantID 
                          AND IsDeleted = 0";

            var parameters = new
            {
                AttendanceDate = holidayDate.Date,
                SessionID = sessionId,
                TenantID = CurrentTenantID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        private int GetAttendanceCountForDate(SqlConnection connection, DateTime holidayDate, Guid sessionId)
        {
            string sql = @"SELECT COUNT(*) FROM StudentAttendance 
                          WHERE AttendanceDate = @AttendanceDate 
                          AND SessionID = @SessionID
                          AND TenantID = @TenantID 
                          AND IsDeleted = 0";

            var parameters = new
            {
                AttendanceDate = holidayDate.Date,
                SessionID = sessionId,
                TenantID = CurrentTenantID
            };

            return connection.ExecuteScalar<int>(sql, parameters);
        }

        private int DeleteAttendanceForDate(SqlConnection connection, DateTime holidayDate, Guid sessionId)
        {
            string sql = @"UPDATE StudentAttendance 
                          SET IsDeleted = 1,
                              ModifiedBy = @ModifiedBy,
                              ModifiedDate = @ModifiedDate
                          WHERE AttendanceDate = @AttendanceDate 
                          AND SessionID = @SessionID
                          AND TenantID = @TenantID 
                          AND IsDeleted = 0";

            var parameters = new
            {
                AttendanceDate = holidayDate.Date,
                SessionID = sessionId,
                TenantID = CurrentTenantID,
                ModifiedBy = CurrentTenantUserID,
                ModifiedDate = DateTime.Now
            };

            return connection.Execute(sql, parameters);
        }

        [HttpPost]
        public JsonResult InsertHoliday(Holiday holidayData, string holidayDateString = null, bool deleteAttendanceIfExists = false)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(holidayData.HolidayName))
                {
                    return Json(new { success = false, message = "Holiday name is required" });
                }

                if (string.IsNullOrEmpty(holidayData.HolidayType))
                {
                    return Json(new { success = false, message = "Holiday type is required" });
                }

                if (holidayData.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Handle date parsing from form data
                if (!string.IsNullOrEmpty(holidayDateString))
                {
                    DateTime parsedDate;
                    if (DateTime.TryParse(holidayDateString, out parsedDate))
                    {
                        holidayData.HolidayDate = parsedDate.Date;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid holiday date format" });
                    }
                }
                else
                {
                    if (holidayData.HolidayDate == DateTime.MinValue || holidayData.HolidayDate == default(DateTime))
                    {
                        return Json(new { success = false, message = "Holiday date is required" });
                    }
                    holidayData.HolidayDate = holidayData.HolidayDate.Date;
                }

                // Set tenant and user information
                holidayData.HolidayID = Guid.NewGuid();
                holidayData.TenantID = CurrentTenantID;
                holidayData.TenantCode = Utils.ParseInt(CurrentTenantCode);
                holidayData.CreatedBy = CurrentTenantUserID;
                holidayData.CreatedDate = DateTime.Now;
                holidayData.IsDeleted = false;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for attendance records
                    int attendanceCount = GetAttendanceCountForDate(connection, holidayData.HolidayDate, holidayData.SessionID);

                    if (attendanceCount > 0)
                    {
                        if (!deleteAttendanceIfExists)
                        {
                            // Return with a flag indicating attendance exists
                            return Json(new
                            {
                                success = false,
                                attendanceExists = true,
                                attendanceCount = attendanceCount,
                                message = $"Student attendance ({attendanceCount} records) exist for {holidayData.HolidayDate.ToString("dd-MMM-yyyy")}. Do you want to delete these attendance records and create the holiday?"
                            });
                        }
                        else
                        {
                            // Delete attendance records
                            int deletedCount = DeleteAttendanceForDate(connection, holidayData.HolidayDate, holidayData.SessionID);
                        }
                    }

                    // Check for duplicate holiday name in the same session
                    if (IsHolidayNameDuplicate(connection, holidayData))
                    {
                        return Json(new { success = false, message = "A holiday with this name already exists for the current session" });
                    }

                    // Check for duplicate holiday date in the same session
                    if (IsHolidayDateDuplicate(connection, holidayData))
                    {
                        return Json(new { success = false, message = "A holiday already exists on this date for the current session" });
                    }

                    string sql = @"INSERT INTO HolidayCalendar 
                          (HolidayID, SortOrder, HolidayDate, HolidayName, HolidayType, SessionID, TenantID, TenantCode, 
                           IsActive, IsDeleted, CreatedBy, CreatedDate) 
                          VALUES 
                          (@HolidayID, @SortOrder, @HolidayDate, @HolidayName, @HolidayType, @SessionID, @TenantID, @TenantCode, 
                           @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, holidayData);

                    if (rowsAffected > 0)
                    {
                        string successMessage = deleteAttendanceIfExists && attendanceCount > 0
                            ? $"Holiday created successfully! {attendanceCount} attendance records were deleted."
                            : "Holiday created successfully!";

                        return Json(new { success = true, message = successMessage });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create holiday" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateHoliday(Holiday holidayData, string holidayDateString = null, bool deleteAttendanceIfExists = false)
        {
            try
            {
                // Validate required data
                if (holidayData.HolidayID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Holiday ID is required" });
                }

                if (string.IsNullOrEmpty(holidayData.HolidayName))
                {
                    return Json(new { success = false, message = "Holiday name is required" });
                }

                if (string.IsNullOrEmpty(holidayData.HolidayType))
                {
                    return Json(new { success = false, message = "Holiday type is required" });
                }

                // Handle date parsing from form data
                if (!string.IsNullOrEmpty(holidayDateString))
                {
                    DateTime parsedDate;
                    if (DateTime.TryParse(holidayDateString, out parsedDate))
                    {
                        holidayData.HolidayDate = parsedDate.Date;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid holiday date format" });
                    }
                }
                else
                {
                    if (holidayData.HolidayDate == DateTime.MinValue || holidayData.HolidayDate == default(DateTime))
                    {
                        return Json(new { success = false, message = "Holiday date is required" });
                    }
                    holidayData.HolidayDate = holidayData.HolidayDate.Date;
                }

                // Set update information
                holidayData.TenantID = CurrentTenantID;
                holidayData.ModifiedBy = CurrentTenantUserID;
                holidayData.ModifiedDate = DateTime.Now;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get the existing holiday record
                    string getCurrentHolidaySql = @"SELECT HolidayDate, SessionID FROM HolidayCalendar 
                                                   WHERE HolidayID = @HolidayID 
                                                   AND TenantID = @TenantID 
                                                   AND IsDeleted = 0";

                    var existingHoliday = connection.QueryFirstOrDefault<dynamic>(getCurrentHolidaySql, new
                    {
                        HolidayID = holidayData.HolidayID,
                        TenantID = holidayData.TenantID
                    });

                    if (existingHoliday == null)
                    {
                        return Json(new { success = false, message = "Holiday not found" });
                    }

                    DateTime existingDate = ((DateTime)existingHoliday.HolidayDate).Date;
                    Guid sessionId = existingHoliday.SessionID;

                    // Use the session ID from the existing holiday if not provided
                    if (holidayData.SessionID == Guid.Empty)
                    {
                        holidayData.SessionID = sessionId;
                    }

                    // Check for attendance on the new date
                    int attendanceCount = GetAttendanceCountForDate(connection, holidayData.HolidayDate, holidayData.SessionID);

                    if (attendanceCount > 0)
                    {
                        if (!deleteAttendanceIfExists)
                        {
                           // string warningMessage = existingDate != holidayData.HolidayDate.Date
                             //   ? $"Cannot change holiday to {holidayData.HolidayDate.ToString("dd-MMM-yyyy")}. Student attendance records ({attendanceCount} records) exist for this date. Do you want to delete these attendance records and update the holiday?"
                               // : $"Student attendance records ({attendanceCount} records) exist for {holidayData.HolidayDate.ToString("dd-MMM-yyyy")}. Do you want to delete these attendance records and update the holiday?";
                            string warningMessage = $"Delete {attendanceCount} records for {holidayData.HolidayDate:dd-MMM-yyyy} and mark holiday ?";

                            return Json(new
                            {
                                success = false,
                                attendanceExists = true,
                                attendanceCount = attendanceCount,
                                message = warningMessage
                            });
                        }
                        else
                        {
                            // Delete attendance records
                            int deletedCount = DeleteAttendanceForDate(connection, holidayData.HolidayDate, holidayData.SessionID);
                        }
                    }

                    // Check for duplicate holiday name
                    string checkNameSql = @"SELECT COUNT(*) FROM HolidayCalendar 
                                          WHERE TenantID = @TenantID 
                                          AND SessionID = @SessionID 
                                          AND HolidayName = @HolidayName 
                                          AND IsDeleted = 0
                                          AND HolidayID != @HolidayID";

                    var nameParams = new
                    {
                        holidayData.TenantID,
                        holidayData.SessionID,
                        holidayData.HolidayName,
                        holidayData.HolidayID
                    };

                    int nameCount = connection.ExecuteScalar<int>(checkNameSql, nameParams);
                    if (nameCount > 0)
                    {
                        return Json(new { success = false, message = "A holiday with this name already exists for the current session" });
                    }

                    // Check for duplicate holiday date
                    string checkDateSql = @"SELECT COUNT(*) FROM HolidayCalendar 
                                          WHERE TenantID = @TenantID 
                                          AND SessionID = @SessionID 
                                          AND HolidayDate = @HolidayDate 
                                          AND IsDeleted = 0
                                          AND HolidayID != @HolidayID";

                    var dateParams = new
                    {
                        holidayData.TenantID,
                        holidayData.SessionID,
                        HolidayDate = holidayData.HolidayDate.Date,
                        holidayData.HolidayID
                    };

                    int dateCount = connection.ExecuteScalar<int>(checkDateSql, dateParams);
                    if (dateCount > 0)
                    {
                        return Json(new { success = false, message = "A holiday already exists on this date for the current session" });
                    }

                    // Perform the update
                    string updateSql = @"UPDATE HolidayCalendar 
                                       SET HolidayName = @HolidayName, 
                                           HolidayDate = @HolidayDate,
                                           HolidayType = @HolidayType,
                                           SortOrder = @SortOrder, 
                                           IsActive = @IsActive, 
                                           ModifiedBy = @ModifiedBy, 
                                           ModifiedDate = @ModifiedDate 
                                       WHERE HolidayID = @HolidayID 
                                       AND TenantID = @TenantID 
                                       AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(updateSql, holidayData);

                    if (rowsAffected > 0)
                    {
                        string successMessage = deleteAttendanceIfExists && attendanceCount > 0
                            ? $"Holiday updated successfully! {attendanceCount} attendance records were deleted."
                            : "Holiday updated successfully!";

                        return Json(new { success = true, message = successMessage });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update holiday. Holiday not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public JsonResult DeleteHoliday(string id)
        {
            try
            {
                Guid holidayId;

                if (!Guid.TryParse(id, out holidayId))
                {
                    return Json(new { success = false, message = "Invalid holiday ID" });
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    // Soft delete
                    string sql = @"UPDATE HolidayCalendar 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE HolidayID = @HolidayID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        HolidayID = holidayId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Holiday deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete holiday. Holiday not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}