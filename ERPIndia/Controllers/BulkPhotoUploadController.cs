using ERPIndia.BulkUpdate;
using ERPIndia.Controllers.Examination;
using ERPIndia.StudentManagement.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Imaging;

namespace ERPIndia.Controllers
{
    public class BulkPhotoUploadController : BaseController
    {
        private readonly StudentRepository _repository;
        private readonly DropdownController _dropdownController;

        public BulkPhotoUploadController()
        {
            _repository = new StudentRepository();
            _dropdownController = new DropdownController();
        }

        // GET: BulkPhotoUpload
        public ActionResult Index()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            var model = new BulkPhotoUploadViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult)
            };

            return View(model);
        }

        // Get students with photo information
        [HttpPost]
        public JsonResult GetStudentsForPhotoUpload(string className, string section)
        {
            try
            {
                var students = _repository.GetStudentsByClassAndSection(className, section);

                // Map to include photo paths and StudentId
                var studentModels = students.Select(s => new
                {
                    StudentId = s.StudentId, // Add StudentId
                    AdmsnNo = s.AdmsnNo,
                    SchoolCode = s.SchoolCode,
                    RollNo = s.RollNo,
                    Class = s.ClassName + " - " + s.SectionName,
                    StudentName = string.Format("{0} {1}", s.FirstName, s.LastName).Trim(),
                    FatherName = s.FatherName,
                    MotherName = s.MotherName,
                    Gender = s.Gender,
                    Mobile = s.Mobile,

                    // Get existing photos from database fields
                    StudentPhoto = s.Photo,
                    FatherPhoto = s.FatherPhoto,
                    MotherPhoto = s.MotherPhoto,
                    GuardianPhoto = s.GuardianPhoto
                }).ToList();

                return Json(new { success = true, data = studentModels });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Save individual student photos - Modified to use StudentId for DB update but AdmsnNo for filename
        [HttpPost]
        public JsonResult SaveStudentPhotos()
        {
            try
            {
                var basePath = Server.MapPath("~/Documents/");
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                var photos = Request.Files.GetMultiple("photos");
                var photoInfoJson = Request.Form.GetValues("photoInfo");

                if (photos == null || photos.Count == 0)
                {
                    return Json(new { success = false, message = "No photos uploaded" });
                }

                var successCount = 0;
                var errors = new List<string>();
                var updates = new List<StudentPhotoUpdate>();

                // Process each photo
                for (int i = 0; i < photos.Count; i++)
                {
                    if (i >= photoInfoJson.Length) break;

                    try
                    {
                        var photo = photos[i];
                        var photoInfo = JsonConvert.DeserializeObject<PhotoInfo>(photoInfoJson[i]);

                        if (photo != null && photo.ContentLength > 0)
                        {
                            // Validate file type
                            var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
                            var extension = Path.GetExtension(photo.FileName).ToLower();

                            if (!validExtensions.Contains(extension))
                            {
                                errors.Add($"Invalid file type for student {photoInfo.AdmsnNo}");
                                continue;
                            }

                            // Create directory structure
                            var schoolPath = Path.Combine(basePath, CurrentSchoolCode, "StudentProfile");
                            if (!Directory.Exists(schoolPath))
                            {
                                Directory.CreateDirectory(schoolPath);
                            }

                            // Generate filename based on pattern using AdmsnNo (NOT StudentId)
                            var photoTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "student", "stu" },
                                { "father", "father" },
                                { "mother", "mother" },
                                { "guardian", "guard" }
                            };

                            var photoTypeSuffix = photoTypeMap.ContainsKey(photoInfo.PhotoType.ToLower())
                                ? photoTypeMap[photoInfo.PhotoType.ToLower()]
                                : photoInfo.PhotoType.ToLower();

                            // Use AdmsnNo for filename (NOT StudentId)
                            var fileName = $"{photoInfo.AdmsnNo}_{photoTypeSuffix}.jpg";
                            var filePath = Path.Combine(schoolPath, fileName);

                            // Delete existing file if it exists
                            if (System.IO.File.Exists(filePath))
                            {
                                try
                                {
                                    System.IO.File.Delete(filePath);
                                    System.Diagnostics.Debug.WriteLine($"Deleted existing file: {filePath}");
                                }
                                catch (Exception deleteEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error deleting existing file: {deleteEx.Message}");
                                }
                            }

                            // Process and save the image
                            using (var image = Image.FromStream(photo.InputStream))
                            {
                                // Ensure image is compressed and optimized
                                var compressedImage = CompressImage(image, 100 * 1024); // 100KB limit

                                // Always save as JPEG for consistency
                                var encoder = ImageCodecInfo.GetImageDecoders()
                                    .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

                                var encoderParams = new EncoderParameters(1);
                                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                                compressedImage.Save(filePath, encoder, encoderParams);
                                compressedImage.Dispose();
                            }

                            // Store path with AdmsnNo in filename
                            var relativePath = $"/Documents/{CurrentSchoolCode}/StudentProfile/{fileName}";

                            // Group updates by StudentId for database update
                            var existingUpdate = updates.FirstOrDefault(u => u.StudentId == Guid.Parse(photoInfo.StudentId));
                            if (existingUpdate == null)
                            {
                                existingUpdate = new StudentPhotoUpdate
                                {
                                    StudentId = Guid.Parse(photoInfo.StudentId),
                                    AdmsnNo = int.Parse(photoInfo.AdmsnNo),
                                    SchoolCode = int.Parse(CurrentSchoolCode)
                                };
                                updates.Add(existingUpdate);
                            }

                            // Set the appropriate photo path
                            switch (photoInfo.PhotoType.ToLower())
                            {
                                case "student":
                                    existingUpdate.StudentPhoto = relativePath;
                                    break;
                                case "father":
                                    existingUpdate.FatherPhoto = relativePath;
                                    break;
                                case "mother":
                                    existingUpdate.MotherPhoto = relativePath;
                                    break;
                                case "guardian":
                                    existingUpdate.GuardianPhoto = relativePath;
                                    break;
                            }

                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error processing photo {i + 1}: {ex.Message}");
                    }
                }

                // Update database with photo paths
                if (updates.Any())
                {
                    bool dbUpdateSuccess = UpdateStudentPhotos(updates);

                    if (!dbUpdateSuccess)
                    {
                        errors.Add("Failed to update database with photo paths");
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"{successCount} {(successCount == 1 ? "image has" : "images have")} been saved successfully.",
                        errors = errors
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "No photos were uploaded successfully",
                        errors = errors
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }

        // Update student photos in database - Modified to use StudentId
        private bool UpdateStudentPhotos(List<StudentPhotoUpdate> updates)
        {
            try
            {
                foreach (var update in updates)
                {
                    // Update photos using the repository method with StudentId
                    _repository.UpdateStudentPhotosByStudentId(update, CurrentTenantUserID);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating photos: {ex.Message}");
                return false;
            }
        }

        // Compress image to specified size
        private Image CompressImage(Image image, long maxSizeInBytes)
        {
            // Calculate initial quality based on current size estimate
            long quality = 90L;

            using (var ms = new MemoryStream())
            {
                // Save with initial quality
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                var jpegCodec = ImageCodecInfo.GetImageDecoders()
                    .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

                image.Save(ms, jpegCodec, encoderParameters);

                // If already under limit, return original
                if (ms.Length <= maxSizeInBytes)
                {
                    return image;
                }

                // Otherwise, create a resized version
                var ratio = Math.Sqrt((double)maxSizeInBytes / ms.Length);
                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                // Ensure minimum dimensions
                if (newWidth < 200) newWidth = 200;
                if (newHeight < 200) newHeight = 200;

                var resizedImage = new Bitmap(newWidth, newHeight);
                using (var graphics = Graphics.FromImage(resizedImage))
                {
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }

                return resizedImage;
            }
        }

        // Modified to use AdmsnNo for retrieving photos
        [HttpGet]
        public ActionResult GetStudentPhoto(string admsnNo, string photoType)
        {
            try
            {
                var photoTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "student", "stu" },
                    { "father", "father" },
                    { "mother", "mother" },
                    { "guardian", "guard" }
                };

                var photoTypeSuffix = photoTypeMap.ContainsKey(photoType.ToLower())
                    ? photoTypeMap[photoType.ToLower()]
                    : photoType.ToLower();

                var fileName = $"{admsnNo}_{photoTypeSuffix}.jpg";
                var filePath = Server.MapPath($"~/Documents/{CurrentSchoolCode}/StudentProfile/{fileName}");

                if (System.IO.File.Exists(filePath))
                {
                    return base.File(filePath, "image/jpeg");
                }
                else
                {
                    return HttpNotFound();
                }
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
        public class PhotoInfo
        {
            public string StudentId { get; set; } // GUID as string for DB update
            public string AdmsnNo { get; set; }   // Admission number for filename
            public string PhotoType { get; set; }
            public string FileName { get; set; }
        }

        public class StudentPhotoUpdate
        {
            public Guid StudentId { get; set; } // Changed from AdmsnNo to StudentId
            public int AdmsnNo { get; set; } // Keep for backward compatibility
            public int SchoolCode { get; set; }
            public string StudentPhoto { get; set; }
            public string FatherPhoto { get; set; }
            public string MotherPhoto { get; set; }
            public string GuardianPhoto { get; set; }
        }

        // Dropdown response classes
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

    // View Model
    public class BulkPhotoUploadViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public string SelectedClass { get; set; }
        public string SelectedSection { get; set; }

        public BulkPhotoUploadViewModel()
        {
            Classes = new List<SelectListItem>();
            Sections = new List<SelectListItem>();
        }
    }
}