using Microsoft.AspNetCore.Http;
using System;

namespace AppointmentSystem.Server.Tenant;

// Resolves tenant slug from the request host (subdomain). E.g. avishai.example.com -> avishai
public class HostTenantResolver : ITenantResolver
{
    public string ResolveTenantSlug(HttpContext httpContext)
    {
        var host = httpContext.Request.Host.Host; // may be "avishai.example.com" or "localhost"
        if (string.IsNullOrWhiteSpace(host)) return "default";

        // If host is localhost (or contains localhost), attempt to parse the left-most label if formatted like "avishai.localhost" or "avishai.localhost:5001"
        // Split on ':' first to remove port
        var hostOnly = host.Split(':')[0];
        var parts = hostOnly.Split('.');
        if (parts.Length >= 3)
        {
            // subdomain.domain.tld -> take first label
            var sub = parts[0];
            if (!string.IsNullOrWhiteSpace(sub) && sub != "www") return sub;
        }

        // If host looks like "avishai.localhost" (two parts) treat first as slug
        if (parts.Length == 2 && parts[1].Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            var sub = parts[0];
            if (!string.IsNullOrWhiteSpace(sub) && sub != "www") return sub;
        }

        // No subdomain detected -> fallback marker meaning no tenant in host
        return string.Empty;
    }
}

