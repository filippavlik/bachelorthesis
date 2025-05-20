﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AdminPart.Models;

public partial class Competition
{
    public string CompetitionId { get; set; } = null!;

    public string CompetitionName { get; set; } = null!;

    public int MatchLength { get; set; }

    public int League { get; set; }

    [JsonIgnore]
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();

    [JsonIgnore]
    public virtual ICollection<Veto> Vetoes { get; set; } = new List<Veto>();
}
