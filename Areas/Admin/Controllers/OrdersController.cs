using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FashionHubWeb.Models;

namespace FashionHubWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly FashionHubContext _context;

        public OrdersController(FashionHubContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var fashionHubContext = _context.Orders.Include(o => o.Coupon).Include(o => o.Shipper).Include(o => o.StatusNavigation).Include(o => o.User);
            return View(await fashionHubContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Coupon)
                .Include(o => o.Shipper)
                .Include(o => o.StatusNavigation)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponId");
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperId");
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,UserId,OrderDate,TotalAmount,Status,CouponId,ReceivedDate,ExpectedDeliveryDate,FullName,PhoneNumber,Address,PaymentMethod,StatusId,ShipperId,ShippingMethod,Notes")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponId", order.CouponId);
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperId", order.ShipperId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusId", order.StatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponId", order.CouponId);
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperId", order.ShipperId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusId", order.StatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("OrderId,UserId,OrderDate,TotalAmount,Status,CouponId,ReceivedDate,ExpectedDeliveryDate,FullName,PhoneNumber,Address,PaymentMethod,StatusId,ShipperId,ShippingMethod,Notes")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponId", order.CouponId);
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperId", order.ShipperId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusId", order.StatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Coupon)
                .Include(o => o.Shipper)
                .Include(o => o.StatusNavigation)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}

