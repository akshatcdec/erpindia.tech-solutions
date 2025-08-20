using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models
{
    public class InvoiceModel
    {
        public long InvoiceId { get; set; }

        public long CompanyId { get; set; }

        [Required]
        [Display(Name = "Client")]
        public long ClientId { get; set; }
        [Required]
        [DefaultValue(0)]
        [Display(Name = "Invoice Type")]
        public long TaxId { get; set; }
        public long FYearId { get; set; }

        public string ClientName { get; set; }
        [Required]
        [Display(Name = "Invoice Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? InvoiceDate { get; set; }
        [Required]
        [Display(Name = "Due Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? DueDate { get; set; }
        [Required]
        [Display(Name = "Active Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? ActiveDate { get; set; }
        [Required]
        [Display(Name = "Exp Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? ExpDate { get; set; }
        public decimal AMCAnnual { get; set; }
        public decimal AMCMonthly { get; set; }
        public decimal AMCQuarterly { get; set; }
        public decimal AMCHalfYearly { get; set; }
        public string Increments { get; set; }
        public string ActiveModules { get; set; }
        public string WebsiteName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SchoolCode { get; set; }
        public string SchoolYear { get; set; }


        [Required]
        [DefaultValue("0")]
        [Display(Name = "Invoice Status")]
        public string InvoiceStatus { get; set; }
        public decimal ServiceTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TaxPercent { get; set; }

        public decimal CESS { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal InvoiceTotal { get; set; }
        public string ReportStatusText
        {
            get
            {
                if (this.InvoiceStatus == "P")
                    return "Pending";
                else if (this.InvoiceStatus == "R")
                    return "Received";
                else if (this.InvoiceStatus == "C")
                    return "Cancel";

                return "Pending";
            }
        }
        public long CreatedBy { get; set; }
        public int TotalRecordCount { get; set; }

        public string DuplicateColumn { get; set; }
    }
    public class InvoiceDetailModel
    {
        public long InvoiceDetailId { get; set; }
        public long InvoiceId { get; set; }
        public long ServiceId { get; set; }


        public string ServiceName { get; set; }


        public string Amount { get; set; }

        public string Remarks { get; set; }
    }
    public class PatientTestDetailModel
    {
        public long PatientTestId { get; set; }
        public long LabTestId { get; set; }
        public string LabTestName { get; set; }
        public string Result { get; set; }
        public string Remarks { get; set; }
    }
}