using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class CategoryPromotion
{
    public string CategoryPromotion1 { get; set; } = null!;

    public string CategoryId { get; set; } = null!;

    public string PromotionId { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual Promotion Promotion { get; set; } = null!;
}
