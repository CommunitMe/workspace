using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class ShareType
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<CommunityManagerShare> CommunityManagerShares { get; } = new List<CommunityManagerShare>();

    public virtual ICollection<CommunityShare> CommunityShares { get; } = new List<CommunityShare>();

    public virtual ICollection<OurShare> OurShares { get; } = new List<OurShare>();

    public virtual ICollection<UserShare> UserShares { get; } = new List<UserShare>();
}
