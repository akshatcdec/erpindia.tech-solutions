using ERPK12Models.DTO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ERPK12Models.ViewModel.Enquiry
{
    public class EnquiryViewModel
    {
        public StudentEnquiry Enquiry { get; set; }

        [Display(Name = "Class")]
        public string SelectedClass { get; set; }
        public List<SelectListItem> ClassList { get; set; }
        public List<EnquiryFollowUp> FollowUps { get; set; }

        public EnquiryViewModel()
        {
            Enquiry = new StudentEnquiry();
            FollowUps = new List<EnquiryFollowUp>();
            ClassList = new List<SelectListItem>();
        }
    }
}
