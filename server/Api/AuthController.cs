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
    public async Task<IActionResult> Login(LoginRequest request)
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

        return Ok(new { user.Id, user.Email, user.Role });
    }
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }
    public record LoginRequest(
        [Required, EmailAddress] string Email,
        [Required] string Password);
}