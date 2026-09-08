using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.ViewModels
{
    public class AttendanceIndexViewModel
    {
        public IEnumerable<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public AttendanceSearchViewModel Search { get; set; } = new();
        public AttendanceFormViewModel Form { get; set; } = new();

        public List<SelectListItem> EmployeeOptions { get; set; } = new();
    }
}
