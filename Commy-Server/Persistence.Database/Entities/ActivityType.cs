using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class ActivityType
{
    public byte Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Activity> Activities { get; } = new List<Activity>();
}
