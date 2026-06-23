using System.Net.Http;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace AppointmentSystem.Client.Services;

public class TenantService
{
    private const string StorageKey = "tenantSlug";
    private readonly IJSRuntime _js;
    public string? TenantSlug { get; private set; }

    public TenantService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync(HttpClient http)
    {
        try
        {
            var s = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(s))
            {
                TenantSlug = s;
                ApplyToHttp(http);
            }
        }
        catch
        {
        }
    }

    public async Task SetTenantAsync(string slug, HttpClient http)
    {
        TenantSlug = slug;
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, slug);
        }
        catch { }
        ApplyToHttp(http);
    }

    private void ApplyToHttp(HttpClient http)
    {
        if (http == null) return;
        http.DefaultRequestHeaders.Remove("X-Tenant-Slug");
        if (!string.IsNullOrWhiteSpace(TenantSlug))
            http.DefaultRequestHeaders.Add("X-Tenant-Slug", TenantSlug);
    }
}

