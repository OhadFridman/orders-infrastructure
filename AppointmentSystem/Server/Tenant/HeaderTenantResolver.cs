using System.Linq;
using Microsoft.AspNetCore.Http;

namespace AppointmentSystem.Server.Tenant;

public class HeaderTenantResolver : ITenantResolver
{
    private const string HeaderName = "X-Tenant-Slug";
    public string ResolveTenantSlug(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            var slug = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(slug)) return slug!;
        }

        // fallback default
        return "default";
    }
}

