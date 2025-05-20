using AdminPart.Models;

namespace AdminPart.Views.ViewModels
{
    public class RefereeTabsViewModel
    {       
            public int LeagueId { get; set; }
            public List<RefereeWithTimeOptions> Referees { get; set; }
    }
}
