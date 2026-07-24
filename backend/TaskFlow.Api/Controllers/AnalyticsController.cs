using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/[controller]")]
public class AnalyticsController(AnalyticsService analyticsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AnalyticsDto>> GetAnalytics(Guid projectId)
        => Ok(await analyticsService.GetAnalyticsAsync(HttpContext.GetUserId(), projectId));
}
