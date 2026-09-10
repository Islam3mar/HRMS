using HRMS.Application.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Web.Authorization;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public UsersController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        [PermissionAuthorize(SystemPage.UsersManagement, PermissionAction.View)]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [PermissionAuthorize(SystemPage.UsersManagement, PermissionAction.Add)]
        public async Task<IActionResult> Create()
        {
            return View(await BuildEmptyFormAsync());
        }

        [HttpPost, PermissionAuthorize(SystemPage.UsersManagement, PermissionAction.Add)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            var result = await _userService.CreateUserAsync(
                model.FullName, model.Username, model.Email, model.Password, model.RoleId);

            if (!result.Success)
            {
                if (result.FullNameError != null) ModelState.AddModelError(nameof(model.FullName), result.FullNameError);
                if (result.UsernameError != null) ModelState.AddModelError(nameof(model.Username), result.UsernameError);
                if (result.EmailError != null) ModelState.AddModelError(nameof(model.Email), result.EmailError);
                if (result.PasswordError != null) ModelState.AddModelError(nameof(model.Password), result.PasswordError);
                if (result.RoleError != null) ModelState.AddModelError(nameof(model.RoleId), result.RoleError);

                model.Roles = await GetRoleOptionsAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = "تم اضافة المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private async Task<UserFormViewModel> BuildEmptyFormAsync()
        {
            return new UserFormViewModel
            {
                Roles = await GetRoleOptionsAsync()
            };
        }

        private async Task<List<SelectListItem>> GetRoleOptionsAsync()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return roles.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList();
        }
    }
}
