using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class MarketItem
{
    public long Id { get; set; }

    public long Owner { get; set; }

    public long RelevantCommunity { get; set; }

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public short PriceCurrency { get; set; }

    public string ImageUid { get; set; } = null!;

    public long Category { get; set; }

    public virtual ICollection<Activity> Activities { get; } = new List<Activity>();

    public virtual CategoryTree CategoryNavigation { get; set; } = null!;

    public virtual Profile OwnerNavigation { get; set; } = null!;

    public virtual Currency PriceCurrencyNavigation { get; set; } = null!;

    public virtual Community RelevantCommunityNavigation { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; } = new List<Tag>();
}
