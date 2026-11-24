using System.Security.Claims;
using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/holiday-requests")]
public class HolidayRequestsController : ControllerBase
{
    private readonly HolidayRequestService requests;
    private readonly UserService users;

    public HolidayRequestsController(HolidayRequestService requestService, UserService userService)
    {
        requests = requestService;
        users = userService;
    }

    private Guid CurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(id!);
    }

    [HttpGet]
    [Authorize(Roles = "Employee")]
    public IEnumerable<HolidayRequestResponse> GetOwn()
    {
        return requests.GetOwn(CurrentUserId()).Select(ToResponse);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Accounting")]
    public IEnumerable<HolidayRequestResponse> GetPending()
    {
        return requests.GetPending().Select(ToResponse);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Employee")]
    public ActionResult<HolidayRequestResponse> Get(Guid id)
    {
        var request = requests.FindOwn(CurrentUserId(), id);
        if (request == null)
        {
            return NotFound();
        }
        return Ok(ToResponse(request));
    }

    [HttpGet("{id:guid}/days")]
    [Authorize(Roles = "Employee")]
    public ActionResult<HolidayRequestDay[]> GetDays(Guid id)
    {
        if (requests.FindOwn(CurrentUserId(), id) == null)
        {
            return NotFound();
        }
        return Ok(requests.GetDays(id));
    }

    [HttpPost("preview")]
    [Authorize(Roles = "Employee")]
    public ActionResult<HolidayRequestPreview> Preview(HolidayRequestInput input)
    {
        try
        {
            return Ok(requests.Preview(CurrentUserId(), input));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (HolidayRequestConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Employee")]
    public ActionResult<HolidayRequestResponse> Create(HolidayRequestInput input)
    {
        try
        {
            var request = requests.Create(CurrentUserId(), input);
            return CreatedAtAction(nameof(Get), new { id = request.Id }, ToResponse(request));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (HolidayRequestConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Employee")]
    public IActionResult Withdraw(Guid id)
    {
        try
        {
            if (!requests.Withdraw(CurrentUserId(), id))
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (HolidayRequestConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    private HolidayRequestResponse ToResponse(HolidayRequest request)
    {
        var response = new HolidayRequestResponse();
        response.Id = request.Id;
        response.UserId = request.UserId;
        response.Email = users.Find(request.UserId)?.Email ?? "";
        response.Account = request.Account;
        response.StartDate = request.StartDate;
        response.EndDate = request.EndDate;
        response.Hours = request.Hours;
        response.Status = request.Status;
        response.CreatedAt = request.CreatedAt;
        response.WithdrawnAt = request.WithdrawnAt;
        return response;
    }

    public class HolidayRequestResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = "";
        public HolidayAccount Account { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Hours { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? WithdrawnAt { get; set; }
    }
}
