using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AppointmentSystem.Shared.Models;

namespace AppointmentSystem.Server.Data;

/// <summary>
/// Very small JSON-file backed repository. Not intended for production.
/// Loads collections from files under the "Data" folder next to the running assembly.
/// </summary>
public class JsonRepository
{
    private readonly string _dataDir;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonRepository()
    {
        // Use a folder inside application base so it works when running from IDE or publish
        _dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        if (!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
    }

    private string TenantDataDir(string tenantSlug)
    {
        var s = string.IsNullOrWhiteSpace(tenantSlug) ? "default" : tenantSlug;
        var dir = Path.Combine(_dataDir, s);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return dir;
    }

    private string PathForTenant(string tenantSlug, string name) => Path.Combine(TenantDataDir(tenantSlug), name + ".json");

    private string PathForGlobal(string name) => Path.Combine(_dataDir, name + ".json");

    private async Task<List<T>> LoadListFromPathAsync<T>(string path)
    {
        await _mutex.WaitAsync();
        try
        {
            if (!File.Exists(path))
            {
                return new List<T>();
            }

            using var stream = File.OpenRead(path);
            var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions);
            return items ?? new List<T>();
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task<T?> LoadObjectFromPathAsync<T>(string path)
    {
        await _mutex.WaitAsync();
        try
        {
            if (!File.Exists(path)) return default;
            using var stream = File.OpenRead(path);
            var obj = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
            return obj;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task SaveObjectToPathAsync<T>(string path, T obj)
    {
        await _mutex.WaitAsync();
        try
        {
            using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, obj, _jsonOptions);
            await stream.FlushAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    // Tenant config helpers
    public Task<AppointmentSystem.Shared.Models.TenantConfig?> GetTenantConfigAsync(string tenantSlug)
        => LoadObjectFromPathAsync<AppointmentSystem.Shared.Models.TenantConfig>(PathForTenant(tenantSlug, "config"));

    public Task SaveTenantConfigAsync(string tenantSlug, AppointmentSystem.Shared.Models.TenantConfig config)
        => SaveObjectToPathAsync(PathForTenant(tenantSlug, "config"), config);

    private async Task SaveListToPathAsync<T>(string path, List<T> items)
    {
        await _mutex.WaitAsync();
        try
        {
            using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, items, _jsonOptions);
            await stream.FlushAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    // Services
    public Task<List<ServiceDefinition>> GetServicesAsync(string tenantSlug) => LoadListFromPathAsync<ServiceDefinition>(PathForTenant(tenantSlug, "services"));
    public Task SaveServicesAsync(string tenantSlug, List<ServiceDefinition> items) => SaveListToPathAsync(PathForTenant(tenantSlug, "services"), items);

    // Providers
    public Task<List<Provider>> GetProvidersAsync(string tenantSlug) => LoadListFromPathAsync<Provider>(PathForTenant(tenantSlug, "providers"));
    public Task SaveProvidersAsync(string tenantSlug, List<Provider> items) => SaveListToPathAsync(PathForTenant(tenantSlug, "providers"), items);

    // Customers
    public Task<List<Customer>> GetCustomersAsync(string tenantSlug) => LoadListFromPathAsync<Customer>(PathForTenant(tenantSlug, "customers"));
    public Task SaveCustomersAsync(string tenantSlug, List<Customer> items) => SaveListToPathAsync(PathForTenant(tenantSlug, "customers"), items);

    // Appointments
    public Task<List<Appointment>> GetAppointmentsAsync(string tenantSlug) => LoadListFromPathAsync<Appointment>(PathForTenant(tenantSlug, "appointments"));
    public Task SaveAppointmentsAsync(string tenantSlug, List<Appointment> items) => SaveListToPathAsync(PathForTenant(tenantSlug, "appointments"), items);

    // Manual slots per tenant
    public Task<List<AppointmentSystem.Shared.Models.ProviderManualSlot>> GetSlotsAsync(string tenantSlug) => LoadListFromPathAsync<AppointmentSystem.Shared.Models.ProviderManualSlot>(PathForTenant(tenantSlug, "slots"));
    public Task SaveSlotsAsync(string tenantSlug, List<AppointmentSystem.Shared.Models.ProviderManualSlot> items) => SaveListToPathAsync(PathForTenant(tenantSlug, "slots"), items);

    // Organizations (global)
    public Task<List<AppointmentSystem.Shared.Models.Organization>> GetOrganizationsAsync() => LoadListFromPathAsync<AppointmentSystem.Shared.Models.Organization>(PathForGlobal("organizations"));
    public Task SaveOrganizationsAsync(List<AppointmentSystem.Shared.Models.Organization> items) => SaveListToPathAsync(PathForGlobal("organizations"), items);
}

