using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pfebackend.DTOs;
using pfebackend.Interfaces;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<DashboardDataDto>> GetDashboardData(string userId)
    {
        var data = await _dashboardService.GetDashboardDataAsync(userId);

        if (data.Message == "No budget periods found for this user")
        {
            return NotFound(data);
        }

        return Ok(data);
    }
}