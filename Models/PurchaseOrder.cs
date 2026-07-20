using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class PurchaseOrder
{
    public string PurchaseOrderId { get; set; } = null!;

    public string? SupplierId { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal? TotalAmount { get; set; }

    public virtual Supplier? Supplier { get; set; }
}
