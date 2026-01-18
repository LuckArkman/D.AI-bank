namespace Fintech.Records;

public record RegisterRequest(string Email, string Password, string Name, decimal InitialDeposit = 0);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, string Name, string Email, Guid AccountId);