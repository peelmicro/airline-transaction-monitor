using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Auth;
using Gateway.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.UnitTests;

public class TokenServiceTests
{
    private readonly JwtSettings _settings;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _settings = new JwtSettings
        {
            Secret = "AirlineTransactionMonitorSuperSecretKey2026!@#$%",
            Issuer = "airline-transaction-monitor",
            Audience = "airline-transaction-monitor-clients",
            ExpirationMinutes = 60
        };

        _tokenService = new TokenService(Options.Create(_settings));
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyToken()
    {
        var result = _tokenService.GenerateToken("admin");

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public void GenerateToken_ReturnsCorrectUsername()
    {
        var result = _tokenService.GenerateToken("admin");

        Assert.Equal("admin", result.Username);
    }

    [Fact]
    public void GenerateToken_SetsExpirationInFuture()
    {
        var before = DateTime.UtcNow;
        var result = _tokenService.GenerateToken("admin");

        Assert.True(result.ExpiresAt > before);
        Assert.True(result.ExpiresAt <= DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes + 1));
    }

    [Fact]
    public void GenerateToken_ContainsCorrectIssuer()
    {
        var result = _tokenService.GenerateToken("admin");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        Assert.Equal(_settings.Issuer, token.Issuer);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectAudience()
    {
        var result = _tokenService.GenerateToken("admin");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        Assert.Contains(_settings.Audience, token.Audiences);
    }

    [Fact]
    public void GenerateToken_ContainsNameClaim()
    {
        var result = _tokenService.GenerateToken("operator");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        var nameClaim = token.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Name ||
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");

        Assert.NotNull(nameClaim);
        Assert.Equal("operator", nameClaim.Value);
    }

    [Fact]
    public void GenerateToken_ContainsRoleClaim()
    {
        var result = _tokenService.GenerateToken("admin");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        var roleClaim = token.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Role ||
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

        Assert.NotNull(roleClaim);
        Assert.Equal("user", roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_ContainsJtiClaim()
    {
        var result = _tokenService.GenerateToken("admin");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        var jtiClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

        Assert.NotNull(jtiClaim);
        Assert.True(Guid.TryParse(jtiClaim.Value, out _));
    }

    [Fact]
    public void GenerateToken_TokenCanBeValidated()
    {
        var result = _tokenService.GenerateToken("admin");

        var handler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret))
        };

        var principal = handler.ValidateToken(result.Token, validationParams, out var validatedToken);

        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);
    }
}
