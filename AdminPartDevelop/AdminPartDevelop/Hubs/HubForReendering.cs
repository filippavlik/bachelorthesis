using Microsoft.AspNet.SignalR;
using AdminPart.Views.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace AdminPart.Hubs
{
    /// <summary>
    /// Hub responsible for real-time updates across clients when match or referee data changes.
    /// </summary>
    public class HubForReendering : Microsoft.AspNetCore.SignalR.Hub
    {
        /// <summary>
        /// Notifies all clients when a referee is added to a match.
        /// </summary>
        /// <remarks>
        ///	Change specified match graphically , add button with referee name , change the last change to user and time of the change.
        /// </remarks>
        /// <param name="matchId">The unique identifier of the match.</param>
        /// <param name="refereeId">The unique identifier of the referee being added.</param>
        /// <param name="RefereeName">The name of the referee being added.</param>
        /// <param name="role">The role assigned to the referee in this match.</param>
        /// <param name="user">The user who made the changes.</param>
        /// <param name="timestamp">When the user made the changes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AnnounceChangeMatchAdd(string matchId, int refereeId, string RefereeName, int role, string user, DateTime timestamp)
        {
            await Clients.All.SendAsync("AcceptChangeMatchAdd", matchId, refereeId, RefereeName, role, user, timestamp);
        }

        /// <summary>
        /// Notifies all clients when a referee is removed from a match.
        /// </summary>
        /// <remarks>
        ///     Change specified match graphically , remove button from match pane, change the last change to user and time of the change.
        /// </remarks>
        /// <param name="matchId">The unique identifier of the match.</param>
        /// <param name="refereeId">The unique identifier of the referee being removed.</param>
        /// <param name="user">The user who made the changes.</param>
        /// <param name="timestamp">When the user made the changes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AnnounceChangeMatchRemove(string matchId, int refereeId, string user, DateTime timestamp)
        {
            await Clients.All.SendAsync("AcceptChangeMatchRemove", matchId, refereeId);
        }

        /// <summary>
        /// Notifies all clients when referee data has been updated.
        /// </summary>
        /// <param name="refereeId">The unique identifier of the updated referee.</param>
        /// <param name="refereeDataToRerender">The updated referee data including availability information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AnnounceChangeReferee(int refereeId, RefereeWithTimeOptions refereeDataToRerender)
        {
            await Clients.All.SendAsync("AcceptChangeReferee", refereeId, refereeDataToRerender);
        }

        /// <summary>
        /// Notifies all clients when the lock status of a match has changed.
        /// </summary>
        /// <param name="matchId">The unique identifier of the match.</param>
        /// <param name="isLockedNow">The new lock status of the match (true if locked, false if unlocked).</param>
        /// <param name="user">The user who made the changes.</param>
        /// <param name="timestamp">When the user made the changes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AnnounceMatchLockUpdate(string matchId, bool isLockedNow, string user, DateTime timestamp)
        {
            await Clients.All.SendAsync("AcceptMatchLockUpdate", matchId, isLockedNow, user, timestamp);
        }
    }
}
