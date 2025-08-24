// Models/Attendance/AttendanceModels.cs
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Models.Attendance
{
    // Add to Models/Attendance folder

    public class MonthlyAttendanceViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public List<SelectListItem> Months { get; set; }
        public List<SelectListItem> Years { get; set; }
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
    }

    public class MonthlyReportData
    {
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string MonthName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalStudents { get; set; }
        public int WorkingDays { get; set; }
        public decimal AverageAttendance { get; set; }
        public List<DateInfo> DatesInMonth { get; set; }
        public List<StudentMonthlyAttendance> Students { get; set; }
    }

    public class DateInfo
    {
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public string WeekDay { get; set; }
        public bool IsSunday { get; set; }
        public bool IsHoliday { get; set; }
    }

   
    public class YearlyAttendanceViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public List<SelectListItem> Years { get; set; }
        public int SelectedYear { get; set; }

        public YearlyAttendanceViewModel()
        {
            Classes = new List<SelectListItem>();
            Sections = new List<SelectListItem>();
            Years = new List<SelectListItem>();
            SelectedYear = DateTime.Now.Year;
        }
    }

    // Main report data model
    public class YearlyReportData
    {
        public List<StudentYearlyAttendance> Students { get; set; }
        public List<MonthlyStats> MonthlyStatistics { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int Year { get; set; }
        public string SessionYear { get; set; }
        public DateTime GeneratedDate { get; set; }
        public int TotalStudents { get; set; }

        public YearlyReportData()
        {
            Students = new List<StudentYearlyAttendance>();
            MonthlyStatistics = new List<MonthlyStats>();
        }
    }

    // Individual student yearly attendance
    public class StudentYearlyAttendance
    {
        public int SerialNo { get; set; }
        public string StudentID { get; set; }
        public string AdmissionNo { get; set; }
        public string RollNumber { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MobileNumber { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public List<MonthlyAttendanceData> MonthlyData { get; set; }
        public int TotalWorkingDays { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLate { get; set; }
        public int TotalHalfDay { get; set; }
        public int TotalHolidays { get; set; }
        public decimal AttendancePercentage { get; set; }
        public string AttendanceGrade { get; set; }
        public string AttendanceColor { get; set; }
    }

    // Monthly attendance data for a student
    public class MonthlyAttendanceData
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
       
        public int WorkingDays { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int HalfDay { get; set; }
        public int Holidays { get; set; }
        public decimal AttendancePercentage { get; set; }

        // Quick status indicator
        public string StatusColor
        {
            get
            {
                if (AttendancePercentage >= 90) return "#28a745"; // Green
                if (AttendancePercentage >= 75) return "#ffc107"; // Yellow
                return "#dc3545"; // Red
            }
        }
    }

    // Monthly statistics for the entire class
    public class MonthlyStats
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int TotalStudents { get; set; }
        public decimal AverageAttendance { get; set; }
        public decimal BestAttendance { get; set; }
        public decimal WorstAttendance { get; set; }
    }
    public class AttendanceViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public DateTime SelectedDate { get; set; }
        public string SelectedClassName { get; set; }
        public string SelectedSectionName { get; set; }
    }
    public class DailyAttendanceViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public List<SelectListItem> StatusOptions { get; set; }
        public DateTime SelectedDate { get; set; }
    }
    public class StudentAttendanceModel
    {
        public string StudentID { get; set; }
        public string AdmissionNo { get; set; }
        public string RollNumber { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MobileNumber { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string Status { get; set; } // Present, Absent, Late, Half Day, Holiday
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string Source { get; set; }
        public string Note { get; set; }
        public DateTime AttendanceDate { get; set; }
    }

    public class SaveAttendanceRequest
    {
        public List<AttendanceEntry> Attendance { get; set; }
        public string ClassId { get; set; }
        public string SectionId { get; set; }
        public DateTime AttendanceDate { get; set; }
    }

    public class AttendanceEntry
    {
        public string StudentID { get; set; }
        public string Status { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string Note { get; set; }
    }

    public class AttendanceReportModel
    {
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string Month { get; set; }
        public string Session { get; set; }
        public List<StudentMonthlyAttendance> Students { get; set; }
        public List<DateTime> DatesInMonth { get; set; }
    }

    public class StudentMonthlyAttendance
    {
        public decimal AttendancePercentage { get; set; }
        public int SerialNo { get; set; }
        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public string RollNumber { get; set; }
        public string AdmissionNo { get; set; }
        public string FatherName { get; set; }
        public Dictionary<int, string> DailyAttendance { get; set; } // Day -> Status
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLate { get; set; }
        public int TotalHalfDay { get; set; }
    }

    public class AttendanceStatusOption
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public string ColorClass { get; set; }
    }
}