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
    private readonly HolidayDecisionService decisions;

    public HolidayRequestsController(HolidayRequestService requestService, UserService userService, HolidayDecisionService decisionService)
    {
        requests = requestService;
        users = userService;
        decisions = decisionService;
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

    [HttpGet("review")]
    [Authorize(Roles = "Accounting")]
    public IEnumerable<HolidayRequestResponse> GetForAccounting()
    {
        return requests.GetForAccounting().Select(ToResponse);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Employee,Accounting")]
    public ActionResult<HolidayRequestResponse> Get(Guid id)
    {
        var request = requests.FindOwn(CurrentUserId(), id);
        if (User.IsInRole("Accounting"))
        {
            request = requests.Find(id);
        }
        if (request == null)
        {
            return NotFound();
        }
        return Ok(ToResponse(request));
    }

    [HttpGet("{id:guid}/days")]
    [Authorize(Roles = "Employee,Accounting")]
    public ActionResult<HolidayRequestDay[]> GetDays(Guid id)
    {
        if (requests.Find(id) == null)
        {
            return NotFound();
        }
        if (requests.FindOwn(CurrentUserId(), id) == null && !User.IsInRole("Accounting"))
        {
            return NotFound();
        }
        return Ok(requests.GetDays(id));
    }

    [HttpPost("{id:guid}/decision")]
    [Authorize(Roles = "Accounting")]
    public ActionResult<HolidayRequestResponse> Decide(Guid id, HolidayDecisionInput input)
    {
        try
        {
            var request = decisions.Decide(id, CurrentUserId(), input);
            if (request == null)
            {
                return NotFound();
            }
            return Ok(ToResponse(request));
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

    [HttpPost("{id:guid}/cancellation")]
    [Authorize(Roles = "Employee")]
    public ActionResult<HolidayRequestResponse> RequestCancellation(Guid id, [FromServices] HolidayCancellationService cancellations)
    {
        try
        {
            var request = cancellations.Request(CurrentUserId(), id);
            if (request == null) return NotFound();
            return Ok(ToResponse(request));
        }
        catch (HolidayRequestConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/cancellation/decision")]
    [Authorize(Roles = "Accounting")]
    public ActionResult<HolidayRequestResponse> DecideCancellation(Guid id, HolidayDecisionInput input,
        [FromServices] HolidayCancellationService cancellations)
    {
        try
        {
            var request = cancellations.Decide(id, CurrentUserId(), input);
            if (request == null) return NotFound();
            return Ok(ToResponse(request));
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
        response.DecidedAt = request.DecidedAt;
        response.DecidedBy = request.DecidedBy;
        response.Comment = request.DecisionComment;
        response.CancellationStatus = request.CancellationStatus;
        response.CancellationRequestedAt = request.CancellationRequestedAt;
        response.CancellationDecidedAt = request.CancellationDecidedAt;
        response.CancellationDecidedBy = request.CancellationDecidedBy;
        response.CancellationComment = request.CancellationComment;
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
        public DateTime? DecidedAt { get; set; }
        public Guid? DecidedBy { get; set; }
        public string Comment { get; set; } = "";
        public string CancellationStatus { get; set; } = "";
        public DateTime? CancellationRequestedAt { get; set; }
        public DateTime? CancellationDecidedAt { get; set; }
        public Guid? CancellationDecidedBy { get; set; }
        public string CancellationComment { get; set; } = "";
    }
}
