// Controllers/AttendanceController.cs - COMPLETE FIXED VERSION
using Dapper;
using ERPIndia.Models.Attendance;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
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
        public class AttendanceValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Messages { get; set; }
        }
        private AttendanceValidationResult ValidateAttendanceDate(DateTime attendanceDate)
        {
            var result = new AttendanceValidationResult { IsValid = true, Messages = new List<string>() };

            // Check if date is in future
            if (attendanceDate.Date > DateTime.Today)
            {
                result.IsValid = false;
                result.Messages.Add("Cannot mark attendance for future dates");
            }

            // Check if Sunday
            if (attendanceDate.DayOfWeek == DayOfWeek.Sunday)
            {
                result.IsValid = false;
                result.Messages.Add("Cannot mark attendance on Sunday");
            }

            // Check if holiday
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                string query = @"
            SELECT HolidayName 
            FROM HolidayCalendar 
            WHERE HolidayDate = @Date 
                AND IsDeleted = 0 
                AND IsActive = 1
                AND TenantID = @TenantID
                AND SessionID = @SessionID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", attendanceDate.Date);
                    cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                    cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                    var holidayName = cmd.ExecuteScalar()?.ToString();
                    if (!string.IsNullOrEmpty(holidayName))
                    {
                        result.IsValid = false;
                        result.Messages.Add($"Cannot mark attendance on holiday: {holidayName}");
                    }
                }
            }

            return result;
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

        #region Yearly Report Methods
        public ActionResult YearlyReport()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            var model = new YearlyAttendanceViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
            };

            return View(model);
        }
        [HttpGet]
        public JsonResult GetSessionMonths()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Get current session details
                    DateTime sessionStartDate = DateTime.MinValue;
                    DateTime sessionEndDate = DateTime.MinValue;
                    string sessionName = "";

                    string sessionQuery = @"
                SELECT StartDate, EndDate, SessionName 
                FROM AcademicSessionMaster 
                WHERE SessionID = @SessionID";

                    using (SqlCommand cmd = new SqlCommand(sessionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sessionStartDate = Convert.ToDateTime(reader["StartDate"]);
                                sessionEndDate = Convert.ToDateTime(reader["EndDate"]);
                                sessionName = reader["SessionName"]?.ToString() ?? "";
                            }
                        }
                    }

                    // Build months list based on session dates
                    var months = new List<object>();
                    var currentDate = new DateTime(sessionStartDate.Year, sessionStartDate.Month, 1);
                    var endDate = new DateTime(sessionEndDate.Year, sessionEndDate.Month, 1);

                    while (currentDate <= endDate)
                    {
                        months.Add(new
                        {
                            value = $"{currentDate.Month}-{currentDate.Year}",
                            text = $"{currentDate.ToString("MMMM")} {currentDate.Year}"
                        });
                        currentDate = currentDate.AddMonths(1);
                    }

                    // Format session year display
                    string sessionYear = $"{sessionStartDate.Year}-{(sessionEndDate.Year % 100).ToString("00")}";

                    return Json(new
                    {
                        success = true,
                        months = months,
                        sessionYear = sessionYear,
                        sessionName = sessionName
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Get yearly attendance report data - FIXED VERSION
        [HttpPost]
        public JsonResult GetYearlyAttendanceReport(string classId, string sectionId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Get current session details
                    DateTime sessionStartDate = DateTime.MinValue;
                    DateTime sessionEndDate = DateTime.MinValue;
                    string sessionName = "";

                    string sessionQuery = @"
                SELECT StartDate, EndDate, SessionName 
                FROM AcademicSessionMaster 
                WHERE SessionID = @SessionID";

                    using (SqlCommand cmd = new SqlCommand(sessionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sessionStartDate = Convert.ToDateTime(reader["StartDate"]);
                                sessionEndDate = Convert.ToDateTime(reader["EndDate"]);
                                sessionName = reader["SessionName"]?.ToString() ?? "";
                            }
                        }
                    }

                    var reportData = new YearlyReportData
                    {
                        Students = new List<StudentYearlyAttendance>(),
                        MonthlyStatistics = new List<MonthlyStats>(),
                        SessionYear = CurrentSessionYear.ToString(),
                        SessionName = sessionName,
                        SessionStartDate = sessionStartDate,
                        SessionEndDate = sessionEndDate
                    };

                    // Get all students
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

                    // Set class and section names
                    if (students.Any())
                    {
                        reportData.ClassName = students.First().ClassName;
                        reportData.SectionName = students.First().SectionName;
                    }

                    // Define academic year months (April = 4 to March = 3)
                    int[] academicMonths = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };
                    string[] monthNames = { "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar" };

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

                        // Initialize all 12 months in academic year order (April to March)
                        for (int i = 0; i < 12; i++)
                        {
                            // Determine the year for each month
                            int monthNumber = academicMonths[i];
                            int yearForMonth = sessionStartDate.Year;

                            // If month is January, February, or March, it belongs to the next calendar year
                            if (monthNumber <= 3)
                            {
                                yearForMonth = sessionStartDate.Year + 1;
                            }

                            // Check if this month falls within the session dates
                            DateTime monthStart = new DateTime(yearForMonth, monthNumber, 1);
                            DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                            MonthlyAttendanceData monthData;

                            // Only fetch data if the month is within session dates
                            if (monthStart <= sessionEndDate && monthEnd >= sessionStartDate)
                            {
                                monthData = GetMonthlyAttendanceData(conn, student.StudentID, monthNumber, yearForMonth);
                            }
                            else
                            {
                                // Month is outside session, initialize with zeros
                                monthData = new MonthlyAttendanceData
                                {
                                    Month = monthNumber,
                                    MonthName = monthNames[i],
                                    WorkingDays = 0,
                                    Present = 0,
                                    Absent = 0,
                                    Late = 0,
                                    HalfDay = 0,
                                    Holidays = 0,
                                    AttendancePercentage = 0
                                };
                            }

                            // Override month name to ensure consistency
                            monthData.MonthName = monthNames[i];
                            yearlyAttendance.MonthlyData.Add(monthData);
                        }

                        // FIXED: Calculate yearly totals with new logic
                        yearlyAttendance.TotalWorkingDays = yearlyAttendance.MonthlyData.Sum(m => m.WorkingDays);

                        // Get original counts
                        int originalPresent = yearlyAttendance.MonthlyData.Sum(m => m.Present);
                        int originalLate = yearlyAttendance.MonthlyData.Sum(m => m.Late);
                        int originalHalfDay = yearlyAttendance.MonthlyData.Sum(m => m.HalfDay);

                        // Present + Late + Half Day all count as Present
                        yearlyAttendance.TotalPresent = originalPresent + originalLate + originalHalfDay;

                        // Store original values separately (for Excel export if needed)
                        yearlyAttendance.TotalLate = originalLate;
                        yearlyAttendance.TotalHalfDay = originalHalfDay;

                        // Calculate absent as Working Days - All Present types
                        yearlyAttendance.TotalAbsent = yearlyAttendance.TotalWorkingDays - yearlyAttendance.TotalPresent;
                        if (yearlyAttendance.TotalAbsent < 0) yearlyAttendance.TotalAbsent = 0;

                        yearlyAttendance.TotalHolidays = yearlyAttendance.MonthlyData.Sum(m => m.Holidays);

                        // Calculate percentage and grade
                        if (yearlyAttendance.TotalWorkingDays > 0)
                        {
                            yearlyAttendance.AttendancePercentage =
                                Math.Round((decimal)yearlyAttendance.TotalPresent / yearlyAttendance.TotalWorkingDays * 100, 2);
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

                    // Calculate monthly statistics for academic year
                    for (int i = 0; i < 12; i++)
                    {
                        int monthNumber = academicMonths[i];
                        int yearForMonth = sessionStartDate.Year;

                        if (monthNumber <= 3)
                        {
                            yearForMonth = sessionStartDate.Year + 1;
                        }

                        DateTime monthStart = new DateTime(yearForMonth, monthNumber, 1);
                        DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                        // Only calculate stats if month is within session
                        if (monthStart <= sessionEndDate && monthEnd >= sessionStartDate)
                        {
                            var monthStats = CalculateMonthlyStatistics(conn, classId, sectionId, monthNumber, yearForMonth);
                            reportData.MonthlyStatistics.Add(monthStats);
                        }
                    }

                    reportData.GeneratedDate = DateTime.Now;
                    reportData.TotalStudents = students.Count;

                    // Add debug information
                    var debugInfo = new
                    {
                        SessionStart = sessionStartDate.ToString("yyyy-MM-dd"),
                        SessionEnd = sessionEndDate.ToString("yyyy-MM-dd"),
                        StudentsFound = students.Count,
                        FirstStudentData = students.FirstOrDefault() != null ?
                            $"{students.First().StudentName} - Total Working Days: {reportData.Students.First().TotalWorkingDays}" : "No students"
                    };

                    return Json(new
                    {
                        success = true,
                        data = reportData,
                        debug = debugInfo,
                        message = $"Report generated for session {sessionName} with {students.Count} students"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost]
        public JsonResult ExportYearlyAttendanceToExcel(string classId, string sectionId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Get session details first
                    DateTime sessionStartDate = DateTime.MinValue;
                    DateTime sessionEndDate = DateTime.MinValue;
                    string sessionName = "";

                    string sessionQuery = @"
                SELECT StartDate, EndDate,PrintName as SessionName 
                FROM AcademicSessionMaster 
                WHERE SessionID = @SessionID";

                    using (SqlCommand cmd = new SqlCommand(sessionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sessionStartDate = Convert.ToDateTime(reader["StartDate"]);
                                sessionEndDate = Convert.ToDateTime(reader["EndDate"]);
                                sessionName = reader["SessionName"]?.ToString() ?? "";
                            }
                        }
                    }

                    // Get school details
                    string schoolName = "";
                    string schoolAddress = "";
                    string schoolQuery = @"
                SELECT 
                    ISNULL(PrintTitle, 'School Name') as SchoolName,
                    ISNULL(Line1, 'School Address') as Address
                FROM Tenants 
                WHERE TenantID = @TenantID";

                    using (SqlCommand cmd = new SqlCommand(schoolQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                schoolName = reader["SchoolName"].ToString();
                                schoolAddress = reader["Address"].ToString();
                            }
                        }
                    }

                    // Get class and section names
                    string className = "";
                    string sectionName = "";

                    string classQuery = "SELECT ClassName FROM AcademicClassMaster WHERE ClassID = @ClassID";
                    using (SqlCommand cmd = new SqlCommand(classQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classId);
                        className = cmd.ExecuteScalar()?.ToString() ?? "";
                    }

                    string sectionQuery = "SELECT SectionName FROM AcademicSectionMaster WHERE SectionID = @SectionID";
                    using (SqlCommand cmd = new SqlCommand(sectionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SectionID", sectionId);
                        sectionName = cmd.ExecuteScalar()?.ToString() ?? "";
                    }

                    // Define academic year months (April = 4 to March = 3)
                    int[] academicMonths = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };
                    string[] monthNames = { "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar" };

                    // Get students data
                    string studentQuery = @"
                SELECT 
                    s.StudentID,
                    s.AdmsnNo AS AdmissionNo,
                    s.RollNo AS RollNumber,
                    RTRIM(LTRIM(s.FirstName + ' ' + ISNULL(s.LastName, ''))) AS StudentName,
                    RTRIM(LTRIM(ISNULL(s.FatherName, ''))) AS FatherName
                FROM StudentInfoBasic s
                WHERE s.ClassID = @ClassID 
                    AND s.SectionID = @SectionID
                    AND s.IsActive = 1
                    AND s.IsDeleted = 0
                    AND s.TenantID = @TenantID
                    AND s.SessionID = @SessionID
                ORDER BY CAST(s.RollNo AS INT), s.FirstName";

                    var students = new List<StudentYearlyAttendance>();
                    var studentList = new List<StudentData>();

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
                                studentList.Add(new StudentData
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

                    // Process each student with ACADEMIC YEAR ORDER
                    int serialNo = 1;
                    foreach (var student in studentList)
                    {
                        var yearlyAttendance = new StudentYearlyAttendance
                        {
                            SerialNo = serialNo++,
                            StudentID = student.StudentID,
                            AdmissionNo = student.AdmissionNo,
                            RollNumber = student.RollNumber,
                            StudentName = student.StudentName,
                            FatherName = student.FatherName,
                            MonthlyData = new List<MonthlyAttendanceData>()
                        };

                        // Initialize all 12 months in ACADEMIC YEAR ORDER (April to March)
                        for (int i = 0; i < 12; i++)
                        {
                            int monthNumber = academicMonths[i];
                            int yearForMonth = sessionStartDate.Year;

                            // If month is January, February, or March, it belongs to the next calendar year
                            if (monthNumber <= 3)
                            {
                                yearForMonth = sessionStartDate.Year + 1;
                            }

                            // Check if this month falls within the session dates
                            DateTime monthStart = new DateTime(yearForMonth, monthNumber, 1);
                            DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                            MonthlyAttendanceData monthData;

                            // Only fetch data if the month is within session dates
                            if (monthStart <= sessionEndDate && monthEnd >= sessionStartDate)
                            {
                                monthData = GetMonthlyAttendanceData(conn, student.StudentID, monthNumber, yearForMonth);
                            }
                            else
                            {
                                // Month is outside session, initialize with zeros
                                monthData = new MonthlyAttendanceData
                                {
                                    Month = monthNumber,
                                    MonthName = monthNames[i],
                                    WorkingDays = 0,
                                    Present = 0,
                                    Absent = 0,
                                    Late = 0,
                                    HalfDay = 0,
                                    Holidays = 0,
                                    AttendancePercentage = 0
                                };
                            }

                            // Override month name to ensure consistency
                            monthData.MonthName = monthNames[i];
                            yearlyAttendance.MonthlyData.Add(monthData);
                        }

                        // FIXED: Calculate yearly totals with new logic
                        yearlyAttendance.TotalWorkingDays = yearlyAttendance.MonthlyData.Sum(m => m.WorkingDays);

                        int originalPresent = yearlyAttendance.MonthlyData.Sum(m => m.Present);
                        int originalLate = yearlyAttendance.MonthlyData.Sum(m => m.Late);
                        int originalHalfDay = yearlyAttendance.MonthlyData.Sum(m => m.HalfDay);

                        // All count as Present
                        yearlyAttendance.TotalPresent = originalPresent + originalLate + originalHalfDay;
                        yearlyAttendance.TotalAbsent = yearlyAttendance.TotalWorkingDays - yearlyAttendance.TotalPresent;
                        if (yearlyAttendance.TotalAbsent < 0) yearlyAttendance.TotalAbsent = 0;

                        yearlyAttendance.TotalLate = originalLate;
                        yearlyAttendance.TotalHalfDay = originalHalfDay;
                        yearlyAttendance.TotalHolidays = yearlyAttendance.MonthlyData.Sum(m => m.Holidays);

                        // Calculate percentage and grade
                        if (yearlyAttendance.TotalWorkingDays > 0)
                        {
                            yearlyAttendance.AttendancePercentage =
                                Math.Round((decimal)yearlyAttendance.TotalPresent / yearlyAttendance.TotalWorkingDays * 100, 2);
                            yearlyAttendance.AttendanceGrade = GetAttendanceGrade(yearlyAttendance.AttendancePercentage);
                        }
                        else
                        {
                            yearlyAttendance.AttendancePercentage = 0;
                            yearlyAttendance.AttendanceGrade = "N/A";
                        }

                        students.Add(yearlyAttendance);
                    }

                    // Create Excel package
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Yearly Attendance");

                        // Set all cells to black font from the start
                        worksheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        // Add Header Information
                        int currentRow = 1;
                        worksheet.Cells[currentRow, 1].Value = schoolName;
                        worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
                        worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        currentRow++;
                        worksheet.Cells[currentRow, 1].Value = schoolAddress;
                        worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        currentRow++;
                        worksheet.Cells[currentRow, 1].Value = $"Class: {className}    Section: {sectionName}    Session: {sessionName}";
                        worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        currentRow++;
                        worksheet.Cells[currentRow, 1].Value = "Yearly Attendance Report".ToUpper();
                        worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[currentRow, 1, currentRow, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[currentRow, 1, currentRow, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                        currentRow++;
                        worksheet.Cells[currentRow, 1].Value = "GRADE SCALE: A+ (95–100) | A (90–94) | B+ (85–89) | B (80–84) | C+ (75–79) | C (70–74) | D (60–69) | F (Below 60)".ToUpper();
                        worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // Add gray background color
                        
                        currentRow += 3; // Add spacing

                        // Create table headers
                        int headerRow = currentRow;
                        int col = 1;

                        // Student info headers
                        worksheet.Cells[headerRow, col].Value = "Sr";
                        worksheet.Cells[headerRow, col, headerRow + 1, col].Merge = true;
                        col++;

                        worksheet.Cells[headerRow, col].Value = "Adm. No";
                        worksheet.Cells[headerRow, col, headerRow + 1, col].Merge = true;
                        col++;

                        worksheet.Cells[headerRow, col].Value = "Roll No";
                        worksheet.Cells[headerRow, col, headerRow + 1, col].Merge = true;
                        col++;

                        worksheet.Cells[headerRow, col].Value = "Student Name";
                        worksheet.Cells[headerRow, col, headerRow + 1, col].Merge = true;
                        col++;

                        worksheet.Cells[headerRow, col].Value = "Father Name";
                        worksheet.Cells[headerRow, col, headerRow + 1, col].Merge = true;
                        col++;

                        // FIXED: Use academic year month order for headers and only show WD, P, A
                        foreach (var month in monthNames)
                        {
                            worksheet.Cells[headerRow, col].Value = month;
                            worksheet.Cells[headerRow, col, headerRow, col + 2].Merge = true; // Changed from 3 to 2 (only 3 columns)
                            worksheet.Cells[headerRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            col += 3;
                        }

                        // Yearly Total
                        worksheet.Cells[headerRow, col].Value = "Session Total";
                        worksheet.Cells[headerRow, col, headerRow, col + 2].Merge = true; // Changed from 3 to 2
                        worksheet.Cells[headerRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        col += 3;

                        worksheet.Cells[headerRow, col].Value = "%";
                        worksheet.Cells[headerRow, col, headerRow + 1, col].Merge = true;
                        col++;

                        worksheet.Cells[headerRow, col].Value = "Grade";
                        worksheet.Cells[headerRow, col, headerRow + 1, col].Merge = true;

                        // Sub headers
                        currentRow++;
                        col = 6;

                        // FIXED: Add sub-headers for each month (WD, P, A only)
                        for (int i = 0; i < 12; i++)
                        {
                            worksheet.Cells[currentRow, col++].Value = "WD";
                            worksheet.Cells[currentRow, col++].Value = "P";
                            worksheet.Cells[currentRow, col++].Value = "A";
                        }

                        // Session totals sub-headers
                        worksheet.Cells[currentRow, col++].Value = "WD";
                        worksheet.Cells[currentRow, col++].Value = "P";
                        worksheet.Cells[currentRow, col++].Value = "A";
                        
                        var fullHeaderRange = worksheet.Cells[headerRow, 1, currentRow, 46]; // currentRow includes sub-header row
                        fullHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        fullHeaderRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                        // Add student data
                        currentRow++;
                        foreach (var student in students)
                        {
                            col = 1;
                            worksheet.Cells[currentRow, col++].Value = student.SerialNo;
                            worksheet.Cells[currentRow, col++].Value = student.AdmissionNo;
                            worksheet.Cells[currentRow, col++].Value = student.RollNumber;
                            worksheet.Cells[currentRow, col++].Value = student.StudentName;
                            worksheet.Cells[currentRow, col++].Value = student.FatherName;

                            // FIXED: Monthly data with combined Present calculation
                            foreach (var monthData in student.MonthlyData)
                            {
                                int totalPresent = monthData.Present + monthData.Late + monthData.HalfDay;
                                int absent = monthData.WorkingDays - totalPresent;
                                if (absent < 0) absent = 0;

                                worksheet.Cells[currentRow, col++].Value = monthData.WorkingDays;
                                worksheet.Cells[currentRow, col++].Value = totalPresent;
                                worksheet.Cells[currentRow, col++].Value = absent;
                            }

                            // Session totals
                            worksheet.Cells[currentRow, col++].Value = student.TotalWorkingDays;
                            worksheet.Cells[currentRow, col++].Value = student.TotalPresent;
                            worksheet.Cells[currentRow, col++].Value = student.TotalAbsent;

                            worksheet.Cells[currentRow, col++].Value = student.AttendancePercentage.ToString("0.00") + "%";
                            worksheet.Cells[currentRow, col++].Value = student.AttendanceGrade;

                            currentRow++;
                        }

                        // Apply styles and borders
                        var headerRange = worksheet.Cells[headerRow, 1, headerRow + 1, col - 1];
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        var tableRange = worksheet.Cells[headerRow, 1, currentRow - 1, col - 1];
                        var border = tableRange.Style.Border;
                        border.Top.Style = ExcelBorderStyle.Thin;
                        border.Bottom.Style = ExcelBorderStyle.Thin;
                        border.Left.Style = ExcelBorderStyle.Thin;
                        border.Right.Style = ExcelBorderStyle.Thin;

                        // Ensure all text is black
                        worksheet.Cells[1, 1, currentRow, col].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        // Auto-fit columns
                        worksheet.Cells.AutoFitColumns();

                        // Convert to byte array
                        byte[] fileBytes = package.GetAsByteArray();
                        string fileName = $"YearlyAttendance_{className}_{sectionName}_{sessionName.Replace("/", "_")}.xlsx";

                        return Json(new
                        {
                            success = true,
                            fileContent = Convert.ToBase64String(fileBytes),
                            fileName = fileName
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        #endregion

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

        // Helper method to get monthly attendance data for a student - FIXED VERSION
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

            // Count total days in month
            int totalDaysInMonth = DateTime.DaysInMonth(year, month);

            // Get first and last date of the month
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = new DateTime(year, month, totalDaysInMonth);

            // Count Sundays
            int sundays = 0;
            for (int day = 1; day <= totalDaysInMonth; day++)
            {
                if (new DateTime(year, month, day).DayOfWeek == DayOfWeek.Sunday)
                    sundays++;
            }

            // Count holidays (excluding Sundays to avoid double counting)
            string holidayCountQuery = @"
        SELECT COUNT(*) AS HolidayCount
        FROM HolidayCalendar 
        WHERE HolidayDate >= @FirstDay 
            AND HolidayDate <= @LastDay
            AND DATEPART(WEEKDAY, HolidayDate) != 1 -- Exclude Sundays
            AND IsDeleted = 0
            AND TenantID = @TenantID";

            int holidays = 0;
            using (SqlCommand cmd = new SqlCommand(holidayCountQuery, conn))
            {
                cmd.Parameters.AddWithValue("@FirstDay", firstDayOfMonth);
                cmd.Parameters.AddWithValue("@LastDay", lastDayOfMonth);
                cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);

                var result = cmd.ExecuteScalar();
                holidays = result != null ? Convert.ToInt32(result) : 0;
            }

            // FIXED: Calculate working days: Total days - Sundays - Holidays
            monthData.WorkingDays = totalDaysInMonth - sundays - holidays;
            monthData.Holidays = holidays;

            // Get attendance summary for the student
            string attendanceQuery = @"
        SELECT 
            Status,
            COUNT(*) as Count
        FROM StudentAttendance
        WHERE StudentID = @StudentID
            AND AttendanceDate >= @FirstDay 
            AND AttendanceDate <= @LastDay
            AND IsDeleted = 0
            AND TenantID = @TenantID
            AND SessionID = @SessionID
        GROUP BY Status";

            using (SqlCommand cmd = new SqlCommand(attendanceQuery, conn))
            {
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                cmd.Parameters.AddWithValue("@FirstDay", firstDayOfMonth);
                cmd.Parameters.AddWithValue("@LastDay", lastDayOfMonth);
                cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string status = reader["Status"].ToString();
                        int count = Convert.ToInt32(reader["Count"]);

                        switch (status.ToLower().Trim())
                        {
                            case "present":
                            case "p":
                                monthData.Present = count;
                                break;
                            case "absent":
                            case "a":
                                monthData.Absent = count;
                                break;
                            case "late":
                            case "l":
                                monthData.Late = count;
                                break;
                            case "half day":
                            case "halfday":
                            case "hd":
                            case "h":
                                monthData.HalfDay = count;
                                break;
                        }
                    }
                }
            }

            // FIXED: Calculate attendance percentage - Present + Late + Half Day all count as present
            if (monthData.WorkingDays > 0)
            {
                decimal effectivePresent = monthData.Present + monthData.Late + monthData.HalfDay;
                monthData.AttendancePercentage = Math.Round((effectivePresent / monthData.WorkingDays) * 100, 2);
            }

            return monthData;
        }

        // Calculate monthly statistics for the entire class - FIXED VERSION
        private MonthlyStats CalculateMonthlyStatistics(SqlConnection conn, string classId, string sectionId, int month, int year)
        {
            var stats = new MonthlyStats
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMMM yyyy"), // Include year for clarity
                TotalStudents = 0,
                AverageAttendance = 0,
                BestAttendance = 0,
                WorstAttendance = 100
            };

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            // FIXED: Updated query to count all Present, Late, and Half Day as present
            string query = @"
        WITH StudentAttendanceStats AS (
            SELECT 
                s.StudentID,
                COUNT(DISTINCT CASE WHEN sa.Status IN ('Present', 'P', 'Late', 'L', 'Half Day', 'HalfDay', 'HD', 'H') 
                    THEN sa.AttendanceDate END) as TotalPresentDays,
                COUNT(DISTINCT sa.AttendanceDate) as TotalRecordedDays
            FROM StudentInfoBasic s
            LEFT JOIN StudentAttendance sa ON s.StudentID = sa.StudentID
                AND sa.AttendanceDate >= @FirstDay
                AND sa.AttendanceDate <= @LastDay
                AND sa.IsDeleted = 0
                AND sa.TenantID = @TenantID
                AND sa.SessionID = @SessionID
            WHERE s.ClassID = @ClassID
                AND s.SectionID = @SectionID
                AND s.IsActive = 1
                AND s.IsDeleted = 0
                AND s.TenantID = @TenantID
                AND s.SessionID = @SessionID
            GROUP BY s.StudentID
        )
        SELECT 
            COUNT(*) as TotalStudents,
            AVG(TotalPresentDays * 100.0 / NULLIF(TotalRecordedDays, 0)) as AverageAttendance,
            MAX(TotalPresentDays * 100.0 / NULLIF(TotalRecordedDays, 0)) as BestAttendance,
            MIN(TotalPresentDays * 100.0 / NULLIF(TotalRecordedDays, 0)) as WorstAttendance
        FROM StudentAttendanceStats";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ClassID", classId);
                cmd.Parameters.AddWithValue("@SectionID", sectionId);
                cmd.Parameters.AddWithValue("@FirstDay", firstDayOfMonth);
                cmd.Parameters.AddWithValue("@LastDay", lastDayOfMonth);
                cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stats.TotalStudents = reader["TotalStudents"] != DBNull.Value ?
                            Convert.ToInt32(reader["TotalStudents"]) : 0;
                        stats.AverageAttendance = reader["AverageAttendance"] != DBNull.Value ?
                            Math.Round(Convert.ToDecimal(reader["AverageAttendance"]), 2) : 0;
                        stats.BestAttendance = reader["BestAttendance"] != DBNull.Value ?
                            Math.Round(Convert.ToDecimal(reader["BestAttendance"]), 2) : 0;
                        stats.WorstAttendance = reader["WorstAttendance"] != DBNull.Value ?
                            Math.Round(Convert.ToDecimal(reader["WorstAttendance"]), 2) : 100;
                    }
                }
            }

            return stats;
        }

        public ActionResult MonthlyReport()
        {
            var classesResult = _dropdownController.GetClasses();
            var sectionsResult = _dropdownController.GetSections();

            // Get session-based months instead of static list
            var sessionMonths = GetSessionMonthsForDropdown();

            var model = new MonthlyAttendanceViewModel
            {
                Classes = ConvertToSelectList(classesResult),
                Sections = ConvertToSelectList(sectionsResult),
                Months = sessionMonths,
                // Remove Years dropdown since months already include year info
            };

            return View(model);
        }
        private List<SelectListItem> GetSessionMonthsForDropdown()
        {
            var months = new List<SelectListItem>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Get current session details
                    DateTime sessionStartDate = DateTime.MinValue;
                    DateTime sessionEndDate = DateTime.MinValue;

                    string sessionQuery = @"
                SELECT StartDate, EndDate, SessionName 
                FROM AcademicSessionMaster 
                WHERE SessionID = @SessionID";

                    using (SqlCommand cmd = new SqlCommand(sessionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sessionStartDate = Convert.ToDateTime(reader["StartDate"]);
                                sessionEndDate = Convert.ToDateTime(reader["EndDate"]);
                            }
                        }
                    }

                    // Build months list based on session dates
                    var currentDate = new DateTime(sessionStartDate.Year, sessionStartDate.Month, 1);
                    var endDate = new DateTime(sessionEndDate.Year, sessionEndDate.Month, 1);

                    while (currentDate <= endDate)
                    {
                        months.Add(new SelectListItem
                        {
                            Value = $"{currentDate.Month}-{currentDate.Year}",
                            Text = $"{currentDate.ToString("MMMM")} {currentDate.Year}",
                            Selected = (currentDate.Month == DateTime.Now.Month && currentDate.Year == DateTime.Now.Year)
                        });
                        currentDate = currentDate.AddMonths(1);
                    }
                }
            }
            catch
            {
                // Fallback to current month if error
                months.Add(new SelectListItem
                {
                    Value = $"{DateTime.Now.Month}-{DateTime.Now.Year}",
                    Text = DateTime.Now.ToString("MMMM yyyy"),
                    Selected = true
                });
            }

            return months;
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
                // Server-side validation
                var validation = ValidateAttendanceDate(attendanceDate);
                if (!validation.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        validationError = true,
                        messages = validation.Messages,
                        message = string.Join(". ", validation.Messages)
                    });
                }

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

                    // Server-side validation
                    var validation = ValidateAttendanceDate(request.AttendanceDate);
                    if (!validation.IsValid)
                    {
                        return Json(new
                        {
                            success = false,
                            validationError = true,
                            message = string.Join(". ", validation.Messages)
                        });
                    }

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
        [HttpGet]
        public JsonResult GetHolidaysForValidation()
        {
            try
            {
                var holidays = new List<object>();

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT 
                    HolidayDate,
                    HolidayName
                FROM HolidayCalendar
                WHERE IsDeleted = 0
                    AND IsActive = 1
                    AND TenantID = @TenantID
                    AND SessionID = @SessionID
                ORDER BY HolidayDate";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                holidays.Add(new
                                {
                                    date = Convert.ToDateTime(reader["HolidayDate"]).ToString("yyyy-MM-dd"),
                                    name = reader["HolidayName"].ToString()
                                });
                            }
                        }
                    }
                }

                return Json(new { success = true, holidays = holidays }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        // Get monthly attendance report data - FIXED VERSION
        [HttpPost]
    
        public JsonResult GetMonthlyAttendanceReport(string classId, string sectionId, string monthYear)
        {
            try
            {
                // Parse month and year from the combined value
                string[] parts = monthYear.Split('-');
                int month = int.Parse(parts[0]);
                int year = int.Parse(parts[1]);

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Get session details first
                    DateTime sessionStartDate = DateTime.MinValue;
                    DateTime sessionEndDate = DateTime.MinValue;
                    string sessionName = "";

                    string sessionQuery = @"
                SELECT StartDate, EndDate, SessionName 
                FROM AcademicSessionMaster 
                WHERE SessionID = @SessionID";

                    using (SqlCommand cmd = new SqlCommand(sessionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sessionStartDate = Convert.ToDateTime(reader["StartDate"]);
                                sessionEndDate = Convert.ToDateTime(reader["EndDate"]);
                                sessionName = reader["SessionName"]?.ToString() ?? "";
                            }
                        }
                    }

                    // Validate the selected month is within session
                    DateTime selectedMonthStart = new DateTime(year, month, 1);
                    DateTime selectedMonthEnd = selectedMonthStart.AddMonths(1).AddDays(-1);

                    // Adjust if month partially falls within session
                    DateTime effectiveStartDate = selectedMonthStart < sessionStartDate ? sessionStartDate : selectedMonthStart;
                    DateTime effectiveEndDate = selectedMonthEnd > sessionEndDate ? sessionEndDate : selectedMonthEnd;

                    // Check if month is completely outside session
                    if (selectedMonthEnd < sessionStartDate || selectedMonthStart > sessionEndDate)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Selected month {selectedMonthStart.ToString("MMMM yyyy")} is outside the session period ({sessionStartDate.ToString("dd-MMM-yyyy")} to {sessionEndDate.ToString("dd-MMM-yyyy")})"
                        });
                    }

                    // Get school details for print header
                    string schoolName = "";
                    string schoolAddress = "";
                    string schoolQuery = @"
                SELECT 
                    ISNULL(PrintTitle, 'School Name') as SchoolName,
                    ISNULL(Line1, 'School Address') as Address
                FROM Tenants 
                WHERE TenantID = @TenantID";

                    using (SqlCommand cmd = new SqlCommand(schoolQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                schoolName = reader["SchoolName"].ToString();
                                schoolAddress = reader["Address"].ToString();
                            }
                        }
                    }

                    var reportData = new MonthlyReportData
                    {
                        Students = new List<StudentMonthlyAttendance>(),
                        DatesInMonth = new List<DateInfo>(),
                        Month = month,
                        Year = year
                    };

                    // Get class and section names
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

                    // Month name with correct year display
                    reportData.MonthName = new DateTime(year, month, 1).ToString("MMMM yyyy");

                    // Build dates list with weekday info (only for days within session)
                    var firstDay = new DateTime(year, month, 1);
                    var lastDay = firstDay.AddMonths(1).AddDays(-1);
                    int totalDaysInMonth = 0;
                    int effectiveDaysInMonth = 0;

                    // Count Sundays (only within effective dates)
                    int sundays = 0;
                    for (DateTime date = effectiveStartDate; date <= effectiveEndDate; date = date.AddDays(1))
                    {
                        if (date.DayOfWeek == DayOfWeek.Sunday)
                            sundays++;
                        effectiveDaysInMonth++;
                    }
                    totalDaysInMonth = DateTime.DaysInMonth(year, month);

                    // Get holidays for the month (only within effective dates and excluding Sundays)
                    var holidays = new HashSet<DateTime>();
                    int holidayCount = 0;
                    string holidayQuery = @"
                SELECT HolidayDate 
                FROM HolidayCalendar 
                WHERE HolidayDate >= @EffectiveStartDate
                    AND HolidayDate <= @EffectiveEndDate
                    AND DATEPART(WEEKDAY, HolidayDate) != 1  -- Exclude Sundays
                    AND IsDeleted = 0
                    AND TenantID = @TenantID";

                    using (SqlCommand cmd = new SqlCommand(holidayQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@EffectiveStartDate", effectiveStartDate);
                        cmd.Parameters.AddWithValue("@EffectiveEndDate", effectiveEndDate);
                        cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["HolidayDate"] != DBNull.Value)
                                {
                                    holidays.Add(Convert.ToDateTime(reader["HolidayDate"]).Date);
                                    holidayCount++;
                                }
                            }
                        }
                    }

                    // Calculate working days = Effective Days - Sundays - Holidays
                    int workingDays = effectiveDaysInMonth - sundays - holidayCount;

                    // Build date list for the entire month (but mark days outside session)
                    for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
                    {
                        bool isSunday = date.DayOfWeek == DayOfWeek.Sunday;
                        bool isHoliday = holidays.Contains(date.Date);
                        bool isWithinSession = date >= effectiveStartDate && date <= effectiveEndDate;

                        reportData.DatesInMonth.Add(new DateInfo
                        {
                            Date = date,
                            Day = date.Day.ToString("00"),
                            WeekDay = date.ToString("ddd").Substring(0, Math.Min(2, date.ToString("ddd").Length)).ToUpper(),
                            IsSunday = isSunday,
                            IsHoliday = isHoliday,
                            IsWithinSession = isWithinSession,
                            // Add color information for UI
                            DayColor = !isWithinSession ? "#D3D3D3" : (isSunday || isHoliday ? "#FFFF00" : "")
                        });
                    }

                    reportData.WorkingDays = workingDays;
                    reportData.TotalDaysInMonth = totalDaysInMonth;
                    reportData.EffectiveDaysInMonth = effectiveDaysInMonth;
                    reportData.TotalSundays = sundays;
                    reportData.TotalHolidays = holidayCount;

                    // Get all students with Roll Number
                    var students = new List<StudentData>();
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
                ORDER BY CAST(s.RollNo AS INT), s.FirstName";

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

                    reportData.TotalStudents = students.Count;

                    // Process each student
                    int serialNo = 1;
                    decimal totalAttendancePercentage = 0;
                    int validStudents = 0;

                    foreach (var student in students)
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
                            DailyAttendanceColors = new Dictionary<string, string>(),
                            TotalPresent = 0,
                            TotalAbsent = 0,
                            TotalLate = 0,
                            TotalHalfDay = 0
                        };

                        // Get attendance for this student for the month (only within effective dates)
                        string attendanceQuery = @"
                    SELECT 
                        DAY(AttendanceDate) as DayOfMonth,
                        AttendanceDate,
                        Status
                    FROM StudentAttendance
                    WHERE StudentID = @StudentID
                        AND AttendanceDate >= @EffectiveStartDate
                        AND AttendanceDate <= @EffectiveEndDate
                        AND IsDeleted = 0
                        AND TenantID = @TenantID
                        AND SessionID = @SessionID";

                        using (SqlCommand cmd = new SqlCommand(attendanceQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@StudentID", student.StudentID);
                            cmd.Parameters.AddWithValue("@EffectiveStartDate", effectiveStartDate);
                            cmd.Parameters.AddWithValue("@EffectiveEndDate", effectiveEndDate);
                            cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                            cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader["DayOfMonth"] != DBNull.Value && reader["Status"] != DBNull.Value)
                                    {
                                        int day = Convert.ToInt32(reader["DayOfMonth"]);
                                        DateTime attendanceDate = Convert.ToDateTime(reader["AttendanceDate"]);
                                        string status = reader["Status"].ToString();
                                        string dayKey = day.ToString("00");

                                        // Only process if within session dates
                                        if (attendanceDate >= effectiveStartDate && attendanceDate <= effectiveEndDate)
                                        {
                                            // Get status code and color
                                            string statusCode = "";
                                            string statusColor = "";

                                            switch (status.ToLower().Trim())
                                            {
                                                case "present":
                                                case "p":
                                                    statusCode = "P";
                                                    statusColor = "#008000"; // Green
                                                    monthlyAttendance.TotalPresent++;
                                                    break;

                                                case "absent":
                                                case "a":
                                                    statusCode = "A";
                                                    statusColor = "#FF0000"; // Red
                                                    monthlyAttendance.TotalAbsent++;
                                                    break;

                                                case "late":
                                                case "l":
                                                    statusCode = "L";
                                                    statusColor = "#FF6600"; // Orange
                                                    monthlyAttendance.TotalLate++;
                                                    break;

                                                case "half day":
                                                case "halfday":
                                                case "hd":
                                                    statusCode = "HD";   // ✅ Half Day
                                                    statusColor = "#0000FF"; // Blue
                                                    monthlyAttendance.TotalHalfDay++;
                                                    break;

                                                case "holiday":
                                                case "holy day":
                                                case "h":
                                                    statusCode = "H";    // ✅ Holiday
                                                    statusColor = "#FFFF00"; // Yellow
                                                    break;
                                            }


                                            monthlyAttendance.DailyAttendanceMonthly[dayKey] = statusCode;
                                            monthlyAttendance.DailyAttendanceColors[dayKey] = statusColor;
                                        }
                                    }
                                }
                            }
                        }

                        // Calculate totals and percentage
                        // Effective Present = Present + Late + Half Day
                        decimal effectivePresent = monthlyAttendance.TotalPresent +
                                                  monthlyAttendance.TotalLate +
                                                  monthlyAttendance.TotalHalfDay;

                        // Recalculate Absent = Working Days - Effective Present
                        monthlyAttendance.TotalAbsent = workingDays - (int)effectivePresent;
                        if (monthlyAttendance.TotalAbsent < 0)
                            monthlyAttendance.TotalAbsent = 0;

                        // Store effective present for display
                        monthlyAttendance.TotalEffectivePresent = (int)effectivePresent;

                        // Calculate attendance percentage
                        if (workingDays > 0)
                        {
                            monthlyAttendance.AttendancePercentage = Math.Round((effectivePresent / workingDays) * 100, 1);
                            totalAttendancePercentage += monthlyAttendance.AttendancePercentage;
                            validStudents++;
                        }
                        else
                        {
                            monthlyAttendance.AttendancePercentage = 0;
                        }

                        reportData.Students.Add(monthlyAttendance);
                    }

                    // Calculate average attendance
                    reportData.AverageAttendance = validStudents > 0 ?
                        Math.Round(totalAttendancePercentage / validStudents, 2) : 0;

                    // Add session info to response
                    reportData.SessionName = sessionName;
                    reportData.SessionStartDate = sessionStartDate;
                    reportData.SessionEndDate = sessionEndDate;
                    reportData.IsPartialMonth = (effectiveStartDate != selectedMonthStart || effectiveEndDate != selectedMonthEnd);

                    return Json(new
                    {
                        success = true,
                        data = reportData,
                        schoolName = schoolName,
                        schoolAddress = schoolAddress,
                        colors = new
                        {
                            present = "#008000",
                            absent = "#FF0000",
                            late = "#FF6600",
                            halfDay = "#0000FF",
                            holiday = "#FFFF00",
                            background = "#D3D3D3",
                            outsideSession = "#E0E0E0",
                            fontColor = "#000000"
                        },
                        sessionInfo = new
                        {
                            sessionName = sessionName,
                            sessionStart = sessionStartDate.ToString("dd-MMM-yyyy"),
                            sessionEnd = sessionEndDate.ToString("dd-MMM-yyyy"),
                            effectiveStart = effectiveStartDate.ToString("dd-MMM-yyyy"),
                            effectiveEnd = effectiveEndDate.ToString("dd-MMM-yyyy"),
                            isPartialMonth = reportData.IsPartialMonth
                        },
                        message = $"Report generated for {reportData.MonthName} with {students.Count} students"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error generating report: {ex.Message}",
                    details = ex.ToString()
                });
            }
        }
        //[HttpPost]
        //public JsonResult ExportMonthlyAttendanceToExcel(string classId, string sectionId, int month, int year)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(ConnectionString))
        //        {
        //            conn.Open();

        //            // Get school details
        //            string schoolName = "";
        //            string schoolAddress = "";
        //            string schoolQuery = @"
        //        SELECT 
        //            ISNULL(PrintTitle, 'School Name') as SchoolName,
        //            ISNULL(Line1, 'School Address') as Address
        //        FROM Tenants 
        //        WHERE TenantID = @TenantID";

        //            using (SqlCommand cmd = new SqlCommand(schoolQuery, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@TenantID", CurrentTenantID);
        //                using (SqlDataReader reader = cmd.ExecuteReader())
        //                {
        //                    if (reader.Read())
        //                    {
        //                        schoolName = reader["SchoolName"].ToString();
        //                        schoolAddress = reader["Address"].ToString();
        //                    }
        //                }
        //            }

        //            // Get class and section names
        //            string className = "";
        //            string sectionName = "";

        //            string classQuery = "SELECT ClassName FROM AcademicClassMaster WHERE ClassID = @ClassID";
        //            using (SqlCommand cmd = new SqlCommand(classQuery, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@ClassID", classId);
        //                className = cmd.ExecuteScalar()?.ToString() ?? "";
        //            }

        //            string sectionQuery = "SELECT SectionName FROM AcademicSectionMaster WHERE SectionID = @SectionID";
        //            using (SqlCommand cmd = new SqlCommand(sectionQuery, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@SectionID", sectionId);
        //                sectionName = cmd.ExecuteScalar()?.ToString() ?? "";
        //            }

        //            // Get session year
        //            string sessionYear = "";
        //            string sessionQuery = "SELECT SessionName FROM AcademicSessionMaster WHERE SessionID = @SessionID";
        //            using (SqlCommand cmd = new SqlCommand(sessionQuery, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@SessionID", CurrentSessionID);
        //                sessionYear = cmd.ExecuteScalar()?.ToString() ?? "2025-26";
        //            }

        //            // Build the report data (reuse the logic from GetMonthlyAttendanceReport)
        //            // [Include the same logic for building report data]

        //            // Create Excel package
        //            using (var package = new ExcelPackage())
        //            {
        //                var worksheet = package.Workbook.Worksheets.Add("Monthly Attendance");

        //                // Set all cells to black font
        //                worksheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);

        //                // Add Header Information
        //                int currentRow = 1;
        //                worksheet.Cells[currentRow, 1].Value = schoolName;
        //                worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
        //                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
        //                worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
        //                worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //                currentRow++;
        //                worksheet.Cells[currentRow, 1].Value = schoolAddress;
        //                worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
        //                worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //                currentRow++;
        //                worksheet.Cells[currentRow, 1].Value = $"Class: {className} | Section: {sectionName} | Month: {new DateTime(year, month, 1).ToString("MMMM yyyy")} | Session: {sessionYear}";
        //                worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
        //                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
        //                worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //                currentRow++;
        //                worksheet.Cells[currentRow, 1].Value = "MONTHLY ATTENDANCE REPORT";
        //                worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
        //                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
        //                worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //                currentRow += 2;

        //                // [Add table headers and data similar to yearly report but for monthly data]
        //                // Headers: S.No, Student, Father, [Days 1-31], Total, P, %, A, L

        //                // Apply borders
        //                var tableRange = worksheet.Cells[currentRow, 1, currentRow + students.Count, 36];
        //                var border = tableRange.Style.Border;
        //                border.Top.Style = ExcelBorderStyle.Thin;
        //                border.Bottom.Style = ExcelBorderStyle.Thin;
        //                border.Left.Style = ExcelBorderStyle.Thin;
        //                border.Right.Style = ExcelBorderStyle.Thin;
        //                border.Top.Color.SetColor(System.Drawing.Color.Black);
        //                border.Bottom.Color.SetColor(System.Drawing.Color.Black);
        //                border.Left.Color.SetColor(System.Drawing.Color.Black);
        //                border.Right.Color.SetColor(System.Drawing.Color.Black);

        //                // Auto-fit columns
        //                worksheet.Cells.AutoFitColumns();

        //                // Convert to byte array
        //                byte[] fileBytes = package.GetAsByteArray();
        //                string fileName = $"MonthlyAttendance_{className}_{sectionName}_{new DateTime(year, month, 1).ToString("MMM_yyyy")}.xlsx";

        //                return Json(new
        //                {
        //                    success = true,
        //                    fileContent = Convert.ToBase64String(fileBytes),
        //                    fileName = fileName
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Error: " + ex.Message });
        //    }
        //}
        private string GetUpdatedStatusCode(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "";

            switch (status.ToLower().Trim())
            {
                case "present": return "P";
                case "absent": return "A";
                case "late": return "L";
                case "half day":
                case "halfday": return "H";  // Changed from HD to H
                case "holiday":
                case "holy day": return "HD"; // Changed from H to HD
                default: return "";
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