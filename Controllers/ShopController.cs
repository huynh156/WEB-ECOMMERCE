using FashionHubWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FashionHubWeb.Controllers
{
    public class ShopController : Controller
    {
        private readonly FashionHubContext _context;

        public ShopController(FashionHubContext context)
        {
            _context = context;
        }

        private async Task<List<string>> GetWishlistProductIdsAsync()
        {
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
            return wishlistIds;
        }

        public async Task<IActionResult> Index(string query, string category, string brand, string sort, decimal? minPrice, decimal? maxPrice)
        {
            var products = _context.Products.Include(p => p.Brand).Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                var term = query.ToLower().Trim();
                products = products.Where(p => p.ProductName.ToLower().Contains(term)
                                            || (p.Category != null && p.Category.CategoryName.ToLower().Contains(term))
                                            || (p.Brand != null && p.Brand.BrandName.ToLower().Contains(term)));
            }

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.CategoryId == category);
            }

            if (!string.IsNullOrEmpty(brand))
            {
                products = products.Where(p => p.BrandId == brand);
            }

            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice.Value);

            products = sort switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "name_asc" => products.OrderBy(p => p.ProductName),
                "name_desc" => products.OrderByDescending(p => p.ProductName),
                _ => products.OrderByDescending(p => p.ProductId)
            };

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Brands = await _context.Brands.ToListAsync();
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentBrand = brand;
            ViewBag.SearchQuery = query;
            ViewBag.CurrentSort = sort;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.WishlistProductIds = await GetWishlistProductIdsAsync();

            var allPrices = await _context.Products.Select(p => p.Price).ToListAsync();
            ViewBag.PriceRangeMin = allPrices.Any() ? allPrices.Min() : 0;
            ViewBag.PriceRangeMax = allPrices.Any() ? allPrices.Max() : 10000;

            var productList = await products.ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductGrid", productList);
            }

            return View(productList);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages.OrderBy(pi => pi.SortOrder))
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            // Fetch 4 related products in same category
            var related = await _context.Products
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = related;
            ViewBag.WishlistProductIds = await GetWishlistProductIdsAsync();

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> SearchAutocomplete(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new List<object>());
            }

            var term = query.ToLower().Trim();

            var matches = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.ProductName.ToLower().Contains(term)
                         || (p.Category != null && p.Category.CategoryName.ToLower().Contains(term))
                         || (p.Brand != null && p.Brand.BrandName.ToLower().Contains(term)))
                .Take(5)
                .Select(p => new
                {
                    productId = p.ProductId,
                    productName = p.ProductName,
                    brandName = p.Brand != null ? p.Brand.BrandName : "",
                    categoryName = p.Category != null ? p.Category.CategoryName : "",
                    price = p.Price,
                    image = p.Image
                })
                .ToListAsync();

            return Json(matches);
        }
    }
}
