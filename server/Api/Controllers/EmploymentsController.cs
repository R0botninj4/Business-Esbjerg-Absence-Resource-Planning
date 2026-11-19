using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:guid}/employments")]
public class EmploymentsController : ControllerBase
{
    private readonly EmploymentService employmentService;

    public EmploymentsController(EmploymentService service)
    {
        employmentService = service;
    }

    [HttpGet]
    public ActionResult<Employment[]> Get(Guid userId)
    {
        var isAdmin = User.IsInRole("Admin");
        var isAccounting = User.IsInRole("Accounting");
        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isOwnAccount = loggedInUserId == userId.ToString();

        if (!isAdmin && !isAccounting && !isOwnAccount)
        {
            return Forbid();
        }

        return Ok(employmentService.GetByUserId(userId));
    }
    [HttpPatch("{employmentId:guid}/end")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult End(Guid userId, Guid employmentId, EndEmploymentRequest request)
    {
        var employment = employmentService.Find(userId, employmentId);

        if (employment is null)
        {
            return NotFound();
        }

        if (request.EndDate < employment.StartDate)
        {
            return BadRequest(new { message = "Slutdatoen må ikke være før startdatoen." });
        }

        if (!employmentService.End(userId, employmentId, request.EndDate))
        {
            return Conflict(new { message = "Perioden er allerede afsluttet, eller en ugeplan starter efter slutdatoen." });
        }

        return NoContent();
    }

    public class EndEmploymentRequest
    {
        public DateOnly EndDate { get; set; }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Employment), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<Employment> Create(Guid userId, CreateEmploymentRequest request)
    {
        if (!Enum.IsDefined(request.Type))
        {
            return BadRequest(new { message = "Vælg en gyldig ansættelsestype." });
        }

        try
        {
            var employment = employmentService.Create(
                userId, request.Type, request.StartDate, request.EndDate, request.Days);

            if (employment is null)
            {
                return NotFound(new { message = "Ingen aktiv medarbejder med det ID." });
            }

            return StatusCode(StatusCodes.Status201Created, employment);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (EmploymentOverlapException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    public class CreateEmploymentRequest
    {
        public EmploymentType Type { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public WorkScheduleDayRequest[]? Days { get; set; }
    }
}
