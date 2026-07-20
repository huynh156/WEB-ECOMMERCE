using FashionHubWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FashionHubWeb.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly FashionHubContext _context;

        public WishlistController(FashionHubContext context)
        {
            _context = context;
        }

        private async Task<string> GetOrCreateUserIdAsync()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return "USER1";
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    return existingUser.UserId;
                }
            }

            var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(googleId))
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == googleId);
                if (existingUser != null)
                {
                    return existingUser.UserId;
                }

                var userName = User.FindFirstValue(ClaimTypes.Name) ?? "GoogleUser";
                var userEmail = email ?? $"{googleId}@google.com";

                var uniqueUsername = userName;
                int count = 1;
                while (await _context.Users.AnyAsync(u => u.Username == uniqueUsername))
                {
                    uniqueUsername = $"{userName}_{count++}";
                }

                var newUser = new User
                {
                    UserId = googleId,
                    Username = uniqueUsername,
                    Email = userEmail,
                    Password = "123",
                    FullName = userName,
                    Address = "Unknown",
                    PhoneNumber = 0,
                    Role = "Customer",
                    RandomKey = Guid.NewGuid().ToString(),
                    IsActive = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return googleId;
            }

            return "USER1";
        }

        public async Task<IActionResult> Index()
        {
            var userId = await GetOrCreateUserIdAsync();
            var wishlistItems = await _context.Wishlists
                .Include(w => w.Product)
                .ThenInclude(p => p.Brand)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return View(wishlistItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string productId)
        {
            var userId = await GetOrCreateUserIdAsync();
            var exists = await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
            if (!exists)
            {
                var wishlist = new Wishlist
                {
                    WishlistId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    ProductId = productId
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, added = true });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(string productId)
        {
            var userId = await GetOrCreateUserIdAsync();
            var wishlist = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
            bool added = false;
            
            if (wishlist == null)
            {
                var newWish = new Wishlist
                {
                    WishlistId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    ProductId = productId
                };
                _context.Wishlists.Add(newWish);
                added = true;
            }
            else
            {
                _context.Wishlists.Remove(wishlist);
            }
            
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, added });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string id)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist != null)
            {
                _context.Wishlists.Remove(wishlist);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
