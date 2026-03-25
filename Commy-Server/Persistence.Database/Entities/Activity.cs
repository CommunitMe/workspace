using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class Activity
{
    public long Id { get; set; }

    public DateTime InsertTime { get; set; }

    public byte Type { get; set; }

    public long? Profile { get; set; }

    public byte? ServiceDuration { get; set; }

    public long? MarketItem { get; set; }

    public long? Community { get; set; }

    public long? ServiceType { get; set; }

    public virtual Community? CommunityNavigation { get; set; }

    public virtual MarketItem? MarketItemNavigation { get; set; }

    public virtual Profile? ProfileNavigation { get; set; }

    public virtual ServiceDuration? ServiceDurationNavigation { get; set; }

    public virtual ServiceType? ServiceTypeNavigation { get; set; }

    public virtual ActivityType TypeNavigation { get; set; } = null!;
}
