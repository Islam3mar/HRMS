using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    public class OfficialHolidayInput
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
