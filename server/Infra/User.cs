using LinqToDB.Mapping;

namespace Infra;

[Table("Users")]
public class User
{
    [PrimaryKey, Column]
    public Guid Id { get; set; }

    [Column, NotNull]
    public string Email { get; set; } = "";

    [Column, NotNull]
    public string PasswordHash { get; set; } = "";

    [Column, NotNull]
    public string Role { get; set; } = "Employee";

    [Column, NotNull]
    public bool IsActive { get; set; } = true;
}