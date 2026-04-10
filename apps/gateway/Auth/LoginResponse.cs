namespace Gateway.Auth;

public record LoginResponse(string Token, string Username, DateTime ExpiresAt);
