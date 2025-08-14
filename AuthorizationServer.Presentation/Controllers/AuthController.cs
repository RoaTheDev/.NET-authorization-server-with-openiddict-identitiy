using Authorization.Application.Commands;
using Authorization.Application.Dto.Response;
using AuthorizationServer.Configs;
using AuthorizationServer.RateLimit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }
    /// <summary>
    /// Authenticate user and get access token
    /// </summary>
    [HttpPost("login")]
    [RateLimit(MaxRequest = 5, WindowMinutes = 1, KeyGen = RateLimitKeyGen.IpAddress)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command with 
        { 
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        });

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        if (result.Value!.RequiresTwoFactor)
        {
            return Ok(new { requiresTwoFactor = true, message = "Two-factor authentication required" });
        }

        return Ok(result.Value);
    }
    
    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [RateLimit(MaxRequest = 10, WindowMinutes = 1, KeyGen = RateLimitKeyGen.IpAddress)]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command with 
        { 
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        });

        if (!result.IsSuccess)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        var result = await _mediator.Send(command with 
        { 
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        return Ok(new { message = "Logged out successfully" });
    }
    
    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var query = new GetCurrentUserQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
    
    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [RateLimit(MaxRequest = 3, WindowMinutes = 5, KeyGen = RateLimitKeyGen.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return Ok(new { message = "Password changed successfully" });
    }
    
    /// <summary>
    /// Enable two-factor authentication
    /// </summary>
    [HttpPost("2fa/enable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EnableTwoFactor()
    {
        var command = new EnableTwoFactorCommand();
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}