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
    public class UsersController : Controller
    {
        private readonly FashionHubContext _context;

        public UsersController(FashionHubContext context)
        {
            _context = context;
        }

        // GET: Users
        // Takes up to 50 users prioritized by recent order date, with pagination
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            int pageSize = 10;
            if (page < 1) page = 1;

            var query = _context.Users
                .Include(u => u.Orders)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(term) ||
                                         u.FullName.ToLower().Contains(term) ||
                                         u.Email.ToLower().Contains(term) ||
                                         u.PhoneNumber.ToString().Contains(term));
            }

            // Order by most recent order date descending, then by username
            var top50Users = await query
                .OrderByDescending(u => u.Orders.Max(o => (DateTime?)o.OrderDate) ?? DateTime.MinValue)
                .ThenByDescending(u => u.Orders.Count)
                .ThenBy(u => u.Username)
                .Take(50)
                .ToListAsync();

            int totalUsers = top50Users.Count;
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var pagedUsers = top50Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.SearchTerm = search;

            return View(pagedUsers);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderDetails)
                .Include(u => u.Reviews)
                .Include(u => u.Wishlists)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Customer", Text = "Customer" },
                new SelectListItem { Value = "Admin", Text = "Admin" }
            };
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Password,Email,FullName,Address,PhoneNumber,Role,IsActive")] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
                ModelState.AddModelError("Username", "Username is required.");
            if (string.IsNullOrWhiteSpace(user.Password))
                ModelState.AddModelError("Password", "Password is required.");
            if (string.IsNullOrWhiteSpace(user.Email))
                ModelState.AddModelError("Email", "Email is required.");

            var existsUsername = await _context.Users.AnyAsync(u => u.Username == user.Username);
            if (existsUsername)
                ModelState.AddModelError("Username", "Username already exists.");

            var existsEmail = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (existsEmail)
                ModelState.AddModelError("Email", "Email is already registered.");

            if (ModelState.IsValid)
            {
                user.UserId = Guid.NewGuid().ToString();
                user.RandomKey = Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(user.Role)) user.Role = "Customer";

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Customer", Text = "Customer", Selected = user.Role == "Customer" },
                new SelectListItem { Value = "Admin", Text = "Admin", Selected = user.Role == "Admin" }
            };
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Customer", Text = "Customer", Selected = user.Role == "Customer" },
                new SelectListItem { Value = "Admin", Text = "Admin", Selected = user.Role == "Admin" }
            };
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserId,Username,Email,FullName,Address,PhoneNumber,Role,IsActive")] User user, string? NewPassword)
        {
            if (id != user.UserId) return NotFound();

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null) return NotFound();

            if (existingUser.Username != user.Username)
            {
                var existsUsername = await _context.Users.AnyAsync(u => u.Username == user.Username && u.UserId != id);
                if (existsUsername)
                    ModelState.AddModelError("Username", "Username already taken.");
            }

            if (existingUser.Email != user.Email)
            {
                var existsEmail = await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != id);
                if (existsEmail)
                    ModelState.AddModelError("Email", "Email already registered.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingUser.Username = user.Username;
                    existingUser.Email = user.Email;
                    existingUser.FullName = user.FullName;
                    existingUser.Address = user.Address;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.Role = user.Role;
                    existingUser.IsActive = user.IsActive;

                    // Update password only if provided
                    if (!string.IsNullOrWhiteSpace(NewPassword))
                    {
                        existingUser.Password = NewPassword;
                    }

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Customer", Text = "Customer", Selected = user.Role == "Customer" },
                new SelectListItem { Value = "Admin", Text = "Admin", Selected = user.Role == "Admin" }
            };
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
