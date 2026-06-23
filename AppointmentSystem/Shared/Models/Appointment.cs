using System;

namespace AppointmentSystem.Shared.Models;

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    Cancelled,
    Completed
}

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid CustomerId { get; set; }
    // Stored in UTC
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }
    public string? MetaJson { get; set; }
}

