using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    public class GeneralSettingsInput
    {
        public decimal AdditionRatePercentage { get; set; }
        public decimal DeductionRatePercentage { get; set; }
        public DayOfWeek WeeklyHoliday1 { get; set; }
        public DayOfWeek WeeklyHoliday2 { get; set; }
    }
}
