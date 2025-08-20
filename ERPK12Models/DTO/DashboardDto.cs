using System;
using System.Collections.Generic;

namespace DashboardModels
{
    public static class DashboardDataFactory
    {
        public static HeaderInfoDto HeaderInfoFromDb { get; set; }
        public static DashboardDto LoadDashboard()
        {
            return new DashboardDto
            {
                // User Information
                UserInfo = new UserInfoDto
                {
                    Name = "Mr. Herald",
                    WelcomeMessage = "Have a Good day at work",
                    LastUpdated = DateTime.Parse("2024-06-15"),
                    ProfileImageUrl = "/assets/images/profile-default.png"
                },

                // Today's Collection Card
                TodayCollection = new TodayCollectionDto
                {
                    TotalCollection = 678,
                    CashCollection = 352,
                    BankCollection = 326
                },

                // Fee Summary Card
                FeeSummary = new FeeSummaryDto
                {
                    TotalFees = 678,
                    TotalReceived = 352,
                    TotalDues = 326
                },

                // Student Summary Card
                StudentSummary = new StudentSummaryDto
                {
                    TotalStudents = 678,
                    TotalMale = 352,
                    TotalFemale = 326,
                    NewStudents = 367,
                    OldStudents = 311,
                    ActiveStudents = 650
                },

                // Attendance Summary Card
                AttendanceSummary = new AttendanceSummaryDto
                {
                    TodayPresent = 678,
                    TodayAbsent = 352,
                    TodayLeave = 326
                },

                // Enquiry Summary Card
                EnquirySummary = new EnquirySummaryDto
                {
                    TodayEnquiry = 18,
                    TotalEnquiry = 142,
                    PendingEnquiry = 24
                },

                // Staff Summary Card
                StaffSummary = new StaffSummaryDto
                {
                    TotalStaff = 62,
                    Teachers = 38,
                    AdminStaff = 24
                },

                // Payment Status Card
                PaymentStatus = new PaymentStatusDto
                {
                    NotPaid = 105,
                    Overdue = 45,
                    ReminderSent = 32
                },

                // Teacher/Staff Attendance
                TeacherStaffAttendance = new TeacherStaffAttendanceDto
                {
                    TeachersPresentToday = 25,
                    TeachersAbsentToday = 7,
                    StaffPresentToday = 50,
                    StaffAbsentToday = 7
                },

                // Classwise Collection
                ClasswiseCollection = new List<ClasswiseCollectionDto>
                {
                    new ClasswiseCollectionDto { ClassName = "Class 1", TotalAmount = 32000, PaidAmount = 5000, DueAmount = 3000 },
                    new ClasswiseCollectionDto { ClassName = "Class 2", TotalAmount = 35000, PaidAmount = 7000, DueAmount = 2000 },
                    new ClasswiseCollectionDto { ClassName = "Class 3", TotalAmount = 30000, PaidAmount = 4000, DueAmount = 6000 },
                    new ClasswiseCollectionDto { ClassName = "Class 4", TotalAmount = 28000, PaidAmount = 2000, DueAmount = 7000 },
                    new ClasswiseCollectionDto { ClassName = "Class 5", TotalAmount = 36000, PaidAmount = 6000, DueAmount = 4000 },
                    new ClasswiseCollectionDto { ClassName = "Class 6", TotalAmount = 40000, PaidAmount = 5000, DueAmount = 3000 }
                },

                // Classwise Students
                ClasswiseStudents = GenerateClasswiseStudents(),

                // Estimated Fee Collection
                EstimatedFeeCollection = new EstimatedFeeCollectionDto
                {
                    MonthlyData = new List<MonthlyEstimatedFeeDto>
                    {
                        new MonthlyEstimatedFeeDto { Month = "Apr", Expected = 450000, Received = 20000, Due = 350000 },
                        new MonthlyEstimatedFeeDto { Month = "May", Expected = 250000, Received = 10000, Due = 230000 },
                        new MonthlyEstimatedFeeDto { Month = "Jun", Expected = 100000, Received = 5000, Due = 95000 },
                        new MonthlyEstimatedFeeDto { Month = "Jul", Expected = 250000, Received = 10000, Due = 240000 },
                        new MonthlyEstimatedFeeDto { Month = "Aug", Expected = 150000, Received = 5000, Due = 145000 },
                        new MonthlyEstimatedFeeDto { Month = "Sep", Expected = 250000, Received = 10000, Due = 240000 },
                        new MonthlyEstimatedFeeDto { Month = "Oct", Expected = 200000, Received = 7000, Due = 193000 },
                        new MonthlyEstimatedFeeDto { Month = "Nov", Expected = 100000, Received = 5000, Due = 95000 },
                        new MonthlyEstimatedFeeDto { Month = "Dec", Expected = 400000, Received = 10000, Due = 390000 },
                        new MonthlyEstimatedFeeDto { Month = "Jan", Expected = 450000, Received = 20000, Due = 430000 },
                        new MonthlyEstimatedFeeDto { Month = "Feb", Expected = 400000, Received = 10000, Due = 390000 },
                        new MonthlyEstimatedFeeDto { Month = "Mar", Expected = 150000, Received = 5000, Due = 145000 }
                    }
                },

                // Financial Trends
                FinancialTrends = new FinancialTrendsDto
                {
                    Earnings = new TrendDataDto
                    {
                        Total = 5000,
                        MonthlyData = new List<MonthlyTrendDto>
                        {
                            new MonthlyTrendDto { Month = "Apr", Amount = 5000 },
                            new MonthlyTrendDto { Month = "May", Amount = 6000 },
                            new MonthlyTrendDto { Month = "Jun", Amount = 7000 },
                            new MonthlyTrendDto { Month = "Jul", Amount = 6500 },
                            new MonthlyTrendDto { Month = "Aug", Amount = 7200 },
                            new MonthlyTrendDto { Month = "Sep", Amount = 7800 },
                            new MonthlyTrendDto { Month = "Oct", Amount = 8000 },
                            new MonthlyTrendDto { Month = "Nov", Amount = 7700 },
                            new MonthlyTrendDto { Month = "Dec", Amount = 8500 },
                            new MonthlyTrendDto { Month = "Jan", Amount = 8100 },
                            new MonthlyTrendDto { Month = "Feb", Amount = 8600 },
                            new MonthlyTrendDto { Month = "Mar", Amount = 9000 }
                        }
                    },
                    Expenses = new TrendDataDto
                    {
                        Total = 5000,
                        MonthlyData = new List<MonthlyTrendDto>
                        {
                            new MonthlyTrendDto { Month = "Apr", Amount = 4200 },
                            new MonthlyTrendDto { Month = "May", Amount = 4800 },
                            new MonthlyTrendDto { Month = "Jun", Amount = 5600 },
                            new MonthlyTrendDto { Month = "Jul", Amount = 4300 },
                            new MonthlyTrendDto { Month = "Aug", Amount = 5900 },
                            new MonthlyTrendDto { Month = "Sep", Amount = 6100 },
                            new MonthlyTrendDto { Month = "Oct", Amount = 5000 },
                            new MonthlyTrendDto { Month = "Nov", Amount = 4700 },
                            new MonthlyTrendDto { Month = "Dec", Amount = 5300 },
                            new MonthlyTrendDto { Month = "Jan", Amount = 4500 },
                            new MonthlyTrendDto { Month = "Feb", Amount = 4200 },
                            new MonthlyTrendDto { Month = "Mar", Amount = 5922.24m }
                        }
                    },
                    Profit = new TrendDataDto
                    {
                        Total = 2500,
                        MonthlyData = new List<MonthlyTrendDto>
                        {
                            new MonthlyTrendDto { Month = "Apr", Amount = 1200 },
                            new MonthlyTrendDto { Month = "May", Amount = 1800 },
                            new MonthlyTrendDto { Month = "Jun", Amount = 2100 },
                            new MonthlyTrendDto { Month = "Jul", Amount = 1700 },
                            new MonthlyTrendDto { Month = "Aug", Amount = 2200 },
                            new MonthlyTrendDto { Month = "Sep", Amount = 2400 },
                            new MonthlyTrendDto { Month = "Oct", Amount = 2000 },
                            new MonthlyTrendDto { Month = "Nov", Amount = 1950 },
                            new MonthlyTrendDto { Month = "Dec", Amount = 2300 },
                            new MonthlyTrendDto { Month = "Jan", Amount = 2000 },
                            new MonthlyTrendDto { Month = "Feb", Amount = 2100 },
                            new MonthlyTrendDto { Month = "Mar", Amount = 2500 }
                        }
                    }
                },

                // Income Expense Chart
                IncomeExpenseChart = new IncomeExpenseChartDto
                {
                    MonthlyData = new List<MonthlyIncomeExpenseDto>
                    {
                        new MonthlyIncomeExpenseDto { Month = "April", Income = 30000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "May", Income = 40000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "June", Income = 38000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "July", Income = 39000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "Aug", Income = 37000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "Sep", Income = 39000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "Oct", Income = 30000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "Nov", Income = 35000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "Dec", Income = 40000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "Jan", Income = 40000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "Feb", Income = 40000, Expenses = 48000 },
                        new MonthlyIncomeExpenseDto { Month = "Mar", Income = 40000, Expenses = 48000 }
                    }
                },

                // Quick Details
                QuickDetails = new QuickDetailsDto
                {
                    UserSummary = new UserSummaryDto
                    {
                        TotalUsers = 13,
                        ActiveUsers = 5,
                        InactiveUsers = 8
                    },
                    ReceiptSummary = new ReceiptSummaryDto
                    {
                        TodayReceipts = 52,
                        TotalReceipts = 496,
                        TotalAmount = 58000
                    },
                    DeleteReceiptSummary = new DeleteReceiptSummaryDto
                    {
                        TodayDeleted = 18,
                        TotalDeleted = 52
                    },
                    // ADD THIS SECTION
                    SelfStudentsSummary = new SelfStudentsSummaryDto
                    {
                        TotalSelfStudents = 69,
                        TransportStudents = 52
                    }
                },

                // Classwise Attendance
                ClasswiseAttendance = new List<ClasswiseAttendanceDto>
                {
                    new ClasswiseAttendanceDto { ClassName = "Nursery", TotalPresent = 30, TotalStrength = 35 },
                    new ClasswiseAttendanceDto { ClassName = "LKG", TotalPresent = 28, TotalStrength = 30 },
                    new ClasswiseAttendanceDto { ClassName = "UKG", TotalPresent = 32, TotalStrength = 35 },
                    new ClasswiseAttendanceDto { ClassName = "Class 1", TotalPresent = 35, TotalStrength = 38 },
                    new ClasswiseAttendanceDto { ClassName = "Class 2", TotalPresent = 40, TotalStrength = 45 },
                    new ClasswiseAttendanceDto { ClassName = "Class 3", TotalPresent = 42, TotalStrength = 45 },
                    new ClasswiseAttendanceDto { ClassName = "Class 4", TotalPresent = 38, TotalStrength = 40 },
                    new ClasswiseAttendanceDto { ClassName = "Class 5", TotalPresent = 36, TotalStrength = 39 },
                    new ClasswiseAttendanceDto { ClassName = "Class 6", TotalPresent = 33, TotalStrength = 35 },
                    new ClasswiseAttendanceDto { ClassName = "Class 7", TotalPresent = 31, TotalStrength = 34 },
                    new ClasswiseAttendanceDto { ClassName = "Class 8", TotalPresent = 30, TotalStrength = 33 },
                    new ClasswiseAttendanceDto { ClassName = "Class 9", TotalPresent = 28, TotalStrength = 30 },
                    new ClasswiseAttendanceDto { ClassName = "Class 10", TotalPresent = 27, TotalStrength = 29 },
                    new ClasswiseAttendanceDto { ClassName = "Class 11", TotalPresent = 26, TotalStrength = 28 }
                },

                // Events
                Events = new List<EventDto>
                {
                    new EventDto { Id = "1", Title = "Math Exam", StartDate = DateTime.Parse("2025-06-23"), ClassName = "event-exam" },
                    new EventDto { Id = "2", Title = "Holiday", StartDate = DateTime.Parse("2025-06-25"), EndDate = DateTime.Parse("2025-06-27"), ClassName = "event-holiday" },
                    new EventDto { Id = "3", Title = "Staff Meeting", StartDate = DateTime.Parse("2025-06-20"), ClassName = "event-meeting" },
                    new EventDto { Id = "4", Title = "Sports Day", StartDate = DateTime.Parse("2025-06-28"), ClassName = "event-sports" },
                    new EventDto { Id = "5", Title = "Cultural Program", StartDate = DateTime.Parse("2025-06-30"), ClassName = "event-cultural" },
                    new EventDto { Id = "6", Title = "Teacher Workshop", StartDate = DateTime.Parse("2025-06-21"), ClassName = "event-workshop" }
                },

                // Income Breakdown
                IncomeBreakdown = new IncomeBreakdownDto
                {
                    Period = "June 2024",
                    Total = 279244,
                    Items = new List<IncomeItemDto>
                    {
                        new IncomeItemDto { Category = "Uniform Sale", Amount = 59234, Color = "#8D7B68" },
                        new IncomeItemDto { Category = "Book Sale", Amount = 40210, Color = "#C084FC" },
                        new IncomeItemDto { Category = "Miscellaneous", Amount = 35800, Color = "#4DD0E1" },
                        new IncomeItemDto { Category = "Rent", Amount = 49000, Color = "#FFD54F" },
                        new IncomeItemDto { Category = "Donation", Amount = 30000, Color = "#66BB6A" },
                        new IncomeItemDto { Category = "Other", Amount = 35000, Color = "#FF8A65" }
                    }
                },

                // Expense Breakdown
                ExpenseBreakdown = new ExpenseBreakdownDto
                {
                    Period = "June 2024",
                    Total = 120000,
                    Items = new List<ExpenseItemDto>
                    {
                        new ExpenseItemDto { Category = "Telephone Bill", Amount = 20000, Color = "#B39DDB" },
                        new ExpenseItemDto { Category = "Flower", Amount = 15000, Color = "#FFE082" },
                        new ExpenseItemDto { Category = "Electricity Bill", Amount = 30000, Color = "#8D7B68" },
                        new ExpenseItemDto { Category = "Stationary", Amount = 25000, Color = "#66BB6A" },
                        new ExpenseItemDto { Category = "Miscellaneous", Amount = 12000, Color = "#FF8A65" },
                        new ExpenseItemDto { Category = "Other", Amount = 18000, Color = "#90CAF9" }
                    }
                },

                // Staff Birthdays
                StaffBirthdays = new List<StaffBirthdayViewModel>
                {
                    new StaffBirthdayViewModel
                    {
                        Name = "Neelam Devi",
                        PhotoUrl = "/images/staff/neelam.png",
                        Role = "TEACHER",
                        RoleColor = "primary",
                        Birthday = new DateTime(2025, 5, 12),
                        RemainingDays = 20,
                        IsActive = true
                    },
                    new StaffBirthdayViewModel
                    {
                        Name = "Vikash Kumar Sharma",
                        PhotoUrl = "/images/staff/vikash.png",
                        Role = "ACCOUNTANT",
                        RoleColor = "success",
                        Birthday = new DateTime(2025, 10, 18),
                        RemainingDays = 15,
                        IsActive = true
                    }
                },

                // Birthday List
          
                // Legacy Stats (kept for compatibility)
                Stats = new DashboardStatsDto
                {
                    Students = new StudentStatsDto
                    {
                        Total = 3654,
                        Active = 3643,
                        Inactive = 11
                    },
                    Teachers = new TeacherStatsDto
                    {
                        Total = 284,
                        Active = 254,
                        Inactive = 30
                    },
                    Staff = new StaffStatsDto
                    {
                        Total = 162,
                        Active = 161,
                        Inactive = 2
                    },
                    Subjects = new SubjectStatsDto
                    {
                        Total = 82,
                        Active = 81,
                        Inactive = 1
                    }
                }
            };
        }

        private static List<ClasswiseStudentDto> GenerateClasswiseStudents()
        {
            var classes = new List<string>
            {
                "Nursery", "LKG", "UKG", "Class 1", "Class 2", "Class 3",
                "Class 4", "Class 5", "Class 6", "Class 7", "Class 8",
                "Class 9", "Class 10", "Class 11", "Class 12"
            };

            var students = new List<int> { 30, 28, 32, 35, 29, 30, 27, 26, 25, 24, 26, 22, 20, 19, 21 };

            var result = new List<ClasswiseStudentDto>();
            var random = new Random();

            for (int i = 0; i < classes.Count; i++)
            {
                result.Add(new ClasswiseStudentDto
                {
                    ClassName = classes[i],
                    StudentCount = students[i],
                    Color = $"#{random.Next(0x1000000):X6}" // Random hex color
                });
            }

            return result;
        }

        public static HeaderInfoDto LoadHeaderInfo()
        {
            return new HeaderInfoDto
            {
                InstitutionName = "Dr. Rammanohar Lohia Avadh University, Kishni",
                Address = "Near SBI Bank of India Bypass Road, Kishni, M",
                SmsBalance = "0",
                ValidTillDate = DateTime.Parse("2028-03-31"),
                SessionInfo = "2025-26"
            };
        }
    }
        // Main Dashboard Model
        public class DashboardDto
    {
        public UserInfoDto UserInfo { get; set; }
        public DashboardStatsDto Stats { get; set; }
        public TodayCollectionDto TodayCollection { get; set; }
        public FeeSummaryDto FeeSummary { get; set; }
        public StudentSummaryDto StudentSummary { get; set; }
        public AttendanceSummaryDto AttendanceSummary { get; set; }
        public EnquirySummaryDto EnquirySummary { get; set; }
        public StaffSummaryDto StaffSummary { get; set; }
        public PaymentStatusDto PaymentStatus { get; set; }
        public List<ClasswiseCollectionDto> ClasswiseCollection { get; set; }
        public List<ClasswiseStudentDto> ClasswiseStudents { get; set; }
        public TeacherStaffAttendanceDto TeacherStaffAttendance { get; set; }
        public List<StaffBirthdayViewModel> StaffBirthdays { get; set; }
        public EstimatedFeeCollectionDto EstimatedFeeCollection { get; set; }
        public FinancialTrendsDto FinancialTrends { get; set; }
        public IncomeExpenseChartDto IncomeExpenseChart { get; set; }
        public QuickDetailsDto QuickDetails { get; set; }
        public List<ClasswiseAttendanceDto> ClasswiseAttendance { get; set; }
        public List<EventDto> Events { get; set; }
        public IncomeBreakdownDto IncomeBreakdown { get; set; }
        public ExpenseBreakdownDto ExpenseBreakdown { get; set; }
        public List<BirthdayStudent> BirthdayList { get; set; }

        // Legacy properties (kept for compatibility)
        public FeesCollectionDto FeesCollection { get; set; }
        public List<LeaveRequestDto> LeaveRequests { get; set; }
        public List<AttendanceDto> Attendance { get; set; }
        public MembershipPlansDto MembershipPlans { get; set; }
        public FinanceAccountsDto FinanceAccounts { get; set; }
        public FinancialSummaryDto FinancialSummary { get; set; }
        public List<NoticeDto> NoticeBoard { get; set; }

        public DashboardDto()
        {
            StaffBirthdays = new List<StaffBirthdayViewModel>();
            BirthdayList = new List<BirthdayStudent>();
            ClasswiseCollection = new List<ClasswiseCollectionDto>();
            ClasswiseStudents = new List<ClasswiseStudentDto>();
            ClasswiseAttendance = new List<ClasswiseAttendanceDto>();
            Events = new List<EventDto>();
        }
    }

    // New DTOs for dashboard cards
    public class TodayCollectionDto
    {
        public decimal TotalCollection { get; set; }
        public decimal CashCollection { get; set; }
        public decimal BankCollection { get; set; }
    }

    public class FeeSummaryDto
    {
        public decimal TotalFees { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal TotalDues { get; set; }
    }

    public class StudentSummaryDto
    {
        public int TotalStudents { get; set; }
        public int TotalMale { get; set; }
        public int TotalFemale { get; set; }
        public int NewStudents { get; set; }
        public int OldStudents { get; set; }
        public int ActiveStudents { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int TodayPresent { get; set; }
        public int TodayAbsent { get; set; }
        public int TodayLeave { get; set; }
    }

    public class EnquirySummaryDto
    {
        public int TodayEnquiry { get; set; }
        public int TotalEnquiry { get; set; }
        public int PendingEnquiry { get; set; }
    }

    public class StaffSummaryDto
    {
        public int TotalStaff { get; set; }
        public int Teachers { get; set; }
        public int AdminStaff { get; set; }
    }

    public class PaymentStatusDto
    {
        public int NotPaid { get; set; }
        public int Overdue { get; set; }
        public int ReminderSent { get; set; }
    }

    // Classwise data
    public class ClasswiseCollectionDto
    {
        public string ClassName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
    }

    public class ClasswiseStudentDto
    {
        public string ClassName { get; set; }
        public int StudentCount { get; set; }
        public string Color { get; set; } // For chart display
    }

    // Teacher/Staff Attendance Today
    public class TeacherStaffAttendanceDto
    {
        public int TeachersPresentToday { get; set; }
        public int TeachersAbsentToday { get; set; }
        public int StaffPresentToday { get; set; }
        public int StaffAbsentToday { get; set; }
    }

    // Estimated Fee Collection
    public class EstimatedFeeCollectionDto
    {
        public List<MonthlyEstimatedFeeDto> MonthlyData { get; set; }

        public EstimatedFeeCollectionDto()
        {
            MonthlyData = new List<MonthlyEstimatedFeeDto>();
        }
    }

    public class MonthlyEstimatedFeeDto
    {
        public string Month { get; set; }
        public decimal Expected { get; set; }
        public decimal Received { get; set; }
        public decimal Due { get; set; }
    }

    // Financial Trends
    public class FinancialTrendsDto
    {
        public TrendDataDto Earnings { get; set; }
        public TrendDataDto Expenses { get; set; }
        public TrendDataDto Profit { get; set; }
    }

    public class TrendDataDto
    {
        public decimal Total { get; set; }
        public List<MonthlyTrendDto> MonthlyData { get; set; }

        public TrendDataDto()
        {
            MonthlyData = new List<MonthlyTrendDto>();
        }
    }

    public class MonthlyTrendDto
    {
        public string Month { get; set; }
        public decimal Amount { get; set; }
    }

    // Income Expense Chart
    public class IncomeExpenseChartDto
    {
        public List<MonthlyIncomeExpenseDto> MonthlyData { get; set; }

        public IncomeExpenseChartDto()
        {
            MonthlyData = new List<MonthlyIncomeExpenseDto>();
        }
    }

    public class MonthlyIncomeExpenseDto
    {
        public string Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }

    // Quick Details
    public class QuickDetailsDto
    {
        public UserSummaryDto UserSummary { get; set; }
        public ReceiptSummaryDto ReceiptSummary { get; set; }
        public DeleteReceiptSummaryDto DeleteReceiptSummary { get; set; }
        public SelfStudentsSummaryDto SelfStudentsSummary { get; set; }


    }

    public class UserSummaryDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
    }

    public class ReceiptSummaryDto
    {
        public int TodayReceipts { get; set; }
        public int TotalReceipts { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class DeleteReceiptSummaryDto
    {
        public int TodayDeleted { get; set; }
        public int TotalDeleted { get; set; }
    }

    // Classwise Attendance
    public class ClasswiseAttendanceDto
    {
        public string ClassName { get; set; }
        public int TotalPresent { get; set; }
        public int TotalStrength { get; set; }
    }

    // Income/Expense Breakdown (for donut charts)
    public class IncomeBreakdownDto
    {
        public string Period { get; set; } // e.g., "June 2024"
        public decimal Total { get; set; }
        public List<IncomeItemDto> Items { get; set; }

        public IncomeBreakdownDto()
        {
            Items = new List<IncomeItemDto>();
        }
    }

    public class IncomeItemDto
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Color { get; set; }
    }

    public class ExpenseBreakdownDto
    {
        public string Period { get; set; } // e.g., "June 2024"
        public decimal Total { get; set; }
        public List<ExpenseItemDto> Items { get; set; }

        public ExpenseBreakdownDto()
        {
            Items = new List<ExpenseItemDto>();
        }
    }

    public class ExpenseItemDto
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Color { get; set; }
    }

    // Event DTO (updated)
    public class EventDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public string EventType { get; set; }
        public string ClassName { get; set; } // CSS class for styling
    }

    // Birthday Student
    public class BirthdayStudent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RollNo { get; set; }
        public DateTime Birthday { get; set; }
        public int DaysRemaining { get; set; }
        public string Gender { get; set; } // "Male" or "Female"
       public string ImageUrl { get; set; } = @"/template/assets/img/noimgstu.png";

    }

    // Staff Birthday
    public class StaffBirthdayViewModel
    {
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public string Role { get; set; } // e.g., "TEACHER", "ACCOUNTANT"
        public string RoleColor { get; set; } // e.g., "danger", "warning"
        public DateTime Birthday { get; set; }
        public int RemainingDays { get; set; }
        public bool IsActive { get; set; }
    }

    // User Information
    public class UserInfoDto
    {
        public string Name { get; set; }
        public string WelcomeMessage { get; set; }
        public DateTime LastUpdated { get; set; }
        public string ProfileImageUrl { get; set; }
    }

    // Dashboard Statistics (legacy structure maintained)
    public class DashboardStatsDto
    {
        public StudentStatsDto Students { get; set; }
        public TeacherStatsDto Teachers { get; set; }
        public StaffStatsDto Staff { get; set; }
        public SubjectStatsDto Subjects { get; set; }
    }

    public class StudentStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
    }

    public class TeacherStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
    }

    public class StaffStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
    }

    public class SubjectStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
    }

    // Legacy DTOs (kept for compatibility)
    public class FeesCollectionDto
    {
        public string Period { get; set; }
        public List<QuarterlyFeesDto> QuarterlyData { get; set; }
    }

    public class QuarterlyFeesDto
    {
        public string Quarter { get; set; }
        public decimal CollectedFee { get; set; }
        public decimal TotalFee { get; set; }
        public decimal PercentageCollected => TotalFee > 0 ? (CollectedFee / TotalFee) * 100 : 0;
    }

    public class LeaveRequestDto
    {
        public string EmployeeName { get; set; }
        public string EmployeeType { get; set; }
        public string Position { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }
        public DateTime ApplyDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
    }

    public class AttendanceDto
    {
        public DateTime Date { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public decimal AttendancePercentage => (Present + Absent + Late) > 0
            ? ((decimal)Present / (Present + Absent + Late)) * 100 : 0;
    }

    public class MembershipPlansDto
    {
        public int TotalPlans { get; set; }
        public int ActivePlans { get; set; }
        public int InactivePlans { get; set; }
    }

    public class FinanceAccountsDto
    {
        public int TotalAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int InactiveAccounts { get; set; }
    }

    public class FinancialSummaryDto
    {
        public EarningsDto Earnings { get; set; }
        public ExpensesDto Expenses { get; set; }
        public CollectionSummaryDto CollectionSummary { get; set; }
    }

    public class EarningsDto
    {
        public decimal TotalEarnings { get; set; }
        public List<MonthlyDataDto> MonthlyData { get; set; }
    }

    public class ExpensesDto
    {
        public decimal TotalExpenses { get; set; }
        public List<MonthlyDataDto> MonthlyData { get; set; }
    }

    public class MonthlyDataDto
    {
        public DateTime Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class CollectionSummaryDto
    {
        public decimal TotalFeesCollected { get; set; }
        public decimal PercentageChange { get; set; }
        public bool IsIncrease { get; set; }
        public decimal FineCollectedTillDate { get; set; }
        public decimal FinePercentageChange { get; set; }
        public bool IsFineIncrease { get; set; }
        public decimal StudentNotPaid { get; set; }
        public decimal StudentNotPaidPercentageChange { get; set; }
        public bool IsStudentNotPaidIncrease { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal OutstandingPercentageChange { get; set; }
        public bool IsOutstandingIncrease { get; set; }
    }

    public class NoticeDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime AddedOn { get; set; }
        public int DaysAgo { get; set; }
        public NoticeType Type { get; set; }
        public string IconClass { get; set; }
    }

    public enum NoticeType
    {
        Instruction,
        Event,
        Notification,
        Preparation,
        Schedule
    }

    public class HeaderInfoDto
    {
        public string InstitutionName { get; set; }
        public string Address { get; set; }
        public string SmsBalance { get; set; }
        public DateTime ValidTillDate { get; set; }
        public string SessionInfo { get; set; }
    }
    public class SelfStudentsSummaryDto
    {
        public int TotalSelfStudents { get; set; }
        public int TransportStudents { get; set; }
    }
}