using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Api.Dtos;

namespace TaskFlow.Api.Tests;

public class TaskFlowApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TaskFlowApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting(WebHostDefaults.EnvironmentKey, "Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={Path.Combine(Path.GetTempPath(), $"taskflow-test-{Guid.NewGuid():N}.db")}");
        });
    }

    [Fact]
    public async Task Login_WithDemoUser_ReturnsToken()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new AuthRequest("alice@taskflow.dev", "Password123!"));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.Token));
        Assert.Equal("alice@taskflow.dev", payload.User.Email);
    }

    [Fact]
    public async Task GetProjects_AfterLogin_ReturnsSeededProject()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new AuthRequest("alice@taskflow.dev", "Password123!"));
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        var response = await client.GetAsync("/api/projects");
        response.EnsureSuccessStatusCode();

        var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        Assert.NotNull(projects);
        Assert.NotEmpty(projects);
        Assert.Contains(projects, p => p.Name == "Product Launch");
    }

    [Fact]
    public async Task MoveTask_WithStaleVersion_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new AuthRequest("alice@taskflow.dev", "Password123!"));
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var taskId = Guid.Parse("44444444-4444-4444-4444-444444444401");

        var tasksResponse = await client.GetAsync($"/api/projects/{projectId}/tasks");
        var tasks = await tasksResponse.Content.ReadFromJsonAsync<List<TaskDto>>();
        var task = tasks!.First(t => t.Id == taskId);

        var response = await client.PatchAsJsonAsync(
            $"/api/projects/{projectId}/tasks/{taskId}/move",
            new MoveTaskRequest("InProgress", 0, task.Version - 1));

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal("conflict", error!.Code);
    }
}
