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
    public class Department
    {
        public Guid DepartmentID { get; set; }
        public int SortOrder { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string Description { get; set; }
        public Guid? ParentDeptID { get; set; }
        public string ParentDeptName { get; set; } // For display purposes
        public Guid TenantID { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class DepartmentController : BaseController
    {
        public ActionResult ManageDepartments()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetAllDepartments(bool checkDuplicate = false, string departmentName = null, string departmentCode = null)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates
                    if (checkDuplicate && (!string.IsNullOrEmpty(departmentName) || !string.IsNullOrEmpty(departmentCode)))
                    {
                        string sql1 = @"SELECT d.*, p.DepartmentName as ParentDeptName 
                                      FROM HR_MST_Department d
                                      LEFT JOIN HR_MST_Department p ON d.ParentDeptID = p.DepartmentID
                                      WHERE d.TenantID = @TenantID 
                                      AND d.IsDeleted = 0";
                        
                        if (!string.IsNullOrEmpty(departmentName))
                            sql1 += " AND d.DepartmentName = @DepartmentName";
                        
                        if (!string.IsNullOrEmpty(departmentCode))
                            sql1 += " AND d.DepartmentCode = @DepartmentCode";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            DepartmentName = departmentName,
                            DepartmentCode = departmentCode
                        };

                        connection.Open();
                        var departments = connection.Query<Department>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = departments });
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

                    // Base query with parent department join
                    var sql = new StringBuilder();
                    sql.Append("WITH DepartmentData AS ( ");
                    sql.Append("SELECT d.*, p.DepartmentName as ParentDeptName, ROW_NUMBER() OVER (");

                    // Handle sorting
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        // Map column names for joined table
                        if (sortColumn == "ParentDeptName")
                            sql.Append($"ORDER BY p.DepartmentName {sortColumnDirection}");
                        else
                            sql.Append($"ORDER BY d.{sortColumn} {sortColumnDirection}");
                    }
                    else
                    {
                        sql.Append("ORDER BY d.SortOrder");
                    }

                    sql.Append(") AS RowNum ");
                    sql.Append("FROM HR_MST_Department d ");
                    sql.Append("LEFT JOIN HR_MST_Department p ON d.ParentDeptID = p.DepartmentID ");
                    sql.Append("WHERE d.TenantID = @TenantID ");
                    sql.Append("AND d.IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (d.DepartmentName LIKE @Search OR d.DepartmentCode LIKE @Search OR d.Description LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM DepartmentData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM HR_MST_Department 
                                    WHERE TenantID = @TenantID 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (DepartmentName LIKE @Search OR DepartmentCode LIKE @Search OR Description LIKE @Search)");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@TenantCode", CurrentTenantCode);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Execute queries
                    var data = connection.Query<Department>(sql.ToString(), parameters).ToList();
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
                    data = new List<Department>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetDepartmentById(string id)
        {
            try
            {
                Guid departmentId;

                if (!Guid.TryParse(id, out departmentId))
                {
                    return Json(new { success = false, message = "Invalid department ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT d.*, p.DepartmentName as ParentDeptName 
                                  FROM HR_MST_Department d
                                  LEFT JOIN HR_MST_Department p ON d.ParentDeptID = p.DepartmentID
                                  WHERE d.DepartmentID = @DepartmentID 
                                  AND d.TenantID = @TenantID 
                                  AND d.IsDeleted = 0";

                    var parameters = new
                    {
                        DepartmentID = departmentId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var department = connection.QueryFirstOrDefault<Department>(sql, parameters);

                    if (department != null)
                    {
                        return Json(new { success = true, data = department }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Department not found" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetParentDepartments(string excludeId = null)
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
                                  AND IsActive = 1";

                    if (!string.IsNullOrEmpty(excludeId))
                    {
                        sql += " AND DepartmentID != @ExcludeId";
                    }

                    sql += " ORDER BY DepartmentName";

                    var parameters = new
                    {
                        TenantID = CurrentTenantID,
                        ExcludeId = excludeId
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
                                  FROM HR_MST_Department 
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

        private bool IsDepartmentDuplicate(SqlConnection connection, Department department)
        {
            string sql = @"SELECT COUNT(*) FROM HR_MST_Department 
                          WHERE TenantID = @TenantID 
                          AND IsDeleted = 0
                          AND DepartmentID != @DepartmentID
                          AND (DepartmentName = @DepartmentName OR 
                               (DepartmentCode IS NOT NULL AND DepartmentCode = @DepartmentCode))";

            var parameters = new
            {
                department.TenantID,
                department.DepartmentName,
                department.DepartmentCode,
                DepartmentID = department.DepartmentID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        private bool HasCircularReference(SqlConnection connection, Guid departmentId, Guid? parentDeptId)
        {
            if (!parentDeptId.HasValue)
                return false;

            // Check if the parent department is a child of the current department
            string sql = @"WITH DepartmentHierarchy AS (
                            SELECT DepartmentID, ParentDeptID
                            FROM HR_MST_Department
                            WHERE DepartmentID = @ParentDeptID
                            AND TenantID = @TenantID
                            AND IsDeleted = 0
                            
                            UNION ALL
                            
                            SELECT d.DepartmentID, d.ParentDeptID
                            FROM HR_MST_Department d
                            INNER JOIN DepartmentHierarchy h ON d.ParentDeptID = h.DepartmentID
                            WHERE d.TenantID = @TenantID
                            AND d.IsDeleted = 0
                        )
                        SELECT COUNT(*) 
                        FROM DepartmentHierarchy 
                        WHERE ParentDeptID = @DepartmentID";

            var parameters = new
            {
                DepartmentID = departmentId,
                ParentDeptID = parentDeptId.Value,
                TenantID = CurrentTenantID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertDepartment(Department department)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(department.DepartmentName))
                {
                    return Json(new { success = false, message = "Department name is required" });
                }

                // Set tenant and user information
                department.DepartmentID = Guid.NewGuid();
                department.TenantID = CurrentTenantID;
                department.TenantCode = Utils.ParseInt(CurrentTenantCode);
                department.CreatedBy = CurrentTenantUserID;
                department.CreatedDate = DateTime.Now;
                department.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate
                    if (IsDepartmentDuplicate(connection, department))
                    {
                        return Json(new { success = false, message = "A department with this name or code already exists" });
                    }

                    string sql = @"INSERT INTO HR_MST_Department 
                                  (DepartmentID, SortOrder, DepartmentName, DepartmentCode, Description, 
                                   ParentDeptID, TenantID, TenantCode, IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@DepartmentID, @SortOrder, @DepartmentName, @DepartmentCode, @Description, 
                                   @ParentDeptID, @TenantID, @TenantCode, @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, department);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Department created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create department" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateDepartment(Department department)
        {
            try
            {
                // Validate required data
                if (department.DepartmentID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Department ID is required" });
                }

                if (string.IsNullOrEmpty(department.DepartmentName))
                {
                    return Json(new { success = false, message = "Department name is required" });
                }

                // Set update information
                department.TenantID = CurrentTenantID;
                department.ModifiedBy = CurrentTenantUserID;
                department.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate
                    if (IsDepartmentDuplicate(connection, department))
                    {
                        return Json(new { success = false, message = "A department with this name or code already exists" });
                    }

                    // Check for circular reference
                    if (HasCircularReference(connection, department.DepartmentID, department.ParentDeptID))
                    {
                        return Json(new { success = false, message = "Cannot set parent department: this would create a circular reference" });
                    }

                    string sql = @"UPDATE HR_MST_Department 
                                  SET DepartmentName = @DepartmentName, 
                                      DepartmentCode = @DepartmentCode,
                                      Description = @Description,
                                      ParentDeptID = @ParentDeptID,
                                      SortOrder = @SortOrder,
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE DepartmentID = @DepartmentID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, department);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Department updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update department. Department not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteDepartment(string id)
        {
            try
            {
                Guid departmentId;

                if (!Guid.TryParse(id, out departmentId))
                {
                    return Json(new { success = false, message = "Invalid department ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if department has child departments
                    string checkChildrenSql = @"SELECT COUNT(*) FROM HR_MST_Department 
                                               WHERE ParentDeptID = @DepartmentID 
                                               AND TenantID = @TenantID 
                                               AND IsDeleted = 0";

                    var checkParams = new
                    {
                        DepartmentID = departmentId,
                        TenantID = CurrentTenantID
                    };

                    int childCount = connection.ExecuteScalar<int>(checkChildrenSql, checkParams);

                    if (childCount > 0)
                    {
                        return Json(new { success = false, message = "Cannot delete department with child departments. Please delete or reassign child departments first." });
                    }

                    // Soft delete
                    string sql = @"UPDATE HR_MST_Department 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE DepartmentID = @DepartmentID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        DepartmentID = departmentId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Department deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete department. Department not found or access denied." });
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