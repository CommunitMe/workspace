using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class CommunityBalance
{
    public long Id { get; set; }

    public long CommunityId { get; set; }

    public decimal CurrentBalance { get; set; }

    public virtual Community Community { get; set; } = null!;
}
