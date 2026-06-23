using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Server.Data;
using AppointmentSystem.Shared.Models;

namespace AppointmentSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly JsonRepository _repo;
    private readonly AppointmentSystem.Server.Tenant.ITenantResolver _tenantResolver;
    public CustomersController(JsonRepository repo, AppointmentSystem.Server.Tenant.ITenantResolver tenantResolver) => (_repo, _tenantResolver) = (repo, tenantResolver);

    [HttpGet]
    public Task<List<Customer>> Get()
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        return _repo.GetCustomersAsync(tenant);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Customer c)
    {
        var tenant = _tenantResolver.ResolveTenantSlug(HttpContext);
        var items = await _repo.GetCustomersAsync(tenant);
        items.Add(c);
        await _repo.SaveCustomersAsync(tenant, items);
        return CreatedAtAction(nameof(Get), new { id = c.Id }, c);
    }
}

