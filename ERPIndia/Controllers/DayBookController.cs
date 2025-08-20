using CrystalDecisions.CrystalReports.Engine;
using Dapper;
using ERPIndia.Class.Helper;
using ERPIndia.Dashboard.DTOs;
using ERPIndia.DTOs.Cashbook;
using ERPIndia.DTOs.Fee;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class DayBookController : BaseController
    {
        private readonly DropdownController _dropdownController;
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        public DayBookController()
        {
            _dropdownController = new DropdownController();
        }
        // GET: DayBook
        public ActionResult DeletedRecords(DateTime? fromDate, DateTime? toDate)
        {
            var model = new DeletedFeeRecordsViewModel();

            // Set default date range if not provided
            if (!fromDate.HasValue)
                fromDate = DateTime.Today.AddDays(-30); // Last 30 days by default

            if (!toDate.HasValue)
                toDate = DateTime.Today;

            model.FromDate = fromDate.Value;
            model.ToDate = toDate.Value;

            try
            {
                // Get session and tenant information
                int tenantCode = Convert.ToInt32(CommonLogic.GetSessionValue(StringConstants.TenantCode));
                Guid sessionId = Guid.Parse(CommonLogic.GetSessionValue(StringConstants.ActiveSessionID));

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand("dbo.GetDeletedFeeRecords", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@FromDate", fromDate.Value);
                        command.Parameters.AddWithValue("@ToDate", toDate.Value);
                        command.Parameters.AddWithValue("@TenantCode", tenantCode);
                        command.Parameters.AddWithValue("@SessionID", sessionId);

                        using (var reader = command.ExecuteReader())
                        {
                            var deletedRecords = new List<DeletedFeeRecordDto>();

                            while (reader.Read())
                            {
                                deletedRecords.Add(new DeletedFeeRecordDto
                                {
                                    ReceiptNo = reader["Rcpt"] != DBNull.Value ? Convert.ToInt32(reader["Rcpt"]) : 0,
                                    DateAuto = reader["DateAuto"] != DBNull.Value ? reader["DateAuto"].ToString() : string.Empty,
                                    Code = reader["Code"] != DBNull.Value ? Convert.ToInt32(reader["Code"]) : 0,
                                    Amount = reader["Amnt"] != DBNull.Value ? Convert.ToDecimal(reader["Amnt"]) : 0,
                                    Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : string.Empty,
                                    Sr = reader["Sr"] != DBNull.Value ? reader["Sr"].ToString() : string.Empty,
                                    Roll = reader["Roll"] != DBNull.Value ? Convert.ToString(reader["Roll"]) : string.Empty,
                                    Class = reader["Cls"] != DBNull.Value ? reader["Cls"].ToString() : string.Empty,
                                    Section = reader["Sec"] != DBNull.Value ? reader["Sec"].ToString() : string.Empty,
                                    Mobile = reader["Mobile"] != DBNull.Value ? reader["Mobile"].ToString() : string.Empty,
                                    DeletedBy = reader["DeletedBy"] != DBNull.Value ? reader["DeletedBy"].ToString() : string.Empty,
                                    DeletedDate = reader["DeletedDate"] != DBNull.Value ? reader["DeletedDate"].ToString() : string.Empty,
                                    DeleteReason = reader["DeleteReason"] != DBNull.Value ? reader["DeleteReason"].ToString() : string.Empty
                                });
                            }

                            model.DeletedRecords = deletedRecords.OrderBy(x => x.ReceiptNo).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                model.ErrorMessage = "An error occurred while retrieving deleted records: " + ex.Message;
            }

            return View(model);
        }
        [HttpPost]
        public JsonResult DeleteFeeReceipt(string id, string reason = null)
        {
            try
            {
                Guid receiptId;
                if (!Guid.TryParse(id, out receiptId))
                {
                    return Json(new { success = false, message = "Invalid receipt ID" });
                }

                string connectionString = _connectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get current user ID
                    Guid currentUserId = CurrentTenantUserID;

                    // Call the stored procedure we created
                    using (var command = new SqlCommand("dbo.DeleteLatestFeeReceipt", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ReceiptId", receiptId));
                        command.Parameters.Add(new SqlParameter("@DeleteReason", (object)reason ?? DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@ModifiedBy", currentUserId));

                        try
                        {
                            // Execute the stored procedure
                            command.ExecuteNonQuery();
                            return Json(new { success = true, message = "Fee receipt deleted successfully!" });
                        }
                        catch (SqlException ex)
                        {
                            // The stored procedure will raise an error if it's not the latest receipt
                            return Json(new { success = false, message = ex.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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

        // GET: CashBookReport
        public async Task<ActionResult> Index(DateTime? fromDate, DateTime? toDate, string paymentMode = "ALL", string selectedClass = "ALL", string selectedSection = "ALL")
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
            int schoolcode = int.Parse(CurrentSchoolCode);
            Guid SessionId = CurrentSessionID;

            // Set default values if parameters are not provided
            fromDate = fromDate ?? DateTime.Today;
            toDate = toDate ?? DateTime.Today;

            // Create view model
            var model = new CashBookReportViewModel
            {
                FromDate = fromDate.Value,
                ToDate = toDate.Value,
                SessionId = SessionId,
                TenantCode = schoolcode,
                PaymentMode = paymentMode,
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
            };

            // Get report data with payment mode filter
            var reportData = await GetReportDataForUI(fromDate.Value, toDate.Value, SessionId, schoolcode, classId, sectionId,paymentMode);
            model.FeeData = reportData.FeeData;
            model.UserSummary = reportData.UserSummary;
            model.PaymentTotals = reportData.PaymentTotals;
            model.SessionYear = reportData.SessionYear;
            model.ClassWiseSummary = reportData.ClassWiseSummary;
            // Get available payment modes for dropdown
            model.AvailablePaymentModes = await GetAvailablePaymentModes(schoolcode);

            return View(model);
        }
        private async Task<ReportData> GetReportDataForUI(DateTime fromDate, DateTime toDate, Guid sessionId, int tenantCode,Guid? classId, Guid? sectionId, string paymentMode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Parameters for the stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@FromDate", fromDate.Date);
                parameters.Add("@ToDate", toDate.Date);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@TenantCode", tenantCode);
                parameters.Add("@PaymentMode", paymentMode); // Add payment mode parameter
                parameters.Add("@classId", classId);
                parameters.Add("@sectionId", sectionId);

                using (var multi = await connection.QueryMultipleAsync("sp_GetCashBookReport", parameters, commandType: CommandType.StoredProcedure))
                {
                    var reportData = new ReportData
                    {
                        OpeningBalance = await multi.ReadFirstOrDefaultAsync<OpeningBalance>(),
                        FeeData = (await multi.ReadAsync<FeeReceiptItem>()).ToList(),
                        ClassWiseSummary = (await multi.ReadAsync<ClassWiseSummary>()).ToList(),
                        UserSummary = (await multi.ReadAsync<UserSummaryItem>()).ToList(),
                        PaymentTotals = await multi.ReadFirstOrDefaultAsync<PaymentTotals>()
                    };
                    return reportData;
                }
            }
        }


        private async Task<ReportData> GetCashBookReportData(DateTime fromDate, DateTime toDate, Guid sessionId, int tenantCode, Guid? classId, Guid? sectionId, string paymentMode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Parameters for the stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@FromDate", fromDate.Date);
                parameters.Add("@ToDate", toDate.Date);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@TenantCode", tenantCode);
                parameters.Add("@PaymentMode", paymentMode);
                parameters.Add("@classId", classId);
                parameters.Add("@sectionId", sectionId);

                using (var multi = await connection.QueryMultipleAsync("sp_GetCashBookReport", parameters, commandType: CommandType.StoredProcedure))
                {
                    var reportData = new ReportData
                    {
                        OpeningBalance = await multi.ReadFirstOrDefaultAsync<OpeningBalance>(),
                        FeeData = (await multi.ReadAsync<FeeReceiptItem>()).ToList(),
                        ClassWiseSummary = (await multi.ReadAsync<ClassWiseSummary>()).ToList(),
                        UserSummary = (await multi.ReadAsync<UserSummaryItem>()).ToList(),
                        PaymentTotals = await multi.ReadFirstOrDefaultAsync<PaymentTotals>()
                    };
                    return reportData;
                }
            }
        }
        public async Task<ReportData> GetReportDataOnly(DateTime fromDate, DateTime toDate, Guid sessionId, int tenantCode,Guid? classId, Guid? sectionId, string paymentMode = "ALL")
        {
            return await GetCashBookReportData(fromDate, toDate, sessionId, tenantCode ,classId, sectionId, paymentMode);
        }
        public async Task<ActionResult> GetReportData(DateTime fromDate, DateTime toDate, Guid sessionId, int tenantCode, Guid? classId, Guid? sectionId, string paymentMode = "ALL")
        {
            // Validate parameters
            paymentMode = string.IsNullOrWhiteSpace(paymentMode) ? "ALL" : paymentMode;
            int schoolcode = int.Parse(CurrentSchoolCode);
            Guid SessionId = CurrentSessionID;

            try
            {
                // Get the report data from database
                var reportData = await GetCashBookReportData(fromDate, toDate, SessionId, schoolcode, classId, sectionId, paymentMode);

                // Create date range display text
                string dateRangeText = $"From: {fromDate:dd/MM/yyyy} To: {toDate:dd/MM/yyyy}";

                // Set the report data
                var combinedData = new List<object>();
                if (reportData.FeeData != null && reportData.FeeData.Any())
                {
                    combinedData.AddRange(reportData.FeeData);
                }
               
                // Get tenant info for report parameters
                string code = CommonLogic.GetSessionValue(StringConstants.TenantCode);
                string sessionprint = "Session: ( " + CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint) + " )";
                string img = CommonLogic.GetSessionValue(StringConstants.LogoImg);
                string logoPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, img));

                // Create a unique ID for the report
                string reportId = "cashBookReport_" + DateTime.Now.Ticks;

                try
                {
                    // Create the Crystal Report
                    ReportDocument reportDocument = new ReportDocument();
                    reportDocument.Load(Server.MapPath("~/Reports/CrystalReport/0/cashbook.rpt"));
                    reportDocument.SetDataSource(combinedData);
                    reportDocument.Subreports["summary"].SetDataSource(reportData.UserSummary);
                    reportDocument.Subreports["classsummary"].SetDataSource(reportData.ClassWiseSummary);
                    // Set parameters
                    reportDocument.SetParameterValue("prmreportlogo", logoPath);
                    reportDocument.SetParameterValue("prmtitle", CommonLogic.GetSessionValue(StringConstants.PrintTitle));
                    reportDocument.SetParameterValue("prmline1", CommonLogic.GetSessionValue(StringConstants.Line1));
                    reportDocument.SetParameterValue("prmline2", CommonLogic.GetSessionValue(StringConstants.Line2));
                    reportDocument.SetParameterValue("prmline3", CommonLogic.GetSessionValue(StringConstants.Line3));
                    reportDocument.SetParameterValue("prmline4", CommonLogic.GetSessionValue(StringConstants.Line4));
                    reportDocument.SetParameterValue("prmreportname", "CASHBOOK");
                    reportDocument.SetParameterValue("prmreportitle1", sessionprint);
                    reportDocument.SetParameterValue("prmreportitle2", dateRangeText);
                    reportDocument.SetParameterValue("prmreportitle3", $"Payment Mode: {paymentMode}");
                    reportDocument.SetParameterValue("prmreportitle4", $"Opening Bal: {Convert.ToInt64(reportData.OpeningBalance.OpBalance)}");
                    // Store in session
                    Session[reportId] = reportDocument;

                    // Set ViewBag properties for the report viewer
                    ViewBag.ReportTitle = "CASHBOOK";
                    ViewBag.AllowPrint = true;
                    ViewBag.AllowExport = true;
                    ViewBag.ReportId = reportId;

                    // Return the report viewer view
                    return PartialView("_ReportViewer", reportId);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error generating report: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error retrieving report data: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        private async Task<List<PaymentModeItem>> GetAvailablePaymentModes(int schoolCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT DISTINCT 
                        PaymentMode
                    FROM 
                        dbo.FeeReceivedTbl
                    WHERE 
                        IsActive = 1 
                        AND IsDeleted = 0
                        AND TenantCode = @SchoolCode
                    ORDER BY 
                        PaymentMode";

                var paymentModes = await connection.QueryAsync<PaymentModeItem>(query, new { SchoolCode = schoolCode });
                return paymentModes.ToList();
            }
        }
    }
}
