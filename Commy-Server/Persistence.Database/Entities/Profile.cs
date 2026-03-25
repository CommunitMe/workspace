using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Persistence.Database.Entities;

public partial class Profile : IdentityUser<long>
{
    public string GivenName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string FamilyName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? State { get; set; }

    public string PostalCode { get; set; } = null!;

    public DateTime Birthday { get; set; }

    public long DefaultCommunity { get; set; }

    public long? ServiceProvider { get; set; }

    public string? ImageUid { get; set; }

    public DateTime LastOpenNotifications { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual ICollection<Activity> Activities { get; } = new List<Activity>();

    public virtual ICollection<CommunityProfile> CommunityProfiles { get; } = new List<CommunityProfile>();

    public virtual Community DefaultCommunityNavigation { get; set; } = null!;

    public virtual ICollection<Invitation> Invitations { get; } = new List<Invitation>();

    public virtual ICollection<MarketItem> MarketItems { get; } = new List<MarketItem>();

    public virtual ServiceProvider? ServiceProviderNavigation { get; set; }

    public virtual ICollection<Transaction> Transactions { get; } = new List<Transaction>();

    public virtual UserBalance? UserBalance { get; set; }

    public virtual ICollection<Event> Events { get; } = new List<Event>();
}
