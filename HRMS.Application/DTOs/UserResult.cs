using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Application.DTOs
{
    public class UserResult
    {
        public bool Success { get; set; }
        public User? User { get; set; }

        public string? FullNameError { get; set; }
        public string? UsernameError { get; set; }
        public string? EmailError { get; set; }
        public string? PasswordError { get; set; }
        public string? RoleError { get; set; }

        public bool HasErrors =>
            FullNameError != null || UsernameError != null ||
            EmailError != null || PasswordError != null || RoleError != null;
    }
}
