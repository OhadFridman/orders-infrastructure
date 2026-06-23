using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Server.Data;
using AppointmentSystem.Shared.Models;

namespace AppointmentSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly JsonRepository _repo;
    private readonly AppointmentSystem.Server.Tenant.ITenantResolver _tenantResolver;
    public ServicesController(JsonRepository repo, AppointmentSystem.Server.Tenant.ITenantResolver tenantResolver) => (_repo, _tenantResolver) = (repo, tenantResolver);

    [HttpGet]
    public Task<List<ServiceDefinition>> Get()
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        return _repo.GetServicesAsync(tenant);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ServiceDefinition s)
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var items = await _repo.GetServicesAsync(tenant);
        items.Add(s);
        await _repo.SaveServicesAsync(tenant, items);
        return CreatedAtAction(nameof(Get), new { id = s.Id }, s);
    }
}

