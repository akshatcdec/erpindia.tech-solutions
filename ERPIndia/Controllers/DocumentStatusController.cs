using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ERPIndia.StudentManagement.Repository;
using ERPIndia.Controllers.Examination;
using Newtonsoft.Json;

namespace ERPIndia.Controllers
{
    public class DocumentStatusController : BaseController
    {
        private readonly StudentRepository _repository;
        private readonly DropdownController _dropdownController;

        public DocumentStatusController()
        {
            _repository = new StudentRepository();
            _dropdownController = new DropdownController();
        }

        // GET: DocumentStatus
        public ActionResult Index()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            var model = new DocumentStatusViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult)
            };

            return View(model);
        }

        // Get students with photo and document status
        [HttpPost]
        public JsonResult GetStudentDocumentStatus(string className, string section)
        {
            try
            {
                var students = _repository.GetStudentsByClassAndSection(className, section);

                var studentDocStatus = students.Select(s => new
                {
                    StudentId = s.StudentId,
                    AdmNo = s.AdmsnNo,
                    Class = s.ClassName + " - " + s.SectionName,
                    Gender = s.Gender,
                    StudentName = string.Format("{0} {1}", s.FirstName, s.LastName).Trim(),
                    FatherName = s.FatherName,
                    MotherName = s.MotherName,
                    Mobile = s.Mobile,

                    // Get existing photos from database fields or file system
                    StudentPhoto = !string.IsNullOrEmpty(s.Photo) ? s.Photo :
                                  GetPhotoPath(s.AdmsnNo.ToString(), "stu", CurrentSchoolCode),
                    FatherPhoto = !string.IsNullOrEmpty(s.FatherPhoto) ? s.FatherPhoto :
                                 GetPhotoPath(s.AdmsnNo.ToString(), "father", CurrentSchoolCode),
                    MotherPhoto = !string.IsNullOrEmpty(s.MotherPhoto) ? s.MotherPhoto :
                                 GetPhotoPath(s.AdmsnNo.ToString(), "mother", CurrentSchoolCode),
                    GuardianPhoto = !string.IsNullOrEmpty(s.GuardianPhoto) ? s.GuardianPhoto :
                                   GetPhotoPath(s.AdmsnNo.ToString(), "guard", CurrentSchoolCode),

                    // Document status - replace these with your actual document fields
                    // You can modify these field names based on your database schema
                    Docs1 = CheckDocumentStatus(s.Doc1),      // Birth Certificate
                    Docs2 = CheckDocumentStatus(s.Doc2),            // Aadhar Card
                    Docs3 = CheckDocumentStatus(s.Doc3),   // Transfer Certificate
                    Docs4 = CheckDocumentStatus(s.Doc4)     // Medical Certificate
                }).ToList();

                // Calculate summary statistics
                var summary = new
                {
                    TotalStudents = studentDocStatus.Count,
                    CompleteProfiles = studentDocStatus.Count(s =>
                        s.Docs1 == "Yes" && s.Docs2 == "Yes" &&
                        s.Docs3 == "Yes" && s.Docs4 == "Yes" &&
                        !string.IsNullOrEmpty(s.StudentPhoto) &&
                        !string.IsNullOrEmpty(s.FatherPhoto) &&
                        !string.IsNullOrEmpty(s.MotherPhoto)),
                    PendingDocs = studentDocStatus.Sum(s =>
                        (s.Docs1 == "No" ? 1 : 0) +
                        (s.Docs2 == "No" ? 1 : 0) +
                        (s.Docs3 == "No" ? 1 : 0) +
                        (s.Docs4 == "No" ? 1 : 0)),
                    MissingPhotos = studentDocStatus.Sum(s =>
                        (string.IsNullOrEmpty(s.StudentPhoto) ? 1 : 0) +
                        (string.IsNullOrEmpty(s.FatherPhoto) ? 1 : 0) +
                        (string.IsNullOrEmpty(s.MotherPhoto) ? 1 : 0) +
                        (string.IsNullOrEmpty(s.GuardianPhoto) ? 1 : 0))
                };

                return Json(new
                {
                    success = true,
                    data = studentDocStatus,
                    summary = summary
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to get photo path
        private string GetPhotoPath(string admsnNo, string photoType, string schoolCode)
        {
            // Check if file exists in the file system
            var fileName = $"{admsnNo}_{photoType}.jpg";
            var filePath = Server.MapPath($"~/Documents/{schoolCode}/StudentProfile/{fileName}");

            if (System.IO.File.Exists(filePath))
            {
                return $"/Documents/{schoolCode}/StudentProfile/{fileName}";
            }

            return null; // No photo found
        }

        // Helper method to check document status
        private string CheckDocumentStatus(object documentField)
        {
            // This depends on how you store document status in your database
            // Could be a boolean, string, or file path

            if (documentField == null)
                return "No";

            if (documentField is bool)
                return (bool)documentField ? "Yes" : "No";

            if (documentField is string)
            {
                var docString = documentField as string;
                if (!string.IsNullOrEmpty(docString))
                    return "Yes";
            }

            return "No";
        }

        // Get document details for a specific student (optional - if you need detailed view)
        [HttpPost]
        public JsonResult GetStudentDocumentDetails(string studentId)
        {
            try
            {
                // This is optional - implement if you need a detailed view of documents
                // You can show document names, upload dates, file sizes, etc.

                return Json(new { success = true, message = "Details feature not implemented yet" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Export data to Excel
        [HttpPost]
        public ActionResult ExportToExcel(string className, string section)
        {
            try
            {
                var students = _repository.GetStudentsByClassAndSection(className, section);

                // Create Excel file using your preferred library (EPPlus, NPOI, etc.)
                // This is a simplified example

                var csvContent = "Adm No,Class,Student Name,Father Name,Mother Name,Mobile,Docs1,Docs2,Docs3,Docs4\n";

                foreach (var student in students)
                {
                    csvContent += $"{student.AdmsnNo},{student.ClassName}-{student.SectionName}," +
                                 $"{student.FirstName} {student.LastName},{student.FatherName}," +
                                 $"{student.MotherName},{student.Mobile}," +
                                 $"{CheckDocumentStatus(student.Doc1)}," +
                                 $"{CheckDocumentStatus(student.Doc2)}," +
                                 $"{CheckDocumentStatus(student.Doc3)}," +
                                 $"{CheckDocumentStatus(student.Doc4)}\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                return File(bytes, "text/csv", $"StudentDocumentStatus_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        // Convert dropdown results to SelectList
        private List<SelectListItem> ConvertToSelectList(JsonResult result)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownResponse>(jsonString);

                if (dropdownResponse != null && dropdownResponse.Success == true && dropdownResponse.Data != null)
                {
                    return dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }

                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
            }
        }

        // Supporting classes
        public class DropdownResponse
        {
            public bool Success { get; set; }
            public List<DropdownItem> Data { get; set; }
        }

        public class DropdownItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }

    // View Model
    public class DocumentStatusViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public string SelectedClass { get; set; }
        public string SelectedSection { get; set; }

        public DocumentStatusViewModel()
        {
            Classes = new List<SelectListItem>();
            Sections = new List<SelectListItem>();
        }
    }
}