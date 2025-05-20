using AdminPart.Models;

namespace AdminPart.Views.ViewModels
{
    public class MainViewModel
    {
        public required List<RefereeTabsViewModel> Referees { get; set; }
        public required List<MatchViewModel> Matches { get; set; }

    }
}
