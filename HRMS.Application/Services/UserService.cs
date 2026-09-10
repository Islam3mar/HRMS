using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Profiles;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Specifications.Employees;
using HRMS.Domain.Specifications.Users;

namespace HRMS.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserResult> CreateUserAsync(string fullName, string username, string email, string password, int roleId)
        {
            var result = new UserResult();

            if (string.IsNullOrWhiteSpace(fullName))
                result.FullNameError = "من فضلك ادخل الاسم بالكامل";

            if (string.IsNullOrWhiteSpace(username))
                result.UsernameError = "من فضلك ادخل اسم مستخدم صالح";

            if (string.IsNullOrWhiteSpace(email))
                result.EmailError = "من فضلك ادخل بريد الكتروني صالح";

            if (string.IsNullOrWhiteSpace(password))
                result.PasswordError = "من فضلك ادخل كلمة مرور صالحة";

            if (roleId <= 0)
                result.RoleError = "من فضلك اختر الصلاحيات";

            if (result.HasErrors) return result;

            // القاعدة الاضافية (منطقية): منع تكرار اسم المستخدم او الايميل
            if (await _unitOfWork.Users.UsernameOrEmailExistsAsync(username, email))
            {
                result.UsernameError = "اسم المستخدم أو البريد الإلكتروني مستخدم بالفعل";
                return result;
            }

            // الـ hashing منطق أمني، فضل هنا في الـ Service ومش اتنقل للـ Profile
            var passwordHash = PasswordHasher.Hash(password);
            var user = UserProfile.ToEntity(fullName, username, email, passwordHash, roleId);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.User = user;
            return result;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllWithRoleAsync();
        }

        public async Task<PagedResult<User>> GetPagedUsersAsync(int page, int pageSize)
        {
            var spec = new UsersPagedSpecification(page, pageSize);
            var items = await _unitOfWork.Users.ListAsync(spec);
            var total = await _unitOfWork.Users.CountAsync(spec);

            return new PagedResult<User> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }
    }
}
