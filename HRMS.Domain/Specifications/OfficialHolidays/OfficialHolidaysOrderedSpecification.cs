using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Specifications.OfficialHolidays
{
    public class OfficialHolidaysOrderedSpecification : BaseSpecification<OfficialHoliday>
    {
        public OfficialHolidaysOrderedSpecification()
        {
            ApplyOrderBy(h => h.Date);
        }
    }
}
