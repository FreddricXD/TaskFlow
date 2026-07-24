using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain.Entities;
using TaskFlow.Api.Domain.Enums;
using TaskFlow.Api.Dtos;

namespace TaskFlow.Api.Services;

public class ProjectService(TaskFlowDbContext db, ActivityService activityService, ProjectAccessService accessService)
{
    public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(Guid userId)
    {
        return await db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.OwnerId,
                p.Owner.DisplayName,
                p.CreatedAt,
                p.Tasks.Count,
                p.Members.Count))
            .ToListAsync();
    }

    public async Task<ProjectDetailDto> GetProjectAsync(Guid userId, Guid projectId)
    {
        await accessService.EnsureMemberAsync(userId, projectId);

        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == projectId)
            ?? throw new NotFoundException("Project not found.");

        return new ProjectDetailDto(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerId,
            project.Owner.DisplayName,
            project.CreatedAt,
            project.Members
                .OrderByDescending(m => m.Role)
                .ThenBy(m => m.User.DisplayName)
                .Select(m => new ProjectMemberDto(
                    m.Id,
                    m.UserId,
                    m.User.DisplayName,
                    m.User.Email,
                    m.Role.ToString()))
                .ToList());
    }

    public async Task<ProjectDetailDto> CreateProjectAsync(Guid userId, CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Project name is required.");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            OwnerId = userId
        };

        db.Projects.Add(project);
        db.ProjectMembers.Add(new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = userId,
            Role = ProjectRole.Owner
        });

        await db.SaveChangesAsync();
        await activityService.LogAsync(project.Id, userId, "Project", project.Id, "Created", $"Created project {project.Name}");

        return await GetProjectAsync(userId, project.Id);
    }

    public async Task<ProjectDetailDto> UpdateProjectAsync(Guid userId, Guid projectId, UpdateProjectRequest request)
    {
        await accessService.EnsureAdminAsync(userId, projectId);

        var project = await db.Projects.FindAsync(projectId)
            ?? throw new NotFoundException("Project not found.");

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim() ?? string.Empty;

        await db.SaveChangesAsync();
        await activityService.LogAsync(projectId, userId, "Project", projectId, "Updated", $"Updated project {project.Name}");

        return await GetProjectAsync(userId, projectId);
    }

    public async Task<ProjectMemberDto> AddMemberAsync(Guid userId, Guid projectId, AddMemberRequest request)
    {
        await accessService.EnsureAdminAsync(userId, projectId);

        if (!Enum.TryParse<ProjectRole>(request.Role, true, out var role) || role == ProjectRole.Owner)
        {
            throw new ValidationException("Role must be Member or Admin.");
        }

        var memberUser = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant())
            ?? throw new NotFoundException("User not found.");

        if (await db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == memberUser.Id))
        {
            throw new ConflictException("User is already a project member.");
        }

        var member = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = memberUser.Id,
            Role = role
        };

        db.ProjectMembers.Add(member);
        await db.SaveChangesAsync();
        await activityService.LogAsync(projectId, userId, "Member", member.Id, "Added", $"Added {memberUser.DisplayName} as {role}");

        return new ProjectMemberDto(member.Id, memberUser.Id, memberUser.DisplayName, memberUser.Email, role.ToString());
    }

}
