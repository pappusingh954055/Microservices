using Inventory.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Notification hamesha logged-in user ke liye honi chahiye
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _repo;
    public NotificationsController(INotificationRepository repo) => _repo = repo;

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread() => Ok(await _repo.GetUnreadNotificationsAsync());

    [HttpGet("count")]
    public async Task<IActionResult> GetCount() => Ok(await _repo.GetUnreadCountAsync());

    [HttpPost("{id}/mark-read")]
    public async Task<IActionResult> MarkRead(long id)
    {
        var result = await _repo.MarkAsReadAsync(id);
        return result ? Ok() : BadRequest("Notification not found");
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead() => Ok(await _repo.MarkAllAsReadAsync());
}