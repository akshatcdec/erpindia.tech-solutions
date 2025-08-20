using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Dapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using ERPIndia.Class.Helper;
using ERPIndia.Controllers.Examination;
using ERPIndia.DTOs.Cashbook;
using ERPIndia.DTOs.Student;
using ERPIndia.FeeSummary.DTO;
using ERPIndia.Models;
using ERPIndia.Models.CollectFee.DTOs;
using ERPIndia.StudentManagement.Models;
using ERPIndia.StudentManagement.Repository;
using ERPIndia.Utilities;
using ERPK12Models.StudentInformation;
using Hangfire.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class ClasswiseListViewModel
    {
        public string CurrentSession { get; set; } = "2024-25";
        public Guid? SessionId { get; set; }
        public int? TenantCode { get; set; }

        public string SelectedClass { get; set; } = "ALL";
        public string SelectedSection { get; set; } = "ALL";
        public string SelectedSubject { get; set; } = "ALL";

        public IEnumerable<SelectListItem> ClassList { get; set; }
        public IEnumerable<SelectListItem> SectionList { get; set; }
        public IEnumerable<SelectListItem> SubjectList { get; set; }

        public IEnumerable<StudentViewInfoDTO> Students { get; set; }

        // Add helper method to create dropdown lists from string collections
        public static IEnumerable<SelectListItem> CreateSelectList(IEnumerable<string> items, string selectedValue = "ALL")
        {
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Value = "ALL", Text = "ALL", Selected = (selectedValue == "ALL") }
            };

            if (items != null)
            {
                selectList.AddRange(items.Select(item => new SelectListItem
                {
                    Value = item,
                    Text = item,
                    Selected = (item == selectedValue)
                }));
            }

            return selectList;
        }
    }
    public class StudentController : BaseController
    {
        private readonly StudentRepository _repository;
        private readonly DropdownController _dropdownController;
        private readonly IExaminationRepository _examinationRepository;

        public StudentController()
        {
            // GetCurrentClientId( = Utils.ParseInt(CurrentTenantCode);
            _repository = new StudentRepository();
            _dropdownController = new DropdownController();
            _examinationRepository = new ExaminationRepository();

        }


        public IEnumerable<StudentViewInfoDTO> GetStudentsByClass(Guid? classId, Guid? sectionId, Guid sessionId, int tenantCode, Guid? subjectId = null)
        {
            // Convert GUIDs to string parameters for backward compatibility
            string className = "ALL";
            string sectionName = "ALL";
            string subjectName = "ALL";

            // Handle empty session ID


            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                // Use the stored procedure with both ID and name parameters
                return conn.Query<StudentViewInfoDTO>("GetStudentsByFilters", new
                {
                    ClassId = classId != Guid.Empty ? classId : null,
                    SectionId = sectionId != Guid.Empty ? sectionId : null,
                    SubjectId = subjectId != Guid.Empty ? subjectId : null,
                    ClassName = className,
                    SectionName = sectionName,
                    SubjectName = subjectName,
                    SessionID = sessionId,
                    TenantCode = tenantCode
                }, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public ActionResult ClassList(string selectedClass = "ALL", string selectedSection = "ALL", string selectedSubject = "ALL")
        {
            // Handle null or empty parameters by defaulting to "ALL"
            selectedClass = string.IsNullOrWhiteSpace(selectedClass) ? "ALL" : selectedClass;
            selectedSection = string.IsNullOrWhiteSpace(selectedSection) ? "ALL" : selectedSection;
            selectedSubject = string.IsNullOrWhiteSpace(selectedSubject) ? "ALL" : selectedSubject;

            // Convert string parameters to GUIDs if necessary
            Guid? classId = null;
            if (selectedClass != "ALL" && Guid.TryParse(selectedClass, out Guid parsedClassId))
            {
                classId = parsedClassId;
            }

            Guid? sectionId = null;
            if (selectedSection != "ALL" && Guid.TryParse(selectedSection, out Guid parsedSectionId))
            {
                sectionId = parsedSectionId;
            }

            Guid? subjectId = null;
            if (selectedSubject != "ALL" && Guid.TryParse(selectedSubject, out Guid parsedSubjectId))
            {
                subjectId = parsedSubjectId;
            }

            // Fetch dropdown data
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();
            var subjectResult = _dropdownController.GetSubjects();

            // Get current session and tenant info
            var sessionId = CurrentSessionID;
            var tenantCode = Utils.ParseInt(CurrentTenantCode);

            // Build the view model
            var viewModel = new ClasswiseListViewModel
            {
                SelectedClass = selectedClass,
                SelectedSection = selectedSection,
                SelectedSubject = selectedSubject,

                // Populate dropdown lists
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
                SubjectList = ConvertToSelectListDefault(subjectResult),

                // Get filtered students - use the appropriate method based on your implementation
                Students = GetStudentsByClass(classId, sectionId, sessionId, tenantCode, subjectId)
            };

            return View(viewModel);
        }
        private List<SelectListItem> ConvertToSelectListDefault(JsonResult result)
        {
            try
            {
                // Serialize the Data to a string to handle dynamic object
                string jsonString = JsonConvert.SerializeObject(result.Data);
                // Deserialize into a strongly-typed object
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownResponse>(jsonString);
                // Check if deserialization was successful and data exists
                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    // Create a new list with "ALL" as the first item
                    var newList = new List<DropdownItem>
            {
                new DropdownItem
                {
                    Id = Guid.Empty,
                    Name = "ALL"
                }
            };

                    // Add the existing items to the new list
                    newList.AddRange(dropdownResponse.Data);

                    // Convert to SelectListItems
                    return newList.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }
                // Return empty list if no data or unsuccessful
                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
                // Log the error
                // LogError(ex, "ConvertToSelectList");
            }
        }
        public ActionResult Discontinued()
        {
            // Redirect to Index with viewType parameter
            return RedirectToAction("Index", new { viewType = "Discontinued" });
        }

        // GET: Student
        public async Task<ActionResult> Index(Guid? ClassId = null, Guid? SectionId = null, string viewType = "Active")
        {
            var dropdownController = new DropdownController();
            var classesResult = dropdownController.GetClasses();
            var sectionsResult = dropdownController.GetSections();
            ViewBag.Classes = ConvertToSelectList(classesResult) ;
            ViewBag.Sections = ConvertToSelectList(sectionsResult);
            ViewBag.ViewType = viewType; // Store the current view type
            var students = await _repository.GetAllStudentsAsync(Utils.ParseInt(CurrentSchoolCode),CurrentSessionID.ToString(),ClassId,SectionId, viewType);
            return View(students);
        }
        public ActionResult PayFee()
        {
            return View();
        }
        private List<SubjectInfo> ConvertToSubjectInfoList(JsonResult result)
        {
            try
            {
                // Serialize the Data to a string to handle dynamic object
                string jsonString = JsonConvert.SerializeObject(result.Data);

                // Deserialize into a strongly-typed object
                var subjectResponse = JsonConvert.DeserializeObject<SubjectResponse>(jsonString);

                // Check if deserialization was successful and data exists
                if (subjectResponse?.Success == true && subjectResponse.Data != null)
                {
                    return subjectResponse.Data.Select(item => new SubjectInfo
                    {
                        KeyValue= Guid.Parse(item.Id.ToString())+"|"+ item.Name,
                        SubjectId = Guid.Parse(item.Id.ToString()), // Convert to Guid if your SubjectId is Guid
                        Name = item.Name,
                        IsSelected = false // Default to false
                    }).ToList();
                }

                // Return empty list if no data or unsuccessful
                return new List<SubjectInfo>();
            }
            catch (Exception ex)
            {
                // Return default subjects if there's an error
                return new List<SubjectInfo>();

                // Log the error
                // LogError(ex, "ConvertToSubjectInfoList");
            }
        }

        // You'll need this class to deserialize the JSON response
        public class SubjectResponse
        {
            public bool Success { get; set; }
            public List<DropdownItem> Data { get; set; }
        }
        private List<SelectListItem> ConvertToSelectList(JsonResult result)
        {
            try
            {
                // Serialize the Data to a string to handle dynamic object
                string jsonString = JsonConvert.SerializeObject(result.Data);

                // Deserialize into a strongly-typed object
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownResponse>(jsonString);

                // Check if deserialization was successful and data exists
                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    return dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }

                // Return empty list if no data or unsuccessful
                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
                // Log the error
                // LogError(ex, "ConvertToSelectList");
            }
        }

        private List<SelectListItem> ConvertToSelectListString(JsonResult result)
        {
            try
            {
                // Serialize the Data to a string to handle dynamic object
                string jsonString = JsonConvert.SerializeObject(result.Data);

                // Deserialize into a strongly-typed object
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownStringResponse>(jsonString);

                // Check if deserialization was successful and data exists
                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    return dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }

                // Return empty list if no data or unsuccessful
                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
                // Log the error
                // LogError(ex, "ConvertToSelectList");
            }
        }
        // GET: Student/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            Guid tenantId = CurrentTenantID;
            Guid sessionId = CurrentSessionID;
            var student = await _repository.GetStudentByIdAsync(id, tenantId, sessionId);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }
        [HttpGet]
        public async Task<ActionResult> GetStudentDetails(Guid id)
        {
            Guid tenantId = CurrentTenantID;
            Guid sessionId = CurrentSessionID;
            var student = await _repository.GetStudentByIdAsync(id, tenantId, sessionId);

            if (student == null)
            {
                return HttpNotFound();
            }

            // Return student details as JSON
            return Json(new
            {
                id = student.Basic.ID,
                fullName = student.Basic.FirstName,
                rollNo = student.Basic.RollNo,
                admissionNo = student.Basic.AdmsnNo,
                className = student.Basic.Class,
                fatherName = student.Family.FName,
                fatherAddress = student.Family.FAadhar
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetAllStudentsBySchholCode(string searchTerm)
        {
            Guid tenantId = CurrentTenantID;
            Guid sessionId = CurrentSessionID;
            List<StudentBasic> student = await _repository.GetFilteredStudentsBySchoolCodeAsync(Utils.ParseInt(CurrentTenantCode), searchTerm, tenantId, sessionId);

            if (student == null)
            {
                return HttpNotFound();
            }

            // Return student details as JSON
            return Json(new
            {
                student
            }, JsonRequestBehavior.AllowGet);
        }
        private async Task RepopulateDropdownLists(StudentViewModel model)
        {
            var dropdownController = new DropdownController();

            // Repopulate all dropdown lists
            var classesResult = dropdownController.GetClasses();
            var sectionsResult = dropdownController.GetSections();
            var FeeCatResult = dropdownController.GetFeeCategories();
            var FeeDisResult = dropdownController.GetFeeDiscountHeads();
            var RouteResult = dropdownController.GetRoutes();
            var VehicleResult = dropdownController.GetVehicle();
            var genderResult = dropdownController.GetGender();
            var bloodgroupResult = dropdownController.GetBloodGroup();
            var CategoryResult = dropdownController.GetCategory();
            var MotherResult = dropdownController.GetMotherTounge();
            var ReligionResult = dropdownController.GetReligion();
            var HouseResult = dropdownController.GetHouses();
            var TownResult = dropdownController.GetTown();
            var subjectResult = dropdownController.GetSubjects();
            var HostelResult = dropdownController.GetHostel();
            var batchResult= dropdownController.GetBatches();
            // Get students list for potential siblings dropdown
            var students = await _repository.GetAllStudentsBySchholCodeAsync(Utils.ParseInt(CurrentTenantCode));
            ViewBag.Students = students ?? new List<StudentBasic>();

            // Repopulate all dropdown lists in the model
            if (model.Basic != null)
            {
                model.Basic.BasicClassList = ConvertToSelectList(classesResult);
                model.Basic.BasicSectionList = ConvertToSelectList(sectionsResult);
                model.Basic.BasicDiscountList = ConvertToSelectList(FeeDisResult);
                model.Basic.BasicFeeList = ConvertToSelectList(FeeCatResult);
                model.Basic.BasicRouteList = ConvertToSelectList(RouteResult);
                model.Basic.BasicVehiclesList = ConvertToSelectList(VehicleResult);
                model.Basic.BasicGenderList = ConvertToSelectListString(genderResult);
                model.Basic.BasicBloodGroupList = ConvertToSelectListString(bloodgroupResult);
                model.Basic.BasicCategoryList = ConvertToSelectListString(CategoryResult);
                model.Basic.BasicMotherList = ConvertToSelectListString(MotherResult);
                model.Basic.BasicReligionList = ConvertToSelectListString(ReligionResult);
                model.Basic.BasicHouseList = ConvertToSelectList(HouseResult);
                model.Basic.BasicTownList = ConvertToSelectListString(TownResult);
                model.Basic.BasicHostelList = ConvertToSelectListString(HostelResult);
                model.Basic.BasicBatchList = ConvertToSelectListString(batchResult);
                model.Basic.BasicPickUpList = new List<SelectListItem>();
                // If subject list is empty, populate it
                if (model.Basic.Subjects == null || !model.Basic.Subjects.Any())
                {
                    model.Basic.Subjects = ConvertToSubjectInfoList(subjectResult);
                }
            }

            // Ensure there's at least one empty sibling entry for the UI
            if (model.Family != null && (model.Family.Siblings == null || !model.Family.Siblings.Any()))
            {
                model.Family.Siblings = new List<SiblingInfo> { new SiblingInfo() };
            }
        }
        // GET: Student/Create
        public async Task<ActionResult> Create()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();
            var FeeCatResult = new DropdownController().GetFeeCategories();
            var FeeDisResult = new DropdownController().GetFeeDiscountHeads();
            var RouteResult = new DropdownController().GetRoutes();
            var VehicleResult = new DropdownController().GetVehicle();
            var genderResult = new DropdownController().GetGender();
            var bloodgroupResult = new DropdownController().GetBloodGroup();
            var CategoryResult = new DropdownController().GetCategory();
            var MotherResult = new DropdownController().GetMotherTounge();
            var ReligionResult = new DropdownController().GetReligion();
            var HouseResult = new DropdownController().GetHouses();
            var TownResult = new DropdownController().GetTown();
            var subjectResult = new DropdownController().GetSubjects();
            var HostelResult = new DropdownController().GetHostel();
            var batchResult= new DropdownController().GetBatches();
            int nextAdmsnNo = await _repository.GetNextAdmissionNumberAsync(Utils.ParseInt(CurrentTenantCode).ToString());
            var students = await _repository.GetAllStudentsBySchholCodeAsync(Utils.ParseInt(CurrentTenantCode));
            ViewBag.Students = students ?? new List<StudentBasic>();
            var sublist = ConvertToSubjectInfoList(subjectResult);
            // Get the next roll number (maximum current roll number + 1)
            string nextRollNo = await _repository.GetNextRollNumberAsync(Utils.ParseInt(CurrentTenantCode).ToString());

            // Get the next serial number (maximum current serial number + 1)
            string nextSrNo = await _repository.GetNextSerialNumberAsync(Utils.ParseInt(CurrentTenantCode).ToString());
            if (string.IsNullOrEmpty(nextAdmsnNo.ToString()))
            {
                // Handle the case where no admission number is generated
                ModelState.AddModelError("", "Could not generate admission number. Please try again or contact administrator.");
                return View("Error");
            }

            // Initialize a new student with next available admission number
            var model = new StudentViewModel
            {
                Basic = new StudentBasic
                {
                    BasicClassList = ConvertToSelectList(classesResult),
                    BasicSectionList = ConvertToSelectList(sectionsResult),
                    BasicDiscountList = ConvertToSelectList(FeeDisResult),
                    BasicFeeList = ConvertToSelectList(FeeCatResult),
                    BasicRouteList = ConvertToSelectList(RouteResult),
                    BasicVehiclesList = ConvertToSelectList(VehicleResult),
                    BasicGenderList = ConvertToSelectListString(genderResult),
                    BasicBloodGroupList = ConvertToSelectListString(bloodgroupResult),
                    BasicCategoryList = ConvertToSelectListString(CategoryResult),
                    BasicMotherList = ConvertToSelectListString(MotherResult),
                    BasicReligionList = ConvertToSelectListString(ReligionResult),
                    BasicHouseList = ConvertToSelectList(HouseResult),
                    BasicTownList = ConvertToSelectListString(TownResult),
                    BasicHostelList = ConvertToSelectListString(HostelResult),
                    BasicBatchList = ConvertToSelectListString(batchResult),
                    BasicPickUpList = new List<SelectListItem>(),
                    Subjects = sublist,
                    RollNo = nextRollNo.ToString(),
                    SrNo = nextSrNo.ToString(),
                    AdmsnNo = nextAdmsnNo.ToString(),
                    LoginId = nextAdmsnNo.ToString(),
                    Password = nextAdmsnNo.ToString(),
                    SchoolCode = Utils.ParseInt(CurrentTenantCode),
                    EntryDate = DateTime.Now,
                    AdmsnDate = DateTime.Now.ToString("dd/MM/yyyy"),
                    IsActive = true,
                    IsLateFee = true,
                    Status = "Active"
                }
            };
            if (model.Family.Siblings == null || !model.Family.Siblings.Any())
            {
                model.Family.Siblings = new List<SiblingInfo> { new SiblingInfo() };
            }
            return View(model);
        }
        [HttpGet]
        public JsonResult GetStudentTransportConfig(Guid studentId)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    connection.Open();
                    var query = @"
                         SELECT 
                             AprilFee, MayFee, JuneFee, JulyFee, 
                             AugustFee, SeptemberFee, OctoberFee, NovemberFee, 
                             DecemberFee, JanuaryFee, FebruaryFee, MarchFee,
                             DefaultFeeAmount
                         FROM StudentTransportConfig 
                         WHERE TenantID = @TenantID 
                         AND SessionID = @SessionID 
                         AND StudentID = @StudentID
                         AND IsDeleted = 0 
                         AND IsActive = 1";

                    var config = connection.QueryFirstOrDefault(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID,
                        StudentID = studentId
                    });

                    return Json(new { success = true, data = config }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private void FixSiblingValidation(StudentViewModel model)
        {
            // Clear any existing errors for sibling properties
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Family.Siblings")).ToList())
            {
                ModelState.Remove(key);
            }

            // Ensure siblings collection exists
            if (model.Family.Siblings == null)
            {
                model.Family.Siblings = new List<SiblingInfo>();
            }

            // Filter out any invalid siblings (no valid ID or admission number)
            var validSiblings = new List<SiblingInfo>();

            if (model.Family.Siblings.Any())
            {
                foreach (var sibling in model.Family.Siblings)
                {
                    // Check if this is a valid sibling entry
                    if (!string.IsNullOrEmpty(sibling.AdmissionNo))
                    {
                        // Ensure SiblingId has a valid value (use 0 or another default if needed)
                        if (sibling.SiblingId <= 0)
                        {
                            sibling.SiblingId = 0; // Or any other default value
                        }

                        validSiblings.Add(sibling);
                    }
                }

                model.Family.Siblings = validSiblings;
            }
        }
        public async Task<TransportConfigViewModel> GetStudentTransportConfigAsync(Guid studentId, Guid sessionId, Guid tenantId)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                await connection.OpenAsync();
                var query = string.Format(@"
                SELECT 
    stc.AprilFee, stc.MayFee, stc.JuneFee, stc.JulyFee, 
    stc.AugustFee, stc.SeptemberFee, stc.OctoberFee, stc.NovemberFee, 
    stc.DecemberFee, stc.JanuaryFee, stc.FebruaryFee, stc.MarchFee,
    stc.DefaultFeeAmount,
    -- Check if fees have been submitted for each month
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Apr' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS AprilSubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'May' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS MaySubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Jun' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS JuneSubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Jul' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS JulySubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Aug' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS AugustSubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Sep' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS SeptemberSubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Oct' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS OctoberSubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Nov' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS NovemberSubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Dec' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS DecemberSubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Jan' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS JanuarySubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Feb' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS FebruarySubmitted,
    CASE WHEN EXISTS (SELECT 1 FROM FeeTransportFeeTbl fs WHERE fs.AdmissionNo = sib.AdmsnNo AND fs.FeeMonth = 'Mar' AND fs.SchoolCode = sib.SchoolCode) THEN 1 ELSE 0 END AS MarchSubmitted
FROM StudentTransportConfig stc
INNER JOIN StudentInfoBasic sib ON stc.StudentID = sib.StudentID
WHERE stc.TenantID = '{0}' 
AND stc.SessionID = '{1}' 
AND stc.StudentID = '{2}' 
AND stc.IsDeleted = 0 
AND stc.IsActive = 1", tenantId, sessionId, studentId);

                return await connection.QueryFirstOrDefaultAsync<TransportConfigViewModel>(
                    query,
                    new
                    {
                        TenantID = tenantId,
                        SessionID = sessionId,
                        StudentID = studentId
                    }
                );
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(StudentViewModel model, HttpPostedFileBase photoFile,
HttpPostedFileBase fatherPhotoFile, HttpPostedFileBase motherPhotoFile, HttpPostedFileBase guardianPhotoFile,
HttpPostedFileBase[] documentFiles, string[] selectedSubjects)
        {
            if (model?.Family?.IsSiblingInSameSchool == true)
            {
                // Fix any sibling validation issues
                FixSiblingValidation(model);
            }

            // Check if the required fields are provided
            if (string.IsNullOrWhiteSpace(model.Basic.AdmsnNo))
            {
                ModelState.AddModelError("Basic.AdmsnNo", "Admission Number is required");
            }

            if (string.IsNullOrWhiteSpace(model.Basic.Class))
            {
                ModelState.AddModelError("Basic.Class", "Class is required");
            }

            if (string.IsNullOrWhiteSpace(model.Basic.FirstName))
            {
                ModelState.AddModelError("Basic.FirstName", "First Name is required");
            }
            model.Basic.ClassId = Utils.ParseGuid(model.Basic.Class);
            model.Basic.SectionId = Utils.ParseGuid(model.Basic.Section);
            model.Basic.FeeCategoryId = Utils.ParseGuid(model.Basic.FeeCategory);
            model.Basic.FeeDiscountId = Utils.ParseGuid(model.Basic.DiscountCategory);
            model.Basic.HouseId = Utils.ParseGuid(model.Basic.House);
            model.Basic.SessionYear = Utils.ParseInt(CommonLogic.GetSessionValue(StringConstants.ActiveSessionYear));
            model.Basic.TenantCode = Utils.ParseInt(CommonLogic.GetSessionValue(StringConstants.TenantCode));
            model.Basic.TenantId = Utils.ParseGuid(CommonLogic.GetSessionValue(StringConstants.TenantID));
            model.Basic.SchoolCode = Utils.ParseInt(CommonLogic.GetSessionValue(StringConstants.SchoolCode));
            if (!ModelState.IsValid)
            {
                // Log all errors to help debug
                var errorList = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Key = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList();

                // Log to console or debug output
                foreach (var error in errorList)
                {
                    Logger.Error($"Error in {error.Key}: {string.Join(", ", error.Errors)}");
                    System.Diagnostics.Debug.WriteLine($"Error in {error.Key}: {string.Join(", ", error.Errors)}");

                    // Ensure all errors have meaningful messages for users
                    foreach (var errorMessage in error.Errors)
                    {
                        if (string.IsNullOrWhiteSpace(errorMessage))
                        {
                            // Add a user-friendly error message to ModelState
                            ModelState.AddModelError(error.Key, $"Invalid value for {error.Key.Split('.').Last()}");
                        }
                        // If error already has a message, it will be displayed automatically
                    }
                }

                // Add a summary error message if needed
               // ModelState.AddModelError("", "Please fix the errors and try again.");

                Logger.Error("Error in Student page Invalid Model");
                // Repopulate all dropdown lists
                await RepopulateDropdownLists(model);
                return View(model);
            }

            try
            {
                if (model.Basic.Subjects != null)
                {
                    foreach (var subject in model.Basic.Subjects)
                    {
                        subject.IsSelected = selectedSubjects != null && selectedSubjects.Contains(subject.KeyValue);
                    }
                }

                if (selectedSubjects != null && selectedSubjects.Length > 0)
                {
                    // This approach seems potentially incorrect - consider refactoring
                    // It's adding new subjects without proper IDs or details
                    foreach (var subjectId in selectedSubjects)
                    {
                        // Check if subject already exists in the list
                        if (!model.Basic.Subjects.Any(s => s.KeyValue == subjectId))
                        {
                            model.Basic.Subjects.Add(new SubjectInfo
                            {
                                IsElective = false,
                                TeacherName = string.Empty,
                                IsSelected = true,
                                KeyValue = subjectId,
                            });
                        }
                    }
                }

                // Handle student photo
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    string fileName = SaveFile(photoFile, model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "stu");
                    model.Basic.Photo = fileName;
                }

                // Handle Father photo
                if (fatherPhotoFile != null && fatherPhotoFile.ContentLength > 0)
                {
                    string fileName = SaveFile(fatherPhotoFile, model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "father");
                    model.Family.FPhoto = fileName;
                }

                // Handle Mother photo
                if (motherPhotoFile != null && motherPhotoFile.ContentLength > 0)
                {
                    string fileName = SaveFile(motherPhotoFile, model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "mother");
                    model.Family.MPhoto = fileName;
                }

                // Handle Guardian photo
                if (guardianPhotoFile != null && guardianPhotoFile.ContentLength > 0)
                {
                    string fileName = SaveFile(guardianPhotoFile, model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "guard");
                    model.Family.GPhoto = fileName;
                }

                // Handle document uploads
                if (documentFiles != null)
                {
                    for (int i = 0; i < documentFiles.Length && i < 6; i++)
                    {
                        if (documentFiles[i] != null && documentFiles[i].ContentLength > 0)
                        {
                            string fileName = SaveFilePdf(documentFiles[i], model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "doc" + (i+1));

                            // Set the document properties based on index
                            switch (i)
                            {
                                case 0:
                                    model.Other.UpldPath1 = fileName;
                                    break;
                                case 1:
                                    model.Other.UpldPath2 = fileName;
                                    break;
                                case 2:
                                    model.Other.UpldPath3 = fileName;
                                    break;
                                case 3:
                                    model.Other.UpldPath4 = fileName;
                                    break;
                                case 4:
                                    model.Other.UpldPath5 = fileName;
                                    break;
                                case 5:
                                    model.Other.UpldPath6 = fileName;
                                    break;
                            }
                        }
                    }
                }

                // Ensure sibling data is properly formatted
                if (model.Family.Siblings != null)
                {
                    // Filter out empty sibling entries
                    model.Family.Siblings = model.Family.Siblings
                        .Where(s => s != null && !string.IsNullOrEmpty(s.AdmissionNo))
                        .ToList();
                }

                // Ensure education details are properly formatted
                if (model.Other.EducationDetails != null)
                {
                    // Filter out empty education entries
                    model.Other.EducationDetails = model.Other.EducationDetails
                        .Where(e => e != null && !string.IsNullOrEmpty(e.Class))
                        .ToList();
                }

               

                // Ensure the primary keys are set
                model.Family.AdmsnNo = Utils.ParseInt(model.Basic.AdmsnNo);
                model.Family.SchoolCode = model.Basic.SchoolCode;
                model.Other.AdmsnNo = Utils.ParseInt(model.Basic.AdmsnNo);
                model.Other.SchoolCode = Utils.ParseInt(Utils.ParseInt(CurrentTenantCode));
                model.Basic.SessionId = CurrentSessionID;
                model.Basic.CreatedBy = CurrentTenantUserID;
                
                model.Basic.VechileId = Utils.ParseGuid(model.Family.VehicleNumber);
                model.Basic.VillegeId = Utils.ParseGuid(model.Family.StPermanentAddress);
                model.Basic.RouteId = Utils.ParseGuid(model.Family.Route); ;
                model.Basic.PickupId = Utils.ParseGuid(model.Basic.PickupPoint); 
                model.Basic.HostelId = Utils.ParseGuid(model.Family.HostelDetail);

                await _repository.SaveStudentAsync(model);
                TempData["SuccessMessage"] = "Student saved successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Detailed exception logging
                string errorMessage = $"Error saving student: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner exception: {ex.InnerException.Message}";
                }

                ModelState.AddModelError("", errorMessage);
                System.Diagnostics.Debug.WriteLine(errorMessage);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                Logger.Error("Error in student Create page" + errorMessage);

                // Repopulate all dropdown lists
                await RepopulateDropdownLists(model);
            }

            return View(model);
        }

        // GET: Student/Edit/5
        public async Task<ActionResult> Edit(Guid id)
        {
            Guid tenantId = CurrentTenantID;
            Guid sessionId = CurrentSessionID;

            // Get all dropdown data (same as Create)
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();
            var FeeCatResult = new DropdownController().GetFeeCategories();
            var FeeDisResult = new DropdownController().GetFeeDiscountHeads();
            var RouteResult = new DropdownController().GetRoutes();
            var VehicleResult = new DropdownController().GetVehicle();
            var genderResult = new DropdownController().GetGender();
            var bloodgroupResult = new DropdownController().GetBloodGroup();
            var CategoryResult = new DropdownController().GetCategory();
            var MotherResult = new DropdownController().GetMotherTounge();
            var ReligionResult = new DropdownController().GetReligion();
            var HouseResult = new DropdownController().GetHouses();
            var HostelResult = new DropdownController().GetHostel();
            var TownResult = new DropdownController().GetTown();
            var BatchResult = new DropdownController().GetBatches();
            var subjectResult = new DropdownController().GetSubjects(); // Added this line

            // Get the student record by ID
            var student = await _repository.GetStudentByIdAsync(id,tenantId,sessionId);

            student.Basic.DOB = student.Basic.vDOB;
            student.Basic.AdmsnDate = student.Basic.vAdm;
            student.Basic.LoginId = student.Basic.AdmsnNo;

            if (student == null)
            {
                return HttpNotFound();
            }

            // Initialize all dropdown lists
            student.Basic.BasicTownList = ConvertToSelectListString(TownResult);
            student.Basic.BasicClassList = ConvertToSelectList(classesResult);
            student.Basic.BasicSectionList = ConvertToSelectList(sectionsResult);
            student.Basic.BasicDiscountList = ConvertToSelectList(FeeDisResult);
            student.Basic.BasicFeeList = ConvertToSelectList(FeeCatResult);
            student.Basic.BasicRouteList = ConvertToSelectList(RouteResult);
            student.Basic.BasicVehiclesList = ConvertToSelectList(VehicleResult);
            student.Basic.BasicGenderList = ConvertToSelectListString(genderResult);
            student.Basic.BasicBloodGroupList = ConvertToSelectListString(bloodgroupResult);
            student.Basic.BasicCategoryList = ConvertToSelectListString(CategoryResult);
            student.Basic.BasicMotherList = ConvertToSelectListString(MotherResult);
            student.Basic.BasicReligionList = ConvertToSelectListString(ReligionResult);
            student.Basic.BasicHouseList = ConvertToSelectList(HouseResult);
            student.Basic.BasicHostelList = ConvertToSelectList(HostelResult);
            student.Basic.BasicBatchList = ConvertToSelectList(BatchResult);
            student.Basic.BasicPickUpList = new List<SelectListItem>();

            // Initialize subjects - either use existing or refresh from dropdown
            if (student.Basic.Subjects == null)
            {
                student.Basic.Subjects = ConvertToSubjectInfoList(subjectResult);
            }

            // Initialize siblings if missing
            if (student.Family.Siblings == null || !student.Family.Siblings.Any())
            {
                student.Family.Siblings = new List<SiblingInfo> { new SiblingInfo() };
            }

            // Get transport config
            student.TransportConfig = await GetStudentTransportConfigAsync(id, sessionId, tenantId);
            if (student.TransportConfig == null)
            {
                student.TransportConfig = new TransportConfigViewModel();
            }

            return View(student);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(StudentViewModel model, HttpPostedFileBase photoFile,
    HttpPostedFileBase fatherPhotoFile, HttpPostedFileBase motherPhotoFile, HttpPostedFileBase guardianPhotoFile,
    HttpPostedFileBase[] documentFiles, string[] selectedSubjects)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update subject selection
                    if (model.Basic.Subjects != null)
                    {
                        foreach (var subject in model.Basic.Subjects)
                        {
                            subject.IsSelected = selectedSubjects != null && selectedSubjects.Contains(subject.KeyValue);
                        }
                    }

                    if (selectedSubjects != null && selectedSubjects.Length > 0)
                    {
                        // This approach seems potentially incorrect - consider refactoring
                        // It's adding new subjects without proper IDs or details
                        foreach (var subjectId in selectedSubjects)
                        {
                            // Check if subject already exists in the list
                            if (!model.Basic.Subjects.Any(s => s.KeyValue == subjectId))
                            {
                                model.Basic.Subjects.Add(new SubjectInfo
                                {
                                    IsElective = false,
                                    TeacherName = string.Empty,
                                    IsSelected = true,
                                    KeyValue = subjectId,
                                });
                            }
                        }
                    }

                    // Handle photos
                    if (photoFile != null && photoFile.ContentLength > 0)
                    {
                        string fileName = SaveFile(photoFile, model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "stu");
                        model.Basic.Photo = fileName;
                    }

                    // Handle Father photo
                    if (fatherPhotoFile != null && fatherPhotoFile.ContentLength > 0)
                    {
                        string fileName = SaveFile(fatherPhotoFile, model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "father");
                        model.Family.FPhoto = fileName;
                    }

                    // Handle Mother photo
                    if (motherPhotoFile != null && motherPhotoFile.ContentLength > 0)
                    {
                        string fileName = SaveFile(motherPhotoFile, model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "mother");
                        model.Family.MPhoto = fileName;
                    }

                    // Handle Guardian photo
                    if (guardianPhotoFile != null && guardianPhotoFile.ContentLength > 0)
                    {
                        string fileName = SaveFile(guardianPhotoFile, model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "guard");
                        model.Family.GPhoto = fileName;
                    }

                    // Handle document uploads
                    if (documentFiles != null)
                    {
                        for (int i = 0; i < documentFiles.Length && i < 6; i++)
                        {
                            if (documentFiles[i] != null && documentFiles[i].ContentLength > 0)
                            {
                                string fileName = SaveFilePdf(documentFiles[i], model.Basic.AdmsnNo, model.Basic.TenantCode.ToString(), "doc" + (i + 1));

                                // Set the document properties based on index
                                switch (i)
                                {
                                    case 0:
                                        model.Other.UpldPath1 = fileName;
                                        break;
                                    case 1:
                                        model.Other.UpldPath2 = fileName;
                                        break;
                                    case 2:
                                        model.Other.UpldPath3 = fileName;
                                        break;
                                    case 3:
                                        model.Other.UpldPath4 = fileName;
                                        break;
                                    case 4:
                                        model.Other.UpldPath5 = fileName;
                                        break;
                                    case 5:
                                        model.Other.UpldPath6 = fileName;
                                        break;
                                }
                            }
                        }
                    }

                    // Ensure sibling data is properly formatted
                    if (model.Family.Siblings != null)
                    {

                        // Filter out empty sibling entries
                        model.Family.Siblings = model.Family.Siblings
                            .Where(s => s != null && !string.IsNullOrEmpty(s.AdmissionNo))
                            .ToList();
                    }

                    // Ensure education details are properly formatted
                    if (model.Other.EducationDetails != null)
                    {
                        // Filter out empty education entries
                        model.Other.EducationDetails = model.Other.EducationDetails
                            .Where(e => e != null && !string.IsNullOrEmpty(e.Class))
                            .ToList();
                    }

                    // Set IDs for dropdown selections
                    model.Basic.ClassId = Utils.ParseGuid(model.Basic.Class);
                    model.Basic.SectionId = Utils.ParseGuid(model.Basic.Section);
                    model.Basic.FeeCategoryId = Utils.ParseGuid(model.Basic.FeeCategory);
                    model.Basic.FeeDiscountId = Utils.ParseGuid(model.Basic.DiscountCategory);
                    model.Basic.HouseId = Utils.ParseGuid(model.Basic.House);
                    model.Basic.CreatedBy = Utils.ParseGuid(CurrentTenantUserID.ToString());
                    // Ensure the primary keys and related ids are set
                    model.Family.AdmsnNo = Utils.ParseInt(model.Basic.AdmsnNo);
                    model.Family.SchoolCode = model.Basic.SchoolCode;
                    model.Other.AdmsnNo = Utils.ParseInt(model.Basic.AdmsnNo);
                    model.Other.SchoolCode = model.Basic.SchoolCode;

                    model.Basic.VechileId = Utils.ParseGuid(model.Family.VehicleNumber);
                    model.Basic.VillegeId = Utils.ParseGuid(model.Family.StPermanentAddress);
                    model.Basic.RouteId = Utils.ParseGuid(model.Family.Route); ;
                    model.Basic.PickupId = Utils.ParseGuid(model.Basic.PickupPoint);
                    model.Basic.HostelId = Utils.ParseGuid(model.Family.HostelDetail);

                    await _repository.UpdateStudentAsync(model);
                    TempData["SuccessMessage"] = "Student updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Detailed exception logging
                    string errorMessage = $"Error updating student: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $" Inner exception: {ex.InnerException.Message}";
                    }

                    ModelState.AddModelError("", errorMessage);
                    System.Diagnostics.Debug.WriteLine(errorMessage);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                    Logger.Error("Error in student Edit page: " + errorMessage);
                }
            }
            else
            {
                await RepopulateDropdownLists(model);
                if (!ModelState.IsValid)
                {
                    // Log all errors to help debug
                    var errorList = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new
                        {
                            Key = x.Key,
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        })
                        .ToList();

                    // Log to console or debug output
                    foreach (var error in errorList)
                    {
                        Logger.Error($"Error in {error.Key}: {string.Join(", ", error.Errors)}");
                        System.Diagnostics.Debug.WriteLine($"Error in {error.Key}: {string.Join(", ", error.Errors)}");
                    }

                    // Add the errors back to ModelState for display
                    foreach (var error in errorList)
                    {
                        // If error doesn't have a specific message, add a clearer one
                        if (error.Errors.Any(e => string.IsNullOrWhiteSpace(e)))
                        {
                            ModelState.AddModelError(error.Key, $"Invalid value for {error.Key.Split('.').Last()}");
                            Logger.Error($"Invalid value for {error.Key.Split('.').Last()}");
                        }
                    }
                }
            }
            return View(model);
        }

        // GET: Student/Delete/5
        public async Task<ActionResult> Delete(Guid id)
        {
            Guid tenantId = CurrentTenantID;
            Guid sessionId = CurrentSessionID;
            var student = await _repository.GetStudentByIdAsync(id, tenantId, sessionId);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id,int code)
        {
            try
            {
                await _repository.DeleteStudentAsync(id, Utils.ParseInt(code));
                TempData["SuccessMessage"] = "Student deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting student: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        private string SaveFile(HttpPostedFileBase file, string id, string schoolcode, string type)
        {
            // Create directory if it doesn't exist
            string directoryPath = Server.MapPath(string.Format("/Documents/{0}/StudentProfile/", schoolcode));
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Generate a unique filename
            string fileName = string.Format("{0}_{1}.jpg", id, type);
            string filePath = System.IO.Path.Combine(directoryPath, fileName);

            // Save the file
            file.SaveAs(filePath);

            // Return the relative path
            return "/Documents/" + schoolcode + "/StudentProfile/" + fileName;
        }
        private string SaveFilePdf(HttpPostedFileBase file, string id, string schoolcode, string type)
        {
            // Create directory if it doesn't exist
            string directoryPath = Server.MapPath(string.Format("/Documents/{0}/StudentDocuments/", schoolcode));
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Generate a unique filename
            string fileName = string.Format("{0}_{1}.pdf", id, type);
            string filePath = System.IO.Path.Combine(directoryPath, fileName);

            // Save the file
            file.SaveAs(filePath);

            // Return the relative path
            return "/Documents/" + schoolcode + "/StudentDocuments/" + fileName;
        }

        private List<StudentViewInfoDTO> GetStudentIdModel(Guid sessionId, Guid? classId, Guid? sectionId, Guid tenantId)
        {
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    conn.Open();
                    var parameters = new DynamicParameters();
                    parameters.Add("@SessionID", sessionId);
                    parameters.Add("@TenantID", tenantId);
                    parameters.Add("@ClassID", classId);
                    parameters.Add("@SectionID", sectionId);
                    var result = conn.Query<StudentViewInfoDTO>("GetStudentInfoFromView", parameters, commandType: CommandType.StoredProcedure);
                    return result.ToList();


                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine("Error in GetFeeSummaryClassWise: " + ex.Message);
                // Return empty list in case of error
                return new List<StudentViewInfoDTO>();
            }
        }
        [HttpGet]
        public ActionResult IdCard(string selectedSession = null, string selectedClass = "ALL", string selectedSection = "ALL")
        {
            selectedClass = string.IsNullOrWhiteSpace(selectedClass) ? "ALL" : selectedClass;
            selectedSection = string.IsNullOrWhiteSpace(selectedSection) ? "ALL" : selectedSection;

            // If Guid is all zeros, treat it as "ALL"
            if (selectedClass == "00000000-0000-0000-0000-000000000000")
            {
                selectedClass = "ALL";
            }

            if (selectedSection == "00000000-0000-0000-0000-000000000000")
            {
                selectedSection = "ALL";
            }

            // If no session is selected, use current session
            if (string.IsNullOrWhiteSpace(selectedSession))
            {
                selectedSession = CurrentSessionID.ToString();
            }

            // Convert string parameters to GUIDs if necessary
            Guid sessionId;
            if (!Guid.TryParse(selectedSession, out sessionId))
            {
                sessionId = CurrentSessionID;
            }

            Guid? classId = null;
            if (selectedClass != "ALL" && Guid.TryParse(selectedClass, out Guid parsedClassId))
            {
                classId = parsedClassId;
            }

            Guid? sectionId = null;
            if (selectedSection != "ALL" && Guid.TryParse(selectedSection, out Guid parsedSectionId))
            {
                sectionId = parsedSectionId;
            }

            // Fetch dropdown data
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            // Get tenant info
            var tenantCode = Utils.ParseInt(CurrentTenantCode);
            var tenantId = CurrentTenantID;

            // Get session year display (e.g., "2024-25")
            string sessionYear = CurrentSessionYear.ToString();
            // Get the model with the selected filters
            var viewModel = new FeeSummaryViewModel
            {
                SelectedSession = selectedSession,
                SelectedClass = selectedClass,
                SelectedSection = selectedSection,
                SessionYear = sessionYear,
                // Populate dropdown lists
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
              
             
            };

            return View(viewModel);
            // Return the partial view with the report
   
        }
        [HttpPost]
        public ActionResult RefreshReport(string selectedClass = "ALL", string selectedSection = "ALL")
        {
            selectedClass = string.IsNullOrWhiteSpace(selectedClass) ? "ALL" : selectedClass;
            selectedSection = string.IsNullOrWhiteSpace(selectedSection) ? "ALL" : selectedSection;

            // If Guid is all zeros, treat it as "ALL"
            if (selectedClass == "00000000-0000-0000-0000-000000000000")
            {
                selectedClass = "ALL";
            }

            if (selectedSection == "00000000-0000-0000-0000-000000000000")
            {
                selectedSection = "ALL";
            }

            Guid? classId = null;
            if (selectedClass != "ALL" && Guid.TryParse(selectedClass, out Guid parsedClassId))
            {
                classId = parsedClassId;
            }

            Guid? sectionId = null;
            if (selectedSection != "ALL" && Guid.TryParse(selectedSection, out Guid parsedSectionId))
            {
                sectionId = parsedSectionId;
            }

            // Get tenant info
            var tenantId = CurrentTenantID;
            var sessionId = CurrentSessionID;

            // Get session year display (e.g., "2024-25")
            string sessionYear = CurrentSessionYear.ToString();

            // Get the model with the selected filters
            var viewModel = new FeeSummaryViewModel
            {
                SelectedClass = selectedClass,
                SelectedSection = selectedSection,
                SessionYear = sessionYear,
                ReportDate = DateTime.Now,
                StudentSummaries = GetStudentIdModel(sessionId, classId, sectionId, tenantId)
            };
           // DataTable dt = ConvertToDataTable(viewModel.StudentSummaries);
            // Return the partial view with the report
           // ReportDocument reportDocument = new ReportDocument();
            //string reportPath = Server.MapPath("~/Reports/CrystalReport/0/idcard.rpt");
            //reportDocument.Load(reportPath);

            // Set DataSource to our DataSet
            //reportDocument.SetDataSource(dt);

            // Store the ReportDocument in ViewBag for use in the partial view
           // ViewBag.ReportDocument = reportDocument;
            //Stream stream = reportDocument.ExportToStream(ExportFormatType.PortableDocFormat);

            // Reset stream position to the beginning
            //stream.Position = 0;

            // Return the stream with content type set to PDF, but without download
            //Response.Buffer = true;
            //Response.Clear();
            //Response.ContentType = "application/pdf";
           // Response.AddHeader("content-disposition", "inline");

            // Return the file stream without prompting for download
            //return new FileStreamResult(stream, "application/pdf");
            return PartialView("_IDViewer", viewModel);
        }
        
        [HttpGet]
        public ActionResult StreamReport(string id)
        {
            // Retrieve the report from session
            ReportDocument reportDocument = Session[id] as ReportDocument;

            if (reportDocument == null)
            {
                return HttpNotFound("Report not found");
            }

            try
            {
                // Export directly to stream - this is the simplified approach you're using
                Stream stream = reportDocument.ExportToStream(ExportFormatType.PortableDocFormat);

                // Reset stream position to the beginning
                stream.Position = 0;

                // Return the stream with content type set to PDF, but without download
                Response.Buffer = true;
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "inline");

                // Return the file stream without prompting for download
                return new FileStreamResult(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Error generating report: " + ex.Message);
            }
        }

        // Cleanup method is still useful
        [HttpGet]
        public ActionResult CleanupReport(string id)
        {
            ReportDocument reportDocument = Session[id] as ReportDocument;

            if (reportDocument != null)
            {
                reportDocument.Close();
                reportDocument.Dispose();
                Session.Remove(id);
            }

            return new EmptyResult();
        }
        private List<StudentFeeLedgerDTO> GetFilteredStudentsFeeLedger(Guid sessionId,Guid tenantId, Guid StudentId,Boolean ShowOnlyDueAmount=false)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@TenantID", tenantId);
                parameters.Add("@StudentID", StudentId);
                parameters.Add("@ShowOnlyDueAmount", ShowOnlyDueAmount);
                var students = connection.Query<StudentFeeLedgerDTO>(
                    "dbo.sp_AllStudentsFeeLedger",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return students;
            }
        }
        public ActionResult StudentLedger(Guid Id)
        {
            try
            {
                // Get data for the report
               // var students = new List<StudentCompleteInfoDto>();
               DataTable table = new DataTable();
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    connection.Open();

                    // Create the command for the stored procedure
                    using (var command = new SqlCommand("GetStudentFeeCompleteInfo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        command.Parameters.AddWithValue("@StudentId", Id);
                        command.Parameters.AddWithValue("@SessionId", CurrentSessionID);
                        command.Parameters.AddWithValue("@TenantId", CurrentTenantID);

                        // Execute and load results
                        using (var reader = command.ExecuteReader())
                        {
                            table.Load(reader);
                        }
                    }
                }

                var studentId = GetFilteredStudentsFeeLedger(CurrentSessionID, CurrentTenantID, Id);
                decimal reqfee = 0;
                decimal revfee = 0;
                decimal remainfee = 0;
                if (studentId != null)
                {
                    reqfee =(studentId[0].OldBalance
                    + studentId[0].AcademicFee
                    + studentId[0].TotalTransportFee
                    + studentId[0].TotalLateFee
                    - studentId[0].TotalMonthlyDiscount
                    - studentId[0].TotalHeadWiseDiscount
                    - studentId[0].TotalReceiptDiscount);
                    revfee = studentId[0].TotalPaid;
                    remainfee = studentId[0].FinalDueAmount;
                }
                // Create a report view model
                var reportModel = new ReportViewModel
                {
                    ReportType = "studentledger",
                    ReportPath = Server.MapPath("~/Reports/CrystalReport/0/studentledger.rpt"),
                    ReportData = table
                };

                // Set up common parameters
                string code = CommonLogic.GetSessionValue(StringConstants.TenantCode);
                string sessionprint = "Session ( " + CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint) + " )";
                string img = CommonLogic.GetSessionValue(StringConstants.LogoImg);
                string logoPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, img));
                string studentImagePath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetStudentImage(Utils.ToStringOrEmpty(table.Rows[0]["Photo"])));

                // Add parameters
                reportModel.Parameters.Add("prmreportlogo", logoPath);
                reportModel.Parameters.Add("prmtitle", CommonLogic.GetSessionValue(StringConstants.PrintTitle));
                reportModel.Parameters.Add("prmline1", CommonLogic.GetSessionValue(StringConstants.Line1));
                reportModel.Parameters.Add("prmline2", CommonLogic.GetSessionValue(StringConstants.Line2));
                reportModel.Parameters.Add("prmline3", CommonLogic.GetSessionValue(StringConstants.Line3));
                reportModel.Parameters.Add("prmline4", CommonLogic.GetSessionValue(StringConstants.Line4));
                reportModel.Parameters.Add("prmreportname", sessionprint);
                reportModel.Parameters.Add("prmreportitle1", studentImagePath);
                reportModel.Parameters.Add("prmreportitle2", "");
                reportModel.Parameters.Add("reqfee", reqfee);
                reportModel.Parameters.Add("revfee", revfee);
                reportModel.Parameters.Add("remainfee", remainfee);

                // Create a unique ID for the report
                string reportId = "studentLedger_" + DateTime.Now.Ticks;

                // Create the Crystal Report
                ReportDocument reportDocument = new ReportDocument();
                reportDocument.Load(reportModel.ReportPath);
                reportDocument.SetDataSource(reportModel.ReportData);

                // Set parameters
                foreach (var parameter in reportModel.Parameters)
                {
                    reportDocument.SetParameterValue(parameter.Key, parameter.Value);
                }

                // Store in session
                Session[reportId] = reportDocument;

                // Always return the partial view for consistent behavior
                ViewBag.ReportId = reportId;
                ViewBag.ReportTitle = "Student Ledger";
                ViewBag.AllowPrint = true;
                ViewBag.AllowExport = true;

                return PartialView("_ReportViewer", reportId);
            }
            catch (Exception ex)
            {
                // Log the exception
                // logger.Error(ex, "Error generating student ledger report");

                return PartialView("_Error", "Error generating report: " + ex.Message);
            }
        }
        public async Task<ActionResult> StudentAdmitCardPrint(Guid? examId, Guid? classId = null, Guid? sectionId = null)
        {
            try
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;
                var currentUserId = CurrentTenantUserID;

                // Get the data
                var examDetails = await _examinationRepository.GetExamDetailsAsync(examId ?? Guid.Empty, classId, sectionId, sessionId, tenantCode);
                var students = await _examinationRepository.GetExamStudentsAsync(examId ?? Guid.Empty, classId, sectionId, sessionId, tenantCode);

                // Check if students exist
                if (!students.Any())
                    return Content("Error: No students found for this exam");

                // Convert to DataTables
                var examDetailsDataTable = Utils.ConvertToDataTable(examDetails);
                var studentsDataTable = Utils.ConvertToDataTable(students);

                // Process student photos if the column exists
                if (studentsDataTable.Columns.Contains("PhotoPath") || studentsDataTable.Columns.Contains("PhotoPath"))
                {
                    string photoColumnName = studentsDataTable.Columns.Contains("PhotoPath") ? "PhotoPath" : "PhotoPath";

                    foreach (DataRow row in studentsDataTable.Rows)
                    {
                        if (row[photoColumnName] != null && !string.IsNullOrEmpty(row[photoColumnName].ToString()))
                        {
                            string physicalPath = Server.MapPath(row[photoColumnName].ToString());
                            if (!System.IO.File.Exists(physicalPath))
                            {
                                row[photoColumnName] = Server.MapPath("/img/default.jpg");
                            }
                            else
                            {
                                row[photoColumnName] = physicalPath;
                            }
                        }
                        else
                        {
                            row[photoColumnName] = Server.MapPath("/img/default.jpg");
                        }
                    }
                }

                // Create view model
                var viewModel = new AdmitCardViewModel
                {
                    ExamDetailsDataTable = examDetailsDataTable,
                    StudentsDataTable = studentsDataTable,
                    ExamId = examId?.ToString() ?? Guid.Empty.ToString()
                };

                return PartialView("_AdmitCard", viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                // Logger.LogError(ex);
                return Content($"Error generating admit card: {ex.Message}");
            }
        }
        public ActionResult StudentAdmitCard()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();
            var examsResult = new DropdownController().GetExamSchedule();
            Guid sessionId = CurrentSessionID;
            var tenantCode = Utils.ParseInt(CurrentTenantCode);
            var tenantId = CurrentTenantID;

            // Build the view model
            var viewModel = new FeeCollectionSearchRequest
            {
                ClassList = ConvertToSelectListSelect(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
                ExamList = ConvertToSelectListSelect(examsResult)
                // Get fee summary data

            };
            return View(viewModel);
        }
        private List<SelectListItem> ConvertToSelectListSelect(JsonResult result)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownResponse>(jsonString);

                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    var newList = new List<DropdownItem>
                    {
                        new DropdownItem
                        {
                            Id = Guid.Empty,
                            Name = "-- Select --"
                        }
                    };

                    newList.AddRange(dropdownResponse.Data);

                    return newList.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }

                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                // Log the error
                return new List<SelectListItem>();
            }
        }
        [HttpGet]
        public ActionResult BusWiseList()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();
            var routeResult = new DropdownController().GetRoutes();
            var vechileResult = new DropdownController().GetVehicle();
            Guid sessionId = CurrentSessionID;
            var tenantCode = Utils.ParseInt(CurrentTenantCode);
            var tenantId = CurrentTenantID;

            // Build the view model
            var viewModel = new TransportStudentSearchRequest
            {
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
                RouteList = ConvertToSelectListDefault(routeResult),
                VehicleList = ConvertToSelectListDefault(vechileResult)
                // Get fee summary data

            };
            return View(viewModel);
        }

        public ActionResult BusWiseListSearch(Guid? classId = null, Guid? sectionId = null, Guid? RouteId = null, Guid? VechileId = null)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    db.Open();

                    var p = new DynamicParameters();
                    p.Add("@TenantID", CurrentTenantID);
                    p.Add("@SessionID", CurrentSessionID);

                    // Optional filters using TryParse
                    p.Add("@ClassId", classId);
                    p.Add("@SectionId", sectionId);
                    p.Add("@VechileId", VechileId);
                    p.Add("@RouteId", RouteId);
                    p.Add("@PickupId", null);

                    var result = db.Query<TransportStudentDto>("sp_SearchStudentTransportInfo", p, commandType: CommandType.StoredProcedure).ToList();

                    var studentsDataTable = Utils.ConvertToDataTable(result);

                    // Process student photos if the column exists
                    if (studentsDataTable.Columns.Contains("Pic") || studentsDataTable.Columns.Contains("Pic"))
                    {
                        string photoColumnName = studentsDataTable.Columns.Contains("Pic") ? "Pic" : "Pic";

                        foreach (DataRow row in studentsDataTable.Rows)
                        {
                            if (row[photoColumnName] != null && !string.IsNullOrEmpty(row[photoColumnName].ToString()))
                            {
                                string physicalPath = Server.MapPath(row[photoColumnName].ToString());
                                if (!System.IO.File.Exists(physicalPath))
                                {
                                    row[photoColumnName] = Server.MapPath("/img/default.jpg");
                                }
                                else
                                {
                                    row[photoColumnName] = physicalPath;
                                }
                            }
                            else
                            {
                                row[photoColumnName] = Server.MapPath("/img/default.jpg");
                            }
                        }
                    }
                    // Reload dropdowns for the view
                    var viewModel = new AdmitCardViewModel
                    {
                        StudentsDataTable = studentsDataTable,
                    };

                    return PartialView("_BusList", viewModel);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while retrieving transport data.");
                return Content("Error: No students found ");
            }
        }

        [HttpGet]
        public ActionResult RouteWiseList()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();
            var routeResult = new DropdownController().GetRoutes();
            Guid sessionId = CurrentSessionID;
            var tenantCode = Utils.ParseInt(CurrentTenantCode);
            var tenantId = CurrentTenantID;

            // Build the view model
            var viewModel = new TransportStudentSearchRequest
            {
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
                RouteList = ConvertToSelectListDefault(routeResult),

            };
            return View(viewModel);
        }

        public ActionResult RouteWiseListSearch(Guid? classId = null, Guid? sectionId = null, Guid? RouteId = null, Guid? VechileId = null)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    db.Open();

                    var p = new DynamicParameters();
                    p.Add("@TenantID", CurrentTenantID);
                    p.Add("@SessionID", CurrentSessionID);

                    // Optional filters using TryParse
                    p.Add("@ClassId", classId);
                    p.Add("@SectionId", sectionId);
                    p.Add("@VechileId", null);
                    p.Add("@RouteId", RouteId);
                    p.Add("@PickupId", null);

                    var result = db.Query<TransportStudentDto>("sp_SearchStudentTransportInfo", p, commandType: CommandType.StoredProcedure).ToList();

                    var studentsDataTable = Utils.ConvertToDataTable(result);

                    // Process student photos if the column exists
                    if (studentsDataTable.Columns.Contains("Pic") || studentsDataTable.Columns.Contains("Pic"))
                    {
                        string photoColumnName = studentsDataTable.Columns.Contains("Pic") ? "Pic" : "Pic";

                        foreach (DataRow row in studentsDataTable.Rows)
                        {
                            if (row[photoColumnName] != null && !string.IsNullOrEmpty(row[photoColumnName].ToString()))
                            {
                                string physicalPath = Server.MapPath(row[photoColumnName].ToString());
                                if (!System.IO.File.Exists(physicalPath))
                                {
                                    row[photoColumnName] = Server.MapPath("/img/default.jpg");
                                }
                                else
                                {
                                    row[photoColumnName] = physicalPath;
                                }
                            }
                            else
                            {
                                row[photoColumnName] = Server.MapPath("/img/default.jpg");
                            }
                        }
                    }
                    // Reload dropdowns for the view
                    var viewModel = new AdmitCardViewModel
                    {
                        StudentsDataTable = studentsDataTable,
                    };

                    return PartialView("_RouteList", viewModel);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while retrieving transport data.");
                return Content("Error: No students found ");
            }
        }
        [HttpGet]
        public ActionResult VillageWiseList()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();
            var townResult = new DropdownController().GetTown();
            Guid sessionId = CurrentSessionID;
            var tenantCode = Utils.ParseInt(CurrentTenantCode);
            var tenantId = CurrentTenantID;

            // Build the view model
            var viewModel = new TransportStudentSearchRequest
            {
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
                TownList = ConvertToSelectListDefault(townResult),

            };
            return View(viewModel);
        }

        public ActionResult VillageWiseListSearch(Guid? classId = null, Guid? sectionId = null, Guid? townId = null)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    db.Open();

                    var p = new DynamicParameters();
                    p.Add("@TenantID", CurrentTenantID);
                    p.Add("@SessionID", CurrentSessionID);

                    // Optional filters using TryParse
                    p.Add("@ClassId", classId);
                    p.Add("@SectionId", sectionId);
                    p.Add("@TownId", townId);
                    p.Add("@VechileId", null);
                    p.Add("@RouteId", null);
                    p.Add("@PickupId", null);

                    var result = db.Query<TransportStudentDto>("sp_SearchStudentTransportInfo", p, commandType: CommandType.StoredProcedure).ToList();

                    var studentsDataTable = Utils.ConvertToDataTable(result);

                    // Process student photos if the column exists
                    if (studentsDataTable.Columns.Contains("Pic") || studentsDataTable.Columns.Contains("Pic"))
                    {
                        string photoColumnName = studentsDataTable.Columns.Contains("Pic") ? "Pic" : "Pic";

                        foreach (DataRow row in studentsDataTable.Rows)
                        {
                            if (row[photoColumnName] != null && !string.IsNullOrEmpty(row[photoColumnName].ToString()))
                            {
                                string physicalPath = Server.MapPath(row[photoColumnName].ToString());
                                if (!System.IO.File.Exists(physicalPath))
                                {
                                    row[photoColumnName] = Server.MapPath("/img/default.jpg");
                                }
                                else
                                {
                                    row[photoColumnName] = physicalPath;
                                }
                            }
                            else
                            {
                                row[photoColumnName] = Server.MapPath("/img/default.jpg");
                            }
                        }
                    }
                    // Reload dropdowns for the view
                    var viewModel = new AdmitCardViewModel
                    {
                        StudentsDataTable = studentsDataTable,
                    };

                    return PartialView("_VillageWiseList", viewModel);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while retrieving transport data.");
                return Content("Error: No students found ");
            }
        }

    }
}

public class AdmitCardViewModel
{
    public DataTable ExamDetailsDataTable { get; set; }
    public DataTable StudentsDataTable { get; set; }
    public string ExamId { get; set; }
}
public class TransportStudentSearchRequest
{
    public TransportStudentSearchRequest()
    {
        ClassList = new List<SelectListItem>();
        SectionList = new List<SelectListItem>();
        VehicleList = new List<SelectListItem>();
        RouteList = new List<SelectListItem>();
        PickupList = new List<SelectListItem>();
        TownList = new List<SelectListItem>();
    }

    public Guid TenantId { get; set; }
    public Guid SessionId { get; set; }

    public string SelectedClass { get; set; }
    public string SelectedSection { get; set; }
    public string SelectedVehicle { get; set; }
    public string SelectedRoute { get; set; }
    public string SelectedPickup { get; set; }
    public string SelectedTown { get; set; }
    public List<SelectListItem> ClassList { get; set; }
    public List<SelectListItem> TownList { get; set; }
    public List<SelectListItem> SectionList { get; set; }
    public List<SelectListItem> VehicleList { get; set; }
    public List<SelectListItem> RouteList { get; set; }
    public List<SelectListItem> PickupList { get; set; }

    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
}
public class TransportStudentDto
{
    public Guid StudentId { get; set; }
    public int SchoolCode { get; set; }

    public string ROUTE { get; set; }
    public string D1 { get; set; }
    public string D2 { get; set; }
    public string D3 { get; set; }
    public string D4 { get; set; }
    public string D5 { get; set; }
    public string Bus { get; set; }
    public string PickupName { get; set; }
    public string CLASS { get; set; }
    public string Section { get; set; }

    public string STUDENT { get; set; }
    public string FATHER { get; set; }
    public string MOTHER { get; set; }
    public string CONTACT { get; set; }
    public string DOB { get; set; }
    public string GEN { get; set; }
    public string ADDRESS { get; set; }
    public string Pic { get; set; }

    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? VechileId { get; set; }
    public Guid? PickupId { get; set; }
    public Guid? RouteId { get; set; }
    public Guid? VillegeId { get; set; }


}