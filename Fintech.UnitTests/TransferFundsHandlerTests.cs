using Fintech.Commands;
using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.ValueObjects;
using Moq;

namespace Fintech.UnitTests;

public class TransferFundsHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<ILedgerRepository> _ledgerRepoMock;
    private readonly Mock<ITransactionManager> _txManagerMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly TransferFundsHandler _handler;

    public TransferFundsHandlerTests()
    {
        _accountRepoMock = new Mock<IAccountRepository>();
        _ledgerRepoMock = new Mock<ILedgerRepository>();
        _txManagerMock = new Mock<ITransactionManager>();
        _uowMock = new Mock<IUnitOfWork>();

        _txManagerMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(_uowMock.Object);

        _handler = new TransferFundsHandler(
            _txManagerMock.Object,
            _accountRepoMock.Object,
            _ledgerRepoMock.Object
        );
    }

    [Fact]
    public async Task Deve_Transferir_Valores_Corretamente()
    {
        // Arrange
        var accFrom = new Account(Guid.NewGuid());
        accFrom.Credit(Money.BRL(200));

        var accTo = new Account(Guid.NewGuid());
        // Saldo 0

        _accountRepoMock.Setup(x => x.GetByIdAsync(accFrom.Id)).ReturnsAsync(accFrom);
        _accountRepoMock.Setup(x => x.GetByIdAsync(accTo.Id)).ReturnsAsync(accTo);

        // Act
        await _handler.Handle(accFrom.Id, accTo.Id, 50m);

        // Assert
        // Verifica atualização da origem
        _accountRepoMock.Verify(x => x.UpdateAsync(It.Is<Account>(a => 
            a.Id == accFrom.Id && a.Balances["BRL"].Amount == 150m)), Times.Once);

        // Verifica atualização do destino
        _accountRepoMock.Verify(x => x.UpdateAsync(It.Is<Account>(a => 
            a.Id == accTo.Id && a.Balances["BRL"].Amount == 50m)), Times.Once);

        // Verifica 2 eventos no Ledger
        _ledgerRepoMock.Verify(x => x.AddAsync(It.IsAny<LedgerEvent>()), Times.Exactly(2));

        _uowMock.Verify(x => x.CommitAsync(default), Times.Once);
    }
}