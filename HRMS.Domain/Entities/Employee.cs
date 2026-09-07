using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities
{
    public class Employee : BaseEntity
    {
        // البيانات الشخصية
        public string FullName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public Gender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string NationalId { get; set; } = null!;
        public string Nationality { get; set; } = null!;

        // بيانات تخص العمل
        public DateTime ContractDate { get; set; }
        public decimal Salary { get; set; }
        public TimeSpan AttendanceTime { get; set; }   // موعد الحضور
        public TimeSpan DepartureTime { get; set; }    // موعد الانصراف

        public int? DepartmentId { get; set; }         // مطلوبة ضمنياً من شاشة تقرير الحضور (فيها عمود "القسم")
        public Department? Department { get; set; }
    }
}
