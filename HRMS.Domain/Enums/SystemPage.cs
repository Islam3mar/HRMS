using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Enums
{
    public enum SystemPage
    {
        Employees = 1,          // الموظفين
        GeneralSettings = 2,    // الاعدادات العامة
        AttendanceReport = 3,   // الحضور و الانصراف
        PayrollReport = 4,      // تقرير الرواتب
        UsersManagement = 5,  // مستخدمين النظام (لازم تتحط برضه عشان الـ HR يقدر يدير المستخدمين)
        OfficialHolidays = 6
    }
}
