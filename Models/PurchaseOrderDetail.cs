using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class PurchaseOrderDetail
{
    public string? PurchaseOrderDetailId { get; set; }

    public string? PurchaseOrderId { get; set; }

    public string? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public virtual Product? Product { get; set; }

    public virtual PurchaseOrder? PurchaseOrder { get; set; }
}
