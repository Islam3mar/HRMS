using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;

namespace HRMS.Domain.Entities
{
    public class GeneralSettings : BaseEntity
    {
        public decimal AdditionRatePerHour { get; set; }   // الاضافة
        public decimal DeductionRatePerHour { get; set; }  // الخصم

        public DayOfWeek WeeklyHoliday1 { get; set; }       // يوم الاجازة الرسمى 1
        public DayOfWeek WeeklyHoliday2 { get; set; }       // يوم الاجازة الرسمى 2
    }
}
