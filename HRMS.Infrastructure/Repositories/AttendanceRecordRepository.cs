using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Specifications.Attendance;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class AttendanceRecordRepository : GenericRepository<AttendanceRecord>, IAttendanceRecordRepository
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

        public async Task<PagedResult<EmployeeAttendanceGroup>> SearchGroupedByEmployeeAsync(AttendanceSearchFilter filter)
        {
            // نفس منطق SearchAllAsync بالظبط (كل السجلات المطابقة للفلتر، من غير Pagination على مستوى السجل)
            var allMatchingRecords = await BuildFilteredQuery(filter)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            // تجميع فى الذاكرة حسب الموظف، وترتيب المجموعات بالاسم
            var groupedAll = allMatchingRecords
                .GroupBy(a => a.EmployeeId)
                .Select(g => new EmployeeAttendanceGroup
                {
                    EmployeeId = g.Key,
                    EmployeeName = g.First().Employee.FullName,
                    DepartmentName = g.First().Employee.Department?.Name ?? "-",
                    RecordsCount = g.Count(),
                    Records = g.ToList()
                })
                .OrderBy(g => g.EmployeeName)
                .ToList();

            var totalEmployees = groupedAll.Count;

            // دلوقتي الـ Skip/Take بتاع الـ Pagination بيحصل على عدد الموظفين مش عدد السجلات
            var pagedGroups = groupedAll
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return new PagedResult<EmployeeAttendanceGroup>
            {
                Items = pagedGroups,
                TotalCount = totalEmployees,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }


        public async Task<bool> RecordExistsAsync(int employeeId, DateTime date, int? excludeId = null)
        {
            return await Query.AnyAsync(a => a.EmployeeId == employeeId &&
                                              a.Date.Date == date.Date &&
                                              (excludeId == null || a.Id != excludeId));
        }

        // ---------- Helper ----------
        // بدل ما كان بيبني الـ Where يدويًا، دلوقتي بيستخدم AttendanceSearchSpecification
        // (نفس الشرط بالظبط، بس دلوقتي في كلاس قابل لإعادة الاستخدام والاختبار لوحده)
        private IQueryable<AttendanceRecord> BuildFilteredQuery(AttendanceSearchFilter filter)
        {
            var spec = new AttendanceSearchSpecification(filter);
            return SpecificationEvaluator<AttendanceRecord>.GetQuery(Query.AsNoTracking(), spec);
        }
    }
}
