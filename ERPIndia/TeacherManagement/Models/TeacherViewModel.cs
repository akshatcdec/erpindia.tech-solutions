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
            Documents = new TeacherDocuments();
        }

        public TeacherBasic Basic { get; set; }
        public TeacherPayroll Payroll { get; set; }
        public TeacherLeaves Leaves { get; set; }
        public TeacherBankDetails BankDetails { get; set; }
        public TeacherSocialMedia SocialMedia { get; set; }
        public TeacherDocuments Documents { get; set; }
    }

    public class TeacherBasic
    {
        public Guid TeacherId { get; set; }

        [Required]
        [Display(Name = "Teacher ID")]
        public string TeacherCode { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Class")]
        public string ClassId { get; set; }

        [Display(Name = "Section")]
        public string SectionId { get; set; }

        [Display(Name = "Subject")]
        public string SubjectId { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

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
        public string DateOfJoining { get; set; }

        [Display(Name = "Father's Name")]
        public string FatherName { get; set; }

        [Display(Name = "Mother's Name")]
        public string MotherName { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public string DateOfBirth { get; set; }

        [Display(Name = "Marital Status")]
        public string MaritalStatus { get; set; }

        [Display(Name = "Languages Known")]
        public string LanguagesKnown { get; set; }

        [Display(Name = "Qualification")]
        public string Qualification { get; set; }

        [Display(Name = "Work Experience")]
        public string WorkExperience { get; set; }

        [Display(Name = "Previous School")]
        public string PreviousSchool { get; set; }

        [Display(Name = "Previous School Address")]
        public string PreviousSchoolAddress { get; set; }

        [Display(Name = "Previous School Phone")]
        public string PreviousSchoolPhone { get; set; }

        [Display(Name = "Current Address")]
        public string CurrentAddress { get; set; }

        [Display(Name = "Permanent Address")]
        public string PermanentAddress { get; set; }

        [Display(Name = "PAN Number / ID Number")]
        public string PANNumber { get; set; }

        [Display(Name = "Aadhar Number")]
        public string AadharNumber { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        public string Photo { get; set; }
        public string LoginId { get; set; }
        public string Password { get; set; }

        // Transport Information
        [Display(Name = "Route")]
        public string RouteId { get; set; }

        [Display(Name = "Vehicle Number")]
        public string VehicleId { get; set; }

        [Display(Name = "Pickup Point")]
        public string PickupId { get; set; }

        // Hostel Information
        [Display(Name = "Hostel")]
        public string HostelId { get; set; }

        [Display(Name = "Room No")]
        public string RoomNo { get; set; }

        // System fields
        public int SchoolCode { get; set; }
        public Guid TenantId { get; set; }
        public Guid SessionId { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; }

        // Dropdown Lists
        public IEnumerable<SelectListItem> ClassList { get; set; }
        public IEnumerable<SelectListItem> SectionList { get; set; }
        public IEnumerable<SelectListItem> SubjectList { get; set; }
        public IEnumerable<SelectListItem> GenderList { get; set; }
        public IEnumerable<SelectListItem> BloodGroupList { get; set; }
        public IEnumerable<SelectListItem> MaritalStatusList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        public IEnumerable<SelectListItem> RouteList { get; set; }
        public IEnumerable<SelectListItem> VehicleList { get; set; }
        public IEnumerable<SelectListItem> PickupList { get; set; }
        public IEnumerable<SelectListItem> HostelList { get; set; }
        public IEnumerable<SelectListItem> ContractTypeList { get; set; }
        public IEnumerable<SelectListItem> WorkShiftList { get; set; }
    }

    public class TeacherPayroll
    {
        public Guid PayrollId { get; set; }
        public Guid TeacherId { get; set; }

        [Display(Name = "EPF No")]
        public string EPFNo { get; set; }

        [Display(Name = "Basic Salary")]
        public decimal? BasicSalary { get; set; }

        [Display(Name = "Contract Type")]
        public string ContractType { get; set; }

        [Display(Name = "Work Shift")]
        public string WorkShift { get; set; }

        [Display(Name = "Work Location")]
        public string WorkLocation { get; set; }

        [Display(Name = "Date of Leaving")]
        [DataType(DataType.Date)]
        public string DateOfLeaving { get; set; }
    }

    public class TeacherLeaves
    {
        public Guid LeaveId { get; set; }
        public Guid TeacherId { get; set; }

        [Display(Name = "Medical Leaves")]
        public int MedicalLeaves { get; set; }

        [Display(Name = "Casual Leaves")]
        public int CasualLeaves { get; set; }

        [Display(Name = "Maternity Leaves")]
        public int MaternityLeaves { get; set; }

        [Display(Name = "Sick Leaves")]
        public int SickLeaves { get; set; }
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
        public string IFSCCode { get; set; }

        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }
    }

    public class TeacherSocialMedia
    {
        public Guid SocialMediaId { get; set; }
        public Guid TeacherId { get; set; }

        [Display(Name = "Facebook")]
        public string Facebook { get; set; }

        [Display(Name = "Instagram")]
        public string Instagram { get; set; }

        [Display(Name = "LinkedIn")]
        public string LinkedIn { get; set; }

        [Display(Name = "YouTube")]
        public string YouTube { get; set; }

        [Display(Name = "Twitter")]
        public string Twitter { get; set; }
    }

    public class TeacherDocuments
    {
        [Display(Name = "Resume")]
        public string ResumePath { get; set; }

        [Display(Name = "Joining Letter")]
        public string JoiningLetterPath { get; set; }
    }
}