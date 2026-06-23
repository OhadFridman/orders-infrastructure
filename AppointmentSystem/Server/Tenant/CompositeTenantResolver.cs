using Microsoft.AspNetCore.Http;

namespace AppointmentSystem.Server.Tenant;

// Composite resolver: prefer host-based subdomain resolution, fall back to header, then default
public class CompositeTenantResolver : ITenantResolver
{
    private readonly HostTenantResolver _hostResolver = new();
    private readonly HeaderTenantResolver _headerResolver = new();

    public string ResolveTenantSlug(HttpContext httpContext)
    {
        // 1. try host/subdomain
        var hostSlug = _hostResolver.ResolveTenantSlug(httpContext);
        if (!string.IsNullOrWhiteSpace(hostSlug)) return hostSlug;

        // 2. try header
        var headerSlug = _headerResolver.ResolveTenantSlug(httpContext);
        if (!string.IsNullOrWhiteSpace(headerSlug)) return headerSlug;

        // 3. fallback
        return "default";
    }
}

