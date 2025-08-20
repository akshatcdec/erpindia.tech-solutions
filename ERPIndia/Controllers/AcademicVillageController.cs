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
    public class AcademicVillage
    {
        public Guid VillageID { get; set; }
        public int SortOrder { get; set; }
        public string VillageName { get; set; }
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

    public class AcademicVillageController : BaseController
    {
        public ActionResult ManageVillages()
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
        public JsonResult GetAllVillages(string sessionId, int sessionYear, bool checkDuplicate = false, string villageName = null)
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
                        data = new List<AcademicVillage>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(villageName))
                    {
                        string sql1 = @"SELECT * FROM AcademicVillageMaster 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND VillageName = @VillageName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            VillageName = villageName
                        };

                        connection.Open();
                        var villages = connection.Query<AcademicVillage>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = villages });
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
                    sql.Append("WITH VillageData AS ( ");
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
                    sql.Append("FROM AcademicVillageMaster WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (VillageName LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM VillageData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM AcademicVillageMaster 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (VillageName LIKE @Search)");
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
                    var data = connection.Query<AcademicVillage>(sql.ToString(), parameters).ToList();
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
                    data = new List<AcademicVillage>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetVillageById(string id)
        {
            try
            {
                Guid villageId;

                if (!Guid.TryParse(id, out villageId))
                {
                    return Json(new { success = false, message = "Invalid village ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM AcademicVillageMaster 
                                  WHERE VillageID = @VillageID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        VillageID = villageId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var villageData = connection.QueryFirstOrDefault<AcademicVillage>(sql, parameters);

                    if (villageData != null)
                    {
                        return Json(new { success = true, data = villageData }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Village not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM AcademicVillageMaster 
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

        private bool IsVillageNameDuplicate(SqlConnection connection, AcademicVillage villageData)
        {
            string sql = @"SELECT COUNT(*) FROM AcademicVillageMaster 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND VillageName = @VillageName 
                          AND IsDeleted = 0
                          AND VillageID != @VillageID";

            var parameters = new
            {
                villageData.TenantID,
                villageData.SessionID,
                villageData.VillageName,
                VillageID = villageData.VillageID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertVillage(AcademicVillage villageData)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(villageData.VillageName))
                {
                    return Json(new { success = false, message = "Village name is required" });
                }

                if (villageData.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Set tenant and user information
                villageData.VillageID = Guid.NewGuid();  // Generate new ID
                villageData.TenantID = CurrentTenantID;
                villageData.TenantCode = Utils.ParseInt(CurrentTenantCode);
                villageData.CreatedBy = CurrentTenantUserID;
                villageData.CreatedDate = DateTime.Now;
                villageData.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate village name in the same session
                    if (IsVillageNameDuplicate(connection, villageData))
                    {
                        return Json(new { success = false, message = "A village with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO AcademicVillageMaster 
                                  (VillageID, SortOrder, VillageName, SessionYear, SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@VillageID, @SortOrder, @VillageName, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, villageData);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Village created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create village" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateVillage(AcademicVillage villageData)
        {
            try
            {
                // Validate required data
                if (villageData.VillageID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Village ID is required" });
                }

                if (string.IsNullOrEmpty(villageData.VillageName))
                {
                    return Json(new { success = false, message = "Village name is required" });
                }

                // Set update information
                villageData.TenantID = CurrentTenantID;
                villageData.ModifiedBy = CurrentTenantUserID;
                villageData.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate village name in the same session
                    if (IsVillageNameDuplicate(connection, villageData))
                    {
                        return Json(new { success = false, message = "A village with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE AcademicVillageMaster 
                                  SET VillageName = @VillageName, 
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE VillageID = @VillageID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, villageData);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Village updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update village. Village not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteVillage(string id)
        {
            try
            {
                Guid villageId;

                if (!Guid.TryParse(id, out villageId))
                {
                    return Json(new { success = false, message = "Invalid village ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE AcademicVillageMaster 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE VillageID = @VillageID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        VillageID = villageId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Village deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete village. Village not found or access denied." });
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