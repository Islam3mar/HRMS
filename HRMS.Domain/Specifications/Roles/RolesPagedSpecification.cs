using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Specifications.Roles
{
    public class RolesPagedSpecification : BaseSpecification<Role>
    {
        public RolesPagedSpecification(int page, int pageSize)
        {
            AddInclude(r => r.Permissions);
            ApplyOrderBy(r => r.Name);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }
}
