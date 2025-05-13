namespace AdminPart.Views.ViewModels
{
    public class RefereeMatchCountViewModel
    {        
            public List<int> RefereeIds { get; set; } = new();
            public string TeamId { get; set; } = null!;
            public string CompetitionId { get; set; } = null!;
            public bool IsReferee { get; set; }      
    }
}
