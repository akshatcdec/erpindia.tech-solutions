using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Dapper;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.DTOs;
using ERPIndia.DTOs.Ledger;
using ERPIndia.DTOs.Receipt;
using ERPIndia.DTOs.Student;
using ERPIndia.FeeDefaultersModels;
using ERPIndia.FeeSummary.DTO;
using ERPIndia.Models.CollectFee.DTOs;
using ERPIndia.Models.SystemSettings;
using ERPIndia.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using SchoolInfoDto = ERPIndia.DTOs.Ledger.SchoolInfoDto;

namespace ERPIndia.Models.CollectFee.DTOs
{
    public class rptFeeReceiptDto
    {
        public string ReceiptNumber { get; set; } = string.Empty;
        public string PaymentDate { get; set; } = string.Empty;
        public decimal SubTotalAmount { get; set; } = 0m;
        public decimal LateFeeAmount { get; set; } = 0m;
        public decimal Total { get; set; } = 0m;
        public decimal ReceiveAmt { get; set; } = 0m;
        public decimal Remain { get; set; } = 0m;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0m;
        public decimal Discount { get; set; } = 0m;
        public decimal LateFee { get; set; } = 0m;
        public decimal Subtotal { get; set; } = 0m;

        // Student Information
        public string FirstName { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string FName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string AdmsnNo { get; set; } = string.Empty;
        public string StCurrentAddress { get; set; } = string.Empty;

        // Institution Information
        public string TenantName { get; set; } = string.Empty;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PrintName { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;

        // Financial Information
        public decimal OldBalance { get; set; } = 0m;
        public string Months { get; set; } = string.Empty;

        // Dynamic fields
        public string D1 { get; set; } = string.Empty;
        public string D2 { get; set; } = string.Empty;
        public string D3 { get; set; } = string.Empty;
        public string D4 { get; set; } = string.Empty;
        public string D5 { get; set; } = string.Empty;
        public string D6 { get; set; } = string.Empty;
        public string D7 { get; set; } = string.Empty;
        public string D8 { get; set; } = string.Empty;
        public string D9 { get; set; } = string.Empty;
        public string D10 { get; set; } = string.Empty;
        public string D11 { get; set; } = string.Empty;
        public string D12 { get; set; } = string.Empty;
        public string D13 { get; set; } = string.Empty;
        public string D14 { get; set; } = string.Empty;
        public string D15 { get; set; } = string.Empty;
        public string D16 { get; set; } = string.Empty;
        public string D17 { get; set; } = string.Empty;
        public string D18 { get; set; } = string.Empty;
        public string D19 { get; set; } = string.Empty;
        public string D20 { get; set; } = string.Empty;
        public string D21 { get; set; } = string.Empty;
        public string D22 { get; set; } = string.Empty;
        public string D23 { get; set; } = string.Empty;
        public string D24 { get; set; } = string.Empty;
        public string D25 { get; set; } = string.Empty;
        public string D26 { get; set; } = string.Empty;
        public string D27 { get; set; } = string.Empty;
        public string D28 { get; set; } = string.Empty;
        public string D29 { get; set; } = string.Empty;
        public string D30 { get; set; } = string.Empty;
        public string D31 { get; set; } = string.Empty;
        public string D32 { get; set; } = string.Empty;
        public string D33 { get; set; } = string.Empty;
        public string D34 { get; set; } = string.Empty;
        public string D35 { get; set; } = string.Empty;
        public string D36 { get; set; } = string.Empty;
        public string D37 { get; set; } = string.Empty;
        public string D38 { get; set; } = string.Empty;
        public string D39 { get; set; } = string.Empty;
        public string D40 { get; set; } = string.Empty;

        // Method to clone the object
        public rptFeeReceiptDto Clone()
        {
            return new rptFeeReceiptDto
            {
                ReceiptNumber = this.ReceiptNumber,
                PaymentDate = this.PaymentDate,
                SubTotalAmount = this.SubTotalAmount,
                LateFeeAmount = this.LateFeeAmount,
                Total = this.Total,
                ReceiveAmt = this.ReceiveAmt,
                Remain = this.Remain,
                PaymentMethod = this.PaymentMethod,
                Note = this.Note,
                Balance = this.Balance,
                Discount = this.Discount,
                LateFee = this.LateFee,
                Subtotal = this.Subtotal,
                FirstName = this.FirstName,
                Class = this.Class,
                Section = this.Section,
                FName = this.FName,
                Mobile = this.Mobile,
                AdmsnNo = this.AdmsnNo,
                StCurrentAddress = this.StCurrentAddress,
                TenantName = this.TenantName,
                Address1 = this.Address1,
                Address2 = this.Address2,
                Email = this.Email,
                PrintName = this.PrintName,
                SessionName = this.SessionName,
                OldBalance = this.OldBalance,
                Months = this.Months,
                D1 = this.D1,
                D2 = this.D2,
                D3 = this.D3,
                D4 = this.D4,
                D5 = this.D5,
                D6 = this.D6,
                D7 = this.D7,
                D8 = this.D8,
                D9 = this.D9,
                D10 = this.D10,
                D11 = this.D11,
                D12 = this.D12,
                D13 = this.D13,
                D14 = this.D14,
                D15 = this.D15,
                D16 = this.D16,
                D17 = this.D17,
                D18 = this.D18,
                D19 = this.D19,
                D20 = this.D20,
                D21 = this.D21,
                D22 = this.D22,
                D23 = this.D23,
                D24 = this.D24,
                D25 = this.D25,
                D26 = this.D26,
                D27 = this.D27,
                D28 = this.D28,
                D29 = this.D29,
                D30 = "FEES RECEIPT(Student Copy)",
                D31 = this.D31,
                D32 = this.D32,
                D33 = this.D33,
                D34 = this.D34,
                D35 = this.D35,
                D36 = this.D36,
                D37 = this.D37,
                D38 = this.D38,
                D39 = this.D39,
                D40 = this.D40
            };
        }

    }

    public class FeeCollectionSearchRequest
    {
        public FeeCollectionSearchRequest()
        {
            ClassList = new List<SelectListItem>();
            SectionList = new List<SelectListItem>();
            ExamList = new List<SelectListItem>();
        }
        public string SelectedClass { get; set; }
        public string SelectedExam { get; set; }
        /// <summary>
        /// Selected Section ID
        /// </summary>
        public string SelectedSection { get; set; }
        public List<SelectListItem> ClassList { get; set; }

        /// <summary>
        /// Section dropdown items
        /// </summary>
        public List<SelectListItem> SectionList { get; set; }
        public List<SelectListItem> ExamList { get; set; }
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Guid TenantId { get; set; }
        public Guid SessionId { get; set; }
        public Guid? ClassId { get; set; }
        public string SectionId { get; set; }
    }

    public class FeeCollectionResult
    {
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public IEnumerable<FeeCollectionDto> Data { get; set; }
    }

    public class FeeCollectionDto
    {
        public string ClassName { get; set; }
        public Guid StudentId { get; set; }
        public Guid TenantID { get; set; }
        public Guid SessionID { get; set; }
        public int AdmsnNo { get; set; }
        public string SectionName { get; set; }
        public string RollNo { get; set; }
        public string SrNo { get; set; }
        public string FirstName { get; set; }
        public string FatherName { get; set; }
        public string Mobile { get; set; }
        public string Gender { get; set; }
        public string CategoryName { get; set; }
        public string TCNumber { get; set; }
        public string TCStatus { get; set; }
        
    }

    public class ReceiptData
    {
        public int ReceiptNo { get; set; }
        public decimal ConcessinAuto { get; set; }
        public int SchoolCode { get; set; }
        public int AdmissionNo { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public string RollNo { get; set; }
        public decimal OldBalance { get; set; }
        public decimal FeeAdded { get; set; }
        public decimal LateFee { get; set; }
        public decimal TotalFee { get; set; }
        public decimal Received { get; set; }
        public decimal Remain { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string Note { get; set; }
        public string PrintTitle { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public List<FeeItem> FeeItems { get; set; }
        public List<TransportItem> TransportItems { get; set; }
    }

    public class FeeItem
    {
        public string Month { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }

    public class TransportItem
    {
        public string Month { get; set; }
        public string RouteName { get; set; }
        public decimal Amount { get; set; }
    }
    
    public class FeePaymentModel
    {
        // Main receipt properties (FeeReceivedTbl)
        [Required]
        public int SchoolCode { get; set; }
        [Required]
        public int TenantCode { get; set; }
        [Required]
        public Guid TenantID { get; set; }
        [Required]
        public Guid SessionID { get; set; }
        [Required]
        public Guid StudentID { get; set; }
        [Required]
        public int SessionYear { get; set; }
        [Required]
        public int AdmissionNo { get; set; }
        [Required]
        public decimal OldBalance { get; set; } = 0;
        [Required]
        public decimal FeeAdded { get; set; } = 0;

        public decimal LateFee { get; set; } = 0;

        public decimal LateFeeAuto { get; set; } = 0;

        [Required]
        public decimal TotalFee { get; set; } = 0;

        [Required]
        public decimal Received { get; set; } = 0;

        [Required]
        public decimal Remain { get; set; } = 0;

        public string LastDepositMonth { get; set; }
        [Required]
        public string PaymentMode { get; set; }
        public int ConcessinAuto { get; set; } = 0;

        public int ConcessinMannual { get; set; } = 0;

        public decimal TotalTransport { get; set; } = 0;

        public string TransportRoute { get; set; }

        public decimal TransportAmount { get; set; } = 0;

        public string Note1 { get; set; }

        public string Note2 { get; set; }
        public string RouteName { get; set; }

        [Required]
        public string EntryTime { get; set; }

        public int UserId { get; set; } = 1; // Default user ID if not provided

        // Collection of monthly fee items (FeeMonthlyFeeTbl)
        [Required]
        public List<MonthlyFeeItem> MonthlyFees { get; set; } = new List<MonthlyFeeItem>();

        // Collection of transport fee items (FeeTransportFeeTbl)
        public List<MonthlyFeeItem> TransportFees { get; set; } = new List<MonthlyFeeItem>();
    }

    /// <summary>
    /// Represents a single fee item for a specific month
    /// </summary>
    public class MonthlyFeeItem
    {
        [Required]
        public string Month { get; set; }

        [Required]
        public string FeeName { get; set; }

        [Required]
        public decimal RegularAmount { get; set; }

        public decimal Discount { get; set; } = 0;

        [Required]
        public decimal FinalAmount { get; set; }

        [Required]
        public int FeeHeadId { get; set; }
    }
    public class FeeCollectionViewModel
    {
        public string StudentId { get; set; }
        public string SessionID { get; set; }
        public string TenantID { get; set; }
        public string TenantCode { get; set; }
        public string SessionYear { get; set; }
        
        public StudentInfoDto StudentInfo { get; set; }
        public List<FeeDetailDTO> FeeDetails { get; set; }
    }


    public class FeeDetailDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, decimal> Months { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> Discounts { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> paidAmounts { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> regularAmounts { get; set; } = new Dictionary<string, decimal>();
    }

    public class PaymentValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
}
namespace ERPIndia.Controllers
{
    public class CollectFeeController : BaseController
    {
        private readonly string _connectionString;
        private readonly DropdownController _dropdownController;
        public CollectFeeController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            _dropdownController = new DropdownController();
        }
        public ActionResult ViewReport(string reportPath, DataTable dataTable,string printdata)
        {
            try
            {
                // Validate and sanitize the path to prevent security issues
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
                string img = CommonLogic.GetSessionValue(StringConstants.ReceiptBannerImg);
                string Signimg = CommonLogic.GetSessionValue(StringConstants.ReceiptSignImg);
                string logoPath =Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, img));
                string SignPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, Signimg));
                // Load the report
                reportDocument.SetParameterValue("fmPrintData", printdata);
                reportDocument.SetParameterValue("fmlogo", logoPath);
                reportDocument.SetParameterValue("frmSign", SignPath);
                // Set any database login info if needed
                // reportDocument.SetDatabaseLogon("username", "password", "server", "database");

                // Set any parameters if needed
                // reportDocument.SetParameterValue("ParameterName", parameterValue);

                // Return the report as a PDF
                Stream stream = reportDocument.ExportToStream(ExportFormatType.PortableDocFormat);

                // Reset stream position to the beginning
                stream.Position = 0;

                // Return the stream with content type set to PDF, but without download
                Response.Buffer = true;
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "inline");

                // Return the file stream without prompting for download
                return new FileStreamResult(stream, "application/pdf");

                // Alternatively, you can use the built-in Crystal Reports viewer
                // return View("ReportViewer", reportDocument);
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
                // Serialize the Data to a string to handle dynamic object
                string jsonString = JsonConvert.SerializeObject(result.Data);
                // Deserialize into a strongly-typed object
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownResponse>(jsonString);
                // Check if deserialization was successful and data exists
                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    // Create a new list with "ALL" as the first item
                    var newList = new List<DropdownItem>
            {
                new DropdownItem
                {
                    Id = Guid.Empty,
                    Name = "ALL"
                }
            };

                    // Add the existing items to the new list
                    newList.AddRange(dropdownResponse.Data);

                    // Convert to SelectListItems
                    return newList.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }
                // Return empty list if no data or unsuccessful
                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
                // Log the error
                // LogError(ex, "ConvertToSelectList");
            }
        }
        [HttpGet]
        public ActionResult Index()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();
            Guid sessionId = CurrentSessionID;
            var tenantCode = Utils.ParseInt(CurrentTenantCode);
            var tenantId = CurrentTenantID;
          
            // Build the view model
            var viewModel = new FeeCollectionSearchRequest
            {
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
                // Get fee summary data
                
            };
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Search(FeeCollectionSearchRequest searchRequest)
        {
            try
            {
                // 1.  Stamp tenant & session
                searchRequest.TenantId = CurrentTenantID;
                searchRequest.SessionId = CurrentSessionID;

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    db.Open();

                    // 2.  Build SQL piecemeal so we add filters only when needed
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

                    // ---- Optional Class filter -------------------------------------
                    if (Guid.TryParse(searchRequest.SelectedClass, out var classId) &&
                        classId != Guid.Empty)
                    {
                        sql.Append(" AND ClassID = @ClassID");
                        p.Add("@ClassID", classId);
                    }

                    // ---- Optional Section filter -----------------------------------
                    if (Guid.TryParse(searchRequest.SelectedSection, out var sectionId) &&
                        sectionId != Guid.Empty)
                    {
                        sql.Append(" AND SectionID = @SectionID");
                        p.Add("@SectionID", sectionId);
                    }

                    // 3.  Execute
                    var students = db.Query<FeeCollectionDto>(sql.ToString(), p).ToList();

                    return PartialView("Partial/_StudentList", students);
                }
            }
            catch (Exception ex)
            {
                // Good place for logging – keeps the UI clean
                Logger.Error("Search failed"+ex.ToString());

                return PartialView("Partial/_StudentList", new List<FeeCollectionDto>());
            }
        }

        [HttpGet]
        public ActionResult Collection(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Account");
            }
            // Create view model and populate with student data
            var viewModel = new FeeCollectionViewModel
            {
                StudentId = studentId,
                SessionID = CurrentSessionID.ToString(),
                TenantID = CurrentTenantID.ToString(),
                TenantCode = CurrentSchoolCode.ToString(),
                SessionYear = CurrentSessionYear.ToString(),
                StudentInfo = GetStudentInfo(studentId),
                FeeDetails = GetStudentFeeDetails1(studentId)
            };

            return View(viewModel);
        }
        [HttpGet]
        public ActionResult SiblingLedger(Guid id, Guid sessionId, Guid tenantId)
        {
            var model = GetStudentLedgerWithSiblings(id, sessionId, tenantId);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        public StudentLedgerWithSiblingDto GetStudentLedgerWithSiblings(Guid studentId, Guid sessionId, Guid tenantId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@StudentID", studentId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@TenantID", tenantId);

                using (var multi = connection.QueryMultiple("sp_StudentFeeLedger1", parameters, commandType: CommandType.StoredProcedure))
                {
                    // First result set: Student and School Information
                    var studentInfo = multi.ReadFirstOrDefault<dynamic>();
                     
                    if (studentInfo == null)
                        return null;

                    // Second result set: Fee Summary Data for student and siblings (if applicable)
                    var feeSummaryDataList = multi.Read<dynamic>().ToList();

                    if (feeSummaryDataList == null || feeSummaryDataList.Count == 0)
                        return null;

                    // Map student and school info
                    var schoolInfo = new SchoolInfoDto
                    {
                        Name = Convert.ToString(studentInfo.PrintTitle),
                        Line1 = Convert.ToString(studentInfo.Line1),
                        Line2 = Convert.ToString(studentInfo.Line2),
                        Line3 = Convert.ToString(studentInfo.Line3),
                        Line4 = Convert.ToString(studentInfo.Line4)
                    };

                    var parentInfo = new ParentInfoDto
                    {
                        SiblingCode = "",
                        FatherName = Convert.ToString(studentInfo.FatherName),
                        FatherAadhar = Convert.ToString(studentInfo.FatherAadhar),
                        Address = Convert.ToString(studentInfo.Address),
                        Mobile = Convert.ToString(studentInfo.Mobile)
                    };

                    // Create a list to hold all students (primary and siblings)
                    var allStudents = new List<ERPIndia.DTOs.Ledger.StudentDto>();
                    decimal totalRequired = 0;
                    decimal totalReceived = 0;
                    int serialNumber = 1;

                    // Process each student's fee data
                    foreach (var feeSummaryData in feeSummaryDataList)
                    {
                        // Get student details from the feeSummaryData
                        string admissionNo = Convert.ToString(feeSummaryData.AdmsnNo);
                        string studentName = Convert.ToString(feeSummaryData.StudentName);
                        string className = Convert.ToString(feeSummaryData.ClassName);
                        string sectionName = Convert.ToString(feeSummaryData.SectionName);

                        // Get the current studentId from the AdmsnNo
                        string getStudentIdQuery = @"
                    SELECT StudentId 
                    FROM dbo.vwStudentInfo 
                    WHERE AdmsnNo = @AdmsnNo 
                    AND SessionID = @SessionID 
                    AND TenantID = @TenantID 
                    AND IsActive = 1 
                    AND IsDeleted = 0";

                        Guid currentStudentId = studentId; // Default to primary student

                        // Only query for studentId if this is not the primary student
                        string studentType = Convert.ToString(feeSummaryData.StudentType);
                        if (studentType == "Sibling")
                        {
                            var siblingStudentId = connection.QueryFirstOrDefault<Guid?>(getStudentIdQuery,
                                new { AdmsnNo = admissionNo, SessionID = sessionId, TenantID = tenantId });

                            if (siblingStudentId.HasValue)
                            {
                                currentStudentId = siblingStudentId.Value;
                            }
                        }

                        // Get receipt numbers 
                        var receiptQuery = @"
                    SELECT ReceiptNo 
                    FROM dbo.FeeReceivedTbl 
                    WHERE StudentID = @StudentID 
                    AND SessionID = @SessionID 
                    AND TenantID = @TenantID 
                    AND IsActive = 1 
                    AND IsDeleted = 0
                    ORDER BY ReceiptNo  ASC";

                        var receiptNumbers = connection.Query<string>(receiptQuery,
                            new { StudentID = currentStudentId, SessionID = sessionId, TenantID = tenantId }).ToList();

                        // Create fee details object
                        var feeDetails = new FeeDetailsDto
                        {
                            OldBalance = Convert.ToDecimal(feeSummaryData.OldBalance ?? 0),
                            AcademicFee = Convert.ToDecimal(feeSummaryData.AcademicFee ?? 0),
                            TotalAcademicFee = Convert.ToDecimal(feeSummaryData.TotalAcademicFee ?? 0),
                            TransportFee = Convert.ToDecimal(feeSummaryData.TotalTransportFee ?? 0),
                            TotalTransportFee = Convert.ToDecimal(feeSummaryData.TotalTransportFee ?? 0),
                            MonthlyDiscount = Convert.ToDecimal(feeSummaryData.TotalMonthlyDiscount ?? 0),
                            HeadWiseDiscount = Convert.ToDecimal(feeSummaryData.TotalHeadWiseDiscount ?? 0),
                            TotalLateFee = Convert.ToDecimal(feeSummaryData.TotalLateFee ?? 0),
                            TotalReceiptDiscount = Convert.ToDecimal(feeSummaryData.TotalReceiptDiscount ?? 0),
                            // Use TotalPaid from the stored procedure instead of separate query
                            TotalReceived = Convert.ToDecimal(feeSummaryData.TotalPaid ?? 0),
                            ReceiptNumbers = receiptNumbers,
                            FinalDueAmount = Convert.ToDecimal(feeSummaryData.FinalDueAmount ?? 0)
                        };
                        feeDetails.Additions = feeDetails.TotalLateFee ;
                        // Calculate derived values
                        feeDetails.Deductions = feeDetails.MonthlyDiscount + feeDetails.HeadWiseDiscount + feeDetails.TotalReceiptDiscount;

                        // Total Required is the sum of fees before discounts
                        feeDetails.TotalRequired = feeDetails.TotalAcademicFee + feeDetails.TotalTransportFee+ feeDetails.Additions- feeDetails.Deductions;

                        feeDetails.TotalDues = feeDetails.TotalRequired;

                        // Calculate proportions for received amounts
                        var totalFees = feeDetails.TotalAcademicFee + feeDetails.TotalTransportFee + feeDetails.Additions - feeDetails.Deductions;
                        if (totalFees > 0)
                        {
                            feeDetails.TransportFeeReceived = Math.Min(feeDetails.TransportFee, feeDetails.TotalReceived);
                            feeDetails.AcademicFeeReceived = feeDetails.TotalReceived - feeDetails.TransportFeeReceived;
                        }

                        // Create student DTO
                        var studentDto = new ERPIndia.DTOs.Ledger.StudentDto
                        {
                            StudentId = currentStudentId,
                            SerialNumber = serialNumber++,
                            Name = studentName,
                            Class = className,
                            Section = sectionName,
                            FeeDetails = feeDetails
                        };

                        // Add to list and update totals
                        allStudents.Add(studentDto);
                        totalRequired += feeDetails.TotalRequired;
                        totalReceived += feeDetails.TotalReceived;
                    }

                    // Calculate totals correctly
                    decimal totalDue = allStudents.Sum(s => s.FeeDetails.TotalRequired) - allStudents.Sum(s => s.FeeDetails.TotalReceived);
                    if(totalDue<0)
                    {
                        totalDue = 0;
                    }
                    decimal totalCollected = allStudents.Sum(s => s.FeeDetails.TotalReceived);

                    // Total fee required is the sum of all total required amounts from each student
                    decimal totalFeeRequired = allStudents.Sum(s => s.FeeDetails.TotalRequired);

                    // Return the complete DTO
                    return new StudentLedgerWithSiblingDto
                    {
                        SchoolInfo = schoolInfo,
                        ParentInfo = parentInfo,
                        TotalAmount = totalFeeRequired,
                        Students = allStudents,
                        FeeSummary = new FeeSummaryDto
                        {
                            RemainingToPay = totalDue,
                            TotalFeeRequired = totalFeeRequired,
                            TotalCollectedFee = totalCollected
                        }
                    };
                }
            }
        }
        [HttpPost]
        public async Task<ActionResult> SubmitPayment(FeePaymentModel model)
        {
            try
            {
                // Generate receipt number
                int receiptNumber = await GenerateReceiptNumber(model.TenantCode.ToString());

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Use a transaction to ensure all operations succeed or fail together
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Insert into FeeReceivedTbl
                            await InsertFeeReceiptAsync(connection, transaction, receiptNumber, model);

                            // 2. Insert into FeeMonthlyFeeTbl
                            await InsertMonthlyFeesAsync(connection, transaction, receiptNumber, model);

                            // 3. Insert into FeeTransportFeeTbl if applicable
                            await InsertTransportFeesAsync(connection, transaction, receiptNumber, model, model.TransportRoute);

                            // Commit the transaction
                            transaction.Commit();

                            // Return success response with receipt number
                            return Json(new { success = true, receiptNumber = receiptNumber });
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message);
                            // Roll back the transaction in case of error
                            transaction.Rollback();
                            throw new Exception("Database transaction failed", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (implement proper logging)
                Console.WriteLine($"Error in SubmitPayment: {ex.Message}");
                return View();
                
            }
        }

        [HttpGet]
        public ActionResult PrintReceipt(int receiptNumber)
        {
            // Get receipt data from the database
            var receiptData = GetReceiptData(receiptNumber);

            // Return the receipt view
            return View(receiptData);
        }
        private DataTable ConvertToDataTable<T>(IEnumerable<T> data)
        {
            var properties = typeof(T).GetProperties();
            var table = new DataTable();

            foreach (var property in properties)
            {
                table.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var property in properties)
                {
                    row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            return table;
        }
        private List<rptFeeReceiptDto> GetDataFromSqlView(string ReceiptNo,string SchoolCode)
        {
            var clonedResults = new List<rptFeeReceiptDto>();
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                // Open the connection
                db.Open();

                // Simple query from view
                string sql = "EXEC dbo.sp_GetFeeReceiptDetails @ReceiptNo, @SchoolCode;";

                // Execute query with parameters and get results
                var results = db.Query<rptFeeReceiptDto>(sql, new { ReceiptNo = ReceiptNo, SchoolCode = SchoolCode }).ToList();
                clonedResults.Add(results[0]);


                foreach (var item in results)
                {

                    // Deep copy each item - assuming rptFeeReceiptDto has a copy constructor or method
                    // If it doesn't, you'll need to manually create a new instance and copy all properties
                    clonedResults.Add(item.Clone());  // Assuming Clone() method exists
                }

                // Execute query and return results as a list of your model
                return clonedResults;
            }
        }
        [HttpGet]
        public ActionResult FeeReceipt(int receiptNumber = 0, int code = 0)
        {
            // Get tenant code from session
            string tenantCode = CommonLogic.GetSessionValue(StringConstants.TenantCode);

            // Check if tenant code exists
            if (string.IsNullOrEmpty(tenantCode))
            {
                return Content("<b>No Fee Record Found</b>", "text/html");
            }
            // Check if receipt number and code are valid
            else if (receiptNumber <= 0 || code <= 0)
            {
                return Content("<b>No Fee Record Found</b>", "text/html");
            }
            // Check if tenant code matches the provided code
            else if (tenantCode != code.ToString())
            {
                return Content("<b>No Fee Record Found</b>", "text/html");
            }
            else
            {
                var viewData = GetDataFromSqlView(receiptNumber.ToString(), tenantCode);
                string printtime = viewData[0].PaymentMethod + " : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : " + CommonLogic.GetSessionValue(StringConstants.FullName);
                // receiptData.PrintedByDateTime = 

                // Convert to DataTable for Crystal Reports
                DataTable dataTable = ConvertToDataTable(viewData);
                string feefile = CommonLogic.GetSessionValue(StringConstants.IsSingleFee);
                if (feefile == "Y" && dataTable.Rows.Count == 2)
                {
                    dataTable.Rows.RemoveAt(0);
                }
                if (feefile =="Y")
                return ViewReport(@"/Reports/CrystalReport/0/FeeSingle.rpt", dataTable, printtime);
                else
                    return ViewReport(@"/Reports/CrystalReport/0/Fees.rpt", dataTable, printtime);
                // Get receipt data from the database
                //var receiptData = ReceiptData1(receiptNumber, code);

                //// Check if receipt data exists
                //if (receiptData == null)
                //{
                //    return Content("<b>No Fee Record Found</b>", "text/html");
                //}

                //// Return the receipt view with data
                //return View(receiptData);
            }
        }
        [HttpGet]
        public ActionResult Receipt1(int receiptNumber = 0, int code = 0)
        {
            // Get tenant code from session
            string tenantCode = CommonLogic.GetSessionValue(StringConstants.TenantCode);

            // Check if tenant code exists
            if (string.IsNullOrEmpty(tenantCode))
            {
                return Content("<b>No Fee Record Found</b>", "text/html");
            }
            // Check if receipt number and code are valid
            else if (receiptNumber <= 0 || code <= 0)
            {
                return Content("<b>No Fee Record Found</b>", "text/html");
            }
            // Check if tenant code matches the provided code
            else if (tenantCode != code.ToString())
            {
                return Content("<b>No Fee Record Found</b>", "text/html");
            }
            else
            {
                var viewData = GetDataFromSqlView(receiptNumber.ToString(), tenantCode);
                string printtime= viewData[0].PaymentMethod + " : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : " + CommonLogic.GetSessionValue(StringConstants.FullName);
                // receiptData.PrintedByDateTime = 

                // Convert to DataTable for Crystal Reports
                DataTable dataTable = ConvertToDataTable(viewData);
                return ViewReport(@"/Reports/CrystalReport/0/Fees.rpt", dataTable,printtime);
                // Get receipt data from the database
                //var receiptData = ReceiptData1(receiptNumber, code);

                //// Check if receipt data exists
                //if (receiptData == null)
                //{
                //    return Content("<b>No Fee Record Found</b>", "text/html");
                //}

                //// Return the receipt view with data
                //return View(receiptData);
            }
        }
        private FeeReceiptDto ReceiptData1(int receiptNumber, int code)
        {

            var receiptData = new FeeReceiptDto { ReceiptNumber = receiptNumber.ToString() };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // First get the main receipt details
                string mainQuery = @"
             SELECT r.*, s.FirstName,s.photo ,s.FatherName, s.ClassName, s.SectionName, s.RollNo,
                t.PrintTitle,
                t.Line1,
                t.Line2,
                t.Line3,
                t.Line4,
                t.FeeNote1,
                t.FeeNote2,
                t.FeeNote3,
                t.FeeNote4,
                t.FeeNote5,
                t.LOGOImg,
                t.SIGNImg,
                a.PrintName,
                dbo.GetMonthsByReceiptNo (r.ReceiptNo, r.SchoolCode) AS Months,
                dbo.GetTransportMonthsByReceiptNo (r.ReceiptNo, r.SchoolCode) AS TransMonth,
                dbo.GetTransportRoutesByReceiptNo (r.ReceiptNo, r.SchoolCode) AS TransRoute,
                dbo.GetTransportAmountsByReceiptNo (r.ReceiptNo, r.SchoolCode) AS TransAmount
                FROM FeeReceivedTbl r
                JOIN vwStudentInfo s ON r.AdmissionNo = s.AdmsnNo
                JOIN Tenants t ON s.TenantID=t.TenantID
                JOIN AcademicSessionMaster a ON a.SessionID = r.SessionID
            WHERE r.ReceiptNo = @ReceiptNo and t.TenantCode=@TenantCode";

                using (SqlCommand cmd = new SqlCommand(mainQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ReceiptNo", receiptNumber);
                    cmd.Parameters.AddWithValue("@TenantCode", code);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            //SetSchoolinforData
                            receiptData.SchoolInfo.Name = reader["PrintTitle"].ToString();
                            receiptData.SchoolInfo.Address = reader["Line1"].ToString();
                            receiptData.SchoolInfo.Phone = reader["Line2"].ToString();
                            receiptData.SchoolInfo.Email = reader["Line3"].ToString();
                            receiptData.SchoolInfo.LOGOImg ="/Documents/"+CommonLogic.GetSessionValue(StringConstants.TenantCode)+"/SchoolProfile/" + reader["LOGOImg"].ToString();
                            receiptData.SchoolInfo.SIGNImg = "/Documents/" + CommonLogic.GetSessionValue(StringConstants.TenantCode) + "/SchoolProfile/" +reader["SIGNImg"].ToString();
                            receiptData.SchoolInfo.FeeNote1 = reader["FeeNote1"].ToString();
                            receiptData.SchoolInfo.FeeNote2 = reader["FeeNote2"].ToString();
                            receiptData.SchoolInfo.FeeNote3 = reader["FeeNote3"].ToString();
                            receiptData.SchoolInfo.FeeNote4 = reader["FeeNote4"].ToString();
                            receiptData.SchoolInfo.FeeNote5 = reader["FeeNote5"].ToString();


                            receiptData.FinancialSummary.TotalAmount = Convert.ToDecimal(reader["TotalFee"]);
                            receiptData.ReceivedAmount = Convert.ToDecimal(reader["Received"]);
                            receiptData.ReceivedAmountInWords = IndianCurrencyConverter.ConvertToWords(receiptData.ReceivedAmount);
                            receiptData.FinancialSummary.OldBalance = Convert.ToDecimal(reader["OldBalance"]); 
                            receiptData.FinancialSummary.OtherCharge = Convert.ToDecimal(reader["LateTotal"]);
                            receiptData.FinancialSummary.Concession = Convert.ToDecimal(reader["ConcessinTotal"]);
                            receiptData.FinancialSummary.RemainingAmount = Convert.ToDecimal(reader["Remain"]);
                            // Set receipt properties from reader
                            receiptData.RegistrationNumber = reader["AdmissionNo"].ToString();
                            receiptData.StudentName = reader["FirstName"].ToString();
                            receiptData.Months = reader["Months"].ToString();
                            receiptData.FatherName = reader["FatherName"].ToString();
                            receiptData.ClassStandard = reader["ClassName"].ToString();
                            receiptData.Section = reader["SectionName"].ToString();
                            receiptData.Session = reader["PrintName"].ToString();
                            receiptData.ReceiptDate = Convert.ToDateTime(reader["EntryTime"]);
                            receiptData.PaymentMode= reader["PaymentMode"].ToString();
                            receiptData.TransportMonth = reader["TransMonth"].ToString();
                            receiptData.TransportRoutes = reader["TransRoute"].ToString();
                            receiptData.TransportAmount = reader["TransAmount"].ToString();
                        }
                    }
                }

                // Now get the fee items
                string feeItemsQuery = @"SELECT  FeeName, sum(FeeAmount) AS FeeAmount FROM FeeMonthlyFeeTbl
WHERE ReceiptNo = @ReceiptNo AND SchoolCode=@SchoolCode
GROUP BY FeeName,FeeAmount
ORDER BY FeeName ";

                receiptData.FeeItems = new List<FeeItemDto>();

                using (SqlCommand cmd = new SqlCommand(feeItemsQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ReceiptNo", receiptNumber);
                    cmd.Parameters.AddWithValue("@SchoolCode", code);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            receiptData.FeeItems.Add(new FeeItemDto
                            {
                                FeeName = reader["FeeName"].ToString(),
                                Amount = Convert.ToDecimal(reader["FeeAmount"])
                            });
                        }
                    }
                }

                // Get any transport fees
                string transportQuery = @"
            SELECT FeeMonth, RouteName, FeeAmount
            FROM FeeTransportFeeTbl
            WHERE ReceiptNo = @ReceiptNo";

                receiptData.TransportDetails = new List<TransportDetailsDto>();

                using (SqlCommand cmd = new SqlCommand(transportQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ReceiptNo", receiptNumber);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            receiptData.TransportDetails.Add(new TransportDetailsDto
                            {
                                Month = reader["FeeMonth"].ToString(),
                                Amount = Convert.ToDecimal(reader["FeeAmount"])
                            });
                        }
                    }
                }
            }
            //Cash Mar 21 2025 7:13PM -Admin
            receiptData.PrintedByDateTime = receiptData.PaymentMode+ " : " +DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : "+ CommonLogic.GetSessionValue(StringConstants.FullName);
            return receiptData;
        }
        private ReceiptData GetReceiptData(int receiptNumber)
        {
            var receiptData = new ReceiptData { ReceiptNo = receiptNumber };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // First get the main receipt details
                string mainQuery = @"
            SELECT r.*, s.FirstName, s.FatherName, s.ClassName, s.SectionName, s.RollNo,
            t.PrintTitle,
            t.Line1,
            t.Line2,
            t.Line3,
            t.Line4
            FROM FeeReceivedTbl r
            JOIN vwStudentInfo s ON r.AdmissionNo = s.AdmsnNo
            JOIN Tenants t ON s.TenantID=t.TenantID
            WHERE r.ReceiptNo = @ReceiptNo";

                using (SqlCommand cmd = new SqlCommand(mainQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ReceiptNo", receiptNumber);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            receiptData.PrintTitle = reader["PrintTitle"].ToString();
                            receiptData.Line1 = reader["Line1"].ToString();
                            receiptData.Line2 = reader["Line2"].ToString();
                            receiptData.Line3 = reader["Line3"].ToString();
                            receiptData.Line4 = reader["Line4"].ToString();
                            // Set receipt properties from reader
                            receiptData.SchoolCode = Convert.ToInt32(reader["SchoolCode"]);
                            receiptData.AdmissionNo = Convert.ToInt32(reader["AdmissionNo"]);
                            receiptData.StudentName = reader["FirstName"].ToString();
                            receiptData.FatherName = reader["FatherName"].ToString();
                            receiptData.Class = reader["ClassName"].ToString();
                            receiptData.Section = reader["SectionName"].ToString();
                            receiptData.RollNo = reader["RollNo"].ToString();
                            receiptData.ConcessinAuto = Convert.ToDecimal(reader["ConcessinAuto"]); 
                            receiptData.OldBalance = Convert.ToDecimal(reader["OldBalance"]);
                            receiptData.FeeAdded = Convert.ToDecimal(reader["FeeAdded"]);
                            receiptData.LateFee = Convert.ToDecimal(reader["LateFee"]);
                            receiptData.TotalFee = Convert.ToDecimal(reader["TotalFee"]);
                            receiptData.Received = Convert.ToDecimal(reader["Received"]);
                            receiptData.Remain = Convert.ToDecimal(reader["Remain"]);
                            receiptData.ReceiptDate = Convert.ToDateTime(reader["EntryTime"]);
                            receiptData.Note = reader["Note1"].ToString();
                        }
                    }
                }

                // Now get the fee items
                string feeItemsQuery = @"
            SELECT FeeMonth, FeeName, FeeAmount
            FROM FeeMonthlyFeeTbl
            WHERE ReceiptNo = @ReceiptNo
            ORDER BY SNo";

                receiptData.FeeItems = new List<FeeItem>();

                using (SqlCommand cmd = new SqlCommand(feeItemsQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ReceiptNo", receiptNumber);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            receiptData.FeeItems.Add(new FeeItem
                            {
                                Month = reader["FeeMonth"].ToString(),
                                Name = reader["FeeName"].ToString(),
                                Amount = Convert.ToDecimal(reader["FeeAmount"])
                            });
                        }
                    }
                }

                // Get any transport fees
                string transportQuery = @"
            SELECT FeeMonth, RouteName, FeeAmount
            FROM FeeTransportFeeTbl
            WHERE ReceiptNo = @ReceiptNo";

                receiptData.TransportItems = new List<TransportItem>();

                using (SqlCommand cmd = new SqlCommand(transportQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ReceiptNo", receiptNumber);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            receiptData.TransportItems.Add(new TransportItem
                            {
                                Month = reader["FeeMonth"].ToString(),
                                RouteName = reader["RouteName"].ToString(),
                                Amount = Convert.ToDecimal(reader["FeeAmount"])
                            });
                        }
                    }
                }
            }

            return receiptData;
        }


        private StudentInfoDto GetStudentInfo(string studentId)
        {
            // Implement your database query to retrieve student information
            // This is a placeholder implementation
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM vwStudentInfo WHERE StudentId = @StudentId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new StudentInfoDto
                            {
                                // Map database columns to DTO properties
                                StudentId = Guid.Parse(reader["StudentId"].ToString()),
                                AdmsnNo = reader["AdmsnNo"].ToString(),
                                PickupName= reader["PickupName"].ToString(),
                                Photo = reader["Photo"].ToString(),
                                FirstName = reader["FirstName"].ToString(),
                                FatherName = reader["FatherName"].ToString(),
                                ClassName = reader["ClassName"].ToString(),
                                SectionName = reader["SectionName"].ToString(),
                                Gender = reader["Gender"].ToString(),
                                SrNo = reader["SrNo"].ToString(),
                                DOBUI = DateHelper.ParseOrNull(reader["DOB"].ToString()),
                                RollNo = reader["RollNo"].ToString(),
                                Mobile = reader["Mobile"].ToString(),
                                MotherName= reader["MotherName"].ToString(),
                                DiscountName = reader["DiscountName"].ToString(),
                                CategoryName= reader["CategoryName"].ToString(),
                                OldBalance= Convert.ToInt32(reader["OldBalance"])
                            };
                        }

                        // Return empty object if student not found
                        return new StudentInfoDto();
                    }
                }
            }
        }

        private decimal GetStudentBalance(int admissionNo)
        {
            // Implement your database query to get the current balance for a student
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT TOP 1 FeeBalance FROM FeeReceivedTbl WHERE AdmissionNo = @AdmissionNo ORDER BY ReceiptNo DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AdmissionNo", admissionNo);

                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDecimal(result);
                    }

                    return 0;
                }
            }
        }
        public List<FeeDetailDTO> GetStudentFeeDetails1(string studentId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                     connection.Open();
                    var parameters = new
                    {
                        StudentId = Guid.Parse(studentId)
                    };

                    // Query the stored procedure and map directly to intermediate results
                    var rawResults =connection.Query(
                        "[dbo].[GetStudentFeeDetails]",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    // Map the dynamic results to your FeeDetailDTO
                    var result = rawResults.Select(r => new FeeDetailDTO
                    {
                        Id = r.id,
                        Name = r.name,
                        paidAmounts= JsonConvert.DeserializeObject<Dictionary<string, decimal>>(r.paidAmounts),
                        Discounts = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(r.discounts),
                        regularAmounts = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(r.regularAmounts),
                        Months = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(r.months),
                        
                    }).ToList();

                    string logString = JsonConvert.SerializeObject(result, Formatting.Indented);
                    Logger.Debug(logString); // Changed from Error to Debug for logging successful results

                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting fee details for student {studentId}: {ex.Message}");
                throw;
            }
        }
        private List<FeeDetailDTO> GetStudentFeeDetails(string studentId)
        {
            // Implement your database query to get fee details for the student
            // This is a placeholder implementation
            List<FeeDetailDTO> feeDetails = new List<FeeDetailDTO>();


            // Add some example fee details
            feeDetails.Add(new FeeDetailDTO
            {
                
                Name = "Admission Fee",
                Months = new Dictionary<string, decimal>
                {
                    { "Apr", 500.00m }
                }
            });

            feeDetails.Add(new FeeDetailDTO
            {
                
                Name = "Tuition Fee",
                Months = new Dictionary<string, decimal>
                {
                    { "Apr", 800.00m },
                    { "May", 800.00m },
                    { "Jun", 800.00m },
                    { "Jul", 800.00m },
                    { "Aug", 800.00m },
                    { "Sep", 800.00m },
                    { "Oct", 800.00m },
                    { "Nov", 800.00m },
                    { "Dec", 800.00m },
                    { "Jan", 800.00m },
                    { "Feb", 800.00m },
                    { "Mar", 800.00m }
                }
            });

            // You should replace this with actual data from your database
            return feeDetails;
        }

        private async Task<int> GenerateReceiptNumber(string SchoolCode)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query =string.Format("SELECT ISNULL(MAX(ReceiptNo), 0) + 1 FROM FeeReceivedTbl where SchoolCode='{0}'",SchoolCode);

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        private async Task InsertFeeReceiptAsync(SqlConnection connection, SqlTransaction transaction, int receiptNumber, FeePaymentModel model)
        {
            DateTime DateMannual = Convert.ToDateTime(model.EntryTime);
            string sql = @"
                INSERT INTO FeeReceivedTbl (
                    ReceiptNo,DateAuto,DateMannual, SchoolCode, AdmissionNo, FeeAdded, ConcessinAuto,ConcessinMannual,
                    FeeBalance, LateFee, LateFeeAuto, OldBalance, 
                    Received, Remain, TotalFee, Note1, Note2, EntryTime, UserId
                    ,SessionYear,SessionID,TenantID,TenantCode,StudentID,PaymentMode
                ) VALUES (
                    @ReceiptNo,getdate(),@DateMannual, @SchoolCode, @AdmissionNo, @FeeAdded, @ConcessinAuto,@ConcessinMannual,
                    @FeeBalance, @LateFee, @LateFeeAuto, @OldBalance, 
                    @Received, @Remain, @TotalFee, @Note1, @Note2, @EntryTime, @UserId
                    ,@SessionYear,@SessionID,@TenantID,@TenantCode,@StudentID,@PaymentMode
                )";

            using (SqlCommand cmd = new SqlCommand(sql, connection, transaction))
            {
                // Add parameters
                cmd.Parameters.AddWithValue("@ReceiptNo", receiptNumber);
                cmd.Parameters.AddWithValue("@DateMannual", DateMannual);
                cmd.Parameters.AddWithValue("@SchoolCode", model.SchoolCode);
                cmd.Parameters.AddWithValue("@AdmissionNo", model.AdmissionNo);
                cmd.Parameters.AddWithValue("@FeeAdded", model.FeeAdded);
                cmd.Parameters.AddWithValue("@ConcessinAuto", model.ConcessinAuto); 
                cmd.Parameters.AddWithValue("@FeeBalance", model.Remain);
                cmd.Parameters.AddWithValue("@LateFee", model.LateFee);
                cmd.Parameters.AddWithValue("@LateFeeAuto", model.LateFeeAuto); // Default to 0 for manual entry
                cmd.Parameters.AddWithValue("@OldBalance", model.OldBalance);
                cmd.Parameters.AddWithValue("@Received", model.Received);
                cmd.Parameters.AddWithValue("@Remain", model.Remain);
                cmd.Parameters.AddWithValue("@TotalFee", model.TotalFee);
                cmd.Parameters.AddWithValue("@Note1", model.Note1 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Note2", model.Note2 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EntryTime", DateTime.Parse(model.EntryTime));
                cmd.Parameters.AddWithValue("@UserId", GetCurrentUserId());
                cmd.Parameters.AddWithValue("@SessionYear", model.SessionYear);
                cmd.Parameters.AddWithValue("@SessionID", model.SessionID);
                cmd.Parameters.AddWithValue("@TenantID", model.TenantID);
                cmd.Parameters.AddWithValue("@TenantCode", model.TenantCode);
                cmd.Parameters.AddWithValue("@StudentID", model.StudentID);
                cmd.Parameters.AddWithValue("@ConcessinMannual", model.ConcessinMannual);
                cmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode); 
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task InsertMonthlyFeesAsync(SqlConnection connection, SqlTransaction transaction, int receiptNumber, FeePaymentModel model)
        {
            string sql = @"
                INSERT INTO FeeMonthlyFeeTbl (
                    id, ReceiptNo, SchoolCode, AdmissionNo, 
                    SNo, FeeMonth, FeeName, FeeAmount, EntryDate
                    ,SessionYear,SessionID,TenantID,TenantCode,StudentID
                ) VALUES (
                    @id, @ReceiptNo, @SchoolCode, @AdmissionNo, 
                    @SNo, @FeeMonth, @FeeName, @FeeAmount, @EntryDate
                    ,@SessionYear,@SessionID,@TenantID,@TenantCode,@StudentID
                )";

            int idCounter = await GetNextFeeMonthlyId(connection, transaction);

            foreach (var fee in model.MonthlyFees)
            {
                using (SqlCommand cmd = new SqlCommand(sql, connection, transaction))
                {
                    // Add parameters
                    cmd.Parameters.AddWithValue("@id", idCounter++);
                    cmd.Parameters.AddWithValue("@ReceiptNo", receiptNumber);
                    cmd.Parameters.AddWithValue("@SchoolCode", model.SchoolCode);
                    cmd.Parameters.AddWithValue("@AdmissionNo", model.AdmissionNo);
                    cmd.Parameters.AddWithValue("@SNo", fee.FeeHeadId); // Using FeeHeadId as SNo
                    cmd.Parameters.AddWithValue("@FeeMonth", fee.Month);
                    cmd.Parameters.AddWithValue("@FeeName", fee.FeeName);
                    cmd.Parameters.AddWithValue("@FeeAmount", fee.RegularAmount);
                    cmd.Parameters.AddWithValue("@EntryDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@SessionYear", model.SessionYear);
                    cmd.Parameters.AddWithValue("@SessionID", model.SessionID);
                    cmd.Parameters.AddWithValue("@TenantID", model.TenantID);
                    cmd.Parameters.AddWithValue("@TenantCode", model.TenantCode);
                    cmd.Parameters.AddWithValue("@StudentID", model.StudentID);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task InsertTransportFeesAsync(SqlConnection connection, SqlTransaction transaction, int receiptNumber, FeePaymentModel model,string routename)
        {
            // Only proceed if there are transport fees
            if (model.TransportFees == null || !model.TransportFees.Any())
            {
                return;
            }

            string sql = @"
                INSERT INTO FeeTransportFeeTbl (
                    id, ReceiptNo, SchoolCode, AdmissionNo, 
                    RouteName, FeeAmount, FeeMonth, EntryDate
                    ,SessionYear,SessionID,TenantID,TenantCode,StudentID
                ) VALUES (
                    @id, @ReceiptNo, @SchoolCode, @AdmissionNo, 
                    @RouteName, @FeeAmount, @FeeMonth, @EntryDate
                    ,@SessionYear,@SessionID,@TenantID,@TenantCode,@StudentID
                )";

            int idCounter = await GetNextTransportFeeId(connection, transaction);
            string defaultRouteName = routename==null?"": routename; // Replace with actual route logic if available
            foreach (var transportFee in model.TransportFees)
            {
                using (SqlCommand cmd = new SqlCommand(sql, connection, transaction))
                {
                    // Add parameters
                    cmd.Parameters.AddWithValue("@id", idCounter++);
                    cmd.Parameters.AddWithValue("@ReceiptNo", receiptNumber);
                    cmd.Parameters.AddWithValue("@SchoolCode", model.TenantCode);
                    cmd.Parameters.AddWithValue("@AdmissionNo", model.AdmissionNo);
                    cmd.Parameters.AddWithValue("@RouteName", defaultRouteName);
                    cmd.Parameters.AddWithValue("@FeeAmount", transportFee.RegularAmount);
                    cmd.Parameters.AddWithValue("@FeeMonth", transportFee.Month);
                    cmd.Parameters.AddWithValue("@EntryDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@SessionYear", model.SessionYear);
                    cmd.Parameters.AddWithValue("@SessionID", model.SessionID);
                    cmd.Parameters.AddWithValue("@TenantID", model.TenantID);
                    cmd.Parameters.AddWithValue("@TenantCode", model.TenantCode);
                    cmd.Parameters.AddWithValue("@StudentID", model.StudentID);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<int> GetNextFeeMonthlyId(SqlConnection connection, SqlTransaction transaction)
        {
            string sql = "SELECT ISNULL(MAX(id), 0) + 1 FROM FeeMonthlyFeeTbl";

            using (SqlCommand cmd = new SqlCommand(sql, connection, transaction))
            {
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        private async Task<int> GetNextTransportFeeId(SqlConnection connection, SqlTransaction transaction)
        {
            string sql = "SELECT ISNULL(MAX(id), 0) + 1 FROM FeeTransportFeeTbl";

            using (SqlCommand cmd = new SqlCommand(sql, connection, transaction))
            {
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        private int GetCurrentUserId()
        {
            // Get the current user ID from the authentication system
            // This is a placeholder - implement based on your authentication system
            return 1; // Default user ID
        }
    }
}