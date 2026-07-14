using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.Auth.AppleLogin;
using StudyVerse.Application.Features.Auth.ForgotPassword;
using StudyVerse.Application.Features.Auth.GetCurrentUser;
using StudyVerse.Application.Features.Auth.GoogleLogin;
using StudyVerse.Application.Features.Auth.Login;
using StudyVerse.Application.Features.Auth.Logout;
using StudyVerse.Application.Features.Auth.RequestOtp;
using StudyVerse.Application.Features.Auth.ResendVerification;
using StudyVerse.Application.Features.Auth.Register;
using StudyVerse.Application.Features.Auth.ResetPassword;
using StudyVerse.Application.Features.Auth.VerifyEmail;
using StudyVerse.Application.Features.Auth.VerifyOtp;
using RefreshTokenCommand = StudyVerse.Application.Features.Auth.RefreshToken.RefreshTokenCommand;

namespace StudyVerse.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new RegisterCommand(request.Email, request.Password, request.DisplayName),
            cancellationToken);

        return FromResult(result, dto => CreatedAtAction(nameof(Register), new { dto.UserId }, dto));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new LoginCommand(request.Email, request.Password, request.DeviceId, request.DeviceName),
            cancellationToken);

        return FromResult(result, session => Ok(session));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new RefreshTokenCommand(request.RefreshToken, request.DeviceId),
            cancellationToken);

        return FromResult(result, tokens => Ok(tokens));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new LogoutCommand(request.RefreshToken, request.DeviceId),
            cancellationToken);

        return FromResult(result, () => NoContent());
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new VerifyEmailCommand(request.UserId, request.Token), cancellationToken);

        return FromResult(result, () => Ok(new { message = "Email verified successfully." }));
    }

    [HttpPost("resend-verification")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new ResendVerificationCommand(request.Email), cancellationToken);

        // Always 202, regardless of whether the account exists, to avoid user enumeration.
        return Accepted(new { message = "If an account exists for that email, a verification link has been sent." });
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ForgotPasswordCommand(request.Email), cancellationToken);

        return Accepted(new { message = "If an account exists for that email, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new ResetPasswordCommand(request.Email, request.Token, request.NewPassword),
            cancellationToken);

        return FromResult(result, () => Ok(new { message = "Password has been reset successfully." }));
    }

    [HttpPost("otp/request")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestOtp([FromBody] OtpRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new RequestOtpCommand(request.Channel, request.Destination, request.Purpose),
            cancellationToken);

        return FromResult(result, () => Accepted(new { message = "A one-time code has been sent." }));
    }

    [HttpPost("otp/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new VerifyOtpCommand(
                request.Channel,
                request.Destination,
                request.Code,
                request.Purpose,
                request.DeviceId,
                request.DeviceName),
            cancellationToken);

        return FromResult(result, session => Ok(session));
    }

    [HttpPost("google")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Google([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GoogleLoginCommand(request.IdToken, request.DeviceId, request.DeviceName),
            cancellationToken);

        return FromResult(result, session => Ok(session));
    }

    [HttpPost("apple")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Apple([FromBody] AppleLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new AppleLoginCommand(request.IdentityToken, request.DeviceId, request.DeviceName, request.FullName),
            cancellationToken);

        return FromResult(result, session => Ok(session));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);

        return FromResult(result, profile => Ok(profile));
    }
}
