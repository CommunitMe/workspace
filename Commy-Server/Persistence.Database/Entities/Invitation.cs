using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class Invitation
{
    public Guid Id { get; set; }

    public long ClusteredIndex { get; set; }

    public int InvitationType { get; set; }

    public DateTime CreationTime { get; set; }

    public long CommunityId { get; set; }

    public long CreatorId { get; set; }

    public DateTime? Expiry { get; set; }

    public int? AllowedUsages { get; set; }

    public virtual Community Community { get; set; } = null!;

    public virtual Profile Creator { get; set; } = null!;

    public virtual ICollection<InvitationsLink> InvitationsLinks { get; } = new List<InvitationsLink>();
}
