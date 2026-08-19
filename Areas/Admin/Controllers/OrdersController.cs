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
    public class OrdersController : Controller
    {
        private readonly FashionHubContext _context;

        public OrdersController(FashionHubContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(int page = 1, string search = "", string status = "")
        {
            int pageSize = 15;
            if (page < 1) page = 1;

            var query = _context.Orders
                .Include(o => o.Coupon)
                .Include(o => o.Shipper)
                .Include(o => o.StatusNavigation)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(o => o.OrderId.ToLower().Contains(term) ||
                                         (o.FullName != null && o.FullName.ToLower().Contains(term)) ||
                                         (o.PhoneNumber != null && o.PhoneNumber.ToString().Contains(term)) ||
                                         (o.User != null && o.User.Username.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            int totalOrders = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.SearchTerm = search;
            ViewBag.CurrentStatus = status;

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Coupon)
                .Include(o => o.Shipper)
                .Include(o => o.StatusNavigation)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Brand)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

            ViewBag.Statuses = new List<string> { "Pending", "Processing", "Shipped", "Completed", "Cancelled" };
            return View(order);
        }

        // POST: Quick Status Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["StatusSuccess"] = $"Order #{order.OrderId.Substring(0, Math.Min(8, order.OrderId.Length)).ToUpper()} status updated to '{status}'.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponCode");
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperName");
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName");
            ViewBag.StatusList = new SelectList(new[] { "Pending", "Processing", "Shipped", "Completed", "Cancelled" });
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,UserId,OrderDate,TotalAmount,Status,CouponId,ReceivedDate,ExpectedDeliveryDate,FullName,PhoneNumber,Address,PaymentMethod,StatusId,ShipperId,ShippingMethod,Notes")] Order order)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(order.OrderId))
                    order.OrderId = Guid.NewGuid().ToString();
                if (!order.OrderDate.HasValue)
                    order.OrderDate = DateTime.Now;

                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponCode", order.CouponId);
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperName", order.ShipperId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName", order.StatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", order.UserId);
            ViewBag.StatusList = new SelectList(new[] { "Pending", "Processing", "Shipped", "Completed", "Cancelled" }, order.Status);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponCode", order.CouponId);
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperName", order.ShipperId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName", order.StatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", order.UserId);
            ViewBag.StatusList = new SelectList(new[] { "Pending", "Processing", "Shipped", "Completed", "Cancelled" }, order.Status);
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("OrderId,UserId,OrderDate,TotalAmount,Status,CouponId,ReceivedDate,ExpectedDeliveryDate,FullName,PhoneNumber,Address,PaymentMethod,StatusId,ShipperId,ShippingMethod,Notes")] Order order)
        {
            if (id != order.OrderId) return NotFound();

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
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CouponId"] = new SelectList(_context.Coupons, "CouponId", "CouponCode", order.CouponId);
            ViewData["ShipperId"] = new SelectList(_context.Shippers, "ShipperId", "ShipperName", order.ShipperId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName", order.StatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", order.UserId);
            ViewBag.StatusList = new SelectList(new[] { "Pending", "Processing", "Shipped", "Completed", "Cancelled" }, order.Status);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Coupon)
                .Include(o => o.Shipper)
                .Include(o => o.StatusNavigation)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

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
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
