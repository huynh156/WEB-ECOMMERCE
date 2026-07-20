using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class Supplier
{
    public string SupplierId { get; set; } = null!;

    public string SupplierName { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Address { get; set; } = null!;

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
