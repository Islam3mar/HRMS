using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IEmployeeRepository Employees { get; }   // هنضيفها لما نوصل لفيتشر الموظفين

        Task<int> SaveChangesAsync();
    }
}
