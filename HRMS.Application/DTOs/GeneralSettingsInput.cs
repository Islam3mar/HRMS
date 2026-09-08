using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    public class GeneralSettingsInput
    {
        public decimal AdditionRatePerHour { get; set; }
        public decimal DeductionRatePerHour { get; set; }
        public DayOfWeek WeeklyHoliday1 { get; set; }
        public DayOfWeek WeeklyHoliday2 { get; set; }
    }
}
