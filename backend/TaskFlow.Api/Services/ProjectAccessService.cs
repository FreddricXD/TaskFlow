using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain.Enums;

namespace TaskFlow.Api.Services;

public class ProjectAccessService(TaskFlowDbContext db)
{
    public async Task EnsureMemberAsync(Guid userId, Guid projectId)
    {
        var isMember = await db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
        if (!isMember)
        {
            throw new ForbiddenException("You do not have access to this project.");
        }
    }

    public async Task EnsureAdminAsync(Guid userId, Guid projectId)
    {
        var member = await db.ProjectMembers.FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
        if (member is null || member.Role is ProjectRole.Member)
        {
            throw new ForbiddenException("Admin access is required for this action.");
        }
    }

    public async Task EnsureOwnerAsync(Guid userId, Guid projectId)
    {
        var isOwner = await db.Projects.AnyAsync(project => project.Id == projectId && project.OwnerId == userId);
        if (!isOwner)
        {
            throw new ForbiddenException("Only the project owner can delete this project.");
        }
    }
}
