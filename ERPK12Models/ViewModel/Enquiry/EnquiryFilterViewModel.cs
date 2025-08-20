using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Mvc;

namespace ERPK12Models.ViewModel.Enquiry
{
    public class EnquiryFilterViewModel
    {
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Class")]
        public string SelectedClass { get; set; }
        public EnquiryFilterViewModel()
        {
            ClassList = new List<SelectListItem>();
        }
        public List<SelectListItem> ClassList { get; set; }

        [Display(Name = "Call Status")]
        public string CallStatus { get; set; }

        [Display(Name = "Interest Level")]
        public string InterestLevel { get; set; }

        [Display(Name = "Follow Up Date")]
        [DataType(DataType.Date)]
        public DateTime? FollowupDate { get; set; }

        [Display(Name = "Next Follow Up")]
        [DataType(DataType.Date)]
        public DateTime? NextFollowup { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
