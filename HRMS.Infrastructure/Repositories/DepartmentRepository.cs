using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await Query.AsNoTracking()
                               .OrderBy(d => d.Name)
                               .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeDepartmentId = null)
        {
            return await Query.AnyAsync(d => d.Name == name &&
                                              (excludeDepartmentId == null || d.Id != excludeDepartmentId));
        }

        public async Task<bool> HasEmployeesAsync(int departmentId)
        {
            return await _context.Employees.AnyAsync(e => e.DepartmentId == departmentId);
        }
    }
}
