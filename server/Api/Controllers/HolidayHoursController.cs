using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/employments/{employmentId:guid}/holiday-hours")]
public class HolidayHoursController : ControllerBase
{
    private readonly WorkScheduleService workSchedules;
    private readonly HolidayHoursService holidayHours;

    public HolidayHoursController(WorkScheduleService schedules, HolidayHoursService hours)
    {
        workSchedules = schedules;
        holidayHours = hours;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HolidayHoursResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<HolidayHoursResult> Calculate(
        Guid employmentId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        var employment = workSchedules.FindEmployment(employmentId);
        if (employment == null)
        {
            return NotFound();
        }

        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool canRead = loggedInUserId == employment.UserId.ToString();

        if (User.IsInRole("Admin") || User.IsInRole("Accounting"))
        {
            canRead = true;
        }

        if (!canRead)
        {
            return Forbid();
        }

        if (startDate == default || endDate == default)
        {
            return BadRequest(new { message = "Angiv både startdato og slutdato." });
        }

        try
        {
            var response = holidayHours.Calculate(employmentId, startDate, endDate);

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
