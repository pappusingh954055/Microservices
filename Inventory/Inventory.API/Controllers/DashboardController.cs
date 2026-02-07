using Inventory.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardRepository _dashboardRepo;

    public DashboardController(IDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        // Top 4 widgets ka data return karega
        var summary = await _dashboardRepo.GetDashboardSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("charts")]
    public async Task<ActionResult<DashboardChartDto>> GetChartData()
    {
        // Line chart aur Donut chart ka dynamic data return karega
        var charts = await _dashboardRepo.GetDashboardChartsAsync();
        return Ok(charts);
    }

    [HttpGet("recent-activities")]
    public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivities()
    {
        // Recent Stock Movements table ke liye dynamic feed
        var activities = await _dashboardRepo.GetRecentActivitiesAsync();
        return Ok(activities);
    }
}