using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models.SystemSettings
{
    public class SchoolModel
    {
        public SchoolModel()
        {
            this.IsActive = true; // Assuming default active status as true
        }

        public long SchoolId { get; set; }

        [Required]
        [Display(Name = "School Name")]
        public string SchoolName { get; set; }

        [Required]
        public string Address1 { get; set; }
        public string Address2 { get; set; }


        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string Phone { get; set; }
        public string Fax { get; set; }


        public string Email { get; set; }
        public string PrintTitle { get; set; }
        public string ActiveHeaderImg { get; set; }
        public string ActiveSessionPrint { get; set; }
        
        public string LogoImg { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string ReceiptBannerImg { get; set; }
        public string AdmitCardBannerImg { get; set; }
        public string ReportCardBannerImg { get; set; }
        public string TransferCertBannerImg { get; set; }
        public string SalarySlipBannerImg { get; set; }
        public string ICardNameBannerImg { get; set; }
        public string ICardAddressBannerImg { get; set; }
        public string PrincipalSignImg { get; set; }
        public string ReceiptSignImg { get; set; }
        public char IsSingleFee { get; set; }
        public char EnableOnlineFee { get; set; }
        public string TopBarName { get; set; }
        public string TopBarAddress { get; set; }
        public bool IsActive { get; set; }

        [Required]
        [Display(Name = "School Code")]
        public string SchoolCode { get; set; }

        [Required]
        [Display(Name = "Subdomain")]
        public string Subdomain { get; set; }

        public long CreatedBy { get; set; }

        public int TotalRecordCount { get; set; }

        public string DuplicateColumn { get; set; }
    }

}