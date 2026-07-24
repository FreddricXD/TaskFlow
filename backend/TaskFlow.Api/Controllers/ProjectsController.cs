using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController(ProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetProjects()
        => Ok(await projectService.GetProjectsAsync(HttpContext.GetUserId()));

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ProjectDetailDto>> GetProject(Guid projectId)
        => Ok(await projectService.GetProjectAsync(HttpContext.GetUserId(), projectId));

    [HttpPost]
    public async Task<ActionResult<ProjectDetailDto>> CreateProject([FromBody] CreateProjectRequest request)
        => Ok(await projectService.CreateProjectAsync(HttpContext.GetUserId(), request));

    [HttpPut("{projectId:guid}")]
    public async Task<ActionResult<ProjectDetailDto>> UpdateProject(Guid projectId, [FromBody] UpdateProjectRequest request)
        => Ok(await projectService.UpdateProjectAsync(HttpContext.GetUserId(), projectId, request));

    [HttpPost("{projectId:guid}/members")]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(Guid projectId, [FromBody] AddMemberRequest request)
        => Ok(await projectService.AddMemberAsync(HttpContext.GetUserId(), projectId, request));

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> DeleteProject(Guid projectId)
    {
        await projectService.DeleteProjectAsync(HttpContext.GetUserId(), projectId);
        return NoContent();
    }
}
