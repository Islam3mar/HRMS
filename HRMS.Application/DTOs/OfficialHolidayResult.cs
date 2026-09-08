using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Application.DTOs
{
    public class OfficialHolidayResult
    {
        public bool Success { get; set; }
        public OfficialHoliday? Holiday { get; set; }

        public string? NameError { get; set; }
        public string? DateError { get; set; }

        public bool HasErrors => NameError != null || DateError != null;
    }
}
