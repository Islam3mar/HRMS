using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Application.DTOs
{
    public class EmployeeResult
    {
        public bool Success { get; set; }
        public Employee? Employee { get; set; }

        public string? FullNameError { get; set; }
        public string? AddressError { get; set; }
        public string? PhoneNumberError { get; set; }
        public string? GenderError { get; set; }
        public string? NationalityError { get; set; }
        public string? BirthDateError { get; set; }
        public string? NationalIdError { get; set; }
        public string? ContractDateError { get; set; }
        public string? SalaryError { get; set; }
        public string? AttendanceTimeError { get; set; }
        public string? DepartureTimeError { get; set; }

        public bool HasErrors =>
            FullNameError != null || AddressError != null || PhoneNumberError != null ||
            GenderError != null || NationalityError != null || BirthDateError != null ||
            NationalIdError != null || ContractDateError != null || SalaryError != null ||
            AttendanceTimeError != null || DepartureTimeError != null;
    }
}
