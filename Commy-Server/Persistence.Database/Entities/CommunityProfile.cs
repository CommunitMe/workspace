using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class CommunityProfile
{
    public long CommunityId { get; set; }

    public long ProfileId { get; set; }

    public int MemberState { get; set; }

    public int ProviderState { get; set; }

    public virtual Community Community { get; set; } = null!;

    public virtual Profile Profile { get; set; } = null!;
}
