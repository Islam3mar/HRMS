using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Application.Profiles
{
    public static class UserProfile
    {
        // بياخد الـ password hash جاهز من الـ Service (الـ hashing مش من مسؤولية الـ Profile)
        // وبيبني User Entity منه
        public static User ToEntity(string fullName, string username, string email, string passwordHash, int roleId)
        {
            return new User
            {
                FullName = fullName.Trim(),
                Username = username.Trim(),
                Email = email.Trim(),
                PasswordHash = passwordHash,
                RoleId = roleId
            };
        }
    }
}
