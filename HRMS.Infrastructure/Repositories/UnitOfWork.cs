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

        #region Private Fields
        private IUserRepository? _users;
        private IEmployeeRepository? _employees;
        private IRoleRepository? _roles;
        private IGeneralSettingsRepository? _generalSettings;
        private IOfficialHolidayRepository? _officialHolidays;
        private IDepartmentRepository? _departments;
        private IAttendanceRecordRepository? _attendanceRecords;
        #endregion

        public UnitOfWork(ApplicationDbContext context) => _context = context;

        #region Public Properties
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);
        public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
        public IGeneralSettingsRepository GeneralSettings => _generalSettings ??= new GeneralSettingsRepository(_context);
        public IOfficialHolidayRepository OfficialHolidays => _officialHolidays ??= new OfficialHolidayRepository(_context);
        public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(_context);
        public IAttendanceRecordRepository AttendanceRecords => _attendanceRecords ??= new AttendanceRecordRepository(_context);

        #endregion
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
