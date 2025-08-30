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
    public class EmployeeType
    {
        public Guid EmployeeTypeID { get; set; }
        public int SortOrder { get; set; }
        public string EmployeeTypeName { get; set; }
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

    public class EmployeeTypeController : BaseController
    {
        public ActionResult ManageEmployeeTypes()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetAllEmployeeTypes(bool checkDuplicate = false, string employeeTypeName = null)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates
                    if (checkDuplicate && !string.IsNullOrEmpty(employeeTypeName))
                    {
                        string sql1 = @"SELECT * FROM HR_MST_EmployeeType 
                                      WHERE TenantID = @TenantID 
                                      AND EmployeeTypeName = @EmployeeTypeName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            EmployeeTypeName = employeeTypeName
                        };

                        connection.Open();
                        var employeeTypes = connection.Query<EmployeeType>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = employeeTypes });
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
                    sql.Append("WITH EmployeeTypeData AS ( ");
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
                    sql.Append("FROM HR_MST_EmployeeType WHERE TenantID = @TenantID ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (EmployeeTypeName LIKE @Search OR Description LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM EmployeeTypeData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM HR_MST_EmployeeType 
                                    WHERE TenantID = @TenantID 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (EmployeeTypeName LIKE @Search OR Description LIKE @Search)");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@TenantCode", CurrentTenantCode);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Execute queries
                    var data = connection.Query<EmployeeType>(sql.ToString(), parameters).ToList();
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
                    data = new List<EmployeeType>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetEmployeeTypeById(string id)
        {
            try
            {
                Guid employeeTypeId;

                if (!Guid.TryParse(id, out employeeTypeId))
                {
                    return Json(new { success = false, message = "Invalid employee type ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM HR_MST_EmployeeType 
                                  WHERE EmployeeTypeID = @EmployeeTypeID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        EmployeeTypeID = employeeTypeId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var employeeType = connection.QueryFirstOrDefault<EmployeeType>(sql, parameters);

                    if (employeeType != null)
                    {
                        return Json(new { success = true, data = employeeType }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Employee type not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM HR_MST_EmployeeType 
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

        private bool IsEmployeeTypeDuplicate(SqlConnection connection, EmployeeType employeeType)
        {
            string sql = @"SELECT COUNT(*) FROM HR_MST_EmployeeType 
                          WHERE TenantID = @TenantID 
                          AND EmployeeTypeName = @EmployeeTypeName 
                          AND IsDeleted = 0
                          AND EmployeeTypeID != @EmployeeTypeID";

            var parameters = new
            {
                employeeType.TenantID,
                employeeType.EmployeeTypeName,
                EmployeeTypeID = employeeType.EmployeeTypeID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertEmployeeType(EmployeeType employeeType)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(employeeType.EmployeeTypeName))
                {
                    return Json(new { success = false, message = "Employee type name is required" });
                }

                // Set tenant and user information
                employeeType.EmployeeTypeID = Guid.NewGuid();
                employeeType.TenantID = CurrentTenantID;
                employeeType.TenantCode = Utils.ParseInt(CurrentTenantCode);
                employeeType.CreatedBy = CurrentTenantUserID;
                employeeType.CreatedDate = DateTime.Now;
                employeeType.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate
                    if (IsEmployeeTypeDuplicate(connection, employeeType))
                    {
                        return Json(new { success = false, message = "An employee type with this name already exists" });
                    }

                    string sql = @"INSERT INTO HR_MST_EmployeeType 
                                  (EmployeeTypeID, SortOrder, EmployeeTypeName, Description, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@EmployeeTypeID, @SortOrder, @EmployeeTypeName, @Description, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, employeeType);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Employee type created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create employee type" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateEmployeeType(EmployeeType employeeType)
        {
            try
            {
                // Validate required data
                if (employeeType.EmployeeTypeID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Employee type ID is required" });
                }

                if (string.IsNullOrEmpty(employeeType.EmployeeTypeName))
                {
                    return Json(new { success = false, message = "Employee type name is required" });
                }

                // Set update information
                employeeType.TenantID = CurrentTenantID;
                employeeType.ModifiedBy = CurrentTenantUserID;
                employeeType.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate
                    if (IsEmployeeTypeDuplicate(connection, employeeType))
                    {
                        return Json(new { success = false, message = "An employee type with this name already exists" });
                    }

                    string sql = @"UPDATE HR_MST_EmployeeType 
                                  SET EmployeeTypeName = @EmployeeTypeName, 
                                      Description = @Description,
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE EmployeeTypeID = @EmployeeTypeID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, employeeType);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Employee type updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update employee type. Employee type not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteEmployeeType(string id)
        {
            try
            {
                Guid employeeTypeId;

                if (!Guid.TryParse(id, out employeeTypeId))
                {
                    return Json(new { success = false, message = "Invalid employee type ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Soft delete
                    string sql = @"UPDATE HR_MST_EmployeeType 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE EmployeeTypeID = @EmployeeTypeID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        EmployeeTypeID = employeeTypeId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Employee type deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete employee type. Employee type not found or access denied." });
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