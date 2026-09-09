using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    // فلتر شاشة تقرير رواتب الموظفين (المطلوب الثامن): بحث باسم الموظف + الشهر و السنة
    public class PayrollSearchFilter
    {
        public string? EmployeeName { get; set; }
        public int Month { get; set; } = DateTime.Today.Month;
        public int Year { get; set; } = DateTime.Today.Year;
    }
}
