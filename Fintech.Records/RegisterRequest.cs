using System.ComponentModel.DataAnnotations;
using Fintech.Enums;

namespace Fintech.Records;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string Name,
    [Range(0, 1000000)] decimal InitialDeposit = 0,
    AccountProfileType ProfileType = AccountProfileType.StandardIndividual,
    string CurrencyCode = "BRL");


public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(string Token, string Name, string Email, Guid AccountId);