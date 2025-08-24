// Controllers/AttendanceController.cs
using Dapper;
using ERPIndia.Models.Attendance;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

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

        // Data Transfer Object for Student Data
        public class StudentData
        {
            public string StudentID { get; set; }
            public string AdmissionNo { get; set; }
            public string RollNumber { get; set; }
            public string StudentName { get; set; }
            public string FatherName { get; set; }
            public string MobileNumber { get; set; }
            public string ClassName { get; set; }
            public string SectionName { get; set; }
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

        public ActionResult DailyAttendance()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();
            var model = new DailyAttendanceViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                StatusOptions = GetAttendanceStatusList(),
                SelectedDate = DateTime.Today
            };

            return View(model);
        }

        public ActionResult YearlyReport()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            var model = new YearlyAttendanceViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                SelectedYear = DateTime.Now.Year,
                Years = GetYearsList()
            };

            return View(model);
        }

        // Get yearly attendance report data
        [HttpPost]
        public JsonResult GetYearlyAttendanceReport(string classId, string sectionId, int year)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    var reportData = new YearlyReportData
                    {
                        Students = new List<StudentYearlyAttendance>(),
                        MonthlyStatistics = new List<MonthlyStats>(),
                        Year = year,
                        SessionYear = CurrentSessionYear.ToString()
                    };

                    // Get all students - Using strongly typed class instead of dynamic
                    string studentQuery = @"
                SELECT 
                    s.StudentID,
                    s.AdmsnNo AS AdmissionNo,
                    s.RollNo AS RollNumber,
                    RTRIM(LTRIM(s.FirstName + ' ' + ISNULL(s.LastName, ''))) AS StudentName,
                    RTRIM(LTRIM(ISNULL(s.FatherName, ''))) AS FatherName,
                    ISNULL(s.Mobile, '') AS MobileNumber,
                    c.ClassName,
                    sec.SectionName
                FROM StudentInfoBasic s
                INNER JOIN AcademicClassMaster c ON s.ClassID = c.ClassID
                INNER JOIN AcademicSectionMaster sec ON s.SectionID = sec.SectionID
                WHERE s.ClassID = @ClassID 
                    AND s.SectionID = @SectionID
                    AND s.IsActive = 1
                    AND s.IsDeleted = 0
                    AND s.TenantID = @TenantID
                    AND s.SessionID = @SessionID
                ORDER BY CAST(s.RollNo AS INT), s.FirstName";

                    var students = new List<StudentData>();
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
                                students.Add(new StudentData
                                {
                                    StudentID = reader["StudentID"]?.ToString() ?? "",
                                    AdmissionNo = reader["AdmissionNo"]?.ToString() ?? "",
                                    RollNumber = reader["RollNumber"]?.ToString() ?? "",
                                    StudentName = reader["StudentName"]?.ToString()?.Trim() ?? "",
                                    FatherName = reader["FatherName"]?.ToString()?.Trim() ?? "",
                                    MobileNumber = reader["MobileNumber"]?.ToString() ?? "",
                                    ClassName = reader["ClassName"]?.ToString() ?? "",
                                    SectionName = reader["SectionName"]?.ToString() ?? ""
                                });
                            }
                        }
                    }

                    // Process each student
                    int serialNo = 1;
                    foreach (var student in students)
                    {
                        var yearlyAttendance = new StudentYearlyAttendance
                        {
                            SerialNo = serialNo++,
                            StudentID = student.StudentID,
                            AdmissionNo = student.AdmissionNo,
                            RollNumber = student.RollNumber,
                            StudentName = student.StudentName,
                            FatherName = student.FatherName,
                            MobileNumber = student.MobileNumber,
                            ClassName = student.ClassName,
                            SectionName = student.SectionName,
                            MonthlyData = new List<MonthlyAttendanceData>()
                        };

                        // Get attendance for each month
                        for (int month = 1; month <= 12; month++)
                        {
                            var monthData = GetMonthlyAttendanceData(conn, student.StudentID, month, year);
                            yearlyAttendance.MonthlyData.Add(monthData);
                        }

                        // Calculate yearly totals
                        yearlyAttendance.TotalWorkingDays = yearlyAttendance.MonthlyData.Sum(m => m.WorkingDays);
                        yearlyAttendance.TotalPresent = yearlyAttendance.MonthlyData.Sum(m => m.Present);
                        yearlyAttendance.TotalAbsent = yearlyAttendance.MonthlyData.Sum(m => m.Absent);
                        yearlyAttendance.TotalLate = yearlyAttendance.MonthlyData.Sum(m => m.Late);
                        yearlyAttendance.TotalHalfDay = yearlyAttendance.MonthlyData.Sum(m => m.HalfDay);
                        yearlyAttendance.TotalHolidays = yearlyAttendance.MonthlyData.Sum(m => m.Holidays);

                        // Calculate percentage and grade
                        if (yearlyAttendance.TotalWorkingDays > 0)
                        {
                            yearlyAttendance.AttendancePercentage =
                                Math.Round((decimal)(yearlyAttendance.TotalPresent + yearlyAttendance.TotalLate + yearlyAttendance.TotalHalfDay * 0.5m)
                                / yearlyAttendance.TotalWorkingDays * 100, 2);

                            // Calculate grade and color
                            yearlyAttendance.AttendanceGrade = GetAttendanceGrade(yearlyAttendance.AttendancePercentage);
                            yearlyAttendance.AttendanceColor = GetAttendanceColor(yearlyAttendance.AttendancePercentage);
                        }
                        else
                        {
                            yearlyAttendance.AttendancePercentage = 0;
                            yearlyAttendance.AttendanceGrade = "N/A";
                            yearlyAttendance.AttendanceColor = "secondary";
                        }

                        reportData.Students.Add(yearlyAttendance);
                    }

                    // Calculate monthly statistics for the class
                    for (int month = 1; month <= 12; month++)
                    {
                        var monthStats = CalculateMonthlyStatistics(conn, classId, sectionId, month, year);
                        reportData.MonthlyStatistics.Add(monthStats);
                    }

                    // Set report metadata
                    if (students.Any())
                    {
                        reportData.ClassName = students.First().ClassName ?? "";
                        reportData.SectionName = students.First().SectionName ?? "";
                    }
                    else
                    {
                        // If no students, get class and section names directly
                        string classNameQuery = "SELECT ClassName FROM AcademicClassMaster WHERE ClassID = @ClassID";
                        string sectionNameQuery = "SELECT SectionName FROM AcademicSectionMaster WHERE SectionID = @SectionID";

                        using (SqlCommand cmd = new SqlCommand(classNameQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@ClassID", classId);
                            reportData.ClassName = cmd.ExecuteScalar()?.ToString() ?? "";
                        }

                        using (SqlCommand cmd = new SqlCommand(sectionNameQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@SectionID", sectionId);
                            reportData.SectionName = cmd.ExecuteScalar()?.ToString() ?? "";
                        }
                    }

                    reportData.GeneratedDate = DateTime.Now;
                    reportData.TotalStudents = students.Count;

                    return Json(new
                    {
                        success = true,
                        data = reportData,
                        message = $"Report generated for {students.Count} students"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Grade calculation methods
        private string GetAttendanceGrade(decimal percentage)
        {
            if (percentage >= 95) return "A+";
            if (percentage >= 90) return "A";
            if (percentage >= 85) return "B+";
            if (percentage >= 80) return "B";
            if (percentage >= 75) return "C+";
            if (percentage >= 70) return "C";
            if (percentage >= 60) return "D";
            return "F";
        }

        private string GetAttendanceColor(decimal percentage)
        {
            if (percentage >= 90) return "success";
            if (percentage >= 75) return "primary";
            if (percentage >= 60) return "warning";
            return "danger";
        }

        // Helper method to get monthly attendance data for a student
        private MonthlyAttendanceData GetMonthlyAttendanceData(SqlConnection conn, string studentId, int month, int year)
        {
            var monthData = new MonthlyAttendanceData
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMM"),
                Present = 0,
                Absent = 0,
                Late = 0,
                HalfDay = 0,
                Holidays = 0,
                WorkingDays = 0
            };

            // Get working days for the month
            string workingDaysQuery = @"
        WITH DateRange AS (
            SELECT DATEFROMPARTS(@Year, @Month, 1) AS Date
            UNION ALL
            SELECT DATEADD(DAY, 1, Date)
            FROM DateRange
            WHERE Date < EOMONTH(DATEFROMPARTS(@Year, @Month, 1))
        )
        SELECT COUNT(*) AS WorkingDays
        FROM DateRange
        WHERE DATEPART(WEEKDAY, Date) NOT IN (1) -- Exclude Sunday
            AND Date NOT IN (
                SELECT HolidayDate 
                FROM HolidayCalendar 
                WHERE YEAR(HolidayDate) = @Year 
                    AND MONTH(HolidayDate) = @Month
                    AND IsDeleted = 0
                    AND TenantID = @TenantID
            )
        OPTION (MAXRECURSION 31)";

            using (SqlCommand cmd = new SqlCommand(workingDaysQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);

                var result = cmd.ExecuteScalar();
                monthData.WorkingDays = result != null ? Convert.ToInt32(result) : 0;
            }

            // Get attendance summary for the student
            string attendanceQuery = @"
        SELECT 
            Status,
            COUNT(*) as Count
        FROM StudentAttendance
        WHERE StudentID = @StudentID
            AND YEAR(AttendanceDate) = @Year
            AND MONTH(AttendanceDate) = @Month
            AND IsDeleted = 0
            AND TenantID = @TenantID
            AND SessionID = @SessionID
        GROUP BY Status";

            using (SqlCommand cmd = new SqlCommand(attendanceQuery, conn))
            {
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string status = reader["Status"].ToString();
                        int count = Convert.ToInt32(reader["Count"]);

                        switch (status)
                        {
                            case "Present":
                                monthData.Present = count;
                                break;
                            case "Absent":
                                monthData.Absent = count;
                                break;
                            case "Late":
                                monthData.Late = count;
                                break;
                            case "Half Day":
                                monthData.HalfDay = count;
                                break;
                            case "Holiday":
                            case "Holy Day":
                                monthData.Holidays = count;
                                break;
                        }
                    }
                }
            }

            // Calculate attendance percentage for the month
            if (monthData.WorkingDays > 0)
            {
                monthData.AttendancePercentage =
                    Math.Round((decimal)(monthData.Present + monthData.Late + monthData.HalfDay * 0.5m)
                    / monthData.WorkingDays * 100, 2);
            }

            return monthData;
        }

        // Calculate monthly statistics for the entire class
        private MonthlyStats CalculateMonthlyStatistics(SqlConnection conn, string classId, string sectionId, int month, int year)
        {
            var stats = new MonthlyStats
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                TotalStudents = 0,
                AverageAttendance = 0,
                BestAttendance = 0,
                WorstAttendance = 100
            };

            string query = @"
        SELECT 
            COUNT(DISTINCT s.StudentID) as TotalStudents,
            AVG(CASE 
                WHEN sa.Status IN ('Present', 'Late') THEN 100.0
                WHEN sa.Status = 'Half Day' THEN 50.0
                ELSE 0
            END) as AverageAttendance
        FROM StudentInfoBasic s
        LEFT JOIN StudentAttendance sa ON s.StudentID = sa.StudentID
            AND YEAR(sa.AttendanceDate) = @Year
            AND MONTH(sa.AttendanceDate) = @Month
            AND sa.IsDeleted = 0
        WHERE s.ClassID = @ClassID
            AND s.SectionID = @SectionID
            AND s.IsActive = 1
            AND s.IsDeleted = 0
            AND s.TenantID = @TenantID
            AND s.SessionID = @SessionID";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ClassID", classId);
                cmd.Parameters.AddWithValue("@SectionID", sectionId);
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stats.TotalStudents = reader["TotalStudents"] != DBNull.Value ?
                            Convert.ToInt32(reader["TotalStudents"]) : 0;
                        stats.AverageAttendance = reader["AverageAttendance"] != DBNull.Value ?
                            Convert.ToDecimal(reader["AverageAttendance"]) : 0;
                    }
                }
            }

            return stats;
        }
        public ActionResult MonthlyReport()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            var model = new MonthlyAttendanceViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                Months = GetMonthsList(),
                Years = GetYearsList(),
                SelectedMonth = DateTime.Now.Month,
                SelectedYear = DateTime.Now.Year
            };

            return View(model);
        }

        // Generate list of years for dropdown
        private List<SelectListItem> GetYearsList()
        {
            var years = new List<SelectListItem>();
            int currentYear = DateTime.Now.Year;

            for (int year = currentYear; year >= currentYear - 1; year--)
            {
                years.Add(new SelectListItem
                {
                    Value = year.ToString(),
                    Text = year.ToString(),
                    Selected = year == currentYear
                });
            }

            return years;
        }
        private List<SelectListItem> GetMonthsList()
        {
            return new List<SelectListItem>
    {
        new SelectListItem { Value = "4", Text = "April" },
        new SelectListItem { Value = "5", Text = "May" },
        new SelectListItem { Value = "6", Text = "June" },
        new SelectListItem { Value = "7", Text = "July" },
        new SelectListItem { Value = "8", Text = "August" },
        new SelectListItem { Value = "9", Text = "September" },
        new SelectListItem { Value = "10", Text = "October" },
        new SelectListItem { Value = "11", Text = "November" },
        new SelectListItem { Value = "12", Text = "December" },
        new SelectListItem { Value = "1", Text = "January" },
        new SelectListItem { Value = "2", Text = "February" },
        new SelectListItem { Value = "3", Text = "March" }
    };
        }
        private List<SelectListItem> GetAttendanceStatusList()
        {
            return new List<SelectListItem>  {
                   new SelectListItem { Value = "ALL", Text = "ALL" },
                   new SelectListItem { Value = "Present", Text = "Present" },
                   new SelectListItem { Value = "Absent", Text = "Absent" },
                   new SelectListItem { Value = "Late", Text = "Late" },
                   new SelectListItem { Value = "Half Day", Text = "Half Day" },
                   new SelectListItem { Value = "Holy Day", Text = "Holy Day" }
                   };
        }

        [HttpPost]
        public JsonResult GetDailyAttendanceReport(string classId, string sectionId, string status, DateTime date)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string query = @"
SELECT 
    s.StudentID,
    s.AdmsnNo AS AdmissionNo,
    s.RollNo AS RollNumber,
    c.ClassName,
    sec.SectionName,
    s.FirstName AS StudentName,
    s.FatherName,
    s.Mobile AS MobileNumber,
    sa.Status,
    CONVERT(VARCHAR(8), CAST(sa.TimeIn AS TIME), 108) AS TimeIn,
    CONVERT(VARCHAR(8), CAST(sa.TimeOut AS TIME), 108) AS TimeOut,
    ISNULL(sa.Note, '') AS Note,
    'Yes' AS AttendanceMarked
FROM StudentAttendance sa
INNER JOIN StudentInfoBasic s ON s.StudentID = sa.StudentID
INNER JOIN AcademicClassMaster c ON s.ClassID = c.ClassID
INNER JOIN AcademicSectionMaster sec ON s.SectionID = sec.SectionID
WHERE sa.AttendanceDate = @AttendanceDate
    AND sa.IsDeleted = 0
    AND s.IsActive = 1
    AND s.IsDeleted = 0
    AND s.TenantID = @TenantID
    AND s.SessionID = @SessionID";

                    // Add filters
                    if (!string.IsNullOrEmpty(classId) && classId != "All")
                    {
                        query += " AND s.ClassID = @ClassID";
                    }

                    if (!string.IsNullOrEmpty(sectionId) && sectionId != "All")
                    {
                        query += " AND s.SectionID = @SectionID";
                    }

                    if (!string.IsNullOrEmpty(status) && status != "ALL")
                    {
                        if (status == "Not Marked")
                        {
                            query += " AND 1=0"; // This will return no results
                        }
                        else
                        {
                            query += " AND sa.Status = @Status";
                        }
                    }

                    query += " ORDER BY CAST(s.RollNo AS INT), s.FirstName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AttendanceDate", date.Date);
                        cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                        if (!string.IsNullOrEmpty(classId) && classId != "All")
                            cmd.Parameters.AddWithValue("@ClassID", classId);

                        if (!string.IsNullOrEmpty(sectionId) && sectionId != "All")
                            cmd.Parameters.AddWithValue("@SectionID", sectionId);

                        if (!string.IsNullOrEmpty(status) && status != "ALL" && status != "Not Marked")
                            cmd.Parameters.AddWithValue("@Status", status);

                        var students = new List<object>();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                students.Add(new
                                {
                                    AdmissionNo = reader["AdmissionNo"].ToString(),
                                    RollNumber = reader["RollNumber"].ToString(),
                                    ClassName = reader["ClassName"].ToString(),
                                    SectionName = reader["SectionName"].ToString(),
                                    StudentName = reader["StudentName"].ToString(),
                                    FatherName = reader["FatherName"].ToString(),
                                    MobileNumber = reader["MobileNumber"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    StudentID = reader["StudentID"].ToString(),
                                    TimeIn = ConvertTimeFormat(reader["TimeIn"].ToString()),
                                    TimeOut = ConvertTimeFormat(reader["TimeOut"].ToString()),
                                    Note = reader["Note"].ToString(),
                                    AttendanceMarked = reader["AttendanceMarked"].ToString()
                                });
                            }
                        }

                        // Calculate summary statistics
                        var summary = new
                        {
                            TotalStudents = students.Count,
                            Present = students.Count(s => ((dynamic)s).Status == "Present"),
                            Absent = students.Count(s => ((dynamic)s).Status == "Absent"),
                            Late = students.Count(s => ((dynamic)s).Status == "Late"),
                            HalfDay = students.Count(s => ((dynamic)s).Status == "Half Day"),
                            Holiday = students.Count(s => ((dynamic)s).Status == "Holiday" || ((dynamic)s).Status == "Holy Day"),
                            NotMarked = 0
                        };

                        return Json(new
                        {
                            success = true,
                            data = students,
                            summary = summary,
                            reportDate = date.ToString("dd-MMM-yyyy"),
                            className = classId == "All" ? "All Classes" : students.FirstOrDefault() != null ? ((dynamic)students.First()).ClassName : "",
                            sectionName = sectionId == "All" ? "All Sections" : students.FirstOrDefault() != null ? ((dynamic)students.First()).SectionName : ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

        private string ConvertTimeFormat(string time)
        {
            if (string.IsNullOrEmpty(time)) return "8:15 AM";

            try
            {
                if (TimeSpan.TryParse(time, out TimeSpan timeSpan))
                {
                    DateTime dateTime = DateTime.Today.Add(timeSpan);
                    return dateTime.ToString("h:mm tt");
                }
            }
            catch { }

            return time;
        }

        // Monthly Report


        // Get monthly attendance report data
        [HttpPost]
        public JsonResult GetMonthlyAttendanceReport(string classId, string sectionId, int month, int year)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    var reportData = new MonthlyReportData
                    {
                        Students = new List<StudentMonthlyAttendance>(),
                        DatesInMonth = new List<DateInfo>(),
                        Month = month,
                        Year = year
                    };

                    // Get class and section names first
                    try
                    {
                        string classNameQuery = "SELECT ClassName FROM AcademicClassMaster WHERE ClassID = @ClassID";
                        using (SqlCommand cmd = new SqlCommand(classNameQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@ClassID", classId);
                            var result = cmd.ExecuteScalar();
                            reportData.ClassName = result?.ToString() ?? "Unknown Class";
                        }

                        string sectionNameQuery = "SELECT SectionName FROM AcademicSectionMaster WHERE SectionID = @SectionID";
                        using (SqlCommand cmd = new SqlCommand(sectionNameQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@SectionID", sectionId);
                            var result = cmd.ExecuteScalar();
                            reportData.SectionName = result?.ToString() ?? "Unknown Section";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log but continue
                        System.Diagnostics.Debug.WriteLine($"Error getting class/section names: {ex.Message}");
                    }

                    // Get month name
                    reportData.MonthName = new DateTime(year, month, 1).ToString("MMMM yyyy");

                    // Build dates list with weekday info
                    var firstDay = new DateTime(year, month, 1);
                    var lastDay = firstDay.AddMonths(1).AddDays(-1);
                    int workingDays = 0;

                    // Get holidays for the month first
                    var holidays = new HashSet<DateTime>();
                    try
                    {
                        string holidayQuery = @"
                    SELECT HolidayDate 
                    FROM HolidayCalendar 
                    WHERE YEAR(HolidayDate) = @Year 
                        AND MONTH(HolidayDate) = @Month
                        AND IsDeleted = 0
                        AND TenantID = @TenantID";

                        using (SqlCommand cmd = new SqlCommand(holidayQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Year", year);
                            cmd.Parameters.AddWithValue("@Month", month);
                            cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader["HolidayDate"] != DBNull.Value)
                                    {
                                        holidays.Add(Convert.ToDateTime(reader["HolidayDate"]).Date);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting holidays: {ex.Message}");
                    }

                    // Build date list
                    for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
                    {
                        bool isSunday = date.DayOfWeek == DayOfWeek.Sunday;
                        bool isHoliday = holidays.Contains(date.Date);

                        if (!isSunday && !isHoliday)
                            workingDays++;

                        reportData.DatesInMonth.Add(new DateInfo
                        {
                            Date = date,
                            Day = date.Day.ToString("00"),
                            WeekDay = date.ToString("ddd").Substring(0, Math.Min(2, date.ToString("ddd").Length)).ToUpper(),
                            IsSunday = isSunday,
                            IsHoliday = isHoliday
                        });
                    }

                    reportData.WorkingDays = workingDays;

                    // Get all students with proper error handling
                    var students = new List<StudentData>();
                    try
                    {
                        string studentQuery = @"
                    SELECT 
                        s.StudentID,
                        s.AdmsnNo AS AdmissionNo,
                        s.RollNo AS RollNumber,
                        RTRIM(LTRIM(ISNULL(s.FirstName, '') + ' ' + ISNULL(s.LastName, ''))) AS StudentName,
                        RTRIM(LTRIM(ISNULL(s.FatherName, ''))) AS FatherName
                    FROM StudentInfoBasic s
                    WHERE s.ClassID = @ClassID 
                        AND s.SectionID = @SectionID
                        AND s.IsActive = 1
                        AND s.IsDeleted = 0
                        AND s.TenantID = @TenantID
                        AND s.SessionID = @SessionID
                    ORDER BY 
                        s.FirstName";

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
                                    students.Add(new StudentData
                                    {
                                        StudentID = reader["StudentID"]?.ToString() ?? "",
                                        AdmissionNo = reader["AdmissionNo"]?.ToString() ?? "",
                                        RollNumber = reader["RollNumber"]?.ToString() ?? "",
                                        StudentName = reader["StudentName"]?.ToString()?.Trim() ?? "",
                                        FatherName = reader["FatherName"]?.ToString()?.Trim() ?? ""
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Error loading students: {ex.Message}",
                            details = ex.ToString()
                        });
                    }

                    reportData.TotalStudents = students.Count;

                    // Process each student
                    int serialNo = 1;
                    decimal totalAttendancePercentage = 0;
                    int validStudents = 0;

                    foreach (var student in students)
                    {
                        try
                        {
                            var monthlyAttendance = new StudentMonthlyAttendance
                            {
                                SerialNo = serialNo++,
                                StudentID = student.StudentID,
                                AdmissionNo = student.AdmissionNo,
                                RollNumber = student.RollNumber,
                                StudentName = student.StudentName,
                                FatherName = student.FatherName,
                                DailyAttendanceMonthly = new Dictionary<string, string>(),
                                TotalPresent = 0,
                                TotalAbsent = 0,
                                TotalLate = 0,
                                TotalHalfDay = 0
                            };

                            // Get attendance for this student for the month
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
                                        if (reader["DayOfMonth"] != DBNull.Value && reader["Status"] != DBNull.Value)
                                        {
                                            int day = Convert.ToInt32(reader["DayOfMonth"]);
                                            string status = reader["Status"].ToString();
                                            string statusCode = GetStatusCode(status);

                                            monthlyAttendance.DailyAttendanceMonthly[day.ToString("00")] = statusCode;

                                            // Update totals
                                            switch (status.ToLower())
                                            {
                                                case "present":
                                                    monthlyAttendance.TotalPresent++;
                                                    break;
                                                case "absent":
                                                    monthlyAttendance.TotalAbsent++;
                                                    break;
                                                case "late":
                                                    monthlyAttendance.TotalLate++;
                                                    break;
                                                case "half day":
                                                case "halfday":
                                                    monthlyAttendance.TotalHalfDay++;
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }

                            // Calculate attendance percentage
                            if (workingDays > 0)
                            {
                                decimal effectivePresent = monthlyAttendance.TotalPresent +
                                                          monthlyAttendance.TotalLate +
                                                          (monthlyAttendance.TotalHalfDay * 0.5m);

                                monthlyAttendance.AttendancePercentage = Math.Round((effectivePresent / workingDays) * 100, 2);
                                totalAttendancePercentage += monthlyAttendance.AttendancePercentage;
                                validStudents++;
                            }
                            else
                            {
                                monthlyAttendance.AttendancePercentage = 0;
                            }

                            reportData.Students.Add(monthlyAttendance);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing student {student.StudentName}: {ex.Message}");
                            // Continue with next student
                        }
                    }

                    // Calculate average attendance
                    reportData.AverageAttendance = validStudents > 0 ?
                        Math.Round(totalAttendancePercentage / validStudents, 2) : 0;
                    var jsonString = JsonConvert.SerializeObject(new
                    {
                        success = true,
                        data = reportData,
                        message = $"Report generated successfully for {students.Count} students"
                    });

                    return Json(new
                    {
                        success = true,
                        data = reportData,
                        message = $"Report generated successfully for {students.Count} students"
                    });
                }
            }
            catch (Exception ex)
            {
                // Return detailed error for debugging
                return Json(new
                {
                    success = false,
                    message = $"Error generating report: {ex.Message}",
                    details = ex.ToString(),
                    stackTrace = ex.StackTrace
                });
            }
        }

        // Updated GetStatusCode method to handle more cases
        private string GetStatusCode(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "";

            switch (status.ToLower().Trim())
            {
                case "present": return "P";
                case "absent": return "A";
                case "late": return "L";
                case "half day":
                case "halfday": return "HD";
                case "holiday":
                case "holy day": return "H";
                default: return "";
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
                                Status = reader["Status"]?.ToString() ?? "Present",
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

        public string GetSingleStringValue(string query, object parameters = null)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                return connection.QuerySingleOrDefault<string>(query, parameters);
            }
        }

        private dynamic GetAttendanceConfigFromDB(string classId, string sectionId)
        {
            string sqltime = string.Format("SELECT isnull(convert(VARCHAR(50),TimeIn+'|'+TimeOut),'') AS SchoolTime  FROM Tenants WHERE TenantID='{0}'", CurrentTenantID);
            string timedetails = GetSingleStringValue(sqltime);
            string[] timevalue = timedetails.Split('|');
            string tenantTimeIn = (timevalue != null && timevalue.Length > 0 && !string.IsNullOrWhiteSpace(timevalue[0]))
                           ? timevalue[0]
                           : null;

            string tenantTimeOut = (timevalue != null && timevalue.Length > 1 && !string.IsNullOrWhiteSpace(timevalue[1]))
                                    ? timevalue[1]
                                    : null;

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
                    ORDER BY ClassID DESC, SectionID DESC";

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
                                DefaultTimeIn = tenantTimeIn ?? (reader["DefaultTimeIn"]?.ToString() ?? "08:30:00"),
                                DefaultTimeOut = tenantTimeOut ?? (reader["DefaultTimeOut"]?.ToString() ?? "14:45:00"),
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
                DefaultTimeIn = tenantTimeIn ?? "08:30:00",
                DefaultTimeOut = tenantTimeOut ?? "14:45:00",
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