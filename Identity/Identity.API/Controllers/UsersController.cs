using Microsoft.AspNetCore.Mvc;
using Identity.Application.Interfaces;
using Identity.Domain.Users;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepository.GetAllUsersAsync();
        // Project to DTO or simple object to avoid circular references and include role names as strings if frontend expects it
        var result = users.Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.IsActive,
            Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
        });
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.SetActive(isActive);
        await _userRepository.UpdateAsync(user);
        return Ok();
    }

    [HttpGet("check-duplicate")]
    public async Task<IActionResult> CheckDuplicate([FromQuery] string? userName, [FromQuery] string? email)
    {
        if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(email))
            return BadRequest("Username or Email must be provided");

        bool exists = false;
        string message = "";

        if (!string.IsNullOrEmpty(userName) && await _userRepository.ExistsByUserNameAsync(userName))
        {
            exists = true;
            message = "Username already exists.";
        }
        else if (!string.IsNullOrEmpty(email) && await _userRepository.ExistsByEmailAsync(email))
        {
            exists = true;
            message = "Email already exists.";
        }

        return Ok(new { Exists = exists, Message = message });
    }
}
