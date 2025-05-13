namespace AdminPart.Views.ViewModels
{
    public class RefereeMatchPointsViewModel
    {
            public List<int> RefereeIds { get; set; } = new();
            public string HomeTeamId { get; set; } = null!;
            public string AwayTeamId { get; set; } = null!;
            public string MatchId { get; set; } = null!;
            public bool IsReferee { get; set; }      
    }
}
