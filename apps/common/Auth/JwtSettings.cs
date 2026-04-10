namespace Common.Auth;

/// <summary>
/// JWT configuration shared across all services.
/// Values are read from appsettings.json or environment variables.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = "AirlineTransactionMonitorSuperSecretKey2026!@#$%";
    public string Issuer { get; set; } = "airline-transaction-monitor";
    public string Audience { get; set; } = "airline-transaction-monitor-clients";
    public int ExpirationMinutes { get; set; } = 60;
}
