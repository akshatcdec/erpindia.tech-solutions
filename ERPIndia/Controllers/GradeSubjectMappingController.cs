using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class GradeSubjectMappingModel
    {
        public Guid MappingID { get; set; }

        [Required]
        public Guid ClassID { get; set; }

        [Required]
        public Guid SectionID { get; set; }

        [Required]
        public Guid SubjectGradeID { get; set; }

        [Required]
        public Guid ExamID { get; set; }

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
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SubjectGradeName { get; set; }
        public string ExamName { get; set; }
        public int TotalCount { get; set; }
    }

    public class GradeSubjectMappingController : BaseController
    {
        private readonly string _connectionString;

        public GradeSubjectMappingController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetAllGradeSubjectMappings(DataTableParameters dtParams)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    WITH MappingCTE AS (
                        SELECT 
                            gsm.MappingID,
                            gsm.ClassID,
                            gsm.SectionID,
                            gsm.SubjectGradeID,
                            gsm.ExamID,
                            cls.ClassName,
                            sec.SectionName,
                            sg.SubjectGradeName,
                            em.ExamName,
                            ROW_NUMBER() OVER (ORDER BY gsm.CreatedDate DESC) AS RowNum,
                            COUNT(*) OVER () AS TotalCount
                        FROM AcademicGradeSubjectMapping gsm
                        INNER JOIN AcademicClassMaster cls ON gsm.ClassID = cls.ClassID
                        INNER JOIN AcademicSectionMaster sec ON gsm.SectionID = sec.SectionID
                        INNER JOIN AcademicSubjectGradeMaster sg ON gsm.SubjectGradeID = sg.SubjectGradeID
                        INNER JOIN ExamMaster em ON gsm.ExamID = em.ExamID
                        WHERE 
                            gsm.TenantID = @TenantID 
                            AND gsm.SessionID = @SessionID 
                            AND gsm.IsDeleted = 0
                    )
                    SELECT 
                        MappingID,
                        ClassID,
                        SectionID,
                        SubjectGradeID,
                        ExamID,
                        ClassName,
                        SectionName,
                        SubjectGradeName,
                        ExamName,
                        TotalCount
                    FROM MappingCTE
                    WHERE RowNum BETWEEN @StartRow AND @EndRow";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        command.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                        command.Parameters.AddWithValue("@StartRow", dtParams.Start + 1);
                        command.Parameters.AddWithValue("@EndRow", dtParams.Start + dtParams.Length);

                        var results = new List<dynamic>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new
                                {
                                    MappingID = reader["MappingID"],
                                    ClassID = reader["ClassID"],
                                    SectionID = reader["SectionID"],
                                    SubjectGradeID = reader["SubjectGradeID"],
                                    ExamID = reader["ExamID"],
                                    ClassName = reader["ClassName"].ToString(),
                                    SectionName = reader["SectionName"].ToString(),
                                    SubjectGradeName = reader["SubjectGradeName"].ToString(),
                                    ExamName = reader["ExamName"].ToString(),
                                    TotalCount = Convert.ToInt32(reader["TotalCount"])
                                });
                            }
                        }

                        int totalCount = results.Any() ? results.First().TotalCount : 0;

                        return Json(new
                        {
                            draw = dtParams.Draw,
                            recordsTotal = totalCount,
                            recordsFiltered = totalCount,
                            data = results
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = true,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetMappingList(string classId = "", string sectionId = "", string subjectId = "", string examId = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        gsm.MappingID AS Id,
                        gsm.ClassID,
                        gsm.SectionID,
                        gsm.SubjectGradeID,
                        gsm.ExamID,
                        cls.ClassName,
                        sec.SectionName,
                        sg.SubjectGradeName,
                        em.ExamName
                    FROM AcademicGradeSubjectMapping gsm
                    INNER JOIN AcademicClassMaster cls ON gsm.ClassID = cls.ClassID
                    INNER JOIN AcademicSectionMaster sec ON gsm.SectionID = sec.SectionID
                    INNER JOIN AcademicSubjectGradeMaster sg ON gsm.SubjectGradeID = sg.SubjectGradeID
                    INNER JOIN ExamMaster em ON gsm.ExamID = em.ExamID
                    WHERE 
                        gsm.TenantID = @TenantID 
                        AND gsm.SessionID = @SessionID 
                        AND gsm.IsDeleted = 0";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        command.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                        // Add filters if provided
                        if (!string.IsNullOrEmpty(classId))
                        {
                            query += " AND gsm.ClassID = @ClassID";
                            command.Parameters.AddWithValue("@ClassID", Guid.Parse(classId));
                        }

                        if (!string.IsNullOrEmpty(sectionId))
                        {
                            query += " AND gsm.SectionID = @SectionID";
                            command.Parameters.AddWithValue("@SectionID", Guid.Parse(sectionId));
                        }

                        if (!string.IsNullOrEmpty(subjectId))
                        {
                            query += " AND gsm.SubjectGradeID = @SubjectGradeID";
                            command.Parameters.AddWithValue("@SubjectGradeID", Guid.Parse(subjectId));
                        }

                        if (!string.IsNullOrEmpty(examId))
                        {
                            query += " AND gsm.ExamID = @ExamID";
                            command.Parameters.AddWithValue("@ExamID", Guid.Parse(examId));
                        }

                        query += " ORDER BY gsm.CreatedDate DESC";
                        command.CommandText = query;

                        var results = new List<dynamic>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new
                                {
                                    Id = Guid.Parse(reader["Id"].ToString()),
                                    ClassID = Guid.Parse(reader["ClassID"].ToString()),
                                    SectionID = Guid.Parse(reader["SectionID"].ToString()),
                                    SubjectGradeID = Guid.Parse(reader["SubjectGradeID"].ToString()),
                                    ExamID = Guid.Parse(reader["ExamID"].ToString()),
                                    ClassName = reader["ClassName"].ToString(),
                                    SectionName = reader["SectionName"].ToString(),
                                    SubjectGradeName = reader["SubjectGradeName"].ToString(),
                                    ExamName = reader["ExamName"].ToString()
                                });
                            }
                        }

                        return Json(results, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CreateGradeSubjectMapping(GradeSubjectMappingModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid model" });
                }

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
                            SELECT COUNT(*) FROM AcademicGradeSubjectMapping 
                            WHERE 
                                TenantID = @TenantID 
                                AND SessionID = @SessionID 
                                AND ClassID = @ClassID 
                                AND SectionID = @SectionID 
                                AND SubjectGradeID = @SubjectGradeID 
                                AND ExamID = @ExamID 
                                AND IsDeleted = 0";

                            var duplicateCount = connection.ExecuteScalar<int>(duplicateCheck, model, transaction);
                            if (duplicateCount > 0)
                            {
                                // Update existing record instead of creating a new one
                                var updateQuery = @"
                                UPDATE AcademicGradeSubjectMapping 
                                SET 
                                    IsActive = @IsActive, 
                                    ModifiedBy = @CreatedBy, 
                                    ModifiedDate = @CreatedDate
                                WHERE 
                                    TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND ClassID = @ClassID 
                                    AND SectionID = @SectionID 
                                    AND SubjectGradeID = @SubjectGradeID 
                                    AND ExamID = @ExamID 
                                    AND IsDeleted = 0";

                                int rowsAffected2 = connection.Execute(updateQuery, model, transaction);
                                transaction.Commit();
                                return Json(new { success = true, message = "Grade Subject Mapping updated successfully" });
                            }

                            // Insert new mapping
                            var insertQuery = @"
                            INSERT INTO AcademicGradeSubjectMapping 
                            (MappingID, ClassID, SectionID, SubjectGradeID, ExamID, 
                             SessionYear, SessionID, TenantID, TenantCode, 
                             IsActive, IsDeleted, CreatedBy, CreatedDate)
                            VALUES 
                            (@MappingID, @ClassID, @SectionID, @SubjectGradeID, @ExamID, 
                             @SessionYear, @SessionID, @TenantID, @TenantCode, 
                             @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                            int rowsAffected = connection.Execute(insertQuery, model, transaction);
                            transaction.Commit();

                            return Json(new { success = true, message = "Grade Subject Mapping created successfully" });
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
        public JsonResult UpdateGradeSubjectMapping(GradeSubjectMappingModel model)
        {
            try
            {
                if (model == null || model.MappingID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Invalid model" });
                }

                model.TenantID = CurrentTenantID;
                model.ModifiedBy = CurrentTenantUserID;
                model.ModifiedDate = DateTime.Now;

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check for existing duplicate excluding current record
                            var duplicateCheck = @"
                            SELECT COUNT(*) FROM AcademicGradeSubjectMapping 
                            WHERE 
                                TenantID = @TenantID 
                                AND SessionID = @SessionID 
                                AND ClassID = @ClassID 
                                AND SectionID = @SectionID 
                                AND SubjectGradeID = @SubjectGradeID 
                                AND ExamID = @ExamID 
                                AND MappingID != @MappingID
                                AND IsDeleted = 0";

                            var duplicateCount = connection.ExecuteScalar<int>(duplicateCheck, model, transaction);
                            if (duplicateCount > 0)
                            {
                                return Json(new { success = false, message = "A similar mapping already exists" });
                            }

                            // Update mapping
                            var updateQuery = @"
                            UPDATE AcademicGradeSubjectMapping 
                            SET 
                                ClassID = @ClassID, 
                                SectionID = @SectionID, 
                                SubjectGradeID = @SubjectGradeID,
                                ExamID = @ExamID,
                                IsActive = @IsActive, 
                                ModifiedBy = @ModifiedBy, 
                                ModifiedDate = @ModifiedDate
                            WHERE 
                                MappingID = @MappingID 
                                AND TenantID = @TenantID 
                                AND IsDeleted = 0";

                            int rowsAffected = connection.Execute(updateQuery, model, transaction);
                            transaction.Commit();

                            return Json(new { success = rowsAffected > 0, message = "Grade Subject Mapping updated successfully" });
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
        public JsonResult DeleteGradeSubjectMapping(Guid id)
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
                            // Hard delete (you can change to soft delete if needed)
                            var deleteQuery = @"
                            DELETE FROM AcademicGradeSubjectMapping 
                            WHERE MappingID = @MappingID AND TenantID = @TenantID";

                            var parameters = new
                            {
                                MappingID = id,
                                TenantID = CurrentTenantID
                            };

                            int rowsAffected = connection.Execute(deleteQuery, parameters, transaction);
                            transaction.Commit();

                            return Json(new
                            {
                                success = rowsAffected > 0,
                                message = rowsAffected > 0
                                    ? "Grade Subject Mapping deleted successfully"
                                    : "Mapping not found or already deleted"
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
    }
}