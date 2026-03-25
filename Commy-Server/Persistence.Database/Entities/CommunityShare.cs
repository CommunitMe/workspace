using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class CommunityShare
{
    public int Id { get; set; }

    public int Type { get; set; }

    public decimal Value { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<TransactionShare> TransactionShares { get; } = new List<TransactionShare>();

    public virtual ShareType TypeNavigation { get; set; } = null!;
}
