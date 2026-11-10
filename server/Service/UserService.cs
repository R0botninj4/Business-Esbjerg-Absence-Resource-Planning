using Infra;
using Microsoft.AspNetCore.Identity;

namespace Service;

public class UserService(MyDatabaseConnection db, IPasswordHasher<User> passwordHasher)
{
    public User? VerifyLogin(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = db.Users.SingleOrDefault(user => user.Email == normalizedEmail);

        if (user is null)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }
}