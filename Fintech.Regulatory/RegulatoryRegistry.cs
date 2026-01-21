using Fintech.Enums;

namespace Fintech.Regulatory;

public interface IRegulatoryRegistry
{
    void RegisterPack(IRegulatoryPack pack);
    IRegulatoryPack GetPack(Jurisdiction jurisdiction);
}

public class RegulatoryRegistry : IRegulatoryRegistry
{
    private readonly Dictionary<Jurisdiction, IRegulatoryPack> _packs = new();

    public void RegisterPack(IRegulatoryPack pack)
    {
        _packs[pack.Jurisdiction] = pack;
    }

    public IRegulatoryPack GetPack(Jurisdiction jurisdiction)
    {
        if (!_packs.TryGetValue(jurisdiction, out var pack))
        {
            // Fallback to Global or throw
            return _packs.GetValueOrDefault(Jurisdiction.Global)
                ?? throw new Exception($"Nenhum Regulatory Pack encontrado para {jurisdiction}");
        }
        return pack;
    }
}
