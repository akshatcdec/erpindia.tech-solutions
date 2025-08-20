using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class ReportViewModel
    {
        // Type of report (e.g., "StudentFeeLedger", "StaffSalary", etc.)
        public string ReportType { get; set; }

        // File path to the report template
        public string ReportPath { get; set; }

        // Data to be displayed in the report
        public object ReportData { get; set; }

        // Dictionary of parameters to pass to the report
        public Dictionary<string, object> Parameters { get; set; }

        // Query parameters for filtering report data
        public Dictionary<string, string> QueryParameters { get; set; }

        // Constructor
        public ReportViewModel()
        {
            Parameters = new Dictionary<string, object>();
            QueryParameters = new Dictionary<string, string>();
        }
    }
    public class ReportController : Controller
    {
        // Generic action to display any report
        public ActionResult DisplayReport(ReportViewModel model, string format = "pdf")
        {
            // Pass the format to the view via ViewBag
            ViewBag.ExportFormat = format;
            return View(model);
        }

        // Action to render a specific report
        public ActionResult RenderReport(string reportType, object reportData)
        {
            // Determine the report path based on the report type
            string reportPath = GetReportPath(reportType);

            // Create a new report model
            var model = new ReportViewModel
            {
                ReportType = reportType,
                ReportPath = reportPath,
                ReportData = reportData
            };

            return RedirectToAction("DisplayReport", model);
        }

        // Action to stream the report in the requested format
        public ActionResult StreamReport(string id, string format = "pdf", bool download = false)
        {
            try
            {
                // Retrieve the report document from session
                ReportDocument reportDocument = Session[id] as ReportDocument;

                if (reportDocument == null)
                {
                    return Content("Report not found. It may have expired.");
                }

                Stream stream;
                string contentType;
                string fileExtension;

                // Export based on requested format
                switch (format.ToLower())
                {
                    case "excel":
                        stream = reportDocument.ExportToStream(ExportFormatType.Excel);
                        contentType = "application/vnd.ms-excel";
                        fileExtension = ".xls";
                        download = true; // Always download Excel
                        break;

                    case "excelrecord":
                        stream = reportDocument.ExportToStream(ExportFormatType.ExcelRecord);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileExtension = ".xlsx";
                        download = true; // Always download Excel
                        break;

                    case "word":
                        stream = reportDocument.ExportToStream(ExportFormatType.WordForWindows);
                        contentType = "application/msword";
                        fileExtension = ".doc";
                        download = true; // Always download Word
                        break;

                    case "csv":
                        stream = reportDocument.ExportToStream(ExportFormatType.CharacterSeparatedValues);
                        contentType = "text/csv";
                        fileExtension = ".csv";
                        download = true; // Always download CSV
                        break;

                    case "pdf":
                    default:
                        stream = reportDocument.ExportToStream(ExportFormatType.PortableDocFormat);
                        contentType = "application/pdf";
                        fileExtension = ".pdf";
                        // PDF can be displayed inline or downloaded based on download parameter
                        break;
                }

                // Generate filename based on report ID and format
                string reportName = id.Split('_')[0]; // Extract report name from report ID
                string fileName = $"{reportName}_{DateTime.Now:yyyyMMdd}{fileExtension}";

                // Return the file - using Content-Disposition: inline for PDFs to show in browser
                // and Content-Disposition: attachment for other formats to download
                if (download)
                {
                    return File(stream, contentType, fileName); // Will download
                }
                else
                {
                    return File(stream, contentType); // Will display inline in browser
                }
            }
            catch (Exception ex)
            {
                return Content("Error streaming report: " + ex.Message);
            }
        }

        // Action to clean up report resources
        public ActionResult CleanupReport(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id) && Session[id] != null)
                {
                    // Get the report document from session
                    ReportDocument reportDocument = Session[id] as ReportDocument;
                    if (reportDocument != null)
                    {
                        // Close and dispose the report
                        reportDocument.Close();
                        reportDocument.Dispose();
                    }

                    // Remove from session
                    Session.Remove(id);

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Report not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
      
        // Helper method to export report to bytes
        private byte[] exportReport(ReportDocument reportDocument, ExportFormatType exportFormat)
        {
            using (var stream = new MemoryStream())
            {
                reportDocument.ExportToStream(exportFormat).CopyTo(stream);
                return stream.ToArray();
            }
        }
        // Helper method to get the report path based on report type
        private string GetReportPath(string reportType)
        {
            // Map report types to their file paths
            switch (reportType.ToLower())
            {
                case "studentfeeledger":
                    return Server.MapPath("~/Reports/CrystalReport/0/studentfeeledger.rpt");
                case "studentattendance":
                    return Server.MapPath("~/Reports/CrystalReport/0/studentattendance.rpt");
                case "staffsalary":
                    return Server.MapPath("~/Reports/CrystalReport/0/staffsalary.rpt");
                // Add more report mappings as needed
                default:
                    throw new ArgumentException("Invalid report type");
            }
        }
    }

}