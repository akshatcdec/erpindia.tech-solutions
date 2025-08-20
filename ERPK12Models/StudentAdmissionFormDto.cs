// DTOs for Student Information
using System;
using System.Collections.Generic;

namespace StudentManagement.DTOs
{
    public class StudentInfoBasicDto
    {
        public int AdmsnNo { get; set; }
        public int SchoolCode { get; set; }
        public string StudentNo { get; set; }
        public string SrNo { get; set; }
        public string RollNo { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string Category { get; set; }
        public string Religion { get; set; }
        public string Caste { get; set; }
        public string Mobile { get; set; }
        public string WhatsAppNum { get; set; }
        public string Email { get; set; }
        public DateTime? AdmsnDate { get; set; }
        public string Photo { get; set; }
        public string FatherPhoto { get; set; }
        public string MotherPhoto { get; set; }
        public string GuardianPhoto { get; set; }
        public string BloodGroup { get; set; }
        public string House { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public DateTime? AsOnDt { get; set; }
        public string SiblingRef { get; set; }
        public string SiblingRef2 { get; set; }
        public string DiscountCategory { get; set; }
        public string DiscountNote { get; set; }
        public string LoginPwd { get; set; }
        public int? OldBalance { get; set; }
        public string FeeCategory { get; set; }
        public string Active { get; set; }
        public string EnquiryData { get; set; }
        public bool? SendSMS { get; set; }
        public string UserId { get; set; }
        public DateTime? EntryDate { get; set; }
        public string AcademicYear { get; set; }
        public string MotherTongue { get; set; }
        public string Status { get; set; }
        public string LanguagesKnown { get; set; }
        public string AadharNo { get; set; }
        public string PENNo { get; set; }
        public string PickupPoint { get; set; }
        public string LoginId { get; set; }
        public string Password { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? HouseId { get; set; }
        public Guid? FeeCategoryId { get; set; }
        public Guid? FeeDiscountId { get; set; }
        public Guid? TenantID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? SessionID { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? TenantCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid StudentId { get; set; }
        public bool? IsLateFee { get; set; }
        public Guid? VechileId { get; set; }
        public Guid? PickupId { get; set; }
        public Guid? RouteId { get; set; }
        public Guid? VillegeId { get; set; }
        public Guid? HostelId { get; set; }
        public string VechileName { get; set; }
        public string PickupName { get; set; }
        public string RouteName { get; set; }
        public string VillegeName { get; set; }
        public string HostelName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string CategoryName { get; set; }
        public string DiscountName { get; set; }
        public int? DobMonth { get; set; }
        public int? DobYear { get; set; }
        public int? DobDay { get; set; }
        public int? AdmMonth { get; set; }
        public int? AdmYear { get; set; }
        public int? AdmDay { get; set; }
        public string TenantName { get; set; }
        public string SessIonName { get; set; }
        public string CreatedName { get; set; }
        public string ModifyName { get; set; }
        public string VehicleNumber { get; set; }
        public string VillageName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string GuardianName { get; set; }
        public string PreviousSchool { get; set; }
        public string UDISE { get; set; }
        public string Address { get; set; }
        public string FatherMobile { get; set; }
        public string MotherMobile { get; set; }
        public string FatherAadhar { get; set; }
        public string MotherAadhar { get; set; }
        public string HasFeeRecords { get; set; }

        public string Doc1 { get; set; }
        public string Doc2 { get; set; }
        public string Doc3 { get; set; }
        public string Doc4 { get; set; }
        public string Doc5 { get; set; }
        public string Doc6 { get; set; }
        public string Doc7 { get; set; }
        public string Doc8 { get; set; }
        public string Doc9 { get; set; }
        public string Doc10 { get; set; }
        // Computed Properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string FormattedDOB => DOB?.ToString("dd-MM-yyyy") ?? "";

     
    }

    public class StudentInfoFamilyDto
    {
        public int AdmsnNo { get; set; }
        public int SchoolCode { get; set; }
        public string FName { get; set; }
        public string FPhone { get; set; }
        public string FOccupation { get; set; }
        public string FAadhar { get; set; }
        public string FNote { get; set; }
        public string FPhoto { get; set; }
        public string MName { get; set; }
        public string MPhone { get; set; }
        public string MOccupation { get; set; }
        public string MAadhar { get; set; }
        public string MNote { get; set; }
        public string MPhoto { get; set; }
        public string GName { get; set; }
        public string GRelation { get; set; }
        public string GEmail { get; set; }
        public string GPhoto { get; set; }
        public string GPhone { get; set; }
        public string GOccupation { get; set; }
        public string GAddress { get; set; }
        public string GRemark { get; set; }
        public string StCurrentAddress { get; set; }
        public string StPermanentAddress { get; set; }
        public string Route { get; set; }
        public string HostelDetail { get; set; }
        public string HostelNo { get; set; }
        public string FEmail { get; set; }
        public string MEmail { get; set; }
        public bool? IsSiblingInSameSchool { get; set; }
        public string SiblingName { get; set; }
        public string SiblingClass { get; set; }
        public string SiblingRollNo { get; set; }
        public string SiblingAdmissionNo { get; set; }
        public bool? TransportNeeded { get; set; }
        public string VehicleNumber { get; set; }
        public string PickupPoint { get; set; }
        public bool? HostelNeeded { get; set; }
        public string FEducation { get; set; }
        public string MEducation { get; set; }
        public string GEducation { get; set; }
        public Guid? TenantID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? SessionID { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? TenantCode { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? PickUpId { get; set; }
        public Guid? VehicleId { get; set; }
        public Guid? RouteId { get; set; }
    }

    public class StudentInfoOtherDto
    {
        public int AdmsnNo { get; set; }
        public int SchoolCode { get; set; }
        public string BankAcNo { get; set; }
        public string BankName { get; set; }
        public string IfscCode { get; set; }
        public string NADID { get; set; }
        public string IDentityLocal { get; set; }
        public string IdentityOther { get; set; }
        public string PreviousSchoolDtl { get; set; }
        public string Note { get; set; }
        public string UploadTitle1 { get; set; }
        public string UpldPath1 { get; set; }
        public string UploadTitle2 { get; set; }
        public string UpldPath2 { get; set; }
        public string UploadTitle3 { get; set; }
        public string UpldPath3 { get; set; }
        public string UploadTitle4 { get; set; }
        public string UpldPath4 { get; set; }
        public string UploadTitle5 { get; set; }
        public string UpldPath5 { get; set; }
        public string UploadTitle6 { get; set; }
        public string UpldPath6 { get; set; }
        public string MedicalCondition { get; set; }
        public string Allergies { get; set; }
        public string Medications { get; set; }
        public string BankBranch { get; set; }
        public string OtherInformation { get; set; }
        public string PreviousSchoolAddress { get; set; }
        public string EduDetailsClass { get; set; }
        public string EduDetailsRollNo { get; set; }
        public decimal? EduDetailsMM { get; set; }
        public decimal? EduDetailsObtainMarks { get; set; }
        public string EduDetailsBoard { get; set; }
        public string EduDetailsSubjects { get; set; }
        public string EduDetailsOthers { get; set; }
        public string EduDetailsPassingYear { get; set; }
        public decimal? EduDetailsPercentage { get; set; }
        public string UdiseCode { get; set; }
        public string SchoolNote { get; set; }
        public Guid? TenantID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? SessionID { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? TenantCode { get; set; }
        public Guid? StudentId { get; set; }
    }

    public class StudentInfoSubjectDto
    {
        public int Id { get; set; }
        public Guid? SubjectId { get; set; }
        public int AdmsnNo { get; set; }
        public int SchoolCode { get; set; }
        public string Name { get; set; }
        public bool IsElective { get; set; }
        public string TeacherName { get; set; }
        public bool IsSelected { get; set; }
        public Guid? TenantID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? SessionID { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? TenantCode { get; set; }
        public Guid? StudentId { get; set; }
    }

    public class StudentInfoSiblingDto
    {
        public int SiblingId { get; set; }
        public int AdmsnNo { get; set; }
        public int SchoolCode { get; set; }
        public string Name { get; set; }
        public string RollNo { get; set; }
        public string AdmissionNo { get; set; }
        public string Class { get; set; }
        public string FatherName { get; set; }
        public string FatherAadharNo { get; set; }
        public Guid? TenantID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? SessionID { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? TenantCode { get; set; }
        public Guid? StudentId { get; set; }
    }

    public class StudentInfoEduDetailDto
    {
        public int EducationId { get; set; }
        public int AdmsnNo { get; set; }
        public int SchoolCode { get; set; }
        public string Class { get; set; }
        public string RollNo { get; set; }
        public decimal? MaximumMarks { get; set; }
        public decimal? ObtainedMarks { get; set; }
        public decimal? Percentage { get; set; }
        public string Board { get; set; }
        public string Subjects { get; set; }
        public string Others { get; set; }
        public string PassingYear { get; set; }
        public Guid? TenantID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? SessionID { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? TenantCode { get; set; }
        public Guid? StudentId { get; set; }
    }

    // Comprehensive DTO for PDF Generation
    public class StudentAdmissionFormDto
    {
        public StudentInfoBasicDto BasicInfo { get; set; }
        public StudentInfoFamilyDto FamilyInfo { get; set; }
        public StudentInfoOtherDto OtherInfo { get; set; }
        public List<StudentInfoSubjectDto> Subjects { get; set; } = new List<StudentInfoSubjectDto>();
        public List<StudentInfoSiblingDto> Siblings { get; set; } = new List<StudentInfoSiblingDto>();
        public List<StudentInfoEduDetailDto> EducationDetails { get; set; } = new List<StudentInfoEduDetailDto>();
    }
}