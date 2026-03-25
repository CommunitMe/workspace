using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class ServiceType
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public long Category { get; set; }

    public virtual ICollection<Activity> Activities { get; } = new List<Activity>();

    public virtual CategoryTree CategoryNavigation { get; set; } = null!;

    public virtual ICollection<ServiceProvider> ServiceProviders { get; } = new List<ServiceProvider>();
}
