using System;
using System.Collections.Generic;

namespace AppointmentSystem.Shared.Models;

public class ServiceDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int BufferBeforeMinutes { get; set; }
    public int BufferAfterMinutes { get; set; }
    public decimal? Price { get; set; }
    public string? CustomFieldsJson { get; set; }
    public List<Guid> CategoryIds { get; set; } = new();
}

