using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Service;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:guid}/holiday-accrual")]
public class HolidayAccrualController : ControllerBase
{
    private readonly HolidayAccrualService accrual;
    private readonly UserService users;

    public HolidayAccrualController(HolidayAccrualService service, UserService userService)
    {
        accrual = service;
        users = userService;
    }

    [HttpGet]
    public ActionResult<HolidayAccrualResult> GetMonth(
        Guid userId, [FromQuery] int year, [FromQuery] int month)
    {
        var accessError = CheckAccess(userId);
        if (accessError != null)
        {
            return accessError;
        }

        try
        {
            return Ok(accrual.GetMonth(userId, year, month));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
    
    [HttpGet("forecast")]
    public ActionResult<HolidayAccrualResult> Forecast(Guid userId, [FromQuery] DateOnly date)
    {
        var accessError = CheckAccess(userId);
        if (accessError != null)
        {
            return accessError;
        }

        try
        {
            return Ok(accrual.Forecast(userId, date));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public ActionResult<HolidayAccrualResult> Post(Guid userId, [FromQuery] int year, [FromQuery] int month)
    {
        var accessError = CheckAccess(userId);
        if (accessError != null)
        {
            return accessError;
        }

        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdText, out var postedBy))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(accrual.Post(userId, year, month, postedBy));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { message = "Optjeningen blev bogført samtidig. Hent måneden igen." });
        }
    }

    private ActionResult? CheckAccess(Guid userId)
    {
        var employee = users.Find(userId);
        if (employee == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Admin") || User.IsInRole("Accounting"))
        {
            return null;
        }

        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (loggedInUserId == userId.ToString())
        {
            return null;
        }

        return Forbid();
    }
}