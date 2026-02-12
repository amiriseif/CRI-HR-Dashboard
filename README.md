# HRDashboard

## Project Overview
**HRDashboard** is an ASP.NET Core 8 MVC application designed to manage an organization’s HR tasks and events (members, tasks, workshops). It provides a dashboard with statistics, integrates with external automation services for query generation and email drafting, and supports secure server-side processing of webhooks.

---

## Tech Stack
- **Framework:** ASP.NET Core 8 (MVC / Razor Views)  
- **Language:** C#  
- **ORM / Database:** Entity Framework Core with SQL Server (`DefaultConnection`)  
- **Authentication:** ASP.NET Core Identity with custom `ApplicationUser`  
- **Frontend:** Bootstrap, jQuery (from `wwwroot/lib`)  
- **External Integrations:**  
  - n8n webhooks (query generation + mail generation)  
  - Google Sheets (read-only client)  
  - Cloudinary (image upload)  
- **Build & Run:**  
```bash
dotnet build
dotnet run --project HRDashboard
