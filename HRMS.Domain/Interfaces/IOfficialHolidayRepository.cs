using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IOfficialHolidayRepository : IGenericRepository<OfficialHoliday>
    {
        Task<IEnumerable<OfficialHoliday>> GetAllOrderedByDateAsync();
        Task<bool> DateExistsAsync(DateTime date, int? excludeId = null);
    }
}
