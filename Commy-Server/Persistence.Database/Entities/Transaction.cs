using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class Transaction
{
    public long Id { get; set; }

    public long ProfileId { get; set; }

    public long CommunityId { get; set; }

    public long? ProductId { get; set; }

    public string Currency { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal OurShare { get; set; }

    public decimal CommunityShare { get; set; }

    public decimal UserShare { get; set; }

    public decimal? ServiceProviderShare { get; set; }

    public int PaymentMethod { get; set; }

    public DateTime Timestampt { get; set; }

    public virtual Community Community { get; set; } = null!;

    public virtual ICollection<OurFlow> OurFlows { get; } = new List<OurFlow>();

    public virtual PaymentMethod PaymentMethodNavigation { get; set; } = null!;

    public virtual Profile Profile { get; set; } = null!;
}
