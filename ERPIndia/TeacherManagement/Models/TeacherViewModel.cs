// TeacherViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ERPIndia.TeacherManagement.Models
{
    public class TeacherViewModel
    {
        public TeacherViewModel()
        {
            Basic = new TeacherBasic();
            Payroll = new TeacherPayroll();
            Leaves = new TeacherLeaves();
            BankDetails = new TeacherBankDetails();
            SocialMedia = new TeacherSocialMedia();
            Documents = new List<TeacherDocument>();
        }

        public TeacherBasic Basic { get; set; }
        public TeacherPayroll Payroll { get; set; }
        public TeacherLeaves Leaves { get; set; }
        public TeacherBankDetails BankDetails { get; set; }
        public TeacherSocialMedia SocialMedia { get; set; }
        public IEnumerable<TeacherDocument> Documents { get; set; }
    }

    public class TeacherBasic
    {
        public Guid TeacherId { get; set; }

        [Required]
        [Display(Name = "Teacher Code")]
        public string TeacherCode { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        // HR Organization Fields
        [Display(Name = "Designation")]
        public Guid DesignationId { get; set; }

        [Display(Name = "Department")]
        public Guid DepartmentId { get; set; }

        [Display(Name = "Employee Type")]
        public Guid EmployeeTypeId { get; set; }

        [Display(Name = "Branch")]
        public Guid BranchId { get; set; }

        [Display(Name = "Manager")]
        public Guid? ManagerId { get; set; }

        // Denormalized Name Fields (for display/reporting)
        public string DesignationName { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeTypeName { get; set; }
        public string BranchName { get; set; }
        public string ManagerName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SubjectName { get; set; }
        public string RouteName { get; set; }
        public string VehicleName { get; set; }
        public string PickupName { get; set; }
        public string HostelName { get; set; }

        // Academic Information
        [Display(Name = "Class")]
        public Guid? ClassId { get; set; }

        [Display(Name = "Section")]
        public Guid? SectionId { get; set; }

        [Display(Name = "Subject")]
        public Guid? SubjectId { get; set; }

        [Display(Name = "Other Subject")]
        public string OtherSubject { get; set; }

        // Personal Information
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Religion")]
        public string Religion { get; set; }

        [Display(Name = "Primary Contact Number")]
        [Phone]
        public string PrimaryContactNumber { get; set; }

        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }

        [Display(Name = "Date of Joining")]
        [DataType(DataType.Date)]
        public DateTime? DateOfJoining { get; set; }

        [Display(Name = "Father's Name")]
        public string FatherName { get; set; }

        [Display(Name = "Mother's Name")]
        public string MotherName { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Marital Status")]
        public string MaritalStatus { get; set; }

        [Display(Name = "Languages Known")]
        public string LanguagesKnown { get; set; }

        // Professional Information
        [Display(Name = "Qualification")]
        public string Qualification { get; set; }

        [Display(Name = "Work Experience")]
        public string WorkExperience { get; set; }

        [Display(Name = "Experience Details")]
        public string ExperienceDetails { get; set; }

        [Display(Name = "Time In")]
        [DataType(DataType.Time)]
        public string TimeIn { get; set; }

        [Display(Name = "Time Out")]
        [DataType(DataType.Time)]
        public string TimeOut { get; set; }

        [Display(Name = "Previous School")]
        public string PreviousSchool { get; set; }

        [Display(Name = "Previous School Address")]
        public string PreviousSchoolAddress { get; set; }

        [Display(Name = "Previous School Phone")]
        public string PreviousSchoolPhone { get; set; }

        // Address Information
        [Display(Name = "Current Address")]
        public string CurrentAddress { get; set; }

        [Display(Name = "Permanent Address")]
        public string PermanentAddress { get; set; }

        // Government IDs
        [Display(Name = "PAN Number")]
        public string PANNumber { get; set; }

        [Display(Name = "Aadhar Number")]
        public string AadharNumber { get; set; }

        [Display(Name = "UAN Number")]
        public string UANNo { get; set; }

        [Display(Name = "NPS Number")]
        public string NPSNo { get; set; }

        [Display(Name = "PF Number")]
        public string PFNO { get; set; }

        // Status and Notes
        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        public string Photo { get; set; }

        // Login Credentials
        public string LoginId { get; set; }
        public string Password { get; set; }

        // Transport Information
        [Display(Name = "Route")]
        public Guid RouteId { get; set; }

        [Display(Name = "Vehicle")]
        public Guid VehicleId { get; set; }

        [Display(Name = "Pickup Point")]
        public Guid PickupId { get; set; }

        // Hostel Information
        [Display(Name = "Hostel")]
        public Guid HostelId { get; set; }

        [Display(Name = "Room No")]
        public string RoomNo { get; set; }

        // System Fields
        public int SchoolCode { get; set; }
        public Guid TenantId { get; set; }
        public int TenantCode { get; set; }
        public Guid SessionId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Dropdown Lists
        public IEnumerable<SelectListItem> ClassList { get; set; }
        public IEnumerable<SelectListItem> SectionList { get; set; }
        public IEnumerable<SelectListItem> SubjectList { get; set; }
        public IEnumerable<SelectListItem> DesignationList { get; set; }
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
        public IEnumerable<SelectListItem> EmployeeTypeList { get; set; }
        public IEnumerable<SelectListItem> BranchList { get; set; }
        public IEnumerable<SelectListItem> ManagerList { get; set; }
        public IEnumerable<SelectListItem> GenderList { get; set; }
        public IEnumerable<SelectListItem> BloodGroupList { get; set; }
        public IEnumerable<SelectListItem> MaritalStatusList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        public IEnumerable<SelectListItem> RouteList { get; set; }
        public IEnumerable<SelectListItem> VehicleList { get; set; }
        public IEnumerable<SelectListItem> PickupList { get; set; }
        public IEnumerable<SelectListItem> HostelList { get; set; }
    }

    public class TeacherPayroll
    {
        public Guid PayrollId { get; set; }
        public Guid TeacherId { get; set; }

        [Display(Name = "EPF Number")]
        public string EPFNo { get; set; }

        [Required]
        [Display(Name = "Basic Salary")]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid salary amount")]
        public decimal BasicSalary { get; set; }

        [Display(Name = "Contract Type")]
        public string ContractType { get; set; }

        [Display(Name = "Work Shift")]
        public string WorkShift { get; set; }

        [Display(Name = "Work Location")]
        public string WorkLocation { get; set; }

        [Display(Name = "Date of Leaving")]
        [DataType(DataType.Date)]
        public DateTime? DateOfLeaving { get; set; }

        [Display(Name = "Late Fine Per Hour")]
        public decimal LateFinePerHour { get; set; }

        [Display(Name = "Payroll Note")]
        public string PayrollNote { get; set; }

        [Display(Name = "Effective Date")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // System Fields
        public int SchoolCode { get; set; }
        public Guid TenantId { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Dropdown Lists
        public IEnumerable<SelectListItem> ContractTypeList { get; set; }
        public IEnumerable<SelectListItem> WorkShiftList { get; set; }
    }

    public class TeacherLeaves
    {
        public Guid LeaveId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid SessionId { get; set; }

        [Display(Name = "Medical Leaves")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid number")]
        public int MedicalLeaves { get; set; }

        [Display(Name = "Casual Leaves")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid number")]
        public int CasualLeaves { get; set; }

        [Display(Name = "Maternity Leaves")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid number")]
        public int MaternityLeaves { get; set; }

        [Display(Name = "Sick Leaves")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid number")]
        public int SickLeaves { get; set; }

        [Display(Name = "Earned Leaves")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid number")]
        public int EarnedLeaves { get; set; }

        // System Fields
        public int SchoolCode { get; set; }
        public Guid TenantId { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class TeacherBankDetails
    {
        public Guid BankId { get; set; }
        public Guid TeacherId { get; set; }

        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

       [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

       [Display(Name = "Bank Name")]
        public string BankName { get; set; }

       [Display(Name = "IFSC Code")]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC Code")]
        public string IFSCCode { get; set; }

        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }

        [Display(Name = "UPI ID")]
        public string UPIID { get; set; }

        // System Fields
        public int SchoolCode { get; set; }
        public Guid TenantId { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class TeacherSocialMedia
    {
        public Guid SocialMediaId { get; set; }
        public Guid TeacherId { get; set; }

        [Display(Name = "Facebook")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Facebook { get; set; }

        [Display(Name = "Instagram")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Instagram { get; set; }

        [Display(Name = "LinkedIn")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string LinkedIn { get; set; }

        [Display(Name = "YouTube")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string YouTube { get; set; }

        [Display(Name = "Twitter")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Twitter { get; set; }

        // System Fields
        public int SchoolCode { get; set; }
        public Guid TenantId { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class TeacherDocument
    {
        public Guid DocumentId { get; set; }
        public Guid TeacherId { get; set; }

        [Required]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        [Required]
        [Display(Name = "Document Title")]
        public string DocumentTitle { get; set; }

        [Required]
        [Display(Name = "Document Path")]
        public string DocumentPath { get; set; }

        [Display(Name = "File Size")]
        public long? FileSize { get; set; }

        [Display(Name = "MIME Type")]
        public string MimeType { get; set; }

        [Display(Name = "Upload Date")]
        public DateTime UploadDate { get; set; }

        // System Fields
        public int SchoolCode { get; set; }
        public Guid TenantId { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    // Supporting Enums
    public static class TeacherConstants
    {
        public static class DocumentTypes
        {
            public const string Resume = "Resume";
            public const string JoiningLetter = "JoiningLetter";
            public const string ExperienceCertificate = "ExperienceCertificate";
            public const string EducationCertificate = "EducationCertificate";
            public const string IdentityProof = "IdentityProof";
            public const string AddressProof = "AddressProof";
            public const string Other = "Other";
        }

        public static class ContractTypes
        {
            public const string Permanent = "Permanent";
            public const string Contract = "Contract";
            public const string Temporary = "Temporary";
            public const string Probation = "Probation";
        }

        public static class WorkShifts
        {
            public const string Morning = "Morning";
            public const string Evening = "Evening";
            public const string Night = "Night";
            public const string Flexible = "Flexible";
        }

        public static class Status
        {
            public const string Active = "Active";
            public const string OnLeave = "OnLeave";
            public const string Resigned = "Resigned";
            public const string Terminated = "Terminated";
            public const string Retired = "Retired";
        }
    }
}