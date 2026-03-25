using System;
using System.Collections.Generic;

namespace Persistence.Database.Entities;

/// <summary>
/// Based of ISO-4217
/// </summary>
public partial class Language
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;
}
