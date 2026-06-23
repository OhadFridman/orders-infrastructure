using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Server.Data;
using AppointmentSystem.Shared.Models;

namespace AppointmentSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvailabilityController : ControllerBase
{
    private readonly JsonRepository _repo;
    private readonly AppointmentSystem.Server.Tenant.ITenantResolver _tenantResolver;
    public AvailabilityController(JsonRepository repo, AppointmentSystem.Server.Tenant.ITenantResolver tenantResolver) => (_repo, _tenantResolver) = (repo, tenantResolver);

    // GET api/availability?providerId={}&serviceId={}&date=YYYY-MM-DD&slotMinutes=15
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid providerId, [FromQuery] Guid serviceId, [FromQuery] string date, [FromQuery] int slotMinutes = 15)
    {
        if (providerId == Guid.Empty || serviceId == Guid.Empty) return BadRequest("providerId and serviceId are required");
        if (!DateOnly.TryParse(date, out var day)) return BadRequest("date must be YYYY-MM-DD");

        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var services = await _repo.GetServicesAsync(tenant);
        var providers = await _repo.GetProvidersAsync(tenant);
        var appointments = await _repo.GetAppointmentsAsync(tenant);
        var manualSlots = await _repo.GetSlotsAsync(tenant);

        var svc = services.FirstOrDefault(s => s.Id == serviceId);
        if (svc == null) return BadRequest("Service not found");

        var prov = providers.FirstOrDefault(p => p.Id == providerId);
        if (prov == null) return BadRequest("Provider not found");

        // If provider day off, none
        if (prov.DaysOff?.Contains(day) == true) return Ok(new List<object>());

        // determine provider timezone
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(prov.TimeZoneId ?? TimeZoneInfo.Utc.Id);
        }
        catch
        {
            tz = TimeZoneInfo.Utc;
        }

        var workStart = prov.WorkDayStart ?? TimeSpan.FromHours(9);
        var workEnd = prov.WorkDayEnd ?? TimeSpan.FromHours(17);
        if (workEnd <= workStart) return Ok(new List<object>());

        var duration = TimeSpan.FromMinutes(svc.DurationMinutes);

        var slots = new List<object>();

        // candidate local DateTime for start
        var cursorLocal = new DateTime(day.Year, day.Month, day.Day, workStart.Hours, workStart.Minutes, workStart.Seconds, DateTimeKind.Unspecified);
        var workEndLocal = new DateTime(day.Year, day.Month, day.Day, workEnd.Hours, workEnd.Minutes, workEnd.Seconds, DateTimeKind.Unspecified);

        while (cursorLocal + duration <= workEndLocal)
        {
            // convert candidate local to UTC using provider timezone
            var candidateStartUtc = TimeZoneInfo.ConvertTimeToUtc(cursorLocal, tz);
            var candidateEndUtc = candidateStartUtc + duration;

            // candidate blocked interval including buffers from this service
            var candBlockedStart = candidateStartUtc.AddMinutes(-svc.BufferBeforeMinutes);
            var candBlockedEnd = candidateEndUtc.AddMinutes(svc.BufferAfterMinutes);

            // build list of existing blocked intervals for provider
            var providerAppointments = appointments.Where(a => a.ProviderId == providerId && a.Status != AppointmentStatus.Cancelled).ToList();
            bool overlaps = false;
            foreach (var a in providerAppointments)
            {
                var otherSvc = services.FirstOrDefault(s => s.Id == a.ServiceId);
                var otherBefore = otherSvc?.BufferBeforeMinutes ?? 0;
                var otherAfter = otherSvc?.BufferAfterMinutes ?? 0;
                var otherBlockedStart = a.StartUtc.AddMinutes(-otherBefore);
                var otherBlockedEnd = a.EndUtc.AddMinutes(otherAfter);

                if (otherBlockedStart < candBlockedEnd && otherBlockedEnd > candBlockedStart)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                // include both UTC and local representation
                var candidateLocalDisplay = TimeZoneInfo.ConvertTimeFromUtc(candidateStartUtc, tz);
                slots.Add(new { StartUtc = candidateStartUtc, StartLocal = candidateLocalDisplay.ToString("s") });
            }

            // include any manual slots for this provider that overlap the candidate blocked interval
            var matchingManual = manualSlots.Where(s => s.ProviderId == providerId && s.StartUtc < candBlockedEnd && s.EndUtc > candBlockedStart).ToList();
            foreach (var m in matchingManual)
            {
                var local = TimeZoneInfo.ConvertTimeFromUtc(m.StartUtc, tz);
                slots.Add(new { StartUtc = m.StartUtc, StartLocal = local.ToString("s") });
            }

            // advance by slotMinutes
            cursorLocal = cursorLocal.AddMinutes(slotMinutes);
        }

        return Ok(slots);
    }
}

