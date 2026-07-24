using Microsoft.AspNetCore.SignalR;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Hubs;

namespace TaskFlow.Api.Services;

public class BoardNotificationService(IHubContext<TaskBoardHub> hubContext)
{
    public Task NotifyTaskChangedAsync(Guid projectId, TaskDto task)
        => hubContext.Clients.Group(TaskBoardHub.ProjectGroup(projectId.ToString()))
            .SendAsync(BoardEvents.TaskChanged, task);

    public Task NotifyTaskDeletedAsync(Guid projectId, Guid taskId)
        => hubContext.Clients.Group(TaskBoardHub.ProjectGroup(projectId.ToString()))
            .SendAsync(BoardEvents.TaskDeleted, taskId);

    public Task NotifyActivityCreatedAsync(Guid projectId, ActivityDto activity)
        => hubContext.Clients.Group(TaskBoardHub.ProjectGroup(projectId.ToString()))
            .SendAsync(BoardEvents.ActivityCreated, activity);

    public Task NotifyAnalyticsChangedAsync(Guid projectId)
        => hubContext.Clients.Group(TaskBoardHub.ProjectGroup(projectId.ToString()))
            .SendAsync(BoardEvents.AnalyticsChanged, projectId);
}
