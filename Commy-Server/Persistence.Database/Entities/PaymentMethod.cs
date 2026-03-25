using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class PaymentMethod
{
    public int Id { get; set; }

    public string PaymentMethod1 { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; } = new List<Transaction>();
}
