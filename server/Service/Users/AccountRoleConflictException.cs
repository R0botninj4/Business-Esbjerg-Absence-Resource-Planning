namespace Service;

public class AccountRoleConflictException : Exception
{
    public AccountRoleConflictException(string message)
        : base(message)
    {
    }
}
