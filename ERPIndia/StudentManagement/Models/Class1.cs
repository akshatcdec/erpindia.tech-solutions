using ERPIndia.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.StudentManagement.Models
{
    public class StudentViewModel
    {
        public StudentBasic Basic { get; set; }
        public StudentFamily Family { get; set; }
        public StudentOther Other { get; set; }
        public TransportConfigViewModel TransportConfig { get; set; } // Add this pr
        public StudentViewModel()
        {
            Basic = new StudentBasic();
            Family = new StudentFamily();
            Other = new StudentOther();
            TransportConfig = new TransportConfigViewModel();
        }
    }
    public class TransportConfigViewModel
    {
        public bool AprilFee { get; set; }
        public bool MayFee { get; set; }
        public bool JuneFee { get; set; }
        public bool JulyFee { get; set; }
        public bool AugustFee { get; set; }
        public bool SeptemberFee { get; set; }
        public bool OctoberFee { get; set; }
        public bool NovemberFee { get; set; }
        public bool DecemberFee { get; set; }
        public bool JanuaryFee { get; set; }
        public bool FebruaryFee { get; set; }
        public bool MarchFee { get; set; }
        public bool AprilSubmitted { get; set; }
        public bool MaySubmitted { get; set; }
        public bool JuneSubmitted { get; set; }
        public bool JulySubmitted { get; set; }
        public bool AugustSubmitted { get; set; }
        public bool SeptemberSubmitted { get; set; }
        public bool OctoberSubmitted { get; set; }
        public bool NovemberSubmitted { get; set; }
        public bool DecemberSubmitted { get; set; }
        public bool JanuarySubmitted { get; set; }
        public bool FebruarySubmitted { get; set; }
        public bool MarchSubmitted { get; set; }
        public Guid StudentId { get; set; }
    }
    public class EducationDetail
    {
        public int? EducationId { get; set; }

        [Display(Name = "Class")]
        public string Class { get; set; }

        [Display(Name = "Roll No")]
        public string RollNo { get; set; }

        [Display(Name = "Maximum Marks")]
        public int? MaximumMarks { get; set; }

        [Display(Name = "Obtained Marks")]
        public int? ObtainedMarks { get; set; }

        [Display(Name = "Percentage")]
        public decimal? Percentage { get; set; }

        [Display(Name = "Board")]
        public string Board { get; set; }

        [Display(Name = "Subjects")]
        public string Subjects { get; set; }

        [Display(Name = "Others")]
        public string Others { get; set; }

        [Display(Name = "Passing Year")]
        public string PassingYear { get; set; }
    }
    public class StudentBasic
    {
        // Add these properties
        public Guid StudentID { get; set; }
        public int SessionYear { get; set; }
        public string FatherAadhar { get; set; }
        public string FatherName { get; set; }
        public string PhotoPath { get; set; }
        [Display(Name = "PEN No.")]
        public string PENNo { get; set; }


        [Required(ErrorMessage = "Admission Number is required")]
        [Display(Name = "Adm. No")]
        public string AdmsnNo { get; set; }

        [Required(ErrorMessage = "Class is required")]
        [Display(Name = "Class")]
        public string Class { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string HouseName { get; set; }
        public string FeeCategoryName { get; set; }
        public string CategoryName { get; set; }
        public string DiscountName { get; set; }
        public string FeeDiscountName { get; set; }
        [Required(ErrorMessage = "Student is required")]
        [Display(Name = "Student")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Student Name must be between 2 and 50 characters")]
        public string FirstName { get; set; }
        public int ID { get; set; }


        [Display(Name = "School Code")]
        public int SchoolCode { get; set; }

        [Display(Name = "Student Number")]
        public string StudentNo { get; set; }
        [Required(ErrorMessage = "SR Number is required")]
        [Display(Name = "SR Number")]
        public string SrNo { get; set; }

        [Display(Name = "Roll No")]
        public string RollNo { get; set; }



        [Display(Name = "Section")]
        public string Section { get; set; }



        [Display(Name = "Other Name")]
        public string LastName { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Date of Birth")]
        public string DOB { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Religion")]
        public string Religion { get; set; }

        [Display(Name = "Caste")]
        public string Caste { get; set; }

        [Display(Name = "Mobile")]
        public string Mobile { get; set; }

        [Display(Name = "WhatsApp Number")]
        public string WhatsAppNum { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Admission Date")]
        [Required(ErrorMessage = "Admission Date is required")]
        public string AdmsnDate { get; set; }

        [Display(Name = "Photo")]
        public string Photo { get; set; }

        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }

        [Display(Name = "Student Aadhar")]
        public string AadharNo { get; set; }

        [Display(Name = "House")]
        public string House { get; set; }

        [Display(Name = "Height")]
        public string Height { get; set; }

        [Display(Name = "Weight")]
        public string Weight { get; set; }
        
        [Display(Name = "ABC ID")]
        public string ABCID { get; set; }

        [Display(Name = "APAR ID")]
        public string APARID { get; set; }
        [Display(Name = "Living Since")]
        public string LivingHere { get; set; }

        [Display(Name = "As On Date")]
        public DateTime? AsOnDt { get; set; }

        [Display(Name = "Sibling Reference")]
        public string SiblingRef { get; set; }

        [Display(Name = "Sibling Reference 2")]
        public string SiblingRef2 { get; set; }

        [Display(Name = "Discount Category")]
        public string DiscountCategory { get; set; }

        [Display(Name = "Pickup Point")]
        public string PickupPoint { get; set; }

        [Display(Name = "Discount Note")]
        public string DiscountNote { get; set; }

        [Display(Name = "Login Password")]
        public string LoginPwd { get; set; }

        [Display(Name = "Old Balance")]
        public int? OldBalance { get; set; }

        [Display(Name = "Fee Category")]
        public string FeeCategory { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Enquiry Data")]
        public string EnquiryData { get; set; }

        [Display(Name = "Send SMS")]
        public bool SendSMS { get; set; }

        [Display(Name = "Student ID")]
        public string UserId { get; set; }

        [Display(Name = "Entry Date")]
        public DateTime? EntryDate { get; set; }

        // New fields
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }

        [Display(Name = "Mother Tongue")]
        public string MotherTongue { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Languages Known")]
        public string LanguagesKnown { get; set; }

        [Display(Name = "Student Login Id")]
        public string LoginId { get; set; }
        [Display(Name = "Password")]
        public string Password { get; set; }
        public List<SubjectInfo> Subjects { get; set; } = new List<SubjectInfo>();
        public List<SelectListItem> BasicClassList { get; set; }
        public List<SelectListItem> BasicBatchList { get; set; }
        public List<SelectListItem> BasicFeeList { get; set; }
        public List<SelectListItem> BasicSectionList { get; set; }
        public List<SelectListItem> BasicDiscountList { get; set; }
        public List<SelectListItem> BasicRouteList { get; set; }
        public List<SelectListItem> BasicVehiclesList { get; set; }
        public List<SelectListItem> BasicPickUpList { get; set; }
        public List<SelectListItem> BasicTownList { get; set; }
        public List<SelectListItem> BasicGenderList { get; set; }
        public List<SelectListItem> BasicBloodGroupList { get; set; }
        public List<SelectListItem> BasicCategoryList { get; set; }
        public List<SelectListItem> BasicMotherList { get; set; }
        public List<SelectListItem> BasicReligionList { get; set; }
        public List<SelectListItem> BasicHouseList { get; set; }
        public List<SelectListItem> BasicHostelList { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? HouseId { get; set; }
        public Guid? FeeCategoryId { get; set; }
        public Guid? FeeDiscountId { get; set; }
        public int TenantCode { get; set; }
        public Guid TenantId { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid SessionId { get; set; }
        public Guid VechileId { get; set; }
        public Guid PickupId { get; set; }
        public Guid RouteId { get; set; }
        public Guid VillegeId { get; set; }
        public Guid HostelId { get; set; }
        public string BatchName { get; set; }
        [Display(Name = "Batch Name")]
        public Guid BatchId { get; set; }
        public string Medium { get; set; }
        public string VechileName { get; set; }
        public string PickupName { get; set; }
        public string RouteName { get; set; }
        public string VillegeName { get; set; }
        public string HostelName { get; set; }
        [Display(Name = "Student Name in Hindi")]
        public string StudentNameHindi { get; set; }
        [Display(Name = "Father Name in Hindi")]
        public string FatherNameHindi { get; set; }
        [Display(Name = "Mother Name in Hindi")]
        public string MotherNameHindi { get; set; }
        [Display(Name = "Guardian Name in Hindi")]
        public string GuardianNameHindi { get; set; }
        public string vDOB { get; set; }
        public string vAdm { get; set; }
         public bool IsFeeRecordExist { get; set; }
        [Display(Name = "Late Fee")]
        public bool IsLateFee { get; set; }
        
        // For file upload

    }

    // Model for StudentFamily table
    public class StudentFamily
    {
        public HttpPostedFileBase PhotoFile { get; set; }
        // FK from basic
        public int AdmsnNo { get; set; }
        public Guid? VehicleId{ get; set; }
        public Guid? RouteId { get; set; }
        public Guid? PickUpId { get; set; }
        public Guid SessionId { get; set; }
        public Guid StudentId { get; set; }
        public Guid CreatedBy { get; set; }
        
        public int TenantCode { get; set; }
        public Guid TenantId { get; set; }
        public int SchoolCode { get; set; }
        // Add these properties
        public string FatherPhotoPath { get; set; }
        public string MotherPhotoPath { get; set; }
        public string GuardianPhotoPath { get; set; }
        public string GuardianType { get; set; }
        public List<SelectListItem> RouteList { get; set; }
        [Display(Name = "Father's Name")]
        public string FName { get; set; }

        [Display(Name = "Father's Phone")]
        public string FPhone { get; set; }

        [Display(Name = "Father's Occupation")]
        public string FOccupation { get; set; }

        [Display(Name = "Father's Education")]
        public string FEducation { get; set; }

        [Display(Name = "Father's Aadhar")]
        public string FAadhar { get; set; }

        [Display(Name = "Father's Note")]
        public string FNote { get; set; }

        [Display(Name = "Father's Photo")]
        public string FPhoto { get; set; }

        [Display(Name = "Mother's Name")]
        public string MName { get; set; }

        [Display(Name = "Mother's Phone")]
        public string MPhone { get; set; }

        [Display(Name = "Mother's Education")]
        public string MEducation { get; set; }

        [Display(Name = "Mother's Occupation")]
        public string MOccupation { get; set; }

        [Display(Name = "Mother's Aadhar")]
        public string MAadhar { get; set; }

        [Display(Name = "Mother's Note")]
        public string MNote { get; set; }

        [Display(Name = "Mother's Photo")]
        public string MPhoto { get; set; }

        [Display(Name = "Guardian's Name")]
        public string GName { get; set; }

        [Display(Name = "Guardian's Relation")]
        public string GRelation { get; set; }

        [Display(Name = "Guardian's Email")]
        [DataType(DataType.EmailAddress)]
        public string GEmail { get; set; }

        [Display(Name = "Guardian's Photo")]
        public string GPhoto { get; set; }

        [Display(Name = "Guardian's Phone")]
        public string GPhone { get; set; }

        [Display(Name = "Guardian's Occupation")]
        public string GOccupation { get; set; }
        [Display(Name = "Guardian's Education")]
        public string GEducation { get; set; }

        [Display(Name = "Guardian's Address")]
        public string GAddress { get; set; }

        [Display(Name = "Guardian's Remark")]
        public string GRemark { get; set; }

        [Display(Name = "Full Address")]
        public string StCurrentAddress { get; set; }

        [Display(Name = "Village/City/Town")]
        public string StPermanentAddress { get; set; }

        [Display(Name = "Route")]
        public string Route { get; set; }

        [Display(Name = "Hostel Detail")]
        public string HostelDetail { get; set; }

        [Display(Name = "Hostel Number")]
        public string HostelNo { get; set; }

        // New Fields
        [Display(Name = "Father's Email")]
        [DataType(DataType.EmailAddress)]
        public string FEmail { get; set; }

        [Display(Name = "Mother's Email")]
        [DataType(DataType.EmailAddress)]
        public string MEmail { get; set; }


        // Individual sibling information

        [Display(Name = "Has Siblings in Same School")]
        public bool IsSiblingInSameSchool { get; set; }

        [Display(Name = "Sibling Name")]
        public string SiblingName { get; set; }

        [Display(Name = "Sibling Class")]
        public string SiblingClass { get; set; }

        [Display(Name = "Sibling Roll No")]
        public string SiblingRollNo { get; set; }

        [Display(Name = "Sibling Admission No")]
        public string SiblingAdmissionNo { get; set; }


        [Display(Name = "Transport Needed")]
        public bool TransportNeeded { get; set; }

        [Display(Name = "Vehicle Number")]
        public string VehicleNumber { get; set; }

        [Display(Name = "Pickup Point")]
        public string PickupPoint { get; set; }

        [Display(Name = "Hostel Needed")]
        public bool HostelNeeded { get; set; }

        // For file uploads
        public HttpPostedFileBase FatherPhotoFile { get; set; }
        public HttpPostedFileBase MotherPhotoFile { get; set; }
        public HttpPostedFileBase GuardianPhotoFile { get; set; }

        public List<SiblingInfo> Siblings { get; set; } = new List<SiblingInfo>();
    }
    public class SubjectInfo
    {
        public Guid SubjectId { get; set; }
        public string Name { get; set; }

        public bool IsElective { get; set; }

        public string TeacherName { get; set; }

        // This can be used to store if subject is selected in a multi-select scenario
        public bool IsSelected { get; set; }
        public string KeyValue { get; set; }

    }
    public class SiblingInfo
    {
        public int? SiblingId { get; set; }
        public string Name { get; set; }
        public string RollNo { get; set; }
        public string AdmissionNo { get; set; }
        public string Class { get; set; }
        public string FatherName { get; set; }
        public string FatherAadharNo { get; set; }
    }
    public class SubjectViewModel
    {
        public int SubjectId { get; set; }
        public string Name { get; set; }
        public bool IsElective { get; set; }
        public string TeacherName { get; set; }
        public bool IsSelected { get; set; }
    }
    // Model for StudentOther table
    public class StudentOther
    {
        public int TenantCode { get; set; }
        public Guid TenantId { get; set; }
        // Add these properties
        public Guid SessionId { get; set; }
        public Guid StudentId { get; set; }
        public Guid CreatedBy { get; set; }
        public string UploadPath1 { get; set; }
        public string UploadPath2 { get; set; }
        public string UploadPath3 { get; set; }
        public string UploadPath4 { get; set; }
        [Display(Name = "Class")]
        public string EduDetailsClass { get; set; }
        [Display(Name = "RollNo")]
        public string EduDetailsRollNo { get; set; }
        [Display(Name = "Maximum Marks")]
        public decimal EduDetailsMM { get; set; }
        [Display(Name = "Obtain Marks")]
        public decimal EduDetailsObtainMarks { get; set; }
        [Display(Name = "Board")]
        public string EduDetailsBoard { get; set; }
        [Display(Name = "Subject")]
        public string EduDetailsSubjects { get; set; }
        [Display(Name = "Other")]
        public string EduDetailsOthers { get; set; }
        [Display(Name = "Passing Year")]
        public string EduDetailsPassingYear { get; set; }
        [Display(Name = "Percentage")]
        public decimal EduDetailsPercentage { get; set; }
        // FK from basic
        public int AdmsnNo { get; set; }
        public int SchoolCode { get; set; }

        [Display(Name = "Bank Account Number")]
        public string BankAcNo { get; set; }

        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Display(Name = "IFSC Code")]
        public string IfscCode { get; set; }

        [Display(Name = "SSSM ID")]
        public string NADID { get; set; }

        [Display(Name = "Family Id")]
        public string IDentityLocal { get; set; }

        [Display(Name = "Identity Other")]
        public string IdentityOther { get; set; }

        [Display(Name = "Previous School Details")]
        public string PreviousSchoolDtl { get; set; }

        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "Upload Title 1")]
        public string UploadTitle1 { get; set; }

        [Display(Name = "Upload Path 1")]
        public string UpldPath1 { get; set; }

        [Display(Name = "Upload Title 2")]
        public string UploadTitle2 { get; set; }

        [Display(Name = "Upload Path 2")]
        public string UpldPath2 { get; set; }

        [Display(Name = "Upload Title 3")]
        public string UploadTitle3 { get; set; }

        [Display(Name = "Upload Path 3")]
        public string UpldPath3 { get; set; }

        [Display(Name = "Upload Title 4")]
        public string UploadTitle4 { get; set; }

        [Display(Name = "Upload Path 4")]
        public string UpldPath4 { get; set; }

        [Display(Name = "Upload Title 5")]
        public string UploadTitle5 { get; set; }

        [Display(Name = "Upload Path 5")]
        public string UpldPath5 { get; set; }

        [Display(Name = "Upload Title 6")]
        public string UploadTitle6 { get; set; }

        [Display(Name = "Upload Path 6")]
        public string UpldPath6 { get; set; }

        // New Fields
        [Display(Name = "Medical Condition")]
        public string MedicalCondition { get; set; }

        [Display(Name = "Allergies")]
        public string Allergies { get; set; }

        [Display(Name = "Medications")]
        public string Medications { get; set; }

        [Display(Name = "Bank Branch")]
        public string BankBranch { get; set; }

        [Display(Name = "Other Information")]
        public string OtherInformation { get; set; }

        [Display(Name = "Previous School Address")]
        public string PreviousSchoolAddress { get; set; }
        [Display(Name = "Udise Code")]
        public string UdiseCode { get; set; }

        [Display(Name = "School Note")]
        public string SchoolNote { get; set; }
        public List<EducationDetail> EducationDetails { get; set; } = new List<EducationDetail>();

        // For file uploads
        public HttpPostedFileBase Document1 { get; set; }
        public HttpPostedFileBase Document2 { get; set; }
        public HttpPostedFileBase Document3 { get; set; }
        public HttpPostedFileBase Document4 { get; set; }
        public HttpPostedFileBase Document5 { get; set; }
        public HttpPostedFileBase Document6 { get; set; }
    }
}
