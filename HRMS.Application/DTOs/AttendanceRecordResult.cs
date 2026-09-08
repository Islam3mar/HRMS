using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Application.DTOs
{
    public class AttendanceRecordResult
    {
        public bool Success { get; set; }
        public AttendanceRecord? Record { get; set; }

        public string? EmployeeIdError { get; set; }
        public string? DateError { get; set; }
        public string? CheckInTimeError { get; set; }
        public string? CheckOutTimeError { get; set; }

        public bool HasErrors =>
            EmployeeIdError != null || DateError != null ||
            CheckInTimeError != null || CheckOutTimeError != null;
    }
}
