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
    public class AcademicSection
    {
        public Guid SectionID { get; set; }
        public int SortOrder { get; set; }
        public string SectionName { get; set; }
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
    public class AcademicSectionController : BaseController
    {

        public ActionResult ManageSections()
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
        public JsonResult GetAllSections(string sessionId, int sessionYear, bool checkDuplicate = false, string sectionName = null)
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
                        data = new List<AcademicSection>()
                    });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    // If we're just checking for duplicates, use a simpler query
                    if (checkDuplicate && !string.IsNullOrEmpty(sectionName))
                    {
                        string sql1 = @"SELECT * FROM AcademicSectionMaster 
                                      WHERE TenantID = @TenantID 
                                      AND SessionID = @SessionID 
                                      AND SectionName = @SectionName
                                      AND IsDeleted = 0";

                        var parameters1 = new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            SectionName = sectionName
                        };

                        connection.Open();
                        var sections = connection.Query<AcademicSection>(sql1, parameters1).ToList();

                        return Json(new { success = true, data = sections });
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
                    sql.Append("WITH SectionData AS ( ");
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
                    sql.Append("FROM AcademicSectionMaster WHERE TenantID = @TenantID ");
                    sql.Append("AND SessionID = @SessionID ");
                    sql.Append("AND SessionYear = @SessionYear ");
                    sql.Append("AND IsDeleted = 0 ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (SectionName LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM SectionData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = @"SELECT COUNT(*) FROM AcademicSectionMaster 
                                    WHERE TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND SessionYear = @SessionYear 
                                    AND IsDeleted = 0";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder(countSql);
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (SectionName LIKE @Search)");
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
                    var data = connection.Query<AcademicSection>(sql.ToString(), parameters).ToList();
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
                    data = new List<AcademicSection>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetSectionById(string id)
        {
            try
            {
                Guid sectionId;

                if (!Guid.TryParse(id, out sectionId))
                {
                    return Json(new { success = false, message = "Invalid section ID" }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT * FROM AcademicSectionMaster 
                                  WHERE SectionID = @SectionID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        SectionID = sectionId,
                        TenantID = CurrentTenantID
                    };

                    connection.Open();
                    var section = connection.QueryFirstOrDefault<AcademicSection>(sql, parameters);

                    if (section != null)
                    {
                        return Json(new { success = true, data = section }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Section not found" }, JsonRequestBehavior.AllowGet);
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
                                  FROM AcademicSectionMaster 
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

        private bool IsSectionNameDuplicate(SqlConnection connection, AcademicSection section)
        {
            string sql = @"SELECT COUNT(*) FROM AcademicSectionMaster 
                          WHERE TenantID = @TenantID 
                          AND SessionID = @SessionID 
                          AND SectionName = @SectionName 
                          AND IsDeleted = 0
                          AND SectionID != @SectionID";

            var parameters = new
            {
                section.TenantID,
                section.SessionID,
                section.SectionName,
                SectionID = section.SectionID
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        [HttpPost]
        public JsonResult InsertSection(AcademicSection section)
        {
            try
            {
                // Validate required data
                if (string.IsNullOrEmpty(section.SectionName))
                {
                    return Json(new { success = false, message = "Section name is required" });
                }

                if (section.SessionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Session ID is required" });
                }

                // Set tenant and user information
                section.SectionID = Guid.NewGuid();  // Generate new ID
                section.TenantID = CurrentTenantID;
                section.TenantCode = Utils.ParseInt(CurrentTenantCode);
                section.CreatedBy = CurrentTenantUserID;
                section.CreatedDate = DateTime.Now;
                section.IsDeleted = false;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate section name in the same session
                    if (IsSectionNameDuplicate(connection, section))
                    {
                        return Json(new { success = false, message = "A section with this name already exists for the current session" });
                    }

                    string sql = @"INSERT INTO AcademicSectionMaster 
                                  (SectionID, SortOrder, SectionName, SessionYear, SessionID, TenantID, TenantCode, 
                                   IsActive, IsDeleted, CreatedBy, CreatedDate) 
                                  VALUES 
                                  (@SectionID, @SortOrder, @SectionName, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                   @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                    int rowsAffected = connection.Execute(sql, section);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Section created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create section" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateSection(AcademicSection section)
        {
            try
            {
                // Validate required data
                if (section.SectionID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Section ID is required" });
                }

                if (string.IsNullOrEmpty(section.SectionName))
                {
                    return Json(new { success = false, message = "Section name is required" });
                }

                // Set update information
                section.TenantID = CurrentTenantID;
                section.ModifiedBy = CurrentTenantUserID;
                section.ModifiedDate = DateTime.Now;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate section name in the same session
                    if (IsSectionNameDuplicate(connection, section))
                    {
                        return Json(new { success = false, message = "A section with this name already exists for the current session" });
                    }

                    string sql = @"UPDATE AcademicSectionMaster 
                                  SET SectionName = @SectionName, 
                                      SortOrder = @SortOrder, 
                                      IsActive = @IsActive, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE SectionID = @SectionID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    int rowsAffected = connection.Execute(sql, section);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Section updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update section. Section not found or access denied." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteSection(string id)
        {
            try
            {
                Guid sectionId;

                if (!Guid.TryParse(id, out sectionId))
                {
                    return Json(new { success = false, message = "Invalid section ID" });
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Instead of hard delete, we'll perform a soft delete
                    string sql = @"UPDATE AcademicSectionMaster 
                                  SET IsDeleted = 1, 
                                      ModifiedBy = @ModifiedBy, 
                                      ModifiedDate = @ModifiedDate 
                                  WHERE SectionID = @SectionID 
                                  AND TenantID = @TenantID 
                                  AND IsDeleted = 0";

                    var parameters = new
                    {
                        SectionID = sectionId,
                        TenantID = CurrentTenantID,
                        ModifiedBy = CurrentTenantUserID,
                        ModifiedDate = DateTime.Now
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Section deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete section. Section not found or access denied." });
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