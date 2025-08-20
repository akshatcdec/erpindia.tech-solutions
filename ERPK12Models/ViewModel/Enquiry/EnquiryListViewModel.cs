using ERPK12Models.DTO;
using System.Collections.Generic;

namespace ERPK12Models.ViewModel.Enquiry
{
    public class EnquiryListViewModel
    {
        public List<StudentEnquiry> Enquiries { get; set; }
        public EnquiryFilterViewModel Filters { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public EnquiryListViewModel()
        {
            Enquiries = new List<StudentEnquiry>();
            Filters = new EnquiryFilterViewModel();
            Pagination = new PaginationViewModel();
        }
    }
}
