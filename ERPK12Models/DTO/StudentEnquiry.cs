using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPK12Models.DTO
{
    public class RequiredGuidAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is Guid guid)
            {
                return guid != Guid.Empty;
            }
            return false;
        }
    }
    public class StudentEnquiry
    {
        public Guid Id { get; set; } // Changed from int to Guid
        [RequiredGuid(ErrorMessage = "Class is required")]
        [Display(Name = "Applying For Class")]
        public Guid ClassId { get; set; }
        public int EnqNo { get; set; } // Missing property

        [Required(ErrorMessage = "Student name is required")]
        [Display(Name = "Student Name")]
        public string Student { get; set; }

        [Required(ErrorMessage = "Father's name is required")]
        [Display(Name = "Father's Name")]
        public string Father { get; set; }

        [Display(Name = "Mother's Name")]
        public string Mother { get; set; }

       
        public string ApplyingForClass { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [Display(Name = "Mobile 1")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Please enter a valid 10-digit mobile number")]
        public string Mobile1 { get; set; }

        [Display(Name = "Mobile 2")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Please enter a valid 10-digit mobile number")]
        public string Mobile2 { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Display(Name = "Previous School")]
        public string PreviousSchool { get; set; }

        public string Relation { get; set; }
        [Required(ErrorMessage = "No. of Children is required")]
        [Display(Name = "No. of Children")]
        public int? NoOfChild { get; set; }

        [Required(ErrorMessage = "Enquiry date is required")]
        [Display(Name = "Enquiry Date")]
        [DataType(DataType.Date)]
        public DateTime EnquiryDate { get; set; }

        [Display(Name = "Deal By")]
        public string DealBy { get; set; }

        public string Source { get; set; }

        [Display(Name = "Send SMS")]
        public string SendSMS { get; set; }

        [Display(Name = "Form Amount")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FormAmt { get; set; }

        [Display(Name = "Receipt Date")]
        [DataType(DataType.Date)]
        public DateTime? RcptDate { get; set; }

        public string Note { get; set; }

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "unpaid"; // Default value

        [Display(Name = "Interest Level")]
        public string InterestLevel { get; set; } = "Pending"; // Default value

        [Display(Name = "Next Follow Up")]
        [DataType(DataType.Date)]
        public DateTime? NextFollowup { get; set; }

        // Audit fields - Missing from original model
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        public int? SessionYear { get; set; }
        public Guid? SessionID { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public Guid? TenantID { get; set; }
        public int? TenantCode { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        public virtual ICollection<EnquiryFollowUp> FollowUps { get; set; } = new List<EnquiryFollowUp>();
    }
}
