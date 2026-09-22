using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Specifications.Employees;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<Employee?> GetByIdAsync(int id)
        {
            return await Query
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public override async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await Query
                .Include(e => e.Department)
                .AsNoTracking()
                .ToListAsync();
        }

        // بترجع الموظفين النشطين بس - مستخدمة في Dropdown الحضور ولوحة التحكم
        public async Task<IEnumerable<Employee>> GetActiveAsync()
        {
            return await Query
                .Where(e => e.IsActive)
                .Include(e => e.Department)
                .AsNoTracking()
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> SearchByNameAsync(string name)
        {
            var spec = new EmployeesByNameSpecification(name);
            return await SpecificationEvaluator<Employee>.GetQuery(Query.AsNoTracking(), spec).ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
        {
            var spec = new EmployeesByDepartmentSpecification(departmentId);
            return await SpecificationEvaluator<Employee>.GetQuery(Query.AsNoTracking(), spec).ToListAsync();
        }

        public async Task<bool> NationalIdExistsAsync(string nationalId, int? excludeEmployeeId = null)
        {
            return await Query
                .AnyAsync(e => e.NationalId == nationalId &&
                               (excludeEmployeeId == null || e.Id != excludeEmployeeId));
        }

        public async Task<EmployeeSchedule?> GetScheduleAsync(int employeeId)
        {
            return await Query
                .AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => new EmployeeSchedule(e.AttendanceTime, e.DepartureTime, e.Salary, e.ContractDate))
                .FirstOrDefaultAsync();
        }

        public async Task<bool> FullNameExistsAsync(string fullName, int? excludeEmployeeId = null)
        {
            return await Query
                .AnyAsync(e => e.IsActive && e.FullName == fullName &&
                               (excludeEmployeeId == null || e.Id != excludeEmployeeId));
        }

        public async Task<Employee?> GetInactiveByNationalIdAsync(string nationalId)
        {
            return await Query.FirstOrDefaultAsync(e => e.NationalId == nationalId && !e.IsActive);
        }
    }
}
