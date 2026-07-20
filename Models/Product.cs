using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class Product
{
    public string ProductId { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string SlugName { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? Image { get; set; }

    public string? Description { get; set; }

    public string? CategoryId { get; set; }

    public string? BrandId { get; set; }

    public string? CouponId { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Category { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
