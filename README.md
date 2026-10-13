# Student Study Hub

A mobile-first web app for high school students to manage homework, study groups,
notes, exams, GPA, SAT prep, scholarships, and college applications — all in one place.

Full design package (PRD, architecture, ERD, API design, wireframes, roadmap, etc.)
is in [`DESIGN.md`](./DESIGN.md).

## Tech Stack

- **Frontend**: React + TypeScript + Tailwind CSS (Vite), mobile-first responsive SPA
- **Backend**: ASP.NET Core on **Azure Functions** (isolated worker, Clean Architecture)
- **Database**: Azure SQL Database (Free Offer tier)
- **Auth**: Custom JWT (access + refresh tokens)
- **Cloud**: Azure Static Web Apps, Azure Functions (Consumption), Azure Key Vault,
  Azure Blob Storage, Application Insights — targeting **$0/month** on Azure's
  Always-Free tiers (see `DESIGN.md` Section 13).

## Repository Structure

```
student-study-hub/
├── backend/                  # .NET 8 solution (Clean Architecture)
│   ├── src/
│   │   ├── StudentStudyHub.Domain/          # Entities, enums, domain exceptions
│   │   ├── StudentStudyHub.Application/     # Use cases, GPA calculator, interfaces
│   │   ├── StudentStudyHub.Infrastructure/  # EF Core DbContext, configurations
│   │   └── StudentStudyHub.Functions/       # Azure Functions (HTTP + Timer triggers)
│   ├── tests/
│   │   ├── StudentStudyHub.Domain.UnitTests/
│   │   ├── StudentStudyHub.Application.UnitTests/
│   │   └── StudentStudyHub.Functions.IntegrationTests/
│   ├── NuGet.Config          # Pins package source to nuget.org
│   └── StudentStudyHub.sln
├── frontend/                 # React + TypeScript + Tailwind (Vite)
│   └── src/
│       ├── app/              # App shell, router, providers
│       ├── pages/             # Route-level pages (dashboard, assignments, etc.)
│       ├── features/          # Feature-sliced API hooks/types per domain
│       ├── components/        # ui/, layout/, shared/ design-system primitives
│       └── lib/                # apiClient, queryClient, utils
└── DESIGN.md                  # Full architecture & product design document
```

## Getting Started

### Backend

```powershell
cd backend
dotnet build
dotnet test
```

To run the Functions API locally you'll need the [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
and a `local.settings.json` (gitignored) with your SQL connection string:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ConnectionStrings:SqlConnectionString": "<your Azure SQL or LocalDB connection string>",
    "Jwt__Secret": "<a long random local-dev-only secret, 32+ chars>",
    "Jwt__Issuer": "StudentStudyHub",
    "Jwt__Audience": "StudentStudyHub.Client",
    "Jwt__AccessTokenMinutes": "15",
    "Jwt__RefreshTokenDays": "30"
  }
}
```

### Frontend

```powershell
cd frontend
npm install
npm run dev
```

Builds with `npm run build`; type-checks with `npx tsc --noEmit`.

## Status

🚧 Early scaffold — see `DESIGN.md` for the full roadmap (Section 16) and
MVP vs. future feature breakdown (Section 17). The GPA calculator (`GpaCalculator.cs`)
is the first fully implemented and unit-tested feature; everything else follows the
same Clean Architecture pattern.
