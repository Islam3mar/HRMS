using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class AttendanceRecordRepository : BaseRepository<AttendanceRecord>, IAttendanceRecordRepository
    {
        public AttendanceRecordRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<AttendanceRecord?> GetByIdAsync(int id)
        {
            return await Query
                .Include(a => a.Employee).ThenInclude(e => e.Department)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AttendanceRecord?> GetByIdWithDetailsAsync(int id) => await GetByIdAsync(id);

        public async Task<PagedResult<AttendanceRecord>> SearchAsync(AttendanceSearchFilter filter)
        {
            var query = BuildFilteredQuery(filter);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<AttendanceRecord>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<IEnumerable<AttendanceRecord>> SearchAllAsync(AttendanceSearchFilter filter)
        {
            return await BuildFilteredQuery(filter)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<bool> RecordExistsAsync(int employeeId, DateTime date, int? excludeId = null)
        {
            return await Query.AnyAsync(a => a.EmployeeId == employeeId &&
                                              a.Date.Date == date.Date &&
                                              (excludeId == null || a.Id != excludeId));
        }

        // ---------- Helper ----------
        private IQueryable<AttendanceRecord> BuildFilteredQuery(AttendanceSearchFilter filter)
        {
            var query = Query
                .Include(a => a.Employee).ThenInclude(e => e.Department)
                .AsNoTracking()
                .AsQueryable();

            // مربع بحث واحد بيدور باسم الموظف او باسم القسم (زي التصميم)
            if (!string.IsNullOrWhiteSpace(filter.EmployeeName))
                query = query.Where(a => a.Employee.FullName.Contains(filter.EmployeeName) ||
                                          (a.Employee.Department != null && a.Employee.Department.Name.Contains(filter.EmployeeName)));

            if (filter.DepartmentId.HasValue)
                query = query.Where(a => a.Employee.DepartmentId == filter.DepartmentId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(a => a.Date.Date >= filter.FromDate.Value.Date);

            if (filter.ToDate.HasValue)
                query = query.Where(a => a.Date.Date <= filter.ToDate.Value.Date);

            return query;
        }
    }
}
