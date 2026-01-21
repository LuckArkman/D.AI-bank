using Fintech.Commands;
using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Repositories;
using Fintech.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace Fintech.UnitTests;

public class DebitAccountHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<ILedgerRepository> _ledgerRepoMock;
    private readonly Mock<IOutboxRepository> _outboxRepoMock;
    private readonly Mock<ITransactionManager> _txManagerMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly DebitAccountHandler _handler;

    public DebitAccountHandlerTests()
    {
        _accountRepoMock = new Mock<IAccountRepository>();
        _ledgerRepoMock = new Mock<ILedgerRepository>();
        _outboxRepoMock = new Mock<IOutboxRepository>();
        _txManagerMock = new Mock<ITransactionManager>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _uowMock = new Mock<IUnitOfWork>();

        var tenantId = Guid.NewGuid();
        _tenantProviderMock.Setup(x => x.TenantId).Returns(tenantId);
        _txManagerMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(_uowMock.Object);

        _handler = new DebitAccountHandler(
            _txManagerMock.Object,
            _accountRepoMock.Object,
            _outboxRepoMock.Object,
            _tenantProviderMock.Object
        );
    }

    [Fact]
    public async Task Deve_Realizar_Debito_Com_Sucesso()
    {
        // Arrange
        var tenantId = _tenantProviderMock.Object.TenantId.Value;
        var accountId = Guid.NewGuid();
        var account = new Account(accountId, tenantId);
        account.Credit(Money.BRL(100)); // Saldo inicial 100

        _accountRepoMock.Setup(x => x.GetByIdAsync(accountId)).ReturnsAsync(account);

        // Act
        await _handler.Handle(accountId, 40m, Guid.NewGuid());

        // Assert
        // 1. Saldo deve ser atualizado
        _accountRepoMock.Verify(x => x.UpdateAsync(It.Is<Account>(a =>
            a.Balances["BRL"].Amount == 60m)), Times.Once);

        // 2. Outbox deve ser gerado
        _outboxRepoMock.Verify(x => x.AddAsync(It.Is<OutboxMessage>(m =>
            m.Topic == "AccountDebited")), Times.Once);

        // 3. Commit realizado
        _uowMock.Verify(x => x.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task Deve_Falhar_Se_Saldo_Insuficiente_Sem_Commit()
    {
        // Arrange
        var tenantId = _tenantProviderMock.Object.TenantId.Value;
        var account = new Account(Guid.NewGuid(), tenantId); // Saldo 0
        _accountRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(account);

        // Act
        Func<Task> action = async () => await _handler.Handle(account.Id, 50m, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("*Saldo insuficiente*");

        // Garante que NADA foi salvo
        _accountRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Account>()), Times.Never);
        _uowMock.Verify(x => x.CommitAsync(default), Times.Never);
    }
}