using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Repositories;

namespace HRMS.Domain.Repositories.Classes
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        private IUserRepository? _users;
        private IEmployeeRepository? _employees;
        private IRoleRepository? _roles;

        public UnitOfWork(ApplicationDbContext context) => _context = context;

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);
        public IRoleRepository Roles => _roles ??= new RoleRepository(_context);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
