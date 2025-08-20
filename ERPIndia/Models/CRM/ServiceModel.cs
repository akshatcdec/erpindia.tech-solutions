using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models
{
    public class ServiceModel
    {
        public ServiceModel()
        {
            this.IsActive = true;
        }
        public long ServiceId { get; set; }
        public long FYearId { get; set; }

        [Required]
        [Display(Name = "Service")]
        public string ServiceName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        public long CompanyId { get; set; }

        public long CreatedBy { get; set; }
        public int TotalRecordCount { get; set; }

        public string DuplicateColumn { get; set; }

    }
}