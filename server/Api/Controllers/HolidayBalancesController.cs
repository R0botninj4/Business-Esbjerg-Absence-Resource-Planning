using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Infra;
using Npgsql;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:guid}/holiday-balances")]
public class HolidayBalancesController : ControllerBase
{
    private readonly HolidayBalanceService balances;
    private readonly UserService users;
    private readonly HolidayAdjustmentService adjustments;

    public HolidayBalancesController(HolidayBalanceService service, UserService userService,
        HolidayAdjustmentService adjustmentService)
    {
        balances = service;
        users = userService;
        adjustments = adjustmentService;
    }

    [HttpGet]
    public ActionResult<HolidayBalanceResult> Get(Guid userId, [FromQuery] DateOnly date)
    {
        var employee = users.Find(userId);
        if (employee == null)
        {
            return NotFound();
        }

        if (!CanRead(userId))
        {
            return Forbid();
        }

        if (date == default)
        {
            return BadRequest(new { message = "Angiv den dato, oversigten skal beregnes til." });
        }

        return Ok(balances.Get(userId, date));
    }

    [HttpGet("adjustments")]
    public ActionResult<HolidayBalanceAdjustment[]> GetAdjustments(Guid userId)
    {
        var employee = users.Find(userId);
        if (employee == null)
        {
            return NotFound();
        }

        if (!CanRead(userId))
        {
            return Forbid();
        }

        return Ok(adjustments.GetHistory(userId));
    }

    [HttpPost("adjustments")]
    [Authorize(Roles = "Admin")]
    public ActionResult<HolidayBalanceAdjustment> CreateAdjustment(Guid userId, HolidayAdjustmentRequest request)
    {
        if (users.Find(userId) == null)
        {
            return NotFound();
        }

        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdText, out var createdBy))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(adjustments.Create(userId, request, createdBy));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { message = "Operations-ID'et er allerede gemt. Gentag kaldet med samme indhold." });
        }
    }

    private bool CanRead(Guid userId)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        if (User.IsInRole("Accounting"))
        {
            return true;
        }

        // Andre brugere må kun læse deres egne oplysninger.
        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (loggedInUserId == userId.ToString())
        {
            return true;
        }

        return false;
    }
}
