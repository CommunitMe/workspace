using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class CategoryTree
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ImageUid { get; set; }

    public long? Parent { get; set; }

    public virtual ICollection<Coupon> Coupons { get; } = new List<Coupon>();

    public virtual ICollection<CategoryTree> InverseParentNavigation { get; } = new List<CategoryTree>();

    public virtual ICollection<MarketItem> MarketItems { get; } = new List<MarketItem>();

    public virtual CategoryTree? ParentNavigation { get; set; }

    public virtual ICollection<ServiceType> ServiceTypes { get; } = new List<ServiceType>();
}
