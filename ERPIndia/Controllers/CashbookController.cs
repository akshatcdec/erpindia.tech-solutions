using ERPIndia.Services;
using ERPK12Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class CashbookController : BaseController
    {
        // GET: Cashbook

        private readonly CashbookPdfService _pdfService;

        public CashbookController()
        {
            _pdfService = new CashbookPdfService();
        }

        // GET: Cashbook
        public ActionResult Index()
        {
            var filterModel = new CashbookFilterViewModel();
            return View(filterModel);
        }

        // POST: Generate PDF
        [HttpPost]
        public ActionResult GeneratePdf(CashbookFilterViewModel filter)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", filter);
            }

            try
            {
                // Get data based on filters (this would typically come from your database)
                var cashbookData = GetCashbookData(filter);

                // Generate PDF
                var pdfBytes = _pdfService.GenerateCashbookPdf(cashbookData);

                // Return PDF file
                var fileName = $"Cashbook_{filter.FromDate:yyyyMMdd}_to_{filter.ToDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while generating the PDF: " + ex.Message);
                return View("Index", filter);
            }
        }

        // This method should be replaced with actual database queries
        private CashbookViewModel GetCashbookData(CashbookFilterViewModel filter)
        {
            // Sample data - replace with actual database queries
            var entries = GetSampleEntries()
                .Where(e => e.Date >= filter.FromDate && e.Date <= filter.ToDate)
                .Where(e => filter.PaymentMode == "ALL" || e.PaymentMode.Equals(filter.PaymentMode, StringComparison.OrdinalIgnoreCase))
                .Where(e => filter.Class == "ALL" || e.Class.Equals(filter.Class, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Date)
                .ThenBy(e => e.ReceiptNo)
                .ToList();

            var totalAmount = entries.Sum(e => e.ReceivedAmount);

            var paymentSummary = new PaymentModeSummary
            {
                Cash = entries.Where(e => e.PaymentMode.Equals("cash", StringComparison.OrdinalIgnoreCase)).Sum(e => e.ReceivedAmount),
                UPI = entries.Where(e => e.PaymentMode.Equals("upi", StringComparison.OrdinalIgnoreCase)).Sum(e => e.ReceivedAmount),
                Paytm = entries.Where(e => e.PaymentMode.Equals("paytm", StringComparison.OrdinalIgnoreCase)).Sum(e => e.ReceivedAmount),
                Bank = entries.Where(e => e.PaymentMode.Equals("bank", StringComparison.OrdinalIgnoreCase)).Sum(e => e.ReceivedAmount),
                Cheque = entries.Where(e => e.PaymentMode.Equals("cheque", StringComparison.OrdinalIgnoreCase)).Sum(e => e.ReceivedAmount)
            };

            var classSummaries = entries
                .GroupBy(e => e.Class)
                .Select(g => new ClassWiseSummary
                {
                    Class = g.Key,
                    Amount = g.Sum(e => e.ReceivedAmount)
                })
                .OrderBy(c => c.Class)
                .ToList();

            return new CashbookViewModel
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                PaymentMode = filter.PaymentMode,
                OpeningBalance = 553865, // This should come from your system
                PageNumber = 1,
                TotalPages = 1, // Calculate based on entries per page
                Entries = entries,
                PaymentSummary = paymentSummary,
                ClassSummaries = classSummaries,
                TotalAmount = totalAmount
            };
        }

        private List<CashbookEntry> GetSampleEntries()
        {
            // Sample data based on the document provided
            // In a real application, this would come from your database
            return new List<CashbookEntry>
            {
                new CashbookEntry
                {
                    ReceiptNo = 121,
                    StudentName = "MD USMAN",
                    FatherName = "MD WASIM",
                    Class = "7th",
                    Section = "A",
                    PaymentMode = "cash",
                    Date = new DateTime(2025, 5, 1),
                    Notes = "discount given by manager sir",
                    UserId = 1,
                    ReceivedAmount = 10000
                },
                new CashbookEntry
                {
                    ReceiptNo = 122,
                    StudentName = "WAZIHA BANO",
                    FatherName = "MOHD. WASEEM",
                    Class = "5th",
                    Section = "A",
                    PaymentMode = "cash",
                    Date = new DateTime(2025, 5, 1),
                    Notes = "",
                    UserId = 1,
                    ReceivedAmount = 9000
                },
                new CashbookEntry
                {
                    ReceiptNo = 123,
                    StudentName = "YASH SINGH",
                    FatherName = "RANBAHADUR SINGH",
                    Class = "10th",
                    Section = "A",
                    PaymentMode = "cash",
                    Date = new DateTime(2025, 5, 1),
                    Notes = "",
                    UserId = 1,
                    ReceivedAmount = 6000
                },
                // Add more sample entries as needed...
                new CashbookEntry
                {
                    ReceiptNo = 129,
                    StudentName = "YASH",
                    FatherName = "RAM KUMAR",
                    Class = "2nd",
                    Section = "A",
                    PaymentMode = "upi",
                    Date = new DateTime(2025, 5, 2),
                    Notes = "",
                    UserId = 1,
                    ReceivedAmount = 3525
                },
                new CashbookEntry
                {
                    ReceiptNo = 130,
                    StudentName = "VISWASH KUMAR",
                    FatherName = "RAM KUMAR",
                    Class = "9th",
                    Section = "A",
                    PaymentMode = "upi",
                    Date = new DateTime(2025, 5, 2),
                    Notes = "",
                    UserId = 1,
                    ReceivedAmount = 6250
                }
            };
        }

        // AJAX method to get filtered data count
        [HttpPost]
        public JsonResult GetFilteredCount(CashbookFilterViewModel filter)
        {
            try
            {
                var entries = GetSampleEntries()
                    .Where(e => e.Date >= filter.FromDate && e.Date <= filter.ToDate)
                    .Where(e => filter.PaymentMode == "ALL" || e.PaymentMode.Equals(filter.PaymentMode, StringComparison.OrdinalIgnoreCase))
                    .Where(e => filter.Class == "ALL" || e.Class.Equals(filter.Class, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return Json(new
                {
                    success = true,
                    count = entries.Count,
                    totalAmount = entries.Sum(e => e.ReceivedAmount)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}