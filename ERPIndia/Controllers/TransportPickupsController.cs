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
    public class TransportPickup
    {
        public Guid TransportPickupsId { get; set; }
        public Guid RouteId { get; set; }
        public string PickupName { get; set; }
        public decimal Fee { get; set; }
        public int SortOrder { get; set; }
        public int SessionYear { get; set; }
        public Guid TenantID { get; set; }
        public Guid SessionID { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties for UI display
        public string RouteName { get; set; }
    }

    public class TransportPickupsController : BaseController
    {
        public ActionResult ManageTransportPickups()
        {
            // Retrieve values from session
            Guid sessionId = CurrentSessionID;
            int sessionYear = CurrentSessionYear;

            // Pass to ViewBag for use in the view
            ViewBag.SessionId = sessionId;
            ViewBag.SessionYear = sessionYear;

            return View();
        }

        [HttpGet]
        public JsonResult GetRoutes()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT TransportRouteId, Name 
                                 FROM TransportRoute 
                                 WHERE TenantID = @TenantID 
                                 AND SessionYear = @SessionYear 
                                 AND IsDeleted = 0 
                                 AND IsActive = 1 
                                 ORDER BY SortOrder, Name";

                    var parameters = new
                    {
                        TenantID = CurrentTenantID,
                        SessionYear = CurrentSessionYear
                    };

                    connection.Open();
                    var routes = connection.Query<dynamic>(sql, parameters).ToList();

                    return Json(new { success = true, data = routes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetAllTransportPickups(string routeId, int sessionYear, bool checkDuplicate = false, string pickupName = null)
        {
            try
            {
                // Validate input
                Guid parsedRouteId;
                bool validRouteId = Guid.TryParse(routeId, out parsedRouteId);

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(pickupName))
                    {
                        string sql1 = @"SELECT p.* FROM TransportPickups p
                                      WHERE p.TenantID = @TenantID 
                                      AND p.SessionYear = @SessionYear 
                                      AND p.RouteId = @RouteId
                                      AND p.PickupName = @PickupName
                                      AND p.IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionYear = sessionYear,
                            RouteId = parsedRouteId,
                            PickupName = pickupName
                        };

                        connection.Open();
                        var transportPickups = connection.Query<TransportPickup>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = transportPickups });
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

                    // Base query with join to get route name
                    var sql = new StringBuilder();
                    sql.Append("WITH PickupsData AS ( ");
                    sql.Append("SELECT p.*, r.Name as RouteName, ROW_NUMBER() OVER (");

                    // Handle sorting
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        sql.Append($"ORDER BY {sortColumn} {sortColumnDirection}");
                    }
                    else
                    {
                        // Default sorting by SortOrder
                        sql.Append("ORDER BY p.SortOrder");
                    }

                    sql.Append(") AS RowNum ");
                    sql.Append("FROM TransportPickups p ");
                    sql.Append("INNER JOIN TransportRoute r ON p.RouteId = r.TransportRouteId ");
                    sql.Append("WHERE p.TenantID = @TenantID ");
                    sql.Append("AND p.SessionYear = @SessionYear ");
                    sql.Append("AND p.IsDeleted = 0 ");

                    // Add route filter if provided
                    if (validRouteId)
                    {
                        sql.Append("AND p.RouteId = @RouteId ");
                    }

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (p.PickupName LIKE @Search OR r.Name LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM PickupsData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM TransportPickups p 
                                    INNER JOIN TransportRoute r ON p.RouteId = r.TransportRouteId
                                    WHERE p.TenantID = @TenantID 
                                    AND p.SessionYear = @SessionYear 
                                    AND p.IsDeleted = 0 ";

                    if (validRouteId)
                    {
                        countSql += "AND p.RouteId = @RouteId ";
                    }

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (p.PickupName LIKE @Search OR r.Name LIKE @Search)");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@TenantCode", CurrentTenantCode);
                    parameters.Add("@SessionYear", sessionYear);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    if (validRouteId)
                    {
                        parameters.Add("@RouteId", parsedRouteId);
                    }

                    connection.Open();

                    // Execute queries
                    var data = connection.Query<TransportPickup>(sql.ToString(), parameters).ToList();
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
                    data = new List<TransportPickup>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetTransportPickupById(string id)
        {
            try
            {
                Guid transportPickupId;

                if (!Guid.TryParse(id, out transportPickupId))
                {
                    return Json(new { success = false, message = "Invalid transport pickup ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT p.*, r.Name as RouteName FROM TransportPickups p
                                  INNER JOIN TransportRoute r ON p.RouteId = r.TransportRouteId
                                  WHERE p.TransportPickupsId = @TransportPickupsId 
                                  AND p.TenantID = @TenantID 
                                  AND p.IsDeleted = 0";

                    var parameters = new
                    {
                        TransportPickupsId = transportPickupId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var transportPickup = connection.QueryFirstOrDefault<TransportPickup>(sql, parameters);

                    if (transportPickup != null)
                    {
                        return Json(new { success = true, data = transportPickup }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Transport pickup not found" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetNextSortOrder(string routeId)
        {
            try
            {
                Guid parsedRouteId;

                if (!Guid.TryParse(routeId, out parsedRouteId))
                {
                    return Json(new { success = false, message = "Invalid route ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT ISNULL(MAX(SortOrder), 0) + 1 
                                  FROM TransportPickups 
                                  WHERE TenantID = @TenantID 
                                  AND RouteId = @RouteId 
                                  AND SessionYear = @SessionYear
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        TenantID = CurrentTenantID,
                        RouteId = parsedRouteId,
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

        private bool IsPickupNameDuplicate(SqlConnection connection, TransportPickup transportPickup)
        {
            string sql = @"SELECT COUNT(*) FROM TransportPickups 
                          WHERE TenantID = @TenantID 
                          AND RouteId = @RouteId 
                          AND SessionYear = @SessionYear
                          AND PickupName = @PickupName 
                          AND IsDeleted = 0
                          AND TransportPickupsId != @TransportPickupsId";

            var parameters = new
            {
                transportPickup.TenantID,
                transportPickup.RouteId,
                transportPickup.SessionYear,
                transportPickup.PickupName,
                TransportPickupsId = transportPickup.TransportPickupsId
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertTransportPickup(TransportPickup transportPickup)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(transportPickup.PickupName))
                {
                    return Json(new { success = false, message = "Pickup name is required" });
                }

                if (transportPickup.RouteId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Route is required" });
                }

                // Set tenant and user information
                transportPickup.TransportPickupsId = Guid.NewGuid();  // Generate new ID
                transportPickup.TenantID = CurrentTenantID;
                transportPickup.TenantCode = Utils.ParseInt(CurrentTenantCode);
                transportPickup.SessionYear = CurrentSessionYear;
                transportPickup.SessionID = CurrentSessionID;
                transportPickup.CreatedBy = CurrentTenantUserID;
                transportPickup.CreatedDate = DateTime.Now;
                transportPickup.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate pickup name for the same route
                    if (IsPickupNameDuplicate(connection, transportPickup))
                    {
                        return Json(new { success = false, message = "A pickup point with this name already exists for the selected route" });
                    }

                    string sql = @"INSERT INTO TransportPickups 
                                  (TransportPickupsId, RouteId, PickupName, Fee, SortOrder, SessionYear,SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@TransportPickupsId, @RouteId, @PickupName, @Fee, @SortOrder, @SessionYear,@SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, transportPickup);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Transport pickup created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create transport pickup" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateTransportPickup(TransportPickup transportPickup)
        {
            try
            {
                // Validate required data
                if (transportPickup.TransportPickupsId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Transport pickup ID is required" });
                }

                if (string.IsNullOrEmpty(transportPickup.PickupName))
                {
                    return Json(new { success = false, message = "Pickup name is required" });
                }

                if (transportPickup.RouteId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Route is required" });
                }

                // Set update information
                transportPickup.TenantID = CurrentTenantID;
                transportPickup.ModifiedBy = CurrentTenantUserID;
                transportPickup.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate pickup name for the same route
                    if (IsPickupNameDuplicate(connection, transportPickup))
                    {
                        return Json(new { success = false, message = "A pickup point with this name already exists for the selected route" });
                    }

                    string sql = @"UPDATE TransportPickups 
                                  SET PickupName = @PickupName, 
                                      RouteId = @RouteId,
                                      Fee = @Fee,
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE TransportPickupsId = @TransportPickupsId 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, transportPickup);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Transport pickup updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update transport pickup. Transport pickup not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteTransportPickup(string id)
        {
            try
            {
                Guid transportPickupId;

                if (!Guid.TryParse(id, out transportPickupId))
                {
                    return Json(new { success = false, message = "Invalid transport pickup ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE TransportPickups 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE TransportPickupsId = @TransportPickupsId 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        TransportPickupsId = transportPickupId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Transport pickup deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete transport pickup. Transport pickup not found or access denied." });
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