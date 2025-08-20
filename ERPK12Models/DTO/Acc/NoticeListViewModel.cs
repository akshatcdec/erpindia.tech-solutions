using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
namespace ERPK12Models.DTO.Acc
{

    public class NoticeViewModel
    {
        public int NoticeId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Notice Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Notice type is required")]
        [Display(Name = "Notice Type")]
        public string NoticeType { get; set; }

        [Display(Name = "Icon Class")]
        public string IconClass { get; set; }

        [Display(Name = "Published By")]
        public string PublishedBy { get; set; }

        [Display(Name = "Published Date")]
        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        // Computed property for display
        public int DaysAgo => (DateTime.Now - PublishedDate).Days;

        public string StatusBadge => IsActive ? "Active" : "Inactive";

        public string TypeBadgeClass
        {
            get
            {
                switch (NoticeType)
                {
                    case "Instruction":
                        return "badge-primary";
                    case "Event":
                        return "badge-success";
                    case "Notification":
                        return "badge-warning";
                    case "Preparation":
                        return "badge-info";
                    case "Schedule":
                        return "badge-secondary";
                    default:
                        return "badge-light";
                }
            }
        }

    }

    public class NoticeListViewModel
    {
        public List<NoticeViewModel> Notices { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
        public string FilterType { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}

