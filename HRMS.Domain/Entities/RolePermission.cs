using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities
{
    public class RolePermission : BaseEntity
    {
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public SystemPage SystemPage { get; set; }

        public bool CanView { get; set; }    // عرض
        public bool CanAdd { get; set; }     // اضافة
        public bool CanEdit { get; set; }    // تعديل
        public bool CanDelete { get; set; }  // حذف
    }
}
