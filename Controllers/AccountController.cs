using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Data;
using SchoolProject.Models;
using SchoolProject.Models.ViewModels;

namespace SchoolProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        [Route("Account/Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "AdminDashboard");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpGet]
        [Route("Account/Setup")]
        public IActionResult SetupAdmin()
        {
            // Check if admin already exists
            if (_context.AdminUsers.Any(u => u.Username == "admin"))
                return Content("Admin already exists");

            var admin = new AdminUser
            {
                Username = "admin",
                Email = "admin@bangaloreeducation.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                Role = "Admin",
                IsActive = true
            };

            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            return Content("Admin created successfully. Username: admin, Password: Admin@123");
        }

        [HttpGet]
        [Route("Account/ResetAdminPassword")]
        public IActionResult ResetAdminPassword()
        {
            var admin = _context.AdminUsers.FirstOrDefault(u => u.Username == "admin");
            if (admin == null)
                return Content("Admin user not found");

            // Generate proper BCrypt hash
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            admin.IsActive = true;
            _context.SaveChanges();

            return Content("Admin password reset successfully. New password: Admin@123");
        }

        // POST: /Account/Login
        [HttpPost]
        [Route("Account/Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.AdminUsers
                .FirstOrDefault(u => u.Username.ToLower() == model.Username.ToLower()
                                  && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Update last login
            user.LastLogin = DateTime.Now;
            _context.SaveChanges();

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName ?? user.Username),
                new Claim("AdminId", user.AdminId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "AdminDashboard");
        }

        // POST: /Account/Logout
        [HttpPost]
        [Route("Account/Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [Route("Account/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}