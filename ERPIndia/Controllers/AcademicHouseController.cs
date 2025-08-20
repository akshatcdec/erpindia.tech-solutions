// =============================================
// CONTROLLER - AcademicHouseController.cs
// =============================================

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
    public class AcademicHouse
    {
        public Guid HouseID { get; set; }
        public int SortOrder { get; set; }
        public string HouseName { get; set; }
        public int SessionYear { get; set; }
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

    public class AcademicHouseController : BaseController
    {
        public ActionResult ManageHouses()
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
        public JsonResult GetAllHouses(string sessionId, int sessionYear, bool checkDuplicate = false, string houseName = null)
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
                        data = new List<AcademicHouse>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(houseName))
                    {
                        string sql1 = @"SELECT * FROM AcademicHouseMaster 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND HouseName = @HouseName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            HouseName = houseName
                        };

                        connection.Open();
                        var houses = connection.Query<AcademicHouse>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = houses });
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
                    sql.Append("WITH HouseData AS ( ");
                    sql.Append("SELECT *, ROW_NUMBER() OVER (");

                    // Handle sorting
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        sql.Append($"ORDER BY {sortColumn} {sortColumnDirection}");
                    }
                    else
                    {
                        // Default sorting by SortOrder
                        sql.Append("ORDER BY SortOrder");
                    }

                    sql.Append(") AS RowNum ");
                    sql.Append("FROM AcademicHouseMaster WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (HouseName LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM HouseData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM AcademicHouseMaster 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (HouseName LIKE @Search)");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@TenantCode", CurrentTenantCode);
                    parameters.Add("@SessionID", parsedSessionId);
                    parameters.Add("@SessionYear", sessionYear);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Execute queries
                    var data = connection.Query<AcademicHouse>(sql.ToString(), parameters).ToList();
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
                    data = new List<AcademicHouse>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetHouseById(string id)
        {
            try
            {
                Guid houseId;

                if (!Guid.TryParse(id, out houseId))
                {
                    return Json(new { success = false, message = "Invalid house ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM AcademicHouseMaster 
                                  WHERE HouseID = @HouseID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        HouseID = houseId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var houseData = connection.QueryFirstOrDefault<AcademicHouse>(sql, parameters);

                    if (houseData != null)
                    {
                        return Json(new { success = true, data = houseData }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "House not found" }, JsonRequestBehavior.AllowGet);
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

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT ISNULL(MAX(SortOrder), 0) + 1 
                                  FROM AcademicHouseMaster 
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

        private bool IsHouseNameDuplicate(SqlConnection connection, AcademicHouse houseData)
        {
            string sql = @"SELECT COUNT(*) FROM AcademicHouseMaster 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND HouseName = @HouseName 
                          AND IsDeleted = 0
                          AND HouseID != @HouseID";

            var parameters = new
            {
                houseData.TenantID,
                houseData.SessionID,
                houseData.HouseName,
                HouseID = houseData.HouseID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertHouse(AcademicHouse houseData)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(houseData.HouseName))
                {
                    return Json(new { success = false, message = "House name is required" });
                }

                if (houseData.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Set tenant and user information
                houseData.HouseID = Guid.NewGuid();  // Generate new ID
                houseData.TenantID = CurrentTenantID;
                houseData.TenantCode = Utils.ParseInt(CurrentTenantCode);
                houseData.CreatedBy = CurrentTenantUserID;
                houseData.CreatedDate = DateTime.Now;
                houseData.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate house name in the same session
                    if (IsHouseNameDuplicate(connection, houseData))
                    {
                        return Json(new { success = false, message = "A house with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO AcademicHouseMaster 
                                  (HouseID, SortOrder, HouseName, SessionYear, SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@HouseID, @SortOrder, @HouseName, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, houseData);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "House created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create house" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateHouse(AcademicHouse houseData)
        {
            try
            {
                // Validate required data
                if (houseData.HouseID == Guid.Empty)
                {
                    return Json(new { success = false, message = "House ID is required" });
                }

                if (string.IsNullOrEmpty(houseData.HouseName))
                {
                    return Json(new { success = false, message = "House name is required" });
                }

                // Set update information
                houseData.TenantID = CurrentTenantID;
                houseData.ModifiedBy = CurrentTenantUserID;
                houseData.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate house name in the same session
                    if (IsHouseNameDuplicate(connection, houseData))
                    {
                        return Json(new { success = false, message = "A house with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE AcademicHouseMaster 
                                  SET HouseName = @HouseName, 
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE HouseID = @HouseID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, houseData);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "House updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update house. House not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteHouse(string id)
        {
            try
            {
                Guid houseId;

                if (!Guid.TryParse(id, out houseId))
                {
                    return Json(new { success = false, message = "Invalid house ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE AcademicHouseMaster 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE HouseID = @HouseID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        HouseID = houseId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "House deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete house. House not found or access denied." });
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
