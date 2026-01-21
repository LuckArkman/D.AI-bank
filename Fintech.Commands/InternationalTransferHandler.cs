using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Exceptions;
using Fintech.Interfaces;
using Fintech.ValueObjects;
using Fintech.Regulatory;

namespace Fintech.Commands;

public class InternationalTransferHandler
{
    private readonly ITransactionManager _txManager;
    private readonly IAccountRepository _accountRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly IRegulatoryService _regulatoryService;
    private readonly IEnumerable<ISettlementGateway> _settlementGateways;
    private readonly ITaxationService _taxationService;
    private readonly ILiquidityService _liquidityService;

    public InternationalTransferHandler(
        ITransactionManager txManager,
        IAccountRepository accountRepo,
        ILedgerRepository ledgerRepo,
        ITenantProvider tenantProvider,
        IRegulatoryService regulatoryService,
        IEnumerable<ISettlementGateway> settlementGateways,
        ITaxationService taxationService,
        ILiquidityService liquidityService)
    {
        _txManager = txManager;
        _accountRepo = accountRepo;
        _ledgerRepo = ledgerRepo;
        _tenantProvider = tenantProvider;
        _regulatoryService = regulatoryService;
        _settlementGateways = settlementGateways;
        _taxationService = taxationService;
        _liquidityService = liquidityService;
    }

    public async Task<string> Handle(Guid fromAccountId, decimal amount, string currencyCode, string network, string destinationBank, string destinationAccount)
    {
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var fromAcc = await _accountRepo.GetByIdAsync(fromAccountId);
            if (fromAcc == null) throw new DomainException("Account not found");

            // 1. Regulatory Validation
            await _regulatoryService.ValidateTransactionAsync(fromAcc, amount, "INTERNATIONAL_TRANSFER");

            // 1.1 Liquidity Check
            await _liquidityService.EnsureLiquidityAsync(network, currencyCode, amount);

            // 2. Taxation
            var totalTax = await _taxationService.CalculateTotalTaxAsync(fromAcc, amount, "INTERNATIONAL_TRANSFER");
            var totalDebit = amount + totalTax;

            // 3. Debit Local Account
            var moneyToBeDebited = Money.Create(totalDebit, currencyCode);
            var netMoneyForSettlement = Money.Create(amount, currencyCode);

            fromAcc.Debit(moneyToBeDebited);
            await _accountRepo.UpdateAsync(fromAcc);

            // 3. Select Gateway
            var gateway = _settlementGateways.FirstOrDefault(g => g.Network.Equals(network, StringComparison.OrdinalIgnoreCase))
                         ?? throw new DomainException($"Network {network} not supported or not configured.");

            // 4. Record Ledger
            var correlationId = Guid.NewGuid();
            var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId context missing");
            await _ledgerRepo.AddAsync(new LedgerEvent(fromAccountId, tenantId, "INTL_TRANSFER_INITIATED", amount, currencyCode, correlationId));

            if (totalTax > 0)
            {
                await _ledgerRepo.AddAsync(new LedgerEvent(fromAccountId, tenantId, "TAX_DEBIT", totalTax, currencyCode, correlationId));
            }

            // 5. External Settlement
            var settlementResponse = await gateway.ProcessSettlementAsync(correlationId, netMoneyForSettlement, destinationBank, destinationAccount);

            if (!settlementResponse.Success)
            {
                throw new DomainException($"Settlement failed: {settlementResponse.ErrorMessage}");
            }

            // 6. Liquidity Accounting
            await _liquidityService.RegisterOutflowAsync(network, currencyCode, amount);

            await uow.CommitAsync();
            return settlementResponse.TransactionReference;
        }
        catch (Exception ex)
        {
            await uow.AbortAsync();
            throw;
        }
    }
}
