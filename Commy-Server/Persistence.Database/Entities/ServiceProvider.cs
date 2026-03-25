using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class ServiceProvider
{
    public long Id { get; set; }

    public int ServiceProviderType { get; set; }

    public string? Name { get; set; }

    public string? ImageUid { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Coupon> Coupons { get; } = new List<Coupon>();

    public virtual ICollection<Profile> Profiles { get; } = new List<Profile>();

    public virtual ICollection<ServiceType> ServiceTypes { get; } = new List<ServiceType>();
}
