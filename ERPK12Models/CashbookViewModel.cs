using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ERPK12Models
{

    
        public class CashbookViewModel
        {
            public string SchoolName { get; set; } = "DRONA PUBLIC SCHOOL";
            public string SchoolAddress { get; set; } = "Rampur Hathigaha Prayagraj";
            public string SchoolEmail { get; set; } = "drona.dps@gmail.com";
            public string SchoolPhone { get; set; } = "9415367642";
            public string Session { get; set; } = "2025-26";
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public string PaymentMode { get; set; } = "ALL";
            public decimal OpeningBalance { get; set; }
            public int PageNumber { get; set; }
            public int TotalPages { get; set; }
            public DateTime GeneratedOn { get; set; } = DateTime.Now;

            public List<CashbookEntry> Entries { get; set; } = new List<CashbookEntry>();
            public PaymentModeSummary PaymentSummary { get; set; } = new PaymentModeSummary();
            public List<ClassWiseSummary> ClassSummaries { get; set; } = new List<ClassWiseSummary>();
            public decimal TotalAmount { get; set; }
        }

        public class CashbookEntry
        {
            public int ReceiptNo { get; set; }
            public string StudentName { get; set; }
            public string FatherName { get; set; }
            public string Class { get; set; }
            public string Section { get; set; }
            public string PaymentMode { get; set; }
            public DateTime Date { get; set; }
            public string Notes { get; set; }
            public int UserId { get; set; }
            public decimal ReceivedAmount { get; set; }
        }

        public class PaymentModeSummary
        {
            public decimal Cash { get; set; }
            public decimal UPI { get; set; }
            public decimal Paytm { get; set; }
            public decimal Bank { get; set; }
            public decimal Cheque { get; set; }
            public decimal Other { get; set; }
            public decimal TotalAmount => Cash + UPI + Paytm + Bank + Cheque + Other;
        }

        public class ClassWiseSummary
        {
            public string Class { get; set; }
            public decimal Amount { get; set; }
        }

        // For MVC Controller
        public class CashbookFilterViewModel
        {
            [Display(Name = "From Date")]
            [DataType(DataType.Date)]
            public DateTime FromDate { get; set; } = DateTime.Now.AddMonths(-1);

            [Display(Name = "To Date")]
            [DataType(DataType.Date)]
            public DateTime ToDate { get; set; } = DateTime.Now;

            [Display(Name = "Payment Mode")]
            public string PaymentMode { get; set; } = "ALL";

            [Display(Name = "Class")]
            public string Class { get; set; } = "ALL";

            public List<string> AvailableClasses { get; set; } = new List<string>
        {
            "ALL", "Nursery", "Prep", "KG", "1st", "2nd", "3rd", "4th",
            "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th"
        };

            public List<string> AvailablePaymentModes { get; set; } = new List<string>
        {
            "ALL", "Cash", "UPI", "Paytm", "Bank", "Cheque"
        };
        }
   
}
