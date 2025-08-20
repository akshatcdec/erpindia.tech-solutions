using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ERPK12Models.DTO
{
    public class EnquiryFollowUp
    {
        public Guid Id { get; set; }
        public Guid EnquiryId { get; set; }
        public int SrNo { get; set; }
        

        [Required]
        [Display(Name = "Follow Date")]
        [DataType(DataType.Date)]
        public DateTime FollowDate { get; set; }

        [Required]
        [Display(Name = "Follow Time")]
        [DataType(DataType.Time)]
        public TimeSpan FollowTime { get; set; }

        [Required]
        [Display(Name = "Call Status")]
        public string CallStatus { get; set; }

        [Display(Name = "Interest Level")]
        public string InterestLevel { get; set; }

        [Required]
        [Display(Name = "Response")]
        public string Response { get; set; }

        [Display(Name = "Next Follow Date")]
        [DataType(DataType.Date)]
        public DateTime? NextFollowDate { get; set; }

        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid? SessionID { get; set; }
        public Guid? TenantID { get; set; }
        public int? TenantCode { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation property
        public virtual StudentEnquiry Enquiry { get; set; }
    }
}
