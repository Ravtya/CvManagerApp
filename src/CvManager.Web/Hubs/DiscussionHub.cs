using Microsoft.AspNetCore.SignalR;

namespace CvManager.Web.Hubs;

public class DiscussionHub : Hub
{
    public static string GroupName(int positionId) => $"position-{positionId}";

    public Task JoinPosition(int positionId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(positionId));
}
