using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace ERPIndia.Dashboard.DTOs
{
    public class AdminDashboardViewModel
    {
        public UserInfo UserInfo { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public SummaryData SummaryData { get; set; }
        public FeesCollection FeesCollection { get; set; }
        public List<LeaveRequest> LeaveRequests { get; set; }
        public List<QuickLink> QuickLinks { get; set; }
        public FinancialSummary FinancialSummary { get; set; }
        public List<NoticeBoard> NoticeBoards { get; set; }
        public CollectionStats CollectionStats { get; set; }

        // Static method to load sample data
        public static AdminDashboardViewModel LoadSampleData()
        {
            return new AdminDashboardViewModel
            {
                UserInfo = new UserInfo
                {
                    Name = "Mr. Herald",
                    ProfileUrl = "profile.html"
                },
                LastUpdatedDate = new DateTime(2024, 6, 15),
                SummaryData = new SummaryData
                {
                    Students = new EntityCount { Total = 3654, Active = 3643, Inactive = 11 },
                    Teachers = new EntityCount { Total = 284, Active = 254, Inactive = 30 },
                    Staff = new EntityCount { Total = 162, Active = 161, Inactive = 2 },
                    Subjects = new EntityCount { Total = 82, Active = 81, Inactive = 1 }
                },
                FeesCollection = new FeesCollection
                {
                    Periods = new List<string> { "Q1: 2023", "Q1: 2023", "Q1: 2023", "Q1: 2023", "Q1: 2023", "uQ1: 2023l", "Q1: 2023", "Q1: 2023", "Q1: 2023" },
                    CollectedFees = new List<int> { 30, 40, 38, 40, 38, 30, 35, 38, 40 },
                    TotalFees = new List<int> { 45, 50, 48, 50, 48, 40, 40, 50, 55 }
                },
                LeaveRequests = new List<LeaveRequest>
                {
                    new LeaveRequest
                    {
                        Name = "James",
                        Position = "Physics Teacher",
                        Avatar = "/template/assets/img/profiles/avatar-14.jpg",
                        Type = "Emergency",
                        LeaveDates = "12 -13 May",
                        ApplyDate = "12 May"
                    },
                    new LeaveRequest
                    {
                        Name = "Ramien",
                        Position = "Accountant",
                        Avatar = "/template/assets/img/profiles/avatar-19.jpg",
                        Type = "Casual",
                        LeaveDates = "12 -13 May",
                        ApplyDate = "11 May"
                    }
                },
                QuickLinks = new List<QuickLink>
                {
                    new QuickLink
                    {
                        Title = "View Attendance",
                        Url = "student-attendance.html",
                        Icon = "ti ti-calendar-share",
                        BackgroundClass = "bg-warning-transparent",
                        ButtonHoverClass = "warning-btn-hover"
                    },
                    new QuickLink
                    {
                        Title = "New Events",
                        Url = "events.html",
                        Icon = "ti ti-speakerphone",
                        BackgroundClass = "bg-success-transparent",
                        ButtonHoverClass = "success-btn-hover"
                    },
                    new QuickLink
                    {
                        Title = "Membership Plans",
                        Url = "membership-plans.html",
                        Icon = "ti ti-sphere",
                        BackgroundClass = "bg-danger-transparent",
                        ButtonHoverClass = "danger-btn-hover"
                    },
                    new QuickLink
                    {
                        Title = "Finance & Accounts",
                        Url = "student-attendance.html",
                        Icon = "ti ti-moneybag",
                        BackgroundClass = "bg-secondary-transparent",
                        ButtonHoverClass = "secondary-btn-hover"
                    }
                },
                FinancialSummary = new FinancialSummary
                {
                    TotalEarnings = 64522.24m,
                    TotalExpenses = 60522.24m,
                    EarningsData = new List<int> { 43, 0, 86, 43, 64, 21, 43 },
                    ExpensesData = new List<int> { 43, 86, 0, 10, 21, 10, 43 }
                },
                NoticeBoards = new List<NoticeBoard>
                {
                    new NoticeBoard
                    {
                        Title = "New Syllabus Instructions",
                        Icon = "ti ti-books",
                        BackgroundClass = "bg-primary-transparent",
                        AddedDate = new DateTime(2024, 3, 11),
                        DaysRemaining = 20
                    },
                    new NoticeBoard
                    {
                        Title = "World Environment Day Program.....!!!",
                        Icon = "ti ti-note",
                        BackgroundClass = "bg-success-transparent",
                        AddedDate = new DateTime(2024, 4, 21),
                        DaysRemaining = 15
                    },
                    new NoticeBoard
                    {
                        Title = "Exam Preparation Notification!",
                        Icon = "ti ti-bell-check",
                        BackgroundClass = "bg-danger-transparent",
                        AddedDate = new DateTime(2024, 3, 13),
                        DaysRemaining = 12
                    },
                    new NoticeBoard
                    {
                        Title = "Online Classes Preparation",
                        Icon = "ti ti-notes",
                        BackgroundClass = "bg-skyblue-transparent",
                        AddedDate = new DateTime(2024, 5, 24),
                        DaysRemaining = 2
                    },
                    new NoticeBoard
                    {
                        Title = "Exam Time Table Release",
                        Icon = "ti ti-package",
                        BackgroundClass = "bg-warning-transparent",
                        AddedDate = new DateTime(2024, 5, 24),
                        DaysRemaining = 6
                    }
                },
                CollectionStats = new CollectionStats
                {
                    TotalFeesCollected = 25000.02m,
                    FineCollected = 456.64m,
                    StudentsNotPaid = 545,
                    TotalOutstanding = 456.64m,
                    FeesGrowthRate = 1.2m,
                    FineGrowthRate = -1.2m,
                    NonPaymentGrowthRate = 1.2m,
                    OutstandingGrowthRate = -1.2m
                }
            };
        }
    }

    public class UserInfo
    {
        public string Name { get; set; }
        public string ProfileUrl { get; set; }
    }

    public class EntityCount
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
    }

    public class SummaryData
    {
        public EntityCount Students { get; set; }
        public EntityCount Teachers { get; set; }
        public EntityCount Staff { get; set; }
        public EntityCount Subjects { get; set; }
    }

    public class FeesCollection
    {
        public List<string> Periods { get; set; }
        public List<int> CollectedFees { get; set; }
        public List<int> TotalFees { get; set; }
    }

    public class LeaveRequest
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Avatar { get; set; }
        public string Type { get; set; }
        public string LeaveDates { get; set; }
        public string ApplyDate { get; set; }
    }

    public class QuickLink
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public string BackgroundClass { get; set; }
        public string ButtonHoverClass { get; set; }
    }

    public class FinancialSummary
    {
        public decimal TotalEarnings { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<int> EarningsData { get; set; }
        public List<int> ExpensesData { get; set; }
    }

    public class NoticeBoard
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string BackgroundClass { get; set; }
        public DateTime AddedDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class CollectionStats
    {
        public decimal TotalFeesCollected { get; set; }
        public decimal FineCollected { get; set; }
        public int StudentsNotPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal FeesGrowthRate { get; set; }
        public decimal FineGrowthRate { get; set; }
        public decimal NonPaymentGrowthRate { get; set; }
        public decimal OutstandingGrowthRate { get; set; }
    }
    public class SchoolStatisticsModel
    {
        // Summary statistics
        public StatisticItem Students { get; set; }
        public StatisticItem Teachers { get; set; }
        public StatisticItem Staff { get; set; }
        public StatisticItem Subjects { get; set; }
    }

    public class StatisticItem
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public double GrowthPercentage { get; set; } // The 1.2% shown in the UI
    }
}

namespace ERPIndia.DTOs
{

    public class StudentInfoDto
    {
        /// <summary>
        /// Gets or sets the old balance
        /// </summary>
        public decimal OldBalance { get; set; }
        public DateTime? DOBUI { get; set; }
        /// <summary>
        /// Gets or sets the admission number
        /// </summary>
        public string AdmsnNo { get; set; }

        /// <summary>
        /// Gets or sets the school code
        /// </summary>
        public string SchoolCode { get; set; }

        /// <summary>
        /// Gets or sets the student number
        /// </summary>
        public string StudentNo { get; set; }

        /// <summary>
        /// Gets or sets the serial number
        /// </summary>
        public string SrNo { get; set; }

        /// <summary>
        /// Gets or sets the roll number
        /// </summary>
        public string RollNo { get; set; }

        /// <summary>
        /// Gets or sets the class ID as GUID
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Gets or sets the class name from AcademicClassMaster
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the section name from AcademicSectionMaster
        /// </summary>
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the section ID as GUID
        /// </summary>
        public string Section { get; set; }

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the father name from studentinfofamily
        /// </summary>
        public string FatherName { get; set; }

        /// <summary>
        /// Gets or sets the mother name from studentinfofamily
        /// </summary>
        public string MotherName { get; set; }

        /// <summary>
        /// Gets or sets the discount name from FeeDiscountMaster
        /// </summary>
        public string DiscountName { get; set; }

        /// <summary>
        /// Gets or sets the category name from FeeCategoryMaster
        /// </summary>
        public string CategoryName { get; set; }
        public string Photo { get; set; }
        public string PickupName { get; set; }
        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the date of birth
        /// </summary>
        public DateTime? DOB { get; set; }

        /// <summary>
        /// Gets or sets the category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the religion
        /// </summary>
        public string Religion { get; set; }

        /// <summary>
        /// Gets or sets the caste
        /// </summary>
        public string Caste { get; set; }

        /// <summary>
        /// Gets or sets the mobile number
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// Gets or sets the discount category as GUID
        /// </summary>
        public Guid? DiscountCategory { get; set; }

        /// <summary>
        /// Gets or sets the fee category as GUID
        /// </summary>
        public Guid FeeCategory { get; set; }

        /// <summary>
        /// Gets or sets the class ID as GUID
        /// </summary>
        public Guid ClassId { get; set; }

        /// <summary>
        /// Gets or sets the section ID as GUID
        /// </summary>
        public Guid SectionId { get; set; }

        /// <summary>
        /// Gets or sets the house ID as GUID, can be empty GUID
        /// </summary>
        public Guid HouseId { get; set; }

        /// <summary>
        /// Gets or sets the fee category ID as GUID
        /// </summary>
        public Guid FeeCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the fee discount ID as GUID, can be empty GUID
        /// </summary>
        public Guid FeeDiscountId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID as GUID
        /// </summary>
        public Guid TenantID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the student is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the student is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the created by user ID as GUID
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the session ID as GUID
        /// </summary>
        public Guid SessionID { get; set; }

        /// <summary>
        /// Gets or sets the student ID as GUID
        /// </summary>
        public Guid StudentId { get; set; }
    }
}