using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Interfaces;

namespace HRMS.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResult> LoginAsync(string usernameOrEmail, string password)
        {
            var result = new LoginResult();

            if (string.IsNullOrWhiteSpace(usernameOrEmail))
                result.UsernameError = "من فضلك ادخل اسم مستخدم صالح";

            if (string.IsNullOrWhiteSpace(password))
                result.PasswordError = "من فضلك ادخل كلمة مرور صالحة";

            if (result.HasErrors) return result;

            var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(usernameOrEmail);

            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                result.UsernameError = "من فضلك ادخل اسم مستخدم صالح";
                result.PasswordError = "من فضلك ادخل كلمة مرور صالحة";
                return result;
            }

            result.Success = true;
            result.User = user;
            return result;
        }
    }
}
