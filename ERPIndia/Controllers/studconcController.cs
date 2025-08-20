using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
namespace ERPIndia.Controllers
{
    public class studconcController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetStudents(Guid tenantId)
        {
            try
            {
                // Validate tenant ID matches the session
                Guid sessionTenantId = CurrentTenantID;
                if (tenantId != sessionTenantId)
                {
                    return Json(new { error = "Invalid tenant ID" }, JsonRequestBehavior.AllowGet);
                }

                int tenantCode = Utils.ParseInt(CurrentTenantCode);
                Guid SessionID = CurrentSessionID;

                // Query to get students for the tenant
                string query = @"
                    SELECT s.StudentId As Id, s.FirstName as Name,s.AdmsnNo as AdmsnNo,s.RollNo as RollNo,s.Mobile as Mobile, s.ClassName as class, s.SectionName as section,s.FatherName as fatherName
                    FROM vwStudentInfo s
                    WHERE s.TenantID = @TenantID 
                    AND s.SchoolCode = @SchoolCode
                    AND s.SessionID = @SessionID
                    AND s.IsActive = 1
                    AND s.IsDeleted = 0
                    ORDER BY s.FirstName";

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to command
                        command.Parameters.AddWithValue("@TenantID", tenantId);
                        command.Parameters.AddWithValue("@SessionID", SessionID);
                        command.Parameters.AddWithValue("@SchoolCode", tenantCode);

                        var result = new List<dynamic>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    Id = reader["Id"].ToString(),
                                    AdmsnNo = reader["AdmsnNo"].ToString(),
                                    RollNo = reader["RollNo"].ToString(),
                                    Mobile = reader["Mobile"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Class = reader["Class"].ToString(),
                                    Section = reader["Section"].ToString(),
                                    FatherName= reader["FatherName"].ToString()
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

        [HttpGet]
        public JsonResult GetStudentDetails(string studentId, Guid tenantId)
        {
            try
            {
                // Validate tenant ID matches the session
                Guid sessionTenantId = CurrentTenantID;
                if (tenantId != sessionTenantId)
                {
                    return Json(new { error = "Invalid tenant ID" }, JsonRequestBehavior.AllowGet);
                }

                int tenantCode = Utils.ParseInt(CurrentTenantCode);
                int schoolCode = Utils.ParseInt(CurrentTenantCode);
                Guid SessionID = CurrentSessionID;

                if (string.IsNullOrEmpty(studentId))
                {
                    return Json(new { error = "Invalid student ID" }, JsonRequestBehavior.AllowGet);
                }

                Guid studentGuid;
                if (!Guid.TryParse(studentId, out studentGuid))
                {
                    return Json(new { error = "Invalid student ID format" }, JsonRequestBehavior.AllowGet);
                }

                // Query to get student details
                string query = @"
                     SELECT s.StudentId As Id, s.FirstName as Name,s.AdmsnNo as AdmsnNo,s.RollNo as RollNo,s.Mobile as Mobile, s.ClassName as class, s.SectionName as section,s.FatherName as fatherName
                    FROM vwStudentInfo s
                    WHERE s.StudentId = @StudentId
                    AND s.TenantID = @TenantID
                    AND s.SchoolCode = @SchoolCode
                    AND s.SessionID = @SessionID
                    AND s.IsActive = 1
                    AND s.IsDeleted = 0";

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameter to command
                        command.Parameters.AddWithValue("@StudentId", studentGuid);
                        command.Parameters.AddWithValue("@TenantID", tenantId);
                        command.Parameters.AddWithValue("@SessionID", SessionID);
                        command.Parameters.AddWithValue("@SchoolCode", schoolCode);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var studentDetails = new
                                {
                                    AdmsnNo = reader["AdmsnNo"].ToString(),
                                    RollNo = reader["RollNo"] != DBNull.Value ? reader["RollNo"].ToString() : "-",
                                    FirstName = reader["Name"].ToString(),
                                    Class = reader["Class"] != DBNull.Value ? reader["Class"].ToString() : "-",
                                    Section = reader["Section"] != DBNull.Value ? reader["Section"].ToString() : "-",
                                    Mobile = reader["Mobile"] != DBNull.Value ? reader["Mobile"].ToString() : "-",
                                    FatherName = reader["FatherName"] != DBNull.Value ? reader["FatherName"].ToString() : "-"
                                };
                                return Json(studentDetails, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { error = "Student not found" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetStudentFeeDiscountList(Guid tenantId)
        {
            try
            {
                // Validate tenant ID matches the session
                Guid sessionTenantId = CurrentTenantID;
                if (tenantId != sessionTenantId)
                {
                    return Json(new { error = "Invalid tenant ID" }, JsonRequestBehavior.AllowGet);
                }

                int tenantCode = Utils.ParseInt(CurrentTenantCode);
                int schoolCode = Utils.ParseInt(CurrentTenantCode);

                // Query to get student fee discounts
                string query = @"
                    SELECT 
                        fd.Id, 
                        fd.StudentID, 
                        s.FirstName + ' ' + ISNULL(s.LastName, '') AS StudentName,
                        fd.April, fd.May, fd.June, fd.July, fd.August, fd.September,
                        fd.October, fd.November, fd.December, fd.January, fd.February, fd.March
                    FROM StudentFeeDiscountMonths fd
                    INNER JOIN StudentInfoBasic s ON fd.StudentID = s.StudentID
                    WHERE fd.TenantID = @TenantID
                    AND fd.TenantCode = @TenantCode
                    AND fd.IsActive = 1
                    AND fd.IsDeleted = 0
                    ORDER BY s.FirstName";

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to command
                        command.Parameters.AddWithValue("@TenantID", tenantId);
                        command.Parameters.AddWithValue("@TenantCode", tenantCode);

                        // Execute query and read results
                        var result = new List<dynamic>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    Id = reader["Id"].ToString(),
                                    StudentID = reader["StudentID"].ToString(),
                                    StudentName = reader["StudentName"].ToString(),
                                    April = Convert.ToDecimal(reader["April"]),
                                    May = Convert.ToDecimal(reader["May"]),
                                    June = Convert.ToDecimal(reader["June"]),
                                    July = Convert.ToDecimal(reader["July"]),
                                    August = Convert.ToDecimal(reader["August"]),
                                    September = Convert.ToDecimal(reader["September"]),
                                    October = Convert.ToDecimal(reader["October"]),
                                    November = Convert.ToDecimal(reader["November"]),
                                    December = Convert.ToDecimal(reader["December"]),
                                    January = Convert.ToDecimal(reader["January"]),
                                    February = Convert.ToDecimal(reader["February"]),
                                    March = Convert.ToDecimal(reader["March"])
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

        public class StudentFeeDiscountModel
        {
            public Guid tenantId { get; set; }
            public string studentId { get; set; }
            public string studentName { get; set; }
            public string @class { get; set; }
            public string section { get; set; }
            public string frequency { get; set; } // For backward compatibility
            public decimal amount { get; set; } // For backward compatibility
            public MonthlyDiscounts months { get; set; }
            public bool forceOverride { get; set; }
        }

        public class MonthlyDiscounts
        {
            public decimal April { get; set; }
            public decimal May { get; set; }
            public decimal June { get; set; }
            public decimal July { get; set; }
            public decimal August { get; set; }
            public decimal September { get; set; }
            public decimal October { get; set; }
            public decimal November { get; set; }
            public decimal December { get; set; }
            public decimal January { get; set; }
            public decimal February { get; set; }
            public decimal March { get; set; }
        }

        [HttpPost]
        public JsonResult SaveStudentFeeDiscount(StudentFeeDiscountModel model)
        {
            try
            {
                Guid tenantId = CurrentTenantID;
                int tenantCode = Utils.ParseInt(CurrentTenantCode);
                int schoolCode = Utils.ParseInt(CurrentTenantCode);
                Guid currentUserId = CurrentTenantUserID;

                // Validate tenant ID matches the current user's tenant
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid data submitted." });
                }

                // Validate tenant ID
                if (model.tenantId == Guid.Empty || model.tenantId != tenantId)
                {
                    return Json(new { success = false, message = "Invalid tenant ID. Please refresh the page and try again." });
                }

                // Validate student
                if (string.IsNullOrEmpty(model.studentId))
                {
                    return Json(new { success = false, message = "Please select a student." });
                }

                Guid studentGuid;
                if (!Guid.TryParse(model.studentId, out studentGuid))
                {
                    return Json(new { success = false, message = "Invalid student ID format." });
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
                            // Check if the student exists
                            string checkStudentQuery = @"
                                SELECT COUNT(1) FROM StudentInfoBasic 
                                WHERE StudentId = @StudentId
                                AND TenantID = @TenantID
                                AND TenantCode = @TenantCode
                                AND SchoolCode = @SchoolCode
                                AND IsActive = 1
                                AND IsDeleted = 0";

                            var checkStudentParams = new
                            {
                                StudentId = studentGuid,
                                TenantID = tenantId,
                                TenantCode = tenantCode,
                                SchoolCode = schoolCode
                            };

                            int studentExists = connection.ExecuteScalar<int>(checkStudentQuery, checkStudentParams, transaction);
                            if (studentExists == 0)
                            {
                                return Json(new { success = false, message = "The selected student does not exist or is inactive." });
                            }

                            // If forceOverride is true, delete existing records for this student
                            if (model.forceOverride)
                            {
                                string updateQuery = @"
                                    UPDATE StudentFeeDiscountMonths 
                                    SET IsDeleted = 1, 
                                        ModifiedBy = @ModifiedBy, 
                                        ModifiedDate = GETDATE()
                                    WHERE TenantID = @TenantID
                                    AND TenantCode = @TenantCode
                                    AND StudentID = @StudentID
                                    AND IsDeleted = 0";

                                var updateParams = new
                                {
                                    TenantID = tenantId,
                                    TenantCode = tenantCode,
                                    StudentID = studentGuid,
                                    ModifiedBy = currentUserId
                                };

                                connection.Execute(updateQuery, updateParams, transaction);
                            }

                            // Check if a discount already exists for this student
                            string checkExistingQuery = @"
                                SELECT COUNT(1) FROM StudentFeeDiscountMonths 
                                WHERE StudentID = @StudentID 
                                AND TenantID = @TenantID
                                AND TenantCode = @TenantCode
                                AND IsDeleted = 0";

                            var checkParams = new
                            {
                                StudentID = studentGuid,
                                TenantID = tenantId,
                                TenantCode = tenantCode
                            };

                            int existingCount = connection.ExecuteScalar<int>(checkExistingQuery, checkParams, transaction);

                            // Only insert if no discount exists or if force override is true
                            if (existingCount == 0 || model.forceOverride)
                            {
                                // Insert the fee discount record
                                string sql = @"
                                    INSERT INTO StudentFeeDiscountMonths 
                                    (StudentID, April, May, June, July, August, September, 
                                     October, November, December, January, February, March,
                                     TenantID, TenantCode, SessionYear,SessionID,CreatedBy, CreatedDate, IsActive, IsDeleted)
                                    VALUES 
                                    (@StudentID, @April, @May, @June, @July, @August, @September,
                                     @October, @November, @December, @January, @February, @March,
                                     @TenantID, @TenantCode,@SessionYear,@SessionID,@CreatedBy, GETDATE(), 1, 0)";

                                var parameters = new
                                {
                                    SessionYear = CurrentSessionYear,
                                    SessionID = CurrentSessionID,
                                    StudentID = studentGuid,
                                    April = model.months.April,
                                    May = model.months.May,
                                    June = model.months.June,
                                    July = model.months.July,
                                    August = model.months.August,
                                    September = model.months.September,
                                    October = model.months.October,
                                    November = model.months.November,
                                    December = model.months.December,
                                    January = model.months.January,
                                    February = model.months.February,
                                    March = model.months.March,
                                    TenantID = tenantId,
                                    TenantCode = tenantCode,

                                    CreatedBy = currentUserId
                                };

                                connection.Execute(sql, parameters, transaction);
                            }
                            else
                            {
                                // If we got here and haven't overridden or inserted, it means the record exists
                                // and we didn't choose to override it
                                return Json(new { success = false, message = "A fee discount already exists for this student." });
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
        public JsonResult DeleteStudentFeeDiscount(string id, Guid tenantId)
        {
            try
            {
                // Validate tenant ID matches the current user's tenant
                Guid sessionTenantId = CurrentTenantID;
                int tenantCode = Utils.ParseInt(CurrentTenantCode);
                Guid currentUserId = CurrentTenantUserID;

                if (tenantId == Guid.Empty || tenantId != sessionTenantId)
                {
                    return Json(new { success = false, message = "Invalid tenant ID. Please refresh the page and try again." });
                }

                // Validate fee discount ID
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid fee discount ID." });
                }

                Guid discountId;
                if (!Guid.TryParse(id, out discountId))
                {
                    return Json(new { success = false, message = "Invalid fee discount ID format." });
                }

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Soft delete the fee discount
                    string updateQuery = @" DELETE FROM  StudentFeeDiscountMonths WHERE Id = @Id ";
                       
                    var updateParams = new
                    {
                        Id = discountId,
                    };

                    int rowsAffected = connection.Execute(updateQuery, updateParams);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete the fee discount." });
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