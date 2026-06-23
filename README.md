# Appointment System (Blazor + ASP.NET Core)

This repository contains a small, multi-tenant appointment system implemented with:
- ASP.NET Core (Server project)
- Blazor (Client project)
- Shared project with domain models

Data persistence is intentionally simple for now: each tenant's data is stored as JSON files under `AppointmentSystem/Server/Data/{tenant}/`.

Quick start (development)

Prerequisites:
- .NET 7 SDK
- Git

Build and run from the solution root:

```bash
cd /Users/ohad.fridman/projects/orders-infrastructure/AppointmentSystem
dotnet build AppointmentSystem.sln
dotnet run --project Server/AppointmentSystem.Server.csproj
```

The server hosts APIs under `/api` and serves the Blazor client.

Tenants
- Tenant data lives in `AppointmentSystem/Server/Data/{tenantSlug}/`.
- Tenant resolution supports subdomain-based routing (Host) and header-based selection (`X-Tenant-Slug`) for local development.
- To create a tenant, use the Organizations API (POST /api/organizations) or the Admin UI in the Blazor client.

Admin UI
- Open the Blazor client in a browser (hosted by the server). Use the "Tenants" or "Admin" pages to create services, providers, customers, and manual slots.

Notes & next steps
- This is a prototype: build artifacts and IDE files were included in an initial commit. Add a `.gitignore` to stop tracking `bin/`, `obj/`, and IDE metadata.
- For production, replace the file-backed repository with a real database, add authentication/authorization, and secure tenant access.

Contact / author
- Ohad Fridman

License
- See project root or choose an appropriate license for your use.
