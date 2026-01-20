using Fintech.Entities;
using Fintech.ValueObjects;
using FluentAssertions;

namespace Fintech.UnitTests;

public class AccountTests
{
    [Fact]
    public void Deve_Debitar_Quando_Saldo_Suficiente()
    {
        // Arrange
        var account = new Account(Guid.NewGuid());
        account.Credit(Money.BRL(100)); // Saldo inicial

        // Act
        account.Debit(Money.BRL(40));

        // Assert
        account.Balances["BRL"].Amount.Should().Be(60);
    }

    [Fact]
    public void Deve_Lancar_Erro_Quando_Saldo_Insuficiente()
    {
        // Arrange
        var account = new Account(Guid.NewGuid());
        account.Credit(Money.BRL(50));

        // Act
        var action = () => account.Debit(Money.BRL(100));

        // Assert
        action.Should().Throw<Exception>().WithMessage("*Saldo insuficiente*");
    }

    [Fact]
    public void Deve_Creditar_Corretamente()
    {
        var account = new Account(Guid.NewGuid());
        account.Credit(Money.BRL(10));
        
        account.Balances["BRL"].Amount.Should().Be(10);
    }
}