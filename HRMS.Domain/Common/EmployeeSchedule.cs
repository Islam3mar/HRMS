using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Common
{
    // نتيجة خفيفة (مش Entity) لجلب بيانات مواعيد الموظف بس، من غير ما نـ track الـ Employee نفسه
    public record EmployeeSchedule(TimeSpan AttendanceTime, TimeSpan DepartureTime, decimal Salary, DateTime ContractDate);
}
