using AdminPartDevelop.Models;

namespace AdminPartDevelop.Views.ViewModels
{
    public class MainViewModel
    {
        public required List<RefereeTabsViewModel> Referees { get; set; }
        public required List<MatchViewModel> Matches { get; set; }

    }
}
