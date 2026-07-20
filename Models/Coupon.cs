using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class Coupon
{
    public string CouponId { get; set; } = null!;

    public string CouponCode { get; set; } = null!;

    public decimal DiscountAmount { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public int Quantity { get; set; }

    public bool? IsActive { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal? MaxDiscount { get; set; }

    public decimal? MinOrderValue { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
