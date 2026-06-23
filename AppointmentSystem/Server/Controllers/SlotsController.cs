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
public class SlotsController : ControllerBase
{
    private readonly JsonRepository _repo;
    private readonly AppointmentSystem.Server.Tenant.ITenantResolver _tenantResolver;

    public SlotsController(JsonRepository repo, AppointmentSystem.Server.Tenant.ITenantResolver tenantResolver) => (_repo, _tenantResolver) = (repo, tenantResolver);

    // GET api/slots?providerId={}
    [HttpGet]
    public async Task<ActionResult<List<ProviderManualSlot>>> Get([FromQuery] Guid providerId)
    {
        if (providerId == Guid.Empty) return BadRequest("providerId is required");
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var slots = await _repo.GetSlotsAsync(tenant);
        var result = slots.Where(s => s.ProviderId == providerId).ToList();
        return Ok(result);
    }

    // POST api/slots
    [HttpPost]
    public async Task<ActionResult<ProviderManualSlot>> Create([FromBody] ProviderManualSlot slot)
    {
        if (slot.ProviderId == Guid.Empty) return BadRequest("ProviderId required");
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var slots = await _repo.GetSlotsAsync(tenant);
        slots.Add(slot);
        await _repo.SaveSlotsAsync(tenant, slots);
        return CreatedAtAction(nameof(Get), new { providerId = slot.ProviderId }, slot);
    }

    // DELETE api/slots/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var slots = await _repo.GetSlotsAsync(tenant);
        var removed = slots.RemoveAll(s => s.Id == id);
        if (removed == 0) return NotFound();
        await _repo.SaveSlotsAsync(tenant, slots);
        return NoContent();
    }
}

