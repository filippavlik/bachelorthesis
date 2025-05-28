namespace AdminPartDevelop.DTOs
{
    public class RefereesTeamsMatchesRequestDto
    {
        public List<int> RefereeIds { get; set; }
        public string TeamId { get; set; }
        public bool IsReferee { get; set; }
        public string CompetitionId { get; set; }
    }
}
