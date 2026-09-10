using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Web.Authorization;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [PermissionAuthorize(SystemPage.RolesManagement, PermissionAction.View)]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return View(roles);
        }

        [PermissionAuthorize(SystemPage.RolesManagement, PermissionAction.Add)]
        public IActionResult Create()
        {
            return View(BuildEmptyForm());
        }

        [HttpPost, PermissionAuthorize(SystemPage.RolesManagement, PermissionAction.Add)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleFormViewModel model)
        {
            var permissions = model.Permissions.Select(p => new PermissionInput
            {
                SystemPage = p.SystemPage,
                CanView = p.CanView,
                CanAdd = p.CanAdd,
                CanEdit = p.CanEdit,
                CanDelete = p.CanDelete
            }).ToList();

            var result = await _roleService.CreateRoleAsync(model.Name, permissions);

            if (!result.Success)
            {
                if (result.NameError != null)
                    ModelState.AddModelError(nameof(model.Name), result.NameError);

                if (result.PermissionsError != null)
                    ModelState.AddModelError(string.Empty, result.PermissionsError);

                return View(model);
            }

            TempData["SuccessMessage"] = "تم اضافة المجموعة بنجاح";
            return RedirectToAction(nameof(Index));
        }


        // 
        private RoleFormViewModel BuildEmptyForm()
        {
            var pages = Enum.GetValues<SystemPage>();

            return new RoleFormViewModel
            {
                Permissions = pages.Select(p => new PermissionRowViewModel
                {
                    SystemPage = p,
                    PageName = p.ToArabicName()
                }).ToList()
            };
        }
    }
}
