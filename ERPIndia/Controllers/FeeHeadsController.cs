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
    public class FeeHead
    {
        public Guid FeeHeadsID { get; set; }
        public int SortOrder { get; set; }
        public string HeadsName { get; set; }
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

    public class FeeHeadsController : BaseController
    {
        public ActionResult ManageFeeHeads()
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
        public JsonResult GetAllFeeHeads(string sessionId, int sessionYear, bool checkDuplicate = false, string headName = null)
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
                        data = new List<FeeHead>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(headName))
                    {
                        string sql1 = @"SELECT * FROM FeeHeadsMaster 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND HeadsName = @HeadsName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            HeadsName = headName
                        };

                        connection.Open();
                        var feeHeads = connection.Query<FeeHead>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = feeHeads });
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
                    sql.Append("WITH FeeHeadData AS ( ");
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
                    sql.Append("FROM FeeHeadsMaster WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (HeadsName LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM FeeHeadData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM FeeHeadsMaster 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (HeadsName LIKE @Search)");
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
                    var data = connection.Query<FeeHead>(sql.ToString(), parameters).ToList();
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
                    data = new List<FeeHead>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetFeeHeadById(string id)
        {
            try
            {
                Guid feeHeadId;

                if (!Guid.TryParse(id, out feeHeadId))
                {
                    return Json(new { success = false, message = "Invalid fee head ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM FeeHeadsMaster 
                                  WHERE FeeHeadsID = @FeeHeadsID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        FeeHeadsID = feeHeadId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var feeHead = connection.QueryFirstOrDefault<FeeHead>(sql, parameters);

                    if (feeHead != null)
                    {
                        return Json(new { success = true, data = feeHead }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Fee head not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM FeeHeadsMaster 
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

        private bool IsFeeHeadNameDuplicate(SqlConnection connection, FeeHead feeHead)
        {
            string sql = @"SELECT COUNT(*) FROM FeeHeadsMaster 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND HeadsName = @HeadsName 
                          AND IsDeleted = 0
                          AND FeeHeadsID != @FeeHeadsID";

            var parameters = new
            {
                feeHead.TenantID,
                feeHead.SessionID,
                feeHead.HeadsName,
                FeeHeadsID = feeHead.FeeHeadsID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertFeeHead(FeeHead feeHead)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(feeHead.HeadsName))
                {
                    return Json(new { success = false, message = "Fee head name is required" });
                }

                if (feeHead.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Set tenant and user information
                feeHead.FeeHeadsID = Guid.NewGuid();  // Generate new ID
                feeHead.TenantID = CurrentTenantID;
                feeHead.TenantCode = Utils.ParseInt(CurrentTenantCode);
                feeHead.CreatedBy = CurrentTenantUserID;
                feeHead.CreatedDate = DateTime.Now;
                feeHead.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate fee head name in the same session
                    if (IsFeeHeadNameDuplicate(connection, feeHead))
                    {
                        return Json(new { success = false, message = "A fee head with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO FeeHeadsMaster 
                                  (FeeHeadsID, SortOrder, HeadsName, SessionYear, SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@FeeHeadsID, @SortOrder, @HeadsName, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, feeHead);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Fee head created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create fee head" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateFeeHead(FeeHead feeHead)
        {
            try
            {
                // Validate required data
                if (feeHead.FeeHeadsID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Fee head ID is required" });
                }

                if (string.IsNullOrEmpty(feeHead.HeadsName))
                {
                    return Json(new { success = false, message = "Fee head name is required" });
                }

                // Set update information
                feeHead.TenantID = CurrentTenantID;
                feeHead.ModifiedBy = CurrentTenantUserID;
                feeHead.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate fee head name in the same session
                    if (IsFeeHeadNameDuplicate(connection, feeHead))
                    {
                        return Json(new { success = false, message = "A fee head with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE FeeHeadsMaster 
                                  SET HeadsName = @HeadsName, 
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE FeeHeadsID = @FeeHeadsID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, feeHead);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Fee head updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update fee head. Fee head not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteFeeHead(string id)
        {
            try
            {
                Guid feeHeadId;

                if (!Guid.TryParse(id, out feeHeadId))
                {
                    return Json(new { success = false, message = "Invalid fee head ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE FeeHeadsMaster 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE FeeHeadsID = @FeeHeadsID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        FeeHeadsID = feeHeadId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Fee head deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete fee head. Fee head not found or access denied." });
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