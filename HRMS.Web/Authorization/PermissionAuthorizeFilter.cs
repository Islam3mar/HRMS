using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRMS.Web.Authorization
{
    public class PermissionAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly SystemPage _page;
        private readonly PermissionAction _action;
        private readonly IUnitOfWork _unitOfWork;

        public PermissionAuthorizeFilter(SystemPage page, PermissionAction action, IUnitOfWork unitOfWork)
        {
            _page = page;
            _action = action;
            _unitOfWork = unitOfWork;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // [Authorize] بتتأكد إنه عامل Login، هنا بس بنتأكد من الصلاحية التفصيلية
            if (user.Identity?.IsAuthenticated != true)
                return;

            var roleIdClaim = user.FindFirst("RoleId")?.Value;
            if (!int.TryParse(roleIdClaim, out var roleId))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            var role = await _unitOfWork.Roles.GetByIdAsync(roleId); // بيجيب الـ Permissions معاه (Include موجود بالفعل)
            var permission = role?.Permissions.FirstOrDefault(p => p.SystemPage == _page);

            var allowed = _action switch
            {
                PermissionAction.View => permission?.CanView == true,
                PermissionAction.Add => permission?.CanAdd == true,
                PermissionAction.Edit => permission?.CanEdit == true,
                PermissionAction.Delete => permission?.CanDelete == true,
                _ => false
            };

            if (!allowed)
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}
