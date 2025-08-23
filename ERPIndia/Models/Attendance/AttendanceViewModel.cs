// Models/Attendance/AttendanceModels.cs
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Models.Attendance
{
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
        public int SerialNo { get; set; }
        public string StudentName { get; set; }
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