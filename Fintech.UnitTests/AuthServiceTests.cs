using System.Transactions;
using Fintech.Commands;
using Fintech.Core.Entities;
using Fintech.Core.Interfaces;
using Fintech.Services;
using Fintech.Interfaces;
using Fintech.Records;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace Fintech.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ICreateAccountHandler> _accountHandlerMock; // Interface criada anteriormente
    private readonly Mock<ITransactionManager> _txManagerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _accountHandlerMock = new Mock<ICreateAccountHandler>();
        _txManagerMock = new Mock<ITransactionManager>();
        _configMock = new Mock<IConfiguration>();

        // Mock do JWT Secret
        _configMock.Setup(x => x["Jwt:Secret"]).Returns("Segredo_Super_Secreto_Para_Testes_Unitarios_123");

        _service = new AuthService(
            _userRepoMock.Object,
            (_accountHandlerMock.Object as CreateAccountHandler),
            _txManagerMock.Object,
            _configMock.Object
        );
    }

    [Fact]
    public async Task Register_Deve_Criar_Usuario_E_Conta_Quando_Email_Unico()
    {
        // Arrange
        string email = "teste@email.com";
        string password = "password123";
        var accountId = Guid.NewGuid();

        _userRepoMock.Setup(x => x.ExistsByEmailAsync(email)).ReturnsAsync(false);
        _accountHandlerMock.Setup(x => x.Handle(0)).ReturnsAsync(accountId);

        // Act
        var token = await _service.RegisterAsync(new RegisterRequest(email, password, "teste"));

        // Assert
        token.Should().NotBeNull();
        
        // Verifica se salvou o usuário
        _userRepoMock.Verify(x => x.AddAsync(It.Is<User>(u => 
            u.Email == email && 
            u.AccountId == accountId)), Times.Once);
    }

    [Fact]
    public async Task Register_Deve_Falhar_Se_Email_Ja_Existe()
    {
        // Arrange
        _userRepoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        Func<Task> action = async () => await _service.RegisterAsync(new RegisterRequest("existente@email.com", "123", "teste"));

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("*Email já cadastrado*");
        _accountHandlerMock.Verify(x => x.Handle(It.IsAny<decimal>()), Times.Never);
    }
}