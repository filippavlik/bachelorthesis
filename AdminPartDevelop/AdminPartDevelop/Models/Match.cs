using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace AdminPart.Models;

public partial class Match
{
    public string MatchId { get; set; } = null!;

    public string CompetitionId { get; set; } = null!;

    public string HomeTeamId { get; set; } = null!;

    public string AwayTeamId { get; set; } = null!;

    public int FieldId { get; set; }

    public DateOnly MatchDate { get; set; }

    public TimeOnly MatchTime { get; set; }

    public string? PostMatch { get; set; }

    public string? PreMatch { get; set; }

    public int? RefereeId { get; set; }

    public int? Ar1Id { get; set; }

    public int? Ar2Id { get; set; }

    public string? Note { get; set; }

    public bool AlreadyPlayed { get; set; }

    public bool Locked { get; set; }

    public string? LastChangedBy { get; set; }

    public DateTime LastChanged { get; set; }

    public virtual Competition Competition { get; set; } = null!;

    public virtual Field Field { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
