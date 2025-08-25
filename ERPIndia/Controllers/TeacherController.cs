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
        public async Task<ActionResult> Index(Guid? ClassId = null, Guid? SectionId = null, string viewType = "Active")
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            ViewBag.Classes = ConvertToSelectList(classesResult);
            ViewBag.Sections = ConvertToSelectList(sectionsResult);
            ViewBag.ViewType = viewType;

            var teachers = await _repository.GetAllTeachersAsync(
                Utils.ParseInt(CurrentSchoolCode),
                CurrentSessionID.ToString(),
                ClassId,
                SectionId,
                viewType
            );

            return View(teachers);
        }

        // GET: Teacher/Create
        public async Task<ActionResult> Create()
        {
            var model = new TeacherViewModel();

            // Generate next Teacher ID
            string nextTeacherId = await _repository.GetNextTeacherIdAsync(Utils.ParseInt(CurrentTenantCode).ToString());

            // Populate dropdown lists
            await PopulateDropdowns(model);

            // Set default values
            model.Basic.TeacherCode = nextTeacherId;
            model.Basic.LoginId = nextTeacherId;
            model.Basic.Password = nextTeacherId;
            model.Basic.SchoolCode = Utils.ParseInt(CurrentTenantCode);
            model.Basic.DateOfJoining = DateTime.Now.ToString("dd/MM/yyyy");
            model.Basic.IsActive = true;
            model.Basic.Status = "Active";

            return View(model);
        }

        // POST: Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TeacherViewModel model,
            HttpPostedFileBase photoFile,
            HttpPostedFileBase resumeFile,
            HttpPostedFileBase joiningLetterFile)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.Basic.TeacherCode))
            {
                ModelState.AddModelError("Basic.TeacherCode", "Teacher ID is required");
            }

            if (string.IsNullOrWhiteSpace(model.Basic.FirstName))
            {
                ModelState.AddModelError("Basic.FirstName", "First Name is required");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                // Handle photo upload
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    string fileName = SaveFile(photoFile, model.Basic.TeacherCode,
                        model.Basic.SchoolCode.ToString(), "teacher");
                    model.Basic.Photo = fileName;
                }

                // Handle resume upload
                if (resumeFile != null && resumeFile.ContentLength > 0)
                {
                    string fileName = SaveDocument(resumeFile, model.Basic.TeacherCode,
                        model.Basic.SchoolCode.ToString(), "resume");
                    model.Documents.ResumePath = fileName;
                }

                // Handle joining letter upload
                if (joiningLetterFile != null && joiningLetterFile.ContentLength > 0)
                {
                    string fileName = SaveDocument(joiningLetterFile, model.Basic.TeacherCode,
                        model.Basic.SchoolCode.ToString(), "joining");
                    model.Documents.JoiningLetterPath = fileName;
                }

                // Set system fields
                model.Basic.TenantId = CurrentTenantID;
                model.Basic.SessionId = CurrentSessionID;
                model.Basic.CreatedBy = CurrentTenantUserID;

                // Convert string IDs to GUIDs
                model.Basic.ClassId = Utils.ParseGuid(model.Basic.ClassId).ToString();
                model.Basic.SectionId = Utils.ParseGuid(model.Basic.SectionId).ToString();
                model.Basic.SubjectId = Utils.ParseGuid(model.Basic.SubjectId).ToString();
                model.Basic.RouteId = Utils.ParseGuid(model.Basic.RouteId).ToString();
                model.Basic.VehicleId = Utils.ParseGuid(model.Basic.VehicleId).ToString();
                model.Basic.PickupId = Utils.ParseGuid(model.Basic.PickupId).ToString();
                model.Basic.HostelId = Utils.ParseGuid(model.Basic.HostelId).ToString();

                // Save teacher
                await _repository.SaveTeacherAsync(model);

                TempData["SuccessMessage"] = "Teacher saved successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving teacher: {ex.Message}");
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
            return View(teacher);
        }

        // POST: Teacher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TeacherViewModel model,
            HttpPostedFileBase photoFile,
            HttpPostedFileBase resumeFile,
            HttpPostedFileBase joiningLetterFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file uploads (same as Create)
                    if (photoFile != null && photoFile.ContentLength > 0)
                    {
                        string fileName = SaveFile(photoFile, model.Basic.TeacherCode,
                            model.Basic.SchoolCode.ToString(), "teacher");
                        model.Basic.Photo = fileName;
                    }

                    if (resumeFile != null && resumeFile.ContentLength > 0)
                    {
                        string fileName = SaveDocument(resumeFile, model.Basic.TeacherCode,
                            model.Basic.SchoolCode.ToString(), "resume");
                        model.Documents.ResumePath = fileName;
                    }

                    if (joiningLetterFile != null && joiningLetterFile.ContentLength > 0)
                    {
                        string fileName = SaveDocument(joiningLetterFile, model.Basic.TeacherCode,
                            model.Basic.SchoolCode.ToString(), "joining");
                        model.Documents.JoiningLetterPath = fileName;
                    }

                    // Update teacher
                    await _repository.UpdateTeacherAsync(model);

                    TempData["SuccessMessage"] = "Teacher updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating teacher: {ex.Message}");
                }
            }

            await PopulateDropdowns(model);
            return View(model);
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

        // Helper Methods
        private async Task PopulateDropdowns(TeacherViewModel model)
        {
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
                new SelectListItem { Value = "", Text = "Select" },
                new SelectListItem { Value = "Active", Text = "Active" },
                new SelectListItem { Value = "Inactive", Text = "Inactive" },
                new SelectListItem { Value = "On Leave", Text = "On Leave" },
                new SelectListItem { Value = "Resigned", Text = "Resigned" }
            };

            model.Basic.ContractTypeList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select" },
                new SelectListItem { Value = "Permanent", Text = "Permanent" },
                new SelectListItem { Value = "Temporary", Text = "Temporary" },
                new SelectListItem { Value = "Contract", Text = "Contract" },
                new SelectListItem { Value = "Probation", Text = "Probation" }
            };

            model.Basic.WorkShiftList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select" },
                new SelectListItem { Value = "Morning", Text = "Morning" },
                new SelectListItem { Value = "Afternoon", Text = "Afternoon" },
                new SelectListItem { Value = "Evening", Text = "Evening" },
                new SelectListItem { Value = "Night", Text = "Night" }
            };

            model.Basic.PickupList = new List<SelectListItem>();
        }

        private List<SelectListItem> ConvertToSelectList(JsonResult result)
        {
            try
            {
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<DropdownResponse>(jsonString);

                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    return dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }

                return new List<SelectListItem>();
            }
            catch
            {
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> ConvertToSelectListString(JsonResult result)
        {
            try
            {
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<DropdownStringResponse>(jsonString);

                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    return dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }

                return new List<SelectListItem>();
            }
            catch
            {
                return new List<SelectListItem>();
            }
        }

        private string SaveFile(HttpPostedFileBase file, string id, string schoolcode, string type)
        {
            string directoryPath = Server.MapPath($"/Documents/{schoolcode}/TeacherProfile/");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fileName = $"{id}_{type}.jpg";
            string filePath = Path.Combine(directoryPath, fileName);
            file.SaveAs(filePath);

            return $"/Documents/{schoolcode}/TeacherProfile/{fileName}";
        }

        private string SaveDocument(HttpPostedFileBase file, string id, string schoolcode, string type)
        {
            string directoryPath = Server.MapPath($"/Documents/{schoolcode}/TeacherDocuments/");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{id}_{type}{extension}";
            string filePath = Path.Combine(directoryPath, fileName);
            file.SaveAs(filePath);

            return $"/Documents/{schoolcode}/TeacherDocuments/{fileName}";
        }

        // AJAX Methods
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
    }
}