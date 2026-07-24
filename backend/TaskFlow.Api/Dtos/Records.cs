namespace TaskFlow.Api.Dtos;

public record AuthRequest(string Email, string Password);

public record RegisterRequest(string DisplayName, string Email, string Password);

public record AuthResponse(string Token, UserDto User);

public record UserDto(Guid Id, string Email, string DisplayName);

public record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    Guid OwnerId,
    string OwnerName,
    DateTime CreatedAt,
    int TaskCount,
    int MemberCount);

public record ProjectDetailDto(
    Guid Id,
    string Name,
    string Description,
    Guid OwnerId,
    string OwnerName,
    DateTime CreatedAt,
    IReadOnlyList<ProjectMemberDto> Members);

public record ProjectMemberDto(Guid Id, Guid UserId, string DisplayName, string Email, string Role);

public record CreateProjectRequest(string Name, string Description);

public record UpdateProjectRequest(string Name, string Description);

public record AddMemberRequest(string Email, string Role);

public record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid? AssigneeId,
    string? AssigneeName,
    DateTime? DueDate,
    int SortOrder,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<TaskLabelDto> Labels);

public record TaskLabelDto(Guid Id, string Name, string Color);

public record CreateTaskRequest(
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid? AssigneeId,
    DateTime? DueDate,
    IReadOnlyList<string>? Labels);

public record UpdateTaskRequest(
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid? AssigneeId,
    DateTime? DueDate,
    int SortOrder,
    int Version,
    IReadOnlyList<string>? Labels);

public record MoveTaskRequest(string Status, int SortOrder, int Version);

public record ActivityDto(
    Guid Id,
    Guid ProjectId,
    Guid UserId,
    string UserName,
    string EntityType,
    Guid EntityId,
    string Action,
    string Description,
    DateTime CreatedAt);

public record AnalyticsDto(
    IReadOnlyList<StatusCountDto> StatusDistribution,
    int OverdueCount,
    IReadOnlyList<TrendPointDto> CompletionTrend);

public record StatusCountDto(string Status, int Count);

public record TrendPointDto(string Date, int Completed);

public record ApiError(string Message, string? Code = null);
