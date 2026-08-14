using FashionHubWeb.Models;
using FashionHubWeb.ViewModels;
using FashionHubWeb.Helper;
using FashionHubWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FashionHubWeb.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly FashionHubContext _context;
        private readonly PaypalClient _paypalClient;
        private readonly IVnPayService _vnPayService;

        public CartController(FashionHubContext context, PaypalClient paypalClient, IVnPayService vnPayService)
        {
            _context = context;
            _paypalClient = paypalClient;
            _vnPayService = vnPayService;
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
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .ThenInclude(p => p.Brand)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string productId, int quantity = 1)
        {
            var userId = await GetOrCreateUserIdAsync();
            var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                _context.Update(cartItem);
            }
            else
            {
                var newCart = new Cart
                {
                    CartId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.Carts.Add(newCart);
            }

            await _context.SaveChangesAsync();

            var cartCount = await _context.Carts.Where(c => c.UserId == userId).SumAsync(c => c.Quantity);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, cartCount });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string cartId)
        {
            var cartItem = await _context.Carts.FindAsync(cartId);
            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CheckOut()
        {
            var userId = await GetOrCreateUserIdAsync();
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .ThenInclude(p => p.Brand)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            ViewBag.PaypalClientId = _paypalClient.ClientId;
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            var model = new CheckOutVM
            {
                FullName = user?.FullName,
                Address = user?.Address != "Unknown" ? user?.Address : "",
                PhoneNumber = user?.PhoneNumber != 0 ? user?.PhoneNumber : null
            };

            ViewBag.CartItems = cartItems;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(CheckOutVM model, string payment = "COD")
        {
            var userId = await GetOrCreateUserIdAsync();
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                ModelState.AddModelError("", "Your cart is empty.");
                ViewBag.CartItems = cartItems;
                ViewBag.PaypalClientId = _paypalClient.ClientId;
                return View(model);
            }

            if (!ValidateStock(cartItems))
            {
                ModelState.AddModelError("", "One or more products exceed available stock quantity.");
                ViewBag.CartItems = cartItems;
                ViewBag.PaypalClientId = _paypalClient.ClientId;
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var originalTotal = cartItems.Sum(p => (p.Product?.Price ?? 0m) * p.Quantity);
                var finalTotal = originalTotal;
                string? appliedCouponId = null;
                Coupon? appliedCoupon = null;

                // Validate and apply coupon if present
                if (!string.IsNullOrEmpty(model.CouponCode))
                {
                    appliedCoupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponCode == model.CouponCode && c.IsActive == true);
                    if (appliedCoupon != null && appliedCoupon.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now))
                    {
                        if (!appliedCoupon.MinOrderValue.HasValue || originalTotal >= appliedCoupon.MinOrderValue.Value)
                        {
                            decimal discount = 0;
                            if (appliedCoupon.DiscountType == "percentage")
                            {
                                discount = originalTotal * (appliedCoupon.DiscountAmount / 100m);
                                if (appliedCoupon.MaxDiscount.HasValue && discount > appliedCoupon.MaxDiscount.Value)
                                {
                                    discount = appliedCoupon.MaxDiscount.Value;
                                }
                            }
                            else
                            {
                                discount = appliedCoupon.DiscountAmount;
                            }
                            finalTotal = Math.Max(0, originalTotal - discount);
                            appliedCouponId = appliedCoupon.CouponId;
                        }
                    }
                }

                if (payment == "Thanh Toán Bằng VN PAY")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = (double)finalTotal,
                        CreatedDate = DateTime.Now,
                        Description = $"{model.FullName} {model.PhoneNumber}",
                        FullName = model.FullName,
                        OrderId = new Random().Next(1000, 100000),
                    };
                    
                    TempData["Checkout_FullName"] = model.FullName;
                    TempData["Checkout_Address"] = model.Address;
                    TempData["Checkout_PhoneNumber"] = model.PhoneNumber?.ToString();
                    TempData["Checkout_Notes"] = model.Notes;
                    TempData["Checkout_CouponCode"] = model.CouponCode;

                    return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                }

                // COD Payment Flow
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                var order = new Order
                {
                    OrderId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    FullName = model.FullName ?? user?.FullName,
                    Address = model.Address ?? user?.Address,
                    PhoneNumber = model.PhoneNumber ?? user?.PhoneNumber,
                    OrderDate = DateTime.UtcNow,
                    ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)),
                    PaymentMethod = "COD",
                    ShippingMethod = "Standard",
                    ShipperId = "1",
                    StatusId = "1", // Pending
                    Notes = model.Notes,
                    TotalAmount = finalTotal,
                    CouponId = appliedCouponId
                };

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        if (appliedCoupon != null)
                        {
                            appliedCoupon.Quantity = Math.Max(0, appliedCoupon.Quantity - 1);
                            _context.Update(appliedCoupon);
                        }

                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();

                        var orderDetails = cartItems.Select(item => new OrderDetail
                        {
                            OrderDetailId = Guid.NewGuid().ToString(),
                            OrderId = order.OrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            SubTotal = (item.Product?.Price ?? 0m) * item.Quantity
                        }).ToList();

                        _context.OrderDetails.AddRange(orderDetails);
                        
                        _context.Carts.RemoveRange(cartItems);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return RedirectToAction("PaymentSuccess");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "An error occurred while placing order: " + ex.Message);
                    }
                }
            }

            ViewBag.CartItems = cartItems;
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(model);
        }

        private bool ValidateStock(List<Cart> cartItems)
        {
            foreach (var item in cartItems)
            {
                if (item.Product == null || item.Quantity > item.Product.StockQuantity)
                {
                    return false;
                }
            }
            return true;
        }

        [HttpPost("/Cart/PaypalOrder")]
        public async Task<IActionResult> PaypalOrder(string? couponCode)
        {
            var userId = await GetOrCreateUserIdAsync();
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var totalAmount = cartItems.Sum(p => (p.Product?.Price ?? 0m) * p.Quantity);

            // Apply coupon if valid
            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponCode == couponCode && c.IsActive == true);
                if (coupon != null && coupon.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now))
                {
                    if (!coupon.MinOrderValue.HasValue || totalAmount >= coupon.MinOrderValue.Value)
                    {
                        decimal discount = 0;
                        if (coupon.DiscountType == "percentage")
                        {
                            discount = totalAmount * (coupon.DiscountAmount / 100m);
                            if (coupon.MaxDiscount.HasValue && discount > coupon.MaxDiscount.Value)
                            {
                                discount = coupon.MaxDiscount.Value;
                            }
                        }
                        else
                        {
                            discount = coupon.DiscountAmount;
                        }
                        totalAmount = Math.Max(0, totalAmount - discount);
                    }
                }
            }

            // Apply tax or shipping (10% tax in original reference)
            var totalWithTax = (totalAmount + (totalAmount * 0.1m)).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            var currency = "USD";
            var reference = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(totalWithTax, currency, reference);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPost("/Cart/CapturePaypalOrder")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, [FromBody] CheckOutVM model)
        {
            var userId = await GetOrCreateUserIdAsync();
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);
                if (response.status != "COMPLETED")
                {
                    return BadRequest(new { message = "Payment not completed." });
                }

                var originalTotal = cartItems.Sum(p => (p.Product?.Price ?? 0m) * p.Quantity);
                var finalTotal = originalTotal;
                string? appliedCouponId = null;
                Coupon? appliedCoupon = null;

                // Validate and apply coupon if present
                if (!string.IsNullOrEmpty(model.CouponCode))
                {
                    appliedCoupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponCode == model.CouponCode && c.IsActive == true);
                    if (appliedCoupon != null && appliedCoupon.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now))
                    {
                        if (!appliedCoupon.MinOrderValue.HasValue || originalTotal >= appliedCoupon.MinOrderValue.Value)
                        {
                            decimal discount = 0;
                            if (appliedCoupon.DiscountType == "percentage")
                            {
                                discount = originalTotal * (appliedCoupon.DiscountAmount / 100m);
                                if (appliedCoupon.MaxDiscount.HasValue && discount > appliedCoupon.MaxDiscount.Value)
                                {
                                    discount = appliedCoupon.MaxDiscount.Value;
                                }
                            }
                            else
                            {
                                discount = appliedCoupon.DiscountAmount;
                            }
                            finalTotal = Math.Max(0, originalTotal - discount);
                            appliedCouponId = appliedCoupon.CouponId;
                        }
                    }
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                var order = new Order
                {
                    OrderId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    FullName = model.FullName ?? user?.FullName,
                    Address = model.Address ?? user?.Address,
                    PhoneNumber = model.PhoneNumber ?? user?.PhoneNumber,
                    OrderDate = DateTime.UtcNow,
                    ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)),
                    PaymentMethod = "PayPal",
                    ShippingMethod = "Standard",
                    ShipperId = "1",
                    StatusId = "2", // Paid / Processing
                    Notes = model.Notes,
                    TotalAmount = finalTotal,
                    CouponId = appliedCouponId
                };

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        if (appliedCoupon != null)
                        {
                            appliedCoupon.Quantity = Math.Max(0, appliedCoupon.Quantity - 1);
                            _context.Update(appliedCoupon);
                        }

                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();

                        var orderDetails = cartItems.Select(item => new OrderDetail
                        {
                            OrderDetailId = Guid.NewGuid().ToString(),
                            OrderId = order.OrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            SubTotal = (item.Product?.Price ?? 0m) * item.Quantity
                        }).ToList();

                        _context.OrderDetails.AddRange(orderDetails);
                        
                        _context.Carts.RemoveRange(cartItems);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return Ok(response);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Error saving order: " + ex.Message });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.GetBaseException().Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"VNPAY Payment failed: {response?.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            var userId = await GetOrCreateUserIdAsync();
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var fullName = TempData["Checkout_FullName"] as string;
            var address = TempData["Checkout_Address"] as string;
            var phoneStr = TempData["Checkout_PhoneNumber"] as string;
            var notes = TempData["Checkout_Notes"] as string;
            var couponCode = TempData["Checkout_CouponCode"] as string;
            int? phone = null;
            if (int.TryParse(phoneStr, out var parsedPhone))
            {
                phone = parsedPhone;
            }

            var originalTotal = cartItems.Sum(p => (p.Product?.Price ?? 0m) * p.Quantity);
            var finalTotal = originalTotal;
            string? appliedCouponId = null;
            Coupon? appliedCoupon = null;

            // Validate and apply coupon if present
            if (!string.IsNullOrEmpty(couponCode))
            {
                appliedCoupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponCode == couponCode && c.IsActive == true);
                if (appliedCoupon != null && appliedCoupon.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now))
                {
                    if (!appliedCoupon.MinOrderValue.HasValue || originalTotal >= appliedCoupon.MinOrderValue.Value)
                    {
                        decimal discount = 0;
                        if (appliedCoupon.DiscountType == "percentage")
                        {
                            discount = originalTotal * (appliedCoupon.DiscountAmount / 100m);
                            if (appliedCoupon.MaxDiscount.HasValue && discount > appliedCoupon.MaxDiscount.Value)
                            {
                                discount = appliedCoupon.MaxDiscount.Value;
                            }
                        }
                        else
                        {
                            discount = appliedCoupon.DiscountAmount;
                        }
                        finalTotal = Math.Max(0, originalTotal - discount);
                        appliedCouponId = appliedCoupon.CouponId;
                    }
                }
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            var order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                UserId = userId,
                FullName = fullName ?? user?.FullName,
                Address = address ?? user?.Address,
                PhoneNumber = phone ?? user?.PhoneNumber,
                OrderDate = DateTime.UtcNow,
                ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)),
                PaymentMethod = "VN PAY",
                ShippingMethod = "Standard",
                ShipperId = "1",
                StatusId = "2", // Paid / Processing
                Notes = notes,
                TotalAmount = finalTotal,
                CouponId = appliedCouponId
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (appliedCoupon != null)
                    {
                        appliedCoupon.Quantity = Math.Max(0, appliedCoupon.Quantity - 1);
                        _context.Update(appliedCoupon);
                    }

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    var orderDetails = cartItems.Select(item => new OrderDetail
                    {
                        OrderDetailId = Guid.NewGuid().ToString(),
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        SubTotal = (item.Product?.Price ?? 0m) * item.Quantity
                    }).ToList();

                    _context.OrderDetails.AddRange(orderDetails);
                    
                    _context.Carts.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    TempData["Message"] = "VNPAY Payment successful!";
                    return RedirectToAction("PaymentSuccess");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Message"] = "VNPAY Payment succeeded but error occurred saving order: " + ex.Message;
                    return RedirectToAction("PaymentFail");
                }
            }
        }

        [HttpGet]
        public IActionResult PaymentFail()
        {
            ViewBag.Message = TempData["Message"] ?? "Payment failed.";
            return View();
        }

        [HttpGet]
        public IActionResult PaymentSuccess()
        {
            ViewBag.Message = TempData["Message"] ?? "Payment completed successfully!";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMiniCart()
        {
            var userId = await GetOrCreateUserIdAsync();
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .ThenInclude(p => p.Brand)
                .Where(c => c.UserId == userId)
                .ToListAsync();
            return PartialView("_MiniCartPartial", cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode, decimal orderTotal)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                return Json(new { success = false, message = "Please enter a coupon code." });
            }

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponCode == couponCode && c.IsActive == true);
            if (coupon == null)
            {
                return Json(new { success = false, message = "Coupon code is invalid or inactive." });
            }

            if (coupon.ExpiryDate < DateOnly.FromDateTime(DateTime.Now))
            {
                return Json(new { success = false, message = "Coupon code has expired." });
            }

            if (coupon.MinOrderValue.HasValue && orderTotal < coupon.MinOrderValue.Value)
            {
                return Json(new { success = false, message = $"Minimum order value of ${coupon.MinOrderValue.Value:0.00} required." });
            }

            if (coupon.Quantity <= 0)
            {
                return Json(new { success = false, message = "Coupon code is fully redeemed." });
            }

            decimal discount = 0;
            if (coupon.DiscountType == "percentage")
            {
                discount = orderTotal * (coupon.DiscountAmount / 100m);
                if (coupon.MaxDiscount.HasValue && discount > coupon.MaxDiscount.Value)
                {
                    discount = coupon.MaxDiscount.Value;
                }
            }
            else // fixed
            {
                discount = coupon.DiscountAmount;
            }

            return Json(new { 
                success = true, 
                discount, 
                couponId = coupon.CouponId, 
                discountText = coupon.DiscountType == "percentage" ? $"{coupon.DiscountAmount:0}% Off" : $"-${coupon.DiscountAmount:0.00}" 
            });
        }
    }
}
