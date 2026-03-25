using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class Notification
{
    public long Id { get; set; }

    public DateTime InsertTime { get; set; }

    public long RelevantCommunity { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual Community RelevantCommunityNavigation { get; set; } = null!;
}
