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
    public class TransportRoute
    {
        public Guid TransportRouteId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
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

    public class TransportRouteController : BaseController
    {
        public ActionResult ManageTransportRoutes()
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
        public JsonResult GetAllTransportRoutes(string sessionId, int sessionYear, bool checkDuplicate = false, string name = null)
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
                        data = new List<TransportRoute>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(name))
                    {
                        string sql1 = @"SELECT * FROM TransportRoute 
                                      WHERE TenantID = @TenantID 
                                      AND SessionYear = @SessionYear 
                                      AND Name = @Name
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionYear = sessionYear,
                            Name = name
                        };

                        connection.Open();
                        var transportRoutes = connection.Query<TransportRoute>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = transportRoutes });
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
                    sql.Append("WITH TransportRouteData AS ( ");
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
                    sql.Append("FROM TransportRoute WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (Name LIKE @Search OR Description LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM TransportRouteData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM TransportRoute 
                                    WHERE TenantID = @TenantID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (Name LIKE @Search OR Description LIKE @Search)");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@TenantCode", CurrentTenantCode);
                    parameters.Add("@SessionYear", sessionYear);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Execute queries
                    var data = connection.Query<TransportRoute>(sql.ToString(), parameters).ToList();
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
                    data = new List<TransportRoute>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetTransportRouteById(string id)
        {
            try
            {
                Guid transportRouteId;

                if (!Guid.TryParse(id, out transportRouteId))
                {
                    return Json(new { success = false, message = "Invalid transport route ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM TransportRoute 
                                  WHERE TransportRouteId = @TransportRouteId 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        TransportRouteId = transportRouteId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var transportRoute = connection.QueryFirstOrDefault<TransportRoute>(sql, parameters);

                    if (transportRoute != null)
                    {
                        return Json(new { success = true, data = transportRoute }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Transport route not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM TransportRoute 
                                  WHERE TenantID = @TenantID 
                                  AND SessionYear = @SessionYear 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        TenantID = CurrentTenantID,
                        SessionYear = CurrentSessionYear
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

        private bool IsRouteNameDuplicate(SqlConnection connection, TransportRoute transportRoute)
        {
            string sql = @"SELECT COUNT(*) FROM TransportRoute 
                          WHERE TenantID = @TenantID 
                          AND SessionYear = @SessionYear 
                          AND Name = @Name 
                          AND IsDeleted = 0
                          AND TransportRouteId != @TransportRouteId";

            var parameters = new
            {
                transportRoute.TenantID,
                transportRoute.SessionYear,
                transportRoute.Name,
                TransportRouteId = transportRoute.TransportRouteId
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertTransportRoute(TransportRoute transportRoute)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(transportRoute.Name))
                {
                    return Json(new { success = false, message = "Route name is required" });
                }

                if (string.IsNullOrEmpty(transportRoute.Description))
                {
                    return Json(new { success = false, message = "Description is required" });
                }

                // Set tenant and user information
                transportRoute.TransportRouteId = Guid.NewGuid();  // Generate new ID
                transportRoute.TenantID = CurrentTenantID;
                transportRoute.SessionID = CurrentSessionID;
                transportRoute.TenantCode = Utils.ParseInt(CurrentTenantCode);
                transportRoute.SessionYear = CurrentSessionYear;
                transportRoute.CreatedBy = CurrentTenantUserID;
                transportRoute.CreatedDate = DateTime.Now;
                transportRoute.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate route name in the same session
                    if (IsRouteNameDuplicate(connection, transportRoute))
                    {
                        return Json(new { success = false, message = "A transport route with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO TransportRoute 
                                  (TransportRouteId, Name, Description,SessionID, SortOrder, SessionYear, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@TransportRouteId, @Name, @Description, @SessionID,@SortOrder, @SessionYear, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, transportRoute);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Transport route created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create transport route" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateTransportRoute(TransportRoute transportRoute)
        {
            try
            {
                // Validate required data
                if (transportRoute.TransportRouteId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Transport route ID is required" });
                }

                if (string.IsNullOrEmpty(transportRoute.Name))
                {
                    return Json(new { success = false, message = "Route name is required" });
                }

                if (string.IsNullOrEmpty(transportRoute.Description))
                {
                    return Json(new { success = false, message = "Description is required" });
                }

                // Set update information
                transportRoute.TenantID = CurrentTenantID;
                transportRoute.ModifiedBy = CurrentTenantUserID;
                transportRoute.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate route name in the same session
                    if (IsRouteNameDuplicate(connection, transportRoute))
                    {
                        return Json(new { success = false, message = "A transport route with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE TransportRoute 
                                  SET Name = @Name, 
                                      Description = @Description,
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE TransportRouteId = @TransportRouteId 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, transportRoute);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Transport route updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update transport route. Transport route not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteTransportRoute(string id)
        {
            try
            {
                Guid transportRouteId;

                if (!Guid.TryParse(id, out transportRouteId))
                {
                    return Json(new { success = false, message = "Invalid transport route ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE TransportRoute 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE TransportRouteId = @TransportRouteId 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        TransportRouteId = transportRouteId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Transport route deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete transport route. Transport route not found or access denied." });
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