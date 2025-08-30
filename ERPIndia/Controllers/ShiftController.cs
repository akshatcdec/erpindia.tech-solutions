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
    public class Shift
    {
        public Guid ShiftID { get; set; }
        public int SortOrder { get; set; }
        public string ShiftName { get; set; }
        public string Description { get; set; }
        public Guid TenantID { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class ShiftController : BaseController
    {
        public ActionResult ManageShifts()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetAllShifts(bool checkDuplicate = false, string shiftName = null)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates
                    if (checkDuplicate && !string.IsNullOrEmpty(shiftName))
                    {
                        string sql1 = @"SELECT * FROM HR_MST_Shift 
                                      WHERE TenantID = @TenantID 
                                      AND ShiftName = @ShiftName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            ShiftName = shiftName
                        };

                        connection.Open();
                        var shifts = connection.Query<Shift>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = shifts });
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
                    sql.Append("WITH ShiftData AS ( ");
                    sql.Append("SELECT *, ROW_NUMBER() OVER (");

                    // Handle sorting
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        sql.Append($"ORDER BY {sortColumn} {sortColumnDirection}");
                    }
                    else
                    {
                        sql.Append("ORDER BY SortOrder");
                    }

                    sql.Append(") AS RowNum ");
                    sql.Append("FROM HR_MST_Shift WHERE TenantID = @TenantID ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (ShiftName LIKE @Search OR Description LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM ShiftData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM HR_MST_Shift 
                                    WHERE TenantID = @TenantID 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (ShiftName LIKE @Search OR Description LIKE @Search)");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@TenantCode", CurrentTenantCode);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Execute queries
                    var data = connection.Query<Shift>(sql.ToString(), parameters).ToList();
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
                    data = new List<Shift>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetShiftById(string id)
        {
            try
            {
                Guid shiftId;

                if (!Guid.TryParse(id, out shiftId))
                {
                    return Json(new { success = false, message = "Invalid shift ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM HR_MST_Shift 
                                  WHERE ShiftID = @ShiftID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        ShiftID = shiftId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var shift = connection.QueryFirstOrDefault<Shift>(sql, parameters);

                    if (shift != null)
                    {
                        return Json(new { success = true, data = shift }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Shift not found" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetNextSortOrder()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT ISNULL(MAX(SortOrder), 0) + 1 
                                  FROM HR_MST_Shift 
                                  WHERE TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        TenantID = CurrentTenantID
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

        private bool IsShiftDuplicate(SqlConnection connection, Shift shift)
        {
            string sql = @"SELECT COUNT(*) FROM HR_MST_Shift 
                          WHERE TenantID = @TenantID 
                          AND ShiftName = @ShiftName 
                          AND IsDeleted = 0
                          AND ShiftID != @ShiftID";

            var parameters = new
            {
                shift.TenantID,
                shift.ShiftName,
                ShiftID = shift.ShiftID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertShift(Shift shift)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(shift.ShiftName))
                {
                    return Json(new { success = false, message = "Shift name is required" });
                }

                // Set tenant and user information
                shift.ShiftID = Guid.NewGuid();
                shift.TenantID = CurrentTenantID;
                shift.TenantCode = Utils.ParseInt(CurrentTenantCode);
                shift.CreatedBy = CurrentTenantUserID;
                shift.CreatedDate = DateTime.Now;
                shift.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate
                    if (IsShiftDuplicate(connection, shift))
                    {
                        return Json(new { success = false, message = "A shift with this name already exists" });
                    }

                    string sql = @"INSERT INTO HR_MST_Shift 
                                  (ShiftID, SortOrder, ShiftName, Description, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@ShiftID, @SortOrder, @ShiftName, @Description, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, shift);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Shift created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create shift" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateShift(Shift shift)
        {
            try
            {
                // Validate required data
                if (shift.ShiftID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Shift ID is required" });
                }

                if (string.IsNullOrEmpty(shift.ShiftName))
                {
                    return Json(new { success = false, message = "Shift name is required" });
                }

                // Set update information
                shift.TenantID = CurrentTenantID;
                shift.ModifiedBy = CurrentTenantUserID;
                shift.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate
                    if (IsShiftDuplicate(connection, shift))
                    {
                        return Json(new { success = false, message = "A shift with this name already exists" });
                    }

                    string sql = @"UPDATE HR_MST_Shift 
                                  SET ShiftName = @ShiftName, 
                                      Description = @Description,
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE ShiftID = @ShiftID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, shift);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Shift updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update shift. Shift not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteShift(string id)
        {
            try
            {
                Guid shiftId;

                if (!Guid.TryParse(id, out shiftId))
                {
                    return Json(new { success = false, message = "Invalid shift ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Soft delete
                    string sql = @"UPDATE HR_MST_Shift 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE ShiftID = @ShiftID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        ShiftID = shiftId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Shift deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete shift. Shift not found or access denied." });
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