using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Application.DTOs
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public User? User { get; set; }
        public string? UsernameError { get; set; }
        public string? PasswordError { get; set; }

        public bool HasErrors => UsernameError != null || PasswordError != null;
    }
}
