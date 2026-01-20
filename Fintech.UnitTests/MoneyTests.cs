using Fintech.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Fintech.UnitTests;

public class MoneyTests
{
    [Fact]
    public void Deve_Somar_Valores_Da_Mesma_Moeda()
    {
        var m1 = Money.BRL(100);
        var m2 = Money.BRL(50);

        var result = m1 + m2;

        result.Amount.Should().Be(150);
        result.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Deve_Lancar_Erro_Ao_Somar_Moedas_Diferentes()
    {
        var m1 = Money.BRL(100);
        var m2 = Money.USD(50);

        var action = () => m1 + m2;

        action.Should().Throw<Exception>().WithMessage("*Moedas diferentes*");
    }

    [Fact]
    public void Deve_Subtrair_Corretamente()
    {
        var m1 = Money.BRL(100);
        var m2 = Money.BRL(40);

        var result = m1 - m2;

        result.Amount.Should().Be(60);
    }
}