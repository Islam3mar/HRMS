using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class PayrollRecordRepository : GenericRepository<PayrollRecord>, IPayrollRecordRepository
    {
        public PayrollRecordRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PayrollRecord?> GetByEmployeeAndMonthAsync(int employeeId, int month, int year)
        {
            return await Query
                .Include(p => p.Employee).ThenInclude(e => e.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.Month == month && p.Year == year);
        }

        public async Task<IEnumerable<PayrollRecord>> GetByMonthAsync(int month, int year)
        {
            return await Query
                .Include(p => p.Employee).ThenInclude(e => e.Department)
                .AsNoTracking()
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();
        }
    }
}
