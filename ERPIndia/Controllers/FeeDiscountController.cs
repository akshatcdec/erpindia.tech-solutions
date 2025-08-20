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
    public class FeeDiscount
    {
        public Guid FeeDiscountID { get; set; }
        public int SortOrder { get; set; }
        public string DiscountName { get; set; }
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

    public class FeeDiscountController : BaseController
    {
        public ActionResult ManageFeeDiscounts()
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
        public JsonResult GetAllFeeDiscounts(string sessionId, int sessionYear, bool checkDuplicate = false, string discountName = null)
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
                        data = new List<FeeDiscount>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(discountName))
                    {
                        string sql1 = @"SELECT * FROM FeeDiscountMaster 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND DiscountName = @DiscountName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            DiscountName = discountName
                        };

                        connection.Open();
                        var feeDiscounts = connection.Query<FeeDiscount>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = feeDiscounts });
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
                    sql.Append("WITH FeeDiscountData AS ( ");
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
                    sql.Append("FROM FeeDiscountMaster WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (DiscountName LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM FeeDiscountData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM FeeDiscountMaster 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (DiscountName LIKE @Search)");
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
                    var data = connection.Query<FeeDiscount>(sql.ToString(), parameters).ToList();
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
                    data = new List<FeeDiscount>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetFeeDiscountById(string id)
        {
            try
            {
                Guid feeDiscountId;

                if (!Guid.TryParse(id, out feeDiscountId))
                {
                    return Json(new { success = false, message = "Invalid fee discount ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM FeeDiscountMaster 
                                  WHERE FeeDiscountID = @FeeDiscountID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        FeeDiscountID = feeDiscountId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var feeDiscount = connection.QueryFirstOrDefault<FeeDiscount>(sql, parameters);

                    if (feeDiscount != null)
                    {
                        return Json(new { success = true, data = feeDiscount }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Fee discount not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM FeeDiscountMaster 
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

        private bool IsFeeDiscountNameDuplicate(SqlConnection connection, FeeDiscount feeDiscount)
        {
            string sql = @"SELECT COUNT(*) FROM FeeDiscountMaster 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND DiscountName = @DiscountName 
                          AND IsDeleted = 0
                          AND FeeDiscountID != @FeeDiscountID";

            var parameters = new
            {
                feeDiscount.TenantID,
                feeDiscount.SessionID,
                feeDiscount.DiscountName,
                FeeDiscountID = feeDiscount.FeeDiscountID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertFeeDiscount(FeeDiscount feeDiscount)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(feeDiscount.DiscountName))
                {
                    return Json(new { success = false, message = "Discount name is required" });
                }

                if (feeDiscount.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Set tenant and user information
                feeDiscount.FeeDiscountID = Guid.NewGuid();  // Generate new ID
                feeDiscount.TenantID = CurrentTenantID;
                feeDiscount.TenantCode = Utils.ParseInt(CurrentTenantCode);
                feeDiscount.CreatedBy = CurrentTenantUserID;
                feeDiscount.CreatedDate = DateTime.Now;
                feeDiscount.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate fee discount name in the same session
                    if (IsFeeDiscountNameDuplicate(connection, feeDiscount))
                    {
                        return Json(new { success = false, message = "A fee discount with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO FeeDiscountMaster 
                                  (FeeDiscountID, SortOrder, DiscountName, SessionYear, SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@FeeDiscountID, @SortOrder, @DiscountName, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, feeDiscount);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Fee discount created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create fee discount" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateFeeDiscount(FeeDiscount feeDiscount)
        {
            try
            {
                // Validate required data
                if (feeDiscount.FeeDiscountID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Fee discount ID is required" });
                }

                if (string.IsNullOrEmpty(feeDiscount.DiscountName))
                {
                    return Json(new { success = false, message = "Discount name is required" });
                }

                // Set update information
                feeDiscount.TenantID = CurrentTenantID;
                feeDiscount.ModifiedBy = CurrentTenantUserID;
                feeDiscount.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate fee discount name in the same session
                    if (IsFeeDiscountNameDuplicate(connection, feeDiscount))
                    {
                        return Json(new { success = false, message = "A fee discount with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE FeeDiscountMaster 
                                  SET DiscountName = @DiscountName, 
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE FeeDiscountID = @FeeDiscountID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, feeDiscount);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Fee discount updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update fee discount. Fee discount not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteFeeDiscount(string id)
        {
            try
            {
                Guid feeDiscountId;

                if (!Guid.TryParse(id, out feeDiscountId))
                {
                    return Json(new { success = false, message = "Invalid fee discount ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE FeeDiscountMaster 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE FeeDiscountID = @FeeDiscountID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        FeeDiscountID = feeDiscountId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Fee discount deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete fee discount. Fee discount not found or access denied." });
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