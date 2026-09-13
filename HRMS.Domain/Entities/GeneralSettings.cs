using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;

namespace HRMS.Domain.Entities
{
    public class GeneralSettings : BaseEntity
    {
        public decimal AdditionRatePercentage { get; set; }    // الاضافة كنسبة %
        public decimal DeductionRatePercentage { get; set; }   // الخصم كنسبة %

        public DayOfWeek WeeklyHoliday1 { get; set; }       // يوم الاجازة الرسمى 1
        public DayOfWeek WeeklyHoliday2 { get; set; }       // يوم الاجازة الرسمى 2
    }
}
