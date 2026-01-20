using Fintech.Commands;
using Fintech.Core.Entities;
using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Repositories;
using Moq;
using Xunit;

namespace Fintech.UnitTests;

public class CreateAccountHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<ILedgerRepository> _ledgerRepoMock;
    private readonly Mock<ITransactionManager> _txManagerMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly CreateAccountHandler _handler;

    public CreateAccountHandlerTests()
    {
        _accountRepoMock = new Mock<IAccountRepository>();
        _ledgerRepoMock = new Mock<ILedgerRepository>();
        _txManagerMock = new Mock<ITransactionManager>();
        _uowMock = new Mock<IUnitOfWork>();

        _txManagerMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(_uowMock.Object);

        _handler = new CreateAccountHandler(
            (_accountRepoMock.Object as AccountRepository),
            _txManagerMock.Object,
            (_ledgerRepoMock.Object as LedgerRepository)
        );
    }

    [Fact]
    public async Task Deve_Criar_Conta_E_Registrar_Ledger()
    {
        // Act
        var result = await _handler.Handle(1000m);

        // Assert
        // 1. Verifica persistência da conta
        _accountRepoMock.Verify(x => x.AddAsync(It.Is<Account>(a => 
            a.Balances["BRL"].Amount == 1000m)), Times.Once);

        // 2. Verifica registro no Ledger
        _ledgerRepoMock.Verify(x => x.AddAsync(It.Is<LedgerEvent>(e => 
            e.Type == "ACCOUNT_CREATED" && 
            e.Amount == 1000m)), Times.Once);

        // 3. Verifica Commit da transação
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}