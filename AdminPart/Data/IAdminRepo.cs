using AdminPart.Common;
using AdminPart.DTOs;
using AdminPart.Models;
using AdminPart.Views.ViewModels;
using System.Security.Cryptography;

namespace AdminPart.Data
{
        /// <summary>
        /// Interface for managing administrative data in the repository.
        /// </summary>
        public interface IAdminRepo
        {
            /// <summary>
            /// Adds a list of matches to the database.
            /// </summary>
            /// <param name="listOfMatches">The list of match entities to add.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> AddMatchesAsync(List<Match> listOfMatches);

            /// <summary>
            /// Adds a veto record to the database.
            /// </summary>
            /// <param name="vetoToAdd">The veto entity to add.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> AddVeto(Veto vetoToAdd);

            /// <summary>
            /// Adds a transfer record to the database.
            /// </summary>
            /// <param name="transfer">The transfer entity to add.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> AddTransfer(Transfer transfer);

            /// <summary>
            /// Adds a new file containing previous delegation information.
            /// </summary>
            /// <param name="filePath">The path to the file.</param>
            /// <param name="amountOfMatches">The number of matches in the file.</param>
            /// <param name="matchesFrom">The start date of matches in the file (optional).</param>
            /// <param name="matchesTo">The end date of matches in the file (optional).</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            RepositoryResponse AddNewFilePreviousDelegation(string filePath, int amountOfMatches, DateOnly? matchesFrom, DateOnly? matchesTo);

            /// <summary>
            /// Assigns a referee to a match with a specified role.
            /// </summary>
            /// <param name="refereeId">The ID of the referee to assign.</param>
            /// <param name="matchId">The ID of the match.</param>
            /// <param name="role">The role ID of the referee in the match.</param>
	    /// <param name="user">The user who is trying to do the action.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> AddRefereeToTheMatch(int refereeId, string matchId, int role,string user);

            /// <summary>
            /// Retrieves a match by its ID.
            /// </summary>
            /// <param name="id">The ID of the match to retrieve.</param>
            /// <returns>A repository result containing the match if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<Models.Match>> GetMatchByIdAsync(string id);

            /// <summary>
            /// Retrieves all matches with referee information.
            /// </summary>
            /// <param name="nameOfReferees">A dictionary mapping referee IDs to their names.</param>
            /// <returns>A repository result containing a list of match view models if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<MatchViewModel>>> GetMatchesAsync(Dictionary<int, string> nameOfReferees);

            /// <summary>
            /// Retrieves matches within a date range with referee information.
            /// </summary>
            /// <param name="dictNameOfReferees">A dictionary mapping referee IDs to their names.</param>
            /// <param name="startDate">The start date for filtering matches.</param>
            /// <param name="endDate">The end date for filtering matches.</param>
            /// <returns>A repository result containing a list of match view models if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<MatchViewModel>>> GetMatchesByDateAsync(Dictionary<int, string> dictNameOfReferees, DateTime startDate, DateTime endDate);

            /// <summary>
            /// Retrieves all matches without additional view model information.
            /// </summary>
            /// <returns>A repository result containing a list of match entities if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<Models.Match>>> GetPureMatchesAsync();

            /// <summary>
            /// Retrieves all matches that have not been played yet.
            /// </summary>
            /// <returns>A repository result containing a list of match entities if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<Models.Match>>> GetPureNotPlayedMatchesAsync();

            /// <summary>
            /// Retrieves all vetoes for a specific referee.
            /// </summary>
            /// <param name="refereeId">The ID of the referee.</param>
            /// <returns>A repository result containing a list of veto entities if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<Models.Veto>>> GetRefereesVetoesAsync(int refereeId);

            /// <summary>
            /// Retrieves all transfers for a specific referee.
            /// </summary>
            /// <param name="refereeId">The ID of the referee.</param>
            /// <returns>A repository result containing a list of transfer entities if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<Models.Transfer>>> GetRefereesTransfersAsync(int refereeId);

            /// <summary>
            /// Gets the start date of the game season.
            /// </summary>
            /// <returns>A repository result containing the start date if successful, or an error message if the operation fails.</returns>
            RepositoryResult<DateOnly> GetStartGameDate();

            /// <summary>
            /// Retrieves all matches for a specific team and competition.
            /// </summary>
            /// <param name="teamId">The ID of the team.</param>
            /// <param name="competitionId">The ID of the competition.</param>
            /// <returns>A repository result containing a list of match entities if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<Models.Match>>> GetTeamsPureMatchesAsync(string teamId, string competitionId);

            /// <summary>
            /// Searches for teams based on input text.
            /// </summary>
            /// <param name="input">The search text.</param>
            /// <returns>A repository result containing a list of team entities if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<Models.Team>>> GetTeamsByInput(string input);

            /// <summary>
            /// Gets an existing team or creates a new one if it doesn't exist.
            /// </summary>
            /// <param name="id">The ID of the team.</param>
            /// <param name="name">The name of the team.</param>
            /// <returns>A repository result containing the team entity if successful, or an error message if the operation fails.</returns>
            RepositoryResult<Team> GetOrSaveTheTeam(string id, string name);

            /// <summary>
            /// Retrieves all transfers occurring within a specific game weekend.
            /// </summary>
            /// <param name="startDayOfWeekend">The start date of the weekend.</param>
            /// <returns>A repository result containing a list of transfer entities if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<Models.Transfer>>> GetTransfersWithinGameWeekend(DateTime startDayOfWeekend);

            /// <summary>
            /// Retrieves all fields from the database.
            /// </summary>
            /// <returns>A repository result containing a list of field entities if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<Field>>> GetFields();

            /// <summary>
            /// Gets an existing field or creates a new one if it doesn't exist.
            /// </summary>
            /// <param name="fieldName">The name of the field.</param>
            /// <returns>A repository result containing the field entity if successful, or an error message if the operation fails.</returns>
            RepositoryResult<Field> GetOrSaveTheField(string fieldName);

            /// <summary>
            /// Retrieves all competitions from the database.
            /// </summary>
            /// <returns>A repository result containing a list of competition entities if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<Competition>>> GetCompetitions();

            /// <summary>
            /// Retrieves files containing previous delegation information.
            /// </summary>
            /// <returns>A repository result containing a list of files with previous matches if successful, or an error message if the operation fails.</returns>
            Task<RepositoryResult<List<FilesPreviousDelegation>>> GetFilesWithPreviousMatchesAsync();

            /// <summary>
            /// Updates a match in the database.
            /// </summary>
            /// <param name="match">The updated match entity.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> UpdateMatchAsync(Models.Match match);

            /// <summary>
            /// Updates the pre-match relationship for a match.
            /// </summary>
            /// <param name="existingMatch">The existing match entity.</param>
            /// <param name="newPreMatchId">The ID of the new pre-match to link.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> UpdatePreMatchRelationship(Models.Match existingMatch, string newPreMatchId);

            /// <summary>
            /// Updates the post-match relationship for a match.
            /// </summary>
            /// <param name="existingMatch">The existing match entity.</param>
            /// <param name="newPostMatchId">The ID of the new post-match to link.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> UpdatePostMatchRelationship(Models.Match existingMatch, string newPostMatchId);

            /// <summary>
            /// Updates multiple matches in the database.
            /// </summary>
            /// <param name="listOfMatches">The list of updated match entities.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> UpdateMatchesAsync(List<Match> listOfMatches);

            /// <summary>
            /// Toggles the lock status of a match.
            /// </summary>
            /// <param name="id">The ID of the match to update.</param>
	    /// <param name="user">The user who is trying to do the action.</param>
            /// <returns>A repository result containing a boolean indicating success and the new lock status, or an error message if the operation fails.</returns>
            Task<RepositoryResult<bool>> UpdateMatchLockAsync(string id,string user);

            /// <summary>
            /// Marks a match as played.
            /// </summary>
            /// <param name="id">The ID of the match to update.</param>
            /// <param name="user">The user who is trying to do the action.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> UpdateMatchPlayedAsync(string id,string user);

            /// <summary>
            /// Links matches with referees and updates the database.
            /// </summary>
            /// <param name="listOfMatches">The list of filled match DTOs.</param>
            /// <param name="refereeDict">A dictionary mapping referee IDs to their internal IDs.</param>
            /// <param name="filePath">The path to the file containing match information.</param>
            /// <param name="user">The user who is trying to do the action.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> TieAndUpdateTheMatchesAsync(List<FilledMatchDto> listOfMatches, Dictionary<string, int> refereeDict, string filePath,string user);

            /// <summary>
            /// Updates multiple fields in the database.
            /// </summary>
            /// <param name="listOfFields">The list of filled field DTOs.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> UpdateFieldsAsync(List<FilledFieldDto> listOfFields);

            /// <summary>
            /// Updates the properties of an existing field.
            /// </summary>
            /// <param name="fieldToUpdate">The field update DTO containing the new values.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            RepositoryResponse UpdateExistingField(FieldToUpdateDto fieldToUpdate);

            /// <summary>
            /// Updates the note for an existing veto.
            /// </summary>
            /// <param name="id">The ID of the veto to update.</param>
            /// <param name="note">The new note text.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            RepositoryResponse UpdateExistingVeto(int id, string note);

            /// <summary>
            /// Sets the start date of the game season.
            /// </summary>
            /// <param name="date">The start date to set.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            RepositoryResponse UploadStartGameDate(DateOnly date);

            /// <summary>
            /// Deletes a veto from the database.
            /// </summary>
            /// <param name="id">The ID of the veto to delete.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            RepositoryResponse DeleteVeto(int id);

            /// <summary>
            /// Removes a referee from a match.
            /// </summary>
            /// <param name="refereeId">The ID of the referee to remove.</param>
            /// <param name="matchId">The ID of the match.</param>
            /// <param name="user">The user who is trying to do the action.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> RemoveRefereeFromTheMatch(int refereeId, string matchId,string user);

            /// <summary>
            /// Removes transfers connected to a specific referee and match.
            /// </summary>
            /// <param name="refereeId">The ID of the referee.</param>
            /// <param name="matchId">The ID of the match.</param>
            /// <returns>A repository response indicating success or failure with an appropriate message.</returns>
            Task<RepositoryResponse> RemoveTransfersConnectedTo(int refereeId, string matchId);

            /// <summary>
            /// Checks if a competition exists in the database.
            /// </summary>
            /// <param name="idOfCompetition">The ID of the competition to check.</param>
            /// <returns>A repository result containing the competition if it exists, or an error message if it doesn't.</returns>
            RepositoryResult<Competition> DoesCompetitionExist(string idOfCompetition);

            /// <summary>
            /// Checks if a match exists in the database.
            /// </summary>
            /// <param name="matchId">The ID of the match to check.</param>
            /// <returns>A repository response indicating whether the match exists.</returns>
            RepositoryResponse DoesMatchExists(string matchId);

            /// <summary>
            /// Checks if a veto exists for a specific match and referee.
            /// </summary>
            /// <param name="matchId">The ID of the match.</param>
            /// <param name="refereeId">The ID of the referee.</param>
            /// <returns>A repository result containing a boolean indicating whether the veto exists, or an error message if the operation fails.</returns>
            Task<RepositoryResult<bool>> DoesVetoExist(string matchId, int refereeId);

            /// <summary>
            /// Checks if a veto exists for a specific team, competition, and referee.
            /// </summary>
            /// <param name="teamId">The ID of the team.</param>
            /// <param name="competitionId">The ID of the competition.</param>
            /// <param name="refereeId">The ID of the referee.</param>
            /// <returns>A repository result containing a boolean indicating whether the veto exists, or an error message if the operation fails.</returns>
            RepositoryResult<bool> DoesVetoExistForTeam(string teamId, string competitionId, int refereeId);
        }

}
