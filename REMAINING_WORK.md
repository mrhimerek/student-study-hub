# Remaining Work — Student Study Hub

> **Purpose:** a resumable snapshot of what's done and what's left, so work can
> pick back up cleanly in a future session. See `DESIGN.md` for the full
> architecture/product spec this all traces back to.

Last updated: 2026-10-13. Current `main` HEAD: builds on `f81026d` with Course
CRUD + GPA frontend wiring, then Exam CRUD + countdown dashboard widget, then
Study Groups + Shared Notes, then Scholarship tracker + College application
tracker, then SAT study planner, added on top.

## How to resume

```powershell
# Backend
cd backend
dotnet test                         # should show 88/88 passing

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
| **Exam/quiz countdown** | ✅ | Full CRUD API (`ExamService`/`ExamFunctions`) + FluentValidation + 9 unit tests, plus a `GET /api/exams/next` endpoint (204 when none upcoming) for the dashboard widget; full frontend (`features/exams`, `pages/exams/ExamsPage` with add/edit/delete), wired into `router.tsx` at `/exams`; Dashboard's "Next Exam" card now uses `useNextExam()` with a live days-left countdown, replacing the old mock `Biology Final` |
| **Study groups + Shared notes** | ✅ | `StudyGroupService`/`StudyGroupFunctions` + `GroupNoteService`/`GroupNoteFunctions`, both membership-scoped (not single-owner like other features) with `ForbiddenException` → 403 for member-but-not-owner actions, 12 unit tests; `CreateStudyGroupRequest.SchoolName` does a find-or-create exact-match against `Schools` (no School CRUD/picker exists yet); invite-code join/leave (owner can't leave), owner-only edit/delete; notes are author-editable, author-or-group-owner-deletable (light moderation); full frontend (`features/studyGroups`, `pages/study-groups/{StudyGroupsPage,StudyGroupDetailPage,...}`) wired into `router.tsx` at `/study-groups` and `/study-groups/:id`, replacing the old `PlaceholderPage` |
| **Scholarship tracker** | ✅ | Full CRUD API (`ScholarshipService`/`ScholarshipFunctions`) + FluentValidation + 10 unit tests; single-owner (like Course/Exam, not membership-scoped), status pipeline `researching → applying → submitted → awarded/rejected`, optional `amount`, deadline, and a simple checklist (`Requirements: string[]`) persisted as a JSON column (`RequirementsJson`) and (de)serialized in the service; full frontend (`features/scholarships`, `pages/scholarships/{ScholarshipsPage,ScholarshipCard,ScholarshipForm}`) wired into `router.tsx` at `/scholarships`; Dashboard gained a "Scholarships" card showing the soonest open deadline |
| **College application tracker** | ✅ | Full CRUD API (`CollegeApplicationService`/`CollegeApplicationFunctions`) + FluentValidation + 10 unit tests; single-owner, status pipeline `notStarted → inProgress → submitted → accepted/rejected/waitlisted`, `applicationType` (`earlyDecision`/`earlyAction`/`regularDecision`/`rolling`), deadline, and the same JSON-backed `Requirements: string[]` checklist pattern as Scholarships; full frontend (`features/collegeApplications`, `pages/college-applications/{CollegeApplicationsPage,CollegeApplicationCard,CollegeApplicationForm}`) wired into `router.tsx` at `/college-applications`; Dashboard gained a "College Apps" card showing the soonest open deadline |
| **SAT study planner** | ✅ | Full CRUD API (`SatPlanService`/`SatPlanFunctions`) + FluentValidation + 13 unit tests; unlike the other trackers this is a **single-record-per-user** resource (no `{id}}` route segment) with a nested practice-score log (`SatPracticeScore`, scoped by `SatPlanId`) - `GET /api/sat-plan` returns 204 when the user hasn't created a plan yet, `POST` throws a `DomainException` → 400 if a plan already exists, `POST/DELETE /api/sat-plan/scores[/{scoreId}]` log/remove a practice test score and return the refreshed plan; full frontend (`features/satPlan`, `pages/sat-plan/{SatPlanPage,SatPlanForm,SatPracticeScoreForm}`) wired into `router.tsx` at `/sat-plan`, showing goal/latest/best score plus a countdown to the target test date; Dashboard gained an "SAT Plan" card |
| Dashboard greeting | ✅ | Pulls real `firstName` from `useAuth()` (the due-today list is still mock data, pending Assignments-on-Dashboard wiring) |

## ❌ Not started

These all have a Domain entity already scaffolded (so the data model exists),
but **no Application service, no Function endpoints, and no frontend** yet.
Build each the same way Assignments was built: DTOs → FluentValidation →
service → Functions endpoints → unit tests → smoke test → frontend
types/api/hooks → pages → wire into `router.tsx`.

| Feature | Domain entities already scaffolded | What's needed |
|---|---|---|
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
3. ~~Exam countdown~~ ✅ done
4. ~~Study groups + Shared notes~~ ✅ done
5. ~~Scholarship tracker and College application tracker~~ ✅ done
6. ~~SAT study planner~~ ✅ done
7. **Calendar view** (aggregates everything above — do this next) — **start here next**
8. **Notifications** (needs most other features to exist first to have something to notify about)
9. **CI/CD GitHub Actions** (can actually be done any time — doesn't depend on features; worth doing early to protect against regressions as more features land)
10. **Azure deployment** (do last, once there's a meaningfully complete MVP to deploy)


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
