using Service;

namespace Service.UnitTests;

public class UserEmailTests
{
    [Fact]
    public void Employee_CanUseCompanyEmail()
    {
        UserService.ValidateEmail("mads@businessesbjerg.com", "Employee");
    }

    [Fact]
    public void Employee_CanUseUppercaseAndSpaces()
    {
        UserService.ValidateEmail(" MADS@BUSINESSESBJERG.COM ", "Employee");
    }

    [Fact]
    public void Employee_CannotUseGmail()
    {
        Assert.Throws<ArgumentException>(() =>
            UserService.ValidateEmail("mads@gmail.com", "Employee"));
    }

    [Fact]
    public void Employee_CannotUseSimilarDomain()
    {
        Assert.Throws<ArgumentException>(() =>
            UserService.ValidateEmail("mads@businessesbjerg.com.example.com", "Employee"));
    }

    [Fact]
    public void Employee_MustHaveNameBeforeAtSign()
    {
        Assert.Throws<ArgumentException>(() =>
            UserService.ValidateEmail("@businessesbjerg.com", "Employee"));
    }

    [Fact]
    public void Employee_CannotUseTwoAtSigns()
    {
        Assert.Throws<ArgumentException>(() =>
            UserService.ValidateEmail("mads@gmail.com@businessesbjerg.com", "Employee"));
    }

    [Fact]
    public void Employee_CannotUseEmptyEmail()
    {
        Assert.Throws<ArgumentException>(() =>
            UserService.ValidateEmail("", "Employee"));
    }

    [Fact]
    public void Accounting_CanUseAnotherDomain()
    {
        UserService.ValidateEmail("accounting@example.test", "Accounting");
    }

    [Fact]
    public void Admin_CanUseAnotherDomain()
    {
        UserService.ValidateEmail("admin@example.test", "Admin");
    }
}
