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
    public class FeeCategory
    {
        public Guid FeeCategoryID { get; set; }
        public int SortOrder { get; set; }
        public string CategoryName { get; set; }
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

    public class FeeCategoryController : BaseController
    {
        public ActionResult ManageFeeCategories()
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
        public JsonResult GetAllFeeCategories(string sessionId, int sessionYear, bool checkDuplicate = false, string categoryName = null)
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
                        data = new List<FeeCategory>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(categoryName))
                    {
                        string sql1 = @"SELECT * FROM FeeCategoryMaster 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND CategoryName = @CategoryName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            CategoryName = categoryName
                        };

                        connection.Open();
                        var feeCategories = connection.Query<FeeCategory>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = feeCategories });
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
                    sql.Append("WITH FeeCategoryData AS ( ");
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
                    sql.Append("FROM FeeCategoryMaster WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (CategoryName LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM FeeCategoryData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM FeeCategoryMaster 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (CategoryName LIKE @Search)");
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
                    var data = connection.Query<FeeCategory>(sql.ToString(), parameters).ToList();
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
                    data = new List<FeeCategory>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetFeeCategoryById(string id)
        {
            try
            {
                Guid feeCategoryId;

                if (!Guid.TryParse(id, out feeCategoryId))
                {
                    return Json(new { success = false, message = "Invalid fee category ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM FeeCategoryMaster 
                                  WHERE FeeCategoryID = @FeeCategoryID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        FeeCategoryID = feeCategoryId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var feeCategory = connection.QueryFirstOrDefault<FeeCategory>(sql, parameters);

                    if (feeCategory != null)
                    {
                        return Json(new { success = true, data = feeCategory }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Fee category not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM FeeCategoryMaster 
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

        private bool IsFeeCategoryNameDuplicate(SqlConnection connection, FeeCategory feeCategory)
        {
            string sql = @"SELECT COUNT(*) FROM FeeCategoryMaster 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND CategoryName = @CategoryName 
                          AND IsDeleted = 0
                          AND FeeCategoryID != @FeeCategoryID";

            var parameters = new
            {
                feeCategory.TenantID,
                feeCategory.SessionID,
                feeCategory.CategoryName,
                FeeCategoryID = feeCategory.FeeCategoryID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertFeeCategory(FeeCategory feeCategory)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(feeCategory.CategoryName))
                {
                    return Json(new { success = false, message = "Category name is required" });
                }

                if (feeCategory.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Set tenant and user information
                feeCategory.FeeCategoryID = Guid.NewGuid();  // Generate new ID
                feeCategory.TenantID = CurrentTenantID;
                feeCategory.TenantCode = Utils.ParseInt(CurrentTenantCode);
                feeCategory.CreatedBy = CurrentTenantUserID;
                feeCategory.CreatedDate = DateTime.Now;
                feeCategory.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate fee category name in the same session
                    if (IsFeeCategoryNameDuplicate(connection, feeCategory))
                    {
                        return Json(new { success = false, message = "A fee category with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO FeeCategoryMaster 
                                  (FeeCategoryID, SortOrder, CategoryName, SessionYear, SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@FeeCategoryID, @SortOrder, @CategoryName, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, feeCategory);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Fee category created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create fee category" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateFeeCategory(FeeCategory feeCategory)
        {
            try
            {
                // Validate required data
                if (feeCategory.FeeCategoryID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Fee category ID is required" });
                }

                if (string.IsNullOrEmpty(feeCategory.CategoryName))
                {
                    return Json(new { success = false, message = "Category name is required" });
                }

                // Set update information
                feeCategory.TenantID = CurrentTenantID;
                feeCategory.ModifiedBy = CurrentTenantUserID;
                feeCategory.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate fee category name in the same session
                    if (IsFeeCategoryNameDuplicate(connection, feeCategory))
                    {
                        return Json(new { success = false, message = "A fee category with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE FeeCategoryMaster 
                                  SET CategoryName = @CategoryName, 
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE FeeCategoryID = @FeeCategoryID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, feeCategory);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Fee category updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update fee category. Fee category not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteFeeCategory(string id)
        {
            try
            {
                Guid feeCategoryId;

                if (!Guid.TryParse(id, out feeCategoryId))
                {
                    return Json(new { success = false, message = "Invalid fee category ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE FeeCategoryMaster 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE FeeCategoryID = @FeeCategoryID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        FeeCategoryID = feeCategoryId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Fee category deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete fee category. Fee category not found or access denied." });
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