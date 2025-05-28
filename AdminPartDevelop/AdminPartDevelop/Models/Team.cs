using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace AdminPartDevelop.Models;

public partial class Team
{
    public string TeamId { get; set; } = null!;

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Veto> Vetoes { get; set; } = new List<Veto>();
    [JsonIgnore]
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
}
