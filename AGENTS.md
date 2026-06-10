# AGENTS.md

## Overview

Monorepo with 3 projects:
- **API_BigFOOD** — .NET 8 ASP.NET Core Web API (SQL Server + EF Core)
- **AplicacionMovil** — .NET 10 MAUI app (Android/iOS/Windows/macCatalyst)
- **Base de Datos/** — SQL scripts

## Commands

```pwsh
# API
dotnet run --project API_BigFOOD\API_BigFOOD            # dev server, Swagger at /swagger
dotnet build API_BigFOOD\API_BigFOOD

# MAUI app
dotnet build AplicacionMovil\AplicacionMovil -f net10.0-android
dotnet build AplicacionMovil\AplicacionMovil -f net10.0-windows10.0.19041.0
```

No test, lint, or typecheck projects exist.

## Architecture

### API (port 5173, Swagger at `/swagger`)

- **Auth** — `POST /Auth/AuthenticationUsers` (LoginDTO), returns JWT (5min expiry). JWT key: `appsettings.json:JwtSettings:Key`. Endpoints with `[Authorize]` require `Authorization: Bearer <token>`.
- **Clientes** — CRUD at `/Clientes/List|Search|Save|Update|Delete`. Save/Update/Delete require auth. `[Authorize]` missing on Search.
- **Productos** — CRUD at `/Productos/List|Search|Save|Update|Delete`. Save/Update/Delete require auth.
- **Facturas** — `POST /Facturas/CrearFactura` (FacturaDTO), `DELETE /Facturas/AnularFactura`, `PUT /Facturas/PagarCuenta`. Create auto-generates PDF (QuestPDF), emails it via SMTP, and logs to Bitacora. Credit invoices create CuentasPorCobrar.
- **Gometa** — External Costa Rica API (`apis.gometa.org`) for ID lookup and exchange rate. No auth key needed.
- **DbContextBigFOOD** — EF Core with SQL Server (`appsettings.json:ConnectionStrings:StringLocal`). Composite PK on `Det_Factura(NumFactura, CodInterno)` configured in `OnModelCreating`.
- **Services** registered as Scoped (IAuthorizationServices) or Transient (Gometa, PDF, Email, Bitacora).
- **WeatherForecast.cs** is template boilerplate — unused.

### MAUI App

- Entry: `MauiProgram.cs` → `App.xaml.cs` → `AppShell.xaml` → `LoginPage.xaml`
- **Login** uses hardcoded mock credentials (`admin` / `admin123`) with Preferences-based "remember me".
- **Design system**: Material 3-like, primary `#E64A19`, fonts Inter (body), Manrope (headings), JetBrainsMono (mono). XAML source gen enabled (`MauiXamlInflator=SourceGen`).
- `MockAuthService.cs` exists but is unused — login logic is inline in `LoginPage.xaml.cs`.

## Database

`Base de Datos/Script base de dato BigFOOD.sql` contains full schema. Live DB is on somee.com. Connection string is in `appsettings.json` (committed with credentials).

## Key conventions

- Controllers return plain strings on success/error (not `IActionResult` or typed responses) for Save/Update/Delete.
- All monetary values in `decimal`. No currency formatting abstractions.
- No DTOs for request bodies except FacturaDTO — models double as request DTOs.
- Bitacora logging on every CUD operation.
- `.slnx` format (new Visual Studio solution format).
- No tests, no CI config, no linters, no formatters.
