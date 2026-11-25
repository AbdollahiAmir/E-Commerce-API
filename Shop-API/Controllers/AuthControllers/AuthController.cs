using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shop_API.Constants;
using Shop_API.DTOs.AuthDTOs;
using Shop_API.Logics.Helpers;
using Shop_API.Logics.IServices.IAuthServices;

namespace Shop_API.Controllers.AuthControllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);

            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshAsync(request, cancellationToken);

            return Ok(result);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        {
            await _authService.LogoutAsync(request, cancellationToken);

            return NoContent();
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
        {
            await _authService.LogoutAllAsync(User.GetUserId(), cancellationToken);

            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<MeDto>> Me(CancellationToken cancellationToken)
        {
            var result = await _authService.GetMeAsync(User.GetUserId(), cancellationToken);

            return Ok(result);
        }

        [HttpPatch("me")]
        [Authorize]
        public async Task<ActionResult<MeDto>> EditMe([FromBody] EditMeDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.EditMeAsync(User.GetUserId(), request, cancellationToken);

            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<ActionResult<AuthResponseDto>> ChangePassword([FromBody] ChangePasswordDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.ChangePasswordAsync(User.GetUserId(), request, cancellationToken);

            return Ok(result);
        }
    }
}