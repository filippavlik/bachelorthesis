using Microsoft.AspNet.SignalR;
using AdminPart.Views.ViewModels;
using Microsoft.AspNetCore.SignalR;
namespace AdminPart.Hubs
{
    public class HubForReendering : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task AnnounceChangeMatchAdd(string matchId, int refereeId,string RefereeName, int role)
        {
            await Clients.All.SendAsync("AcceptChangeMatchAdd", matchId,refereeId, RefereeName, role);
        }
        public async Task AnnounceChangeMatchRemove(string matchId, int refereeId)
        {
            await Clients.All.SendAsync("AcceptChangeMatchRemove", matchId, refereeId);
        }
        public async Task AnnounceChangeReferee(int refereeId, RefereeWithTimeOptions refereeDataToRerender)
        {
            await Clients.All.SendAsync("AcceptChangeReferee", refereeId, refereeDataToRerender);
        }
        public async Task AnnounceMatchLockUpdate( string matchId,bool isLockedNow) 
        {
            await Clients.All.SendAsync("AcceptMatchLockUpdate", matchId, isLockedNow);
        }



    }
}
