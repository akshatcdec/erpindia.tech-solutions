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
    public class AcademicClass
    {
        public Guid ClassID { get; set; }
        public int SortOrder { get; set; }
        public string ClassName { get; set; }
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

    public class AcademicClassController : BaseController
    {
        public ActionResult ManageClasses()
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
        public JsonResult GetAllClasses(string sessionId, int sessionYear, bool checkDuplicate = false, string className = null)
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
                        data = new List<AcademicClass>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(className))
                    {
                        string sql1 = @"SELECT * FROM AcademicClassMaster 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND ClassName = @ClassName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            ClassName = className
                        };

                        connection.Open();
                        var classes = connection.Query<AcademicClass>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = classes });
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
                    sql.Append("WITH ClassData AS ( ");
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
                    sql.Append("FROM AcademicClassMaster WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (ClassName LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM ClassData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM AcademicClassMaster 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (ClassName LIKE @Search)");
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
                    var data = connection.Query<AcademicClass>(sql.ToString(), parameters).ToList();
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
                    data = new List<AcademicClass>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetClassById(string id)
        {
            try
            {
                Guid classId;

                if (!Guid.TryParse(id, out classId))
                {
                    return Json(new { success = false, message = "Invalid class ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM AcademicClassMaster 
                                  WHERE ClassID = @ClassID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        ClassID = classId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var classData = connection.QueryFirstOrDefault<AcademicClass>(sql, parameters);

                    if (classData != null)
                    {
                        return Json(new { success = true, data = classData }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Class not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM AcademicClassMaster 
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

        private bool IsClassNameDuplicate(SqlConnection connection, AcademicClass classData)
        {
            string sql = @"SELECT COUNT(*) FROM AcademicClassMaster 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND ClassName = @ClassName 
                          AND IsDeleted = 0
                          AND ClassID != @ClassID";

            var parameters = new
            {
                classData.TenantID,
                classData.SessionID,
                classData.ClassName,
                ClassID = classData.ClassID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertClass(AcademicClass classData)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(classData.ClassName))
                {
                    return Json(new { success = false, message = "Class name is required" });
                }

                if (classData.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Set tenant and user information
                classData.ClassID = Guid.NewGuid();  // Generate new ID
                classData.TenantID = CurrentTenantID;
                classData.TenantCode = Utils.ParseInt(CurrentTenantCode);
                classData.CreatedBy = CurrentTenantUserID;
                classData.CreatedDate = DateTime.Now;
                classData.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate class name in the same session
                    if (IsClassNameDuplicate(connection, classData))
                    {
                        return Json(new { success = false, message = "A class with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO AcademicClassMaster 
                                  (ClassID, SortOrder, ClassName, SessionYear, SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@ClassID, @SortOrder, @ClassName, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, classData);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Class created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create class" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateClass(AcademicClass classData)
        {
            try
            {
                // Validate required data
                if (classData.ClassID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Class ID is required" });
                }

                if (string.IsNullOrEmpty(classData.ClassName))
                {
                    return Json(new { success = false, message = "Class name is required" });
                }

                // Set update information
                classData.TenantID = CurrentTenantID;
                classData.ModifiedBy = CurrentTenantUserID;
                classData.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate class name in the same session
                    if (IsClassNameDuplicate(connection, classData))
                    {
                        return Json(new { success = false, message = "A class with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE AcademicClassMaster 
                                  SET ClassName = @ClassName, 
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE ClassID = @ClassID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, classData);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Class updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update class. Class not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteClass(string id)
        {
            try
            {
                Guid classId;

                if (!Guid.TryParse(id, out classId))
                {
                    return Json(new { success = false, message = "Invalid class ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE AcademicClassMaster 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE ClassID = @ClassID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        ClassID = classId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Class deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete class. Class not found or access denied." });
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