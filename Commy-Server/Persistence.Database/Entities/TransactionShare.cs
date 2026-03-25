using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class TransactionShare
{
    public long CommunityId { get; set; }

    public int? OurShareId { get; set; }

    public int? CommunityShareId { get; set; }

    public int? UserShareId { get; set; }

    public int? CommunityManagerShareId { get; set; }

    public virtual Community Community { get; set; } = null!;

    public virtual CommunityManagerShare? CommunityManagerShare { get; set; }

    public virtual CommunityShare? CommunityShare { get; set; }

    public virtual OurShare? OurShare { get; set; }

    public virtual UserShare? UserShare { get; set; }
}
