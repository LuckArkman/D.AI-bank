namespace Fintech.Interfaces;

public record BridgeResponse(bool Success, string TransactionHash, decimal FinalAmount, decimal Fee);

public interface IWeb3BridgeGateway
{
    Task<BridgeResponse> BridgeLiquidityAsync(string sourceNetwork, string targetNetwork, decimal amount, string currencyCode);
}
