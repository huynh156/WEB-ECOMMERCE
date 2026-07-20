using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class Brand
{
    public string BrandId { get; set; } = null!;

    public string BrandName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
