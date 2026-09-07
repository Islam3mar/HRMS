using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Application.DTOs
{
    public class RoleResult
    {
        public bool Success { get; set; }
        public Role? Role { get; set; }
        public string? NameError { get; set; }
        public string? PermissionsError { get; set; }

        public bool HasErrors => NameError != null || PermissionsError != null;
    }
}
