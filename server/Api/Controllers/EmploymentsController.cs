using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users/{userId:guid}/employments")]
public class EmploymentsController(EmploymentService employmentService) : ControllerBase
{
    [HttpGet]
    public Employment[] Get(Guid userId)
    {
        return employmentService.GetByUserId(userId);
    }
    [HttpPatch("{employmentId:guid}/end")]
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
            return Conflict(new { message = "Ansættelsesperioden er allerede afsluttet." });
        }

        return NoContent();
    }

    public record EndEmploymentRequest(DateOnly EndDate);

    [HttpPost]
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
                userId, request.Type, request.StartDate, request.EndDate);

            if (employment is null)
            {
                return NotFound(new { message = "Ingen aktiv medarbejder med det ID." });
            }

            return StatusCode(StatusCodes.Status201Created, employment);
        }
        catch (ArgumentException exception)
            when (exception.ParamName is "startDate" or "endDate")
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (EmploymentOverlapException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    public record CreateEmploymentRequest(
        EmploymentType Type,
        DateOnly StartDate,
        DateOnly? EndDate);
}
