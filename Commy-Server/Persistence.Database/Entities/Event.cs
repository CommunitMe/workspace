using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class Event
{
    public long Id { get; set; }

    public long RelevantCommunity { get; set; }

    public string Name { get; set; } = null!;

    public string Location { get; set; } = null!;

    public DateTime EventTime { get; set; }

    public string ImageUid { get; set; } = null!;

    public virtual Community RelevantCommunityNavigation { get; set; } = null!;

    public virtual ICollection<Profile> Profiles { get; } = new List<Profile>();
}
