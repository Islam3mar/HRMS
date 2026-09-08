using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Application.DTOs
{
    public class DepartmentResult
    {
        public bool Success { get; set; }
        public Department? Department { get; set; }

        public string? NameError { get; set; }

        public bool HasErrors => NameError != null;
    }
}
