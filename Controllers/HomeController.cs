using System.Diagnostics;
using FashionHubWeb.Models;
using FashionHubWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace FashionHubWeb.Controllers;

public class HomeController : Controller
{
    private readonly FashionHubContext _context;

    public HomeController(FashionHubContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Get 6 newest products for the landing page
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .OrderByDescending(p => p.ProductId)
            .Take(6)
            .ToListAsync();
            
        // Get 4 best sellers (mocked by ordering by price or random for seed data)
        var bestSellers = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .OrderBy(p => p.Price)
            .Take(4)
            .ToListAsync();

        var wishlistIds = new List<string>();
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    wishlistIds = await _context.Wishlists
                        .Where(w => w.UserId == user.UserId)
                        .Select(w => w.ProductId)
                        .ToListAsync();
                }
            }
        }

        ViewBag.BestSellers = bestSellers;
        ViewBag.WishlistProductIds = wishlistIds;
            
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
