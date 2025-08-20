using System;
using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models.School
{
    public class TenantModel
    {
        [Display(Name = "Tenant ID")]
        public Guid TenantID { get; set; } = Guid.NewGuid();

        [Display(Name = "Tenant Name")]
        [StringLength(500)]
        public string TenantName { get; set; }

        [Display(Name = "Address Line 1")]
        [StringLength(250)]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(250)]
        public string Address2 { get; set; }

        [Display(Name = "City")]
        [StringLength(50)]
        public string City { get; set; }

        [Display(Name = "State")]
        [StringLength(50)]
        public string State { get; set; }

        [Display(Name = "Zip Code")]
        [StringLength(20)]
        public string ZipCode { get; set; }

        [Display(Name = "Email")]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; }

        [Display(Name = "Fax")]
        [StringLength(20)]
        public string Fax { get; set; }

        [Display(Name = "Is Active")]
        public bool? IsActive { get; set; }

        [Display(Name = "Is Deleted")]
        public bool? IsDeleted { get; set; } = false;

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Modified Date")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "Created By")]
        public long? CreatedBy { get; set; }

        [Required]
        [Display(Name = "Tenant Code")]
        public int TenantCode { get; set; }

        [Display(Name = "Fee Serial Number")]
        public int? FeeSrNo { get; set; }

        [Display(Name = "TC Serial Number")]
        public int? TCSrNo { get; set; }

        [Display(Name = "CC Serial Number")]
        public int? CCSrNo { get; set; }

        [Display(Name = "Print Title")]
        [StringLength(200)]
        public string PrintTitle { get; set; }

        [Display(Name = "Line 1")]
        [StringLength(200)]
        public string Line1 { get; set; }

        [Display(Name = "Line 2")]
        [StringLength(200)]
        public string Line2 { get; set; }

        [Display(Name = "Line 3")]
        [StringLength(200)]
        public string Line3 { get; set; }

        [Display(Name = "Line 4")]
        [StringLength(200)]
        public string Line4 { get; set; }

        [Display(Name = "Fee Note 1")]
        [StringLength(100)]
        public string FeeNote1 { get; set; }

        [Display(Name = "Fee Note 2")]
        [StringLength(100)]
        public string FeeNote2 { get; set; }

        [Display(Name = "Fee Note 3")]
        [StringLength(100)]
        public string FeeNote3 { get; set; }

        [Display(Name = "Fee Note 4")]
        [StringLength(100)]
        public string FeeNote4 { get; set; }

        [Display(Name = "Fee Note 5")]
        [StringLength(100)]
        public string FeeNote5 { get; set; }

        [Display(Name = "Signature Image")]
        [StringLength(100)]
        public string SIGNImg { get; set; }

        [Display(Name = "Logo Image")]
        [StringLength(100)]
        public string LOGOImg { get; set; }

        [Display(Name = "ID Card Image")]
        [StringLength(100)]
        public string IdCardImg { get; set; }

        [Display(Name = "Manager Name")]
        [StringLength(100)]
        public string MgrName { get; set; }

        [Display(Name = "Disease Code")]
        [StringLength(100)]
        public string DiseCode { get; set; }

        [Display(Name = "Registration Number")]
        [StringLength(100)]
        public string RegNo { get; set; }

        [Display(Name = "Website")]
        [StringLength(100)]
        [Url]
        public string Website { get; set; }

        [Display(Name = "School Note 1")]
        [StringLength(100)]
        public string SchoolNote1 { get; set; }

        [Display(Name = "School Note 2")]
        [StringLength(100)]
        public string SchoolNote2 { get; set; }

        [Display(Name = "School Note 3")]
        [StringLength(100)]
        public string SchoolNote3 { get; set; }

        [Display(Name = "School Note 4")]
        [StringLength(100)]
        public string SchoolNote4 { get; set; }

        [Display(Name = "School Note 5")]
        [StringLength(100)]
        public string SchoolNote5 { get; set; }

        [Display(Name = "Manager Contact Number")]
        [StringLength(100)]
        [Phone]
        public string ManagerContactNo { get; set; }

        [Display(Name = "Header Image")]
        [StringLength(100)]
        public string HeaderImg { get; set; }
    }
}