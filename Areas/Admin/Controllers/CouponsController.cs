using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FashionHubWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace FashionHubWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponsController : Controller
    {
        private readonly FashionHubContext _context;

        public CouponsController(FashionHubContext context)
        {
            _context = context;
        }

        // GET: Coupons
        public async Task<IActionResult> Index()
        {
            return View(await _context.Coupons.Include(c => c.Orders).ToListAsync());
        }

        // GET: Coupons/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var coupon = await _context.Coupons
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(m => m.CouponId == id);
            if (coupon == null) return NotFound();

            return View(coupon);
        }

        // GET: Coupons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Coupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CouponId,CouponCode,DiscountAmount,ExpiryDate,Quantity,IsActive,DiscountType,MaxDiscount,MinOrderValue")] Coupon coupon)
        {
            if (string.IsNullOrWhiteSpace(coupon.CouponId))
                coupon.CouponId = "CPN_" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            if (string.IsNullOrWhiteSpace(coupon.DiscountType))
                coupon.DiscountType = "Fixed";

            if (ModelState.IsValid)
            {
                _context.Add(coupon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        // GET: Coupons/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();
            return View(coupon);
        }

        // POST: Coupons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CouponId,CouponCode,DiscountAmount,ExpiryDate,Quantity,IsActive,DiscountType,MaxDiscount,MinOrderValue")] Coupon coupon)
        {
            if (id != coupon.CouponId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coupon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CouponExists(coupon.CouponId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        // GET: Coupons/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var coupon = await _context.Coupons
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(m => m.CouponId == id);
            if (coupon == null) return NotFound();

            return View(coupon);
        }

        // POST: Coupons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CouponExists(string id)
        {
            return _context.Coupons.Any(e => e.CouponId == id);
        }
    }
}
