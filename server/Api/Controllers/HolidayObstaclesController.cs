using System.Security.Claims;
using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Npgsql;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "Employee,Accounting")]
[Route("api/holiday-obstacles")]
public class HolidayObstaclesController : ControllerBase
{
    private readonly HolidayObstacleService obstacles;
    public HolidayObstaclesController(HolidayObstacleService service) { obstacles = service; }
    private Guid UserId() { return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); }

    [HttpGet]
    public HolidayObstacle[] List() { return obstacles.List(UserId(), User.IsInRole("Accounting")); }

    [HttpGet("{id:guid}")]
    public ActionResult<HolidayObstacleResult> Get(Guid id)
    {
        var result = obstacles.Get(id, UserId(), User.IsInRole("Accounting"));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Employee")]
    public ActionResult<HolidayObstacle> Create(HolidayObstacleInput input)
    {
        try
        {
            var saved = obstacles.Create(UserId(), input);
            return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
        }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
        catch (HolidayRequestConflictException exception) { return Conflict(new { message = exception.Message }); }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { message = "Anmeldelsen er allerede gemt. Gentag med samme operations-ID og indhold." });
        }
    }

    [HttpPost("{id:guid}/decision")]
    [Authorize(Roles = "Accounting")]
    public ActionResult<HolidayObstacle> Decide(Guid id, HolidayDecisionInput input)
    {
        try
        {
            var saved = obstacles.Decide(id, UserId(), input);
            if (saved == null) return NotFound();
            return Ok(saved);
        }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
        catch (HolidayRequestConflictException exception) { return Conflict(new { message = exception.Message }); }
    }
}
