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
    public class Designation
    {
        public Guid DesignationID { get; set; }
        public int SortOrder { get; set; }
        public string DesignationName { get; set; }
        public string DesignationCode { get; set; }
        public string Description { get; set; }
        public int? Level { get; set; }
        public Guid? DepartmentID { get; set; }
        public string DepartmentName { get; set; } // For display purposes
        public Guid TenantID { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class DesignationController : BaseController
    {
        public ActionResult ManageDesignations()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetAllDesignations(bool checkDuplicate = false, string designationName = null, string designationCode = null)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates
                    if (checkDuplicate && (!string.IsNullOrEmpty(designationName) || !string.IsNullOrEmpty(designationCode)))
                    {
                        string sql1 = @"SELECT des.*, dept.DepartmentName 
                                      FROM HR_MST_Designation des
                                      LEFT JOIN HR_MST_Department dept ON des.DepartmentID = dept.DepartmentID
                                      WHERE des.TenantID = @TenantID 
                                      AND des.IsDeleted = 0";
                        
                        if (!string.IsNullOrEmpty(designationName))
                            sql1 += " AND des.DesignationName = @DesignationName";
                        
                        if (!string.IsNullOrEmpty(designationCode))
                            sql1 += " AND des.DesignationCode = @DesignationCode";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            DesignationName = designationName,
                            DesignationCode = designationCode
                        };

                        connection.Open();
                        var designations = connection.Query<Designation>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = designations });
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

                    // Base query with department join
                    var sql = new StringBuilder();
                    sql.Append("WITH DesignationData AS ( ");
                    sql.Append("SELECT des.*, dept.DepartmentName, ROW_NUMBER() OVER (");

                    // Handle sorting
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        // Map column names for joined table
                        if (sortColumn == "DepartmentName")
                            sql.Append($"ORDER BY dept.DepartmentName {sortColumnDirection}");
                        else
                            sql.Append($"ORDER BY des.{sortColumn} {sortColumnDirection}");
                    }
                    else
                    {
                        sql.Append("ORDER BY des.SortOrder");
                    }

                    sql.Append(") AS RowNum ");
                    sql.Append("FROM HR_MST_Designation des ");
                    sql.Append("LEFT JOIN HR_MST_Department dept ON des.DepartmentID = dept.DepartmentID ");
                    sql.Append("WHERE des.TenantID = @TenantID ");
                    sql.Append("AND des.IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (des.DesignationName LIKE @Search OR des.DesignationCode LIKE @Search OR des.Description LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM DesignationData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM HR_MST_Designation 
                                    WHERE TenantID = @TenantID 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (DesignationName LIKE @Search OR DesignationCode LIKE @Search OR Description LIKE @Search)");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@TenantCode", CurrentTenantCode);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Execute queries
                    var data = connection.Query<Designation>(sql.ToString(), parameters).ToList();
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
                    data = new List<Designation>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetDesignationById(string id)
        {
            try
            {
                Guid designationId;

                if (!Guid.TryParse(id, out designationId))
                {
                    return Json(new { success = false, message = "Invalid designation ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT des.*, dept.DepartmentName 
                                  FROM HR_MST_Designation des
                                  LEFT JOIN HR_MST_Department dept ON des.DepartmentID = dept.DepartmentID
                                  WHERE des.DesignationID = @DesignationID 
                                  AND des.TenantID = @TenantID 
                                  AND des.IsDeleted = 0";

                    var parameters = new
                    {
                        DesignationID = designationId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var designation = connection.QueryFirstOrDefault<Designation>(sql, parameters);

                    if (designation != null)
                    {
                        return Json(new { success = true, data = designation }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Designation not found" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetDepartments()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT DepartmentID, DepartmentName 
                                  FROM HR_MST_Department 
                                  WHERE TenantID = @TenantID 
                                  AND IsDeleted = 0 
                                  AND IsActive = 1
                                  ORDER BY DepartmentName";

                    var parameters = new
                    {
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var departments = connection.Query<dynamic>(sql, parameters).ToList();

                    return Json(new { success = true, data = departments }, JsonRequestBehavior.AllowGet);
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
                                  FROM HR_MST_Designation 
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

        private bool IsDesignationDuplicate(SqlConnection connection, Designation designation)
        {
            string sql = @"SELECT COUNT(*) FROM HR_MST_Designation 
                          WHERE TenantID = @TenantID 
                          AND IsDeleted = 0
                          AND DesignationID != @DesignationID
                          AND (DesignationName = @DesignationName OR 
                               (DesignationCode IS NOT NULL AND DesignationCode = @DesignationCode))";

            var parameters = new
            {
                designation.TenantID,
                designation.DesignationName,
                designation.DesignationCode,
                DesignationID = designation.DesignationID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertDesignation(Designation designation)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(designation.DesignationName))
                {
                    return Json(new { success = false, message = "Designation name is required" });
                }

                // Set tenant and user information
                designation.DesignationID = Guid.NewGuid();
                designation.TenantID = CurrentTenantID;
                designation.TenantCode = Utils.ParseInt(CurrentTenantCode);
                designation.CreatedBy = CurrentTenantUserID;
                designation.CreatedDate = DateTime.Now;
                designation.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate
                    if (IsDesignationDuplicate(connection, designation))
                    {
                        return Json(new { success = false, message = "A designation with this name or code already exists" });
                    }

                    string sql = @"INSERT INTO HR_MST_Designation 
                                  (DesignationID, SortOrder, DesignationName, DesignationCode, Description, 
                                   [Level], DepartmentID, TenantID, TenantCode, IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@DesignationID, @SortOrder, @DesignationName, @DesignationCode, @Description, 
                                   @Level, @DepartmentID, @TenantID, @TenantCode, @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, designation);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Designation created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create designation" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateDesignation(Designation designation)
        {
            try
            {
                // Validate required data
                if (designation.DesignationID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Designation ID is required" });
                }

                if (string.IsNullOrEmpty(designation.DesignationName))
                {
                    return Json(new { success = false, message = "Designation name is required" });
                }

                // Set update information
                designation.TenantID = CurrentTenantID;
                designation.ModifiedBy = CurrentTenantUserID;
                designation.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate
                    if (IsDesignationDuplicate(connection, designation))
                    {
                        return Json(new { success = false, message = "A designation with this name or code already exists" });
                    }

                    string sql = @"UPDATE HR_MST_Designation 
                                  SET DesignationName = @DesignationName, 
                                      DesignationCode = @DesignationCode,
                                      Description = @Description,
                                      [Level] = @Level,
                                      DepartmentID = @DepartmentID,
                                      SortOrder = @SortOrder,
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE DesignationID = @DesignationID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, designation);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Designation updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update designation. Designation not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteDesignation(string id)
        {
            try
            {
                Guid designationId;

                if (!Guid.TryParse(id, out designationId))
                {
                    return Json(new { success = false, message = "Invalid designation ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Soft delete
                    string sql = @"UPDATE HR_MST_Designation 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE DesignationID = @DesignationID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        DesignationID = designationId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Designation deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete designation. Designation not found or access denied." });
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