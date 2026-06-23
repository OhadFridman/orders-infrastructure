using System;

namespace AppointmentSystem.Shared.Models;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    // Free-form metadata stored as JSON string (for custom fields)
    public string? MetadataJson { get; set; }
}

