using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.DTOs.Cashbook
{
    // Model classes
    public class CashBookReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid SessionId { get; set; }
        public string SelectedClass { get; set; }
        public List<SelectListItem> ClassList { get; set; }

        /// <summary>
        /// Section dropdown items
        /// </summary>
        public List<SelectListItem> SectionList { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? SectionId { get; set; }
        /// <summary>
        /// Selected Section ID
        /// </summary>
        public string SelectedSection { get; set; }
        public int TenantCode { get; set; }
        public string PaymentMode { get; set; } = "ALL"; // Default value
        public List<PaymentModeItem> AvailablePaymentModes { get; set; } = new List<PaymentModeItem>();
        public List<FeeReceiptItem> FeeData { get; set; } = new List<FeeReceiptItem>();
        public List<UserSummaryItem> UserSummary { get; set; } = new List<UserSummaryItem>();
        public List<ClassWiseSummary> ClassWiseSummary { get; set; } = new List<ClassWiseSummary>();
        
        public PaymentTotals PaymentTotals { get; set; } = new PaymentTotals();
        public int? SessionYear { get; set; }
        public CashBookReportViewModel()
        {

            ClassList = new List<SelectListItem>();
            SectionList = new List<SelectListItem>();
        }
        public string DisplayDate
        {
            get
            {
                if (FromDate.Date == ToDate.Date)
                {
                    return FromDate.ToString("dd-MM-yyyy");
                }
                else
                {
                    return $"{FromDate.ToString("dd-MM-yyyy")} to {ToDate.ToString("dd-MM-yyyy")}";
                }
            }
        }

        public string SessionDisplay
        {
            get
            {
                if (SessionYear.HasValue)
                {
                    return $"{SessionYear}-{(SessionYear + 1).ToString().Substring(2, 2)}";
                }
                return string.Empty;
            }
        }
    }

}