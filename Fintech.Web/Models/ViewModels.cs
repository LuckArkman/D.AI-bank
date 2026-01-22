using System.ComponentModel.DataAnnotations;
using Fintech.Enums;

namespace Fintech.Web.Models;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public decimal InitialDeposit { get; set; } = 0;
}

public class IssueCardViewModel
{
    [Required]
    public string Brand { get; set; } = "Mastercard";

    public CardType Type { get; set; } = CardType.Debit;

    public bool IsVirtual { get; set; } = true;

    [Range(0, 1000000)]
    public decimal CreditLimit { get; set; } = 0;
}
