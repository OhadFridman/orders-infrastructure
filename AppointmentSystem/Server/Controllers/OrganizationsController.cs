using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Server.Data;
using AppointmentSystem.Shared.Models;
using System.Linq;
using System.Collections.Generic;

namespace AppointmentSystem.Server.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly JsonRepository _repo;
    public OrganizationsController(JsonRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<List<Organization>> Get()
    {
        return await _repo.GetOrganizationsAsync();
    }

    [HttpGet("{slug}/config")]
    public async Task<ActionResult<AppointmentSystem.Shared.Models.TenantConfig?>> GetConfig(string slug)
    {
        var cfg = await _repo.GetTenantConfigAsync(slug);
        if (cfg == null) return NotFound();
        return Ok(cfg);
    }

    [HttpPost]
    public async Task<ActionResult<Organization>> Create([FromBody] CreateOrganizationRequest req)
    {
        var org = req.Organization ?? new Organization();
        var config = req.Config;
        // normalize slug
        if (string.IsNullOrWhiteSpace(org.Slug)) org.Slug = org.Name?.ToLowerInvariant().Replace(' ', '-') ?? "org";
        var list = await _repo.GetOrganizationsAsync();
        if (list.Any(o => o.Slug == org.Slug)) return Conflict("Slug already exists");
        list.Add(org);
        await _repo.SaveOrganizationsAsync(list);

        // seed tenant directory
        var services = config?.DefaultServices ?? new List<ServiceDefinition>();
        var providers = config?.DefaultProviders ?? new List<AppointmentSystem.Shared.Models.Provider>();
        await _repo.SaveServicesAsync(org.Slug, services);
        await _repo.SaveProvidersAsync(org.Slug, providers);
        await _repo.SaveCustomersAsync(org.Slug, new List<AppointmentSystem.Shared.Models.Customer>());
        await _repo.SaveAppointmentsAsync(org.Slug, new List<AppointmentSystem.Shared.Models.Appointment>());
        if (config != null)
        {
            await _repo.SaveTenantConfigAsync(org.Slug, config);
        }

        return CreatedAtAction(nameof(Get), new { id = org.Id }, org);
    }
}

public class CreateOrganizationRequest
{
    public AppointmentSystem.Shared.Models.Organization? Organization { get; set; }
    public AppointmentSystem.Shared.Models.TenantConfig? Config { get; set; }
}




