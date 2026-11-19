using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/employments/{employmentId:guid}/work-schedules")]
public class WorkSchedulesController : ControllerBase
{
    private readonly WorkScheduleService workSchedules;

    public WorkSchedulesController(WorkScheduleService service)
    {
        workSchedules = service;
    }

    [HttpGet]
    public ActionResult<WorkSchedule[]> Get(Guid employmentId)
    {
        var employment = workSchedules.FindEmployment(employmentId);

        if (employment == null)
        {
            return NotFound();
        }

        if (!CanRead(employment))
        {
            return Forbid();
        }

        var schedules = workSchedules.GetByEmploymentId(employmentId);

        return Ok(schedules);
    }

    [HttpGet("for-date")]
    [ProducesResponseType(typeof(WorkSchedule), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<WorkSchedule> GetForDate(Guid employmentId, [FromQuery] DateOnly date)
    {
        var employment = workSchedules.FindEmployment(employmentId);
        if (employment == null)
        {
            return NotFound();
        }

        if (!CanRead(employment))
        {
            return Forbid();
        }

        if (date == default)
        {
            return BadRequest(new { message = "Angiv en dato." });
        }

        var schedule = workSchedules.GetForDate(employmentId, date);

        if (schedule == null)
        {
            return NotFound();
        }

        return Ok(schedule);
    }

    [HttpGet("{workScheduleId:guid}/days")]
    public ActionResult<WorkScheduleDay[]> GetDays(
        Guid employmentId, Guid workScheduleId)
    {
        var employment = workSchedules.FindEmployment(employmentId);
        if (employment == null)
        {
            return NotFound();
        }

        if (!CanRead(employment))
        {
            return Forbid();
        }

        var schedule = workSchedules.Find(employmentId, workScheduleId);
        if (schedule == null)
        {
            return NotFound();
        }

        var days = workSchedules.GetDays(workScheduleId);

        return Ok(days);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Accounting")]
    [ProducesResponseType(typeof(WorkSchedule), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<WorkSchedule> Create(
        Guid employmentId, CreateWorkScheduleRequest request)
    {
        try
        {
            var schedule = workSchedules.Create(
                employmentId,
                request.StartDate,
                request.EndDate,
                request.Days);

            if (schedule == null)
            {
                return NotFound();
            }

            return StatusCode(StatusCodes.Status201Created, schedule);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (WorkScheduleOverlapException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPatch("{workScheduleId:guid}/end")]
    [Authorize(Roles = "Admin,Accounting")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult End(
        Guid employmentId, Guid workScheduleId, EndWorkScheduleRequest request)
    {
        var schedule = workSchedules.Find(employmentId, workScheduleId);

        if (schedule == null)
        {
            return NotFound();
        }

        if (request.EndDate < schedule.StartDate)
        {
            return BadRequest(new { message = "Slutdatoen må ikke være før startdatoen." });
        }

        if (!workSchedules.End(employmentId, workScheduleId, request.EndDate))
        {
            return Conflict(new { message = "Ugeplanen er allerede afsluttet." });
        }

        return NoContent();
    }

    [HttpPut("{workScheduleId:guid}")]
    [Authorize(Roles = "Admin,Accounting")]
    [ProducesResponseType(typeof(WorkSchedule), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<WorkSchedule> Change(
        Guid employmentId, Guid workScheduleId, ChangeWorkScheduleRequest request)
    {
        try
        {
            var schedule = workSchedules.Change(
                employmentId, workScheduleId, request.StartDate, request.Days);

            if (schedule == null)
            {
                return NotFound();
            }

            return StatusCode(StatusCodes.Status201Created, schedule);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (WorkScheduleOverlapException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    private bool CanRead(Employment employment)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        if (User.IsInRole("Accounting"))
        {
            return true;
        }

        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return loggedInUserId == employment.UserId.ToString();
    }

    public class ChangeWorkScheduleRequest
    {
        public DateOnly StartDate { get; set; }

        public WorkScheduleDayRequest[] Days { get; set; } = new WorkScheduleDayRequest[0];
    }

    public class EndWorkScheduleRequest
    {
        public DateOnly EndDate { get; set; }
    }

    public class CreateWorkScheduleRequest
    {
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public WorkScheduleDayRequest[] Days { get; set; } = new WorkScheduleDayRequest[0];
    }
}
