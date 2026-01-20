using Fintech.Enums;

namespace Fintech.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Identifier { get; private set; }
    public TenantBranding Branding { get; private set; }
    public List<Jurisdiction> ActiveJurisdictions { get; private set; }
    public List<BusinessMode> ActiveModes { get; private set; }
    public Dictionary<string, string> RegulatoryConfig { get; private set; } // Declarative rules per country
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Tenant(string name, string identifier, TenantBranding branding)
    {
        Id = Guid.NewGuid();
        Name = name;
        Identifier = identifier.ToLowerInvariant();
        Branding = branding;
        ActiveJurisdictions = new List<Jurisdiction>();
        ActiveModes = new List<BusinessMode>();
        RegulatoryConfig = new Dictionary<string, string>();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddJurisdiction(Jurisdiction jurisdiction) => ActiveJurisdictions.Add(jurisdiction);
    public void AddBusinessMode(BusinessMode mode) => ActiveModes.Add(mode);
    public void SetRegulatoryConfig(string key, string value) => RegulatoryConfig[key] = value;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void UpdateBranding(TenantBranding branding) => Branding = branding;
}

public class TenantBranding
{
    public string PrimaryColor { get; set; } = "#8B5CF6";
    public string LogoUrl { get; set; } = string.Empty;
    public string FaviconUrl { get; set; } = string.Empty;
    public string CustomDomain { get; set; } = string.Empty;
}
