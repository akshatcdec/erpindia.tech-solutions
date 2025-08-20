using Dapper;
using DashboardModels;
using ERPIndia.Dashboard.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public interface IDashboardRepository
    {
        Task<DashboardDto> GetDashboardDataAsync(int TenantCode);
    }

    public class DashboardRepository : IDashboardRepository
    {
        private readonly string _connectionString;

        public DashboardRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(int TenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var dashboard = new DashboardDto();
                var parameters = new DynamicParameters();
                parameters.Add("@TenantCode", TenantCode);
                using (var multi = await connection.QueryMultipleAsync("sp_GetDashboardData", parameters,
                    commandType: CommandType.StoredProcedure))
                {

                    // Result Set 1: Today's Collection
                    var todayCollection = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    dashboard.TodayCollection = new TodayCollectionDto
                    {
                        TotalCollection = todayCollection?.TotalCollection ?? 0,
                        CashCollection = todayCollection?.CashCollection ?? 0,
                        BankCollection = todayCollection?.BankCollection ?? 0
                    };

                    // Result Set 2: Fee Summary
                    var feeSummary = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    dashboard.FeeSummary = new FeeSummaryDto
                    {
                        TotalFees = feeSummary?.TotalFees ?? 0,
                        TotalReceived = feeSummary?.TotalReceived ?? 0,
                        TotalDues = feeSummary?.TotalDues ?? 0
                    };

                    // Result Set 3: Student Summary
                    var studentSummary = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    dashboard.StudentSummary = new StudentSummaryDto
                    {
                        TotalStudents = studentSummary?.TotalStudents ?? 0,
                        TotalMale = studentSummary?.TotalMale ?? 0,
                        TotalFemale = studentSummary?.TotalFemale ?? 0,
                        NewStudents = studentSummary?.NewStudents ?? 0,
                        OldStudents = studentSummary?.OldStudents ?? 0,
                        ActiveStudents = studentSummary?.ActiveStudents ?? 0
                    };

                    // Result Set 4: Attendance Summary
                    var attendanceSummary = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    dashboard.AttendanceSummary = new AttendanceSummaryDto
                    {
                        TodayPresent = attendanceSummary?.TodayPresent ?? 0,
                        TodayAbsent = attendanceSummary?.TodayAbsent ?? 0,
                        TodayLeave = attendanceSummary?.TodayLeave ?? 0
                    };

                    // Result Set 5: Enquiry Summary
                    var enquirySummary = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    dashboard.EnquirySummary = new EnquirySummaryDto
                    {
                        TodayEnquiry = enquirySummary?.TodayEnquiry ?? 0,
                        TotalEnquiry = enquirySummary?.TotalEnquiry ?? 0,
                        PendingEnquiry = enquirySummary?.PendingEnquiry ?? 0
                    };

                    // Result Set 6: Staff Summary
                    var staffSummary = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    dashboard.StaffSummary = new StaffSummaryDto
                    {
                        TotalStaff = staffSummary?.TotalStaff ?? 0,
                        Teachers = staffSummary?.Teachers ?? 0,
                        AdminStaff = staffSummary?.AdminStaff ?? 0
                    };

                    // Result Set 7: Payment Status
                    var paymentStatus = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    dashboard.PaymentStatus = new PaymentStatusDto
                    {
                        NotPaid = paymentStatus?.NotPaid ?? 0,
                        Overdue = paymentStatus?.Overdue ?? 0,
                        ReminderSent = paymentStatus?.ReminderSent ?? 0
                    };

                    // Result Set 8: Teacher/Staff Attendance
                    var teacherStaffAttendance = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    dashboard.TeacherStaffAttendance = new TeacherStaffAttendanceDto
                    {
                        TeachersPresentToday = teacherStaffAttendance?.TeachersPresentToday ?? 0,
                        TeachersAbsentToday = teacherStaffAttendance?.TeachersAbsentToday ?? 0,
                        StaffPresentToday = teacherStaffAttendance?.StaffPresentToday ?? 0,
                        StaffAbsentToday = teacherStaffAttendance?.StaffAbsentToday ?? 0
                    };

                    // Result Set 9: Classwise Collection
                    dashboard.ClasswiseCollection = (await multi.ReadAsync<ClasswiseCollectionDto>()).ToList();

                    // Result Set 10: Classwise Students
                    dashboard.ClasswiseStudents = (await multi.ReadAsync<ClasswiseStudentDto>()).ToList();

                    // Result Set 11: Staff Birthdays
                    dashboard.StaffBirthdays = (await multi.ReadAsync<StaffBirthdayViewModel>()).ToList();

                    // Result Set 12: Estimated Fee Collection
                    var estimatedFeeData = (await multi.ReadAsync<MonthlyEstimatedFeeDto>()).ToList();
                    dashboard.EstimatedFeeCollection = new EstimatedFeeCollectionDto
                    {
                        MonthlyData = estimatedFeeData
                    };

                    // Result Set 13: Financial Trends - Earnings
                    var earningsData = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    if (earningsData != null)
                    {
                        dashboard.FinancialTrends = dashboard.FinancialTrends ?? new FinancialTrendsDto();
                        dashboard.FinancialTrends.Earnings = new TrendDataDto
                        {
                            Total = earningsData.Total ?? 0,
                            MonthlyData = string.IsNullOrEmpty(earningsData.MonthlyData?.ToString())
                                ? new List<MonthlyTrendDto>()
                                : JsonConvert.DeserializeObject<List<MonthlyTrendDto>>(earningsData.MonthlyData.ToString())
                        };
                    }

                    // Result Set 14: Financial Trends - Expenses
                    var expensesData = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    if (expensesData != null)
                    {
                        dashboard.FinancialTrends = dashboard.FinancialTrends ?? new FinancialTrendsDto();
                        dashboard.FinancialTrends.Expenses = new TrendDataDto
                        {
                            Total = expensesData.Total ?? 0,
                            MonthlyData = string.IsNullOrEmpty(expensesData.MonthlyData?.ToString())
                                ? new List<MonthlyTrendDto>()
                                : JsonConvert.DeserializeObject<List<MonthlyTrendDto>>(expensesData.MonthlyData.ToString())
                        };
                    }

                    // Calculate Profit from Earnings and Expenses
                    if (dashboard.FinancialTrends != null)
                    {
                        dashboard.FinancialTrends.Profit = CalculateProfit(
                            dashboard.FinancialTrends.Earnings,
                            dashboard.FinancialTrends.Expenses);
                    }

                    // Result Set 15: Income Expense Chart
                    var incomeExpenseData = (await multi.ReadAsync<MonthlyIncomeExpenseDto>()).ToList();
                    dashboard.IncomeExpenseChart = new IncomeExpenseChartDto
                    {
                        MonthlyData = incomeExpenseData
                    };

                    // Result Set 16: Quick Details
                    var quickDetails = await multi.ReadSingleOrDefaultAsync<dynamic>();
                    dashboard.QuickDetails = new QuickDetailsDto
                    {
                        UserSummary = new UserSummaryDto
                        {
                            TotalUsers = quickDetails?.TotalUsers ?? 0,
                            ActiveUsers = quickDetails?.ActiveUsers ?? 0,
                            InactiveUsers = quickDetails?.InactiveUsers ?? 0
                        },
                        ReceiptSummary = new ReceiptSummaryDto
                        {
                            TodayReceipts = quickDetails?.TodayReceipts ?? 0,
                            TotalReceipts = quickDetails?.TotalReceipts ?? 0,
                            TotalAmount = quickDetails?.TotalAmount ?? 0
                        },
                        DeleteReceiptSummary = new DeleteReceiptSummaryDto
                        {
                            TodayDeleted = quickDetails?.TodayDeleted ?? 0,
                            TotalDeleted = quickDetails?.TotalDeleted ?? 0
                        },
                        SelfStudentsSummary = new SelfStudentsSummaryDto
                        {
                            TotalSelfStudents = quickDetails?.TotalSelfStudents ?? 0,
                            TransportStudents = quickDetails?.TransportStudents ?? 0
                        }
                    };

                    // Result Set 17: Classwise Attendance
                    dashboard.ClasswiseAttendance = (await multi.ReadAsync<ClasswiseAttendanceDto>()).ToList();

                    // Result Set 18: Events
                    dashboard.Events = (await multi.ReadAsync<EventDto>()).ToList();

                    // Result Set 19: Income Breakdown
                    var incomeBreakdownData = (await multi.ReadAsync<dynamic>()).ToList();
                    if (incomeBreakdownData.Any())
                    {
                        dashboard.IncomeBreakdown = new IncomeBreakdownDto
                        {
                            Period = incomeBreakdownData.First().Period,
                            Total = incomeBreakdownData.Sum(x => (decimal)(x.Amount ?? 0)),
                            Items = incomeBreakdownData.Select(x => new IncomeItemDto
                            {
                                Category = x.Category,
                                Amount = x.Amount ?? 0,
                                Color = x.Color
                            }).ToList()
                        };
                    }
                    else
                    {
                        dashboard.IncomeBreakdown = new IncomeBreakdownDto();
                    }

                    // Result Set 20: Expense Breakdown
                    var expenseBreakdownData = (await multi.ReadAsync<dynamic>()).ToList();
                    if (expenseBreakdownData.Any())
                    {
                        dashboard.ExpenseBreakdown = new ExpenseBreakdownDto
                        {
                            Period = expenseBreakdownData.First().Period,
                            Total = expenseBreakdownData.Sum(x => (decimal)(x.Amount ?? 0)),
                            Items = expenseBreakdownData.Select(x => new ExpenseItemDto
                            {
                                Category = x.Category,
                                Amount = x.Amount ?? 0,
                                Color = x.Color
                            }).ToList()
                        };
                    }
                    else
                    {
                        dashboard.ExpenseBreakdown = new ExpenseBreakdownDto();
                    }
                    // Result Set 21: Student Birthdays
                    dashboard.BirthdayList = (await multi.ReadAsync<BirthdayStudent>()).ToList();

                    // Result Set 22: Header Information (if implementing header data from DB)
                    if (!multi.IsConsumed)
                    {
                        var headerInfo = await multi.ReadSingleOrDefaultAsync<HeaderInfoDto>();
                        if (headerInfo != null)
                        {
                            // Store header info in a static property or pass it separately
                            DashboardDataFactory.HeaderInfoFromDb = headerInfo;
                        }
                    }

                    // Result Set 23: Self Transport Students
                    if (!multi.IsConsumed)
                    {
                        var transportData = await multi.ReadSingleOrDefaultAsync<dynamic>();
                        if (transportData != null && dashboard.QuickDetails != null)
                        {
                            // Add self students data to quick details
                            dashboard.QuickDetails.SelfStudentsSummary = new SelfStudentsSummaryDto
                            {
                                TotalSelfStudents = transportData?.TotalSelfStudents ?? 69,
                                TransportStudents = transportData?.TransportStudents ?? 52
                            };
                        }
                    }

                    // Set default values for legacy properties
                    SetDefaultLegacyProperties(dashboard);
                }

                return dashboard;
            }
        }

        private TrendDataDto CalculateProfit(TrendDataDto earnings, TrendDataDto expenses)
        {
            var profit = new TrendDataDto
            {
                Total = earnings.Total - expenses.Total,
                MonthlyData = new List<MonthlyTrendDto>()
            };

            // Assuming both have the same months
            for (int i = 0; i < earnings.MonthlyData.Count; i++)
            {
                var earningMonth = earnings.MonthlyData[i];
                var expenseMonth = expenses.MonthlyData.FirstOrDefault(e => e.Month == earningMonth.Month);

                profit.MonthlyData.Add(new MonthlyTrendDto
                {
                    Month = earningMonth.Month,
                    Amount = earningMonth.Amount - (expenseMonth?.Amount ?? 0)
                });
            }

            return profit;
        }

        private void SetDefaultLegacyProperties(DashboardDto dashboard)
        {
            // Set user info
            dashboard.UserInfo = new UserInfoDto
            {
                Name = "Mr. Herald",
                WelcomeMessage = "Have a Good day at work",
                LastUpdated = DateTime.Now,
                ProfileImageUrl = "/assets/images/profile-default.png"
            };

            // Legacy Stats
            dashboard.Stats = new DashboardStatsDto
            {
                Students = new StudentStatsDto
                {
                    Total = dashboard.StudentSummary.TotalStudents,
                    Active = dashboard.StudentSummary.ActiveStudents,
                    Inactive = dashboard.StudentSummary.TotalStudents - dashboard.StudentSummary.ActiveStudents
                },
                Teachers = new TeacherStatsDto
                {
                    Total = dashboard.StaffSummary.Teachers,
                    Active = dashboard.StaffSummary.Teachers,
                    Inactive = 0
                },
                Staff = new StaffStatsDto
                {
                    Total = dashboard.StaffSummary.TotalStaff,
                    Active = dashboard.StaffSummary.TotalStaff,
                    Inactive = 0
                },
                Subjects = new SubjectStatsDto
                {
                    Total = 82,
                    Active = 81,
                    Inactive = 1
                }
            };

            // Initialize empty collections for legacy properties
            dashboard.LeaveRequests = new List<LeaveRequestDto>();
            dashboard.Attendance = new List<AttendanceDto>();
            dashboard.NoticeBoard = new List<NoticeDto>();
        }
    }
    public class AdminDashboardController : BaseController
    {
        private readonly IDashboardRepository _dashboardRepository;

        public AdminDashboardController()
        {
            _dashboardRepository=new DashboardRepository();
        }
        // GET: AdminDasboard
        public async Task<ActionResult> Index()
        {
            try
            {
                // Load dashboard data from database
                int code =Utils.ParseInt(CurrentTenantCode);
                var dashboardData = await _dashboardRepository.GetDashboardDataAsync(code);
                // Return the data to the view
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                // You can log the exception here if needed
                return Json(new { success = false, message = "Error loading statistics" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}