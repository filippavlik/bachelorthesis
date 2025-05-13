using AdminPart.Models;

namespace AdminPart.Views.ViewModels
{
    public class MatchViewModel
    {
        public required Match Match { get; set; }
        public required string CompetitionName { get; set; }
        public required string FieldName { get; set; }
        public required string HomeTeamName { get; set; }
        public required string AwayTeamName { get; set; }
        
        public string? RefereeName { get; set; }
        public string? Ar1Name { get; set; }
        public string? Ar2Name { get; set; }
        public string? WeekendPartColor { get; set; }
    }
}
