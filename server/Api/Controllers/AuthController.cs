using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Service;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UserService userService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<UserInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserInfo>> Login(LoginRequest request)
    {
        var user = userService.VerifyLogin(request.Email, request.Password);

        if (user is null)
        {
            return Unauthorized();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, scheme));
        await HttpContext.SignInAsync(scheme, principal);

        return Ok(new UserInfo(user.Id, user.Email, user.Role));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<UserInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserInfo> Me()
    {
        return Ok(new UserInfo(
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            User.FindFirstValue(ClaimTypes.Email)!,
            User.FindFirstValue(ClaimTypes.Role)!));
    }

    public record UserInfo(Guid Id, string Email, string Role);
    public record LoginRequest(
        [Required, EmailAddress] string Email,
        [Required] string Password);
}