using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class GradeConfigurationController : BaseController
    {
        public string ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private readonly DropdownController _dropdownController;

        public GradeConfigurationController()
        {
            _dropdownController = new DropdownController();
        }

        // GET: GradeConfiguration
        public ActionResult Index()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();
            var examsResult = _dropdownController.GetExamMarks();


            var model = new GradeConfigViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                ExamTypes = ConvertToSelectList(examsResult)
            };

            return View(model);
        }

        // Get grade configurations based on class, section, and exam
        [HttpPost]
        public JsonResult GetGradeConfigurations(string classId, string sectionId, string examTypeId)
        {
            try
            {
                // Get existing configurations
                var existingConfigurations = GetGradeConfigurationsFromDB(classId, sectionId, examTypeId);

                // Get all mapped subjects for this class and section
                var allMappedSubjects = GetMappedSubjectsFromDB(classId, sectionId);

                // Check for new subjects and merge
                var finalConfigurations = MergeConfigurations(existingConfigurations, allMappedSubjects,
                                                              classId, sectionId, examTypeId);

                return Json(new { success = true, data = finalConfigurations });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Update all grade configurations at once
        [HttpPost]
        public JsonResult UpdateAllGradeConfigurations()
        {
            try
            {
                var requestJson = Request.InputStream;
                requestJson.Position = 0;
                using (var reader = new System.IO.StreamReader(requestJson))
                {
                    var json = reader.ReadToEnd();
                    var request = JsonConvert.DeserializeObject<BulkUpdateAllRequest>(json);

                    if (request == null || request.Updates == null || !request.Updates.Any())
                    {
                        return Json(new { success = false, message = "No configurations to update" });
                    }

                    var errors = ValidateAllUpdates(request);
                    if (errors.Any())
                    {
                        return Json(new { success = false, message = "Validation errors found", errors = errors });
                    }

                    bool result = BulkUpdateAllGradeConfigurations(request);

                    if (result)
                    {
                        // Refresh data
                        var updatedConfigs = GetGradeConfigurationsFromDB(
                            request.ClassId,
                            request.SectionId,
                            request.ExamTypeId
                        );

                        return Json(new
                        {
                            success = true,
                            message = string.Format("Successfully updated {0} configurations", request.Updates.Count),
                            updatedData = updatedConfigs
                        });
                    }

                    return Json(new { success = false, message = "Update failed" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Private helper methods
        private List<SubjectMappingModel> GetMappedSubjectsFromDB(string classId, string sectionId)
        {
            var mappedSubjects = new List<SubjectMappingModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT DISTINCT
                        sm.MappingID,
                        sm.SubjectID,
                        s.SubjectName,
                        sm.ClassID,
                        sm.SectionID,
                        c.ClassName,
                        sec.SectionName
                    FROM AcademicSubjectMapping sm
                    INNER JOIN AcademicSubjectMaster s ON sm.SubjectID = s.SubjectID
                    INNER JOIN AcademicClassMaster c ON sm.ClassID = c.ClassID
                    INNER JOIN AcademicSectionMaster sec ON sm.SectionID = sec.SectionID
                    WHERE sm.ClassID = @ClassID 
                        AND sm.SectionID = @SectionID
                        AND sm.IsActive = 1
                        AND sm.IsDeleted = 0
                        AND sm.TenantID = @TenantID
                        AND sm.SessionID = @SessionID
                    ORDER BY s.SubjectName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    cmd.Parameters.AddWithValue("@SectionID", sectionId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            mappedSubjects.Add(new SubjectMappingModel
                            {
                                MappingID = Utils.ParseGuid(reader["MappingID"].ToString()),
                                SubjectID = Utils.ParseGuid(reader["SubjectID"].ToString()),
                                SubjectName = reader["SubjectName"].ToString(),
                                ClassID = Utils.ParseGuid(reader["ClassID"].ToString()),
                                SectionID = Utils.ParseGuid(reader["SectionID"].ToString()),
                                ClassName = reader["ClassName"].ToString(),
                                SectionName = reader["SectionName"].ToString()
                            });
                        }
                    }
                }
            }

            return mappedSubjects;
        }

        private List<GradeConfigModel> GetGradeConfigurationsFromDB(string classId, string sectionId, string examTypeId)
        {
            var configurations = new List<GradeConfigModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        gc.GradeConfigID,
                        gc.MappingID,
                        gc.ClassID,
                        gc.SectionID,
                        gc.SubjectID,
                        gc.ExamTypeID,
                        gc.MaxMarks,
                        gc.MinimumGrade,
                        gc.IsMinimumRequired,
                        gc.GradeFormula,
                        s.SubjectName,
                        c.ClassName,
                        sec.SectionName,
                        gc.IsActive,
                        gc.IsDeleted
                    FROM AcademicGradeConfiguration gc
                    INNER JOIN AcademicSubjectMaster s ON gc.SubjectID = s.SubjectID
                    INNER JOIN AcademicClassMaster c ON gc.ClassID = c.ClassID
                    INNER JOIN AcademicSectionMaster sec ON gc.SectionID = sec.SectionID
                    WHERE gc.ClassID = @ClassID 
                        AND gc.SectionID = @SectionID 
                        AND gc.ExamTypeID = @ExamTypeID
                        AND gc.IsDeleted = 0
                        AND gc.IsActive = 1
                        AND gc.TenantID = @TenantID
                        AND gc.SessionID = @SessionID
                    ORDER BY s.SubjectName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    cmd.Parameters.AddWithValue("@SectionID", sectionId);
                    cmd.Parameters.AddWithValue("@ExamTypeID", examTypeId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            configurations.Add(new GradeConfigModel
                            {
                                GradeConfigID = reader["GradeConfigID"].ToString(),
                                MappingID = reader["MappingID"]?.ToString(),
                                ClassID = reader["ClassID"].ToString(),
                                SectionID = reader["SectionID"].ToString(),
                                SubjectID = reader["SubjectID"].ToString(),
                                ExamTypeID = reader["ExamTypeID"].ToString(),
                                SubjectName = reader["SubjectName"].ToString(),
                                MaxMarks = Convert.ToInt32(reader["MaxMarks"]),
                                MinimumGrade = Convert.ToInt32(reader["MinimumGrade"]),
                                IsMinimumRequired = Convert.ToBoolean(reader["IsMinimumRequired"]),
                                GradeFormula = reader["GradeFormula"].ToString(),
                                ClassName = reader["ClassName"].ToString(),
                                SectionName = reader["SectionName"].ToString(),
                                IsNew = false
                            });
                        }
                    }
                }
            }

            return configurations;
        }

        private List<GradeConfigModel> MergeConfigurations(
            List<GradeConfigModel> existingConfigs,
            List<SubjectMappingModel> allMappedSubjects,
            string classId, string sectionId, string examTypeId)
        {
            var finalConfigurations = new List<GradeConfigModel>();

            // Add all existing configurations
            if (existingConfigs != null && existingConfigs.Any())
            {
                finalConfigurations.AddRange(existingConfigs);
            }

            // Find subjects that don't have grade configurations yet
            var existingSubjectIds = existingConfigs?.Select(c => c.SubjectID).ToList() ?? new List<string>();
            var newSubjects = allMappedSubjects.Where(s => !existingSubjectIds.Contains(s.SubjectID.ToString())).ToList();

            // Create configurations for new subjects
            if (newSubjects.Any())
            {
                var newConfigurations = new List<GradeConfigModel>();

                foreach (var subject in newSubjects)
                {
                    var configId = Guid.NewGuid();
                    var newConfig = new GradeConfigModel
                    {
                        GradeConfigID = configId.ToString(),
                        MappingID = subject.MappingID.ToString(),
                        ClassID = classId,
                        SectionID = sectionId,
                        SubjectID = subject.SubjectID.ToString(),
                        ExamTypeID = examTypeId,
                        SubjectName = subject.SubjectName,
                        ClassName = subject.ClassName,
                        SectionName = subject.SectionName,
                        MaxMarks = 100,
                        MinimumGrade = 33,
                        IsMinimumRequired = true,
                        GradeFormula = "100-75|H,74-60|F,59-45|S,45-33|T,31-1|F,-1|M",
                        IsNew = true
                    };

                    newConfigurations.Add(newConfig);
                    finalConfigurations.Add(newConfig);
                }

                // Insert new configurations into database
                if (newConfigurations.Any())
                {
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        InsertDefaultConfigurations(conn, newConfigurations, examTypeId);
                    }
                }
            }

            return finalConfigurations;
        }

        private void InsertDefaultConfigurations(SqlConnection conn, List<GradeConfigModel> configs, string examTypeId)
        {
            string insertQuery = @"
                INSERT INTO AcademicGradeConfiguration 
                (GradeConfigID, MappingID, ClassID, SectionID, SubjectID, ExamTypeID, MaxMarks, MinimumGrade, 
                 IsMinimumRequired, GradeFormula, SessionYear, SessionID, TenantID, TenantCode, CreatedBy)
                VALUES 
                (@GradeConfigID, @MappingID, @ClassID, @SectionID, @SubjectID, @ExamTypeID, @MaxMarks, @MinimumGrade,
                 @IsMinimumRequired, @GradeFormula, @SessionYear, @SessionID, @TenantID, @TenantCode, @CreatedBy)";

            foreach (var config in configs)
            {
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@GradeConfigID", config.GradeConfigID);
                    cmd.Parameters.AddWithValue("@MappingID", string.IsNullOrEmpty(config.MappingID) ? (object)DBNull.Value : config.MappingID);
                    cmd.Parameters.AddWithValue("@ClassID", config.ClassID);
                    cmd.Parameters.AddWithValue("@SectionID", config.SectionID);
                    cmd.Parameters.AddWithValue("@SubjectID", config.SubjectID);
                    cmd.Parameters.AddWithValue("@ExamTypeID", examTypeId);
                    cmd.Parameters.AddWithValue("@MaxMarks", config.MaxMarks);
                    cmd.Parameters.AddWithValue("@MinimumGrade", config.MinimumGrade);
                    cmd.Parameters.AddWithValue("@IsMinimumRequired", config.IsMinimumRequired);
                    cmd.Parameters.AddWithValue("@GradeFormula", config.GradeFormula);
                    cmd.Parameters.AddWithValue("@SessionYear", CurrentSessionYear);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@TenantCode", CurrentTenantCode);
                    cmd.Parameters.AddWithValue("@CreatedBy", CurrentTenantUserID);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<string> ValidateAllUpdates(BulkUpdateAllRequest request)
        {
            var errors = new List<string>();

            foreach (var update in request.Updates)
            {
                // Validate MaxMarks
                if (update.MaxMarks.HasValue)
                {
                    if (update.MaxMarks.Value < 1 || update.MaxMarks.Value > 500)
                    {
                        errors.Add($"Config {update.GradeConfigID}: Max Marks must be between 1 and 500");
                    }
                }

                // Validate MinimumGrade
                if (update.MinimumGrade.HasValue)
                {
                    if (update.MinimumGrade.Value < 0 || update.MinimumGrade.Value > 100)
                    {
                        errors.Add($"Config {update.GradeConfigID}: Minimum Grade must be between 0 and 100");
                    }
                }

                // Validate GradeFormula
                if (!string.IsNullOrEmpty(update.GradeFormula))
                {
                    var regex = new System.Text.RegularExpressions.Regex(@"^[\d\-|,A-Z]+$");
                    if (!regex.IsMatch(update.GradeFormula))
                    {
                        errors.Add($"Config {update.GradeConfigID}: Invalid grade formula format");
                    }
                }
            }

            return errors;
        }

        private bool BulkUpdateAllGradeConfigurations(BulkUpdateAllRequest request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var update in request.Updates)
                            {
                                string updateQuery = @"
                                    UPDATE AcademicGradeConfiguration 
                                    SET MaxMarks = @MaxMarks,
                                        MinimumGrade = @MinimumGrade,
                                        IsMinimumRequired = @IsMinimumRequired,
                                        GradeFormula = @GradeFormula,
                                        ModifiedBy = @ModifiedBy,
                                        ModifiedDate = GETDATE()
                                    WHERE GradeConfigID = @GradeConfigID
                                        AND TenantID = @TenantID";

                                using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@GradeConfigID", update.GradeConfigID);
                                    cmd.Parameters.AddWithValue("@MaxMarks", update.MaxMarks ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@MinimumGrade", update.MinimumGrade ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@IsMinimumRequired", update.IsMinimumRequired ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@GradeFormula", update.GradeFormula ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@ModifiedBy", CurrentTenantUserID);
                                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Update error: " + ex.Message);
                return false;
            }
        }

        private List<SelectListItem> ConvertToSelectList(JsonResult result)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = JsonConvert.DeserializeObject<BulkDropdownResponse>(jsonString);

                if (dropdownResponse != null && dropdownResponse.Success == true && dropdownResponse.Data != null)
                {
                    return dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }

                return new List<SelectListItem>();
            }
            catch
            {
                return new List<SelectListItem>();
            }
        }
    }

    // Supporting models
    public class GradeConfigViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public List<SelectListItem> ExamTypes { get; set; }
    }

    public class GradeConfigModel
    {
        public string GradeConfigID { get; set; }
        public string MappingID { get; set; }
        public string ClassID { get; set; }
        public string SectionID { get; set; }
        public string SubjectID { get; set; }
        public string ExamTypeID { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public int MaxMarks { get; set; }
        public int MinimumGrade { get; set; }
        public bool IsMinimumRequired { get; set; }
        public string GradeFormula { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public bool IsNew { get; set; }
    }

   

    public class BulkUpdateAllRequest
    {
        public List<UpdateAllItem> Updates { get; set; }
        public string ClassId { get; set; }
        public string SectionId { get; set; }
        public string ExamTypeId { get; set; }
    }

    public class UpdateAllItem
    {
        public string GradeConfigID { get; set; }
        public int? MaxMarks { get; set; }
        public int? MinimumGrade { get; set; }
        public bool? IsMinimumRequired { get; set; }
        public string GradeFormula { get; set; }
    }


   
}