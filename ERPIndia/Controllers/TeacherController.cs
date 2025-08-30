// TeacherController.cs
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
using Dapper;
using ERPIndia.TeacherManagement.Models;
using ERPIndia.TeacherManagement.Repository;
using ERPIndia.Utilities;
using Newtonsoft.Json;

namespace ERPIndia.Controllers
{
    public class TeacherController : BaseController
    {
        private readonly TeacherRepository _repository;
        private readonly DropdownController _dropdownController;

        public TeacherController()
        {
            _repository = new TeacherRepository();
            _dropdownController = new DropdownController();
        }

        // GET: Teacher
        public async Task<ActionResult> Index(Guid? ClassId = null, Guid? SectionId = null,
            Guid? DesignationId = null, string viewType = "Active")
        {
            try
            {
                // Get dropdowns for filters
                var classesResult = _dropdownController.GetClasses();
                var sectionsResult = _dropdownController.GetSections();
                var designations = await _repository.GetDesignationsAsync(Utils.ParseInt(CurrentTenantCode));

                ViewBag.Classes = ConvertToSelectList(classesResult);
                ViewBag.Sections = ConvertToSelectList(sectionsResult);
                ViewBag.Designations = ConvertDynamicToSelectList(designations, "DesignationID", "DesignationName");
                ViewBag.ViewType = viewType;
                ViewBag.DesignationId = DesignationId;

                // Get teachers based on filters
                var teachers = await _repository.GetAllTeachersAsync(
                    Utils.ParseInt(CurrentTenantCode),
                    CurrentSessionID,
                    ClassId,
                    SectionId,
                    DesignationId,
                    viewType
                );

                return View(teachers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading teachers: " + ex.Message;
                return View(new List<TeacherBasic>());
            }
        }

        // GET: Teacher/Create
        public async Task<ActionResult> Create()
        {
            var model = new TeacherViewModel();

            // Generate next Teacher Code
            string nextTeacherCode = await _repository.GetNextTeacherCodeAsync(Utils.ParseInt(CurrentTenantCode));

            // Populate dropdown lists
            await PopulateDropdowns(model);

            // Set default values
            model.Basic.TeacherCode = nextTeacherCode;
            model.Basic.LoginId = nextTeacherCode;
            model.Basic.Password = nextTeacherCode;
            model.Basic.SchoolCode = Utils.ParseInt(CurrentSchoolCode);
            model.Basic.TenantCode = Utils.ParseInt(CurrentTenantCode);
            model.Basic.TenantId = CurrentTenantID;
            model.Basic.SessionId = CurrentSessionID;
            model.Basic.DateOfJoining = DateTime.Now;  // Set as DateTime
            model.Basic.IsActive = true;
            model.Basic.Status = "Active";
            model.Basic.CreatedBy = CurrentTenantUserID;

            // Initialize sub-objects
            model.Payroll = model.Payroll ?? new TeacherPayroll { EffectiveDate = DateTime.Now };  // Set as DateTime
            model.Leaves = model.Leaves ?? new TeacherLeaves();
            model.BankDetails = model.BankDetails ?? new TeacherBankDetails();
            model.SocialMedia = model.SocialMedia ?? new TeacherSocialMedia();
            model.Documents = new List<TeacherDocument>();

            return View(model);
        }

        // POST: Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TeacherViewModel model,
    HttpPostedFileBase photoFile,
    HttpPostedFileBase[] documentFiles,
    FormCollection form)
        {
            try
            {
                // Initialize sub-objects if null
                model.Payroll = model.Payroll ?? new TeacherPayroll();
                model.Leaves = model.Leaves ?? new TeacherLeaves();
                model.BankDetails = model.BankDetails ?? new TeacherBankDetails();
                model.SocialMedia = model.SocialMedia ?? new TeacherSocialMedia();

                var documents = new List<TeacherDocument>();

                // Map Employee Name to FirstName and LastName
                string employeeName = form["EmployeeName"];
                if (!string.IsNullOrWhiteSpace(employeeName))
                {
                    var nameParts = employeeName.Trim().Split(' ');
                    if (nameParts.Length > 0)
                    {
                        model.Basic.FirstName = nameParts[0];
                        if (nameParts.Length > 1)
                        {
                            model.Basic.LastName = string.Join(" ", nameParts.Skip(1));
                        }
                    }
                }

                // Map HR Organization fields
                model.Basic.DesignationId = Utils.ParseGuid(form["Basic.DesignationId"]);
                model.Basic.DepartmentId = Utils.ParseGuid(form["DepartmentId"]);
                model.Basic.BranchId = Utils.ParseGuid(form["BranchId"]);

                // Get the designation name to check if it's a teacher
                string designationName = "";
                if (!string.IsNullOrEmpty(model.Basic.DesignationId.ToString()) && model.Basic.DesignationId != Guid.Empty)
                {
                    designationName = await GetNameFromId("HR_MST_Designation", "DesignationID", "DesignationName", model.Basic.DesignationId.ToString());
                    model.Basic.DesignationName = designationName;
                }

                // Check if designation is teacher-related
                bool isTeacher = !string.IsNullOrEmpty(designationName) &&
                                designationName.ToLower().Contains("teacher");

                // CONDITIONAL VALIDATION FOR TEACHER FIELDS
                if (isTeacher)
                {
                    // Parse ClassId and SubjectId from form
                    var classIdString = form["Basic.ClassId"];
                    var subjectIdString = form["Basic.SubjectId"];

                    // Validate ClassId for teachers
                    if (string.IsNullOrEmpty(classIdString) || classIdString == "Select Class" || classIdString == Guid.Empty.ToString())
                    {
                        ModelState.AddModelError("Basic.ClassId", "Class is required for teachers");
                    }
                    else
                    {
                        model.Basic.ClassId = Utils.ParseGuid(classIdString);
                        if (model.Basic.ClassId != Guid.Empty)
                        {
                            model.Basic.ClassName = await GetNameFromId("AcademicClassMaster", "ClassId", "ClassName", model.Basic.ClassId.ToString());
                        }
                    }

                    // Validate SubjectId for teachers
                    if (string.IsNullOrEmpty(subjectIdString) || subjectIdString == "Select Subject" || subjectIdString == Guid.Empty.ToString())
                    {
                        ModelState.AddModelError("Basic.SubjectId", "Subject is required for teachers");
                    }
                    else
                    {
                        model.Basic.SubjectId = Utils.ParseGuid(subjectIdString);
                        if (model.Basic.SubjectId != Guid.Empty)
                        {
                            model.Basic.SubjectName = await GetNameFromId("AcademicSubjectMaster", "SubjectId", "SubjectName", model.Basic.SubjectId.ToString());
                        }
                    }
                }
                else
                {
                    // For non-teachers, set ClassId and SubjectId to empty/null
                    model.Basic.ClassId = Guid.Empty;
                    model.Basic.SubjectId = Guid.Empty;
                    model.Basic.ClassName = null;
                    model.Basic.SubjectName = null;
                }

                // Map denormalized name fields for other fields
                if (!string.IsNullOrEmpty(model.Basic.DepartmentId.ToString()) && model.Basic.DepartmentId != Guid.Empty)
                {
                    model.Basic.DepartmentName = form["DepartmentId.Text"] ??
                        await GetNameFromId("HR_MST_Department", "DepartmentID", "DepartmentName", model.Basic.DepartmentId.ToString());
                }
                if (!string.IsNullOrEmpty(model.Basic.EmployeeTypeId.ToString()) && model.Basic.EmployeeTypeId != Guid.Empty)
                {
                    model.Basic.EmployeeTypeName = form["EmployeeTypeId.Text"] ??
                        await GetNameFromId("HR_MST_EmployeeType", "EmployeeTypeID", "EmployeeTypeName", model.Basic.EmployeeTypeId.ToString());
                }
                if (!string.IsNullOrEmpty(model.Basic.BranchId.ToString()) && model.Basic.BranchId != Guid.Empty)
                {
                    model.Basic.BranchName = form["BranchId.Text"] ??
                        await GetNameFromId("HR_MST_Branch", "BranchID", "BranchName", model.Basic.BranchId.ToString());
                }
                if (!string.IsNullOrEmpty(model.Basic.ManagerId.ToString()) && model.Basic.ManagerId != Guid.Empty)
                {
                    model.Basic.ManagerName = await GetManagerName(model.Basic.ManagerId.ToString());
                }

                // Map SectionId if provided
                if (!string.IsNullOrEmpty(model.Basic.SectionId.ToString()) && model.Basic.SectionId != Guid.Empty)
                {
                    model.Basic.SectionName = await GetNameFromId("AcademicSectionMaster", "SectionId", "SectionName", model.Basic.SectionId.ToString());
                }

                // Map other fields
                model.Basic.Religion = form["Basic.Religion"];
                model.Basic.ExperienceDetails = form["Basic.ExperienceDetails"];
                model.Basic.OtherSubject = form["Basic.OtherSubject"];

                // Map government IDs
                model.Basic.AadharNumber = form["Basic.AadharNumber"];
                model.Basic.PANNumber = form["Basic.PANNumber"];
                model.Basic.UANNo = form["Basic.UANNo"];
                model.Basic.NPSNo = form["Basic.NPSNo"];
                model.Basic.PFNO = form["Basic.PFNO"];

                // Parse time fields
                if (!string.IsNullOrEmpty(form["Basic.TimeIn"]))
                {
                    if (TimeSpan.TryParse(form["Basic.TimeIn"], out TimeSpan timeIn))
                    {
                        model.Basic.TimeIn = timeIn.ToString(@"hh\:mm");
                    }
                }

                if (!string.IsNullOrEmpty(form["Basic.TimeOut"]))
                {
                    if (TimeSpan.TryParse(form["Basic.TimeOut"], out TimeSpan timeOut))
                    {
                        model.Basic.TimeOut = timeOut.ToString(@"hh\:mm");
                    }
                }

                // Map payroll fields
                if (!string.IsNullOrEmpty(form["Payroll.BasicSalary"]))
                {
                    if (decimal.TryParse(form["Payroll.BasicSalary"], out decimal salary))
                    {
                        model.Payroll.BasicSalary = salary;
                    }
                }
                if (!string.IsNullOrEmpty(form["Payroll.LateFinePerHour"]))
                {
                    if (decimal.TryParse(form["Payroll.LateFinePerHour"], out decimal lateFine))
                    {
                        model.Payroll.LateFinePerHour = lateFine;
                    }
                }

                model.Payroll.PayrollNote = form["Payroll.PayrollNote"];
                model.Payroll.ContractType = form["ContractType"];
                model.Payroll.WorkShift = form["WorkShift"];
                model.Payroll.WorkLocation = form["Payroll.WorkLocation"];
                model.Payroll.EPFNo = form["Payroll.EPFNo"];

                // Parse EffectiveDate for Payroll
                if (!string.IsNullOrEmpty(form["EffectiveDate"]))
                {
                    model.Payroll.EffectiveDate = ParseDate(form["EffectiveDate"]);
                }
                else
                {
                    model.Payroll.EffectiveDate = DateTime.Now;
                }

                // Map leaves
                if (!string.IsNullOrEmpty(form["Leaves.MedicalLeaves"]))
                    int.TryParse(form["Leaves.MedicalLeaves"], out int medicalLeaves);
                if (!string.IsNullOrEmpty(form["Leaves.CasualLeaves"]))
                    int.TryParse(form["Leaves.CasualLeaves"], out int casualLeaves);
                if (!string.IsNullOrEmpty(form["Leaves.MaternityLeaves"]))
                    int.TryParse(form["Leaves.MaternityLeaves"], out int maternityLeaves);
                if (!string.IsNullOrEmpty(form["Leaves.SickLeaves"]))
                    int.TryParse(form["Leaves.SickLeaves"], out int sickLeaves);
                if (!string.IsNullOrEmpty(form["Leaves.EarnedLeaves"]))
                    int.TryParse(form["Leaves.EarnedLeaves"], out int earnedLeaves);

                // Map bank fields
                model.BankDetails.UPIID = form["BankDetails.UPIID"];

                // Ensure Status is set
                model.Basic.Status = form["Basic.Status"] ?? "Active";

                // CONDITIONAL VALIDATION: Date of Leaving for Inactive status
                if (model.Basic.Status == "Inactive" || model.Basic.Status == "InActive")
                {
                    if (string.IsNullOrEmpty(form["Payroll.DateOfLeaving"]))
                    {
                        ModelState.AddModelError("Payroll.DateOfLeaving", "Date of Leaving is required when status is Inactive");
                    }
                    else
                    {
                        model.Payroll.DateOfLeaving = ParseDate(form["Payroll.DateOfLeaving"]);
                    }
                }
                else if (!string.IsNullOrEmpty(form["Payroll.DateOfLeaving"]))
                {
                    model.Payroll.DateOfLeaving = ParseDate(form["Payroll.DateOfLeaving"]);
                }

                // Basic required field validations
                if (string.IsNullOrWhiteSpace(model.Basic.TeacherCode))
                {
                    ModelState.AddModelError("Basic.TeacherCode", "Employee Code is required");
                }

                if (string.IsNullOrWhiteSpace(model.Basic.FirstName))
                {
                    ModelState.AddModelError("Basic.FirstName", "Employee Name is required");
                }

                if (!ModelState.IsValid)
                {
                    var errorList = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new
                        {
                            Key = x.Key,
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        })
                        .ToList();

                    var errorMessages = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e =>
                            $"{x.Key}: {(string.IsNullOrWhiteSpace(e.ErrorMessage) ? $"Invalid value for {x.Key.Split('.').Last()}" : e.ErrorMessage)}"
                        ))
                        .ToList();

                    string allErrors = string.Join(" | ", errorMessages);
                    Logger.Error($"Validation Errors: {allErrors}");

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
                        }
                    }

                    await PopulateDropdowns(model);
                    return View(model);
                }

                // Handle photo upload
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    string fileName = SaveFile(photoFile, model.Basic.TeacherCode,
                        model.Basic.SchoolCode.ToString(), "photo");
                    model.Basic.Photo = fileName;
                }

                // Handle multiple document uploads
                if (documentFiles != null && documentFiles.Length > 0)
                {
                    foreach (var file in documentFiles.Where(f => f != null && f.ContentLength > 0))
                    {
                        string documentType = DetermineDocumentType(file.FileName, form);
                        string fileName = SaveDocument(file, model.Basic.TeacherCode,
                            model.Basic.SchoolCode.ToString(), documentType);

                        documents.Add(new TeacherDocument
                        {
                            DocumentType = documentType,
                            DocumentTitle = Path.GetFileNameWithoutExtension(file.FileName),
                            DocumentPath = fileName,
                            FileSize = file.ContentLength,
                            MimeType = file.ContentType,
                            UploadDate = DateTime.Now
                        });
                    }
                }
                model.Documents = documents;

                // Set system fields
                model.Basic.TenantId = CurrentTenantID;
                model.Basic.TenantCode = Utils.ParseInt(CurrentTenantCode);
                model.Basic.SessionId = CurrentSessionID;
                model.Basic.CreatedBy = CurrentTenantUserID;
                model.Basic.SchoolCode = Utils.ParseInt(CurrentSchoolCode);
                model.Basic.IsDeleted = false;
                model.Basic.IsActive = (model.Basic.Status == "Active");

                // Parse other GUID fields (handle null/empty values)
                model.Basic.RouteId = Utils.ParseGuid(model.Basic.RouteId);
                model.Basic.VehicleId = Utils.ParseGuid(model.Basic.VehicleId);
                model.Basic.PickupId = Utils.ParseGuid(model.Basic.PickupId);
                model.Basic.HostelId = Utils.ParseGuid(model.Basic.HostelId);
                model.Basic.SectionId = Utils.ParseGuid(model.Basic.SectionId);

                // Parse and set dates from form
                if (!string.IsNullOrEmpty(form["Basic.DateOfJoining"]))
                {
                    model.Basic.DateOfJoining = ParseDate(form["Basic.DateOfJoining"]);
                }

                if (!string.IsNullOrEmpty(form["Basic.DateOfBirth"]))
                {
                    model.Basic.DateOfBirth = ParseDate(form["Basic.DateOfBirth"]);
                }

                if (!string.IsNullOrEmpty(form["EndDate"]))
                {
                    model.Payroll.EndDate = ParseDate(form["EndDate"]);
                }

                // Save teacher
                string teacherId = await _repository.SaveTeacherAsync(model);

                TempData["SuccessMessage"] = "Employee saved successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving employee: {ex.Message}");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        // GET: Teacher/Edit/5
        public async Task<ActionResult> Edit(Guid id)
        {
            var teacher = await _repository.GetTeacherByIdAsync(id, CurrentTenantID, CurrentSessionID);

            if (teacher == null)
            {
                return HttpNotFound();
            }

            await PopulateDropdowns(teacher);

            // Dates are already DateTime? in the model, no conversion needed for display
            // The view should handle date formatting

            return View(teacher);
        }

        // POST: Teacher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TeacherViewModel model,
            HttpPostedFileBase photoFile,
            HttpPostedFileBase[] documentFiles,
            FormCollection form)
        {
            try
            {
                // Initialize sub-objects if null
                model.Payroll = model.Payroll ?? new TeacherPayroll();
                model.Leaves = model.Leaves ?? new TeacherLeaves();
                model.BankDetails = model.BankDetails ?? new TeacherBankDetails();
                model.SocialMedia = model.SocialMedia ?? new TeacherSocialMedia();

                var documents = new List<TeacherDocument>();

                // Map Employee Name to FirstName and LastName
                string employeeName = form["EmployeeName"];
                if (!string.IsNullOrWhiteSpace(employeeName))
                {
                    var nameParts = employeeName.Trim().Split(' ');
                    if (nameParts.Length > 0)
                    {
                        model.Basic.FirstName = nameParts[0];
                        if (nameParts.Length > 1)
                        {
                            model.Basic.LastName = string.Join(" ", nameParts.Skip(1));
                        }
                    }
                }

                // Map HR Organization fields
                model.Basic.DesignationId = Utils.ParseGuid(form["DesignationId"]);
                model.Basic.DepartmentId = Utils.ParseGuid(form["DepartmentId"]);
                model.Basic.EmployeeTypeId = Utils.ParseGuid(form["EmployeeTypeId"]);
                model.Basic.BranchId = Utils.ParseGuid(form["BranchId"]);
                model.Basic.ManagerId = Utils.ParseGuid(form["ManagerId"]);

                // Map denormalized name fields (same as Create)
                if (!string.IsNullOrEmpty(model.Basic.DesignationId.ToString()))
                {
                    model.Basic.DesignationName = form["DesignationId.Text"] ??
                        await GetNameFromId("HR_MST_Designation", "DesignationID", "DesignationName", model.Basic.DesignationId.ToString());
                }
                if (!string.IsNullOrEmpty(model.Basic.DepartmentId.ToString()))
                {
                    model.Basic.DepartmentName = form["DepartmentId.Text"] ??
                        await GetNameFromId("HR_MST_Department", "DepartmentID", "DepartmentName", model.Basic.DepartmentId.ToString());
                }
                if (!string.IsNullOrEmpty(model.Basic.EmployeeTypeId.ToString()))
                {
                    model.Basic.EmployeeTypeName = form["EmployeeTypeId.Text"] ??
                        await GetNameFromId("HR_MST_EmployeeType", "EmployeeTypeID", "EmployeeTypeName", model.Basic.EmployeeTypeId.ToString());
                }
                if (!string.IsNullOrEmpty(model.Basic.BranchId.ToString()))
                {
                    model.Basic.BranchName = form["BranchId.Text"] ??
                        await GetNameFromId("HR_MST_Branch", "BranchID", "BranchName", model.Basic.BranchId.ToString());
                }
                if (!string.IsNullOrEmpty(model.Basic.ManagerId.ToString()))
                {
                    model.Basic.ManagerName = await GetManagerName(model.Basic.ManagerId.ToString());
                }

                // Map other fields
                model.Basic.Religion = form["Religion"];
                model.Basic.ExperienceDetails = form["ExperienceDetails"];
                model.Basic.OtherSubject = form["OtherSubject"];
                model.Basic.Status = form["Status"] ?? "Active";

                // Map government IDs
                model.Basic.AadharNumber = form["AadharNumber"];
                model.Basic.PANNumber = form["PANNumber"];
                model.Basic.UANNo = form["UANNo"];
                model.Basic.NPSNo = form["NPSNo"];
                model.Basic.PFNO = form["PFNO"];

                // Parse time fields
                if (!string.IsNullOrEmpty(form["TimeIn"]))
                {
                    if (TimeSpan.TryParse(form["TimeIn"], out TimeSpan timeIn))
                    {
                        model.Basic.TimeIn = timeIn.ToString(@"hh\:mm");
                    }
                }

                if (!string.IsNullOrEmpty(form["TimeOut"]))
                {
                    if (TimeSpan.TryParse(form["TimeOut"], out TimeSpan timeOut))
                    {
                        model.Basic.TimeOut = timeOut.ToString(@"hh\:mm");
                    }
                }

                // Map payroll fields
                if (!string.IsNullOrEmpty(form["BasicSalary"]))
                {
                    if (decimal.TryParse(form["BasicSalary"], out decimal salary))
                    {
                        model.Payroll.BasicSalary = salary;
                    }
                }
                if (!string.IsNullOrEmpty(form["LateFinePerHour"]))
                {
                    if (decimal.TryParse(form["LateFinePerHour"], out decimal lateFine))
                    {
                        model.Payroll.LateFinePerHour = lateFine;
                    }
                }
                model.Payroll.PayrollNote = form["PayrollNote"];
                model.Payroll.ContractType = form["ContractType"];
                model.Payroll.WorkShift = form["WorkShift"];
                model.Payroll.WorkLocation = form["WorkLocation"];
                model.Payroll.EPFNo = form["EPFNo"];

                // Map bank fields
                model.BankDetails.UPIID = form["UPIID"];

                // Handle photo upload
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    string fileName = SaveFile(photoFile, model.Basic.TeacherCode,
                        model.Basic.SchoolCode.ToString(), "photo");
                    model.Basic.Photo = fileName;
                }

                // Handle multiple document uploads
                if (documentFiles != null && documentFiles.Length > 0)
                {
                    foreach (var file in documentFiles.Where(f => f != null && f.ContentLength > 0))
                    {
                        string documentType = DetermineDocumentType(file.FileName, form);
                        string fileName = SaveDocument(file, model.Basic.TeacherCode,
                            model.Basic.SchoolCode.ToString(), documentType);

                        documents.Add(new TeacherDocument
                        {
                            DocumentType = documentType,
                            DocumentTitle = Path.GetFileNameWithoutExtension(file.FileName),
                            DocumentPath = fileName,
                            FileSize = file.ContentLength,
                            MimeType = file.ContentType,
                            UploadDate = DateTime.Now
                        });
                    }
                }

                if (documents.Any())
                {
                    model.Documents = documents;
                }

                // Parse and set dates from form
                if (!string.IsNullOrEmpty(form["DateOfJoining"]))
                {
                    model.Basic.DateOfJoining = ParseDate(form["DateOfJoining"]);
                }

                if (!string.IsNullOrEmpty(form["DateOfBirth"]))
                {
                    model.Basic.DateOfBirth = ParseDate(form["DateOfBirth"]);
                }

                if (!string.IsNullOrEmpty(form["DateOfLeaving"]))
                {
                    model.Payroll.DateOfLeaving = ParseDate(form["DateOfLeaving"]);
                }

                if (!string.IsNullOrEmpty(form["EffectiveDate"]))
                {
                    model.Payroll.EffectiveDate = ParseDate(form["EffectiveDate"]);
                }

                if (!string.IsNullOrEmpty(form["EndDate"]))
                {
                    model.Payroll.EndDate = ParseDate(form["EndDate"]);
                }

                // Set modified fields
                model.Basic.ModifiedBy = CurrentTenantUserID;
                model.Basic.ModifiedDate = DateTime.Now;

                // Update teacher
                await _repository.UpdateTeacherAsync(model);

                TempData["SuccessMessage"] = "Teacher updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating teacher: {ex.Message}");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        // GET: Teacher/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            var teacher = await _repository.GetTeacherByIdAsync(id, CurrentTenantID, CurrentSessionID);

            if (teacher == null)
            {
                return HttpNotFound();
            }

            return View(teacher);
        }

        // GET: Teacher/Delete/5
        public async Task<ActionResult> Delete(Guid id)
        {
            var teacher = await _repository.GetTeacherByIdAsync(id, CurrentTenantID, CurrentSessionID);

            if (teacher == null)
            {
                return HttpNotFound();
            }

            return View(teacher);
        }

        // POST: Teacher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _repository.DeleteTeacherAsync(id, Utils.ParseInt(CurrentTenantCode));
                TempData["SuccessMessage"] = "Teacher deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting teacher: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        #region Helper Methods

        private async Task PopulateDropdowns(TeacherViewModel model)
        {
            // Existing dropdowns
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();
            var subjectsResult = _dropdownController.GetSubjects();
            var routeResult = _dropdownController.GetRoutes();
            var vehicleResult = _dropdownController.GetVehicle();
            var hostelResult = _dropdownController.GetHostel();
            var genderResult = _dropdownController.GetGender();
            var bloodGroupResult = _dropdownController.GetBloodGroup();

            model.Basic.ClassList = ConvertToSelectList(classesResult);
            model.Basic.SectionList = ConvertToSelectList(sectionsResult);
            model.Basic.SubjectList = ConvertToSelectList(subjectsResult);
            model.Basic.RouteList = ConvertToSelectList(routeResult);
            model.Basic.VehicleList = ConvertToSelectList(vehicleResult);
            model.Basic.HostelList = ConvertToSelectList(hostelResult);
            model.Basic.GenderList = ConvertToSelectListString(genderResult);
            model.Basic.BloodGroupList = ConvertToSelectListString(bloodGroupResult);

            // New HR dropdowns
            int tenantCode = Utils.ParseInt(CurrentTenantCode);
            var designations = await _repository.GetDesignationsAsync(tenantCode);
            var departments = await _repository.GetDepartmentsAsync(tenantCode);
            var employeeTypes = await _repository.GetEmployeeTypesAsync(tenantCode);
            var branches = await _repository.GetBranchesAsync(tenantCode);
            var managers = await _repository.GetManagersAsync(tenantCode);

            model.Basic.DesignationList = ConvertDynamicToSelectList(designations, "DesignationID", "DesignationName");
            model.Basic.DepartmentList = ConvertDynamicToSelectList(departments, "DepartmentID", "DepartmentName");
            model.Basic.EmployeeTypeList = ConvertDynamicToSelectList(employeeTypes, "EmployeeTypeID", "EmployeeTypeName");
            model.Basic.BranchList = ConvertDynamicToSelectList(branches, "BranchID", "BranchName");
            model.Basic.ManagerList = ConvertDynamicToSelectList(managers, "ManagerId", "ManagerName");

            // Static dropdowns
            model.Basic.MaritalStatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select" },
                new SelectListItem { Value = "Single", Text = "Single" },
                new SelectListItem { Value = "Married", Text = "Married" },
                new SelectListItem { Value = "Divorced", Text = "Divorced" },
                new SelectListItem { Value = "Widowed", Text = "Widowed" }
            };

            model.Basic.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Active", Text = "Active" },
                new SelectListItem { Value = "Inactive", Text = "Inactive" }
              
            };

            // Contract Type dropdown for Payroll
            if (model.Payroll != null)
            {
                model.Payroll.ContractTypeList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Select" },
                    new SelectListItem { Value = "Permanent", Text = "Permanent" },
                    new SelectListItem { Value = "Contract", Text = "Contract" },
                    new SelectListItem { Value = "Temporary", Text = "Temporary" },
                    new SelectListItem { Value = "Probation", Text = "Probation" }
                };

                model.Payroll.WorkShiftList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Select" },
                    new SelectListItem { Value = "Morning", Text = "Morning" },
                    new SelectListItem { Value = "Evening", Text = "Evening" },
                    new SelectListItem { Value = "Night", Text = "Night" },
                    new SelectListItem { Value = "Flexible", Text = "Flexible" }
                };
            }

            // Initialize PickupList if null
            model.Basic.PickupList = model.Basic.PickupList ?? new List<SelectListItem>();
        }

        private List<SelectListItem> ConvertDynamicToSelectList(IEnumerable<dynamic> items, string valueField, string textField)
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "", Text = "Select" } };

            if (items != null)
            {
                foreach (var item in items)
                {
                    var dict = (IDictionary<string, object>)item;
                    list.Add(new SelectListItem
                    {
                        Value = dict[valueField]?.ToString(),
                        Text = dict[textField]?.ToString()
                    });
                }
            }

            return list;
        }

        private List<SelectListItem> ConvertToSelectList(JsonResult result)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownResponse>(jsonString);

                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    var list = new List<SelectListItem>();

                    list.AddRange(dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }));

                    return list;
                }
                else
                {
                    return new List<SelectListItem> { new SelectListItem { Value = "", Text = "Select" } };
                }
            }
            catch
            {
                return new List<SelectListItem> { new SelectListItem { Value = "", Text = "Select" } };
            }
        }

        private List<SelectListItem> ConvertToSelectListString(JsonResult result)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownStringResponse>(jsonString);

                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    var list = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "", Text = "Select" }
                    };

                    list.AddRange(dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Id,
                        Text = item.Name
                    }));

                    return list;
                }

                return new List<SelectListItem> { new SelectListItem { Value = "", Text = "Select" } };
            }
            catch
            {
                return new List<SelectListItem> { new SelectListItem { Value = "", Text = "Select" } };
            }
        }

        private string SaveFile(HttpPostedFileBase file, string id, string schoolcode, string type)
        {
            string directoryPath = Server.MapPath($"~/Documents/{schoolcode}/TeacherProfile/");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{id}_{type}_{DateTime.Now.Ticks}{extension}";
            string filePath = Path.Combine(directoryPath, fileName);
            file.SaveAs(filePath);

            return $"/Documents/{schoolcode}/TeacherProfile/{fileName}";
        }

        private string SaveDocument(HttpPostedFileBase file, string id, string schoolcode, string type)
        {
            string directoryPath = Server.MapPath($"~/Documents/{schoolcode}/TeacherDocuments/");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{id}_{type}_{DateTime.Now.Ticks}{extension}";
            string filePath = Path.Combine(directoryPath, fileName);
            file.SaveAs(filePath);

            return $"/Documents/{schoolcode}/TeacherDocuments/{fileName}";
        }

        private string DetermineDocumentType(string fileName, FormCollection form)
        {
            // Logic to determine document type based on filename or form input
            string lowerFileName = fileName.ToLower();

            if (lowerFileName.Contains("resume") || lowerFileName.Contains("cv"))
                return TeacherConstants.DocumentTypes.Resume;
            if (lowerFileName.Contains("joining") || lowerFileName.Contains("appointment"))
                return TeacherConstants.DocumentTypes.JoiningLetter;
            if (lowerFileName.Contains("experience"))
                return TeacherConstants.DocumentTypes.ExperienceCertificate;
            if (lowerFileName.Contains("education") || lowerFileName.Contains("degree"))
                return TeacherConstants.DocumentTypes.EducationCertificate;
            if (lowerFileName.Contains("identity") || lowerFileName.Contains("aadhar") || lowerFileName.Contains("pan"))
                return TeacherConstants.DocumentTypes.IdentityProof;
            if (lowerFileName.Contains("address"))
                return TeacherConstants.DocumentTypes.AddressProof;

            return TeacherConstants.DocumentTypes.Other;
        }

        private async Task<string> GetNameFromId(string tableName, string idColumn, string nameColumn, string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                var query = $"SELECT {nameColumn} FROM {tableName} WHERE {idColumn} = @Id";
                return await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = id });
            }
        }

        private async Task<string> GetManagerName(string managerId)
        {
            if (string.IsNullOrEmpty(managerId))
                return null;

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                var query = "SELECT CONCAT(FirstName, ' ', ISNULL(LastName, '')) FROM HR_MST_Teacher WHERE TeacherId = @Id";
                return await connection.QueryFirstOrDefaultAsync<string>(query, new { Id = managerId });
            }
        }

        private string ConvertToGuidString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Guid.TryParse(value, out Guid guid) && guid != Guid.Empty)
                return guid.ToString();

            return null;
        }

        // New method to parse dates properly
        private DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // Try different date formats
            string[] formats = {
                "dd/MM/yyyy",
                "yyyy-MM-dd",
                "MM/dd/yyyy",
                "dd-MM-yyyy",
                "dd/MM/yyyy HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }

            // Try default parse as last resort
            if (DateTime.TryParse(dateString, out DateTime defaultResult))
            {
                return defaultResult;
            }

            return null;
        }

        // Method to format DateTime? to string for display (if needed in views)
        private string FormatDateForDisplay(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.ToString("dd/MM/yyyy");

            return string.Empty;
        }

        #endregion

        #region AJAX Methods

        [HttpGet]
        public JsonResult GetSectionsByClass(Guid classId)
        {
            try
            {
                var sectionsResult = _dropdownController.GetSectionByTenant(classId.ToString(), classId.ToString());
                var sections = ConvertToSelectList(sectionsResult);
                return Json(sections, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<SelectListItem>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetPickupPointsByRoute(Guid routeId)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    var pickupPoints = connection.Query<dynamic>(
                        "SELECT PickupId as Id, PickupName as Name FROM PickupPoints WHERE RouteId = @RouteId AND IsActive = 1",
                        new { RouteId = routeId }
                    ).ToList();

                    return Json(new { success = true, data = pickupPoints }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetTeachersByClass(Guid classId)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    var teachers = connection.Query<dynamic>(
                        @"SELECT TeacherId as Id, CONCAT(FirstName, ' ', ISNULL(LastName, '')) as Name 
                          FROM HR_MST_Teacher 
                          WHERE ClassId = @ClassId AND IsActive = 1 AND ISNULL(IsDeleted, 0) = 0",
                        new { ClassId = classId }
                    ).ToList();

                    return Json(new { success = true, data = teachers }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetDepartmentsByDesignation(Guid designationId)
        {
            try
            {
                // Logic to get departments based on designation if needed
                var departments = await _repository.GetDepartmentsAsync(Utils.ParseInt(CurrentTenantCode));
                return Json(new { success = true, data = departments }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }

}