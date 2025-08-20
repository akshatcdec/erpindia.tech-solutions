using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ERPIndia.BulkUpdate
{
    public class StudentUpdate
    {
        public string AdmsnNo { get; set; }
        public string SchoolCode { get; set; }
        public string Value { get; set; }
    }
    public class BulkUpdateRequest
    {
        public Guid StudentId { get; set; }
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public Guid ModifiedBy { get; set; }
        public List<UpdateItem> Updates { get; set; }
    }

    public class UpdateItem
    {
        public Guid StudentId { get; set; }
        public int AdmsnNo { get; set; }
        public int SchoolCode { get; set; }
        public string Value { get; set; }
    }

    public class BulkUpdateViewModel
    {
        [Required(ErrorMessage = "Please select a class")]
        public string SelectedClass { get; set; }

        public string SelectedSection { get; set; }

        [Required(ErrorMessage = "Please select a column to update")]
        public string SelectedColumn { get; set; }

        public List<StudentUpdateModel> Students { get; set; }
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public List<SelectListItem> Columns { get; set; }

        public BulkUpdateViewModel()
        {
            Students = new List<StudentUpdateModel>();
            Classes = new List<SelectListItem>();
            Sections = new List<SelectListItem>();
            Columns = new List<SelectListItem>();
        }
    }

    public class StudentUpdateModel
    {
        public Guid StudentId { get; set; }
        public int AdmsnNo { get; set; }
        public int SchoolCode { get; set; }
        public string StudentNo { get; set; }
        public string SrNo { get; set; }
        public string RollNo { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string Gender { get; set; }
        public string Mobile { get; set; }
        public string Photo { get; set; }
        public string Email { get; set; }
        public string Category { get; set; }
        public string BloodGroup { get; set; }
        public string Address { get; set; }
        public string House { get; set; }
        public string PickupPoint { get; set; }
        public string FatherMobile { get; set; }
        public string MotherMobile { get; set; }
        public string AadharNo { get; set; }
        public string FatherAadhar { get; set; }
        public string MotherAadhar { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? AdmsnDate { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? OldBalance { get; set; }
        public string PENNo { get; set; }
        public string PreviousSchool { get; set; }
        public string UDISE { get; set; }
        public string VillegeName { get; set; }
        public string Password { get; set; }
        public string FeeCategory { get; set; }
        public string GuardianName { get; set; }
        public bool IsActive { get; set; }
        public string UpdateValue { get; set; } // This will hold the value to update
        public bool IsSelected { get; set; }
    }

    // Dropdown response model for parsing JSON
    public class DropdownResponse
    {
        public bool Success { get; set; }
        public List<DropdownItem> Data { get; set; }
    }

    public class DropdownItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
