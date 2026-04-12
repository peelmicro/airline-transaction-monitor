using Gateway.Auth;

namespace Gateway.UnitTests;

public class UserStoreTests
{
    [Theory]
    [InlineData("admin", "admin")]
    [InlineData("operator", "operator")]
    [InlineData("viewer", "viewer")]
    [InlineData("simulator", "simulator")]
    public void ValidateCredentials_ReturnsTrue_ForValidCredentials(string username, string password)
    {
        var result = UserStore.ValidateCredentials(username, password);

        Assert.True(result);
    }

    [Fact]
    public void ValidateCredentials_ReturnsFalse_ForInvalidPassword()
    {
        var result = UserStore.ValidateCredentials("admin", "wrongpassword");

        Assert.False(result);
    }

    [Fact]
    public void ValidateCredentials_ReturnsFalse_ForNonExistentUser()
    {
        var result = UserStore.ValidateCredentials("nonexistent", "password");

        Assert.False(result);
    }

    [Fact]
    public void ValidateCredentials_IsCaseInsensitiveForUsername()
    {
        var result = UserStore.ValidateCredentials("ADMIN", "admin");

        Assert.True(result);
    }

    [Fact]
    public void ValidateCredentials_IsCaseSensitiveForPassword()
    {
        var result = UserStore.ValidateCredentials("admin", "ADMIN");

        Assert.False(result);
    }

    [Fact]
    public void ValidateCredentials_ReturnsFalse_ForEmptyCredentials()
    {
        var result = UserStore.ValidateCredentials("", "");

        Assert.False(result);
    }
}
