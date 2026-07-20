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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Challenge();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

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
            // If the return URL is to a POST action (like Add to Cart), redirect to Home instead
            // to avoid HTTP 405 Method Not Allowed on the callback GET request.
            if (returnUrl != null && (returnUrl.Contains("/Add", StringComparison.OrdinalIgnoreCase) || returnUrl.Contains("/Remove", StringComparison.OrdinalIgnoreCase)))
            {
                returnUrl = "/";
            }

            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
