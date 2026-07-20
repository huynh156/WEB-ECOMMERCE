using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class ProductPromotion
{
    public string ProductPromotionId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string PromotionId { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Promotion Promotion { get; set; } = null!;
}
