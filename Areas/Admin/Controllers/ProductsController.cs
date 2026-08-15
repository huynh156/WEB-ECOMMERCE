using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FashionHubWeb.Models;
using FashionHubWeb.Helpers;

using Microsoft.AspNetCore.Authorization;

namespace FashionHubWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly FashionHubContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(FashionHubContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var fashionHubContext = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Coupon)
                .Include(p => p.ProductImages);
            return View(await fashionHubContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Coupon)
                .Include(p => p.ProductImages.OrderBy(pi => pi.SortOrder))
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponCode");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,SlugName,Price,StockQuantity,Description,CategoryId,BrandId,CouponId")] Product product, List<IFormFile> Images)
        {
            if (ModelState.IsValid)
            {
                // Handle multiple image uploads
                if (Images != null && Images.Count > 0)
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        var fileName = await MyTool.UploadFileToFolder(Images[i], "products");
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var productImage = new ProductImage
                            {
                                ProductImageId = Guid.NewGuid().ToString(),
                                ProductId = product.ProductId,
                                ImagePath = fileName,
                                IsMain = (i == 0), // First image is main
                                SortOrder = i
                            };
                            _context.ProductImages.Add(productImage);

                            // Also set the first image as the Product.Image for backward compatibility
                            if (i == 0)
                            {
                                product.Image = $"/images/products/{fileName}";
                            }
                        }
                    }
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponCode", product.CouponId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductImages.OrderBy(pi => pi.SortOrder))
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponCode", product.CouponId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProductId,ProductName,SlugName,Price,StockQuantity,Image,Description,CategoryId,BrandId,CouponId")] Product product, List<IFormFile> NewImages)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle new image uploads
                    if (NewImages != null && NewImages.Count > 0)
                    {
                        var existingCount = await _context.ProductImages.CountAsync(pi => pi.ProductId == product.ProductId);
                        for (int i = 0; i < NewImages.Count; i++)
                        {
                            var fileName = await MyTool.UploadFileToFolder(NewImages[i], "products");
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                var productImage = new ProductImage
                                {
                                    ProductImageId = Guid.NewGuid().ToString(),
                                    ProductId = product.ProductId,
                                    ImagePath = fileName,
                                    IsMain = (existingCount == 0 && i == 0),
                                    SortOrder = existingCount + i
                                };
                                _context.ProductImages.Add(productImage);

                                // Update main image reference if this is the first image ever
                                if (existingCount == 0 && i == 0)
                                {
                                    product.Image = $"/images/products/{fileName}";
                                }
                            }
                        }
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponCode", product.CouponId);
            return View(product);
        }

        // POST: Products/DeleteImage (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeleteImage(string imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found." });
            }

            // Delete physical file
            MyTool.DeleteFileFromFolder(image.ImagePath, "products");

            var productId = image.ProductId;
            var wasMain = image.IsMain;

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            // If deleted image was main, set next one as main
            if (wasMain)
            {
                var nextImage = await _context.ProductImages
                    .Where(pi => pi.ProductId == productId)
                    .OrderBy(pi => pi.SortOrder)
                    .FirstOrDefaultAsync();
                if (nextImage != null)
                {
                    nextImage.IsMain = true;
                    // Also update Product.Image
                    var product = await _context.Products.FindAsync(productId);
                    if (product != null)
                    {
                        product.Image = $"/images/products/{nextImage.ImagePath}";
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return Json(new { success = true });
        }

        // POST: Products/SetMainImage (AJAX)
        [HttpPost]
        public async Task<IActionResult> SetMainImage(string imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found." });
            }

            // Unset all existing main images for this product
            var allImages = await _context.ProductImages
                .Where(pi => pi.ProductId == image.ProductId)
                .ToListAsync();
            foreach (var img in allImages)
            {
                img.IsMain = false;
            }

            // Set new main
            image.IsMain = true;

            // Update Product.Image
            var product = await _context.Products.FindAsync(image.ProductId);
            if (product != null)
            {
                product.Image = $"/images/products/{image.ImagePath}";
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Coupon)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product != null)
            {
                // Delete all physical image files
                foreach (var img in product.ProductImages)
                {
                    MyTool.DeleteFileFromFolder(img.ImagePath, "products");
                }

                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
