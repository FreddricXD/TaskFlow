using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain.Enums;
using TaskFlow.Api.Dtos;

namespace TaskFlow.Api.Services;

public class AnalyticsService(TaskFlowDbContext db, ProjectAccessService accessService)
{
    public async Task<AnalyticsDto> GetAnalyticsAsync(Guid userId, Guid projectId)
    {
        await accessService.EnsureMemberAsync(userId, projectId);

        var tasks = await db.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();

        var statusDistribution = Enum.GetNames<BoardStatus>()
            .Select(status => new StatusCountDto(
                status,
                tasks.Count(t => t.Status.ToString() == status)))
            .ToList();

        var overdueCount = tasks.Count(t =>
            t.DueDate.HasValue &&
            t.DueDate.Value.Date < DateTime.UtcNow.Date &&
            t.Status != BoardStatus.Done);

        var completionTrend = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var day = DateTime.UtcNow.Date.AddDays(-offset);
                var completed = tasks.Count(t =>
                    t.Status == BoardStatus.Done &&
                    t.UpdatedAt.Date == day);

                return new TrendPointDto(day.ToString("yyyy-MM-dd"), completed);
            })
            .Reverse()
            .ToList();

        return new AnalyticsDto(statusDistribution, overdueCount, completionTrend);
    }
}
