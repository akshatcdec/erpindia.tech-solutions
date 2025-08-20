using Dapper;
using ERPIndia.Models.CollectFee.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class ExamScheduleViewModel
    {
        public ExamScheduleViewModel()
        {
            ClassList = new List<SelectListItem>();
            SectionList = new List<SelectListItem>();
            ExamList = new List<SelectListItem>();
            ScheduleEntries = new List<ExamScheduleEntry>();
        }

        public string SelectedClass { get; set; }
        public string SelectedExam { get; set; }
        public string SelectedSection { get; set; }

        public List<SelectListItem> ClassList { get; set; }
        public List<SelectListItem> SectionList { get; set; }
        public List<SelectListItem> ExamList { get; set; }

        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Guid TenantId { get; set; }
        public Guid SessionId { get; set; }
        public Guid? ClassId { get; set; }
        public string SectionId { get; set; }
        public List<ExamScheduleEntry> ScheduleEntries { get; set; }
    }
    public class ExistingScheduleData
    {
        public Guid SubjectID { get; set; }
        public DateTime? ExamDate { get; set; }
        public string ExamTime { get; set; }
        public string SubjectName { get; set; }
        public string Exam { get; set; }
    }
    public class ExamScheduleEntry
    {
        public int Sr { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }

        public string Day { get; set; }

        public string Time { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        public Guid SubjectId { get; set; }
    }

    public class ExamScheduleController : BaseController
    {
        private readonly string _connectionString;
        public ExamScheduleController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
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

        // Overloaded method for direct List<DropdownItem>
        private List<SelectListItem> ConvertToSelectListDefault(List<DropdownItem> items)
        {
            try
            {
                if (items != null)
                {
                    var newList = new List<DropdownItem>
                    {
                        new DropdownItem
                        {
                            Id = Guid.Empty,
                            Name = "-- Select --"
                        }
                    };

                    newList.AddRange(items);

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

        // GET: ExamSchedule
        public ActionResult Index()
        {
            var classesResult = new DropdownController().GetClasses();
            var sectionsResult = new DropdownController().GetSections();
            var examsResult = new DropdownController().GetExamSchedule();

            var viewModel = new ExamScheduleViewModel
            {
                ClassList = ConvertToSelectListDefault(classesResult),
                SectionList = ConvertToSelectListDefault(sectionsResult),
                ExamList = ConvertToSelectListDefault(examsResult),
                TenantId = CurrentTenantID,
                SessionId = CurrentSessionID
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult GenerateSchedule(ExamScheduleViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.SelectedClass) ||
                    string.IsNullOrEmpty(model.SelectedSection) ||
                    string.IsNullOrEmpty(model.SelectedExam))
                {
                    return Json(new { success = false, message = "Please select all required fields." });
                }

                // Get subjects based on class and section
                var dropdownController = new DropdownController();
                var subjects = dropdownController.GetSubjectsByClassSection(
                    Guid.Parse(model.SelectedClass),
                    Guid.Parse(model.SelectedSection)
                );

                // Check for existing schedule
                var existingScheduleData = new Dictionary<Guid, (DateTime? Date, string Time)>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = @"
                        SELECT SubjectID, ExamDate, ExamTime
                        FROM dbo.ExamSchedule
                        WHERE TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND ClassID = @ClassID 
                        AND SectionID = @SectionID 
                        AND ExamID = @ExamID 
                        AND IsDeleted = 0";

                    var existingSchedules = connection.Query<ExistingScheduleData>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID,
                        ClassID = Guid.Parse(model.SelectedClass),
                        SectionID = Guid.Parse(model.SelectedSection),
                        ExamID = Guid.Parse(model.SelectedExam)
                    });

                    foreach (var schedule in existingSchedules)
                    {
                        existingScheduleData[schedule.SubjectID] = (schedule.ExamDate, schedule.ExamTime ?? "");
                    }
                }

                // subjects is already a List<DropdownItem>, no need for JSON conversion
                if (subjects != null && subjects.Any())
                {
                    model.ScheduleEntries = subjects.Select((subject, idx) => {
                        var entry = new ExamScheduleEntry
                        {
                            Sr = idx + 1,
                            SubjectId = subject.Id,
                            Subject = subject.Name,
                            Date = null,
                            Day = "",
                            Time = ""
                        };

                        // Populate existing data if available
                        if (existingScheduleData.ContainsKey(subject.Id))
                        {
                            var existingData = existingScheduleData[subject.Id];
                            entry.Date = existingData.Date;
                            entry.Time = existingData.Time ?? "";

                            // Calculate day if date exists
                            if (entry.Date.HasValue)
                            {
                                var days = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                                entry.Day = days[(int)entry.Date.Value.DayOfWeek];
                            }
                        }

                        return entry;
                    }).ToList();
                }
                else
                {
                    model.ScheduleEntries = new List<ExamScheduleEntry>();
                }

                // Regenerate dropdown lists for the partial view
                model.ClassList = ConvertToSelectListDefault(new DropdownController().GetClasses());
                model.SectionList = ConvertToSelectListDefault(new DropdownController().GetSections());
                model.ExamList = ConvertToSelectListDefault(new DropdownController().GetExams());

                // Add flag to indicate if this is an update
                ViewBag.IsUpdate = existingScheduleData.Any();

                return PartialView("_ScheduleTable", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult SaveSchedule(ExamScheduleViewModel model)
        {
            try
            {
                // Remove default model state validation for Date and Time fields
                for (int i = 0; i < model.ScheduleEntries?.Count; i++)
                {
                    ModelState.Remove($"ScheduleEntries[{i}].Date");
                    ModelState.Remove($"ScheduleEntries[{i}].Time");
                }

                // Custom validation
                if (model.ScheduleEntries == null || !model.ScheduleEntries.Any())
                {
                    ModelState.AddModelError("", "No schedule entries to save.");
                    return PrepareViewModelAndReturn(model);
                }

                // Check if at least one entry has a date
                bool hasAtLeastOneDate = model.ScheduleEntries.Any(e => e.Date.HasValue);
                if (!hasAtLeastOneDate)
                {
                    ModelState.AddModelError("", "At least one subject must have an exam date.");
                }

                // Validate each entry - if date is provided, time is required
                for (int i = 0; i < model.ScheduleEntries.Count; i++)
                {
                    var entry = model.ScheduleEntries[i];

                    if (entry.Date.HasValue && string.IsNullOrWhiteSpace(entry.Time))
                    {
                        ModelState.AddModelError($"ScheduleEntries[{i}].Time",
                            $"Time is required for {entry.Subject} when date is provided.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    return PrepareViewModelAndReturn(model);
                }

                // Save to database using Dapper
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // First, delete existing schedules for this exam to handle updates
                            var deleteQuery = @"
                                DELETE FROM dbo.ExamSchedule 
                                WHERE TenantID = @TenantID 
                                AND SessionID = @SessionID 
                                AND ClassID = @ClassID 
                                AND SectionID = @SectionID 
                                AND ExamID = @ExamID";

                            connection.Execute(deleteQuery, new
                            {
                                TenantID = CurrentTenantID,
                                SessionID = CurrentSessionID,
                                ClassID = Guid.Parse(model.SelectedClass),
                                SectionID = Guid.Parse(model.SelectedSection),
                                ExamID = Guid.Parse(model.SelectedExam)
                            }, transaction);

                            // Insert new schedules - only for entries that have dates
                            var entriesToSave = model.ScheduleEntries.Where(e => e.Date.HasValue).ToList();

                            if (entriesToSave.Any())
                            {
                                var insertQuery = @"
                                    INSERT INTO dbo.ExamSchedule 
                                    (ExamScheduleID, TenantID, TenantCode, SessionID, ClassID, SectionID, 
                                     ExamID, SubjectID, ExamDate, ExamTime, IsActive, IsDeleted, 
                                     CreatedBy, CreatedDate)
                                    VALUES 
                                    (@ExamScheduleID, @TenantID, @TenantCode, @SessionID, @ClassID, @SectionID, 
                                     @ExamID, @SubjectID, @ExamDate, @ExamTime, @IsActive, @IsDeleted, 
                                     @CreatedBy, @CreatedDate)";

                                foreach (var entry in entriesToSave)
                                {
                                    var parameters = new
                                    {
                                        ExamScheduleID = Guid.NewGuid(),
                                        TenantID = CurrentTenantID,
                                        TenantCode = Utils.ParseInt(CurrentTenantCode),
                                        SessionID = CurrentSessionID,
                                        ClassID = Guid.Parse(model.SelectedClass),
                                        SectionID = Guid.Parse(model.SelectedSection),
                                        ExamID = Guid.Parse(model.SelectedExam),
                                        SubjectID = entry.SubjectId,
                                        ExamDate = entry.Date.Value,
                                        ExamTime = entry.Time,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedBy = CurrentTenantUserID,
                                        CreatedDate = DateTime.Now
                                    };

                                    connection.Execute(insertQuery, parameters, transaction);
                                }
                            }

                            transaction.Commit();
                            TempData["Success"] = $"Exam schedule saved successfully! {entriesToSave.Count} subject(s) scheduled.";
                        }
                        catch (SqlException sqlEx)
                        {
                            transaction.Rollback();

                            // Check for unique constraint violation
                            if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                            {
                                ModelState.AddModelError("", "A schedule already exists for one or more subjects on the selected dates.");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Database error: " + sqlEx.Message);
                            }

                            return PrepareViewModelAndReturn(model);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving: " + ex.Message);
                return PrepareViewModelAndReturn(model);
            }
        }

        private ActionResult PrepareViewModelAndReturn(ExamScheduleViewModel model)
        {
            // Regenerate dropdown lists
            model.ClassList = ConvertToSelectListDefault(new DropdownController().GetClasses());
            model.SectionList = ConvertToSelectListDefault(new DropdownController().GetSections());
            model.ExamList = ConvertToSelectListDefault(new DropdownController().GetExams());

            return View("Index", model);
        }

       
    }
}