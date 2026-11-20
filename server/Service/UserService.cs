using Infra;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using LinqToDB;

namespace Service;

public class UserService(MyDatabaseConnection db, IPasswordHasher<User> passwordHasher)
{
    public User[] List() => db.Users.OrderBy(user => user.Email).ToArray();

    public User? Find(Guid id) => db.Users.SingleOrDefault(user => user.Id == id);

    public User Create(string email, string password, string role)
    {
        ValidateEmail(email, role);
        if (!IsRoleAccount(role))
        {
            return SaveUser(email, password, role);
        }

        using var transaction = db.BeginTransaction();
        CheckAvailableRole(role, Guid.Empty);
        var user = SaveUser(email, password, role);
        transaction.Commit();
        return user;
    }

    private User SaveUser(string email, string password, string role)
    {
        var user = new User { Id = Guid.NewGuid(), Email = NormalizeEmail(email), Role = role };
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        db.Insert(user);
        return user;
    }

    public bool Update(Guid id, string email, string role)
    {
        ValidateEmail(email, role);
        using var transaction = db.BeginTransaction();
        var user = Find(id);
        if (user == null)
        {
            return false;
        }

        if (IsRoleAccount(user.Role) && user.Role != role)
        {
            throw new AccountRoleConflictException(
                "Systemejerens og regnskabets konti skal beholde deres roller.");
        }

        CheckAvailableRole(role, id);
        var updatedRows = db.Users
            .Where(user => user.Id == id)
            .Set(user => user.Email, NormalizeEmail(email))
            .Set(user => user.Role, role)
            .Update();

        transaction.Commit();
        return updatedRows != 0;
    }

    public bool Delete(Guid id)
    {
        var user = Find(id);
        if (user == null)
        {
            return false;
        }

        if (IsRoleAccount(user.Role))
        {
            throw new AccountRoleConflictException(
                "Systemejerens og regnskabets konti kan ikke deaktiveres.");
        }

        return db.Users.Where(user => user.Id == id &&
                                      user.Role != "Admin" && user.Role != "Accounting")
            .Set(user => user.IsActive, false)
            .Update() != 0;
    }

    private static bool IsRoleAccount(string role)
    {
        return role == "Admin" || role == "Accounting";
    }

    private void CheckAvailableRole(string role, Guid userId)
    {
        if (!IsRoleAccount(role))
        {
            return;
        }

        var alreadyExists = db.Users.Any(user => user.Role == role && user.Id != userId);
        if (alreadyExists)
        {
            throw new AccountRoleConflictException(
                "Der findes allerede en konto med rollen " + role + ".");
        }
    }

    public bool EmailExists(string email) => db.Users.Any(user => user.Email == NormalizeEmail(email));

    public User? VerifyLogin(string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = db.Users.SingleOrDefault(user => user.Email == normalizedEmail);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    public static void ValidateEmail(string email, string role)
    {
        if (role != "Employee")
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Angiv medarbejderens e-mail.", nameof(email));
        }

        var address = email.Trim();
        var emailValidator = new EmailAddressAttribute();

        if (!emailValidator.IsValid(address) ||
            !address.EndsWith("@businessesbjerg.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Medarbejdere skal have en e-mail på @businessesbjerg.com.", nameof(email));
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
