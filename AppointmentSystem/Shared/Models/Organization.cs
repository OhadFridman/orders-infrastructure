using System;

namespace AppointmentSystem.Shared.Models;

public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Slug { get; set; } = string.Empty; // e.g. "avishai-hair"
    public string Name { get; set; } = string.Empty;
    public string? OwnerEmail { get; set; }
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;
    public string? MetaJson { get; set; }
}

