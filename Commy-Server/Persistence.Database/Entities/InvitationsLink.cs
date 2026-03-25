using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class InvitationsLink
{
    public long Id { get; set; }

    public Guid InvitationId { get; set; }

    public int Usages { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual Invitation Invitation { get; set; } = null!;
}
