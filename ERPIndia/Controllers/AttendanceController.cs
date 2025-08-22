// Controllers/AttendanceController.cs
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using ERPIndia.Models.Attendance;
using Newtonsoft.Json;

namespace ERPIndia.Controllers
{
    public class AttendanceController : BaseController
    {
        public string ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private readonly DropdownController _dropdownController;

        public AttendanceController()
        {
            _dropdownController = new DropdownController();
        }

        // GET: Attendance Entry
        public ActionResult Entry()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            var model = new AttendanceViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                SelectedDate = DateTime.Today
            };

            return View(model);
        }

        // Get students for attendance entry
        [HttpPost]
        public JsonResult GetStudentsForAttendance(string classId, string sectionId, DateTime attendanceDate)
        {
            try
            {
                var students = GetStudentsWithAttendanceFromDB(classId, sectionId, attendanceDate);
                var config = GetAttendanceConfigFromDB(classId, sectionId);
                var isHoliday = CheckIfHoliday(attendanceDate);

                return Json(new
                {
                    success = true,
                    students = students,
                    config = config,
                    isHoliday = isHoliday,
                    attendanceDate = attendanceDate.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading data: " + ex.Message });
            }
        }

        // Save attendance
        [HttpPost]
        public JsonResult SaveAttendance()
        {
            try
            {
                var requestJson = Request.InputStream;
                requestJson.Position = 0;
                using (var reader = new System.IO.StreamReader(requestJson))
                {
                    var json = reader.ReadToEnd();
                    var request = JsonConvert.DeserializeObject<SaveAttendanceRequest>(json);

                    if (request == null || request.Attendance == null || !request.Attendance.Any())
                    {
                        return Json(new { success = false, message = "No attendance data to save" });
                    }

                    bool result = SaveAttendanceToDB(request);

                    if (result)
                    {
                        return Json(new
                        {
                            success = true,
                            message = string.Format("Successfully saved attendance for {0} students", request.Attendance.Count)
                        });
                    }

                    return Json(new { success = false, message = "Failed to save attendance" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Get attendance status options
        [HttpGet]
        public JsonResult GetAttendanceStatusOptions()
        {
            var options = new List<AttendanceStatusOption>
            {
                new AttendanceStatusOption { Value = "Present", Text = "Present", ColorClass = "status-present" },
                new AttendanceStatusOption { Value = "Absent", Text = "Absent", ColorClass = "status-absent" },
                new AttendanceStatusOption { Value = "Late", Text = "Late", ColorClass = "status-late" },
                new AttendanceStatusOption { Value = "Half Day", Text = "Half Day", ColorClass = "status-halfday" },
                new AttendanceStatusOption { Value = "Holiday", Text = "Holiday", ColorClass = "status-holiday" }
            };

            return Json(options, JsonRequestBehavior.AllowGet);
        }

        // Monthly Report
        public ActionResult MonthlyReport()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            var model = new AttendanceViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult)
            };

            return View(model);
        }

        // Get monthly attendance report data
        [HttpPost]
        public JsonResult GetMonthlyAttendanceReport(string classId, string sectionId, int month, int year)
        {
            try
            {
                var reportData = GetMonthlyAttendanceFromDB(classId, sectionId, month, year);

                return Json(new
                {
                    success = true,
                    data = reportData
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error generating report: " + ex.Message });
            }
        }

        // Private helper methods
        private List<StudentAttendanceModel> GetStudentsWithAttendanceFromDB(string classId, string sectionId, DateTime attendanceDate)
        {
            var students = new List<StudentAttendanceModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        s.StudentID,
                        s.AdmsnNo AS AdmissionNo,
                        s.RollNo AS RollNumber,
                        s.FirstName AS StudentName,
                        s.FatherName,
                        s.Mobile AS MobileNumber,
                        c.ClassName,
                        sec.SectionName,
                        sa.Status,
                        sa.TimeIn,
                        sa.TimeOut,
                        sa.Source,
                        sa.Note
                    FROM StudentInfoBasic s
                    INNER JOIN AcademicClassMaster c ON s.ClassID = c.ClassID
                    INNER JOIN AcademicSectionMaster sec ON s.SectionID = sec.SectionID
                    LEFT JOIN StudentAttendance sa ON s.StudentID = sa.StudentID 
                        AND sa.AttendanceDate = @AttendanceDate
                        AND sa.IsDeleted = 0
                    WHERE s.ClassID = @ClassID 
                        AND s.SectionID = @SectionID
                        AND s.IsActive = 1
                        AND s.IsDeleted = 0
                        AND s.TenantID = @TenantID
                        AND s.SessionID = @SessionID
                    ORDER BY CAST(s.RollNo AS INT), s.FirstName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    cmd.Parameters.AddWithValue("@SectionID", sectionId);
                    cmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate.Date);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new StudentAttendanceModel
                            {
                                StudentID = reader["StudentID"].ToString(),
                                AdmissionNo = reader["AdmissionNo"].ToString(),
                                RollNumber = reader["RollNumber"].ToString(),
                                StudentName = reader["StudentName"].ToString().Trim(),
                                FatherName = reader["FatherName"].ToString(),
                                MobileNumber = reader["MobileNumber"]?.ToString() ?? "",
                                ClassName = reader["ClassName"].ToString(),
                                SectionName = reader["SectionName"].ToString(),
                                Status = reader["Status"]?.ToString() ?? "Present", // Default to Present
                                TimeIn = reader["TimeIn"]?.ToString() ?? "08:30 AM",
                                TimeOut = reader["TimeOut"]?.ToString() ?? "02:45 PM",
                                Source = reader["Source"]?.ToString() ?? "Manual",
                                Note = reader["Note"]?.ToString() ?? "",
                                AttendanceDate = attendanceDate
                            });
                        }
                    }
                }
            }

            return students;
        }

        private dynamic GetAttendanceConfigFromDB(string classId, string sectionId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT TOP 1
                        DefaultTimeIn,
                        DefaultTimeOut,
                        LateMarkAfter,
                        HalfDayBefore,
                        MinAttendancePercentage
                    FROM AttendanceConfiguration
                    WHERE (ClassID = @ClassID OR ClassID IS NULL)
                        AND (SectionID = @SectionID OR SectionID IS NULL)
                        AND IsActive = 1
                        AND IsDeleted = 0
                        AND TenantID = @TenantID
                        AND SessionID = @SessionID
                    ORDER BY ClassID DESC, SectionID DESC"; // Prioritize specific configs

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    cmd.Parameters.AddWithValue("@SectionID", sectionId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new
                            {
                                DefaultTimeIn = reader["DefaultTimeIn"]?.ToString() ?? "08:30:00",
                                DefaultTimeOut = reader["DefaultTimeOut"]?.ToString() ?? "14:45:00",
                                LateMarkAfter = reader["LateMarkAfter"]?.ToString() ?? "08:45:00",
                                HalfDayBefore = reader["HalfDayBefore"]?.ToString() ?? "12:00:00",
                                MinAttendancePercentage = Convert.ToDecimal(reader["MinAttendancePercentage"] ?? 75)
                            };
                        }
                    }
                }
            }

            // Return default config if none found
            return new
            {
                DefaultTimeIn = "08:30:00",
                DefaultTimeOut = "14:45:00",
                LateMarkAfter = "08:45:00",
                HalfDayBefore = "12:00:00",
                MinAttendancePercentage = 75.0m
            };
        }

        private bool CheckIfHoliday(DateTime date)
        {
            // Check if it's Sunday
            if (date.DayOfWeek == DayOfWeek.Sunday)
                return true;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string query = @"
                    SELECT COUNT(*) 
                    FROM HolidayCalendar
                    WHERE HolidayDate = @Date
                        AND IsDeleted = 0
                        AND TenantID = @TenantID
                        AND SessionID = @SessionID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date.Date);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private bool SaveAttendanceToDB(SaveAttendanceRequest request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var attendance in request.Attendance)
                            {
                                // Check if attendance already exists
                                string checkQuery = @"
                                    SELECT AttendanceID FROM StudentAttendance 
                                    WHERE StudentID = @StudentID 
                                        AND AttendanceDate = @AttendanceDate
                                        AND TenantID = @TenantID
                                        AND SessionID = @SessionID
                                        AND IsDeleted = 0";

                                string existingAttendanceId = null;
                                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                                {
                                    checkCmd.Parameters.AddWithValue("@StudentID", attendance.StudentID);
                                    checkCmd.Parameters.AddWithValue("@AttendanceDate", request.AttendanceDate.Date);
                                    checkCmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                                    checkCmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                                    var result = checkCmd.ExecuteScalar();
                                    existingAttendanceId = result?.ToString();
                                }

                                if (!string.IsNullOrEmpty(existingAttendanceId))
                                {
                                    // Update existing attendance
                                    string updateQuery = @"
                                        UPDATE StudentAttendance 
                                        SET Status = @Status,
                                            TimeIn = @TimeIn,
                                            TimeOut = @TimeOut,
                                            Note = @Note,
                                            ModifiedBy = @ModifiedBy,
                                            ModifiedDate = GETDATE()
                                        WHERE AttendanceID = @AttendanceID";

                                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction))
                                    {
                                        updateCmd.Parameters.AddWithValue("@AttendanceID", existingAttendanceId);
                                        updateCmd.Parameters.AddWithValue("@Status", attendance.Status);
                                        updateCmd.Parameters.AddWithValue("@TimeIn", ParseTime(attendance.TimeIn));
                                        updateCmd.Parameters.AddWithValue("@TimeOut", ParseTime(attendance.TimeOut));
                                        updateCmd.Parameters.AddWithValue("@Note", attendance.Note ?? (object)DBNull.Value);
                                        updateCmd.Parameters.AddWithValue("@ModifiedBy", CurrentTenantUserID);

                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    // Insert new attendance
                                    string insertQuery = @"
                                        INSERT INTO StudentAttendance 
                                        (AttendanceID, StudentID, ClassID, SectionID, AttendanceDate, 
                                         Status, TimeIn, TimeOut, Source, Note, 
                                         SessionYear, SessionID, TenantID, TenantCode, CreatedBy, CreatedDate, IsDeleted)
                                        VALUES 
                                        (@AttendanceID, @StudentID, @ClassID, @SectionID, @AttendanceDate,
                                         @Status, @TimeIn, @TimeOut, @Source, @Note,
                                         @SessionYear, @SessionID, @TenantID, @TenantCode, @CreatedBy, GETDATE(), 0)";

                                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@AttendanceID", Guid.NewGuid());
                                        insertCmd.Parameters.AddWithValue("@StudentID", attendance.StudentID);
                                        insertCmd.Parameters.AddWithValue("@ClassID", request.ClassId);
                                        insertCmd.Parameters.AddWithValue("@SectionID", request.SectionId);
                                        insertCmd.Parameters.AddWithValue("@AttendanceDate", request.AttendanceDate.Date);
                                        insertCmd.Parameters.AddWithValue("@Status", attendance.Status);
                                        insertCmd.Parameters.AddWithValue("@TimeIn", ParseTime(attendance.TimeIn));
                                        insertCmd.Parameters.AddWithValue("@TimeOut", ParseTime(attendance.TimeOut));
                                        insertCmd.Parameters.AddWithValue("@Source", "Manual");
                                        insertCmd.Parameters.AddWithValue("@Note", attendance.Note ?? (object)DBNull.Value);
                                        insertCmd.Parameters.AddWithValue("@SessionYear", CurrentSessionYear);
                                        insertCmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                                        insertCmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                                        insertCmd.Parameters.AddWithValue("@TenantCode", CurrentTenantCode);
                                        insertCmd.Parameters.AddWithValue("@CreatedBy", CurrentTenantUserID);

                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Save attendance error: " + ex.Message);
                return false;
            }
        }

        private object ParseTime(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return DBNull.Value;

            if (TimeSpan.TryParse(timeString, out TimeSpan time))
                return time;

            // Try parsing AM/PM format
            if (DateTime.TryParse("2000-01-01 " + timeString, out DateTime dateTime))
                return dateTime.TimeOfDay;

            return DBNull.Value;
        }

        private AttendanceReportModel GetMonthlyAttendanceFromDB(string classId, string sectionId, int month, int year)
        {
            var report = new AttendanceReportModel
            {
                Students = new List<StudentMonthlyAttendance>(),
                DatesInMonth = new List<DateTime>()
            };

            // Get all dates in the month
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
            {
                report.DatesInMonth.Add(date);
            }

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                // Get students
                string studentQuery = @"
                    SELECT 
                        s.StudentID,
                        s.FirstName AS StudentName,
                        s.FatherName,
                        s.RollNo
                    FROM StudentInfoBasic s
                    WHERE s.ClassID = @ClassID 
                        AND s.SectionID = @SectionID
                        AND s.IsActive = 1
                        AND s.IsDeleted = 0
                        AND s.TenantID = @TenantID
                        AND s.SessionID = @SessionID
                    ORDER BY CAST(s.RollNo AS INT), s.FirstName";

                var students = new List<(string StudentID, string StudentName, string FatherName)>();

                using (SqlCommand cmd = new SqlCommand(studentQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    cmd.Parameters.AddWithValue("@SectionID", sectionId);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add((
                                reader["StudentID"].ToString(),
                                reader["StudentName"].ToString(),
                                reader["FatherName"].ToString()
                            ));
                        }
                    }
                }

                // Get attendance for each student
                int serialNo = 1;
                foreach (var student in students)
                {
                    var studentAttendance = new StudentMonthlyAttendance
                    {
                        SerialNo = serialNo++,
                        StudentName = student.StudentName,
                        FatherName = student.FatherName,
                        DailyAttendance = new Dictionary<int, string>(),
                        TotalPresent = 0,
                        TotalAbsent = 0,
                        TotalLate = 0,
                        TotalHalfDay = 0
                    };

                    // Get attendance records for this student
                    string attendanceQuery = @"
                        SELECT 
                            DAY(AttendanceDate) as DayOfMonth,
                            Status
                        FROM StudentAttendance
                        WHERE StudentID = @StudentID
                            AND MONTH(AttendanceDate) = @Month
                            AND YEAR(AttendanceDate) = @Year
                            AND IsDeleted = 0
                            AND TenantID = @TenantID
                            AND SessionID = @SessionID";

                    using (SqlCommand cmd = new SqlCommand(attendanceQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentID", student.StudentID);
                        cmd.Parameters.AddWithValue("@Month", month);
                        cmd.Parameters.AddWithValue("@Year", year);
                        cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int day = Convert.ToInt32(reader["DayOfMonth"]);
                                string status = reader["Status"].ToString();

                                studentAttendance.DailyAttendance[day] = GetStatusCode(status);

                                // Update totals
                                switch (status)
                                {
                                    case "Present":
                                        studentAttendance.TotalPresent++;
                                        break;
                                    case "Absent":
                                        studentAttendance.TotalAbsent++;
                                        break;
                                    case "Late":
                                        studentAttendance.TotalLate++;
                                        break;
                                    case "Half Day":
                                        studentAttendance.TotalHalfDay++;
                                        break;
                                }
                            }
                        }
                    }

                    report.Students.Add(studentAttendance);
                }
            }

            return report;
        }

        private string GetStatusCode(string status)
        {
            switch (status)
            {
                case "Present": return "P";
                case "Absent": return "A";
                case "Late": return "L";
                case "Half Day": return "HD";
                case "Holiday": return "H";
                default: return "";
            }
        }

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
            catch
            {
                return new List<SelectListItem>();
            }
        }
    }
}