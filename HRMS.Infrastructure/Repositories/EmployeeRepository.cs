using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Specifications.Employees;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
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
    }
}
