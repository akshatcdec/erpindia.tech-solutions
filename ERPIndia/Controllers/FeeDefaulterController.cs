// DTOs
using Dapper;
using ERPIndia.Controllers;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using System;
using System.Linq;
using System.Data;
using Newtonsoft.Json;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using CrystalDecisions.Shared;
using ERPIndia.Class.Helper;

namespace ERPIndia.Controllers
{
    public class StudentFeeLedgerViewModel
    {
        public List<StudentFeeLedgerDTO> Students { get; set; }
        public int TotalCount { get; set; }
        public DateTime ReportGeneratedOn { get; set; }
        public Guid SessionId { get; set; }
        public Guid TenantId { get; set; }

        // Report metadata
        public string ReportTitle { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string SelectedSession { get; set; }

        /// <summary>
        /// Selected Class ID
        /// </summary>
        public string SelectedClass { get; set; }

        /// <summary>
        /// Selected Section ID
        /// </summary>
        public string SelectedSection { get; set; }

        /// <summary>
        /// Session dropdown items
        /// </summary>
       
        /// <summary>
        /// Class dropdown items
        /// </summary>
        public List<SelectListItem> ClassList { get; set; }

        /// <summary>
        /// Section dropdown items
        /// </summary>
        public List<SelectListItem> SectionList { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? SectionId { get; set; }
        public int SelectedMonth { get; set; }

        /// <summary>
        /// Month dropdown items - fiscal year from April to March
        /// </summary>
        public List<SelectListItem> MonthList { get; set; }
        public string SelectedMonthDisplayText { get; set; }
        public StudentFeeLedgerViewModel()
        {
            Students = new List<StudentFeeLedgerDTO>();
            ReportGeneratedOn = DateTime.Now;
            ClassList = new List<SelectListItem>();
            SectionList = new List<SelectListItem>();
            MonthList = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "ALL" },
            new SelectListItem { Value = "4", Text = "April" },
            new SelectListItem { Value = "5", Text = "May" },
            new SelectListItem { Value = "6", Text = "June" },
            new SelectListItem { Value = "7", Text = "July" },
            new SelectListItem { Value = "8", Text = "August" },
            new SelectListItem { Value = "9", Text = "September" },
            new SelectListItem { Value = "10", Text = "October" },
            new SelectListItem { Value = "11", Text = "November" },
            new SelectListItem { Value = "12", Text = "December" },
            new SelectListItem { Value = "1", Text = "January" },
            new SelectListItem { Value = "2", Text = "February" },
            new SelectListItem { Value = "3", Text = "March" }
        };
        }
    }

    // The StudentFeeLedgerDTO class as previously defined
    public class StudentFeeLedgerDTO
    {
        public string TenantCode { get; set; }
        public string AdmsnNo { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public decimal OldBalance { get; set; }
        public string StudentType { get; set; }
        public decimal AcademicFee { get; set; }
        public decimal TotalAcademicFee { get; set; }
        public decimal TotalTransportFee { get; set; }
        public decimal TotalMonthlyDiscount { get; set; }
        public decimal TotalHeadWiseDiscount { get; set; }
        public decimal TotalReceiptDiscount { get; set; }
        public decimal TotalLateFee { get; set; }
        public decimal FinalDueAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public string FatherName { get; set; }
        public string FatherAadhar { get; set; }
        public string FatherMobile { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string PrintTitle { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
    }
    
    public class FeeDefaulterDTO
    {
        public int SrNo { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public decimal OldBal { get; set; }
        public decimal Concession { get; set; }
        public decimal LateFee { get; set; }
        public decimal GenerateFee { get; set; }
        public decimal ReqFee { get; set; }
        public decimal Rcvd { get; set; }
        public decimal Dues { get; set; }
        public decimal TrnsPay { get; set; }
        public decimal TrnsRcv { get; set; }
        public decimal TrnsDue { get; set; }
        public decimal TtlRcvd { get; set; }
        public decimal TtlDues { get; set; }
    }

    public class FeeDefaulterHeaderDTO
    {
        public string ReportTitle { get; set; }
        public string ReportDateTime { get; set; }
        public string SessionInfo { get; set; }
        public string ClassSectionInfo { get; set; }
    }

    public class FeeDefaulterViewModel
    {
        public string SelectedSession { get; set; }
        public string SelectedClass { get; set; }
        public string SelectedSection { get; set; }
        public string SessionYear { get; set; }
        public List<SelectListItem> ClassList { get; set; }
        public List<SelectListItem> SectionList { get; set; }
        public List<FeeDefaulterDTO> StudentDefaulters { get; set; }
        public List<FeeDefaulterDTO> ClasswiseTotals { get; set; }
        public FeeDefaulterDTO GrandTotal { get; set; }
        public FeeDefaulterHeaderDTO HeaderInfo { get; set; }
        public DateTime ReportDate { get; set; }
    }

    // Controller
    public class FeeDefaulterController : BaseController
    {
        private readonly DropdownController _dropdownController;
        public FeeDefaulterController()
        {
            _dropdownController = new DropdownController();
        }
        /// <summary>
        /// Generates display text for the selected month range (April to selected month)
        /// </summary>
        private string GetMonthRangeDisplayText(int selectedMonth)
        {
            if (selectedMonth == 0)
            {
                return "April To March";
            }

            string[] monthNames = {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

            // April is always the start month (index 3)
            string startMonth = monthNames[3]; // April

            // Get the end month name
            string endMonth = monthNames[selectedMonth - 1]; // Adjust for 0-based array
            if (selectedMonth == 4)
            {
                return "Only April";
            }
            else
            {
                return $"{startMonth} to {endMonth}";
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
        public ActionResult Index(string selectedSession = null, string selectedClass = "ALL", string selectedSection = "ALL")
        {
            selectedClass = string.IsNullOrWhiteSpace(selectedClass) ? "ALL" : selectedClass;
            selectedSection = string.IsNullOrWhiteSpace(selectedSection) ? "ALL" : selectedSection;

            // If Guid is all zeros, treat it as "ALL"
            if (selectedClass == "00000000-0000-0000-0000-000000000000")
            {
                selectedClass = "ALL";
            }

            if (selectedSection == "00000000-0000-0000-0000-000000000000")
            {
                selectedSection = "ALL";
            }

            // If no session is selected, use current session
            if (string.IsNullOrWhiteSpace(selectedSession))
            {
                selectedSession = CurrentSessionID.ToString();
            }

            // Convert string parameters to GUIDs if necessary
            Guid sessionId;
            if (!Guid.TryParse(selectedSession, out sessionId))
            {
                sessionId = CurrentSessionID;
            }

            Guid? classId = null;
            if (selectedClass != "ALL" && Guid.TryParse(selectedClass, out Guid parsedClassId))
            {
                classId = parsedClassId;
            }

            Guid? sectionId = null;
            if (selectedSection != "ALL" && Guid.TryParse(selectedSection, out Guid parsedSectionId))
            {
                sectionId = parsedSectionId;
            }

            // Fetch dropdown data
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            // Get tenant info
            var tenantCode = Utils.ParseInt(CurrentTenantCode);
            var tenantId = CurrentTenantID;

            // Get session year display (e.g., "2024-25")
            string sessionYear = CurrentSessionYear.ToString();

            // Get the model with the selected filters
            var viewModel = new StudentFeeLedgerViewModel
            {
                SelectedSession = selectedSession,
                SelectedClass = selectedClass,
                SelectedSection = selectedSection,
                SessionId = sessionId,
                TenantId = tenantId,
                // Populate dropdown lists
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
                Students= GetFilteredStudentsFeeLedger(sessionId, classId, sectionId, tenantId)
            };

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult RefreshFeeDefaulterReport1(string selectedClass = "ALL", string selectedSection = "ALL", int selectedMonth = 0)
        {
            selectedClass = string.IsNullOrWhiteSpace(selectedClass) ? "ALL" : selectedClass;
            selectedSection = string.IsNullOrWhiteSpace(selectedSection) ? "ALL" : selectedSection;

            // If Guid is all zeros, treat it as "ALL"
            if (selectedClass == "00000000-0000-0000-0000-000000000000")
            {
                selectedClass = "ALL";
            }

            if (selectedSection == "00000000-0000-0000-0000-000000000000")
            {
                selectedSection = "ALL";
            }

            Guid? classId = null;
            if (selectedClass != "ALL" && Guid.TryParse(selectedClass, out Guid parsedClassId))
            {
                classId = parsedClassId;
            }

            Guid? sectionId = null;
            if (selectedSection != "ALL" && Guid.TryParse(selectedSection, out Guid parsedSectionId))
            {
                sectionId = parsedSectionId;
            }
            string monthDisplayText = GetMonthRangeDisplayText(selectedMonth);
            // Get tenant info
            var tenantId = CurrentTenantID;
            var sessionId = CurrentSessionID;

            // Get the filtered student fee data
            var students = GetFilteredStudentsFeeLedger(sessionId, classId, sectionId, tenantId, selectedMonth);

            // Get the model with the selected filters
            var viewModel = new StudentFeeLedgerViewModel
            {
                SelectedClass = selectedClass,
                SelectedSection = selectedSection,
                SelectedMonth = selectedMonth,
                SelectedMonthDisplayText = monthDisplayText,
                ReportGeneratedOn = DateTime.Now,
                Students = students,
            };

            // Return the partial view with the report
            return PartialView("_Viewer", viewModel);
        }
        public ActionResult RefreshFeeDefaulterReport(string selectedClass = "ALL", string selectedSection = "ALL", int selectedMonth = 0)
        {
            selectedClass = string.IsNullOrWhiteSpace(selectedClass) ? "ALL" : selectedClass;
            selectedSection = string.IsNullOrWhiteSpace(selectedSection) ? "ALL" : selectedSection;
            // If Guid is all zeros, treat it as "ALL"
            if (selectedClass == "00000000-0000-0000-0000-000000000000")
            {
                selectedClass = "ALL";
            }
            if (selectedSection == "00000000-0000-0000-0000-000000000000")
            {
                selectedSection = "ALL";
            }

            Guid? classId = null;
            if (selectedClass != "ALL" && Guid.TryParse(selectedClass, out Guid parsedClassId))
            {
                classId = parsedClassId;
            }

            Guid? sectionId = null;
            if (selectedSection != "ALL" && Guid.TryParse(selectedSection, out Guid parsedSectionId))
            {
                sectionId = parsedSectionId;
            }

            string monthDisplayText = GetMonthRangeDisplayText(selectedMonth);
            // Get tenant info
            var tenantId = CurrentTenantID;
            var sessionId = CurrentSessionID;

            // Get the filtered student fee data
            var students = GetFilteredStudentsFeeLedger(sessionId, classId, sectionId, tenantId, selectedMonth);

            // Create a report view model
            var reportModel = new ReportViewModel
            {
                ReportType = "studentfeeledger",
                // IMPORTANT FIX: Set the ReportPath using the ReportController's GetReportPath method
                ReportPath = Server.MapPath("~/Reports/CrystalReport/0/studentfeeledger.rpt")
            };

            // Set the report data
            reportModel.ReportData = students;
            // Get tenant code from the first student record
            string code = CommonLogic.GetSessionValue(StringConstants.TenantCode);
            string sessionprint = "Session ( " + CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint) + " )";
            string img = CommonLogic.GetSessionValue(StringConstants.LogoImg);
            string logoPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, img));
           
            // Add parameters
            reportModel.Parameters.Add("prmreportlogo", logoPath);
            reportModel.Parameters.Add("prmtitle", "");
            reportModel.Parameters.Add("prmline1", "");
            reportModel.Parameters.Add("prmline2", "");
            reportModel.Parameters.Add("prmline3", "");
            reportModel.Parameters.Add("prmline4", "");
            reportModel.Parameters.Add("prmreportname", "");
            reportModel.Parameters.Add("prmreportitle1", sessionprint);
            reportModel.Parameters.Add("prmreportitle2", monthDisplayText);
            // If using AJAX, handle differently
            if (Request.IsAjaxRequest())
            {
                // Create a unique ID for the report
                string reportId = "feeDefaulter_" + DateTime.Now.Ticks;

                try
                {
                    // Create the Crystal Report
                    ReportDocument reportDocument = new ReportDocument();
                    reportDocument.Load(reportModel.ReportPath);
                    reportDocument.SetDataSource(students);
                    // Set parameters
                    foreach (var parameter in reportModel.Parameters)
                    {
                        reportDocument.SetParameterValue(parameter.Key, parameter.Value);
                    }

                    // Set data source
                   

                    // Store in session
                    Session[reportId] = reportDocument;
                    ViewBag.ReportTitle = "Fee Defaulter";
                    ViewBag.AllowPrint = true;
                    ViewBag.AllowExport = true;
                    // Return partial view with report ID
                    ViewBag.ReportId = reportId;
                    return PartialView("_ReportViewer", reportId);
                }
                catch (Exception ex)
                {
                    return PartialView("_Error", "Error generating report: " + ex.Message);
                }
            }

            // For non-AJAX requests, redirect to the DisplayReport action in ReportController
            return RedirectToAction("DisplayReport", "Report", reportModel);
        }
        private List<StudentFeeLedgerDTO> GetFilteredStudentsFeeLedger(Guid sessionId, Guid? classId, Guid? sectionId, Guid tenantId, int month = 0)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@TenantID", tenantId);
                parameters.Add("@Month", month);

                // Add class and section parameters if provided
                if (classId.HasValue && classId.Value != Guid.Empty)
                {
                    parameters.Add("@ClassID", classId.Value);
                }

                if (sectionId.HasValue && sectionId.Value != Guid.Empty)
                {
                    parameters.Add("@SectionID", sectionId.Value);
                }

                var students = connection.Query<StudentFeeLedgerDTO>(
                    "dbo.sp_AllStudentsFeeLedger",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return students;
            }
        }
     
        public ActionResult StreamReport(string id)
        {
            // Retrieve the report from session
            ReportDocument reportDocument = Session[id] as ReportDocument;

            if (reportDocument == null)
            {
                return HttpNotFound("Report not found");
            }

            try
            {
                // Export directly to stream
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
            }
            catch (Exception ex)
            {
                return Content("Error generating report: " + ex.Message);
            }
        }

        [HttpGet]
        public ActionResult CleanupReport(string id)
        {
            ReportDocument reportDocument = Session[id] as ReportDocument;

            if (reportDocument != null)
            {
                reportDocument.Close();
                reportDocument.Dispose();
                Session.Remove(id);
            }

            return new EmptyResult();
        }

    }
}