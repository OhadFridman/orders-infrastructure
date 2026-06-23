using Microsoft.AspNetCore.ResponseCompression;
using AppointmentSystem.Server.Data;
using AppointmentSystem.Server.Tenant;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// Simple JSON-backed repository for development
builder.Services.AddSingleton<JsonRepository>();
// Tenant resolver: prefer host/subdomain, fall back to header
builder.Services.AddSingleton<ITenantResolver, CompositeTenantResolver>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
