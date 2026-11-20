using System.ComponentModel.DataAnnotations;
using Infra;
using Service;

namespace Api.Models;

public class CreateEmployeeRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";

    [Required, StringLength(128, MinimumLength = 12)]
    public string Password { get; set; } = "";

    public EmploymentType Type { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public WorkScheduleDayRequest[]? Days { get; set; }
}