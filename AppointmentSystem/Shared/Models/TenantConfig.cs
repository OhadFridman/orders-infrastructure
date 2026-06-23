using System.Collections.Generic;

namespace AppointmentSystem.Shared.Models;

public class TenantConfig
{
    // Default services to seed when creating a tenant
    public List<ServiceDefinition> DefaultServices { get; set; } = new();

    // Default providers to seed when creating a tenant
    public List<Provider> DefaultProviders { get; set; } = new();

    // Default slot granularity in minutes
    public int SlotMinutes { get; set; } = 15;
}

