using System;
using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models
{
    public class ReportModel
    {
        public long ReportId { get; set; }

        [Required]
        [Display(Name = "Report Type")]
        public long ReportTypeId { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public long PatientId { get; set; }
        public long CompanyId { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        [Display(Name = "Report Date")]
        public DateTime? ReportDate { get; set; }
        public long CreatedBy { get; set; }

        public string PatientName { get; set; }

        [Display(Name = "Report File")]
        public string ReportFile { get; set; }
        public int TotalRecordCount { get; set; }

        public string DuplicateColumn { get; set; }

    }
}