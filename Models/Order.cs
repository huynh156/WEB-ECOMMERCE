using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class Order
{
    public string OrderId { get; set; } = null!;

    public string? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Status { get; set; }

    public string? CouponId { get; set; }

    public DateOnly? ReceivedDate { get; set; }

    public DateOnly? ExpectedDeliveryDate { get; set; }

    public string? FullName { get; set; }

    public int? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? PaymentMethod { get; set; }

    public string? StatusId { get; set; }

    public string? ShipperId { get; set; }

    public string? ShippingMethod { get; set; }

    public string? Notes { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Shipper? Shipper { get; set; }

    public virtual Status? StatusNavigation { get; set; }

    public virtual User? User { get; set; }
}
