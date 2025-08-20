
using System;

namespace ERPIndia.DTOs.Student
{
    public class StudentViewInfoDTO
    {
        // Basic Information
        public string AdmsnDate { get; set; }
        public string AdmsnNo { get; set; }
        public string SchoolCode { get; set; }
        public string StudentNo { get; set; }
        public string SrNo { get; set; }
        public string RollNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string DOB { get; set; }
        public string Photo { get; set; }
        public string Category { get; set; }
        public string Religion { get; set; }
        public string Caste { get; set; }
        public string Mobile { get; set; }
        public string AadharNo { get; set; }
        public string PENNo { get; set; }

        // Family Information
        public string FatherName { get; set; }
        public string FatherAadhar { get; set; }
        public string MotherName { get; set; }
        public string MotherAadhar { get; set; }
        public string GuardianName { get; set; }
        public string StCurrentAddress { get; set; }
        public string StPermanentAddress { get; set; }
        public string VillageName { get; set; }

        // Academic Information
        public string Class { get; set; }
        public string ClassName { get; set; }
        public string Section { get; set; }
        public string SectionName { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? HouseId { get; set; }
        public string UdiseCode { get; set; }

        // Fee Information
        public string CategoryName { get; set; }
        public string DiscountName { get; set; }
        public string DiscountCategory { get; set; }
        public string FeeCategory { get; set; }
        public Guid? FeeCategoryId { get; set; }
        public Guid? FeeDiscountId { get; set; }
        public decimal? OldBalance { get; set; }

        // Transport Information
        public string PickupName { get; set; }
        public decimal? Fee { get; set; }

        // System Information
        public Guid StudentId { get; set; }
        public Guid? TenantID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? SessionID { get; set; }
    }
}