using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "Employee,Accounting,Admin")]
[Route("api/colleague-calendar")]
public class ColleagueCalendarController : ControllerBase
{
    private readonly ColleagueCalendarService calendar;

    public ColleagueCalendarController(ColleagueCalendarService service)
    {
        calendar = service;
    }

    [HttpGet]
    public ActionResult<ColleagueCalendarResult> Get([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        try
        {
            return Ok(calendar.Get(startDate, endDate));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
