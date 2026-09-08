using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Enums
{
    public static class DayOfWeekExtensions
    {
        public static string ToArabicName(this DayOfWeek day) => day switch
        {
            DayOfWeek.Saturday => "السبت",
            DayOfWeek.Sunday => "الاحد",
            DayOfWeek.Monday => "الاثنين",
            DayOfWeek.Tuesday => "الثلاثاء",
            DayOfWeek.Wednesday => "الاربعاء",
            DayOfWeek.Thursday => "الخميس",
            DayOfWeek.Friday => "الجمعة",
            _ => day.ToString()
        };
    }
}
