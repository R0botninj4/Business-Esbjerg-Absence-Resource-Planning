using LinqToDB.Mapping;

namespace Infra;

[Table("users")]
public class User
{
    [PrimaryKey, Column("id")]
    public Guid Id { get; set; }

    [Column("email"), NotNull]
    public string Email { get; set; } = "";

    [Column("password_hash"), NotNull]
    public string PasswordHash { get; set; } = "";

    [Column("role"), NotNull]
    public string Role { get; set; } = "Employee";

    [Column("is_active"), NotNull]
    public bool IsActive { get; set; } = true;
}