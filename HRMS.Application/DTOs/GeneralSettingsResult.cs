using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Application.DTOs
{
    public class GeneralSettingsResult
    {
        public bool Success { get; set; }
        public GeneralSettings? Settings { get; set; }

        public string? AdditionRateError { get; set; }
        public string? DeductionRateError { get; set; }
        public string? WeeklyHoliday1Error { get; set; }
        public string? WeeklyHoliday2Error { get; set; }

        public bool HasErrors =>
            AdditionRateError != null || DeductionRateError != null ||
            WeeklyHoliday1Error != null || WeeklyHoliday2Error != null;
    }
}
