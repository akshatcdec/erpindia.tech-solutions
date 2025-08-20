using Dapper;
using ERPIndia.DTOs.Student;
using ERPIndia.FeeSummary.DTO;
using Microsoft.ReportingServices.RdlExpressions.ExpressionHostObjectModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace ERPIndia.FeeSummary.DTO
{
    /// <summary>
    /// Data Transfer Object for Fee Summary Report
    /// </summary>
    public class FeeSummaryDTO
    {
        /// <summary>
        /// Sequence number in the report
        /// </summary>
        public int Seq { get; set; }

        /// <summary>
        /// Student's Roll Number
        /// </summary>
        public string RollNo { get; set; }

        /// <summary>
        /// Student's SR Number
        /// </summary>
        public string SRNo { get; set; }

        /// <summary>
        /// Student's Name
        /// </summary>
        public string StudentName { get; set; }

        /// <summary>
        /// Father's Name
        /// </summary>
        public string FatherName { get; set; }

        /// <summary>
        /// Class Name with Section (e.g., "I- A")
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Mobile Number
        /// </summary>
        public string MobileNo { get; set; }

        /// <summary>
        /// Current Address
        /// </summary>
        public string CurrentAddress { get; set; }

        /// <summary>
        /// Previous balance from the last session
        /// </summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Regular tuition fee total for selected months
        /// </summary>
        public decimal RegularFee { get; set; }

        /// <summary>
        /// Transport fee total for selected months
        /// </summary>
        public decimal TransportFee { get; set; }

        /// <summary>
        /// Late fee total
        /// </summary>
        public decimal LateFee { get; set; }

        /// <summary>
        /// Gross total (OpeningBalance + RegularFee + TransportFee + LateFee)
        /// </summary>
        public decimal GrossTotal { get; set; }

        /// <summary>
        /// Monthly fixed discounts
        /// </summary>
        public decimal MonthlyDiscount { get; set; }

        /// <summary>
        /// Headwise discounts (fee structure based)
        /// </summary>
        public decimal HeadwiseDiscount { get; set; }

        /// <summary>
        /// Additional concessions given at payment time
        /// </summary>
        public decimal AdditionalConcession { get; set; }

        /// <summary>
        /// Total discounts (MonthlyDiscount + HeadwiseDiscount + AdditionalConcession)
        /// </summary>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Net total amount to be paid (GrossTotal - TotalDiscount)
        /// </summary>
        public decimal NetTotal { get; set; }

        /// <summary>
        /// Total amount actually paid (from Received column)
        /// </summary>
        public decimal TotalPaid { get; set; }

        /// <summary>
        /// Net payable amount (dues) (NetTotal - TotalPaid)
        /// </summary>
        public decimal NetPayable { get; set; }
    }
    /// <summary>
    /// View Model for Fee Summary Report page
    /// </summary>
    public class FeeSummaryViewModel
    {
        public FeeSummaryViewModel()
        {
            FeeSummaries = new List<FeeSummaryDTO>();
            StudentSummaries = new List<StudentViewInfoDTO>();
            ClassList = new List<SelectListItem>();
            SectionList = new List<SelectListItem>();
            SessionList = new List<SelectListItem>();
        }

        /// <summary>
        /// List of Fee Summary items
        /// </summary>
        public List<FeeSummaryDTO> FeeSummaries { get; set; }
        public List<StudentViewInfoDTO> StudentSummaries { get; set; }
        /// <summary>
        /// Selected Session ID
        /// </summary>
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
        public List<SelectListItem> SessionList { get; set; }

        /// <summary>
        /// Class dropdown items
        /// </summary>
        public List<SelectListItem> ClassList { get; set; }

        /// <summary>
        /// Section dropdown items
        /// </summary>
        public List<SelectListItem> SectionList { get; set; }

        /// <summary>
        /// Report generation date
        /// </summary>
        public DateTime ReportDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Session year display (e.g., "2024-25")
        /// </summary>
        public string SessionYear { get; set; }

        /// <summary>
        /// Total amount across all students
        /// </summary>
        public decimal TotalAmount => FeeSummaries.Sum(f => f.NetTotal);

        /// <summary>
        /// Total paid amount across all students
        /// </summary>
        public decimal TotalPaid => FeeSummaries.Sum(f => f.TotalPaid);

        /// <summary>
        /// Total due amount across all students
        /// </summary>
        public decimal TotalDue => FeeSummaries.Sum(f => f.NetPayable);
    }
}
    namespace ERPIndia.Controllers
{
    [Authorize]
    public class FeeSummaryController : BaseController
    {
       
        private readonly DropdownController _dropdownController;
        public FeeSummaryController()
        {
            _dropdownController = new DropdownController();
        }

        // GET: FeeSummary
        public ActionResult Index(string selectedSession = null, string selectedClass = "ALL", string selectedSection = "ALL")
        {
            // Handle null or empty parameters by defaulting to "ALL"
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

            // Build the view model
            var viewModel = new FeeSummaryViewModel
            {
                SelectedSession = selectedSession,
                SelectedClass = selectedClass,
                SelectedSection = selectedSection,
                SessionYear = sessionYear,
                // Populate dropdown lists
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
                // Get fee summary data
                FeeSummaries = GetFeeSummaryClassWise(sessionId, classId, sectionId, tenantId),
                ReportDate = DateTime.Now
            };

            return View(viewModel);
        }


        private List<FeeSummaryDTO> GetFeeSummaryClassWise(Guid sessionId, Guid? classId, Guid? sectionId, Guid tenantId)
        {
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    conn.Open();
                    var parameters = new DynamicParameters();
                    parameters.Add("@SessionID", sessionId);
                    parameters.Add("@TenantID", tenantId);
                    parameters.Add("@ClassID", classId);
                    parameters.Add("@SectionID", sectionId);
                    parameters.Add("@AsOfDate", DateTime.Now);

                    var result = conn.Query<FeeSummaryDTO>("GetFeesSummaryClassWise", parameters, commandType: CommandType.StoredProcedure);
                    return result.ToList();

                   
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine("Error in GetFeeSummaryClassWise: " + ex.Message);
                // Return empty list in case of error
                return new List<FeeSummaryDTO>();
            }
        }

        // Export to Excel
        public ActionResult ExportToExcel(string selectedSession, string selectedClass, string selectedSection)
        {
            // Convert parameters to GUIDs
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

            // Get tenant info and session year
            var tenantId = CurrentTenantID;
            string sessionYear = CurrentSessionYear.ToString();

            // Get data
            var feeData = GetFeeSummaryClassWise(sessionId, classId, sectionId, tenantId);

            // Generate Excel file (implementation depends on your Excel library)
            // This is a placeholder - actual implementation would use a library like EPPlus or ClosedXML

            // Return file
            return File(new byte[0], "application/vnd.ms-excel", "FeeSummary_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx");
        }

        // Export to PDF
        public ActionResult ExportToPdf(string selectedSession, string selectedClass, string selectedSection)
        {
            // Convert parameters to GUIDs
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

            // Get tenant info and session year
            var tenantId = CurrentTenantID;
            string sessionYear = CurrentSessionYear.ToString();

            // Get data
            var feeData = GetFeeSummaryClassWise(sessionId, classId, sectionId, tenantId);

            // Generate PDF file (implementation depends on your PDF library)
            // This is a placeholder - actual implementation would use a library like iTextSharp or Rotativa

            // Return file
            return File(new byte[0], "application/pdf", "FeeSummary_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
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
    }
}