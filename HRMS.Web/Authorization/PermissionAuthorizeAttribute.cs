using HRMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Authorization
{
    public class PermissionAuthorizeAttribute : TypeFilterAttribute
    {
        public PermissionAuthorizeAttribute(SystemPage page, PermissionAction action)
            : base(typeof(PermissionAuthorizeFilter))
        {
            Arguments = new object[] { page, action };
        }
    }
}
