using AdminPart.Models;

namespace AdminPart.Views.ViewModels
{
    public class RefereeCardViewModel
    {
        public required List<Veto> Vetoes { get; set; }
        public required RefereeWithTimeOptions RefereeWTimeOptions { get; set; }
    }
}
