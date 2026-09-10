using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Specifications.Users
{
    public class UsersPagedSpecification : BaseSpecification<User>
    {
        public UsersPagedSpecification(int page, int pageSize)
        {
            AddInclude(u => u.Role);
            ApplyOrderBy(u => u.FullName);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }
}
