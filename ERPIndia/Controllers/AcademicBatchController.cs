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
    public class AcademicBatch
    {
        public Guid BatchID { get; set; }
        public int SortOrder { get; set; }
        public string BatchName { get; set; }
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

    public class AcademicBatchController : BaseController
    {
        public ActionResult ManageBatches()
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
        public JsonResult GetAllBatches(string sessionId, int sessionYear, bool checkDuplicate = false, string batchName = null)
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
                        data = new List<AcademicBatch>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(batchName))
                    {
                        string sql1 = @"SELECT * FROM AcademicBatchMaster 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND BatchName = @BatchName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            BatchName = batchName
                        };

                        connection.Open();
                        var batches = connection.Query<AcademicBatch>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = batches });
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
                    sql.Append("WITH BatchData AS ( ");
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
                    sql.Append("FROM AcademicBatchMaster WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (BatchName LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM BatchData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM AcademicBatchMaster 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (BatchName LIKE @Search)");
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
                    var data = connection.Query<AcademicBatch>(sql.ToString(), parameters).ToList();
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
                    data = new List<AcademicBatch>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetBatchById(string id)
        {
            try
            {
                Guid batchId;

                if (!Guid.TryParse(id, out batchId))
                {
                    return Json(new { success = false, message = "Invalid batch ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM AcademicBatchMaster 
                                  WHERE BatchID = @BatchID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        BatchID = batchId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var batch = connection.QueryFirstOrDefault<AcademicBatch>(sql, parameters);

                    if (batch != null)
                    {
                        return Json(new { success = true, data = batch }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Batch not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM AcademicBatchMaster 
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

        private bool IsBatchNameDuplicate(SqlConnection connection, AcademicBatch batch)
        {
            string sql = @"SELECT COUNT(*) FROM AcademicBatchMaster 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND BatchName = @BatchName 
                          AND IsDeleted = 0
                          AND BatchID != @BatchID";

            var parameters = new
            {
                batch.TenantID,
                batch.SessionID,
                batch.BatchName,
                BatchID = batch.BatchID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertBatch(AcademicBatch batch)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(batch.BatchName))
                {
                    return Json(new { success = false, message = "Batch name is required" });
                }

                if (batch.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Set tenant and user information
                batch.BatchID = Guid.NewGuid();  // Generate new ID
                batch.TenantID = CurrentTenantID;
                batch.TenantCode = Utils.ParseInt(CurrentTenantCode);
                batch.CreatedBy = CurrentTenantUserID;
                batch.CreatedDate = DateTime.Now;
                batch.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate batch name in the same session
                    if (IsBatchNameDuplicate(connection, batch))
                    {
                        return Json(new { success = false, message = "A batch with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO AcademicBatchMaster 
                                  (BatchID, SortOrder, BatchName, SessionYear, SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@BatchID, @SortOrder, @BatchName, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, batch);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Batch created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create batch" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateBatch(AcademicBatch batch)
        {
            try
            {
                // Validate required data
                if (batch.BatchID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Batch ID is required" });
                }

                if (string.IsNullOrEmpty(batch.BatchName))
                {
                    return Json(new { success = false, message = "Batch name is required" });
                }

                // Set update information
                batch.TenantID = CurrentTenantID;
                batch.ModifiedBy = CurrentTenantUserID;
                batch.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate batch name in the same session
                    if (IsBatchNameDuplicate(connection, batch))
                    {
                        return Json(new { success = false, message = "A batch with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE AcademicBatchMaster 
                                  SET BatchName = @BatchName, 
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE BatchID = @BatchID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, batch);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Batch updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update batch. Batch not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteBatch(string id)
        {
            try
            {
                Guid batchId;

                if (!Guid.TryParse(id, out batchId))
                {
                    return Json(new { success = false, message = "Invalid batch ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE AcademicBatchMaster 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE BatchID = @BatchID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        BatchID = batchId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Batch deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete batch. Batch not found or access denied." });
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