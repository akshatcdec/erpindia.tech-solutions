using ERPIndia.BulkUpdate;
using ERPIndia.Controllers.Examination;
using ERPIndia.StudentManagement.Repository;
using Hangfire.Common;
using Newtonsoft.Json;
using StudentManagement.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class BulkUpdateController : BaseController
    {
        private readonly StudentRepository _repository;
        private readonly DropdownController _dropdownController;

        // Column configuration with data types and validation rules
        private static readonly Dictionary<string, ColumnConfig> ColumnConfigurations = new Dictionary<string, ColumnConfig>(StringComparer.OrdinalIgnoreCase)
        {
            // Numeric fields with specific validations
            { "SrNo", new ColumnConfig { DataType = "number", SqlType = SqlDbType.NVarChar, MaxLength = 10, ValidationPattern = @"^\d+$" } },
            { "RollNo", new ColumnConfig { DataType = "text", SqlType = SqlDbType.NVarChar, MaxLength = 10, ValidationPattern = @"^[a-zA-Z0-9_\-]+$" } },
            { "FirstName", new ColumnConfig { DataType = "text", SqlType = SqlDbType.NVarChar, MaxLength = 100, ValidationPattern = @"^[a-zA-Z\s]+$" } },
            { "LastName", new ColumnConfig { DataType = "text", SqlType = SqlDbType.NVarChar, MaxLength = 100, ValidationPattern = @"^[a-zA-Z\s]+$" } },
            { "OldBalance", new ColumnConfig { DataType = "decimal", SqlType = SqlDbType.Decimal, MaxLength = 6, ValidationPattern = @"^\d*\.?\d+$" } },
            { "Height", new ColumnConfig { DataType = "decimal", SqlType = SqlDbType.Decimal, MaxLength = 6, ValidationPattern = @"^\d*\.?\d+$" } },
            { "Weight", new ColumnConfig { DataType = "decimal", SqlType = SqlDbType.Decimal, MaxLength = 6, ValidationPattern = @"^\d*\.?\d+$" } },
            { "Password", new ColumnConfig { DataType = "number", SqlType = SqlDbType.NVarChar, MaxLength = 4 , ValidationPattern = @"^\d+$" } },
          
            // Text fields with specific validations
            { "AdmsnNo", new ColumnConfig { DataType = "number", SqlType = SqlDbType.NVarChar, MaxLength = 10, ValidationPattern = @"^\d+$" } },
            { "FatherName", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 100, ValidationPattern = @"^[a-zA-Z\s]+$" } },
            { "MotherName", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 100, ValidationPattern = @"^[a-zA-Z\s]+$" } },
            { "GuardianName", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 100, ValidationPattern = @"^[a-zA-Z\s]+$" } },
            { "AadharNo", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 15, ValidationPattern = @"^\d{12}$|^\d{4}\s\d{4}\s\d{4}$", ExactLength = 12 } },
            { "FatherAadhar", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 15, ValidationPattern = @"^\d{12}$|^\d{4}\s\d{4}\s\d{4}$", ExactLength = 12 } },
            { "MotherAadhar", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 15, ValidationPattern = @"^\d{12}$|^\d{4}\s\d{4}\s\d{4}$", ExactLength = 12 } },
            { "Mobile", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 10, ValidationPattern = @"^[6-9]\d{9}$", ExactLength = 10 } },
            
            // Dynamic dropdown fields from database
            { "Gender", new ColumnConfig {
                DataType = "dropdown",
                SqlType = SqlDbType.NVarChar,
                MaxLength = 1,
                Dynamic = true,
                DropdownMethod = "GetGender"
            } },

            { "BloodGroup", new ColumnConfig {
                DataType = "dropdown",
                SqlType = SqlDbType.NVarChar,
                MaxLength = 5,
                Dynamic = true,
                DropdownMethod = "GetBloodGroup"
            } },

            { "Category", new ColumnConfig {
                DataType = "dropdown",
                SqlType = SqlDbType.NVarChar,
                MaxLength = 50,
                Dynamic = true,
                DropdownMethod = "GetCategory"
            } },

            { "Section", new ColumnConfig {
                DataType = "dropdown",
                SqlType = SqlDbType.NVarChar,
                MaxLength = 10,
                Dynamic = true,
                DropdownMethod = "GetSections"
            } },

            { "FeeCategory", new ColumnConfig {
                DataType = "dropdown",
                SqlType = SqlDbType.NVarChar,
                MaxLength = 50,
                Dynamic = true,
                DropdownMethod = "GetFeeCategories"
            } },

            { "VillegeName", new ColumnConfig {
                DataType = "dropdown",
                SqlType = SqlDbType.NVarChar,
                MaxLength = 100,
                Dynamic = true,
                DropdownMethod = "GetTown"
            } },

            { "House", new ColumnConfig {
                DataType = "dropdown",
                SqlType = SqlDbType.NVarChar,
                MaxLength = 50,
                Dynamic = true,
                DropdownMethod = "GetHouses"
            } },

            { "Religion", new ColumnConfig {
                DataType = "dropdown",
                SqlType = SqlDbType.NVarChar,
                MaxLength = 50,
                Dynamic = true,
                DropdownMethod = "GetReligion"
            } },

            { "MotherTongue", new ColumnConfig {
                DataType = "dropdown",
                SqlType = SqlDbType.NVarChar,
                MaxLength = 50,
                Dynamic = true,
                DropdownMethod = "GetMotherTounge"
            } },
            
            // Other fields
            { "Address", new ColumnConfig { DataType = "textarea", SqlType = SqlDbType.NVarChar, MaxLength = 100 } },
            { "PENNo", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 50 } },
            { "PreviousSchool", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 80 } },
            { "UDISE", new ColumnConfig { DataType = "string", SqlType = SqlDbType.NVarChar, MaxLength = 30 } },
            
            // Date fields
            { "DOB", new ColumnConfig { DataType = "date", SqlType = SqlDbType.DateTime } },
            { "AdmsnDate", new ColumnConfig { DataType = "date", SqlType = SqlDbType.DateTime } },
            
            // Boolean field
            { "IsActive", new ColumnConfig { DataType = "bool", SqlType = SqlDbType.Bit } },
            
            // File field
            { "Photo", new ColumnConfig { DataType = "file", SqlType = SqlDbType.NVarChar, MaxLength = 500 } }
        };

        public BulkUpdateController()
        {
            _repository = new StudentRepository();
            _dropdownController = new DropdownController();
        }

        // GET: BulkUpdate
        public ActionResult BulkUpdate()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();
            var columnResult = _dropdownController.GetUpdateColumn();

            var model = new BulkUpdateViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                Columns = ConvertToCoulmnSelectList(columnResult)
            };

            return View(model);
        }

        // Get students based on class and section
        [HttpPost]
        public JsonResult GetStudents(string className, string section)
        {
            try
            {
                var students = _repository.GetStudentsByClassAndSection(className, section);

                // Map to StudentUpdateModel with all required fields
                var studentModels = students.Select(s => new StudentUpdateModel
                {
                    StudentId = s.StudentId,
                    AdmsnNo = s.AdmsnNo,
                    SchoolCode = s.SchoolCode,
                    StudentNo = s.StudentNo,
                    SrNo = s.SrNo,
                    RollNo = s.RollNo,
                    ClassName = s.ClassName,
                    Class=s.Class,
                    Section=s.Section,
                    SectionName = s.SectionName,
                    FirstName = string.Format("{0}", s.FirstName).Trim(),
                    LastName = string.Format("{0}", s.LastName).Trim(),
                    StudentName = string.Format("{0}", s.FirstName).Trim(),
                    FatherName = s.FatherName,
                    MotherName = s.MotherName,
                    MotherAadhar = s.MotherAadhar,
                    FatherAadhar = s.FatherAadhar,
                    AadharNo = s.AadharNo,
                    Gender = s.Gender,
                    Mobile = s.Mobile,
                    Photo = s.Photo,
                    Email = s.Email,
                    Category = s.Category,
                    BloodGroup = s.BloodGroup,
                    Address = s.Address,
                    PickupPoint = s.PickupPoint,
                    House = s.House,
                    FatherMobile = s.FatherMobile,
                    MotherMobile = s.MotherMobile,
                    DOB = s.DOB.HasValue ? s.DOB.Value.ToString("yyyy-MM-dd") : "",
                    Height = string.IsNullOrWhiteSpace(s.Height) ? (decimal?)null : decimal.Parse(s.Height),
                    Weight = string.IsNullOrWhiteSpace(s.Weight) ? (decimal?)null : decimal.Parse(s.Weight),
                    GuardianName = s.GuardianName,
                    OldBalance = s.OldBalance,
                    Password = s.Password,
                    FeeCategory = s.FeeCategory,
                    PENNo = s.PENNo,
                    PreviousSchool = s.PreviousSchool,
                    UDISE = s.UDISE,
                    VillegeName = s.VillegeId.ToString(),
                    AdmsnDate = s.AdmsnDate.HasValue ? s.AdmsnDate.Value.ToString("yyyy-MM-dd") : "",
                    IsActive = (s.IsActive.HasValue) ? s.IsActive.Value : false,
                    Religion = s.Religion,
                    MotherTongue = s.MotherTongue,
                    HasFeeRecords = s.HasFeeRecords
                }).ToList();

                return Json(new { success = true, data = studentModels });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateStudents()
        {
            try
            {
                BulkUpdateRequest request = null;

                // Check if the request is JSON (Content-Type: application/json)
                if (Request.ContentType != null && Request.ContentType.Contains("application/json"))
                {
                    // Read JSON from request body
                    Request.InputStream.Position = 0;
                    using (var reader = new StreamReader(Request.InputStream))
                    {
                        string json = reader.ReadToEnd();
                        request = JsonConvert.DeserializeObject<BulkUpdateRequest>(json);
                    }
                }
                else
                {
                    // Handle FormData submission (for file uploads)
                    var columnName = Request.Form["ColumnName"];
                    var updatesJson = Request.Form["Updates"];

                    if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(updatesJson))
                    {
                        request = new BulkUpdateRequest
                        {
                            ColumnName = columnName,
                            Updates = JsonConvert.DeserializeObject<List<UpdateItem>>(updatesJson)
                        };
                    }
                }

                // Validate request
                if (request == null)
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                if (string.IsNullOrEmpty(request.ColumnName))
                {
                    return Json(new { success = false, message = "Please select a column to update" });
                }

                if (request.Updates == null || !request.Updates.Any())
                {
                    return Json(new { success = false, message = "No students selected for update" });
                }

                var config = ColumnConfigurations[request.ColumnName];
                var successCount = 0;
                var errors = new List<string>();

                // Handle file uploads if updating photo column
                if (config.DataType == "file" && request.ColumnName.Equals("Photo", StringComparison.OrdinalIgnoreCase))
                {
                    var photoResults = HandlePhotoUploads();
                    if (photoResults.Any())
                    {
                        foreach (var update in request.Updates)
                        {
                            var studentIdStr = update.StudentId.ToString() ?? "";
                            if (photoResults.ContainsKey(studentIdStr))
                            {
                                update.Value = photoResults[studentIdStr];
                            }
                        }
                    }
                }

                // Clean and validate all updates before processing
                foreach (var update in request.Updates)
                {
                    if (request.ColumnName.Contains("Aadhar") && !string.IsNullOrEmpty(update.Value))
                    {
                        update.Value = update.Value.Replace(" ", "");
                    }

                    var validationResult = ValidateValue(update.Value, config);
                    if (!validationResult.IsValid)
                    {
                        errors.Add(string.Format("Student {0}: {1}", update.StudentId, validationResult.ErrorMessage));
                    }
                }

                if (errors.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Validation errors found",
                        errors = errors
                    });
                }

                request.ModifiedBy = CurrentTenantUserID;

                // Process updates using StudentId
                bool result = _repository.BulkUpdateStudentsByStudentId(request.ColumnName,request.Updates, request.ModifiedBy);

                if (result)
                {
                    // Get the class and section from the first update item to refresh data
                    if (request.Updates.Any())
                    {
                        var className = Request.Form["ClassName"] ?? "";
                        var section = Request.Form["Section"] ?? "";

                        var updatedStudents = _repository.GetStudentsByClassAndSection(className, section);

                        // Map to StudentUpdateModel
                        var studentModels = updatedStudents.Select(s => new StudentUpdateModel
                        {
                            StudentId = s.StudentId,
                            AdmsnNo = s.AdmsnNo,
                            SchoolCode = s.SchoolCode,
                            StudentNo = s.StudentNo,
                            SrNo = s.SrNo,
                            RollNo = s.RollNo,
                            Class = s.Class,
                            Section = s.Section,
                            ClassName = s.ClassName,
                            SectionName = s.SectionName,
                            StudentName = string.Format("{0}", s.FirstName).Trim(),
                            FatherName = s.FatherName,
                            Gender = s.Gender,
                            Mobile = s.Mobile,
                            Photo = s.Photo,
                            Email = s.Email,
                            Category = s.Category,
                            BloodGroup = s.BloodGroup,
                            Address = s.Address,
                            PickupPoint = s.PickupPoint,
                            House = s.House,
                            FatherMobile = s.FatherMobile,
                            MotherMobile = s.MotherMobile,
                            LastName = s.LastName,
                            AadharNo = s.AadharNo,
                            FatherAadhar = s.FatherAadhar,
                            MotherAadhar = s.MotherAadhar,
                            DOB = s.DOB.HasValue? s.DOB.Value.ToString("yyyy-MM-dd") :"",
                            Height = string.IsNullOrWhiteSpace(s.Height) ? (decimal?)null : decimal.Parse(s.Height),
                            Weight = string.IsNullOrWhiteSpace(s.Weight) ? (decimal?)null : decimal.Parse(s.Weight),
                            MotherName = s.MotherName,
                            GuardianName = s.GuardianName,
                            OldBalance = s.OldBalance,
                            Password = s.Password,
                            FeeCategory = s.FeeCategory,
                            PENNo = s.PENNo,
                            PreviousSchool = s.PreviousSchool,
                            UDISE = s.UDISE,
                            VillegeName = s.VillegeName,
                            AdmsnDate = s.AdmsnDate.HasValue ? s.AdmsnDate.Value.ToString("yyyy-MM-dd") : "",
                            IsActive = (s.IsActive.HasValue) ? s.IsActive.Value : false,
                            Religion = s.Religion,
                            MotherTongue = s.MotherTongue
                        }).ToList();

                        return Json(new
                        {
                            success = true,
                            message = string.Format("Successfully updated {0} students", request.Updates.Count),
                            updatedStudents = studentModels
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        message = string.Format("Successfully updated {0} students", request.Updates.Count)
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Update failed. Please check the server logs for details."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateStudents Error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);

                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }

        // Get dropdown data for dynamic columns
        [HttpGet]
        public JsonResult GetDropdownData(string columnName)
        {
            try
            {
                if (!ColumnConfigurations.ContainsKey(columnName))
                {
                    return Json(new { success = false, message = "Invalid column name" }, JsonRequestBehavior.AllowGet);
                }

                var config = ColumnConfigurations[columnName];
                if (!config.Dynamic || string.IsNullOrEmpty(config.DropdownMethod))
                {
                    return Json(new { success = false, message = "Column is not a dynamic dropdown" }, JsonRequestBehavior.AllowGet);
                }

                // Get dropdown data based on the method name
                var methodInfo = _dropdownController.GetType().GetMethod(config.DropdownMethod);

                if (methodInfo == null)
                {
                    return Json(new { success = false, message = "Dropdown method not found" }, JsonRequestBehavior.AllowGet);
                }

                var result = methodInfo.Invoke(_dropdownController, null) as JsonResult;

                if (result != null)
                {
                    // Extract the data from the JsonResult
                    var jsonData = JsonConvert.SerializeObject(result.Data);
                    var dropdownResponse = JsonConvert.DeserializeObject<BulkDropdownResponse>(jsonData);

                    if (dropdownResponse != null && dropdownResponse.Success && dropdownResponse.Data != null)
                    {
                        var items = dropdownResponse.Data.Select(item => new
                        {
                            value = item.Id?.ToString() ?? item.Key ?? item.Value,
                            text = item.Name ?? item.Value ?? item.Text,
                            id = item.Id
                        }).ToList();

                        return Json(new { success = true, data = items }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { success = false, message = "Failed to load dropdown data" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Get multiple dropdown data at once
        [HttpPost]
        public JsonResult GetMultipleDropdownData(List<string> columnNames)
        {
            try
            {
                var results = new Dictionary<string, object>();

                foreach (var columnName in columnNames)
                {
                    if (!ColumnConfigurations.ContainsKey(columnName))
                        continue;

                    var config = ColumnConfigurations[columnName];
                    if (!config.Dynamic || string.IsNullOrEmpty(config.DropdownMethod))
                        continue;

                    try
                    {
                        var methodInfo = _dropdownController.GetType().GetMethod(config.DropdownMethod);
                        if (methodInfo != null)
                        {
                            var result = methodInfo.Invoke(_dropdownController, null) as JsonResult;
                            if (result != null)
                            {
                                var jsonData = JsonConvert.SerializeObject(result.Data);
                                var dropdownResponse = JsonConvert.DeserializeObject<BulkDropdownResponse>(jsonData);

                                if (dropdownResponse != null && dropdownResponse.Success && dropdownResponse.Data != null)
                                {
                                    var items = dropdownResponse.Data.Select(item => new
                                    {
                                        value = item.Id?.ToString() ?? item.Key ?? item.Value,
                                        text = item.Name ?? item.Value ?? item.Text
                                    }).ToList();

                                    results[columnName] = items;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Skip this dropdown if there's an error
                        continue;
                    }
                }

                return Json(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Get column configuration for client-side
        public JsonResult GetColumnConfig(string columnName)
        {
            if (string.IsNullOrEmpty(columnName) || !ColumnConfigurations.ContainsKey(columnName))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            var config = ColumnConfigurations[columnName];
            return Json(new
            {
                success = true,
                config = new
                {
                    dataType = config.DataType,
                    maxLength = config.MaxLength,
                    validationPattern = config.ValidationPattern,
                    exactLength = config.ExactLength,
                    allowedValues = config.AllowedValues,
                    dynamic = config.Dynamic,
                    dropdownMethod = config.DropdownMethod
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // Helper methods
        private List<SelectListItem> ConvertToSelectList(JsonResult result)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = JsonConvert.DeserializeObject<BulkDropdownResponse>(jsonString);

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

        private List<SelectListItem> ConvertToCoulmnSelectList(JsonResult result)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(result.Data);
                var dropdownResponse = JsonConvert.DeserializeObject<BulkDropdownResponse>(jsonString);

                if (dropdownResponse != null && dropdownResponse.Success == true && dropdownResponse.Data != null)
                {
                    return dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Key,
                        Text = item.Value
                    }).ToList();
                }

                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
            }
        }

        private ValidationResult ValidateValue(string value, ColumnConfig config)
        {
            if (string.IsNullOrEmpty(value) && config.DataType != "bool")
            {
                return new ValidationResult { IsValid = true };
            }

            // Check max length for string types
            if ((config.DataType == "string" || config.DataType == "textarea" || config.DataType == "password")
                && config.MaxLength > 0 && !string.IsNullOrEmpty(value) && value.Length > config.MaxLength)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = string.Format("Value exceeds maximum length of {0} characters", config.MaxLength)
                };
            }

            // Check exact length if specified
            if (config.ExactLength > 0 && !string.IsNullOrEmpty(value))
            {
                // For fields like Aadhar, remove spaces before checking length
                var cleanValue = value.Replace(" ", "");
                if (cleanValue.Length != config.ExactLength)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = string.Format("Value must be exactly {0} digits", config.ExactLength)
                    };
                }
            }

            // Check allowed values for dropdowns
            if (config.AllowedValues != null && config.AllowedValues.Length > 0 && !string.IsNullOrEmpty(value))
            {
                bool isValidValue = false;
                foreach (var allowedValue in config.AllowedValues)
                {
                    if (string.Equals(allowedValue, value, StringComparison.OrdinalIgnoreCase))
                    {
                        isValidValue = true;
                        break;
                    }
                }

                if (!isValidValue)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = string.Format("Invalid value. Allowed values are: {0}", string.Join(", ", config.AllowedValues))
                    };
                }
            }

            // Check pattern if specified
            if (!string.IsNullOrEmpty(config.ValidationPattern) && !string.IsNullOrEmpty(value))
            {
                var regex = new System.Text.RegularExpressions.Regex(config.ValidationPattern);
                if (!regex.IsMatch(value))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = GetValidationErrorMessage(config)
                    };
                }
            }

            // Type-specific validation
            switch (config.DataType)
            {
                case "number":
                    if (!string.IsNullOrEmpty(value))
                    {
                        int intValue;
                        if (!int.TryParse(value, out intValue))
                        {
                            return new ValidationResult
                            {
                                IsValid = false,
                                ErrorMessage = "Please enter only numbers"
                            };
                        }
                    }
                    break;

                case "decimal":
                    if (!string.IsNullOrEmpty(value))
                    {
                        decimal decimalValue;
                        if (!decimal.TryParse(value, out decimalValue))
                        {
                            return new ValidationResult
                            {
                                IsValid = false,
                                ErrorMessage = "Please enter a valid decimal number"
                            };
                        }
                    }
                    break;

                case "date":
                    if (!string.IsNullOrEmpty(value))
                    {
                        DateTime dateValue;
                        if (!DateTime.TryParse(value, out dateValue))
                        {
                            return new ValidationResult
                            {
                                IsValid = false,
                                ErrorMessage = "Please enter a valid date"
                            };
                        }
                    }
                    break;

                case "bool":
                    if (!string.IsNullOrEmpty(value) &&
                        value != "0" && value != "1" &&
                        value.ToLower() != "true" && value.ToLower() != "false")
                    {
                        return new ValidationResult
                        {
                            IsValid = false,
                            ErrorMessage = "Please enter a valid boolean value"
                        };
                    }
                    break;
            }

            return new ValidationResult { IsValid = true };
        }

        private string GetValidationErrorMessage(ColumnConfig config)
        {
            if (config.ValidationPattern == @"^\d+$")
                return "Please enter only numbers";
            if (config.ValidationPattern == @"^[a-zA-Z\s]+$")
                return "Please enter only letters";
            if (config.ValidationPattern == @"^[a-zA-Z0-9_\-]+$")
                return "Please enter only letters, numbers, hyphens and underscores";
            if (config.ValidationPattern == @"^\d{12}$|^\d{4}\s\d{4}\s\d{4}$")
                return "Please enter a valid 12-digit Aadhar number";
            if (config.ValidationPattern == @"^[6-9]\d{9}$")
                return "Please enter a valid 10-digit mobile number starting with 6-9";
            if (config.ValidationPattern == @"^\d*\.?\d+$")
                return "Please enter a valid decimal number";

            return "Invalid format";
        }

        private Dictionary<string, string> HandlePhotoUploads()
        {
            var photoResults = new Dictionary<string, string>();

            try
            {
                if (Request.Files.Count == 0)
                    return photoResults;

                var uploadPath = Server.MapPath("~/Uploads/StudentPhotos/");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (string fileKey in Request.Files.Keys)
                {
                    if (fileKey.StartsWith("photos_"))
                    {
                        var parts = fileKey.Split('_');
                        if (parts.Length >= 2)
                        {
                            var studentId = parts[1];
                            var file = Request.Files[fileKey];

                            if (file != null && file.ContentLength > 0)
                            {
                                // Validate file type
                                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                                var extension = Path.GetExtension(file.FileName).ToLower();

                                if (!validExtensions.Contains(extension))
                                    continue;

                                // Create subdirectory for student
                                var studentPath = Path.Combine(uploadPath, studentId);
                                if (!Directory.Exists(studentPath))
                                {
                                    Directory.CreateDirectory(studentPath);
                                }

                                // Generate unique filename
                                var fileName = string.Format("{0}{1}", Guid.NewGuid(), extension);
                                var filePath = Path.Combine(studentPath, fileName);

                                // Save file
                                file.SaveAs(filePath);

                                // Store relative path
                                var relativePath = string.Format("/Uploads/StudentPhotos/{0}/{1}", studentId, fileName);

                                // If student already has a path, append with separator
                                if (photoResults.ContainsKey(studentId))
                                {
                                    photoResults[studentId] += ";" + relativePath;
                                }
                                else
                                {
                                    photoResults[studentId] = relativePath;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error handling photo uploads: " + ex.Message);
            }

            return photoResults;
        }
    }

    // Supporting classes
    public class ColumnConfig
    {
        public string DataType { get; set; }
        public SqlDbType SqlType { get; set; }
        public int MaxLength { get; set; }
        public int? Max { get; set; }
        public int? Min { get; set; }
        public string ValidationPattern { get; set; }
        public int ExactLength { get; set; }
        public string[] AllowedValues { get; set; }
        public bool Dynamic { get; set; }
        public string DropdownMethod { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class BulkUpdateViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public List<SelectListItem> Columns { get; set; }
        public string SelectedClass { get; set; }
        public string SelectedSection { get; set; }
        public string SelectedColumn { get; set; }
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
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string MotherAadhar { get; set; }
        public string FatherAadhar { get; set; }
        public string AadharNo { get; set; }
        public string Gender { get; set; }
        public string Mobile { get; set; }
        public string Photo { get; set; }
        public string Email { get; set; }
        public string Category { get; set; }
        public string BloodGroup { get; set; }
        public string Address { get; set; }
        public string PickupPoint { get; set; }
        public string House { get; set; }
        public string FatherMobile { get; set; }
        public string MotherMobile { get; set; }
        public string DOB { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string GuardianName { get; set; }
        public decimal? OldBalance { get; set; }
        public string Password { get; set; }
        public string FeeCategory { get; set; }
        public string PENNo { get; set; }
        public string PreviousSchool { get; set; }
        public string UDISE { get; set; }
        public string VillegeName { get; set; }
        public string AdmsnDate { get; set; }
        public bool IsActive { get; set; }
        public string Religion { get; set; }
        public string MotherTongue { get; set; }
        public string HasFeeRecords { get; set; }
    }

    public class BulkDropdownResponse
    {
        public bool Success { get; set; }
        public List<BulkDropdownItem> Data { get; set; }
        public string Message { get; set; }
    }

    public class BulkDropdownItem
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
    }
}