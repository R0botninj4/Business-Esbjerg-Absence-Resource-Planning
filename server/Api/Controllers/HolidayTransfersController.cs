using System.Security.Claims;
using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Service;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "Employee,Accounting,Admin")]
[Route("api/holiday-transfers")]
public class HolidayTransfersController : ControllerBase
{
    private readonly HolidayTransferService transfers;
    public HolidayTransfersController(HolidayTransferService service) { transfers = service; }
    private Guid UserId() { return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); }
    private bool CanReadAll() { return User.IsInRole("Accounting") || User.IsInRole("Admin"); }

    [HttpGet]
    public HolidayTransfer[] List() { return transfers.List(UserId(), CanReadAll()); }

    [HttpGet("{id:guid}")]
    public ActionResult<HolidayTransfer> Get(Guid id)
    {
        var result = transfers.Get(id, UserId(), CanReadAll());
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Employee")]
    public ActionResult<HolidayTransfer> Create(HolidayTransferInput input)
    {
        try
        {
            var saved = transfers.Create(UserId(), input);
            return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
        }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
        catch (HolidayRequestConflictException exception) { return Conflict(new { message = exception.Message }); }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        { return Conflict(new { message = "Operations-ID'et er allerede gemt." }); }
    }

    [HttpPost("{id:guid}/decision")]
    [Authorize(Roles = "Accounting")]
    public ActionResult<HolidayTransfer> Decide(Guid id, HolidayTransferDecisionInput input)
    {
        try
        {
            var saved = transfers.Decide(id, UserId(), input);
            if (saved == null) return NotFound();
            return Ok(saved);
        }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
        catch (HolidayRequestConflictException exception) { return Conflict(new { message = exception.Message }); }
    }
}
