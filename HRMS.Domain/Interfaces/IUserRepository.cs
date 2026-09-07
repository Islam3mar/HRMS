using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        void Update(User user);
        void Delete(User user);

        // الخاصة بس
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> UsernameOrEmailExistsAsync(string username, string email);
    }
}
