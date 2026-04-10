namespace Gateway.Auth;

/// <summary>
/// In-memory user store for the assessment.
/// In production, replaced by a proper identity provider.
/// </summary>
public static class UserStore
{
    private static readonly Dictionary<string, string> Users = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = "admin",
        ["operator"] = "operator",
        ["viewer"] = "viewer",
        ["simulator"] = "simulator"
    };

    public static bool ValidateCredentials(string username, string password)
    {
        return Users.TryGetValue(username, out var storedPassword) && storedPassword == password;
    }
}
