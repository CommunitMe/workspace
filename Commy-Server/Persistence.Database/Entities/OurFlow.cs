using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class OurFlow
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
