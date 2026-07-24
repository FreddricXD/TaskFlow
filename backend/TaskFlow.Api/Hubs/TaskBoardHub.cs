using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskFlow.Api.Hubs;

[Authorize]
public class TaskBoardHub : Hub
{
    public Task JoinProject(string projectId) => Groups.AddToGroupAsync(Context.ConnectionId, ProjectGroup(projectId));

    public Task LeaveProject(string projectId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, ProjectGroup(projectId));

    public static string ProjectGroup(string projectId) => $"project-{projectId}";
}
