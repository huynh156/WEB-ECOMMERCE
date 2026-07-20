using System;
using System.Collections.Generic;

namespace FashionHubWeb.Models;

public partial class Promotion
{
    public string PromotionId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }
}
