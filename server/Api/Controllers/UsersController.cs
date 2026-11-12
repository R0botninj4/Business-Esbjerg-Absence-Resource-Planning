using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Service;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UsersController(UserService users) : ControllerBase
{
    [HttpGet]
    public IEnumerable<UserResponse> List() => users.List().Select(UserResponse.From);

    [HttpGet("{id:guid}")]
    public ActionResult<UserResponse> Get(Guid id)
    {
        var user = users.Find(id);
        return user is null ? NotFound() : Ok(UserResponse.From(user));
    }

    [HttpPost]
    public ActionResult<UserResponse> Create(CreateUserRequest request)
    {
        try
        {
            var user = users.Create(request.Email, request.Password, request.Role);
            return CreatedAtAction(nameof(Get), new { user.Id }, UserResponse.From(user));
        }
        catch (SqliteException exception) when (exception.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new { message = "E-mailadressen er allerede i brug." });
        }
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, UpdateUserRequest request)
    {
        if (IsOwnAccount(id) && request.Role != "Admin")
        {
            return Conflict(new { message = "Du kan ikke fjerne din egen adminrolle." });
        }

        try
        {
            return users.Update(id, request.Email, request.Role) ? NoContent() : NotFound();
        }
        catch (SqliteException exception) when (exception.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new { message = "E-mailadressen er allerede i brug." });
        }
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        if (IsOwnAccount(id))
        {
            return Conflict(new { message = "Du kan ikke slette din egen konto." });
        }

        return users.Delete(id) ? NoContent() : NotFound();
    }

    private bool IsOwnAccount(Guid id) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentId) && currentId == id;

    public record CreateUserRequest(
        [Required, EmailAddress, StringLength(254)] string Email,
        [Required, StringLength(128, MinimumLength = 12)] string Password,
        [Required, RegularExpression("Admin|Employee|Accounting")] string Role);

    public record UpdateUserRequest(
        [Required, EmailAddress, StringLength(254)] string Email,
        [Required, RegularExpression("Admin|Employee|Accounting")] string Role);

    public record UserResponse(Guid Id, string Email, string Role)
    {
        public static UserResponse From(User user) => new(user.Id, user.Email, user.Role);
    }
}
