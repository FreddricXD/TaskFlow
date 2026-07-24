using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain.Entities;
using TaskFlow.Api.Dtos;

namespace TaskFlow.Api.Services;

public class ActivityService(TaskFlowDbContext db, ProjectAccessService accessService, BoardNotificationService notifications)
{
    public async Task<IReadOnlyList<ActivityDto>> GetActivitiesAsync(Guid userId, Guid projectId, int take = 25)
    {
        await accessService.EnsureMemberAsync(userId, projectId);

        return await db.ActivityLogs
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .Select(a => new ActivityDto(
                a.Id,
                a.ProjectId,
                a.UserId,
                a.User.DisplayName,
                a.EntityType,
                a.EntityId,
                a.Action,
                a.Description,
                a.CreatedAt))
            .ToListAsync();
    }

    public async Task<ActivityDto> LogAsync(Guid projectId, Guid userId, string entityType, Guid entityId, string action, string description)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found.");

        var activity = new ActivityLog
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        db.ActivityLogs.Add(activity);
        await db.SaveChangesAsync();

        var dto = new ActivityDto(
            activity.Id,
            activity.ProjectId,
            activity.UserId,
            user.DisplayName,
            activity.EntityType,
            activity.EntityId,
            activity.Action,
            activity.Description,
            activity.CreatedAt);

        await notifications.NotifyActivityCreatedAsync(projectId, dto);
        await notifications.NotifyAnalyticsChangedAsync(projectId);

        return dto;
    }
}
