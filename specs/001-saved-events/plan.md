# Implementation Plan: Saved Events

**Branch**: `feature/saved-events-pazag` | **Date**: 2026-11-07 | **Spec**:
[spec.md](./spec.md)

**Input**: Feature specification from `specs/001-saved-events/spec.md`

## Summary

Add a persistent, authenticated saved-event experience to the existing .NET 8
Razor Pages application. Use ASP.NET Core Identity for real account sign-in and
user identity, EF Core with SQLite for local persistence, and a user-scoped
`SavedEvent` relationship with a database-enforced unique user/event pair.
Provide only the minimal event listing needed to exercise the feature, using
clearly labeled development sample events; do not implement event search or
provider integrations.

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**: ASP.NET Core Razor Pages, Microsoft ASP.NET Core
Identity UI and Identity EF Core integration, Entity Framework Core 8 with the
SQLite provider

**Storage**: SQLite at `CincyEvents/App_Data/cincy-events.db` for local
Identity accounts, events, and saved-event relationships; keep the directory
outside `wwwroot`, create it when needed, git-ignore the database and SQLite
journal files, and do not commit secrets

**Testing**: New focused .NET test project using xUnit; unit tests for save
rules, SQLite-backed integration tests for migrations, Identity persistence,
uniqueness, and sample-seed environment behavior; Razor Pages integration tests
for successful and invalid registration, login, logout, authorization redirects,
ownership, and visible outcomes

**Target Platform**: ASP.NET Core web application on .NET 8; local development
on the supported team environments

**Project Type**: Single ASP.NET Core Razor Pages application

**Performance Goals**: On a local development database with sample data, common
saved-event page actions complete within 2 seconds in manual acceptance runs;
no production-scale throughput target is required for this student project

**Constraints**: Preserve .NET 8 and current Razor Pages structure; use
Microsoft's standard ASP.NET Core Identity UI and EF Core stores for registration,
login, logout, and cookie authentication; never use hardcoded identities or
authentication bypasses; apply authorization and antiforgery protection to
mutations; enforce uniqueness in persistent storage; keep sample records visibly
marked and out of live-event claims; keep event-provider integrations and
discovery out of scope. Do not require email confirmation in this feature because
no mail-delivery flow is included; retain Identity's standard password
validation and sign-in protections.

**Scale/Scope**: One web application, standard local Identity account flows, a
small local development catalog, and user-specific saved lists. No external
identity provider, distributed architecture, or production event-feed scale is
assumed.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle or constraint | Plan alignment |
|---|---|
| Maintainable C# and clear separation | Razor Pages handle binding/presentation; a saved-event service owns use cases; EF Core context owns persistence. |
| Secure authentication and user-scoped data | Configure Microsoft's ASP.NET Core Identity UI and EF Core stores for real registration/login/logout; derive owner identity from authenticated principal claims and scope every list/remove operation. |
| Validated inputs and consistent data | Validate event IDs and event start times; enforce user/event uniqueness and foreign keys in the database; keep event fields provider-neutral. |
| Accessible, responsive experience | Use semantic Razor markup, labeled actions, keyboard access, visible feedback, and existing Bootstrap layout. |
| Tested, debuggable changes | Add focused unit and integration coverage, build and test before review, and document checks that cannot run. |
| .NET 8, focused scope, future integrations | Stay in the existing application; add no search or provider-feed feature; isolate event source data from saved-list behavior. |

**Pre-research gate**: PASS. ASP.NET Core Identity's standard UI and EF Core
stores provide the minimum real account flows needed by Saved Events without a
custom authentication system or unrelated account features.

**Post-design gate**: PASS. The proposed design preserves the same boundaries,
security requirements, test coverage, and narrow feature scope.

## Project Structure

### Documentation (this feature)

```text
specs/001-saved-events/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── page-flows.md
```

### Source Code (repository root)

```text
CincyEvents/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DevelopmentEventSeeder.cs
├── Models/
│   ├── Event.cs
│   └── SavedEvent.cs
├── Services/
│   └── SavedEventsService.cs
├── Pages/
│   ├── Events/
│   │   ├── Index.cshtml
│   │   └── Index.cshtml.cs
│   ├── SavedEvents/
│   │   ├── Index.cshtml
│   │   ├── Index.cshtml.cs
│   │   ├── Save.cshtml
│   │   └── Save.cshtml.cs
│   └── Shared/
├── Migrations/
└── Program.cs

CincyEvents.Tests/
├── Services/
└── Pages/
```

`Events/Index` is a minimal, non-searching catalog page used to expose upcoming
sample records and the Save action. `SavedEvents/Save` and `SavedEvents/Index`
are authenticated pages; the index also handles removal. Register Microsoft's
standard Identity UI with EF Core stores and expose its built-in Register,
Login, and Logout pages; do not scaffold or customize those pages unless a
feature requirement cannot be met by the defaults. Configure the authentication
and authorization middleware in `Program.cs`, with authentication middleware
before authorization middleware, then map Razor Pages so Identity UI routes are
available. Use `CincyEvents/App_Data/cincy-events.db` as the local default and
ensure its parent directory exists before opening the database. Do not add a
generic repository layer, external identity provider, or separate API project
for this feature. Account confirmation and email delivery are out of scope, so a
newly registered user can sign in without a confirmation email.

**Structure Decision**: Extend the existing single `CincyEvents` web project,
which currently contains only the default Razor Pages scaffold. Add one
corresponding test project for isolated and end-to-end feature coverage. Keep
the saved-event use case in a small application service rather than placing
database rules in page handlers. The feature owns the minimum standard Identity
registration, login, and logout setup so its authenticated flows can run without
an external authentication foundation. Leave Identity UI pages in the standard
framework UI unless a documented requirement cannot be met by the defaults.

## Complexity Tracking

No constitution violations or additional architecture projects are proposed.
