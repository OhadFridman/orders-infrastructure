using System;

namespace AppointmentSystem.Shared.Models;

public class ProviderManualSlot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    // which provider this slot belongs to
    public Guid ProviderId { get; set; }
    // Stored in UTC
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string? Notes { get; set; }
}

