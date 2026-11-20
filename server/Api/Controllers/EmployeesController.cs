using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Service;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService employees;

    public EmployeesController(EmployeeService service)
    {
        employees = service;
    }

    [HttpPost]
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
        catch (SqliteException exception)
            when (exception.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new { message = "E-mailadressen er allerede i brug." });
        }
    }
}