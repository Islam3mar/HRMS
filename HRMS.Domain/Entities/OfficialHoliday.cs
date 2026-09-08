using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;

namespace HRMS.Domain.Entities
{
    public class OfficialHoliday : BaseEntity
    {
        public string Name { get; set; } = null!;
        public DateTime Date { get; set; }
    }
}
