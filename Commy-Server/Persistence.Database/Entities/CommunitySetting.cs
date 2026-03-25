using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

public partial class CommunitySetting
{
    public long Id { get; set; }

    public long CommunityId { get; set; }

    public string DefaultCurrency { get; set; } = null!;

    public string DefaultLanguage { get; set; } = null!;

    public int Privacy { get; set; }

    public string? CommunityUsername { get; set; }

    public int MembersApprovalPreferences { get; set; }

    public int SuppliersApprovalPreferences { get; set; }

    public string? Guidelines { get; set; }

    public int AgeRestriction { get; set; }

    public virtual Community Community { get; set; } = null!;
}
