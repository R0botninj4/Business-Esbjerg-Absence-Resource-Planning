using Infra;
using Microsoft.AspNetCore.Identity;
using LinqToDB;

namespace Service;

public class UserService(MyDatabaseConnection db, IPasswordHasher<User> passwordHasher)
{
    public User[] List() => db.Users.OrderBy(user => user.Email).ToArray();

    public User? Find(Guid id) => db.Users.SingleOrDefault(user => user.Id == id);

    public User Create(string email, string password, string role)
    {
        var user = new User { Id = Guid.NewGuid(), Email = NormalizeEmail(email), Role = role };
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        db.Insert(user);
        return user;
    }

    public bool Update(Guid id, string email, string role) => db.Users
        .Where(user => user.Id == id)
        .Set(user => user.Email, NormalizeEmail(email))
        .Set(user => user.Role, role)
        .Update() != 0;

    public bool Delete(Guid id) => db.Users.Where(user => user.Id == id).Delete() != 0;

    public bool EmailExists(string email) => db.Users.Any(user => user.Email == NormalizeEmail(email));

    public User? VerifyLogin(string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = db.Users.SingleOrDefault(user => user.Email == normalizedEmail);

        if (user is null)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
