using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class Coupon
{
    public long Id { get; set; }

    public long RelevantCommunity { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public long Category { get; set; }

    public long ServiceProviderId { get; set; }

    public string? LocationName { get; set; }

    public int? Amount { get; set; }

    public DateTime? Expiration { get; set; }

    public bool IsActive { get; set; }

    public string ImageUid { get; set; } = null!;

    public virtual CategoryTree CategoryNavigation { get; set; } = null!;

    public virtual Community RelevantCommunityNavigation { get; set; } = null!;

    public virtual ServiceProvider ServiceProvider { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; } = new List<Tag>();
}
