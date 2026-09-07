using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResult> CreateUserAsync(string fullName, string username, string email, string password, int roleId);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
