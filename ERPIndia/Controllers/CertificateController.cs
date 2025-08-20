using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Dapper;
using ERPIndia.Class.Helper;
using ERPIndia.Models.Certificate;
using ERPIndia.Models.CollectFee.DTOs;
using ERPIndia.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
namespace ERPIndia.Models.Certificate
{
    public class TCDetailsViewModel
    {
        // Identity
        public Guid? TCId { get; set; }
        public Guid StudentId { get; set; }
        public int AdmissionNo { get; set; }
        public string TCNumber { get; set; }
        public bool IsFinalSaved { get; set; }

        // Display Info
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string ClassSection { get; set; }

        // Hindi Names
        public string StudentNameHindi { get; set; }
        public string FatherNameHindi { get; set; }
        public string MotherNameHindi { get; set; }

        // Academic Info
        public string AffiliationNo { get; set; }
        public string ScholarRegistrationNo { get; set; }
        public string BoardRegistrationNo { get; set; }
        public string ClassWhenFirstAdmitted { get; set; }
        public string LastClassStudied { get; set; }
        public string PromotedFromClass { get; set; }
        public string SubjectsStudied { get; set; }

        // Dates
        public DateTime? CertificateApplicationDate { get; set; }
        public DateTime? DateOfLeavingSchool { get; set; }
        public DateTime? DateOfIssue { get; set; }

        // Status
        public string FailedInPast { get; set; }
        public bool IsQualifiedForPromotion { get; set; }
        public string LastExamResult { get; set; }
        public bool AreAllDuesPaid { get; set; }
        public bool HasFeeConcession { get; set; }
        public string FeeConcessionDetails { get; set; }
        public string StudentConduct { get; set; }

        // Activities
        public bool IsNCCCadetScoutGuide { get; set; }
        public string NCCCadetScoutGuideDetails { get; set; }
        public string ExtraCurricularActivities { get; set; }

        // Attendance
        public int? TotalWorkingDays { get; set; }
        public int? TotalDaysAttended { get; set; }

        // Leaving Info
        public string ReasonForLeaving { get; set; }
        public bool NextClassNotAvailable { get; set; }

        // Remarks
        public string Remarks1 { get; set; }
        public string Remarks2 { get; set; }

        // Approval
        public string PreparedBy { get; set; }
        public string CheckedBy { get; set; }
        public string ApprovedBy { get; set; }
    }

    public class StudentListViewModel
    {
        public Guid StudentId { get; set; }
        public int AdmissionNo { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string TCStatus { get; set; }
        public string TCNumber { get; set; }
    }

    public class TCSearchViewModel
    {
        public Guid? SelectedClassId { get; set; }
        public Guid? SelectedSectionId { get; set; }
        public List<SelectListItem> ClassList { get; set; }
        public List<SelectListItem> SectionList { get; set; }
    }

    public class StudentIdentifier
    {
        public string StudentId { get; set; }
        public int AdmissionNo { get; set; }
    }

    public class TCStatusViewModel
    {
        public Guid StudentId { get; set; }
        public int AdmissionNo { get; set; }
        public string TCNumber { get; set; }
        public bool IsFinalSaved { get; set; }
        public string Status { get; set; }
        public DateTime? TCGeneratedDate { get; set; }
        public DateTime? DateOfIssue { get; set; }
    }

    public class TCPrintViewModel : TCDetailsViewModel
    {
        public string SchoolName { get; set; }
        public string SchoolAddress { get; set; }
        public string SchoolPhone { get; set; }
        public string SessionName { get; set; }
        public byte[] SchoolLogo { get; set; }
    }
}
namespace ERPIndia.Controllers
{
    public class CertificateController : BaseController
    {
        private readonly string _connectionString;
        private readonly DropdownController _dropdownController;
        private void AddOptionalParameter(DynamicParameters parameters, string name, object value)
        {
            if (value == null)
            {
                parameters.Add(name, DBNull.Value);
            }
            else if (value is string str)
            {
                parameters.Add(name, string.IsNullOrWhiteSpace(str) ? (object)DBNull.Value : str);
            }
            else
            {
                parameters.Add(name, value);
            }
        }
        #region
        //TC Code 
        [HttpPost]
        public JsonResult CheckTCStatus(string studentId, int admissionNo)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var tcStatus = connection.QueryFirstOrDefault<dynamic>(
                        @"SELECT 
                    TCId,
                    TCNumber, 
                    IsFinalSaved,
                    TCGeneratedDate,
                    DateOfIssue,
                    CASE 
                        WHEN TCId IS NULL THEN 'Not Created'
                        WHEN IsFinalSaved = 0 THEN 'Draft'
                        ELSE 'Finalized'
                    END AS Status
                FROM TransferCertificate
                WHERE StudentId = @StudentId 
                AND AdmissionNo = @AdmissionNo
                AND TenantID = @TenantID
                AND IsDeleted = 0",
                        new
                        {
                            StudentId = Guid.Parse(studentId),
                            AdmissionNo = admissionNo,
                            TenantID = CurrentTenantID
                        });

                    if (tcStatus != null)
                    {
                        return Json(new
                        {
                            success = true,
                            hasTCDetails = true,
                            tcNumber = tcStatus.TCNumber,
                            isFinalSaved = tcStatus.IsFinalSaved ?? false,
                            status = tcStatus.Status,
                            tcGeneratedDate = tcStatus.TCGeneratedDate,
                            dateOfIssue = tcStatus.DateOfIssue
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = true,
                            hasTCDetails = false,
                            tcNumber = "",
                            isFinalSaved = false,
                            status = "Not Created"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CheckTCStatus Error: " + ex.ToString());
                return Json(new
                {
                    success = false,
                    hasTCDetails = false,
                    message = "Error checking TC status"
                });
            }
        }
        [HttpPost]
        public ActionResult SearchStudents(TCSearchViewModel searchModel)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var students = connection.Query<StudentListViewModel>(
                        "sp_SearchStudentsForTC",
                        new
                        {
                            TenantID = CurrentTenantID,
                            SessionID = CurrentSessionID,
                            ClassId = string.IsNullOrEmpty(searchModel.SelectedClassId?.ToString())
                                ? (Guid?)null : searchModel.SelectedClassId,
                            SectionId = string.IsNullOrEmpty(searchModel.SelectedSectionId?.ToString())
                                ? (Guid?)null : searchModel.SelectedSectionId
                        },
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    return PartialView("_StudentList", students);
                }
            }
            catch (Exception ex)
            {
              
                return PartialView("_StudentList", new List<StudentListViewModel>());
            }
        }

        [HttpGet]
        public ActionResult EditTC(string studentId, int admissionNo)
        {

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Parse the school code from session
                    int schoolCode = int.Parse(Session["TenantCode"]?.ToString() ?? "0");

                    var tcDetails = connection.QueryFirstOrDefault<TCDetailsViewModel>(
                        "sp_GetTCDetails",
                        new
                        {
                            StudentId = Guid.Parse(studentId),
                            AdmsnNo = admissionNo,
                            TenantCode = schoolCode
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (tcDetails == null)
                    {
                        return HttpNotFound("Student not found");
                    }

                    return PartialView("Partial/_TCDetailsForm", tcDetails);
                }
            }
            catch (Exception ex)
            {
               
                return Content("<div class='alert alert-danger'>Error loading TC details</div>");
            }
        }

        [HttpPost]
        public JsonResult SaveTC(TCDetailsViewModel model, bool isDraft)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Parse the school code from session
                    int schoolCode = int.Parse(Session["TenantCode"]?.ToString() ?? "0");

                    // Create parameters using DynamicParameters for proper output parameter handling
                    var parameters = new DynamicParameters();

                    // Add all input parameters
                    parameters.Add("@StudentId", model.StudentId);
                    parameters.Add("@AdmissionNo", model.AdmissionNo);
                    parameters.Add("@SchoolCode", schoolCode);
                    parameters.Add("@TenantID", CurrentTenantID);
                    parameters.Add("@SessionID", CurrentSessionID);
                    parameters.Add("@UserId", CurrentTenantUserID);
                    parameters.Add("@IsDraft", isDraft);

                    // Add optional parameters with proper null handling
                    parameters.Add("@StudentNameHindi", string.IsNullOrWhiteSpace(model.StudentNameHindi) ? null : model.StudentNameHindi);
                    parameters.Add("@FatherNameHindi", string.IsNullOrWhiteSpace(model.FatherNameHindi) ? null : model.FatherNameHindi);
                    parameters.Add("@MotherName", string.IsNullOrWhiteSpace(model.MotherName) ? null : model.MotherName);
                    parameters.Add("@MotherNameHindi", string.IsNullOrWhiteSpace(model.MotherNameHindi) ? null : model.MotherNameHindi);
                    parameters.Add("@AffiliationNo", string.IsNullOrWhiteSpace(model.AffiliationNo) ? null : model.AffiliationNo);
                    parameters.Add("@ScholarRegistrationNo", string.IsNullOrWhiteSpace(model.ScholarRegistrationNo) ? null : model.ScholarRegistrationNo);
                    parameters.Add("@BoardRegistrationNo", string.IsNullOrWhiteSpace(model.BoardRegistrationNo) ? null : model.BoardRegistrationNo);
                    parameters.Add("@ClassWhenFirstAdmitted", string.IsNullOrWhiteSpace(model.ClassWhenFirstAdmitted) ? null : model.ClassWhenFirstAdmitted);
                    parameters.Add("@LastClassStudied", string.IsNullOrWhiteSpace(model.LastClassStudied) ? null : model.LastClassStudied);
                    parameters.Add("@PromotedFromClass", string.IsNullOrWhiteSpace(model.PromotedFromClass) ? null : model.PromotedFromClass);
                    parameters.Add("@SubjectsStudied", string.IsNullOrWhiteSpace(model.SubjectsStudied) ? null : model.SubjectsStudied);
                    parameters.Add("@CertificateApplicationDate", model.CertificateApplicationDate);
                    parameters.Add("@DateOfLeavingSchool", model.DateOfLeavingSchool);
                    parameters.Add("@DateOfIssue", model.DateOfIssue ?? DateTime.Now);
                    parameters.Add("@FailedInPast", string.IsNullOrWhiteSpace(model.FailedInPast) ? null : model.FailedInPast);
                    parameters.Add("@IsQualifiedForPromotion", model.IsQualifiedForPromotion);
                    parameters.Add("@LastExamResult", string.IsNullOrWhiteSpace(model.LastExamResult) ? null : model.LastExamResult);
                    parameters.Add("@AreAllDuesPaid", model.AreAllDuesPaid);
                    parameters.Add("@HasFeeConcession", model.HasFeeConcession);
                    parameters.Add("@FeeConcessionDetails", string.IsNullOrWhiteSpace(model.FeeConcessionDetails) ? null : model.FeeConcessionDetails);
                    parameters.Add("@StudentConduct", string.IsNullOrWhiteSpace(model.StudentConduct) ? null : model.StudentConduct);
                    parameters.Add("@IsNCCCadetScoutGuide", model.IsNCCCadetScoutGuide);
                    parameters.Add("@NCCCadetScoutGuideDetails", string.IsNullOrWhiteSpace(model.NCCCadetScoutGuideDetails) ? null : model.NCCCadetScoutGuideDetails);
                    parameters.Add("@ExtraCurricularActivities", string.IsNullOrWhiteSpace(model.ExtraCurricularActivities) ? null : model.ExtraCurricularActivities);
                    parameters.Add("@TotalWorkingDays", model.TotalWorkingDays);
                    parameters.Add("@TotalDaysAttended", model.TotalDaysAttended);
                    parameters.Add("@ReasonForLeaving", string.IsNullOrWhiteSpace(model.ReasonForLeaving) ? null : model.ReasonForLeaving);
                    parameters.Add("@NextClassNotAvailable", model.NextClassNotAvailable);
                    parameters.Add("@Remarks1", string.IsNullOrWhiteSpace(model.Remarks1) ? null : model.Remarks1);
                    parameters.Add("@Remarks2", string.IsNullOrWhiteSpace(model.Remarks2) ? null : model.Remarks2);
                    parameters.Add("@PreparedBy", string.IsNullOrWhiteSpace(model.PreparedBy) ? null : model.PreparedBy);
                    parameters.Add("@CheckedBy", string.IsNullOrWhiteSpace(model.CheckedBy) ? null : model.CheckedBy);
                    parameters.Add("@ApprovedBy", string.IsNullOrWhiteSpace(model.ApprovedBy) ? null : model.ApprovedBy);

                    // Add output parameters
                    parameters.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
                    parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);
                    parameters.Add("@GeneratedTCNumber", dbType: DbType.String, size: 100, direction: ParameterDirection.Output);

                    connection.Execute("sp_SaveTCDetails", parameters, commandType: CommandType.StoredProcedure);

                    var success = parameters.Get<bool>("@Success");
                    var message = parameters.Get<string>("@Message");
                    var tcNumber = parameters.Get<string>("@GeneratedTCNumber");

                    return Json(new
                    {
                        success = success,
                        message = message,
                        tcNumber = tcNumber,
                        isFinalSaved = !isDraft
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SaveTC Error: " + ex.ToString());
                return Json(new
                {
                    success = false,
                    message = "An error occurred while saving TC details: " + ex.Message
                });
            }
        }
        [HttpPost]
        public JsonResult GetBulkTCStatus(List<StudentIdentifier> students)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Create DataTable for the User Defined Type
                    var dataTable = new DataTable();
                    dataTable.Columns.Add("StudentId", typeof(Guid));
                    dataTable.Columns.Add("AdmissionNo", typeof(int));

                    foreach (var student in students)
                    {
                        dataTable.Rows.Add(Guid.Parse(student.StudentId), student.AdmissionNo);
                    }

                    // Use Dapper's table valued parameter
                    var results = connection.Query<TCStatusViewModel>(
                        "sp_GetTCStatusBulk",
                        new
                        {
                            StudentList = dataTable.AsTableValuedParameter("dbo.StudentListType"),
                            TenantID = CurrentTenantID
                        },
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    return Json(new
                    {
                        success = true,
                        data = results
                    });
                }
            }
            catch (Exception ex)
            {
             
                return Json(new
                {
                    success = false,
                    message = "Error retrieving TC status: " + ex.Message
                });
            }
        }
        
        #endregion
        public CertificateController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            _dropdownController = new DropdownController();
        }

        [HttpGet]
        public ActionResult Bonafide()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();

            // Build the view model
            var viewModel = new FeeCollectionSearchRequest
            {
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult)
            };

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Dob()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();

            // Build the view model
            var viewModel = new FeeCollectionSearchRequest
            {
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult)
            };

            return View(viewModel);
        }
        [HttpGet]
        public ActionResult CC()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();

            // Build the view model
            var viewModel = new FeeCollectionSearchRequest
            {
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult)
            };

            return View(viewModel);
        }
        [HttpGet]
        public ActionResult TC()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();

            // Build the view model
            var viewModel = new FeeCollectionSearchRequest
            {
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult)
            };

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Bonafide(FeeCollectionSearchRequest searchRequest)
        {
            try
            {
                // Stamp tenant & session
                searchRequest.TenantId = CurrentTenantID;
                searchRequest.SessionId = CurrentSessionID;

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    db.Open();

                    // Build SQL query
                    var sql = new StringBuilder(@"
                        SELECT *
                        FROM   vwStudentInfo
                        WHERE  TenantID  = @TenantID
                          AND  SessionID = @SessionID
                          AND  IsDeleted = 0
                          AND  IsActive  = 1
                    ");

                    var p = new DynamicParameters();
                    p.Add("@TenantID", searchRequest.TenantId);
                    p.Add("@SessionID", searchRequest.SessionId);

                    // Optional Class filter
                    if (Guid.TryParse(searchRequest.SelectedClass, out var classId) &&
                        classId != Guid.Empty)
                    {
                        sql.Append(" AND ClassID = @ClassID");
                        p.Add("@ClassID", classId);
                    }

                    // Optional Section filter
                    if (Guid.TryParse(searchRequest.SelectedSection, out var sectionId) &&
                        sectionId != Guid.Empty)
                    {
                        sql.Append(" AND SectionID = @SectionID");
                        p.Add("@SectionID", sectionId);
                    }

                    // Execute
                    var students = db.Query<FeeCollectionDto>(sql.ToString(), p).ToList();

                    return PartialView("Partial/_BonafideStudentList", students);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Search failed: " + ex.ToString());
                return PartialView("Partial/_StudentList", new List<FeeCollectionDto>());
            }
        }

        [HttpPost]
        public ActionResult Dob(FeeCollectionSearchRequest searchRequest)
        {
            try
            {
                // Stamp tenant & session
                searchRequest.TenantId = CurrentTenantID;
                searchRequest.SessionId = CurrentSessionID;

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    db.Open();

                    // Build SQL query
                    var sql = new StringBuilder(@"
                        SELECT *
                        FROM   vwStudentInfo
                        WHERE  TenantID  = @TenantID
                          AND  SessionID = @SessionID
                          AND  IsDeleted = 0
                          AND  IsActive  = 1
                    ");

                    var p = new DynamicParameters();
                    p.Add("@TenantID", searchRequest.TenantId);
                    p.Add("@SessionID", searchRequest.SessionId);

                    // Optional Class filter
                    if (Guid.TryParse(searchRequest.SelectedClass, out var classId) &&
                        classId != Guid.Empty)
                    {
                        sql.Append(" AND ClassID = @ClassID");
                        p.Add("@ClassID", classId);
                    }

                    // Optional Section filter
                    if (Guid.TryParse(searchRequest.SelectedSection, out var sectionId) &&
                        sectionId != Guid.Empty)
                    {
                        sql.Append(" AND SectionID = @SectionID");
                        p.Add("@SectionID", sectionId);
                    }

                    // Execute
                    var students = db.Query<FeeCollectionDto>(sql.ToString(), p).ToList();

                    return PartialView("Partial/_DobStudentList", students);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Search failed: " + ex.ToString());
                return PartialView("Partial/_DobStudentList", new List<FeeCollectionDto>());
            }
        }
        [HttpPost]
        public ActionResult CC(FeeCollectionSearchRequest searchRequest)
        {
            try
            {
                // Stamp tenant & session
                searchRequest.TenantId = CurrentTenantID;
                searchRequest.SessionId = CurrentSessionID;

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    db.Open();

                    // Build SQL query
                    var sql = new StringBuilder(@"
                        SELECT *
                        FROM   vwStudentInfo
                        WHERE  TenantID  = @TenantID
                          AND  SessionID = @SessionID
                          AND  IsDeleted = 0
                          AND  IsActive  = 1
                    ");

                    var p = new DynamicParameters();
                    p.Add("@TenantID", searchRequest.TenantId);
                    p.Add("@SessionID", searchRequest.SessionId);

                    // Optional Class filter
                    if (Guid.TryParse(searchRequest.SelectedClass, out var classId) &&
                        classId != Guid.Empty)
                    {
                        sql.Append(" AND ClassID = @ClassID");
                        p.Add("@ClassID", classId);
                    }

                    // Optional Section filter
                    if (Guid.TryParse(searchRequest.SelectedSection, out var sectionId) &&
                        sectionId != Guid.Empty)
                    {
                        sql.Append(" AND SectionID = @SectionID");
                        p.Add("@SectionID", sectionId);
                    }

                    // Execute
                    var students = db.Query<FeeCollectionDto>(sql.ToString(), p).ToList();

                    return PartialView("Partial/_CCStudentList", students);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Search failed: " + ex.ToString());
                return PartialView("Partial/_CCStudentList", new List<FeeCollectionDto>());
            }
        }
        [HttpPost]
        public ActionResult TC(FeeCollectionSearchRequest searchRequest)
        {
            try
            {
                // Stamp tenant & session
                searchRequest.TenantId = CurrentTenantID;
                searchRequest.SessionId = CurrentSessionID;

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    db.Open();

                    // Build SQL query
                    var sql = new StringBuilder(@"
                       SELECT si.*,
                        tc.TCNumber,
                        CASE 
                        WHEN tc.TCId IS NULL THEN 'Not Generated'
                        WHEN tc.IsFinalSaved = 0 THEN 'Draft'
                        WHEN tc.IsFinalSaved = 1 AND tc.TCNumber IS NOT NULL THEN tc.TCNumber
                        ELSE 'Not Generated'
                        END AS TCStatus
                        FROM   vwStudentInfo si
                        LEFT JOIN TransferCertificate tc
                        ON si.StudentId = tc.StudentId
                        AND tc.TenantID = @TenantID
                        AND tc.IsDeleted = 0
                        WHERE  si.TenantID  = @TenantID
                        AND si.SessionID = @SessionID
                        AND si.IsDeleted = 0
                        AND si.IsActive  = 1
                    ");

                    var p = new DynamicParameters();
                    p.Add("@TenantID", searchRequest.TenantId);
                    p.Add("@SessionID", searchRequest.SessionId);

                    // Optional Class filter
                    if (Guid.TryParse(searchRequest.SelectedClass, out var classId) &&
                        classId != Guid.Empty)
                    {
                        sql.Append(" AND ClassID = @ClassID");
                        p.Add("@ClassID", classId);
                    }

                    // Optional Section filter
                    if (Guid.TryParse(searchRequest.SelectedSection, out var sectionId) &&
                        sectionId != Guid.Empty)
                    {
                        sql.Append(" AND SectionID = @SectionID");
                        p.Add("@SectionID", sectionId);
                    }

                    // Execute
                    var students = db.Query<FeeCollectionDto>(sql.ToString(), p).ToList();

                    return PartialView("Partial/_TCStudentList", students);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Search failed: " + ex.ToString());
                return PartialView("Partial/_TCStudentList", new List<FeeCollectionDto>());
            }
        }

        [HttpGet]
        public ActionResult PrintBonafide(string studentId, int admissionNo, int code)
        {
            // Get tenant code from session
            string tenantCode = CommonLogic.GetSessionValue(StringConstants.TenantCode);

            // Validate request
            if (string.IsNullOrEmpty(tenantCode) || tenantCode != code.ToString())
            {
                return Content("<b>Invalid Request</b>", "text/html");
            }

            try
            {
                // Get student data for certificate
                var studentData = GetStudentCertificateData(studentId, admissionNo, tenantCode);
                var gender = studentData.Rows[0]["Gender"].ToString();

                if (studentData == null)
                {
                    return Content("<b>Student Not Found</b>", "text/html");
                }
                var photo = studentData.Rows[0]["Photo"].ToString();
                // Convert to DataTable for Crystal Reports

                // Print metadata
                string printInfo = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : " +
                                  CommonLogic.GetSessionValue(StringConstants.FullName);
              
                // Return the Bonafide certificate report
                return ViewReport(@"/Reports/CrystalReport/0/bonafide.rpt", studentData, printInfo,"Y", gender);
            }
            catch (Exception ex)
            {
                Logger.Error("PrintBonafide error: " + ex.ToString());
                return Content("<b>Error generating certificate</b>", "text/html");
            }
        }

        [HttpGet]
        public ActionResult PrintDOB(string studentId, int admissionNo, int code)
        {
            // Get tenant code from session
            string tenantCode = CommonLogic.GetSessionValue(StringConstants.TenantCode);

            // Validate request
            if (string.IsNullOrEmpty(tenantCode) || tenantCode != code.ToString())
            {
                return Content("<b>Invalid Request</b>", "text/html");
            }

            try
            {
                // Get student data for certificate
                var studentData = GetStudentCertificateData(studentId, admissionNo, tenantCode);
                if (studentData == null)
                {
                    return Content("<b>Student Not Found</b>", "text/html");
                }
                var gender = studentData.Rows[0]["Gender"].ToString();
                var photo = studentData.Rows[0]["Photo"].ToString();
                // Convert to DataTable for Crystal Reports

                // Print metadata
                string printInfo = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : " +
                                  CommonLogic.GetSessionValue(StringConstants.FullName);

                // Return the DOB certificate report
                return ViewReport(@"/Reports/CrystalReport/0/dob.rpt", studentData, printInfo,"Y",gender, photo);
            }
            catch (Exception ex)
            {
                Logger.Error("PrintDOB error: " + ex.ToString());
                return Content("<b>Error generating certificate</b>", "text/html");
            }
        }

        [HttpGet]
        public ActionResult PrintCharacter(string studentId, int admissionNo, int code)
        {
            // Get tenant code from session
            string tenantCode = CommonLogic.GetSessionValue(StringConstants.TenantCode);

            // Validate request
            if (string.IsNullOrEmpty(tenantCode) || tenantCode != code.ToString())
            {
                return Content("<b>Invalid Request</b>", "text/html");
            }

            try
            {
                // Get student data for certificate
                var studentData = GetStudentCertificateData(studentId, admissionNo, tenantCode);
                  // Convert to DataTable for Crystal Reports

                if (studentData == null)
                {
                    return Content("<b>Student Not Found</b>", "text/html");
                }
                var gender = studentData.Rows[0]["Gender"].ToString();
                var photo = studentData.Rows[0]["Photo"].ToString();

                string printInfo = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : " +
                                  CommonLogic.GetSessionValue(StringConstants.FullName);

                // Return the Character certificate report
                return ViewReport(@"/Reports/CrystalReport/0/cc.rpt", studentData, printInfo, "Y", gender, photo);
            }
            catch (Exception ex)
            {
                Logger.Error("PrintCharacter error: " + ex.ToString());
                return Content("<b>Error generating certificate</b>", "text/html");
            }
        }
        [HttpGet]
        public ActionResult PrintTC(string studentId="", int admissionNo=0, int code=0)
        {
            // Get tenant code from session
            string tenantCode = CommonLogic.GetSessionValue(StringConstants.TenantCode);

            // Validate request
            if (string.IsNullOrEmpty(tenantCode) || tenantCode != code.ToString())
            {
                return Content("<b>Invalid Request</b>", "text/html");
            }

            try
            {
                // Get student data for certificate
                var studentData = GetStudentTCCertificateData(studentId, admissionNo, tenantCode);
                // Convert to DataTable for Crystal Reports

                if (studentData == null)
                {
                    return Content("<b>Student Not Found</b>", "text/html");
                }
                var gender = studentData.Rows[0]["Gender"].ToString();
                var photo = studentData.Rows[0]["Photo"].ToString();

                string printInfo = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : " +
                                  CommonLogic.GetSessionValue(StringConstants.FullName);

                // Return the Character certificate report
                return ViewReport(@"/Reports/CrystalReport/0/tc.rpt", studentData, printInfo, "Y", gender, photo);
            }
            catch (Exception ex)
            {
                Logger.Error("PrintCharacter error: " + ex.ToString());
                return Content("<b>Error generating certificate</b>", "text/html");
            }
        }

        private DataTable GetStudentCertificateData(string studentId, int admissionNo, string tenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Query to get student and school information for certificates
                string query = @"
            SELECT 
                s.StudentId as Id,
                s.AdmsnNo,
                s.FirstName as StudentName,
                s.FatherName as Father,
                s.MotherName,
                FORMAT(DOB, 'dd/MM/yyyy') as D2,
                s.Gender,
                s.ClassName as Class,
                FORMAT(AdmsnDate, 'dd/MM/yyyy') as D1,
                s.SectionName,
                s.RollNo as D11,
                FORMAT(AdmsnDate, 'yyyy') AS D12,
                s.Mobile,
                s.Photo,
                s.CategoryName,
                t.PrintTitle as SchoolName,
                t.Line1 as SchoolAddress1,
                t.Line2 as SchoolAddress2,
                t.Line3 as SchoolPhone,
                t.Line4 as SchoolEmail,
                t.LOGOImg,
                t.SIGNImg,
                s.StCurrentAddress AS D13,
                CASE 
                WHEN s.AdmsnDate IS NULL THEN 0
                ELSE DATEDIFF(YEAR, s.AdmsnDate, GETDATE())
                END as D14,
                a.PrintName as SessionName,
                GETDATE() as CertificateDate
            FROM vwStudentInfo s
            JOIN Tenants t ON s.TenantID = t.TenantID
            JOIN AcademicSessionMaster a ON s.SessionID = a.SessionID
            WHERE s.StudentId = @StudentId 
            AND s.AdmsnNo = @AdmsnNo
            AND t.TenantCode = @TenantCode";

                using (var command = new SqlCommand(query, connection))
                {
                    // Add parameters
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    command.Parameters.AddWithValue("@AdmsnNo", admissionNo);
                    command.Parameters.AddWithValue("@TenantCode", tenantCode);

                    // Use SqlDataAdapter to fill DataTable
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }
        private DataTable GetStudentTCCertificateData(string studentId, int admissionNo, string tenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("dbo.sp_GetTCDetails", connection))
                {
                    // Specify that this is a stored procedure
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters with explicit types
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    command.Parameters.AddWithValue("@AdmsnNo", admissionNo);
                    command.Parameters.AddWithValue("@TenantCode", tenantCode);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        private ActionResult ViewReport(string reportPath, DataTable dataTable, string printdata,string type="N", string typevalue = "",string image="")
        {
            try
            {
                // Validate and sanitize the path
                if (string.IsNullOrEmpty(reportPath) || !reportPath.EndsWith(".rpt", StringComparison.OrdinalIgnoreCase))
                {
                    return HttpNotFound("Invalid report path");
                }

                // Get the full path to the report file
                string fullPath = Server.MapPath(reportPath);

                if (!System.IO.File.Exists(fullPath))
                {
                    return HttpNotFound("Report file not found");
                }

                // Create a new Crystal Report document
                ReportDocument reportDocument = new ReportDocument();
                reportDocument.Load(fullPath);
                reportDocument.SetDataSource(dataTable);

                string code = CommonLogic.GetSessionValue(StringConstants.TenantCode);
                string img = CommonLogic.GetSessionValue(StringConstants.LogoImg);
                string tranferimg = CommonLogic.GetSessionValue(StringConstants.TransferCertBannerImg);
                string Pimg = image;
                string SessionName = CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint);
                string logoPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, img));
                string tcPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, tranferimg));
                string pPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetStudentImagePath(code, Pimg));
               

                string sessionprint = string.Format("( {0} )", CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint));
                // Add parameters
                if(reportPath.Contains("tc.rpt"))
                {
                    reportDocument.SetParameterValue("fmlogo", tcPath);
                }
                else
                {
                    reportDocument.SetParameterValue("fmlogo", logoPath);
                }
                reportDocument.SetParameterValue("prmtitle", CommonLogic.GetSessionValue(StringConstants.PrintTitle));
                reportDocument.SetParameterValue("prmline1", CommonLogic.GetSessionValue(StringConstants.Line1));
                reportDocument.SetParameterValue("prmline2", CommonLogic.GetSessionValue(StringConstants.Line2));
                reportDocument.SetParameterValue("prmline3", CommonLogic.GetSessionValue(StringConstants.Line3));
                reportDocument.SetParameterValue("prmline4", CommonLogic.GetSessionValue(StringConstants.Line4));
                reportDocument.SetParameterValue("session", sessionprint);
               
                if (type == "Y")
                {
                    reportDocument.SetParameterValue("psign", pPath);
                    reportDocument.SetParameterValue("gender", typevalue);
                }
                else
                {
                    reportDocument.SetParameterValue("psign", "");
                }
                    // Return the report as a PDF
                    Stream stream = reportDocument.ExportToStream(ExportFormatType.PortableDocFormat);
                stream.Position = 0;

                // Return the file stream without prompting for download
                Response.Buffer = true;
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "inline");

                return new FileStreamResult(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }


        private List<SelectListItem> ConvertToSelectListDefault(JsonResult result)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownResponse>(jsonString);

                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    var newList = new List<DropdownItem>
                    {
                        new DropdownItem
                        {
                            Id = Guid.Empty,
                            Name = "ALL"
                        }
                    };

                    newList.AddRange(dropdownResponse.Data);

                    return newList.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }

                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
            }
        }
        
   

       

       

     



      
    }
   

}