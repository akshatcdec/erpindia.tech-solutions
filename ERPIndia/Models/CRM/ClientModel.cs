using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models
{

    public class ClientModel
    {
        public ClientModel()
        {
            this.IsActive = true;
        }

        public long ClientId { get; set; }
        public long ParentId { get; set; }

        public long BranchId { get; set; }
        public long FYearId { get; set; }

        [Required]
        [Display(Name = "School Name")]
        public string ClientName { get; set; }
        [Display(Name = "Client Code")]
        public string ClientCode { get; set; }
        [Display(Name = "Password")]
        public string Password { get; set; }
        public string BranchName { get; set; }
        public string GSTNo { get; set; }
        public string PANNo { get; set; }

        [Required]
        public string Address1 { get; set; }
        public string GSTCategory { get; set; }
        public string Address2 { get; set; }

        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Display(Name = "Pin Code")]
        public string ZipCode { get; set; }
        [Required]
        [Display(Name = "Mobile")]
        public string Phone { get; set; }
        [Display(Name = "Software Link")]
        public string Fax { get; set; }
        [Required]
        [Display(Name = "Whatsapp Mobile")]
        public string Whatsapp { get; set; }

        [Display(Name = "Upload Document")]
        public string WebSite { get; set; }
        [Display(Name = "Principal Name")]
        public string PrincipalName { get; set; }
        [Display(Name = "Principal Number")]
        public string PrincipalNumber { get; set; }
        [Display(Name = "Logo 1")]
        public string PrincipalPhoto { get; set; }
        [Required]
        [Display(Name = "Manager Name")]
        public string ManagerName { get; set; }
        [Display(Name = "Manager Number")]
        public string ManagerNumber { get; set; }
        [Display(Name = "Logo 2")]
        public string ManagerPhoto { get; set; }
        [Display(Name = "School Website")]
        public string SchoolLogo { get; set; }
        public string FacebookAccountLink { get; set; }
        public string TwitterLink { get; set; }
        public string OtherLink { get; set; }
        public string SMSId { get; set; }
        public string SMSPassword { get; set; }
        [Display(Name = "Logo 2")]
        public string SMSLoginCode { get; set; }
        public decimal AMC { get; set; }
        public decimal StudentLimit { get; set; }

        public string LoginLink { get; set; }
        public string Username { get; set; }

        public string Sesssion { get; set; }

        public string AgentName { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string Note3 { get; set; }
        public string Note4 { get; set; }
        public long Increment { get; set; }
        public long CustomerId { get; set; }
        public long OldBalance { get; set; }

        [Required]
        [Display(Name = "Email Id")]
        public string Email { get; set; }
        [Required]
        public bool IsActive { get; set; }

        public long CreatedBy { get; set; }

        public int TotalRecordCount { get; set; }

        public string DuplicateColumn { get; set; }
    }
}