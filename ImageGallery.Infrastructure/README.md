# ImageGallery.Infrastructure

Shared infrastructure components for the ImageGallery platform.

## Responsibilities

- ASP.NET Core Data Protection key persistence
- Shared security infrastructure
- Future signing key persistence
- Infrastructure-specific EF Core migrations

## EF Core Migration Commands

Add migration:

```powershell
dotnet ef migrations add InitialDataProtectionKeys --project .\ImageGallery.Infrastructure\ --startup-project .\ImageGallery.Client\
```

Apply migration:

```powershell
dotnet ef database update --project .\ImageGallery.Infrastructure\ --startup-project .\ImageGallery.Client\
```