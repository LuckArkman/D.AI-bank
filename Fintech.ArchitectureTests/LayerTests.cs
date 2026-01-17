namespace Fintech.ArchitectureTests;

using NetArchTest.Rules;
using Xunit;
using Fintech.Entities;
using Fintech.Commands;

public class LayerTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Account).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Fintech.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}