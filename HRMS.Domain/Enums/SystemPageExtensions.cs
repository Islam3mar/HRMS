using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Enums
{
    public static class SystemPageExtensions
    {
        public static string ToArabicName(this SystemPage page) => page switch
        {
            SystemPage.Employees => "الموظفين",
            SystemPage.GeneralSettings => "الاعدادات العامة",
            SystemPage.AttendanceReport => "الحضور و الانصراف",
            SystemPage.PayrollReport => "تقرير الرواتب",
            SystemPage.UsersManagement => "مستخدمين النظام",
            _ => page.ToString()
        };
    }
}
