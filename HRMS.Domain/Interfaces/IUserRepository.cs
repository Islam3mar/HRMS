using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // الخاصة بس
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> UsernameOrEmailExistsAsync(string username, string email);

        Task<IEnumerable<User>> GetAllWithRoleAsync();
    }
}
