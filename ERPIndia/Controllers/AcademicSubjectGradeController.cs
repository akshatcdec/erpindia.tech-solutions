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
    public class AcademicSubjectGrade
    {
        public Guid SubjectGradeID { get; set; }
        public int SortOrder { get; set; }
        public string SubjectGradeName { get; set; }
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

    public class AcademicSubjectGradeController : BaseController
    {
        public ActionResult ManageSubjectGrades()
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
        public JsonResult GetAllSubjectGrades(string sessionId, int sessionYear, bool checkDuplicate = false, string subjectGradeName = null)
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
                        data = new List<AcademicSubjectGrade>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(subjectGradeName))
                    {
                        string sql1 = @"SELECT * FROM AcademicSubjectGradeMaster 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND SubjectGradeName = @SubjectGradeName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            SubjectGradeName = subjectGradeName
                        };

                        connection.Open();
                        var subjectGrades = connection.Query<AcademicSubjectGrade>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = subjectGrades });
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
                    sql.Append("WITH SubjectGradeData AS ( ");
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
                    sql.Append("FROM AcademicSubjectGradeMaster WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (SubjectGradeName LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM SubjectGradeData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM AcademicSubjectGradeMaster 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (SubjectGradeName LIKE @Search)");
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
                    var data = connection.Query<AcademicSubjectGrade>(sql.ToString(), parameters).ToList();
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
                    data = new List<AcademicSubjectGrade>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetSubjectGradeById(string id)
        {
            try
            {
                Guid subjectGradeId;

                if (!Guid.TryParse(id, out subjectGradeId))
                {
                    return Json(new { success = false, message = "Invalid subject grade ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM AcademicSubjectGradeMaster 
                                  WHERE SubjectGradeID = @SubjectGradeID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        SubjectGradeID = subjectGradeId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var subjectGradeData = connection.QueryFirstOrDefault<AcademicSubjectGrade>(sql, parameters);

                    if (subjectGradeData != null)
                    {
                        return Json(new { success = true, data = subjectGradeData }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Subject grade not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM AcademicSubjectGradeMaster 
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

        private bool IsSubjectGradeNameDuplicate(SqlConnection connection, AcademicSubjectGrade subjectGradeData)
        {
            string sql = @"SELECT COUNT(*) FROM AcademicSubjectGradeMaster 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND SubjectGradeName = @SubjectGradeName 
                          AND IsDeleted = 0
                          AND SubjectGradeID != @SubjectGradeID";

            var parameters = new
            {
                subjectGradeData.TenantID,
                subjectGradeData.SessionID,
                subjectGradeData.SubjectGradeName,
                SubjectGradeID = subjectGradeData.SubjectGradeID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertSubjectGrade(AcademicSubjectGrade subjectGradeData)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(subjectGradeData.SubjectGradeName))
                {
                    return Json(new { success = false, message = "Subject grade name is required" });
                }

                if (subjectGradeData.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Set tenant and user information
                subjectGradeData.SubjectGradeID = Guid.NewGuid();  // Generate new ID
                subjectGradeData.TenantID = CurrentTenantID;
                subjectGradeData.TenantCode = Utils.ParseInt(CurrentTenantCode);
                subjectGradeData.CreatedBy = CurrentTenantUserID;
                subjectGradeData.CreatedDate = DateTime.Now;
                subjectGradeData.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate subject grade name in the same session
                    if (IsSubjectGradeNameDuplicate(connection, subjectGradeData))
                    {
                        return Json(new { success = false, message = "A subject grade with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO AcademicSubjectGradeMaster 
                                  (SubjectGradeID, SortOrder, SubjectGradeName, SessionYear, SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@SubjectGradeID, @SortOrder, @SubjectGradeName, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, subjectGradeData);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "The grade subject has been created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create subject grade" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateSubjectGrade(AcademicSubjectGrade subjectGradeData)
        {
            try
            {
                // Validate required data
                if (subjectGradeData.SubjectGradeID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Subject grade ID is required" });
                }

                if (string.IsNullOrEmpty(subjectGradeData.SubjectGradeName))
                {
                    return Json(new { success = false, message = "Subject grade name is required" });
                }

                // Set update information
                subjectGradeData.TenantID = CurrentTenantID;
                subjectGradeData.ModifiedBy = CurrentTenantUserID;
                subjectGradeData.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate subject grade name in the same session
                    if (IsSubjectGradeNameDuplicate(connection, subjectGradeData))
                    {
                        return Json(new { success = false, message = "A subject grade with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE AcademicSubjectGradeMaster 
                                  SET SubjectGradeName = @SubjectGradeName, 
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE SubjectGradeID = @SubjectGradeID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, subjectGradeData);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Subject grade updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update subject grade. Subject grade not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteSubjectGrade(string id)
        {
            try
            {
                Guid subjectGradeId;

                if (!Guid.TryParse(id, out subjectGradeId))
                {
                    return Json(new { success = false, message = "Invalid subject grade ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE AcademicSubjectGradeMaster 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE SubjectGradeID = @SubjectGradeID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        SubjectGradeID = subjectGradeId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Subject grade deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete subject grade. Subject grade not found or access denied." });
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