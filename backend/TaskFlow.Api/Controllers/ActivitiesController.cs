using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/[controller]")]
public class ActivitiesController(ActivityService activityService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActivityDto>>> GetActivities(Guid projectId, [FromQuery] int take = 25)
        => Ok(await activityService.GetActivitiesAsync(HttpContext.GetUserId(), projectId, take));
}
