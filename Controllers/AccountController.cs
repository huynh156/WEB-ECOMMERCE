using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FashionHubWeb.Models;

namespace FashionHubWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly FashionHubContext _context;

        public AccountController(FashionHubContext context)
        {
            _context = context;
        }

        private async Task<User> GetOrCreateUserFromClaimsAsync(ClaimsPrincipal principal)
        {
            var email = principal.FindFirstValue(ClaimTypes.Email) ?? "";
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString();
            
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email || u.UserId == userId);
            if (existingUser != null)
            {
                return existingUser;
            }

            var name = principal.FindFirstValue(ClaimTypes.Name) ?? "GoogleUser";
            var uniqueUsername = name.Replace(" ", "");
            int count = 1;
            while (await _context.Users.AnyAsync(u => u.Username == uniqueUsername))
            {
                uniqueUsername = $"{name.Replace(" ", "")}_{count++}";
            }

            var newUser = new User
            {
                UserId = userId,
                Username = uniqueUsername,
                Email = email,
                Password = "123",
                FullName = name,
                Address = "Unknown",
                PhoneNumber = 0,
                Role = "Customer",
                RandomKey = Guid.NewGuid().ToString(),
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var user = await GetOrCreateUserFromClaimsAsync(User);
            
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Brand)
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect(returnUrl);
            }

            if (returnUrl != null && (returnUrl.Contains("/Add", StringComparison.OrdinalIgnoreCase) || returnUrl.Contains("/Remove", StringComparison.OrdinalIgnoreCase)))
            {
                returnUrl = "/";
            }

            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);
            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Username && u.Password == model.Password);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "This account is inactive.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            if (returnUrl != null && (returnUrl.Contains("/Add", StringComparison.OrdinalIgnoreCase) || returnUrl.Contains("/Remove", StringComparison.OrdinalIgnoreCase)))
            {
                returnUrl = "/";
            }

            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = "/")
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new RegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model, string returnUrl = "/")
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var usernameExists = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            int parsedPhone = 0;
            int.TryParse(model.PhoneNumber, out parsedPhone);

            var newUser = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Username = model.Username,
                Password = model.Password, // Simple seed storage structure
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                PhoneNumber = parsedPhone,
                Role = "Customer",
                RandomKey = Guid.NewGuid().ToString(),
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Auto-login
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.UserId),
                new Claim(ClaimTypes.Name, newUser.FullName),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.Role, newUser.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
