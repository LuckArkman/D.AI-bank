namespace Fintech.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Identifier { get; private set; } // e.g., subdomain or unique slug
    public TenantBranding Branding { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Tenant(string name, string identifier, TenantBranding branding)
    {
        Id = Guid.NewGuid();
        Name = name;
        Identifier = identifier.ToLowerInvariant();
        Branding = branding;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

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
