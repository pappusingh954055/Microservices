using Identity.Application.Commands.Logout;
using Identity.Application.Commands.RegisterUser;
using Identity.Application.DTOs;
using Identity.Application.Queries.LoginUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;


        public AuthController(IMediator mediator)
        {
            _mediator = mediator;

        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserCommand command)
        {
            var userId = await _mediator.Send(command);

            return Ok(new
            {
                UserId = userId
            });
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginUserQuery query)
        {
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return Unauthorized(result.Error);

            return Ok(result.Value);
        }

        // ---------------- REFRESH TOKEN ----------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh token is required");

            var result = await _mediator.Send(
                new RefreshTokenCommand(request.RefreshToken));

            if (!result.IsSuccess)
                return Unauthorized(result.Error);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
         [FromBody] LogOutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefrershToken))
                return BadRequest("Refresh token is required");

            var result = await _mediator.Send(
                new LogoutCommand(request.UserId, request.RefrershToken));

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

    }
}
