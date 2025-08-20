using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ERPK12Models.ViewModel.GatePass
{
    public class GatePassInfo
    {
        public Guid Id { get; set; }
        public string PassNo { get; set; }
        public DateTime Date { get; set; }
        public string StudentName { get; set; }
        public string Father { get; set; }
        public string Mother { get; set; }
        public string Class { get; set; }
        public Guid? ClassId { get; set; }
        public string Address { get; set; }

        // TIME columns → TimeSpan?
        public TimeSpan? TimeIn { get; set; }
        public TimeSpan? TimeOut { get; set; }

        public string ParentGuardianName { get; set; }
        public string GuardianMobile { get; set; }
        public string RelationshipToStudent { get; set; }
        public string ReasonForLeave { get; set; }
        public DateTime? PrintTime { get; set; }
        public int? PrintCount { get; set; }

        public Guid SessionId { get; set; }
        public string TenantCode { get; set; }   // ← match NVARCHAR(50) in table

        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // D1 - D40 Fields
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string D3 { get; set; }
        public string D4 { get; set; }
        public string D5 { get; set; }
        public string D6 { get; set; }
        public string D7 { get; set; }
        public string D8 { get; set; }
        public string D9 { get; set; }
        public string D10 { get; set; }
        public string D11 { get; set; }
        public string D12 { get; set; }
        public string D13 { get; set; }
        public string D14 { get; set; }
        public string D15 { get; set; }
        public string D16 { get; set; }
        public string D17 { get; set; }
        public string D18 { get; set; }
        public string D19 { get; set; }
        public string D20 { get; set; }
        public string D21 { get; set; }
        public string D22 { get; set; }
        public string D23 { get; set; }
        public string D24 { get; set; }
        public string D25 { get; set; }
        public string D26 { get; set; }
        public string D27 { get; set; }
        public string D28 { get; set; }
        public string D29 { get; set; }
        public string D30 { get; set; }
        public string D31 { get; set; }
        public string D32 { get; set; }
        public string D33 { get; set; }
        public string D34 { get; set; }
        public string D35 { get; set; }
        public string D36 { get; set; }
        public string D37 { get; set; }
        public string D38 { get; set; }
        public string D39 { get; set; }
        public string D40 { get; set; }
    }
    // Main Gate Pass View Model for Create/Edit
    public class GatePassViewModel
    {
        public GatePassRecord GatePass { get; set; }
        public List<SelectListItem> ClassList { get; set; }

        public GatePassViewModel()
        {
            GatePass = new GatePassRecord();
            ClassList = new List<SelectListItem>();
        }
    }

    // Gate Pass List View Model for Index page
    public class GatePassListViewModel
    {
        public List<GatePassRecord> GatePasses { get; set; }
        public GatePassFilterViewModel Filters { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public GatePassListViewModel()
        {
            GatePasses = new List<GatePassRecord>();
            Filters = new GatePassFilterViewModel();
            Pagination = new PaginationViewModel();
        }
    }

    // Filter View Model for searching gate passes
    public class GatePassFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SelectedClass { get; set; }
        public string StudentName { get; set; }
        public string PassNo { get; set; }
        public List<SelectListItem> ClassList { get; set; }

        public GatePassFilterViewModel()
        {
            ClassList = new List<SelectListItem>();
        }
    }

    // Gate Pass History View Model
    public class GatePassHistoryViewModel
    {
        public string StudentName { get; set; }
        public List<GatePassRecord> GatePasses { get; set; }
        public List<YearlyGatePassStats> YearlyStats { get; set; }

        public GatePassHistoryViewModel()
        {
            GatePasses = new List<GatePassRecord>();
            YearlyStats = new List<YearlyGatePassStats>();
        }
    }

    // Yearly statistics for gate passes
    public class YearlyGatePassStats
    {
        public int Year { get; set; }
        public int TotalPasses { get; set; }
    }

    // Gate Pass Record Model
    // Update your GatePassRecord class in GatePassViewModel.cs with these validation attributes

    public class GatePassRecord
    {
        public Guid Id { get; set; }

        [Display(Name = "Pass No.")]
        public string PassNo { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Student name is required")]
        [Display(Name = "Student Name")]
        [StringLength(100, ErrorMessage = "Student name cannot exceed 100 characters")]
        public string StudentName { get; set; }

        [Display(Name = "Father")]
        [StringLength(100)]
        public string Father { get; set; }

        [Display(Name = "Mother")]
        [StringLength(100)]
        public string Mother { get; set; }

        [Display(Name = "Class")]
        public string Class { get; set; }

        [Required(ErrorMessage = "Class selection is required")]
        [Display(Name = "Class")]
        public Guid? ClassId { get; set; }

        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Time In is required")]
        [Display(Name = "Time In")]
        [DataType(DataType.Time)]
        public TimeSpan? TimeIn { get; set; }

        [Required(ErrorMessage = "Time Out is required")]
        [Display(Name = "Time Out")]
        [DataType(DataType.Time)]
        public TimeSpan? TimeOut { get; set; }

        [Required(ErrorMessage = "Parent/Guardian name is required")]
        [Display(Name = "Parent/Guardian Name")]
        [StringLength(100, ErrorMessage = "Parent/Guardian name cannot exceed 100 characters")]
        public string ParentGuardianName { get; set; }

        [Required(ErrorMessage = "Guardian mobile is required")]
        [Display(Name = "Guardian Mobile")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be exactly 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be 10 digits")]
        public string GuardianMobile { get; set; }

        [Required(ErrorMessage = "Relationship to student is required")]
        [Display(Name = "Relationship to Student")]
        [StringLength(50, ErrorMessage = "Relationship cannot exceed 50 characters")]
        public string RelationshipToStudent { get; set; }

        [Required(ErrorMessage = "Reason for leave is required")]
        [Display(Name = "Reason for Leave")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string ReasonForLeave { get; set; }

        [StringLength(100)]
        public string ReasonFor { get; set; }

        [Display(Name = "Print Time")]
        public DateTime? PrintTime { get; set; }

        public int PrintCount { get; set; }

        [Display(Name = "Admission No.")]
        public int? AdmNo { get; set; }

        [Required(ErrorMessage = "Student selection is required")]
        public Guid? StudentId { get; set; }

        // Audit fields
        public Guid SessionId { get; set; }
        public string TenantCode { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string D3 { get; set; }
        public string D4 { get; set; }
        public string D5 { get; set; }
        public string D6 { get; set; }
        public string D7 { get; set; }
        public string D8 { get; set; }
        public string D9 { get; set; }
        public string D10 { get; set; }
        public string D11 { get; set; }
        public string D12 { get; set; }
        public string D13 { get; set; }
        public string D14 { get; set; }
        public string D15 { get; set; }
        public string D16 { get; set; }
        public string D17 { get; set; }
        public string D18 { get; set; }
        public string D19 { get; set; }
        public string D20 { get; set; }
        public string D21 { get; set; }
        public string D22 { get; set; }
        public string D23 { get; set; }
        public string D24 { get; set; }
        public string D25 { get; set; }
        public string D26 { get; set; }
        public string D27 { get; set; }
        public string D28 { get; set; }
        public string D29 { get; set; }
        public string D30 { get; set; }
        public string D31 { get; set; }
        public string D32 { get; set; }
        public string D33 { get; set; }
        public string D34 { get; set; }
        public string D35 { get; set; }
        public string D36 { get; set; }
        public string D37 { get; set; }
        public string D38 { get; set; }
        public string D39 { get; set; }
        public string D40 { get; set; }
    }

    // Pagination View Model (reused from Enquiry)
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    // Student search result for autocomplete
    public class StudentSearchResult
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        public string Student { get; set; }
        public string Father { get; set; }
        public string Mother { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public string Address { get; set; }
        public string Mobile1 { get; set; }
        public string Mobile2 { get; set; }
        public int? AdmNo { get; set; }
        public Guid? StudentId { get; set; } = Guid.Empty;
    }
}