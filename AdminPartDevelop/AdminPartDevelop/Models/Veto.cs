using System;
using System.Collections.Generic;

namespace AdminPartDevelop.Models;

public partial class Veto
{
    public int VetoId { get; set; }

    public string CompetitionId { get; set; } = null!;

    public string TeamId { get; set; } = null!;

    public int RefereeId { get; set; }

    public string? Note { get; set; }

    public virtual Competition Competition { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
