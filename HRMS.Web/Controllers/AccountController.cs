using System.Security.Claims;
using HRMS.Application.Interfaces;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Controllers
{
    [AllowAnonymous]   // أي حد يقدر يوصل للـ Controller دا حتى لو مش عامل Login
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // لو المستخدم مسجل دخول بالفعل، متعرضلوش شاشة الدخول تاني
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // القاعدة رقم 3: حقل فاضي (بيتغطى هنا من ناحية السيرفر برضه)
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.LoginAsync(model.Username, model.Password);

            // القاعدة رقم 2: بيانات غير صحيحة
            if (!result.Success)
            {
                if (result.UsernameError != null)
                    ModelState.AddModelError(nameof(model.Username), result.UsernameError);

                if (result.PasswordError != null)
                    ModelState.AddModelError(nameof(model.Password), result.PasswordError);

                return View(model);
            }

            // القاعدة رقم 1: نجاح تسجيل الدخول
            await SignInUserAsync(result.User!);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        private async Task SignInUserAsync(Domain.Entities.User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.Name),   // اسم المجموعة (Role) مش رقمها، عشان نستخدمه في [Authorize(Roles = "...")]
                new("RoleId", user.RoleId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
     CookieAuthenticationDefaults.AuthenticationScheme,
     principal,
     new AuthenticationProperties
     {
         IsPersistent = false
     });
        }
    }
}
