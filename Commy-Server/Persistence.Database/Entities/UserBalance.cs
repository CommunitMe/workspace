using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class UserBalance
{
    public long ProfileId { get; set; }

    public decimal CurrentBalance { get; set; }

    public virtual Profile Profile { get; set; } = null!;
}
