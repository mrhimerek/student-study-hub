# Remaining Work — Student Study Hub

> **Purpose:** a resumable snapshot of what's done and what's left, so work can
> pick back up cleanly in a future session. See `DESIGN.md` for the full
> architecture/product spec this all traces back to.

Last updated: 2026-10-13. Current `main` HEAD: builds on `f81026d` with Course
CRUD + GPA frontend wiring added on top (not yet committed — see below).

## How to resume

```powershell
# Backend
cd backend
dotnet test                         # should show 29/29 passing

cd src/StudentStudyHub.Functions
func start                          # local API on http://localhost:7071

# Frontend (separate terminal)
cd frontend
npm run dev                         # http://localhost:5173, proxies /api -> :7071
```

You'll need a `backend/src/StudentStudyHub.Functions/local.settings.json`
(gitignored) — see the "Backend" section of `README.md` for the required
keys (SQL connection string, JWT secret, etc.). The LocalDB database
(`StudentStudyHub` on `(localdb)\mssqllocaldb`) already has the `InitialCreate`
migration applied.

## ✅ Done

| Area | Status | Notes |
|---|---|---|
| Repo scaffold | ✅ | .NET 8 Clean Architecture backend (Domain/Application/Infrastructure/Functions) + Vite/React/TS/Tailwind frontend |
| Auth (backend) | ✅ | Register/login/refresh/logout, JWT access + rotating refresh tokens (SHA-256 hashed at rest), BCrypt password hashing, 14 unit tests |
| Auth (frontend) | ✅ | Login/Register pages, `AuthContext`, protected routes, axios interceptor with silent refresh-on-401 |
| EF Core + LocalDB | ✅ | `InitialCreate` migration applied; all 14 tables present and verified via live smoke tests |
| JSON contract | ✅ | camelCase everywhere (`JsonSerializerDefaults.Web`); enums serialize as camelCase strings (e.g. `"notStarted"`), not raw ints |
| **Assignments / homework tracker** | ✅ | Full CRUD API + FluentValidation + 8 unit tests; full frontend (list, filter tabs, add/edit form, tap-to-cycle status, delete) — **first fully complete vertical slice** |
| **Course CRUD** | ✅ | Full CRUD API (`CourseService`/`CourseFunctions`) + FluentValidation + 7 unit tests; full frontend (`features/courses`, `pages/courses/CoursesPage` with add/edit/delete), wired into `router.tsx` at `/courses` (not in bottom nav — reached via the Dashboard GPA card's "Manage courses" link) |
| **GPA calculator** | ✅ | `GpaCalculator.cs` (pure, unit-tested) + `GET /api/gpa/summary` now return real numbers since Course CRUD exists; Dashboard's GPA card (`features/gpa`) pulls the real weighted GPA via `useGpaSummary()`, replacing the old mock `3.8`; course mutations invalidate the GPA query cache so the Dashboard stays in sync |
| Dashboard greeting | ✅ | Pulls real `firstName` from `useAuth()` (rest of the dashboard — due-today list, next exam — is still mock data) |

## ❌ Not started

These all have a Domain entity already scaffolded (so the data model exists),
but **no Application service, no Function endpoints, and no frontend** yet.
Build each the same way Assignments was built: DTOs → FluentValidation →
service → Functions endpoints → unit tests → smoke test → frontend
types/api/hooks → pages → wire into `router.tsx`.

| Feature | Domain entities already scaffolded | What's needed |
|---|---|---|
| **Study groups** | `StudyGroup`, `StudyGroupMember` | School-scoped create/join/leave, member list, owner-only edit/delete. Needs a `School` to belong to first — there's currently no School seed data or School CRUD/picker UI. |
| **Shared notes in study groups** | `GroupNote` | CRUD scoped to group membership (only members can read/write notes for their group). Depends on Study Groups existing first. |
| **Exam/quiz countdown dashboard** | `Exam` | CRUD + a dashboard widget showing soonest upcoming exam(s) (replace the `mockNextExam` in `DashboardPage.tsx`). |
| **SAT study planner** | `SatPlan`, `SatPracticeScore` | CRUD for a target test date/score goal + logging practice test scores over time (a small chart would be a nice touch, not required for MVP). |
| **Scholarship tracker** | `Scholarship` | CRUD with a status pipeline (Researching → Applying → Submitted → Awarded/Rejected), deadline countdown. |
| **College application tracker** | `CollegeApplication` | CRUD with a status pipeline (NotStarted → InProgress → Submitted → Accepted/Rejected/Waitlisted), deadline countdown, application type (ED/EA/RD/Rolling). |
| **Calendar view** | *(aggregates the above)* | A month/week view that pulls due dates from Assignments + Exams + Scholarship/College deadlines into one place. Build last, once those features exist. |
| **Notifications (in-app + email)** | `Notification` | `DeadlineReminderFunction` (hourly timer trigger) is a **stub only** — it has a `TODO` comment and does nothing yet. Needs: (1) query logic to find assignments/exams/scholarships/applications due in 1 or 3 days, (2) dedupe against existing `Notification` rows so the same deadline doesn't re-notify every hour, (3) an actual `IEmailService` implementation (interface exists, no implementation — e.g. Azure Communication Services or SendGrid), (4) a frontend notifications bell/list UI. |
| **Profile page** | `User` | Currently a `PlaceholderPage`. Needs a view/edit form for name, grade level, school; likely also where a future avatar upload (`IBlobStorageService` — interface exists, no implementation) would live. |

## 🏗️ Infra / ops not started

- **CI/CD**: no GitHub Actions workflow yet. Should run `dotnet test` (backend)
  and `npm run build` (frontend, which includes `tsc -b`) on every push/PR.
  See `DESIGN.md` Section 14 for the originally designed pipeline.
- **Actual Azure deployment**: everything so far has only been run and verified
  locally (LocalDB + `func start` + Vite dev server). Nothing has been deployed
  to Azure Static Web Apps / Azure Functions / Azure SQL yet. `DESIGN.md`
  Section 13 has the target $0/month architecture and resource list.
- **`IEmailService` implementation**: interface-only right now (used nowhere
  yet since Notifications aren't implemented).
- **`IBlobStorageService` implementation**: interface-only right now (nothing
  uploads files yet — would first be needed for a profile picture or shared
  note attachments).
- **`DeadlineReminderFunction`**: logged a known local-dev limitation —
  `func start` errors trying to reach the Azurite storage emulator
  (`127.0.0.1:10000`) for this timer trigger specifically, since
  `AzureWebJobsStorage=UseDevelopmentStorage=true` and no emulator is running
  locally. Doesn't block any HTTP endpoint, but means this timer can't be
  smoke-tested locally without installing/running Azurite (or pointing it at
  a real Azure Storage account).

## Suggested order for next session

1. ~~Course CRUD~~ ✅ done
2. ~~Wire GPA calculator into the frontend~~ ✅ done
3. **Exam countdown** (small-ish, replaces `mockNextExam` on the Dashboard, high visual payoff) — **start here next**
4. **Study groups** + **Shared notes** (bigger, but core to the product's name/positioning)
5. **Scholarship tracker** and **College application tracker** (similar CRUD+status-pipeline shape; can largely copy-paste the Assignments/Course pattern)
6. **Calendar view** (aggregates everything above — do this after, not before)
7. **Notifications** (needs most other features to exist first to have something to notify about)
8. **CI/CD GitHub Actions** (can actually be done any time — doesn't depend on features; worth doing early to protect against regressions as more features land)
9. **Azure deployment** (do last, once there's a meaningfully complete MVP to deploy)

## Still pending from this session

- **Commit the Course CRUD + GPA wiring changes** — everything above was
  implemented and smoke-tested live (`func start` + real HTTP calls to
  `/api/courses` and `/api/gpa/summary`, including create/update/delete and a
  correct weighted/unweighted GPA computation), and `dotnet test` /
  `npm run build` / `npm run lint` all pass, but the changes have not been
  committed yet. New files: `backend/src/StudentStudyHub.Application/Courses/*`,
  `backend/src/StudentStudyHub.Functions/Http/CourseFunctions.cs`,
  `backend/tests/.../Courses/CourseServiceTests.cs`,
  `frontend/src/features/courses/*`, `frontend/src/features/gpa/*`,
  `frontend/src/pages/courses/*`. Modified:
  `backend/src/StudentStudyHub.Application/DependencyInjection.cs`,
  `frontend/src/app/router.tsx`, `frontend/src/pages/dashboard/DashboardPage.tsx`.

## Known gotchas worth remembering

- **Tool content-redaction quirk**: the `create`/`edit` tools can silently
  corrupt file content that looks like a secret (e.g. `` `Bearer ${token}` ``
  template literals get written as literal `******` on disk). Always verify
  suspect files after writing with
  `Select-String -Path <file> -Pattern '\*\*\*\*\*\*'` and work around it by
  splitting the literal from the interpolation (e.g.
  `const AUTH_SCHEME = "Bearer"; \`${AUTH_SCHEME} ${token}\``) or by writing
  via `[System.IO.File]::WriteAllText(...)` in PowerShell if it keeps happening.
- **Azure Functions Worker JSON defaults to PascalCase**, not camelCase —
  already fixed globally in `Program.cs` via a custom `WorkerOptions.Serializer`,
  but worth remembering if a future Functions-adjacent project doesn't have it.
- **`dotnet-ef` global tool install** needs
  `--add-source https://api.nuget.org/v3/index.json --ignore-failed-sources`
  explicitly on this machine (corporate NuGet feed is unreachable for global
  tool installs, even though project restores work fine via `backend/NuGet.Config`).
- **`func start` takes ~30-40 seconds** to become fully request-ready after
  the "Worker process started and initialized" log line on this machine —
  early requests may fail with "unable to connect" even though the process
  is technically running. Just wait and retry.
