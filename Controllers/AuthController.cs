using System.Net;
using Api.Services.Extensions;
using Auth;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly Authenticator.AuthenticatorClient _authClient;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(Authenticator.AuthenticatorClient authClient, ILogger<AuthController> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var grpcResponse = await _authClient.RegisterAsync(request);
            return grpcResponse.Status switch
            {
                RegisterResponse.Types.Status.Success => Ok(new { Message = grpcResponse.Message }),
                RegisterResponse.Types.Status.EmailConflict => Conflict(new { Message = grpcResponse.Message }),
                RegisterResponse.Types.Status.InvalidData => BadRequest(new { Message = grpcResponse.Message }),
                _ => StatusCode(500, new { Message = grpcResponse.Message })
            };
        }
        catch (RpcException e)
        {
            _logger.LogError("Error occured when calling auth service {error}", e);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var grpcResponse = await _authClient.LoginAsync(request);
            if (grpcResponse.IsSuccess)
            {
                //is secure is false for now as we do not have https set up
                SetAccessToken(grpcResponse.LoginToken);
                this.SetCookie("refresh_token", grpcResponse.RefreshToken, DateTimeOffset.Now.AddDays(30));
                return Ok();
            }

            return Unauthorized("Invalid login data");
        }
        catch (RpcException e)
        {
            _logger.LogError("Error occured when calling auth service {error}", e);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            string refreshToken;
            if (!HttpContext.Request.Cookies.TryGetValue("refresh_token", out refreshToken!))
            {
                return Unauthorized(new { Message = "Refresh token cookie has not been sent!" });
            }

            var refreshRequest = new RefreshRequest { RefreshToken = refreshToken };
            var grpcResponse = await _authClient.RefreshAsync(refreshRequest);
            if (grpcResponse.IsSuccess)
            {
                SetAccessToken(grpcResponse.AuthToken);
                return Ok();
            }

            return Unauthorized(new { Message = grpcResponse.Message });
        }
        catch (RpcException e)
        {
            _logger.LogError("Error occured when calling auth service {error}", e);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    private void SetAccessToken(string token)
    {
        this.SetCookie("access_token", token, DateTimeOffset.Now.AddMinutes(20), isSecure: false);
    }
}