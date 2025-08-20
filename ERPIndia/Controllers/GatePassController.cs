using Dapper;
using ERPIndia.Repositories.GatePass;
using ERPIndia.Utilities;
using ERPK12Models.ViewModel.GatePass;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class GatePassController : BaseController
    {
        private readonly IGatePassRepository _gatePassRepository;
        private readonly string _connectionString;
        private readonly DropdownController _dropdownController;

        public GatePassController()
        {
            _gatePassRepository = new GatePassRepository();
        }

        public GatePassController(IGatePassRepository gatePassRepository)
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            _gatePassRepository = gatePassRepository;
            _dropdownController = new DropdownController();
        }

        private List<SelectListItem> ConvertToSelectListDefault(JsonResult result)
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
                            Name = "ALL"
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
                return new List<SelectListItem>();
            }
        }

        // GET: GatePass - Shows the search form and list
        public ActionResult Index()
        {
            var classesResult = new DropdownController().GetClasses();
            var viewModel = new GatePassListViewModel();
            viewModel.Filters = new GatePassFilterViewModel();
            viewModel.Filters.ClassList = ConvertToSelectListDefault(classesResult);

            // Initialize empty gate passes list for initial load
            viewModel.GatePasses = new List<GatePassRecord>();
            viewModel.Pagination = new PaginationViewModel
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalRecords = 0
            };

            return View(viewModel);
        }

        // POST: GatePass/Search - Handles AJAX search requests
        [HttpPost]
        public async Task<ActionResult> Search(GatePassFilterViewModel filters, int page = 1, int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;

                var viewModel = new GatePassListViewModel
                {
                    Filters = filters ?? new GatePassFilterViewModel(),
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize
                    }
                };

                // Handle date filters
                if (viewModel.Filters.FromDate.HasValue && !viewModel.Filters.StartDate.HasValue)
                {
                    viewModel.Filters.StartDate = viewModel.Filters.FromDate;
                }
                if (viewModel.Filters.ToDate.HasValue && !viewModel.Filters.EndDate.HasValue)
                {
                    viewModel.Filters.EndDate = viewModel.Filters.ToDate;
                }

                // Handle empty class selection
                if (!string.IsNullOrEmpty(viewModel.Filters.SelectedClass) &&
                    viewModel.Filters.SelectedClass == Guid.Empty.ToString())
                {
                    viewModel.Filters.SelectedClass = null;
                }

                // Validate date range
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

                // Check if this is a default search
                bool isDefaultSearch = IsDefaultSearch(viewModel.Filters);
                // Get gate passes based on search criteria
                var gatePassesTask = _gatePassRepository.GetAllGatePassesAsync(sessionId, tenantCode, viewModel.Filters, page, pageSize);
                var countTask = _gatePassRepository.GetTotalGatePassesCountAsync(sessionId, tenantCode, viewModel.Filters);

                await Task.WhenAll(gatePassesTask, countTask);

                viewModel.GatePasses = gatePassesTask.Result?.ToList() ?? new List<GatePassRecord>();
                viewModel.Pagination.TotalRecords = countTask.Result;

                // Calculate pagination info
                viewModel.Pagination.TotalPages = (int)Math.Ceiling((double)viewModel.Pagination.TotalRecords / pageSize);

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_GatePassSearchResults", viewModel);
                }

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Search failed. Please try again." });
                }

                TempData["ErrorMessage"] = "Search failed. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // Helper method to check if this is a default search
        private bool IsDefaultSearch(GatePassFilterViewModel filters)
        {
            return filters == null ||
                   (!filters.FromDate.HasValue &&
                    !filters.ToDate.HasValue &&
                    string.IsNullOrEmpty(filters.SelectedClass) &&
                    string.IsNullOrEmpty(filters.StudentName) &&
                    string.IsNullOrEmpty(filters.PassNo) &&
                    !filters.StartDate.HasValue &&
                    !filters.EndDate.HasValue);
        }

        // GET: GatePass/Create
        public ActionResult Create()
        {

            var viewModel = new GatePassViewModel();
            var classesResult = new DropdownController().GetClasses();
            viewModel.ClassList = ConvertToSelectListDefault(classesResult);

            // Initialize the GatePass object
            viewModel.GatePass = new GatePassRecord
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Today,
                TimeIn = DateTime.Now.TimeOfDay,
                PrintTime = DateTime.Now
            };

            // DO NOT generate Pass Number here - it will be generated on save
            viewModel.GatePass.PassNo = "AUTO-GENERATE"; // Placeholder text

            PopulateDropDowns();

            return PartialView("_CreatePartial", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(GatePassViewModel viewModel)
        {
            try
            {
                // Initialize dropdown data first
                var classesResult = new DropdownController().GetClasses();
                viewModel.ClassList = ConvertToSelectListDefault(classesResult);

                // Handle time parsing
                var timeInStr = Request.Form["GatePass.TimeIn"];
                var timeOutStr = Request.Form["GatePass.TimeOut"];
                viewModel.GatePass.ReasonFor = Request.Form["GatePass.ReasonFor"];
                viewModel.GatePass.ReasonForLeave = Request.Form["GatePass.ReasonForLeave"];

                // Remove from ModelState since we'll handle manually
                ModelState.Remove("GatePass.TimeIn");
                ModelState.Remove("GatePass.TimeOut");

                // SERVER-SIDE VALIDATION - ENFORCE ALL REQUIRED FIELDS
                var validationErrors = new List<string>();

                // 1. Validate Student Name
                if (string.IsNullOrWhiteSpace(viewModel.GatePass.StudentName))
                {
                    validationErrors.Add("Student name is required");
                    ModelState.AddModelError("GatePass.StudentName", "Student name is required");
                }

                // 2. Validate Student ID
                if (!viewModel.GatePass.StudentId.HasValue || viewModel.GatePass.StudentId == Guid.Empty)
                {
                    validationErrors.Add("Please select a valid student from the search results");
                    ModelState.AddModelError("GatePass.StudentId", "Please select a valid student from the search results");
                }

                // 3. Validate Parent/Guardian Name
                if (string.IsNullOrWhiteSpace(viewModel.GatePass.ParentGuardianName))
                {
                    validationErrors.Add("Parent/Guardian name is required");
                    ModelState.AddModelError("GatePass.ParentGuardianName", "Parent/Guardian name is required");
                }

                // 4. Validate Guardian Mobile
                if (string.IsNullOrWhiteSpace(viewModel.GatePass.GuardianMobile))
                {
                    validationErrors.Add("Guardian mobile number is required");
                    ModelState.AddModelError("GatePass.GuardianMobile", "Guardian mobile number is required");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(viewModel.GatePass.GuardianMobile, @"^\d{10}$"))
                {
                    validationErrors.Add("Guardian mobile must be exactly 10 digits");
                    ModelState.AddModelError("GatePass.GuardianMobile", "Guardian mobile must be exactly 10 digits");
                }

                // 5. Validate Relationship to Student
                if (string.IsNullOrWhiteSpace(viewModel.GatePass.RelationshipToStudent))
                {
                    validationErrors.Add("Relationship to student is required");
                    ModelState.AddModelError("GatePass.RelationshipToStudent", "Relationship to student is required");
                }

                // 6. Validate Reason for Leave
                if (string.IsNullOrWhiteSpace(viewModel.GatePass.ReasonFor) &&
                    string.IsNullOrWhiteSpace(viewModel.GatePass.ReasonForLeave))
                {
                    validationErrors.Add("Reason for leave is required");
                    ModelState.AddModelError("GatePass.ReasonForLeave", "Reason for leave is required");
                }

                // 7. Validate Class ID
                if (!viewModel.GatePass.ClassId.HasValue || viewModel.GatePass.ClassId == Guid.Empty)
                {
                    validationErrors.Add("Student class information is missing");
                    ModelState.AddModelError("GatePass.ClassId", "Student class information is missing");
                }

                // 8. Validate Date
                if (viewModel.GatePass.Date == default(DateTime))
                {
                    validationErrors.Add("Date is required");
                    ModelState.AddModelError("GatePass.Date", "Date is required");
                }
                else if (viewModel.GatePass.Date.Date < DateTime.Today)
                {
                    validationErrors.Add("Gate pass date cannot be in the past");
                    ModelState.AddModelError("GatePass.Date", "Gate pass date cannot be in the past");
                }

                // Convert time formats
                if (!string.IsNullOrEmpty(timeInStr))
                {
                    try
                    {
                        viewModel.GatePass.TimeIn = Parse12HourTime(timeInStr);
                    }
                    catch (Exception ex)
                    {
                        validationErrors.Add("Invalid time format for Time In");
                        ModelState.AddModelError("GatePass.TimeIn", "Invalid time format for Time In");
                        Logger.Error($"Time parsing error for TimeIn '{timeInStr}': {ex.Message}");
                    }
                }
                else
                {
                    validationErrors.Add("Time In is required");
                    ModelState.AddModelError("GatePass.TimeIn", "Time In is required");
                }

                if (!string.IsNullOrEmpty(timeOutStr))
                {
                    try
                    {
                        viewModel.GatePass.TimeOut = Parse12HourTime(timeOutStr);
                    }
                    catch (Exception ex)
                    {
                        validationErrors.Add("Invalid time format for Time Out");
                        ModelState.AddModelError("GatePass.TimeOut", "Invalid time format for Time Out");
                        Logger.Error($"Time parsing error for TimeOut '{timeOutStr}': {ex.Message}");
                    }
                }
                else
                {
                    validationErrors.Add("Time Out is required");
                    ModelState.AddModelError("GatePass.TimeOut", "Time Out is required");
                }

                // Validate time logic
                if (viewModel.GatePass.TimeIn.HasValue && viewModel.GatePass.TimeOut.HasValue)
                {
                    if (viewModel.GatePass.TimeOut <= viewModel.GatePass.TimeIn)
                    {
                        validationErrors.Add("Time Out must be after Time In");
                        ModelState.AddModelError("GatePass.TimeOut", "Time Out must be after Time In");
                    }
                }

                // If validation errors exist, return early
                if (validationErrors.Any())
                {
                    Logger.Error($"Server-side validation failed: {string.Join(", ", validationErrors)}");

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Please fix the following errors: " + string.Join(", ", validationErrors),
                            errors = validationErrors
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (ModelState.IsValid)
                {
                    var sessionId = CurrentSessionID;
                    var tenantCode = TenantCode;
                    var tenantId = CurrentTenantID;
                    var currentUserId = CurrentTenantUserID;

                    // Set print time
                    viewModel.GatePass.PrintTime = DateTime.Now;

                    // Generate Pass Number
                    var passNo = _gatePassRepository.GeneratePassNumber(sessionId, tenantCode);
                    if (string.IsNullOrEmpty(passNo))
                    {
                        throw new InvalidOperationException("Failed to generate pass number");
                    }
                    viewModel.GatePass.PassNo = passNo;

                    Logger.Info($"Creating gate pass for: {viewModel.GatePass.StudentName}, Pass No: {passNo}");

                    // Create the gate pass
                    var id = await _gatePassRepository.CreateGatePassAsync(
                        viewModel.GatePass,
                        sessionId,
                        tenantCode,
                        currentUserId,
                        tenantId
                    );

                    Logger.Info($"Gate pass created with ID: {id}, Pass No: {passNo}");

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Gate pass created successfully!",
                            gatePassId = id,
                            passNo = viewModel.GatePass.PassNo
                        }, JsonRequestBehavior.AllowGet);
                    }

                    TempData["SuccessMessage"] = "Gate pass created successfully!";
                    TempData["PassNo"] = viewModel.GatePass.PassNo;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in Create action: {ex}");

                string errorMessage = ex is InvalidOperationException || ex is ArgumentException
                    ? ex.Message
                    : "An unexpected error occurred. Please try again.";

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        message = errorMessage
                    }, JsonRequestBehavior.AllowGet);
                }

                ModelState.AddModelError("", errorMessage);
            }

            // Return view with errors
            PopulateDropDowns();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_CreatePartial", viewModel);
            }

            return View(viewModel);
        }

        // Helper method to validate required session data
        private bool IsValidSession()
        {
            return true;
        }

        // Improved time parsing with better error handling
        private TimeSpan? Parse12HourTime(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return null;

            // Try multiple formats
            string[] formats = {
        "h:mm tt", "hh:mm tt", "h:mm:ss tt", "hh:mm:ss tt",
        "h:mm", "hh:mm", "H:mm", "HH:mm"
    };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(timeString.Trim(), format,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                {
                    return dateTime.TimeOfDay;
                }
            }

            throw new FormatException($"Unable to parse time: {timeString}");
        }

        // GET: GatePass/Edit/5
        public async Task<ActionResult> Edit(Guid id)
        {
            var sessionId = CurrentSessionID;
            var tenantCode = TenantCode;
            var classesResult = new DropdownController().GetClasses();

            var gatePass = await _gatePassRepository.GetGatePassByIdAsync(id, sessionId, tenantCode);
            if (gatePass == null)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Gate pass not found." });
                }
                return HttpNotFound();
            }

            var viewModel = new GatePassViewModel
            {
                GatePass = gatePass
            };
            viewModel.ClassList = ConvertToSelectListDefault(classesResult);
            PopulateDropDowns();

            return PartialView("_EditPartial", viewModel);
        }

        // POST: GatePass/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(GatePassViewModel viewModel)
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

                    var success = await _gatePassRepository.UpdateGatePassAsync(viewModel.GatePass, sessionId, tenantCode, currentUserId);
                    if (success)
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = true, message = "Gate pass updated successfully!" });
                        }

                        TempData["SuccessMessage"] = "Gate pass updated successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error updating gate pass.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating gate pass: " + ex.Message);
                }
            }

            if (Request.IsAjaxRequest())
            {
                PopulateDropDowns();
                return PartialView("_EditPartial", viewModel);
            }

            PopulateDropDowns();
            return View(viewModel);
        }

        // GET: GatePass/History/studentName
        public async Task<ActionResult> History(Guid studentId)
        {
            var sessionId = CurrentSessionID;
            var tenantCode = TenantCode;
            var GatePasses = await _gatePassRepository.GetGatePassHistoryByStudentIdAsync(studentId, sessionId, tenantCode);
            var viewModel = new GatePassHistoryViewModel
            {
                StudentName = GatePasses.Any()? GatePasses.First().StudentName: String.Empty,
                GatePasses = GatePasses.ToList(),
            };

            // Calculate yearly statistics
            viewModel.YearlyStats = viewModel.GatePasses
                .GroupBy(g => g.Date.Year)
                .Select(g => new YearlyGatePassStats
                {
                    Year = g.Key,
                    TotalPasses = g.Count()
                })
                .OrderByDescending(y => y.Year)
                .ToList();

            return PartialView("_HistoryPartial", viewModel);
        }

        // POST: GatePass/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;
                var currentUserId = CurrentTenantUserID;

                var success = await _gatePassRepository.DeleteGatePassAsync(id, sessionId, tenantCode, currentUserId);
                if (success)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = true, message = "Gate pass deleted successfully!" }, JsonRequestBehavior.AllowGet);
                    }

                    TempData["SuccessMessage"] = "Gate pass deleted successfully!";
                }
                else
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Error deleting gate pass." }, JsonRequestBehavior.AllowGet);
                    }

                    TempData["ErrorMessage"] = "Error deleting gate pass.";
                }
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Error deleting gate pass: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }

                TempData["ErrorMessage"] = "Error deleting gate pass: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: GatePass/Print/5
        public ActionResult PrintGatePass(Guid? id = null)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    db.Open();

                    const string proc = "dbo.usp_GatePass_GetById";
                    var p = new DynamicParameters();
                    p.Add("@TenantCode", CurrentTenantCode);
                    p.Add("@SessionID", CurrentSessionID);
                    p.Add("@GatePassId", id);

                    var result = db.Query<GatePassInfo>(proc, p, commandType: CommandType.StoredProcedure).ToList();
                   

                    var studentsDataTable = Utils.ConvertToDataTable(result);

                    // Process student photos if the column exists
                    if (studentsDataTable.Columns.Contains("D11") || studentsDataTable.Columns.Contains("D11"))
                    {
                        string photoColumnName = studentsDataTable.Columns.Contains("D11") ? "D11" : "D11";

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

                    return PartialView("_GatePass", viewModel);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while retrieving transport data.");
                return Content("Error: No students found ");
            }
        }


        // Helper Methods
        private void PopulateDropDowns()
        {
            ViewBag.RelationshipList = new SelectList(new[]
            {
        "Father", "Mother", "Guardian", "Driver", "Other"
    });
            var reasonDict = new Dictionary<string, string>
{
    {"Medical Emergency",   "Student is not feeling well and needs to visit the doctor"},
    {"Family Emergency",    "Urgent family matter requiring the student's presence."},
    {"Personal Reason",     "Leaving due to personal reasons (informed and approved by parent/guardian)."},
    {"Home Emergency",      "Unexpected situation at home – needs to leave early."},
    {"Religious Reason",    "Going for religious observance or function."},
    {"Request by Parent",   "Parent has requested early pickup due to travel/personal reason."},
    {"Transport Issue",     "No transport available later – student needs to leave now."},
    {"School Event",        "Leaving for an inter-school event/competition/workshop."},
    {"Other",               "Other"}
};

            // NOTE: dataValueField = "Value" (the long description)
            //       dataTextField  = "Key"   (the short label)
            ViewBag.ReasonList = new SelectList(reasonDict, "Value", "Key");

        }

        // POST: GatePass/SearchStudents - For autocomplete
        [HttpPost]
        public async Task<ActionResult> SearchStudents(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var sessionId = CurrentSessionID;
            var tenantCode = TenantCode;

            var students = await _gatePassRepository.SearchStudentsAsync(term, sessionId, tenantCode);

            var result = students.Select(s => new
            {
                id = s.Id,
                classId = s.ClassId,
                studentId = s.StudentId,
                label = $"{s.Student} - {s.Father} - {s.Class} =#{s.AdmNo} ",
                value = s.Student,
                student = s.Student,
                father = s.Father,
                mother = s.Mother,
                @class = s.Class,
                section = s.Section,
                classSection = $"{s.Class} - {s.Section}",
                address = s.Address,
                mobile1 = s.Mobile1,
                mobile2 = s.Mobile2,
                admNo = s.AdmNo
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ExportHistory(Guid studentId)
        {
            try
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;

                // Get gate pass history
                var gatePasses = _gatePassRepository.GetGatePassHistoryByStudentIdAsync(studentId, sessionId, tenantCode).Result;

                if (!gatePasses.Any())
                {
                    return Json(new { success = false, message = "No data to export" });
                }

                // Create Excel package
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Gate Pass History");

                    // Add headers
                    worksheet.Cells[1, 1].Value = "Pass No";
                    worksheet.Cells[1, 2].Value = "Date";
                    worksheet.Cells[1, 3].Value = "Day";
                    worksheet.Cells[1, 4].Value = "Time In";
                    worksheet.Cells[1, 5].Value = "Time Out";
                    worksheet.Cells[1, 6].Value = "Guardian Name";
                    worksheet.Cells[1, 7].Value = "Relationship";
                    worksheet.Cells[1, 8].Value = "Mobile";
                    worksheet.Cells[1, 9].Value = "Reason";
                    worksheet.Cells[1, 10].Value = "Print Count";

                    // Style headers
                    using (var range = worksheet.Cells[1, 1, 1, 10])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    // Add data
                    var row = 2;
                    foreach (var gp in gatePasses.OrderByDescending(g => g.Date))
                    {
                        worksheet.Cells[row, 1].Value = gp.PassNo;
                        worksheet.Cells[row, 2].Value = gp.Date.ToString("dd-MMM-yyyy");
                        worksheet.Cells[row, 3].Value = gp.Date.ToString("dddd");
                        worksheet.Cells[row, 4].Value = gp.TimeIn?.ToString(@"hh\:mm") ?? "N/A";
                        worksheet.Cells[row, 5].Value = gp.TimeOut?.ToString(@"hh\:mm") ?? "N/A";
                        worksheet.Cells[row, 6].Value = gp.ParentGuardianName ?? "N/A";
                        worksheet.Cells[row, 7].Value = gp.RelationshipToStudent ?? "N/A";
                        worksheet.Cells[row, 8].Value = gp.GuardianMobile ?? "N/A";
                        worksheet.Cells[row, 9].Value = gp.ReasonForLeave ?? "";
                        worksheet.Cells[row, 10].Value = gp.PrintCount;
                        row++;
                    }

                    // Add student info at the top
                    worksheet.InsertRow(1, 2);
                    worksheet.Cells[1, 1].Value = "Student Name:";
                    worksheet.Cells[1, 2].Value = gatePasses.First().StudentName;
                    worksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Generate file
                    var fileName = $"GatePassHistory_{gatePasses.First().StudentName.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error exporting gate pass history: {ex}");
                return Json(new { success = false, message = "Error exporting data" });
            }
        }
    }
}