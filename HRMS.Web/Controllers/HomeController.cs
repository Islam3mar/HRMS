using System.Diagnostics;
using HRMS.Application.Interfaces;
using HRMS.Domain.Common;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IAttendanceRecordService _attendanceService;
        private readonly IOfficialHolidayService _holidayService;
        private readonly IGeneralSettingsService _settingsService;

        public HomeController(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IAttendanceRecordService attendanceService,
            IOfficialHolidayService holidayService,
            IGeneralSettingsService settingsService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _attendanceService = attendanceService;
            _holidayService = holidayService;
            _settingsService = settingsService;
        }

        public async Task<IActionResult> Index()
        {
            var employees = (await _employeeService.GetAllEmployeesAsync()).ToList();
            var departments = (await _departmentService.GetAllAsync()).ToList();
            var settings = await _settingsService.GetSettingsAsync();
            var holidays = (await _holidayService.GetAllAsync()).ToList();

            var today = DateTime.Today;

            var isWeeklyHoliday = settings != null &&
                (today.DayOfWeek == settings.WeeklyHoliday1 || today.DayOfWeek == settings.WeeklyHoliday2);

            var todayOfficialHoliday = holidays.FirstOrDefault(h => h.Date.Date == today);

            var todayFilter = new AttendanceSearchFilter
            {
                FromDate = today,
                ToDate = today,
                Page = 1,
                PageSize = int.MaxValue
            };
            var todayRecords = await _attendanceService.SearchAllAsync(todayFilter);
            var presentTodayCount = todayRecords.Select(r => r.EmployeeId).Distinct().Count();

            var model = new DashboardViewModel
            {
                TotalEmployees = employees.Count,
                TotalDepartments = departments.Count,
                PresentTodayCount = presentTodayCount,
                AbsentTodayCount = Math.Max(0, employees.Count - presentTodayCount),
                IsTodayHoliday = isWeeklyHoliday || todayOfficialHoliday != null,
                TodayHolidayReason = todayOfficialHoliday?.Name ?? (isWeeklyHoliday ? "اجازة اسبوعية" : null),
                UpcomingHolidays = holidays.Where(h => h.Date.Date >= today).OrderBy(h => h.Date).Take(3).ToList(),
                IsGeneralSettingsConfigured = settings != null
            };

            return View(model);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
