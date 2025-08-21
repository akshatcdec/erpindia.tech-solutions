using DocumentFormat.OpenXml.Wordprocessing;
using ERPIndia.Models.Exam;
using iText.StyledXmlParser.Jsoup.Select;
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
    public class StudentMarksController : BaseController
    {
        public string ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private readonly DropdownController _dropdownController;

        public StudentMarksController()
        {
            _dropdownController = new DropdownController();
        }
        [HttpPost]
        public JsonResult GetSubjects(string classId, string sectionId)
        {
            try
            {
                var subjects = GetMappedSubjectsFromDB(classId, sectionId);
                return Json(new { success = true, subjects = subjects });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult GetGradeSubjects(string classId, string sectionId, string examId)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(sectionId) || string.IsNullOrEmpty(examId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please select Class, Section, and Exam to load grade subjects"
                    });
                }

                var subjects = GetMappedGradeSubjectsFromDB(classId, sectionId, examId);

                if (subjects == null || !subjects.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No grade subjects found for the selected combination. Please configure grade subject mapping first."
                    });
                }

                return Json(new
                {
                    success = true,
                    subjects = subjects
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error loading grade subjects: " + ex.Message
                });
            }
        }
        [HttpPost]
        public JsonResult GetStudentMarksForSubject(string classId, string sectionId, string examTypeId, string subjectId)
        {
            try
            {
                // Get students with their existing marks for this subject
                var students = GetStudentsWithMarksFromDB(classId, sectionId, examTypeId, subjectId);

                // Get grade configuration for this subject
                var gradeConfig = GetSingleGradeConfigFromDB(classId, sectionId, examTypeId, subjectId);

                // Get subject details
                var subject = GetSubjectDetailsFromDB(subjectId);

                return Json(new
                {
                    success = true,
                    students = students,
                    gradeConfig = gradeConfig,
                    subject = subject
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading data: " + ex.Message });
            }
        }
        // Private helper methods
        private List<StudentWithMarksModel> GetStudentsWithMarksFromDB(string classId, string sectionId, string examTypeId, string subjectId)
        {
            var students = new List<StudentWithMarksModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                   SELECT 
                      s.StudentID,
                      s.AdmsnNo AS AdmissionNo,
                      s.RollNo AS RollNumber,
                      s.FirstName AS StudentName,
                      s.FatherName,
                      s.MotherName,
                      c.ClassName,
                    sec.SectionName,
                    sm.MarksObtained,
                    sm.Grade
                    FROM StudentInfoBasic s
                    INNER JOIN AcademicClassMaster c ON s.ClassID = c.ClassID
                    INNER JOIN AcademicSectionMaster sec ON s.SectionID = sec.SectionID
                    LEFT JOIN StudentMarks sm ON s.StudentID = sm.StudentID 
                        AND sm.SubjectID = @SubjectID 
                        AND sm.ExamTypeID = @ExamTypeID
                        AND sm.IsDeleted = 0
                    WHERE s.ClassID = @ClassID 
                        AND s.SectionID = @SectionID
                        AND s.IsActive = 1
                        AND s.IsDeleted = 0
                        AND s.TenantID = @TenantID
                        AND s.SessionID = @SessionID
                    ORDER BY CAST(s.RollNo AS INT), s.FirstName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    cmd.Parameters.AddWithValue("@SectionID", sectionId);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectId);
                    cmd.Parameters.AddWithValue("@ExamTypeID", examTypeId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new StudentWithMarksModel
                            {
                                StudentID = reader["StudentID"].ToString(),
                                AdmissionNo = reader["AdmissionNo"].ToString(),
                                RollNumber = reader["RollNumber"].ToString(),
                                StudentName = reader["StudentName"].ToString().Trim(),
                                FatherName = reader["FatherName"].ToString(),
                                ClassName = reader["ClassName"].ToString(),
                                SectionName = reader["SectionName"].ToString(),
                                MarksObtained = reader["MarksObtained"] != DBNull.Value ? Convert.ToDecimal(reader["MarksObtained"]) : (decimal?)null,
                                Grade = reader["Grade"]?.ToString()
                            });
                        }
                    }
                }
            }

            return students;
        }

        private GradeConfigModel GetSingleGradeConfigFromDB(string classId, string sectionId, string examTypeId, string subjectId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        gc.GradeConfigID,
                        gc.SubjectID,
                        gc.MaxMarks,
                        gc.MinimumGrade,
                        gc.IsMinimumRequired,
                        gc.GradeFormula,
                        s.SubjectName
                    FROM AcademicGradeConfiguration gc
                    INNER JOIN AcademicSubjectMaster s ON gc.SubjectID = s.SubjectID
                    WHERE gc.ClassID = @ClassID 
                        AND gc.SectionID = @SectionID 
                        AND gc.ExamTypeID = @ExamTypeID
                        AND gc.SubjectID = @SubjectID
                        AND gc.IsDeleted = 0
                        AND gc.IsActive = 1
                        AND gc.TenantID = @TenantID
                        AND gc.SessionID = @SessionID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    cmd.Parameters.AddWithValue("@SectionID", sectionId);
                    cmd.Parameters.AddWithValue("@ExamTypeID", examTypeId);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new GradeConfigModel
                            {
                                GradeConfigID = reader["GradeConfigID"].ToString(),
                                SubjectID = reader["SubjectID"].ToString(),
                                SubjectName = reader["SubjectName"].ToString(),
                                MaxMarks = Convert.ToInt32(reader["MaxMarks"]),
                                MinimumGrade = Convert.ToInt32(reader["MinimumGrade"]),
                                IsMinimumRequired = Convert.ToBoolean(reader["IsMinimumRequired"]),
                                GradeFormula = reader["GradeFormula"].ToString()
                            };
                        }
                    }
                }
            }

            // Return default config if not found
            return new GradeConfigModel
            {
                MaxMarks = 100,
                MinimumGrade = 33,
                IsMinimumRequired = true,
                GradeFormula = "100-75|A,74-60|B,59-45|C,44-33|D,32-0|F,-1|AB"
            };
        }

        private SubjectModel GetSubjectDetailsFromDB(string subjectId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT SubjectID, SubjectName
                    FROM AcademicSubjectMaster
                    WHERE SubjectID = @SubjectID
                        AND IsDeleted = 0
                        AND TenantID = @TenantID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SubjectID", subjectId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SubjectModel
                            {
                                SubjectID = reader["SubjectID"].ToString(),
                                SubjectName = reader["SubjectName"].ToString(),
                               
                            };
                        }
                    }
                }
            }

            return null;
        }
        private SubjectModel GetGradeSubjectDetailsFromDB(string subjectId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT SubjectGradeID as SubjectID,SubjectGradeName As SubjectName
                    FROM AcademicSubjectGradeMaster
                    WHERE SubjectGradeID = @SubjectID
                        AND IsDeleted = 0
                        AND TenantID = @TenantID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SubjectID", subjectId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SubjectModel
                            {
                                SubjectID = reader["SubjectID"].ToString(),
                                SubjectName = reader["SubjectName"].ToString(),

                            };
                        }
                    }
                }
            }

            return null;
        }
        // GET: StudentMarks
        public ActionResult Index()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();
            var examsResult = _dropdownController.GetExamMarks();

            var model = new StudentMarksViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                ExamTypes = ConvertToSelectList(examsResult)
            };

            return View(model);
        }
        public ActionResult SubjectWiseMarks()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();
            var examsResult = _dropdownController.GetExamMarks();

            var model = new StudentMarksViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                ExamTypes = ConvertToSelectList(examsResult)
            };

            return View(model);
        }
        // Get student marks data for entry
        [HttpPost]
       public JsonResult GetStudentMarksData(string classId, string sectionId, string examTypeId)
        {
            try
            {
                // Get students
                var students = GetStudentsFromDB(classId, sectionId);

                // Get subjects mapped to this class/section
                var subjects = GetMappedSubjectsFromDB(classId, sectionId);

                // Check if subjects are found
                if (subjects == null || !subjects.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Subject mapping not found. Please add subjects to the exam."
                    });
                }

                // Get grade configurations for validation
                var gradeConfigs = GetGradeConfigsFromDB(classId, sectionId, examTypeId);

                // Get existing marks if any
                var existingMarks = GetExistingMarksFromDB(classId, sectionId, examTypeId);

                // Get exam name
                var examName = GetExamNameFromDB(examTypeId);

                // Convert grade configs to dictionary for easier access
                var configDict = new Dictionary<string, object>();
                foreach (var config in gradeConfigs)
                {
                    configDict[config.SubjectID] = new
                    {
                        MaxMarks = config.MaxMarks,
                        MinimumGrade = config.MinimumGrade,
                        IsMinimumRequired = config.IsMinimumRequired,
                        GradeFormula = config.GradeFormula
                    };
                }

                return Json(new
                {
                    success = true,
                    students = students,
                    subjects = subjects,
                    gradeConfigs = configDict,
                    existingMarks = existingMarks,
                    examName = examName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading data: " + ex.Message });
            }
        }
        // Save student marks
        [HttpPost]
        public JsonResult SaveStudentMarks()
        {
            try
            {
                var requestJson = Request.InputStream;
                requestJson.Position = 0;
                using (var reader = new System.IO.StreamReader(requestJson))
                {
                    var json = reader.ReadToEnd();
                    var request = JsonConvert.DeserializeObject<SaveMarksRequest>(json);

                    if (request == null || request.Marks == null || !request.Marks.Any())
                    {
                        return Json(new { success = false, message = "No marks to save" });
                    }

                    // Validate marks against grade configurations
                    var validationErrors = ValidateMarks(request);
                    if (validationErrors.Any())
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Validation errors found",
                            errors = validationErrors
                        });
                    }

                    // Save marks to database
                    bool result = SaveMarksToDB(request);

                    if (result)
                    {
                        return Json(new
                        {
                            success = true,
                            message = string.Format("Successfully saved marks for {0} entries", request.Marks.Count)
                        });
                    }

                    return Json(new { success = false, message = "Failed to save marks" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Private helper methods
        private List<StudentModel> GetStudentsFromDB(string classId, string sectionId)
        {
            var students = new List<StudentModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                      s.StudentID,
                      s.AdmsnNo AS AdmissionNo,
                      s.RollNo AS RollNumber,
                      s.FirstName AS StudentName,
                      s.FatherName,
                      s.MotherName,
                      c.ClassName,
                    sec.SectionName
                    FROM StudentInfoBasic s
                    INNER JOIN AcademicClassMaster c ON s.ClassID = c.ClassID
                    INNER JOIN AcademicSectionMaster sec ON s.SectionID = sec.SectionID
                    WHERE s.ClassID = @ClassID 
                        AND s.SectionID = @SectionID
                        AND s.IsActive = 1
                        AND s.IsDeleted = 0
                        AND s.TenantID = @TenantID
                        AND s.SessionID = @SessionID
                    ORDER BY CAST(s.RollNo AS INT), s.FirstName";

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
                            students.Add(new StudentModel
                            {
                                StudentID = reader["StudentID"].ToString(),
                                AdmissionNo = reader["AdmissionNo"].ToString(),
                                RollNumber = reader["RollNumber"].ToString(),
                                StudentName = reader["StudentName"].ToString().Trim(),
                                FatherName = reader["FatherName"].ToString(),
                                MotherName = reader["MotherName"].ToString(),
                                ClassName = reader["ClassName"].ToString(),
                                SectionName = reader["SectionName"].ToString()
                            });
                        }
                    }
                }
            }

            return students;
        }
        private List<SubjectModel> GetMappedGradeSubjectsFromDB(string classId, string sectionId, string examId)
        {
            var subjects = new List<SubjectModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
            SELECT DISTINCT
                sm.SubjectGradeID as SubjectID,
                sm.SubjectGradeName as SubjectName
            FROM AcademicSubjectGradeMaster sm
            INNER JOIN AcademicGradeSubjectMapping gsm 
                ON sm.SubjectGradeID = gsm.SubjectGradeID
            WHERE 
                sm.IsActive = 1
                AND sm.IsDeleted = 0
                AND sm.TenantID = @TenantID
                AND sm.SessionID = @SessionID
                AND gsm.IsActive = 1
                AND gsm.IsDeleted = 0
                AND gsm.TenantID = @TenantID
                AND gsm.SessionID = @SessionID
                AND gsm.ClassID = @ClassID
                AND gsm.SectionID = @SectionID
                AND gsm.ExamID = @ExamID
            ORDER BY sm.SubjectGradeName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                    cmd.Parameters.AddWithValue("@ClassID", Guid.Parse(classId));
                    cmd.Parameters.AddWithValue("@SectionID", Guid.Parse(sectionId));
                    cmd.Parameters.AddWithValue("@ExamID", Guid.Parse(examId));

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subjects.Add(new SubjectModel
                            {
                                SubjectID = reader["SubjectID"].ToString(),
                                SubjectName = reader["SubjectName"].ToString()
                            });
                        }
                    }
                }
            }

            return subjects;
        }

        private List<SubjectModel> GetMappedSubjectsFromDB(string classId, string sectionId)
        {
            var subjects = new List<SubjectModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT DISTINCT
                        sm.SubjectID,
                        s.SubjectName,
                        sm.MappingID
                    FROM AcademicSubjectMapping sm
                    INNER JOIN AcademicSubjectMaster s ON sm.SubjectID = s.SubjectID
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
                            subjects.Add(new SubjectModel
                            {
                                SubjectID = reader["SubjectID"].ToString(),
                                SubjectName = reader["SubjectName"].ToString(),
                                MappingID = reader["MappingID"].ToString()
                            });
                        }
                    }
                }
            }

            return subjects;
        }

        private List<GradeConfigModel> GetGradeConfigsFromDB(string classId, string sectionId, string examTypeId)
        {
            var configs = new List<GradeConfigModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        gc.GradeConfigID,
                        gc.SubjectID,
                        gc.MaxMarks,
                        gc.MinimumGrade,
                        gc.IsMinimumRequired,
                        gc.GradeFormula,
                        s.SubjectName
                    FROM AcademicGradeConfiguration gc
                    INNER JOIN AcademicSubjectMaster s ON gc.SubjectID = s.SubjectID
                    WHERE gc.ClassID = @ClassID 
                        AND gc.SectionID = @SectionID 
                        AND gc.ExamTypeID = @ExamTypeID
                        AND gc.IsDeleted = 0
                        AND gc.IsActive = 1
                        AND gc.TenantID = @TenantID
                        AND gc.SessionID = @SessionID";

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
                            configs.Add(new GradeConfigModel
                            {
                                GradeConfigID = reader["GradeConfigID"].ToString(),
                                SubjectID = reader["SubjectID"].ToString(),
                                SubjectName = reader["SubjectName"].ToString(),
                                MaxMarks = Convert.ToInt32(reader["MaxMarks"]),
                                MinimumGrade = Convert.ToInt32(reader["MinimumGrade"]),
                                IsMinimumRequired = Convert.ToBoolean(reader["IsMinimumRequired"]),
                                GradeFormula = reader["GradeFormula"].ToString()
                            });
                        }
                    }
                }
            }

            return configs;
        }

        private List<StudentMarkModel> GetExistingMarksFromDB(string classId, string sectionId, string examTypeId)
        {
            var marks = new List<StudentMarkModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        sm.MarkID,
                        sm.StudentID,
                        sm.SubjectID,
                        sm.ExamTypeID,
                        sm.MarksObtained,
                        sm.Grade,
                        sm.Remarks
                    FROM StudentMarks sm
                    WHERE sm.ClassID = @ClassID 
                        AND sm.SectionID = @SectionID 
                        AND sm.ExamTypeID = @ExamTypeID
                        AND sm.IsDeleted = 0
                        AND sm.TenantID = @TenantID
                        AND sm.SessionID = @SessionID";

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
                            marks.Add(new StudentMarkModel
                            {
                                MarkID = reader["MarkID"].ToString(),
                                StudentID = reader["StudentID"].ToString(),
                                SubjectID = reader["SubjectID"].ToString(),
                                ExamTypeID = reader["ExamTypeID"].ToString(),
                                MarksObtained = reader["MarksObtained"] == DBNull.Value? (decimal?)null: Convert.ToDecimal(reader["MarksObtained"]),
                                Grade = reader["Grade"]?.ToString(),
                                Remarks = reader["Remarks"]?.ToString()
                            });
                        }
                    }
                }
            }

            return marks;
        }


        private string GetExamNameFromDB(string examTypeId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                // Query to get exam name, preferring AdmitCard value if it exists
                string query = @"
            SELECT 
               ExamName AS Name
            FROM ExamMaster
            WHERE ExamID = @ExamTypeID
                AND IsDeleted = 0
                AND IsActive = 1
                AND TenantID = @TenantID
                AND SessionID = @SessionID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ExamTypeID", examTypeId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "Exam";
                }
            }
        }
        private List<string> ValidateMarks(SaveMarksRequest request)
        {
            var errors = new List<string>();

            // Get grade configurations for validation
            var gradeConfigs = GetGradeConfigsFromDB(request.ClassId, request.SectionId, request.ExamTypeId);
            var configDict = gradeConfigs.ToDictionary(c => c.SubjectID);

            foreach (var mark in request.Marks)
            {
                // Skip validation for absent marks (-1)
                if (mark.MarksObtained == -1)
                    continue;

                if (configDict.ContainsKey(mark.SubjectID))
                {
                    var config = configDict[mark.SubjectID];

                    // Validate max marks
                    if (mark.MarksObtained > config.MaxMarks)
                    {
                        errors.Add($"Marks for {config.SubjectName} cannot exceed {config.MaxMarks}");
                    }

                    // Validate minimum if required (warning only)
                    if (config.IsMinimumRequired && mark.MarksObtained < config.MinimumGrade)
                    {
                        // This is just a warning, not an error
                        // You can track this separately if needed
                    }
                }
            }

            return errors;
        }

        private bool SaveMarksToDB(SaveMarksRequest request)
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
                            // Get grade configurations for grade calculation
                            var gradeConfigs = GetGradeConfigsFromDB(request.ClassId, request.SectionId, request.ExamTypeId);
                            var configDict = gradeConfigs.ToDictionary(c => c.SubjectID);

                            foreach (var mark in request.Marks)
                            {
                                // Debug logging to track what we're processing
                                System.Diagnostics.Debug.WriteLine($"Processing StudentID: {mark.StudentID}, SubjectID: {mark.SubjectID}, MarksObtained: {mark.MarksObtained}");

                                // Handle different mark scenarios:
                                // 1. HasValue = true, Value = 0 -> Save as 0 (valid zero marks)
                                // 2. HasValue = true, Value > 0 -> Save the actual marks
                                // 3. HasValue = false (null/blank) -> Save as null or 0 based on business logic

                                decimal? marksToSave = mark.MarksObtained;

                                // Option A: If you want to save blank entries as NULL
                                // (Keep marksToSave as is - it will be null)

                                // Option B: If you want to save blank entries as 0, uncomment below:
                                // if (!mark.MarksObtained.HasValue)
                                // {
                                //     marksToSave = 0;
                                // }

                                // Calculate grade based on the marks we're going to save
                                GradeConfigModel config = configDict.ContainsKey(mark.SubjectID)
                                    ? configDict[mark.SubjectID]
                                    : null;

                                string grade = CalculateGrade(marksToSave, config);
                                System.Diagnostics.Debug.WriteLine($"Calculated grade: {grade}");

                                // Check if mark already exists
                                string checkQuery = @"
                            SELECT MarkID FROM StudentMarks 
                            WHERE StudentID = @StudentID 
                                AND SubjectID = @SubjectID 
                                AND ExamTypeID = @ExamTypeID
                                AND TenantID = @TenantID
                                AND SessionID = @SessionID
                                AND IsDeleted = 0";

                                string existingMarkId = null;
                                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                                {
                                    checkCmd.Parameters.AddWithValue("@StudentID", mark.StudentID);
                                    checkCmd.Parameters.AddWithValue("@SubjectID", mark.SubjectID);
                                    checkCmd.Parameters.AddWithValue("@ExamTypeID", mark.ExamTypeID);
                                    checkCmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                                    checkCmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                                    var result = checkCmd.ExecuteScalar();
                                    existingMarkId = result?.ToString();
                                }

                                if (!string.IsNullOrEmpty(existingMarkId))
                                {
                                    // Update existing mark
                                    System.Diagnostics.Debug.WriteLine($"Updating existing mark with MarkID: {existingMarkId}");

                                    string updateQuery = @"
                                UPDATE StudentMarks 
                                SET MarksObtained = @MarksObtained,
                                    Grade = @Grade,
                                    ModifiedBy = @ModifiedBy,
                                    ModifiedDate = GETDATE()
                                WHERE MarkID = @MarkID";

                                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction))
                                    {
                                        updateCmd.Parameters.AddWithValue("@MarkID", existingMarkId);

                                        // Handle nullable MarksObtained - allow both 0 and null
                                        if (marksToSave.HasValue)
                                        {
                                            updateCmd.Parameters.AddWithValue("@MarksObtained", marksToSave.Value);
                                            System.Diagnostics.Debug.WriteLine($"Updating with marks: {marksToSave.Value}");
                                        }
                                        else
                                        {
                                            updateCmd.Parameters.AddWithValue("@MarksObtained", DBNull.Value);
                                            System.Diagnostics.Debug.WriteLine("Updating with NULL marks");
                                        }

                                        updateCmd.Parameters.AddWithValue("@Grade", grade ?? (object)DBNull.Value);
                                        updateCmd.Parameters.AddWithValue("@ModifiedBy", CurrentTenantUserID);

                                        int rowsAffected = updateCmd.ExecuteNonQuery();
                                        System.Diagnostics.Debug.WriteLine($"Update affected {rowsAffected} rows");
                                    }
                                }
                                else
                                {
                                    // Insert new mark
                                    System.Diagnostics.Debug.WriteLine("Inserting new mark record");

                                    string insertQuery = @"
                                INSERT INTO StudentMarks 
                                (MarkID, StudentID, SubjectID, ClassID, SectionID, ExamTypeID, 
                                 MarksObtained, Grade, SessionYear, SessionID, TenantID, TenantCode, CreatedBy, CreatedDate, IsDeleted)
                                VALUES 
                                (@MarkID, @StudentID, @SubjectID, @ClassID, @SectionID, @ExamTypeID,
                                 @MarksObtained, @Grade, @SessionYear, @SessionID, @TenantID, @TenantCode, @CreatedBy, GETDATE(), 0)";

                                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction))
                                    {
                                        string newMarkId = Guid.NewGuid().ToString();
                                        insertCmd.Parameters.AddWithValue("@MarkID", newMarkId);
                                        insertCmd.Parameters.AddWithValue("@StudentID", mark.StudentID);
                                        insertCmd.Parameters.AddWithValue("@SubjectID", mark.SubjectID);
                                        insertCmd.Parameters.AddWithValue("@ClassID", request.ClassId);
                                        insertCmd.Parameters.AddWithValue("@SectionID", request.SectionId);
                                        insertCmd.Parameters.AddWithValue("@ExamTypeID", mark.ExamTypeID);

                                        // Handle nullable MarksObtained - allow both 0 and null
                                        if (marksToSave.HasValue)
                                        {
                                            insertCmd.Parameters.AddWithValue("@MarksObtained", marksToSave.Value);
                                            System.Diagnostics.Debug.WriteLine($"Inserting with marks: {marksToSave.Value}");
                                        }
                                        else
                                        {
                                            insertCmd.Parameters.AddWithValue("@MarksObtained", DBNull.Value);
                                            System.Diagnostics.Debug.WriteLine("Inserting with NULL marks");
                                        }

                                        insertCmd.Parameters.AddWithValue("@Grade", grade ?? (object)DBNull.Value);
                                        insertCmd.Parameters.AddWithValue("@SessionYear", CurrentSessionYear);
                                        insertCmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                                        insertCmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                                        insertCmd.Parameters.AddWithValue("@TenantCode", CurrentTenantCode);
                                        insertCmd.Parameters.AddWithValue("@CreatedBy", CurrentTenantUserID);

                                        int rowsAffected = insertCmd.ExecuteNonQuery();
                                        System.Diagnostics.Debug.WriteLine($"Insert affected {rowsAffected} rows, New MarkID: {newMarkId}");
                                    }
                                }
                            }

                            transaction.Commit();
                            System.Diagnostics.Debug.WriteLine("Transaction committed successfully");
                            return true;
                        }
                        catch (Exception innerEx)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine("Transaction error: " + innerEx.Message);
                            System.Diagnostics.Debug.WriteLine("Inner exception stack trace: " + innerEx.StackTrace);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Save marks error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                return false;
            }
        }

        private string CalculateGrade(decimal? marksObtained, GradeConfigModel config)
        {
            // Check config first
            if (config == null || string.IsNullOrEmpty(config.GradeFormula))
                return null;

            // Check if marks are null
            if (!marksObtained.HasValue)
                return null;

            decimal marks = marksObtained.Value;

            // Handle absent case
            if (marks == -1)
                return "AB";

            // Parse grade formula (e.g., "100-75|H,74-60|F,59-45|S,45-33|T,31-1|F,-1|M")
            var gradeParts = config.GradeFormula.Split(',');

            foreach (var part in gradeParts)
            {
                var rangeParts = part.Split('|');
                if (rangeParts.Length != 2)
                    continue;

                var range = rangeParts[0].Trim();
                var grade = rangeParts[1].Trim();

                // Handle special case for absent (already handled above, but keeping for completeness)
                if (range == "-1" && marks == -1)
                    return grade;

                // Parse range
                var bounds = range.Split('-');
                if (bounds.Length == 2)
                {
                    // Parse as decimal to match the marks type
                    if (decimal.TryParse(bounds[0], out decimal upper) &&
                        decimal.TryParse(bounds[1], out decimal lower))
                    {
                        // Fix the comparison logic (upper bound first, lower bound second)
                        if (marks <= upper && marks >= lower)
                            return grade;
                    }
                }
            }

            return null; // No grade found
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

        public ActionResult SubjectGradeEntry()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();
            var examsResult = _dropdownController.GetExamMarks();

            var model = new StudentMarksViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                ExamTypes = ConvertToSelectList(examsResult)
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult GetStudentGradesForSubject(string classId, string sectionId, string examTypeId, string subjectId)
        {
            try
            {
                // Get students with their existing grades for this subject
                var students = GetStudentsWithGradesFromDB(classId, sectionId, examTypeId, subjectId);

                // Get grade options from master
                var gradeOptions = GetGradeOptionsFromDB();

                // Get subject details
                var subject = GetGradeSubjectDetailsFromDB(subjectId);

                return Json(new
                {
                    success = true,
                    students = students,
                    gradeOptions = gradeOptions,
                    subject = subject
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading data: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SaveStudentGrades()
        {
            try
            {
                var requestJson = Request.InputStream;
                requestJson.Position = 0;
                using (var reader = new System.IO.StreamReader(requestJson))
                {
                    var json = reader.ReadToEnd();
                    var request = JsonConvert.DeserializeObject<SaveGradesRequest>(json);

                    if (request == null || request.Grades == null || !request.Grades.Any())
                    {
                        return Json(new { success = false, message = "No grades to save" });
                    }

                    // Save grades to database
                    bool result = SaveGradesToDB(request);

                    if (result)
                    {
                        return Json(new
                        {
                            success = true,
                            message = string.Format("Successfully saved grades for {0} entries", request.Grades.Count)
                        });
                    }

                    return Json(new { success = false, message = "Failed to save grades" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Private helper methods for grades
        private List<StudentWithGradesModel> GetStudentsWithGradesFromDB(string classId, string sectionId, string examTypeId, string subjectId)
        {
            var students = new List<StudentWithGradesModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
           SELECT 
              s.StudentID,
              s.AdmsnNo AS AdmissionNo,
              s.RollNo AS RollNumber,
              s.FirstName AS StudentName,
              s.FatherName,
              s.MotherName,
              c.ClassName,
              sec.SectionName,
              sg.Grade,
              sg.Remarks
            FROM StudentInfoBasic s
            INNER JOIN AcademicClassMaster c ON s.ClassID = c.ClassID
            INNER JOIN AcademicSectionMaster sec ON s.SectionID = sec.SectionID
            LEFT JOIN StudentGrades sg ON s.StudentID = sg.StudentID 
                AND sg.SubjectID = @SubjectID 
                AND sg.ExamTypeID = @ExamTypeID
                AND sg.IsDeleted = 0
            WHERE s.ClassID = @ClassID 
                AND s.SectionID = @SectionID
                AND s.IsActive = 1
                AND s.IsDeleted = 0
                AND s.TenantID = @TenantID
                AND s.SessionID = @SessionID
            ORDER BY CAST(s.RollNo AS INT), s.FirstName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    cmd.Parameters.AddWithValue("@SectionID", sectionId);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectId);
                    cmd.Parameters.AddWithValue("@ExamTypeID", examTypeId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new StudentWithGradesModel
                            {
                                StudentID = reader["StudentID"].ToString(),
                                AdmissionNo = reader["AdmissionNo"].ToString(),
                                RollNumber = reader["RollNumber"].ToString(),
                                StudentName = reader["StudentName"].ToString().Trim(),
                                FatherName = reader["FatherName"].ToString(),
                                ClassName = reader["ClassName"].ToString(),
                                SectionName = reader["SectionName"].ToString(),
                                Grade = reader["Grade"]?.ToString(),
                                Remarks = reader["Remarks"]?.ToString()
                            });
                        }
                    }
                }
            }

            return students;
        }

        private List<GradeOption> GetGradeOptionsFromDB()
        {
            var gradeOptions = new List<GradeOption>();

            if (gradeOptions.Count == 0)
            {
                gradeOptions = new List<GradeOption>
        {
            new GradeOption { GradeName = "A+", SortOrder = 1 },
            new GradeOption { GradeName = "A", SortOrder = 2 },
            new GradeOption { GradeName = "B+", SortOrder = 3 },
            new GradeOption { GradeName = "B", SortOrder = 4 },
            new GradeOption { GradeName = "C+", SortOrder = 5 },
            new GradeOption { GradeName = "C", SortOrder = 6 },
            new GradeOption { GradeName = "D", SortOrder = 7 },
            new GradeOption { GradeName = "F", SortOrder = 8 },
            new GradeOption { GradeName = "AB", SortOrder = 9 }
        };
            }

            return gradeOptions;
        }

        private bool SaveGradesToDB(SaveGradesRequest request)
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
                            foreach (var grade in request.Grades)
                            {
                                // Check if grade already exists
                                string checkQuery = @"
                            SELECT GradeID FROM StudentGrades 
                            WHERE StudentID = @StudentID 
                                AND SubjectID = @SubjectID 
                                AND ExamTypeID = @ExamTypeID
                                AND TenantID = @TenantID
                                AND SessionID = @SessionID
                                AND IsDeleted = 0";

                                string existingGradeId = null;
                                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                                {
                                    checkCmd.Parameters.AddWithValue("@StudentID", grade.StudentID);
                                    checkCmd.Parameters.AddWithValue("@SubjectID", grade.SubjectID);
                                    checkCmd.Parameters.AddWithValue("@ExamTypeID", grade.ExamTypeID);
                                    checkCmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                                    checkCmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                                    var result = checkCmd.ExecuteScalar();
                                    existingGradeId = result?.ToString();
                                }

                                if (!string.IsNullOrEmpty(existingGradeId))
                                {
                                    // Update existing grade
                                    string updateQuery = @"
                                UPDATE StudentGrades 
                                SET Grade = @Grade,
                                    Remarks = @Remarks,
                                    ModifiedBy = @ModifiedBy,
                                    ModifiedDate = GETDATE()
                                WHERE GradeID = @GradeID";

                                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction))
                                    {
                                        updateCmd.Parameters.AddWithValue("@GradeID", existingGradeId);
                                        updateCmd.Parameters.AddWithValue("@Grade", grade.Grade ?? (object)DBNull.Value);
                                        updateCmd.Parameters.AddWithValue("@Remarks", grade.Remarks ?? (object)DBNull.Value);
                                        updateCmd.Parameters.AddWithValue("@ModifiedBy", CurrentTenantUserID);

                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    // Insert new grade
                                    string insertQuery = @"
                                INSERT INTO StudentGrades 
                                (GradeID, StudentID, SubjectID, ClassID, SectionID, ExamTypeID, 
                                 Grade, Remarks, SessionYear, SessionID, TenantID, TenantCode, CreatedBy)
                                VALUES 
                                (@GradeID, @StudentID, @SubjectID, @ClassID, @SectionID, @ExamTypeID,
                                 @Grade, @Remarks, @SessionYear, @SessionID, @TenantID, @TenantCode, @CreatedBy)";

                                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@GradeID", Guid.NewGuid().ToString());
                                        insertCmd.Parameters.AddWithValue("@StudentID", grade.StudentID);
                                        insertCmd.Parameters.AddWithValue("@SubjectID", grade.SubjectID);
                                        insertCmd.Parameters.AddWithValue("@ClassID", request.ClassId);
                                        insertCmd.Parameters.AddWithValue("@SectionID", request.SectionId);
                                        insertCmd.Parameters.AddWithValue("@ExamTypeID", grade.ExamTypeID);
                                        insertCmd.Parameters.AddWithValue("@Grade", grade.Grade ?? (object)DBNull.Value);
                                        insertCmd.Parameters.AddWithValue("@Remarks", grade.Remarks ?? (object)DBNull.Value);
                                        insertCmd.Parameters.AddWithValue("@SessionYear", CurrentSessionYear);
                                        insertCmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                                        insertCmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                                        insertCmd.Parameters.AddWithValue("@TenantCode", CurrentTenantCode);
                                        insertCmd.Parameters.AddWithValue("@CreatedBy", CurrentTenantUserID);

                                        insertCmd.ExecuteNonQuery();
                                    }
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
                System.Diagnostics.Debug.WriteLine("Save grades error: " + ex.Message);
                return false;
            }
        }
    }
    public class StudentWithGradesModel
    {
        public string StudentID { get; set; }
        public string AdmissionNo { get; set; }
        public string RollNumber { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string Grade { get; set; }
        public string Remarks { get; set; }
    }

    public class GradeOption
    {
        public string GradeID { get; set; }
        public string GradeName { get; set; }
        public int SortOrder { get; set; }
    }
    public class SaveGradesRequest
    {
        public List<GradeEntry> Grades { get; set; }
        public string ClassId { get; set; }
        public string SectionId { get; set; }
        public string ExamTypeId { get; set; }
    }

    public class GradeEntry
    {
        public string StudentID { get; set; }
        public string SubjectID { get; set; }
        public string ExamTypeID { get; set; }
        public string Grade { get; set; }
        public string Remarks { get; set; }
    }
    // Supporting models
    public class StudentMarksViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public List<SelectListItem> ExamTypes { get; set; }
    }

    public class StudentModel
    {
        public string StudentID { get; set; }
        public string AdmissionNo { get; set; }
        public string RollNumber { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
    }

    public class SubjectModel
    {
        public string SubjectID { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string MappingID { get; set; }
    }

    public class StudentMarkModel
    {
        public string MarkID { get; set; }
        public string StudentID { get; set; }
        public string SubjectID { get; set; }
        public string ExamTypeID { get; set; }
        public decimal? MarksObtained { get; set; }
        public string Grade { get; set; }
        public string Remarks { get; set; }
    }

    public class SaveMarksRequest
    {
        public List<MarkEntry> Marks { get; set; }
        public string ClassId { get; set; }
        public string SectionId { get; set; }
        public string ExamTypeId { get; set; }
    }

    public class MarkEntry
    {
        public string StudentID { get; set; }
        public string SubjectID { get; set; }
        public string ExamTypeID { get; set; }
        public decimal? MarksObtained { get; set; }
    }
    public class StudentWithMarksModel
    {
        public string StudentID { get; set; }
        public string AdmissionNo { get; set; }
        public string RollNumber { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public decimal? MarksObtained { get; set; }
        public string Grade { get; set; }
    }
}