using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

/// <summary>
/// Based of ISO-4217
/// </summary>
public partial class Currency
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public virtual ICollection<MarketItem> MarketItems { get; } = new List<MarketItem>();
}
