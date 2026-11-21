using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Service;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/calendar-days")]
public class CalendarDaysController : ControllerBase
{
    private readonly CalendarService calendar;

    public CalendarDaysController(CalendarService service)
    {
        calendar = service;
    }

    [HttpGet]
    public ActionResult<CalendarDay[]> Get()
    {
        return Ok(calendar.GetHistory());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CalendarDay), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<CalendarDay> Create(CreateCalendarDayRequest request)
    {
        if (!request.UsesHolidayHours.HasValue)
        {
            return BadRequest(new { message = "Angiv, om dagen bruger ferietimer." });
        }

        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdText, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var day = calendar.Create(
                request.Date, request.Name, request.UsesHolidayHours.Value, userId);

            return StatusCode(StatusCodes.Status201Created, day);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (SqliteException exception) when (exception.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new { message = "Datoen blev ændret samtidig. Prøv igen." });
        }
    }

    public class CreateCalendarDayRequest
    {
        public DateOnly Date { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public bool? UsesHolidayHours { get; set; }
    }
}
