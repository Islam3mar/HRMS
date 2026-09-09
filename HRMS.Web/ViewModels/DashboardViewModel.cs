using HRMS.Domain.Entities;

namespace HRMS.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }

        public int PresentTodayCount { get; set; }
        public int AbsentTodayCount { get; set; }

        public bool IsTodayHoliday { get; set; }
        public string? TodayHolidayReason { get; set; }

        public List<OfficialHoliday> UpcomingHolidays { get; set; } = new();

        public bool IsGeneralSettingsConfigured { get; set; }
    }
}
