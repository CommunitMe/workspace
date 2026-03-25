using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class Tag
{
    public long Id { get; set; }

    public string Text { get; set; } = null!;

    public virtual ICollection<Coupon> Coupons { get; } = new List<Coupon>();

    public virtual ICollection<MarketItem> Items { get; } = new List<MarketItem>();
}
