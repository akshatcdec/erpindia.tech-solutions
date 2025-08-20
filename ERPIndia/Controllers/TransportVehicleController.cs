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
    public class TransportVehicleController : BaseController
    {
        public class TransportVehicle
        {
            public Guid TransportVehiclesId { get; set; }
            public string VehicleNo { get; set; }
            public string DriverName { get; set; }
            public string Note { get; set; }
            public int SortOrder { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public int SessionYear { get; set; }
            public Guid SessionID { get; set; }
            public Guid TenantID { get; set; }
            public int TenantCode { get; set; }
            public Guid CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public Guid? ModifiedBy { get; set; }
            public DateTime? ModifiedDate { get; set; }
        }

        public ActionResult ManageTransportVehicles()
        {
            ViewBag.SessionId = CurrentSessionID;
            ViewBag.SessionYear = CurrentSessionYear;
            return View();
        }

        [HttpPost]
        public JsonResult GetAllTransportVehicles()
        {
            try
            {
                var request = HttpContext.Request;
                var draw = request.Form["draw"];
                var start = Convert.ToInt32(request.Form["start"] ?? "0");
                var length = Convert.ToInt32(request.Form["length"] ?? "10");
                var searchValue = request.Form["search[value]"];
                var sortColumn = request.Form["columns[" + request.Form["order[0][column]"] + "][name]"];
                var sortDirection = request.Form["order[0][dir]"];

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                var sql = new StringBuilder();
                sql.Append("WITH FilteredData AS ( ");
                sql.Append("SELECT * FROM TransportVehicles ");
                sql.Append("WHERE TenantID = @TenantID AND SessionYear = @SessionYear AND IsDeleted = 0 ");

                if (!string.IsNullOrEmpty(searchValue))
                {
                    sql.Append("AND (VehicleNo LIKE @Search OR DriverName LIKE @Search OR Note LIKE @Search) ");
                }

                sql.Append(") ");
                sql.Append("SELECT * FROM FilteredData ");

                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    sql.Append($"ORDER BY {sortColumn} {sortDirection} ");
                }
                else
                {
                    sql.Append("ORDER BY SortOrder ");
                }

                sql.Append("OFFSET @Start ROWS FETCH NEXT @Length ROWS ONLY");

                var countSql = @"SELECT COUNT(*) FROM TransportVehicles 
                                WHERE TenantID = @TenantID AND SessionYear = @SessionYear AND IsDeleted = 0";

                var filteredCountSql = new StringBuilder(countSql);
                if (!string.IsNullOrEmpty(searchValue))
                {
                    filteredCountSql.Append(" AND (VehicleNo LIKE @Search OR DriverName LIKE @Search OR Note LIKE @Search)");
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@SessionYear", CurrentSessionYear);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    var data = connection.Query<TransportVehicle>(sql.ToString(), parameters).ToList();
                    var recordsTotal = connection.ExecuteScalar<int>(countSql, parameters);
                    var recordsFiltered = connection.ExecuteScalar<int>(filteredCountSql.ToString(), parameters);

                    return Json(new
                    {
                        draw = draw,
                        recordsTotal = recordsTotal,
                        recordsFiltered = recordsFiltered,
                        data = data
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = "0",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<TransportVehicle>(),
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public JsonResult CheckDuplicateVehicleNo(string vehicleNo, string id)
        {
            try
            {
                Guid vehicleId = Guid.Empty;
                if (!string.IsNullOrEmpty(id))
                {
                    Guid.TryParse(id, out vehicleId);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT COUNT(*) FROM TransportVehicles 
                                  WHERE TenantID = @TenantID 
                                  AND SessionYear = @SessionYear 
                                  AND VehicleNo = @VehicleNo 
                                  AND IsDeleted = 0";

                    if (vehicleId != Guid.Empty)
                    {
                        sql += " AND TransportVehiclesId != @TransportVehiclesId";
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@SessionYear", CurrentSessionYear);
                    parameters.Add("@VehicleNo", vehicleNo);

                    if (vehicleId != Guid.Empty)
                    {
                        parameters.Add("@TransportVehiclesId", vehicleId);
                    }

                    connection.Open();
                    int count = connection.ExecuteScalar<int>(sql, parameters);

                    return Json(new { isDuplicate = count > 0 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetTransportVehicleById(string id)
        {
            try
            {
                Guid vehicleId;
                if (!Guid.TryParse(id, out vehicleId))
                {
                    return Json(new { success = false, message = "Invalid vehicle ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM TransportVehicles 
                                  WHERE TransportVehiclesId = @TransportVehiclesId 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        TransportVehiclesId = vehicleId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var vehicle = connection.QueryFirstOrDefault<TransportVehicle>(sql, parameters);

                    if (vehicle != null)
                    {
                        return Json(new { success = true, data = vehicle }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Vehicle not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM TransportVehicles 
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

        [HttpPost]
        public JsonResult SaveTransportVehicle(TransportVehicle vehicle)
        {
            try
            {
                if (string.IsNullOrEmpty(vehicle.VehicleNo))
                {
                    return Json(new { success = false, message = "Vehicle Number is required" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if this is an insert or update
                    bool isInsert = string.IsNullOrEmpty(vehicle.TransportVehiclesId.ToString()) || vehicle.TransportVehiclesId == Guid.Empty;

                    if (isInsert)
                    {
                        // INSERT
                        vehicle.TransportVehiclesId = Guid.NewGuid();
                        vehicle.TenantID = CurrentTenantID;
                        vehicle.TenantCode = Utils.ParseInt(CurrentTenantCode);
                        vehicle.SessionID = CurrentSessionID;
                        vehicle.SessionYear = CurrentSessionYear;
                        vehicle.CreatedBy = CurrentTenantUserID;
                        vehicle.CreatedDate = DateTime.Now;
                        vehicle.IsDeleted = false;

                        string sql = @"INSERT INTO TransportVehicles 
                                      (TransportVehiclesId, VehicleNo, DriverName, Note, SortOrder, 
                                       IsActive, IsDeleted, SessionYear, TenantID, TenantCode, 
                                       SessionID, CreatedBy, CreatedDate) 
                                      VALUES 
                                      (@TransportVehiclesId, @VehicleNo, @DriverName, @Note, @SortOrder, 
                                       @IsActive, @IsDeleted, @SessionYear, @TenantID, @TenantCode, 
                                       @SessionID, @CreatedBy, @CreatedDate)";

                        int rowsAffected = connection.Execute(sql, vehicle);

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Transport vehicle saved successfully!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to save transport vehicle" });
                        }
                    }
                    else
                    {
                        // UPDATE
                        vehicle.TenantID = CurrentTenantID;
                        vehicle.ModifiedBy = CurrentTenantUserID;
                        vehicle.ModifiedDate = DateTime.Now;

                        string sql = @"UPDATE TransportVehicles 
                                      SET VehicleNo = @VehicleNo, 
                                          DriverName = @DriverName, 
                                          Note = @Note, 
                                          SortOrder = @SortOrder, 
                                          IsActive = @IsActive, 
                                          ModifiedBy = @ModifiedBy, 
                                          ModifiedDate = @ModifiedDate 
                                      WHERE TransportVehiclesId = @TransportVehiclesId 
                                      AND TenantID = @TenantID 
                                      AND IsDeleted = 0";

                        int rowsAffected = connection.Execute(sql, vehicle);

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Transport vehicle updated successfully!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to update transport vehicle" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteTransportVehicle(string id)
        {
            try
            {
                Guid vehicleId;
                if (!Guid.TryParse(id, out vehicleId))
                {
                    return Json(new { success = false, message = "Invalid vehicle ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Soft delete
                    string sql = @"UPDATE TransportVehicles 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE TransportVehiclesId = @TransportVehiclesId 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        TransportVehiclesId = vehicleId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Transport vehicle deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete transport vehicle" });
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