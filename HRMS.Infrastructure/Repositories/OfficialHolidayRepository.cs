using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class OfficialHolidayRepository : GenericRepository<OfficialHoliday>, IOfficialHolidayRepository
    {
        public OfficialHolidayRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<OfficialHoliday>> GetAllOrderedByDateAsync()
            => await Query.AsNoTracking().OrderBy(h => h.Date).ToListAsync();

        public async Task<bool> DateExistsAsync(DateTime date, int? excludeId = null)
            => await Query.AnyAsync(h => h.Date.Date == date.Date &&
                                          (excludeId == null || h.Id != excludeId));
    }
}
