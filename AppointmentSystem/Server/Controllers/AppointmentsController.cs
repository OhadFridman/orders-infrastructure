using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Server.Data;
using AppointmentSystem.Shared.Models;
using System.Collections.Generic;

namespace AppointmentSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly JsonRepository _repo;
    private readonly AppointmentSystem.Server.Tenant.ITenantResolver _tenantResolver;

    public AppointmentsController(JsonRepository repo, AppointmentSystem.Server.Tenant.ITenantResolver tenantResolver)
    {
        _repo = repo;
        _tenantResolver = tenantResolver;
    }

    [HttpGet]
    public async Task<IEnumerable<Appointment>> GetAll()
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        return await _repo.GetAppointmentsAsync(tenant);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> Get(Guid id)
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var items = await _repo.GetAppointmentsAsync(tenant);
        var ap = items.FirstOrDefault(a => a.Id == id);
        if (ap == null) return NotFound();
        return ap;
    }

    [HttpPost]
    public async Task<ActionResult<Appointment>> Create([FromBody] Appointment input)
    {
        // Basic validation
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var services = await _repo.GetServicesAsync(tenant);
        var providers = await _repo.GetProvidersAsync(tenant);

        var svc = services.FirstOrDefault(s => s.Id == input.ServiceId);
        if (svc == null) return BadRequest("Service not found");

        var prov = providers.FirstOrDefault(p => p.Id == input.ProviderId);
        if (prov == null) return BadRequest("Provider not found");

        // Compute end time if not supplied
        if (input.EndUtc == default)
        {
            input.EndUtc = input.StartUtc.AddMinutes(svc.DurationMinutes);
        }

        // Quick overlap check: provider must not have overlapping scheduled appointments
        var appointments = await _repo.GetAppointmentsAsync(tenant);
        var overlapping = appointments.Any(a => a.ProviderId == input.ProviderId
            && a.Status != AppointmentStatus.Cancelled
            && a.StartUtc < input.EndUtc
            && a.EndUtc > input.StartUtc);
        if (overlapping) return Conflict("Time slot already booked");

        appointments.Add(input);
        await _repo.SaveAppointmentsAsync(tenant, appointments);

        return CreatedAtAction(nameof(Get), new { id = input.Id }, input);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] Appointment update)
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var appointments = await _repo.GetAppointmentsAsync(tenant);
        var idx = appointments.FindIndex(a => a.Id == id);
        if (idx == -1) return NotFound();

        // For simplicity replace updatable fields
        appointments[idx].StartUtc = update.StartUtc;
        appointments[idx].EndUtc = update.EndUtc;
        appointments[idx].Status = update.Status;
        appointments[idx].Notes = update.Notes;
        appointments[idx].MetaJson = update.MetaJson;

        await _repo.SaveAppointmentsAsync(tenant, appointments);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var appointments = await _repo.GetAppointmentsAsync(tenant);
        var removed = appointments.RemoveAll(a => a.Id == id);
        if (removed == 0) return NotFound();
        await _repo.SaveAppointmentsAsync(tenant, appointments);
        return NoContent();
    }
}


