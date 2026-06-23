using Microsoft.AspNetCore.Http;

namespace AppointmentSystem.Server.Tenant;

public interface ITenantResolver
{
    string ResolveTenantSlug(HttpContext httpContext);
}

