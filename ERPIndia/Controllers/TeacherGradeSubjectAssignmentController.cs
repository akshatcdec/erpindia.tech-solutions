using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class TeacherGradeSubjectAssignmentController : BaseController
    {

        private int GetCurrentClientId()
        {
            // Replace with your actual implementation
            // For example, from claims or session
            // return Convert.ToInt32(User.Identity.GetClientId());

            // For testing, return a default value or from session if available
            return Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetConfigurationLists()
        {
            try
            {
                int clientId = GetCurrentClientId();
                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Query to get all configurations for the client
                    string sql = @"SELECT Id, ClientID, KeyName, KeyValue, Module, SortOrder 
                                  FROM Configurations 
                                  WHERE ClientID = @ClientID
                                  ORDER BY Module, SortOrder";
                    var parameters = new { ClientID = clientId };
                    connection.Open();
                    var configs = connection.Query<ERPIndia.Controllers.Configuration>(sql, parameters).ToList();

                    return Json(configs, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetExistingAssignments()
        {
            try
            {
                int clientId = GetCurrentClientId();
                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Query to get all teacher grade subject assignments for validation
                    string sql = @"SELECT Id, ClientID, TeacherID, ClassID, SectionID, SubjectID
                                  FROM TeacherGradeSubjectAssignments
                                  WHERE ClientID = @ClientID";
                    var parameters = new { ClientID = clientId };
                    connection.Open();
                    var assignments = connection.Query(sql, parameters).ToList();

                    return Json(assignments, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetTeacherGradeAssignmentList(int clientId)
        {
            try
            {
                // Validate client ID matches the session
                int sessionClientId = Convert.ToInt32(Session["UserId"]);
                if (clientId != sessionClientId)
                {
                    return Json(new { error = "Invalid client ID" }, JsonRequestBehavior.AllowGet);
                }

                // Build query to get teacher grade assignments with names
                string query = @"
                SELECT tgsa.Id, tgsa.TeacherID, tgsa.ClassID, tgsa.SectionID, tgsa.SubjectID,
                       t.KeyValue AS TeacherName, c.KeyValue AS ClassName, 
                       s.KeyValue AS SectionName, sub.KeyValue AS SubjectName
                FROM TeacherGradeSubjectAssignments tgsa
                INNER JOIN Configurations t ON tgsa.TeacherID = t.Id AND t.KeyName = 'TEACHER'
                INNER JOIN Configurations c ON tgsa.ClassID = c.Id AND c.KeyName = 'CLASS'
                INNER JOIN Configurations s ON tgsa.SectionID = s.Id AND s.KeyName = 'SECTION'
                INNER JOIN Configurations sub ON tgsa.SubjectID = sub.Id AND sub.KeyName = 'GRADESUBJECT'
                WHERE tgsa.ClientID = @ClientID
                ORDER BY tgsa.Id DESC";

                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ClientID", clientId)
                };

                // Execute query
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to command
                        command.Parameters.AddRange(parameters.ToArray());

                        // Execute query and read results
                        var result = new List<dynamic>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    TeacherID = Convert.ToInt32(reader["TeacherID"]),
                                    ClassID = Convert.ToInt32(reader["ClassID"]),
                                    SectionID = Convert.ToInt32(reader["SectionID"]),
                                    SubjectID = Convert.ToInt32(reader["SubjectID"]),
                                    TeacherName = reader["TeacherName"].ToString(),
                                    ClassName = reader["ClassName"].ToString(),
                                    SectionName = reader["SectionName"].ToString(),
                                    SubjectName = reader["SubjectName"].ToString()
                                });
                            }
                        }

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public class TeacherGradeAssignmentModel
        {
            public int clientId { get; set; }
            public List<string> teachers { get; set; }
            public List<string> classes { get; set; }
            public List<string> sections { get; set; }
            public List<string> subjects { get; set; }
            public bool forceOverride { get; set; }
        }

        [HttpPost]
        public JsonResult SaveTeacherGradeAssignment(TeacherGradeAssignmentModel model)
        {
            try
            {
                int clientId = GetCurrentClientId();
                // Validate model
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid data submitted." });
                }

                // Validate client ID
                if (model.clientId <= 0 || model.clientId != clientId)
                {
                    return Json(new { success = false, message = "Invalid client ID. Please refresh the page and try again." });
                }

                // Validate teachers
                if (model.teachers == null || model.teachers.Count == 0)
                {
                    return Json(new { success = false, message = "Please select at least one Teacher." });
                }

                // Validate classes
                if (model.classes == null || model.classes.Count == 0)
                {
                    return Json(new { success = false, message = "Please select at least one Class." });
                }

                // Validate sections
                if (model.sections == null || model.sections.Count == 0)
                {
                    return Json(new { success = false, message = "Please select at least one Section." });
                }

                // Validate subjects
                if (model.subjects == null || model.subjects.Count == 0)
                {
                    return Json(new { success = false, message = "Please select at least one Subject." });
                }

                // Validate IDs are greater than zero
                foreach (var teacher in model.teachers)
                {
                    int teacherId;
                    if (!int.TryParse(teacher, out teacherId) || teacherId <= 0)
                    {
                        return Json(new { success = false, message = "Invalid Teacher ID selected." });
                    }
                }

                foreach (var cls in model.classes)
                {
                    int classId;
                    if (!int.TryParse(cls, out classId) || classId <= 0)
                    {
                        return Json(new { success = false, message = "Invalid Class ID selected." });
                    }
                }

                foreach (var section in model.sections)
                {
                    int sectionId;
                    if (!int.TryParse(section, out sectionId) || sectionId <= 0)
                    {
                        return Json(new { success = false, message = "Invalid Section ID selected." });
                    }
                }

                foreach (var subject in model.subjects)
                {
                    int subjectId;
                    if (!int.TryParse(subject, out subjectId) || subjectId <= 0)
                    {
                        return Json(new { success = false, message = "Invalid Subject ID selected." });
                    }
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // If force override is true, delete existing assignments
                            if (model.forceOverride)
                            {
                                string deleteQuery = @"DELETE FROM TeacherGradeSubjectAssignments 
                                                     WHERE ClientID = @ClientID
                                                     AND TeacherID IN @TeacherIDs
                                                     AND ClassID IN @ClassIDs
                                                     AND SectionID IN @SectionIDs
                                                     AND SubjectID IN @SubjectIDs";

                                var deleteParams = new
                                {
                                    ClientID = clientId,
                                    TeacherIDs = model.teachers,
                                    ClassIDs = model.classes,
                                    SectionIDs = model.sections,
                                    SubjectIDs = model.subjects
                                };

                                connection.Execute(deleteQuery, deleteParams, transaction);
                            }

                            // Insert individual records for each combination
                            foreach (var teacher in model.teachers)
                            {
                                foreach (var cls in model.classes)
                                {
                                    foreach (var section in model.sections)
                                    {
                                        foreach (var subject in model.subjects)
                                        {
                                            // Check if this exact combination already exists
                                            string checkQuery = @"SELECT COUNT(1) FROM TeacherGradeSubjectAssignments 
                                                               WHERE ClientID = @ClientID 
                                                               AND TeacherID = @TeacherID
                                                               AND ClassID = @ClassID
                                                               AND SectionID = @SectionID
                                                               AND SubjectID = @SubjectID";

                                            var checkParams = new
                                            {
                                                ClientID = clientId,
                                                TeacherID = teacher,
                                                ClassID = cls,
                                                SectionID = section,
                                                SubjectID = subject
                                            };

                                            int existingCount = connection.ExecuteScalar<int>(checkQuery, checkParams, transaction);

                                            // Only insert if the record doesn't exist or if force override is true
                                            if (existingCount == 0 || model.forceOverride)
                                            {
                                                // Insert a record for each combination
                                                string insertSql = @"INSERT INTO TeacherGradeSubjectAssignments 
                                                                 (ClientID, TeacherID, ClassID, SectionID, SubjectID, 
                                                                  CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                                                                 VALUES 
                                                                 (@ClientID, @TeacherID, @ClassID, @SectionID, @SubjectID,
                                                                  @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate)";

                                                var insertParams = new
                                                {
                                                    ClientID = clientId,
                                                    TeacherID = teacher,
                                                    ClassID = cls,
                                                    SectionID = section,
                                                    SubjectID = subject,
                                                    CreatedBy = clientId,
                                                    CreatedDate = DateTime.Now,
                                                    ModifiedBy = clientId,
                                                    ModifiedDate = DateTime.Now
                                                };

                                                connection.Execute(insertSql, insertParams, transaction);
                                            }
                                        }
                                    }
                                }
                            }

                            transaction.Commit();
                            return Json(new { success = true });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
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
        public JsonResult DeleteTeacherGradeAssignment(int id, int clientId)
        {
            try
            {
                // Validate client ID matches the current user's client
                int sessionClientId = GetCurrentClientId();
                if (clientId <= 0 || clientId != sessionClientId)
                {
                    return Json(new { success = false, message = "Invalid client ID. Please refresh the page and try again." });
                }

                // Validate assignment ID
                if (id <= 0)
                {
                    return Json(new { success = false, message = "Invalid assignment ID." });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // First verify that the assignment exists and belongs to the client
                    string verifyQuery = @"SELECT COUNT(1) FROM TeacherGradeSubjectAssignments 
                                        WHERE Id = @Id AND ClientID = @ClientID";

                    var verifyParams = new { Id = id, ClientID = clientId };
                    int count = connection.ExecuteScalar<int>(verifyQuery, verifyParams);

                    if (count == 0)
                    {
                        return Json(new { success = false, message = "Assignment not found or does not belong to this client." });
                    }

                    // Delete the assignment
                    string deleteQuery = "DELETE FROM TeacherGradeSubjectAssignments WHERE Id = @Id AND ClientID = @ClientID";
                    var deleteParams = new { Id = id, ClientID = clientId };

                    int rowsAffected = connection.Execute(deleteQuery, deleteParams);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete the assignment." });
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