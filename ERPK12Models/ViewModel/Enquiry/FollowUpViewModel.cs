using ERPK12Models.DTO;
using System;
using System.Collections.Generic;

namespace ERPK12Models.ViewModel.Enquiry
{
    public class FollowUpViewModel
    {
        public Guid EnquiryId { get; set; } // Changed from int to Guid
        public StudentEnquiry StudentInfo { get; set; }
        public EnquiryFollowUp NewFollowUp { get; set; }
        public List<EnquiryFollowUp> PreviousFollowUps { get; set; }

        public FollowUpViewModel()
        {
            StudentInfo = new StudentEnquiry();
            NewFollowUp = new EnquiryFollowUp();
            PreviousFollowUps = new List<EnquiryFollowUp>();
        }
    }
}
