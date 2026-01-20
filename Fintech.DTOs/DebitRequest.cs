using System.ComponentModel.DataAnnotations;

namespace Fintech.DTOs;

public class DebitRequest
{
    [Required]
    [Range(0.01, 1000000, ErrorMessage = "O valor deve estar entre 0.01 e 1,000,000.")]
    public decimal Amount { get; set; }
}