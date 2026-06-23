using System;
using System.Collections.Generic;

namespace AppointmentSystem.Shared.Models;

public class Provider
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    // IANA time zone id like "Europe/London" or Windows id depending on usage.
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;
    // List of service ids this provider can perform
    public List<Guid> ServiceIds { get; set; } = new();
    // Simple working hours: assume same hours every weekday for now
    public TimeSpan? WorkDayStart { get; set; }
    public TimeSpan? WorkDayEnd { get; set; }
    // Days off as a set of dates
    public List<DateOnly> DaysOff { get; set; } = new();
    // Optional physical address for the provider/location
    public string? Address { get; set; }

    // Manual ad-hoc slots (stored in UTC) admins can create per provider
    public List<Guid> ManualSlotIds { get; set; } = new();
}

