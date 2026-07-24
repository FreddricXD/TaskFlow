using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/[controller]")]
public class TasksController(TaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetTasks(
        Guid projectId,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? priority)
        => Ok(await taskService.GetTasksAsync(HttpContext.GetUserId(), projectId, search, status, priority));

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(Guid projectId, [FromBody] CreateTaskRequest request)
        => Ok(await taskService.CreateTaskAsync(HttpContext.GetUserId(), projectId, request));

    [HttpPut("{taskId:guid}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(Guid projectId, Guid taskId, [FromBody] UpdateTaskRequest request)
        => Ok(await taskService.UpdateTaskAsync(HttpContext.GetUserId(), projectId, taskId, request));

    [HttpPatch("{taskId:guid}/move")]
    public async Task<ActionResult<TaskDto>> MoveTask(Guid projectId, Guid taskId, [FromBody] MoveTaskRequest request)
        => Ok(await taskService.MoveTaskAsync(HttpContext.GetUserId(), projectId, taskId, request));

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid projectId, Guid taskId)
    {
        await taskService.DeleteTaskAsync(HttpContext.GetUserId(), projectId, taskId);
        return NoContent();
    }
}
