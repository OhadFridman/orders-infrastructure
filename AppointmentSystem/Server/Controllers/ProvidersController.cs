using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Server.Data;
using AppointmentSystem.Shared.Models;

namespace AppointmentSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProvidersController : ControllerBase
{
    private readonly JsonRepository _repo;
    private readonly AppointmentSystem.Server.Tenant.ITenantResolver _tenantResolver;
    public ProvidersController(JsonRepository repo, AppointmentSystem.Server.Tenant.ITenantResolver tenantResolver) => (_repo, _tenantResolver) = (repo, tenantResolver);

    [HttpGet]
    public Task<List<Provider>> Get()
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        return _repo.GetProvidersAsync(tenant);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Provider p)
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var items = await _repo.GetProvidersAsync(tenant);
        items.Add(p);
        await _repo.SaveProvidersAsync(tenant, items);
        return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
    }
}

