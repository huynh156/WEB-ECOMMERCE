using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class Status
{
    public string StatusId { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
