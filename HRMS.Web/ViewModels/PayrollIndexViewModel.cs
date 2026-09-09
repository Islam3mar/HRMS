using HRMS.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.ViewModels
{
    public class PayrollIndexViewModel
    {
        public List<PayrollRowDto> Rows { get; set; } = new();

        public PayrollSearchViewModel Search { get; set; } = new();

        public List<SelectListItem> MonthOptions { get; set; } = new();
        public List<SelectListItem> YearOptions { get; set; } = new();

        public string? SearchError { get; set; }
        public string? YearError { get; set; }
    }
}
