using System;
using System.Collections.Generic;

namespace ERPK12Models.StudentInformation
{
    // Main DTO to hold all student information
    public class StudentCompleteInfoDto
    {
        // Basic student information
        public StudentBasicInfoDto BasicInfo { get; set; }

        // Education details (one-to-many)
        public List<StudentEducationDetailDto> EducationDetails { get; set; }

        // Siblings information (one-to-many)
        public List<StudentSiblingDto> Siblings { get; set; }

        // Subjects information (one-to-many)
        public List<StudentSubjectDto> Subjects { get; set; }

        public StudentCompleteInfoDto()
        {
            EducationDetails = new List<StudentEducationDetailDto>();
            Siblings = new List<StudentSiblingDto>();
            Subjects = new List<StudentSubjectDto>();
        }
    }

    // Combined basic info from StudentInfoBasic, StudentInfoFamily, and StudentInfoOther tables
    public class StudentBasicInfoDto
    {
        // Basic Info from StudentInfoBasic
        public Guid StudentId { get; set; }
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

        // GUIDs from StudentInfoBasic
        public Guid? ClassId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? HouseId { get; set; }
        public Guid? FeeCategoryId { get; set; }
        public Guid? FeeDiscountId { get; set; }
        public Guid? VechileId { get; set; }
        public Guid? PickupId { get; set; }
        public Guid? RouteId { get; set; }
        public Guid? VillegeId { get; set; }
        public Guid? HostelId { get; set; }

        // Names from StudentInfoBasic
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string CategoryName { get; set; }
        public string DiscountName { get; set; }
        public string VechileName { get; set; }
        public string PickupName { get; set; }
        public string RouteName { get; set; }
        public string VillegeName { get; set; }
        public string HostelName { get; set; }

        // System fields from StudentInfoBasic
        public string TenantName { get; set; }
        public string SessionName { get; set; }
        public string CreatedName { get; set; }
        public string ModifyName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? SessionID { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? TenantID { get; set; }
        public int? TenantCode { get; set; }
        public bool? IsLateFee { get; set; }

        // Family Info from StudentInfoFamily
        public string FatherName { get; set; } // FName in DB
        public string FatherPhone { get; set; } // FPhone in DB
        public string FatherOccupation { get; set; } // FOccupation in DB
        public string FatherAadhar { get; set; } // FAadhar in DB
        public string FatherNote { get; set; } // FNote in DB
        public string FatherPhoto { get; set; } // FPhoto in DB
        public string FatherEmail { get; set; } // FEmail in DB
        public string FatherEducation { get; set; } // FEducation in DB

        public string MotherName { get; set; } // MName in DB
        public string MotherPhone { get; set; } // MPhone in DB
        public string MotherOccupation { get; set; } // MOccupation in DB
        public string MotherAadhar { get; set; } // MAadhar in DB
        public string MotherNote { get; set; } // MNote in DB
        public string MotherPhoto { get; set; } // MPhoto in DB
        public string MotherEmail { get; set; } // MEmail in DB
        public string MotherEducation { get; set; } // MEducation in DB

        public string GuardianName { get; set; } // GName in DB
        public string GuardianRelation { get; set; } // GRelation in DB
        public string GuardianEmail { get; set; } // GEmail in DB
        public string GuardianPhoto { get; set; } // GPhoto in DB
        public string GuardianPhone { get; set; } // GPhone in DB
        public string GuardianOccupation { get; set; } // GOccupation in DB
        public string GuardianAddress { get; set; } // GAddress in DB
        public string GuardianRemark { get; set; } // GRemark in DB
        public string GuardianEducation { get; set; } // GEducation in DB

        public string CurrentAddress { get; set; } // StCurrentAddress in DB
        public string PermanentAddress { get; set; } // StPermanentAddress in DB

        // Transport & Hostel Info from StudentInfoFamily
        public bool? TransportNeeded { get; set; }
        public string VehicleNumber { get; set; }
        public string Route { get; set; }
        public bool? HostelNeeded { get; set; }
        public string HostelDetail { get; set; }
        public string HostelNo { get; set; }

        // Sibling Info from StudentInfoFamily
        public bool? IsSiblingInSameSchool { get; set; }
        public string SiblingName { get; set; }
        public string SiblingClass { get; set; }
        public string SiblingRollNo { get; set; }
        public string SiblingAdmissionNo { get; set; }

        // Other Info from StudentInfoOther
        public string BankAcNo { get; set; }
        public string BankName { get; set; }
        public string BankBranch { get; set; }
        public string IfscCode { get; set; }
        public string NADID { get; set; }
        public string IdentityLocal { get; set; }
        public string IdentityOther { get; set; }
        public string PreviousSchoolDetails { get; set; } // PreviousSchoolDtl in DB
        public string PreviousSchoolAddress { get; set; }
        public string Note { get; set; }
        public string MedicalCondition { get; set; }
        public string Allergies { get; set; }
        public string Medications { get; set; }
        public string OtherInformation { get; set; }
        public string UdiseCode { get; set; }
        public string SchoolNote { get; set; }

        // Upload fields from StudentInfoOther
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

        // EduDetails from StudentInfoOther (summary fields)
        public string EduDetailsClass { get; set; }
        public string EduDetailsRollNo { get; set; }
        public decimal? EduDetailsMM { get; set; }
        public decimal? EduDetailsObtainMarks { get; set; }
        public string EduDetailsBoard { get; set; }
        public string EduDetailsSubjects { get; set; }
        public string EduDetailsOthers { get; set; }
        public string EduDetailsPassingYear { get; set; }
        public decimal? EduDetailsPercentage { get; set; }

        public int? DobMonth { get; set; }
        public int? DobYear { get; set; }
        public int? DobDay { get; set; }
        public int? AdmMonth { get; set; }
        public int? AdmYear { get; set; }
        public int? AdmDay { get; set; }
    }

    // Educational details (from StudentInfoEduDetails table)
    public class StudentEducationDetailDto
    {
        public int EducationId { get; set; }
        public string Class { get; set; }
        public string RollNo { get; set; }
        public decimal? MaximumMarks { get; set; }
        public decimal? ObtainedMarks { get; set; }
        public decimal? Percentage { get; set; }
        public string Board { get; set; }
        public string Subjects { get; set; }
        public string PassingYear { get; set; }
    }

    // Siblings information (from StudentInfoSiblings table)
    public class StudentSiblingDto
    {
        public int SiblingId { get; set; }
        public string Name { get; set; }
        public string RollNo { get; set; }
        public string AdmissionNo { get; set; }
        public string Class { get; set; }
        public string FatherName { get; set; }
        public string FatherAadharNo { get; set; }
    }

    // Subject information (from StudentInfoSubjects table)
    public class StudentSubjectDto
    {
        public int Id { get; set; }
        public Guid? SubjectId { get; set; }
        public string Name { get; set; }
        public bool IsElective { get; set; }
        public string TeacherName { get; set; }
        public bool IsSelected { get; set; }
    }
}