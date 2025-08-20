using Dapper;
using ERPIndia.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class SubjectMappingModel
    {
        public Guid MappingID { get; set; }

        [Required]
        public Guid FeeHeadsID { get; set; }

        [Required]
        public Guid ClassID { get; set; }

        [Required]
        public Guid SectionID { get; set; }

        [Required]
        public Guid SubjectID { get; set; }

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

        // Additional display properties
        public string FeeHeadName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SubjectName { get; set; }
        public int TotalCount { get; set; }
    }
    public class SubjectMappingController : BaseController
    {
        private readonly string _connectionString;
        private readonly IFeeManagementRepository _feeRepository;

        public SubjectMappingController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            _feeRepository = new FeeManagementRepository(
                _connectionString,
                CurrentTenantID,
                CurrentSessionID,
                Utils.ParseInt(CurrentTenantCode),
                CurrentSessionYear,
                CurrentTenantUserID
            );
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetSubjectMappingList(string feeHeadId = "", string classId = "",
                                        string sectionId = "", string subjectId = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var sql = @"
                SELECT
                    sm.MappingID AS Id,
                    sm.ClassID,
                    sm.SectionID,
                    sm.SubjectID,
                    cls.ClassName,
                    sec.SectionName,
                    sub.SubjectName
                FROM AcademicSubjectMapping sm
                INNER JOIN AcademicClassMaster cls ON sm.ClassID = cls.ClassID
                INNER JOIN AcademicSectionMaster sec ON sm.SectionID = sec.SectionID
                INNER JOIN AcademicSubjectMaster sub ON sm.SubjectID = sub.SubjectID
                WHERE sm.TenantID = @TenantID
                  AND sm.SessionID = @SessionID
                  AND sm.IsDeleted = 0
            ";

                    var parameters = new DynamicParameters();
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@SessionID", CurrentSessionID);

                    if (!string.IsNullOrEmpty(classId))
                    {
                        sql += " AND sm.ClassID = @ClassID";
                        parameters.Add("@ClassID", Guid.Parse(classId));
                    }

                    if (!string.IsNullOrEmpty(sectionId))
                    {
                        sql += " AND sm.SectionID = @SectionID";
                        parameters.Add("@SectionID", Guid.Parse(sectionId));
                    }

                    if (!string.IsNullOrEmpty(subjectId))
                    {
                        sql += " AND sm.SubjectID = @SubjectID";
                        parameters.Add("@SubjectID", Guid.Parse(subjectId));
                    }

                    sql += " ORDER BY cls.SortOrder, cls.ClassName";

                    var results = connection.Query<SubjectMappingDto>(sql, parameters).ToList();

                    return Json(results, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public class SubjectMappingDto
        {
            public Guid Id { get; set; }
            public Guid ClassID { get; set; }
            public Guid SectionID { get; set; }
            public Guid SubjectID { get; set; }
            public string ClassName { get; set; }
            public string SectionName { get; set; }
            public string SubjectName { get; set; }
        }
        [HttpPost]
        public JsonResult CreateSubjectMapping(SubjectMappingModel model)
        {
            try
            {
                // Validate input
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid model" });
                }

                // Prepare model for insertion
                model.MappingID = Guid.NewGuid();
                model.TenantID = CurrentTenantID;
                model.TenantCode = Utils.ParseInt(CurrentTenantCode);
                model.SessionID = CurrentSessionID;
                model.SessionYear = CurrentSessionYear;
                model.CreatedBy = CurrentTenantUserID;
                model.CreatedDate = DateTime.Now;
                model.IsActive = true;
                model.IsDeleted = false;

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check for existing duplicate
                            var duplicateCheck = @"
                            SELECT COUNT(*) FROM AcademicSubjectMapping 
                            WHERE 
                                TenantID = @TenantID 
                                AND SessionID = @SessionID 
                                AND ClassID = @ClassID 
                                AND SectionID = @SectionID 
                                AND SubjectID = @SubjectID 
                                AND IsDeleted = 0";

                            var duplicateCount = connection.ExecuteScalar<int>(duplicateCheck, model, transaction);
                            if (duplicateCount > 0)
                            {
                                // Update existing record instead of creating a new one
                                var updateQuery = @"
                                UPDATE AcademicSubjectMapping 
                                SET 
                                    IsActive = @IsActive, 
                                    ModifiedBy = @CreatedBy, 
                                    ModifiedDate = @CreatedDate
                                WHERE 
                                    TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND ClassID = @ClassID 
                                    AND SectionID = @SectionID 
                                    AND SubjectID = @SubjectID 
                                    AND IsDeleted = 0";

                                int rowsAffected = connection.Execute(updateQuery, model, transaction);
                                transaction.Commit();
                                return Json(new { success = true, message = "Subject Mapping updated successfully" });
                            }

                            // Insert new Subject Mapping
                            var insertQuery = @"
                            INSERT INTO AcademicSubjectMapping 
                            (MappingID, ClassID, SectionID, SubjectID, 
                             SessionYear, SessionID, TenantID, TenantCode, 
                             IsActive, IsDeleted, CreatedBy, CreatedDate)
                            VALUES 
                            (@MappingID, @ClassID, @SectionID, @SubjectID, 
                             @SessionYear, @SessionID, @TenantID, @TenantCode, 
                             @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                            int rowsInserted = connection.Execute(insertQuery, model, transaction);

                            transaction.Commit();

                            return Json(new { success = true, message = "Subject Mapping created successfully" });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
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
        public JsonResult DeleteSubjectMapping(Guid id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Hard delete Subject Mapping
                            var deleteQuery = @"DELETE FROM AcademicSubjectMapping WHERE MappingID = @MappingID";
                            var parameters = new
                            {
                                MappingID = id,
                            };

                            int rowsAffected = connection.Execute(deleteQuery, parameters, transaction);

                            transaction.Commit();

                            return Json(new
                            {
                                success = rowsAffected > 0,
                                message = rowsAffected > 0
                                    ? "Subject mapping deleted successfully"
                                    : "Subject mapping not found or already deleted"
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetExistingMappings()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Query to get existing subject mappings
                    string query = @"
                        SELECT ClassID, SectionID, SubjectID
                        FROM AcademicSubjectMapping
                        WHERE TenantID = @TenantID AND SessionID = @SessionID AND IsDeleted = 0";

                    var parameters = new { TenantID = CurrentTenantID, SessionID = CurrentSessionID };
                    var result = connection.Query(query, parameters).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}