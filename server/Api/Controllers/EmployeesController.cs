using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Service;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Accounting")]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService employees;

    public EmployeesController(EmployeeService service)
    {
        employees = service;
    }

    [HttpGet]
    public IEnumerable<UsersController.UserResponse> List([FromServices] UserService users)
    {
        return users.List()
            .Where(user => user.Role == "Employee")
            .Select(UsersController.UserResponse.From);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public ActionResult<UsersController.UserResponse> Create(
        CreateEmployeeRequest request)
    {
        try
        {
            var user = employees.Create(
                request.Email,
                request.Password,
                request.Type,
                request.StartDate,
                request.EndDate,
                request.Days);

            var response = UsersController.UserResponse.From(user);

            return CreatedAtAction("Get", "Users", new { id = user.Id }, response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { message = "E-mailadressen er allerede i brug." });
        }
    }
}
