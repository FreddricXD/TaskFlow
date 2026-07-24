using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Domain.Entities;
using TaskFlow.Api.Domain.Enums;

namespace TaskFlow.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(TaskFlowDbContext db)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var alice = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "alice@taskflow.dev",
            DisplayName = "Alice Chen",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        };

        var bob = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "bob@taskflow.dev",
            DisplayName = "Bob Rivera",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        };

        var project = new Project
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Product Launch",
            Description = "Coordinate the Q3 launch across design, engineering, and marketing.",
            OwnerId = alice.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-14)
        };

        db.Users.AddRange(alice, bob);
        db.Projects.Add(project);
        db.ProjectMembers.AddRange(
            new ProjectMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = alice.Id, Role = ProjectRole.Owner },
            new ProjectMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = bob.Id, Role = ProjectRole.Member });

        var tasks = new List<TaskItem>
        {
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444401"),
                ProjectId = project.Id,
                Title = "Finalize landing page copy",
                Description = "Review hero messaging and CTA variants with marketing.",
                Status = BoardStatus.Todo,
                Priority = TaskPriority.High,
                AssigneeId = bob.Id,
                DueDate = DateTime.UtcNow.AddDays(3),
                SortOrder = 0
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444402"),
                ProjectId = project.Id,
                Title = "Implement onboarding flow",
                Description = "Build responsive onboarding with analytics events.",
                Status = BoardStatus.InProgress,
                Priority = TaskPriority.Critical,
                AssigneeId = alice.Id,
                DueDate = DateTime.UtcNow.AddDays(1),
                SortOrder = 0
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444403"),
                ProjectId = project.Id,
                Title = "QA mobile breakpoints",
                Description = "Validate Kanban and dashboard layouts on tablet and phone.",
                Status = BoardStatus.Review,
                Priority = TaskPriority.Medium,
                AssigneeId = bob.Id,
                DueDate = DateTime.UtcNow.AddDays(-1),
                SortOrder = 0
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444404"),
                ProjectId = project.Id,
                Title = "Ship beta announcement",
                Description = "Publish changelog and notify early adopters.",
                Status = BoardStatus.Done,
                Priority = TaskPriority.Low,
                AssigneeId = alice.Id,
                DueDate = DateTime.UtcNow.AddDays(-3),
                SortOrder = 0
            }
        };

        db.Tasks.AddRange(tasks);
        db.TaskLabels.AddRange(
            new TaskLabel { Id = Guid.NewGuid(), TaskId = tasks[0].Id, Name = "marketing", Color = "#f97316" },
            new TaskLabel { Id = Guid.NewGuid(), TaskId = tasks[1].Id, Name = "engineering", Color = "#6366f1" },
            new TaskLabel { Id = Guid.NewGuid(), TaskId = tasks[2].Id, Name = "qa", Color = "#14b8a6" });

        db.ActivityLogs.AddRange(
            new ActivityLog
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                UserId = alice.Id,
                EntityType = "Project",
                EntityId = project.Id,
                Action = "Created",
                Description = "Created project Product Launch",
                CreatedAt = DateTime.UtcNow.AddDays(-14)
            },
            new ActivityLog
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                UserId = bob.Id,
                EntityType = "Task",
                EntityId = tasks[2].Id,
                Action = "Moved",
                Description = "Moved QA mobile breakpoints to Review",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            });

        await db.SaveChangesAsync();
    }
}
