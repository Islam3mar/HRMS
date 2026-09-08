using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs
{
    public class EmployeeInput
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string NationalId { get; set; } = string.Empty;

        public DateTime ContractDate { get; set; }
        public decimal Salary { get; set; }
        public TimeSpan AttendanceTime { get; set; }
        public TimeSpan DepartureTime { get; set; }

        public int? DepartmentId { get; set; }
    }
}
