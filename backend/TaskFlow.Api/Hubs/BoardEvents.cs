namespace TaskFlow.Api.Hubs;

public static class BoardEvents
{
    public const string TaskChanged = "TaskChanged";
    public const string TaskDeleted = "TaskDeleted";
    public const string ActivityCreated = "ActivityCreated";
    public const string AnalyticsChanged = "AnalyticsChanged";
}
