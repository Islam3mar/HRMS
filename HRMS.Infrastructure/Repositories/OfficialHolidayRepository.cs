using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Specifications.OfficialHolidays;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class OfficialHolidayRepository : GenericRepository<OfficialHoliday>, IOfficialHolidayRepository
    {
        public OfficialHolidayRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<OfficialHoliday>> GetAllOrderedByDateAsync()
        {
            var spec = new OfficialHolidaysOrderedSpecification();
            return await SpecificationEvaluator<OfficialHoliday>.GetQuery(Query.AsNoTracking(), spec).ToListAsync();
        }

        public async Task<bool> DateExistsAsync(DateTime date, int? excludeId = null)
            => await Query.AnyAsync(h => h.Date.Date == date.Date &&
                                          (excludeId == null || h.Id != excludeId));
    }
}
