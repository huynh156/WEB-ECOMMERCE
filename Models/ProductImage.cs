using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class ProductImage
{
    public string ProductImageId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string ImagePath { get; set; } = null!;

    public bool IsMain { get; set; }

    public int SortOrder { get; set; }

    public virtual Product Product { get; set; } = null!;
}
