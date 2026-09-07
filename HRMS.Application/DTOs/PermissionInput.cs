using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs
{
    public class PermissionInput
    {
        public SystemPage SystemPage { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        public bool HasAnyPermission => CanView || CanAdd || CanEdit || CanDelete;
    }
}
