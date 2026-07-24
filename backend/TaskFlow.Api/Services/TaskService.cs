using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain.Entities;
using TaskFlow.Api.Domain.Enums;
using TaskFlow.Api.Dtos;

namespace TaskFlow.Api.Services;

public class TaskService(TaskFlowDbContext db, ProjectAccessService accessService, ActivityService activityService, BoardNotificationService notifications)
{
    public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(Guid userId, Guid projectId, string? search, string? status, string? priority)
    {
        await accessService.EnsureMemberAsync(userId, projectId);

        var query = db.Tasks
            .AsNoTracking()
            .Include(t => t.Assignee)
            .Include(t => t.Labels)
            .Where(t => t.ProjectId == projectId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BoardStatus>(status, true, out var statusFilter))
        {
            query = query.Where(t => t.Status == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TaskPriority>(priority, true, out var priorityFilter))
        {
            query = query.Where(t => t.Priority == priorityFilter);
        }

        var tasks = await query
            .OrderBy(t => t.Status)
            .ThenBy(t => t.SortOrder)
            .ThenByDescending(t => t.UpdatedAt)
            .ToListAsync();

        return tasks.Select(MapTask).ToList();
    }

    public async Task<TaskDto> CreateTaskAsync(Guid userId, Guid projectId, CreateTaskRequest request)
    {
        await accessService.EnsureMemberAsync(userId, projectId);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationException("Task title is required.");
        }

        var status = ParseStatus(request.Status);
        var priority = ParsePriority(request.Priority);
        await ValidateAssigneeAsync(projectId, request.AssigneeId);

        var sortOrder = await db.Tasks
            .Where(t => t.ProjectId == projectId && t.Status == status)
            .MaxAsync(t => (int?)t.SortOrder) ?? -1;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Status = status,
            Priority = priority,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate?.ToUniversalTime(),
            SortOrder = sortOrder + 1
        };

        db.Tasks.Add(task);
        AddLabels(task, request.Labels);
        await db.SaveChangesAsync();

        await activityService.LogAsync(projectId, userId, "Task", task.Id, "Created", $"Created task {task.Title}");
        var dto = await GetTaskDtoAsync(task.Id);
        await notifications.NotifyTaskChangedAsync(projectId, dto);
        return dto;
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid userId, Guid projectId, Guid taskId, UpdateTaskRequest request)
    {
        await accessService.EnsureMemberAsync(userId, projectId);

        var task = await db.Tasks
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId)
            ?? throw new NotFoundException("Task not found.");

        if (task.Version != request.Version)
        {
            throw new ConflictException("Task was updated by another collaborator. Refresh and try again.");
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim() ?? string.Empty;
        task.Status = ParseStatus(request.Status);
        task.Priority = ParsePriority(request.Priority);
        await ValidateAssigneeAsync(projectId, request.AssigneeId);
        task.AssigneeId = request.AssigneeId;
        task.DueDate = request.DueDate?.ToUniversalTime();
        task.SortOrder = request.SortOrder;
        task.Version += 1;
        task.UpdatedAt = DateTime.UtcNow;

        SyncLabels(task, request.Labels);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Task was updated by another collaborator. Refresh and try again.");
        }

        await activityService.LogAsync(projectId, userId, "Task", task.Id, "Updated", $"Updated task {task.Title}");
        var dto = await GetTaskDtoAsync(task.Id);
        await notifications.NotifyTaskChangedAsync(projectId, dto);
        return dto;
    }

    public async Task<TaskDto> MoveTaskAsync(Guid userId, Guid projectId, Guid taskId, MoveTaskRequest request)
    {
        await accessService.EnsureMemberAsync(userId, projectId);

        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId)
            ?? throw new NotFoundException("Task not found.");

        if (task.Version != request.Version)
        {
            throw new ConflictException("Task was updated by another collaborator. Refresh and try again.");
        }

        var previousStatus = task.Status;
        task.Status = ParseStatus(request.Status);
        task.SortOrder = request.SortOrder;
        task.Version += 1;
        task.UpdatedAt = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Task was updated by another collaborator. Refresh and try again.");
        }

        await activityService.LogAsync(
            projectId,
            userId,
            "Task",
            task.Id,
            "Moved",
            $"Moved {task.Title} from {previousStatus} to {task.Status}");

        var dto = await GetTaskDtoAsync(task.Id);
        await notifications.NotifyTaskChangedAsync(projectId, dto);
        return dto;
    }

    public async Task DeleteTaskAsync(Guid userId, Guid projectId, Guid taskId)
    {
        await accessService.EnsureMemberAsync(userId, projectId);

        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId)
            ?? throw new NotFoundException("Task not found.");

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        await activityService.LogAsync(projectId, userId, "Task", taskId, "Deleted", $"Deleted task {task.Title}");
        await notifications.NotifyTaskDeletedAsync(projectId, taskId);
    }

    private async Task ValidateAssigneeAsync(Guid projectId, Guid? assigneeId)
    {
        if (assigneeId is null)
        {
            return;
        }

        var isMember = await db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == assigneeId);
        if (!isMember)
        {
            throw new ValidationException("Assignee must be a project member.");
        }
    }

    private static BoardStatus ParseStatus(string status)
    {
        if (!Enum.TryParse<BoardStatus>(status, true, out var parsed))
        {
            throw new ValidationException("Invalid task status.");
        }

        return parsed;
    }

    private static TaskPriority ParsePriority(string priority)
    {
        if (!Enum.TryParse<TaskPriority>(priority, true, out var parsed))
        {
            throw new ValidationException("Invalid task priority.");
        }

        return parsed;
    }

    private static void AddLabels(TaskItem task, IReadOnlyList<string>? labels)
    {
        if (labels is null)
        {
            return;
        }

        foreach (var label in labels.Where(l => !string.IsNullOrWhiteSpace(l)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            task.Labels.Add(new TaskLabel
            {
                Id = Guid.NewGuid(),
                Name = label.Trim().ToLowerInvariant(),
                Color = "#6366f1"
            });
        }
    }

    private void SyncLabels(TaskItem task, IReadOnlyList<string>? labels)
    {
        var requestedNames = (labels ?? [])
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => label.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var labelsToRemove = task.Labels
            .Where(label => !requestedNames.Contains(label.Name))
            .ToList();

        db.TaskLabels.RemoveRange(labelsToRemove);

        var existingNames = task.Labels
            .Except(labelsToRemove)
            .Select(label => label.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var name in requestedNames.Where(name => !existingNames.Contains(name)))
        {
            db.TaskLabels.Add(new TaskLabel
            {
                Id = Guid.NewGuid(),
                TaskId = task.Id,
                Name = name,
                Color = "#6366f1"
            });
        }
    }

    private async Task<TaskDto> GetTaskDtoAsync(Guid taskId)
    {
        var task = await db.Tasks
            .AsNoTracking()
            .Include(t => t.Assignee)
            .Include(t => t.Labels)
            .FirstAsync(t => t.Id == taskId);

        return MapTask(task);
    }

    public static TaskDto MapTask(TaskItem task) => new(
        task.Id,
        task.ProjectId,
        task.Title,
        task.Description,
        task.Status.ToString(),
        task.Priority.ToString(),
        task.AssigneeId,
        task.Assignee?.DisplayName,
        task.DueDate,
        task.SortOrder,
        task.Version,
        task.CreatedAt,
        task.UpdatedAt,
        task.Labels.Select(l => new TaskLabelDto(l.Id, l.Name, l.Color)).ToList());
}
