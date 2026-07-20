using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class Shipper
{
    public string ShipperId { get; set; } = null!;

    public string ShipperName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
