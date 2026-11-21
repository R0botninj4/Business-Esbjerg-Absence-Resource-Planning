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
[Route("api/holiday-rules")]
public class HolidayRulesController : ControllerBase
{
    private readonly HolidayRuleService rules;

    public HolidayRulesController(HolidayRuleService service)
    {
        rules = service;
    }

    [HttpGet]
    public ActionResult<HolidayRule[]> GetHistory()
    {
        return Ok(rules.GetHistory());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<HolidayRule> Get(Guid id)
    {
        var rule = rules.Find(id);
        if (rule == null)
        {
            return NotFound();
        }

        return Ok(rule);
    }

    [HttpGet("presets")]
    public ActionResult<HolidayRuleSettings[]> GetPresets()
    {
        return Ok(rules.GetPresets());
    }

    [HttpGet("for-date")]
    public ActionResult<HolidayRule> GetForDate([FromQuery] DateOnly date)
    {
        if (date == default)
        {
            return BadRequest(new { message = "Angiv en dato." });
        }

        var rule = rules.GetForDate(date);
        if (rule == null)
        {
            return NotFound(new { message = "Der er ingen regler for den valgte dato." });
        }

        return Ok(rule);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(HolidayRule), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<HolidayRule> Create(CreateHolidayRuleRequest request)
    {
        if (request.Settings == null)
        {
            return BadRequest(new { message = "Angiv feriereglerne." });
        }

        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdText, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var rule = rules.Create(request.EffectiveFrom, request.Settings, userId);
            return CreatedAtAction(nameof(Get), new { id = rule.Id }, rule);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (SqliteException exception) when (exception.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new { message = "Reglerne blev ændret samtidig. Prøv igen." });
        }
    }

    public class CreateHolidayRuleRequest
    {
        public DateOnly EffectiveFrom { get; set; }

        [Required]
        public HolidayRuleSettings? Settings { get; set; }
    }
}
