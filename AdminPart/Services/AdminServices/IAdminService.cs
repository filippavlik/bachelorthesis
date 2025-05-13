using AdminPart.Common;
using AdminPart.DTOs;
using AdminPart.Models;
using AdminPart.Views.ViewModels;

namespace AdminPart.Services.AdminServices
{
    public interface IAdminService 
    {
        /// <summary>
        /// Calculates the percentage of matches that have been delegated to referees.
        /// </summary>
        /// <param name="matches">The list of matches to evaluate.</param>
        /// <returns>The percentage of delegated matches as an integer (0-100).</returns>
        public int GetPercentageOfDelegatedMatches(List<MatchViewModel> matches);
        /// <summary>
        /// Retrieves and compiles statistics about referees' involvement with a specific team's matches.
        /// </summary>
        /// <remarks>
        /// This asynchronous method analyzes the history of referee assignments for a specific team by:
        /// 1. Retrieving all matches for the specified team and competition
        /// 2. Calculating statistics based on whether the official served as main referee or assistant referee
        /// 3. Counting home and away matches for each referee
        /// 4. Checking for any existing veto records between the team and referees
        /// 5. Combining all data into comprehensive statistics objects
        /// 
        /// For vetoed referees, the method returns predefined values (6 for both home and away counts).
        /// </remarks>
        /// <param name="request">
        /// A DTO containing filtering parameters including:
        /// - TeamId: The ID of the team to analyze
        /// - CompetitionId: The ID of the competition to analyze
        /// - RefereeIds: Collection of referee IDs to include in the analysis
        /// - IsReferee: Flag indicating whether to consider main referee (true) or assistant referee (false) assignments
        /// </param>
        /// <returns>
        /// A ServiceResult containing either:
        /// - A success result with a list of RefereesTeamsMatchesResponseDto objects containing statistics, or
        /// - A failure result with an error message if processing failed
        /// </returns>
        /// <exception cref="Exception">May throw exceptions during data retrieval or processing</exception>
        public Task<ServiceResult<List<RefereesTeamsMatchesResponseDto>>> GetRefereeMatchStatsAsync(RefereesTeamsMatchesRequestDto request);

        /// <summary>
        /// Transforms a collection of unfilled match DTOs into fully-populated Match domain models.
        /// </summary>
        /// <remarks>
        /// This method processes each UnfilledMatchDto by:
        /// 1. Extracting and validating the competition code from the match number
        /// 2. Retrieving or creating the appropriate competition record
        /// 3. Retrieving or creating the field record
        /// 4. Validating and retrieving or creating team records for both home and away teams
        /// 5. Converting match date and time information from the DTO
        /// 6. Creating a timestamp using Central European Standard Time
        /// 7. Assembling all components into a complete Match entity
        /// </remarks>
        /// <param name="listOfMatches">A collection of UnfilledMatchDto objects containing raw match data</param>
        /// <returns>
        /// A ServiceResult containing either:
        /// - A success result with a list of fully-populated Match objects, or
        /// - A failure result with an error message if processing failed
        /// </returns>
        /// <exception cref="Exception">May throw exceptions during competition, field, or team processing</exception>
        public ServiceResult<List<Models.Match>> ProccessDtosToMatches(List<UnfilledMatchDto> listOfMatches,string user);

        /// <summary>
        /// Establishes sequential connections between matches scheduled on the same field.
        /// </summary>
        /// <remarks>
        /// This method creates a linked structure between consecutive matches by:
        /// 1. Sorting matches by field and start time
        /// 2. Analyzing each pair of consecutive matches on the same field
        /// 3. Calculating the expected end time of each match based on competition rules
        /// 4. Establishing bi-directional links (PreMatch/PostMatch) between matches when:
        ///    - They occur on the same field
        ///    - The second match starts within an acceptable time window after the first ends
        ///      (between reserveBetweenMatches and reserveToSecondMatch minutes)
        /// 
        /// The time window for valid connections is controlled by the class fields:
        /// - reserveBetweenMatches: Minimum minutes required between matches
        /// - reserveToSecondMatch: Maximum minutes allowed between matches
        /// </remarks>
        /// <param name="matches">A list of MatchViewModel objects to be processed and connected</param>
        /// <returns>
        /// A ServiceResult containing either:
        /// - A success result with the processed list of matches including connection information, or
        /// - A failure result with an error message if processing failed
        /// </returns>
        /// <exception cref="Exception">May throw exceptions during the match connection process</exception>
        public ServiceResult<List<MatchViewModel>> MakeConnectionsOfMatches(List<MatchViewModel> matches);

        /// <summary>
        /// Sorts the given matches based on the specified sort key.
        /// </summary>
        /// <param name="sortKey">The key indicating which property to sort by (e.g., "Field", "Time").</param>
        /// <param name="matches">The collection of matches to sort.</param>
        /// <returns>A ServiceResult containing a sorted list of MatchViewModel items.</returns>
        public ServiceResult<List<MatchViewModel>> SortMatches(string sortKey, IEnumerable<MatchViewModel> matches);

        /// <summary>
        /// Sorts matches using a provided selector function and sort direction.
        /// </summary>
        /// <typeparam name="TKey">The type of the key to sort by.</typeparam>
        /// <param name="matches">The collection of matches to sort.</param>
        /// <param name="selector">A function to select the key for sorting.</param>
        /// <param name="asc">Indicates whether the sorting should be ascending (true) or descending (false).</param>
        /// <returns>A sorted list of MatchViewModel items.</returns>
        public List<MatchViewModel> SortBy<TKey>(IEnumerable<MatchViewModel> matches, Func<MatchViewModel, TKey> selector, bool asc);

        /// <summary>
        /// Sorts matches by their assigned field.
        /// </summary>
        /// <param name="asc">True for ascending order; false for descending.</param>
        /// <param name="matches">The collection of matches to sort.</param>
        /// <returns>A sorted list of MatchViewModel items by field.</returns>
        public List<MatchViewModel> SortByField(bool asc, IEnumerable<MatchViewModel> matches);

        /// <summary>
        /// Sorts matches by their scheduled game time.
        /// </summary>
        /// <param name="asc">True for ascending order; false for descending.</param>
        /// <param name="matches">The collection of matches to sort.</param>
        /// <returns>A sorted list of MatchViewModel items by game time.</returns>
        public List<MatchViewModel> SortByGameTime(bool asc, IEnumerable<MatchViewModel> matches);

        /// <summary>
        /// Sorts matches by the home team name.
        /// </summary>
        /// <param name="asc">True for ascending order; false for descending.</param>
        /// <param name="matches">The collection of matches to sort.</param>
        /// <returns>A sorted list of MatchViewModel items by home team.</returns>
        public List<MatchViewModel> SortByHomeTeam(bool asc, IEnumerable<MatchViewModel> matches);

        /// <summary>
        /// Sorts matches by the away team name.
        /// </summary>
        /// <param name="asc">True for ascending order; false for descending.</param>
        /// <param name="matches">The collection of matches to sort.</param>
        /// <returns>A sorted list of MatchViewModel items by away team.</returns>
        public List<MatchViewModel> SortByAwayTeam(bool asc, IEnumerable<MatchViewModel> matches);

        /// <summary>
        /// Sorts matches by category (e.g., league or division).
        /// </summary>
        /// <param name="asc">True for ascending order; false for descending.</param>
        /// <param name="matches">The collection of matches to sort.</param>
        /// <returns>A sorted list of MatchViewModel items by category.</returns>
        public List<MatchViewModel> SortByCategory(bool asc, IEnumerable<MatchViewModel> matches);

        /// <summary>
        /// Filters and returns a list of matches that have not yet been delegated to referees.
        /// </summary>
        /// <param name="matches">The collection of matches to filter.</param>
        /// <returns>A list of undelegated MatchViewModel items.</returns>
        public List<MatchViewModel> SortByUndelegatedMatches(IEnumerable<MatchViewModel> matches);


    }

}
