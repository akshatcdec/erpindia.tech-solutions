using ERPIndia.StudentManagement.Repository;
using ERPK12Models.DTO;
using ERPK12Models.ViewModel.Enquiry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class EnquiryController : BaseController
    {
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly string _connectionString;
        private readonly DropdownController _dropdownController;
        public EnquiryController()
        {
            _enquiryRepository = new EnquiryRepository();
        }
        public EnquiryController(IEnquiryRepository enquiryRepository)
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            _enquiryRepository = enquiryRepository;
            _dropdownController = new DropdownController();

        }
        private List<SelectListItem> ConvertToSelectListDefaultAll(JsonResult result)
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
                    Name = "All"
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
                    Name = "Select"
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
        // GET: Enquiry - Shows the search form only
        public ActionResult Index()
        {
            var classesResult = new DropdownController().GetClasses();
            var viewModel = new EnquiryListViewModel();
            viewModel.Filters = new EnquiryFilterViewModel();
            viewModel.Filters.ClassList = ConvertToSelectListDefaultAll(classesResult);
            // Initialize empty enquiries list for initial load
            viewModel.Enquiries = new List<StudentEnquiry>();
            viewModel.Pagination = new PaginationViewModel
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalRecords = 0
            };

            return View(viewModel);
        }

        // POST: Enquiry/Search - Handles AJAX search requests
        [HttpPost]
        public async Task<ActionResult> Search(EnquiryFilterViewModel filters, int page = 1, int pageSize = 10)
        {
            try
            {
                // Validate input parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;

                var viewModel = new EnquiryListViewModel
                {
                    Filters = filters ?? new EnquiryFilterViewModel(),
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize
                    }
                };

                // Use FromDate and ToDate if provided, otherwise use StartDate and EndDate
                if (viewModel.Filters.FromDate.HasValue && !viewModel.Filters.StartDate.HasValue)
                {
                    viewModel.Filters.StartDate = viewModel.Filters.FromDate;
                }
                if (viewModel.Filters.ToDate.HasValue && !viewModel.Filters.EndDate.HasValue)
                {
                    viewModel.Filters.EndDate = viewModel.Filters.ToDate;
                }

                // FIX: Handle empty class selection (Guid.Empty should be treated as null)
                if (!string.IsNullOrEmpty(viewModel.Filters.SelectedClass) &&
                    viewModel.Filters.SelectedClass == Guid.Empty.ToString())
                {
                    viewModel.Filters.SelectedClass = null;
                }

                // Validate date range logic
                if (viewModel.Filters.StartDate.HasValue && viewModel.Filters.EndDate.HasValue)
                {
                    if (viewModel.Filters.StartDate > viewModel.Filters.EndDate)
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = false, message = "Start date cannot be later than end date." });
                        }
                        ModelState.AddModelError("StartDate", "Start date cannot be later than end date.");
                        return View("Index", viewModel);
                    }

                    // Limit date range to reasonable period (e.g., 1 year)
                    var daysDifference = (viewModel.Filters.EndDate.Value - viewModel.Filters.StartDate.Value).Days;
                    if (daysDifference > 365)
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = false, message = "Date range cannot exceed 365 days." });
                        }
                        ModelState.AddModelError("StartDate", "Date range cannot exceed 365 days.");
                        return View("Index", viewModel);
                    }
                }

                // Check if this is a default search (no specific criteria)
                bool isDefaultSearch = IsDefaultSearch(viewModel.Filters);

                if (isDefaultSearch)
                {
                    // For default search, show recent enquiries (last 30 days)
                    viewModel.Filters.StartDate = DateTime.Today.AddDays(-30);
                    viewModel.Filters.EndDate = DateTime.Today;
                }

                // Get enquiries based on search criteria
                var enquiriesTask = _enquiryRepository.GetAllEnquiriesAsync(sessionId, tenantCode, viewModel.Filters, page, pageSize);
                var countTask = _enquiryRepository.GetTotalEnquiriesCountAsync(sessionId, tenantCode, viewModel.Filters);

                await Task.WhenAll(enquiriesTask, countTask);

                viewModel.Enquiries = enquiriesTask.Result?.ToList() ?? new List<StudentEnquiry>();
                viewModel.Pagination.TotalRecords = countTask.Result;

                // Calculate pagination info
                viewModel.Pagination.TotalPages = (int)Math.Ceiling((double)viewModel.Pagination.TotalRecords / pageSize);

                // Always return partial view for AJAX search requests
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_EnquirySearchResults", viewModel);
                }

                // Fallback for non-AJAX requests
                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                //LogError("Search", ex);

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Search failed. Please try again." });
                }

                TempData["ErrorMessage"] = "Search failed. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // Helper method to check if this is a default search (no specific criteria)
        private bool IsDefaultSearch(EnquiryFilterViewModel filters)
        {
            return filters == null ||
                   (!filters.FromDate.HasValue &&
                    !filters.ToDate.HasValue &&
                    string.IsNullOrEmpty(filters.SelectedClass) &&  // Changed from filters.Class
                    string.IsNullOrEmpty(filters.InterestLevel) &&
                    string.IsNullOrEmpty(filters.CallStatus) &&     // Added CallStatus check
                    !filters.FollowupDate.HasValue &&
                    !filters.NextFollowup.HasValue &&
                    !filters.StartDate.HasValue &&
                    !filters.EndDate.HasValue);
        }

        // GET: Enquiry/ExportToExcel - Export search results to Excel
        public async Task<ActionResult> ExportToExcel(EnquiryFilterViewModel filters)
        {
            try
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;

                // Parse date range if provided
                if (filters?.FromDate.HasValue == true && !filters.StartDate.HasValue)
                {
                    filters.StartDate = filters.FromDate;
                }
                if (filters?.ToDate.HasValue == true && !filters.EndDate.HasValue)
                {
                    filters.EndDate = filters.ToDate;
                }

                // Apply default date range if no filters provided
                if (filters == null || IsDefaultSearch(filters))
                {
                    filters = filters ?? new EnquiryFilterViewModel();
                    filters.StartDate = DateTime.Today.AddDays(-30);
                    filters.EndDate = DateTime.Today;
                }

                // Get all enquiries matching the search criteria (no pagination for export)
                var enquiries = await _enquiryRepository.GetAllEnquiriesAsync(sessionId, tenantCode, filters, 1, int.MaxValue);

                // Create Excel file (you'll need to implement this based on your Excel library)
                var excelData = GenerateExcelReport(enquiries);

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"StudentEnquiries_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Export failed: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Enquiry/Create
        public ActionResult Create()
        {
            var viewModel = new EnquiryViewModel();
            var classesResult = new DropdownController().GetClasses();
            viewModel.ClassList = ConvertToSelectListDefault(classesResult);

            // Initialize the Enquiry object first
            viewModel.Enquiry = new StudentEnquiry
            {
                Id = Guid.NewGuid(), // Generate new GUID
                EnquiryDate = DateTime.Today,
                FormAmt = 0, // Default form amount
                PaymentStatus = "unpaid",
                InterestLevel = "Pending"
            };

            PopulateDropDowns();

            // Return partial view for modal
            return PartialView("_CreatePartial", viewModel);
        }

        // POST: Enquiry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EnquiryViewModel viewModel)
        {
            var classesResult = new DropdownController().GetClasses();
            viewModel.ClassList = ConvertToSelectListDefault(classesResult);

            if (ModelState.IsValid)
            {
                try
                {
                    var sessionId = CurrentSessionID;
                    var tenantCode = TenantCode;
                    var currentUserId = CurrentTenantUserID;
                    var TenantId = CurrentTenantID;
                    // Set default values
                    viewModel.Enquiry.PaymentStatus = "unpaid";
                    viewModel.Enquiry.InterestLevel = "Pending";
                    var id = await _enquiryRepository.CreateEnquiryAsync(viewModel.Enquiry, sessionId, tenantCode, currentUserId);

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = true, message = "Student enquiry created successfully!" });
                    }

                    TempData["SuccessMessage"] = "Student enquiry created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Error creating enquiry: " + ex.Message });
                    }

                    ModelState.AddModelError("", "Error creating enquiry: " + ex.Message);
                }
            }

            if (Request.IsAjaxRequest())
            {
                PopulateDropDowns();
                return PartialView("_CreatePartial", viewModel);
            }

            PopulateDropDowns();
            return View(viewModel);
        }

        // GET: Enquiry/Edit/5
        public async Task<ActionResult> Edit(Guid id)
        {
            var sessionId =  CurrentSessionID;
            var tenantCode = TenantCode;
            var classesResult = new DropdownController().GetClasses();
           

            var enquiry = await _enquiryRepository.GetEnquiryByIdAsync(id, sessionId, tenantCode);
            if (enquiry == null)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Enquiry not found." });
                }
                return HttpNotFound();
            }
           
            var viewModel = new EnquiryViewModel
            {
                Enquiry = enquiry,
                FollowUps = enquiry.FollowUps?.ToList() ?? new List<EnquiryFollowUp>()
            };
            viewModel.ClassList = ConvertToSelectListDefault(classesResult);
            PopulateDropDowns();
            return PartialView("_EditPartial", viewModel);
        }

        // POST: Enquiry/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EnquiryViewModel viewModel)
        {
            var classesResult = new DropdownController().GetClasses();
            viewModel.ClassList = ConvertToSelectListDefault(classesResult);

            if (ModelState.IsValid)
            {
                try
                {
                    var sessionId = CurrentSessionID;
                    var tenantCode = TenantCode;
                    var currentUserId = CurrentTenantUserID;

                    var success = await _enquiryRepository.UpdateEnquiryAsync(viewModel.Enquiry, sessionId, tenantCode, currentUserId);
                    if (success)
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = true, message = "Student enquiry updated successfully!" });
                        }

                        TempData["SuccessMessage"] = "Student enquiry updated successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error updating enquiry.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating enquiry: " + ex.Message);
                }
            }

            // Handle validation errors or update failures
            if (Request.IsAjaxRequest())
            {
                PopulateDropDowns();
                // Reload follow-ups for the edit form
                if (viewModel.Enquiry?.Id != Guid.Empty)
                {
                    var sessionId = CurrentSessionID;
                    var tenantCode = TenantCode;
                    viewModel.FollowUps = (await _enquiryRepository.GetFollowUpsByEnquiryIdAsync(viewModel.Enquiry.Id, sessionId, tenantCode)).ToList();
                }
                return PartialView("_EditPartial", viewModel);
            }

            // For non-AJAX requests (fallback)
            PopulateDropDowns();
            if (viewModel.Enquiry?.Id != Guid.Empty)
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;
                viewModel.FollowUps = (await _enquiryRepository.GetFollowUpsByEnquiryIdAsync(viewModel.Enquiry.Id, sessionId, tenantCode)).ToList();
            }
            return View(viewModel);
        }

        // GET: Enquiry/FollowUp/5
        public async Task<ActionResult> FollowUp(Guid id)
        {
            var sessionId = CurrentSessionID;
            var tenantCode = TenantCode;

            var enquiry = await _enquiryRepository.GetEnquiryByIdAsync(id, sessionId, tenantCode);
            if (enquiry == null)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Enquiry not found." });
                }
                return HttpNotFound();
            }

            var viewModel = new FollowUpViewModel
            {
                EnquiryId = id,
                StudentInfo = enquiry,
                NewFollowUp = new EnquiryFollowUp
                {
                    Id = Guid.NewGuid(),
                    EnquiryId = id,  // Ensure this is set
                    FollowDate = DateTime.Today,
                    FollowTime = DateTime.Now.TimeOfDay,
                    NextFollowDate= DateTime.Today,
                    InterestLevel = "Pending"
                },
                PreviousFollowUps = enquiry.FollowUps?.ToList() ?? new List<EnquiryFollowUp>()
            };

            PopulateCallStatusDropDown();
            PopulateInterestLevelDropDown();

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"FollowUp GET - EnquiryId: {id}, NewFollowUp.EnquiryId: {viewModel.NewFollowUp.EnquiryId}");

            // Return partial view for modal
            return PartialView("_FollowUpPartial", viewModel);
        }

        // POST: Enquiry/FollowUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FollowUp(FollowUpViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var sessionId = CurrentSessionID;
                    var tenantCode = TenantCode;
                    var currentUserId = CurrentTenantUserID;

                    // Ensure EnquiryId is set properly
                    if (viewModel.NewFollowUp.EnquiryId == Guid.Empty)
                    {
                        viewModel.NewFollowUp.EnquiryId = viewModel.EnquiryId;
                    }

                    // Debug logging
                    System.Diagnostics.Debug.WriteLine($"FollowUp POST - EnquiryId: {viewModel.EnquiryId}, NewFollowUp.EnquiryId: {viewModel.NewFollowUp.EnquiryId}");

                    // Validate that the enquiry exists before creating follow-up
                    var enquiry = await _enquiryRepository.GetEnquiryByIdAsync(viewModel.NewFollowUp.EnquiryId, sessionId, tenantCode);
                    if (enquiry == null)
                    {
                        ModelState.AddModelError("", "The enquiry record was not found. Please refresh and try again.");
                    }
                    else
                    {
                        var id = await _enquiryRepository.CreateFollowUpAsync(viewModel.NewFollowUp, sessionId, tenantCode, currentUserId);

                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = true, message = "Follow-up recorded successfully!" });
                        }

                        TempData["SuccessMessage"] = "Follow-up recorded successfully!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    // Log the full error for debugging
                    System.Diagnostics.Debug.WriteLine($"Follow-up creation error: {ex.Message}");

                    string errorMessage = "Error recording follow-up: ";

                    // Handle specific database errors
                    if (ex.Message.Contains("FOREIGN KEY constraint"))
                    {
                        errorMessage += "The enquiry record is no longer available. Please refresh the page and try again.";
                    }
                    else if (ex.Message.Contains("INSERT statement"))
                    {
                        errorMessage += "There was a database error. Please check the data and try again.";
                    }
                    else
                    {
                        errorMessage += ex.Message;
                    }

                    ModelState.AddModelError("", errorMessage);
                }
            }

            // Handle validation errors or database errors
            try
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;

                // Reload data on error
                viewModel.StudentInfo = await _enquiryRepository.GetEnquiryByIdAsync(viewModel.EnquiryId, sessionId, tenantCode);
                if (viewModel.StudentInfo == null)
                {
                    // If enquiry doesn't exist, return error for AJAX or redirect for non-AJAX
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "The enquiry record was not found. Please refresh the page." });
                    }
                    TempData["ErrorMessage"] = "The enquiry record was not found.";
                    return RedirectToAction("Index");
                }

                viewModel.PreviousFollowUps = (await _enquiryRepository.GetFollowUpsByEnquiryIdAsync(viewModel.EnquiryId, sessionId, tenantCode)).ToList();

                // Ensure the NewFollowUp object has the correct EnquiryId for the form reload
                if (viewModel.NewFollowUp.EnquiryId == Guid.Empty)
                {
                    viewModel.NewFollowUp.EnquiryId = viewModel.EnquiryId;
                }

                PopulateCallStatusDropDown();
                PopulateInterestLevelDropDown();

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_FollowUpPartial", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // If we can't even reload the data, there's a serious issue
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Unable to load enquiry data. Please refresh the page and try again." });
                }

                TempData["ErrorMessage"] = "Unable to load enquiry data. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;
                var currentUserId = CurrentTenantUserID;

                var success = await _enquiryRepository.DeleteEnquiryAsync(id, sessionId, tenantCode, currentUserId);
                if (success)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = true, message = "Enquiry deleted successfully!" }, JsonRequestBehavior.AllowGet);
                    }

                    TempData["SuccessMessage"] = "Enquiry deleted successfully!";
                }
                else
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Error deleting enquiry." }, JsonRequestBehavior.AllowGet);
                    }

                    TempData["ErrorMessage"] = "Error deleting enquiry.";
                }
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Error deleting enquiry: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }

                TempData["ErrorMessage"] = "Error deleting enquiry: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<ActionResult> DeleteFollowup(Guid id)
        {
            try
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;
                var currentUserId = CurrentTenantUserID;

                var success = await _enquiryRepository.DeleteFollowUpAsync(id, sessionId, tenantCode, currentUserId);
                if (success)
                {
                        return Json(new { success = true, message = "Enquiry deleted successfully!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                        return Json(new { success = false, message = "Error deleting enquiry." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Error deleting enquiry: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }

                TempData["ErrorMessage"] = "Error deleting enquiry: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: Enquiry/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            var sessionId = CurrentSessionID;
            var tenantCode = TenantCode;

            var enquiry = await _enquiryRepository.GetEnquiryByIdAsync(id, sessionId, tenantCode);
            if (enquiry == null)
            {
                return HttpNotFound();
            }

            var viewModel = new EnquiryViewModel
            {
                Enquiry = enquiry,
                FollowUps = enquiry.FollowUps?.ToList() ?? new List<EnquiryFollowUp>()
            };

            return View(viewModel);
        }
        // Add this method to your EnquiryController.cs

        // Add this method to your EnquiryController.cs (or create AdmissionController)

        [HttpPost]
        public async Task<ActionResult> SaveAdmission(Guid studentId, Guid? enquiryId = null)
        {
            try
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;
                var currentUserId = CurrentTenantUserID;

                // Use studentId as enquiryId if enquiryId is not provided
                Guid actualEnquiryId = enquiryId ?? studentId;

                // Validate that the enquiry exists and is not already admitted
                var enquiry = await _enquiryRepository.GetEnquiryByIdAsync(actualEnquiryId, sessionId, tenantCode);
                if (enquiry == null)
                {
                    return Json(new { success = false, message = "Enquiry not found." }, JsonRequestBehavior.AllowGet);
                }

                if (enquiry.InterestLevel == "Admitted" || enquiry.InterestLevel == "Completed")
                {
                    return Json(new { success = false, message = "This enquiry has already been converted to admission." }, JsonRequestBehavior.AllowGet);
                }

                // Call repository method to convert enquiry to admission
                var result = await _enquiryRepository.ConvertEnquiryToAdmissionAsync(
                    actualEnquiryId,
                    tenantCode, // Using tenantCode as SchoolCode
                    DateTime.Today, // Today's date as admission date
                    sessionId,
                    tenantCode,
                    currentUserId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        admissionNo = result.AdmissionNo,
                        studentName = enquiry.Student
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"SaveAdmission Error: {ex.Message}");

                return Json(new
                {
                    success = false,
                    message = "Error saving admission: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        // Helper Methods to get current session/tenant/user info
        // These methods should be implemented in your BaseController

        private void PopulateDropDowns()
        {
            ViewBag.ClassList = new SelectList(new[]
            {
                "Nursery", "LKG", "UKG", "1st", "2nd", "3rd", "4th", "5th",
                "6th", "7th", "8th", "9th", "10th", "11th", "12th"
            });

            ViewBag.GenderList = new SelectList(new[] { "Male", "Female" });

            ViewBag.RelationList = new SelectList(new[]
            {
                "Father", "Mother", "Guardian", "Other"
            });

            ViewBag.SourceList = new SelectList(new[]
            {
                "General Enquiry", "NewPaper", "Hording", "Website", "Reference",
                "Social Media", "Facebook Ads", "Google Ads", "Teacher", "Other"
            });

            ViewBag.SendSMSList = new SelectList(new[] { "No", "Yes" });
        }

        private void PopulateCallStatusDropDown()
        {
            ViewBag.CallStatusList = new SelectList(new[]
            {
                "Answer", "Not Answer", "Busy", "Switched Off", "Call Cut",
                "No Incoming", "Call Not Connect", "Wrong No.", "No. In-active",
                "Out of Service", "Out of Network"
            });
        }

        private void PopulateInterestLevelDropDown()
        {
            ViewBag.InterestLevelList = new SelectList(new[]
            {
                "Pending", "Hot", "Excellent", "Very Good", "Normal",
                "Not Interested", "Negative", "Bad", "Completed"
            });
        }

        private void ParseDateRange(string dateRange, EnquiryFilterViewModel filters)
        {
            if (string.IsNullOrEmpty(dateRange)) return;

            var dates = dateRange.Split('-');
            if (dates.Length == 2)
            {
                if (DateTime.TryParseExact(dates[0].Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                {
                    filters.StartDate = startDate;
                }

                if (DateTime.TryParseExact(dates[1].Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                {
                    filters.EndDate = endDate;
                }
            }
        }

        private byte[] GenerateExcelReport(IEnumerable<StudentEnquiry> enquiries)
        {
            // Implement your Excel generation logic here
            // This is a placeholder - you'll need to use a library like EPPlus, ClosedXML, etc.

            // Example using EPPlus (you'll need to install the EPPlus NuGet package):
            /*
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Student Enquiries");
                
                // Add headers
                worksheet.Cells[1, 1].Value = "Enq No.";
                worksheet.Cells[1, 2].Value = "Student Name";
                worksheet.Cells[1, 3].Value = "Father Name";
                worksheet.Cells[1, 4].Value = "Mother Name";
                worksheet.Cells[1, 5].Value = "Mobile";
                worksheet.Cells[1, 6].Value = "Class";
                worksheet.Cells[1, 7].Value = "Gender";
                worksheet.Cells[1, 8].Value = "Enquiry Date";
                worksheet.Cells[1, 9].Value = "Payment Status";
                worksheet.Cells[1, 10].Value = "Interest Level";
                
                // Add data
                int row = 2;
                foreach (var enquiry in enquiries)
                {
                    worksheet.Cells[row, 1].Value = enquiry.EnqNo;
                    worksheet.Cells[row, 2].Value = enquiry.Student;
                    worksheet.Cells[row, 3].Value = enquiry.Father;
                    worksheet.Cells[row, 4].Value = enquiry.Mother;
                    worksheet.Cells[row, 5].Value = enquiry.Mobile1;
                    worksheet.Cells[row, 6].Value = enquiry.ApplyingForClass;
                    worksheet.Cells[row, 7].Value = enquiry.Gender;
                    worksheet.Cells[row, 8].Value = enquiry.EnquiryDate.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 9].Value = enquiry.PaymentStatus;
                    worksheet.Cells[row, 10].Value = enquiry.InterestLevel;
                    row++;
                }
                
                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();
                
                return package.GetAsByteArray();
            }
            */

            // For now, return empty byte array
            return new byte[0];
        }
    }
}