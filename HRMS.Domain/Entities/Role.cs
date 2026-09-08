using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;

namespace HRMS.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = null!;   // اسم المجموعة

        public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
