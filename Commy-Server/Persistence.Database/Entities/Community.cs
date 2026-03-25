using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class Community
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string ImageUid { get; set; } = null!;

    public virtual ICollection<Activity> Activities { get; } = new List<Activity>();

    public virtual ICollection<CommunityBalance> CommunityBalances { get; } = new List<CommunityBalance>();

    public virtual ICollection<CommunityProfile> CommunityProfiles { get; } = new List<CommunityProfile>();

    public virtual ICollection<CommunitySetting> CommunitySettings { get; } = new List<CommunitySetting>();

    public virtual ICollection<Coupon> Coupons { get; } = new List<Coupon>();

    public virtual ICollection<Event> Events { get; } = new List<Event>();

    public virtual ICollection<Invitation> Invitations { get; } = new List<Invitation>();

    public virtual ICollection<MarketItem> MarketItems { get; } = new List<MarketItem>();

    public virtual ICollection<Notification> Notifications { get; } = new List<Notification>();

    public virtual ICollection<Profile> Profiles { get; } = new List<Profile>();

    public virtual TransactionShare? TransactionShare { get; set; }

    public virtual ICollection<Transaction> Transactions { get; } = new List<Transaction>();
}
