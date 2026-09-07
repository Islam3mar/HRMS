using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string usernameOrEmail, string password);
    }
}
