using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERPIndia.DTOs.Cashbook
{
    public class PaymentModeItem
    {
        public string PaymentMode { get; set; }
    }
    public class OpeningBalance
    {
        public decimal OpBalance { get; set; }
        public string Description { get; set; }
    }

    public class ClassWiseSummary
    {
        public Guid ClassID { get; set; }
        public string ClassName { get; set; }
        public int TotalReceipts { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
    }
    public class ReportData
    {
        public OpeningBalance OpeningBalance { get; set; }  // NEW
        public List<ClassWiseSummary> ClassWiseSummary { get; set; } = new List<ClassWiseSummary>();
        public List<FeeReceiptItem> FeeData { get; set; } = new List<FeeReceiptItem>();
        public List<UserSummaryItem> UserSummary { get; set; } = new List<UserSummaryItem>();
        public PaymentTotals PaymentTotals { get; set; } = new PaymentTotals();
        public int? SessionYear { get; set; }
    }

    public class FeeReceiptItem
    {
        public int ReceiptNo { get; set; }
        public Guid ReceiptId { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public long TransportCharge { get; set; }
        public string Note { get; set; }
        public decimal FeeReceived { get; set; }
        public string EntryDate { get; set; }
        public DateTime EntryTime { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string D1 { get; set; }
        public decimal Received { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string PaymentMode { get; set; }
        public FeeReceiptItem()
        {
            TransportCharge = 0;
        }
    }

    public class UserSummaryItem
    {
        public string UserName { get; set; }
        public decimal CASH { get; set; }
        public decimal UPI { get; set; }
        public decimal PAYTM { get; set; }
        public decimal BANK { get; set; }
        public decimal CHEQUE { get; set; }
        public decimal OTHER { get; set; }
        public decimal TOTAL_COLLECTION { get; set; }
    }

    public class PaymentTotals
    {
        public decimal CASH { get; set; }
        public decimal UPI { get; set; }
        public decimal PAYTM { get; set; }
        public decimal BANK { get; set; }
        public decimal CHEQUE { get; set; }
        public decimal OTHER { get; set; }
        public decimal TOTAL { get; set; }
    }

    public class SessionItem
    {
        public Guid SessionID { get; set; }
        public int SessionYear { get; set; }
        public string DisplayName => $"{SessionYear}-{(SessionYear + 1).ToString().Substring(2, 2)}";
    }

}