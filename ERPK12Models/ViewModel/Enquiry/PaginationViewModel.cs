using System;
using System.Collections.Generic;

namespace ERPK12Models.ViewModel.Enquiry
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages
        {
            get
            {
                if (PageSize <= 0) return 0;
                return (int)Math.Ceiling((double)TotalRecords / PageSize);
            }
            set { } // Allow setting for backwards compatibility
        }

        // Properties for backwards compatibility with existing views
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public int StartRecord => TotalRecords == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
        public int EndRecord => Math.Min(CurrentPage * PageSize, TotalRecords);

        // Additional common pagination properties
        public int PreviousPage => HasPrevious ? CurrentPage - 1 : CurrentPage;
        public int NextPage => HasNext ? CurrentPage + 1 : CurrentPage;

        public int FirstPage => 1;
        public int LastPage => TotalPages;

        // Helper method to get page numbers for pagination display
        public List<int> GetPageNumbers(int maxPagesToShow = 5)
        {
            var pages = new List<int>();

            if (TotalPages <= maxPagesToShow)
            {
                // Show all pages
                for (int i = 1; i <= TotalPages; i++)
                {
                    pages.Add(i);
                }
            }
            else
            {
                // Calculate start and end page numbers
                int start = Math.Max(1, CurrentPage - (maxPagesToShow / 2));
                int end = Math.Min(TotalPages, start + maxPagesToShow - 1);

                // Adjust start if we're near the end
                if (end == TotalPages && end - start < maxPagesToShow - 1)
                {
                    start = Math.Max(1, end - maxPagesToShow + 1);
                }

                for (int i = start; i <= end; i++)
                {
                    pages.Add(i);
                }
            }

            return pages;
        }

        // Display text for current page info
        public string PageInfoText
        {
            get
            {
                if (TotalRecords == 0)
                    return "No records found";

                return $"Showing {StartRecord} to {EndRecord} of {TotalRecords} entries";
            }
        }
    }
}
