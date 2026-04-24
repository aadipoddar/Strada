# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

Strada is fleet management software for Ashok Roadlines. It is a multi-platform .NET 10 application with a Blazor Server web app and a MAUI Hybrid desktop/mobile app sharing a single Blazor UI library.

## Build & Run

```bash
# Build entire solution
dotnet build Strada.slnx

# Run web app (https://localhost:7261)
dotnet run --project Strada/Strada.Web/Strada.Web.csproj

# Run MAUI app on Windows
dotnet run --project Strada/Strada/Strada.csproj -f net10.0-windows10.0.19041.0

# If Razor compilation cache causes strange errors
dotnet clean && dotnet build Strada.slnx
```

There are no automated tests in this codebase.

The database is a SQL Server SSDT project (`DBStrada/`). Deploy it via Visual Studio: right-click DBStrada → Publish, selecting the appropriate profile from `DBStrada/PublishLocations/`.

## Project Map

| Project | Role |
|---|---|
| `StradaLibrary` | All data access, models, exports, and business logic |
| `Strada.Shared` | All Blazor pages, components, and shared services (interfaces) |
| `Strada.Web` | Blazor Server host — thin wrapper, just DI registration and startup |
| `Strada` | MAUI Hybrid host — thin wrapper, just DI registration and startup |
| `DBStrada` | SSDT SQL Server project — stored procedures, tables, views |
| `ExcelImport` | One-off console utility for importing Excel data |

Business logic and new data access always goes in `StradaLibrary`. New UI components and pages always go in `Strada.Shared`. The host projects (`Strada.Web`, `Strada`) contain only `Program.cs`/`MauiProgram.cs`.

## Data Access Pattern

All database calls go through stored procedures via Dapper. The entry point for most reads is:

```csharp
// Generic loaders in StradaLibrary/Data/Common/CommonData.cs
await CommonData.LoadTableData<T>(TableName)          // all rows
await CommonData.LoadTableDataById<T>(TableName, id)  // by ID
await CommonData.LoadTableDataByStatus<T>(TableName)  // active only
await CommonData.LoadTableDataByDate<T>(TableName, start, end)
```

`TableName` is a constant from the name classes in `StradaLibrary/Data/Operations/`:
- `FleetNames.Vehicle`, `FleetNames.VehicleTrip`, etc.
- `AccountNames.Company`, `AccountNames.Ledger`, etc.
- `OperationNames.User`, `OperationNames.Settings`, etc.

For writes and multi-step transactions, each domain has a `*Data.cs` static class (e.g., `VehicleData`, `FinancialAccountingData`) that wraps `SqlDataAccess` directly and handles `SqlDataAccessTransaction` commit/rollback.

Stored procedure naming convention: `Load_TableName`, `Load_TableName_By_Id`, `Insert_TableName`.

## Page Architecture

Every page is a partial class split across two files:

- `PageName.razor` — markup only
- `PageName.razor.cs` — all logic, typed as `public partial class PageName`

**Lifecycle**: Always use `OnAfterRenderAsync(firstRender)`, never `OnInitializedAsync`, because services like `IDataStorageService` require JS interop which isn't available before the first render.

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (!firstRender) return;
    _user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Fleet]);
    await LoadData();
}
```

Every page calls `AuthenticationService.ValidateUser(...)` as its first action. The `userRoles` list controls which roles are permitted; omitting it allows any authenticated user.

**Route constants** are in `StradaLibrary/Data/Operations/PageRouteNames.cs`. The `@attribute [Route(PageRouteNames.X)]` directive in `.razor` files uses these.

## Services & Injection

All services are injected globally via `_Imports.razor` — do not add `[Inject]` attributes or `@inject` in individual pages. The following are available everywhere:

```
NavigationManager, IJSRuntime, IFormFactor, ISaveAndViewService,
IDataStorageService, IVibrationService, ISoundService,
IUpdateService, INotificationService
```

`AuthenticationService` is a static class in `Strada.Shared/Services/` and is called directly, not injected.

Platform-specific implementations of the service interfaces live in the host projects (`Strada/` for MAUI, `Strada.Web/` for web) and are registered in `MauiProgram.cs` / `Program.cs`.

## UI Components

**Always use Syncfusion Blazor components** — never native HTML `<input>`, `<select>`, `<button>`, etc. for form elements, grids, or date pickers. All Syncfusion namespaces are already imported globally.

Reusable components live in `Strada.Shared/Components/`:
- `Components/Input/MasterAutoComplete` — generic autocomplete for master selection with optional "Add New" navigation
- `Components/Dialog/` — delete/recover/reset confirmation dialogs, toast notification, document upload
- `Components/Page/` — `Header`, `Footer`, `AnimatedLoader`
- `Components/Button/` — `IconButton`, `FolderCard`, `FileCard`

Before creating a new component, check if one already exists here.

**CSS**: `Strada.Shared/wwwroot/app.css` defines all global CSS variables (`--primary-blue`, `--gray-*`, etc.) and utility classes (`form-field`, `field-label`, `required`, `custom-autocomplete`, `custom-textbox`, `section-card`, `form-grid`, etc.). Check this file before adding any new styles. Page-scoped styles go in a `.razor.css` sibling file.

## Secrets & Configuration

Secrets are managed via .NET User Secrets on the `StradaLibrary` project. Required secrets:

```bash
dotnet user-secrets set "AzureConnectionString" "..." -p StradaLibrary/StradaLibrary.csproj
dotnet user-secrets set "AzureBlobStorageConnectionString" "..." -p StradaLibrary/StradaLibrary.csproj
dotnet user-secrets set "SyncfusionLicense" "..." -p StradaLibrary/StradaLibrary.csproj
dotnet user-secrets set "EmailPassword" "..." -p StradaLibrary/StradaLibrary.csproj
```

All secrets are accessed through `StradaLibrary/DataAccess/Secrets.cs`. `Secrets.SetupConfiguration()` must be called at startup (already wired in both host projects) — it registers Syncfusion license, Dapper type handlers for `DateOnly`/`TimeOnly`, and sets a 10-minute SQL command timeout.

## C# Conventions

- **Nullable is disabled** (`<Nullable>disable</Nullable>`). Use `is null` / `is not null`, never `== null`.
- Use C# 14 features: collection expressions `[.. items]`, file-scoped namespaces, primary constructors, pattern matching.
- Private fields: `_camelCase`. Public members: `PascalCase`.
- Local draft/session data is stored via `IDataStorageService` using constants from `StorageFileNames`.
- `decimal.FormatIndianCurrency()` and `decimal.FormatSmartDecimal()` are extension methods in `Helper.cs` for display formatting.
