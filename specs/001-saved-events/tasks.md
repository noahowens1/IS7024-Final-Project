# Tasks: Saved Events

**Input**: Design documents from `specs/001-saved-events/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/page-flows.md`, `quickstart.md`

**Organization**: Tasks are dependency-ordered and grouped by user story after
the shared application, database, and standard Identity foundation. The feature
implements its own minimum registration, login, and logout flows using
Microsoft ASP.NET Core Identity; no external authentication foundation is
required.

## Phase 1: Setup

**Purpose**: Add only project dependencies and test infrastructure needed for
Saved Events and standard Identity.

- [X] T001 Add EF Core 8 SQLite, ASP.NET Core Identity EF Core/UI, and EF design package references to `CincyEvents/CincyEvents.csproj`.
- [X] T002 [P] Create `CincyEvents.Tests/CincyEvents.Tests.csproj` with xUnit, ASP.NET Core web-host testing, and EF Core SQLite test dependencies; add it to `CincyEvents/CincyEvents.sln`.
- [X] T003 [P] Ignore `CincyEvents/App_Data/cincy-events.db` and its SQLite journal files in `.gitignore`.

## Phase 2: Foundational

**Purpose**: Configure persisted Identity, SQLite, migrations, and shared
entities used by all user stories.

- [X] T004 Create `CincyEvents/Models/Event.cs` and `CincyEvents/Models/SavedEvent.cs` with these `data-model.md` constraints verbatim: Event `Title` is "Required, non-empty"; `Category` is "Optional"; `StartAtUtc` is "Required; persisted consistently as UTC"; `EndAtUtc` is "Optional; if present, must not precede start"; `TimeZoneId` is "Optional only when the source has no zone"; `Venue` is "Optional if the source has no location"; `PriceMin` is "Optional; non-negative when present"; `PriceMax` is "Optional; non-negative and not less than minimum when both are present"; `TicketUrl` "Must be a safe absolute HTTP(S) URL when present"; `SourceName` is "Required for seeded sample records"; and `IsSample` is "True for development seed events; display an explicit sample-data label". SavedEvent `Id` is a "Primary key"; `UserId` is a "Required foreign key to Identity user; assigned from authenticated principal"; `EventId` is a "Required foreign key to Event"; and `SavedAtUtc` is a "Required UTC timestamp".
- [X] T005 Configure `CincyEvents/Data/ApplicationDbContext.cs` to inherit from `IdentityDbContext<IdentityUser>` and expose event/saved-event sets; do not introduce a custom authentication user type without a feature need.
- [X] T006 Register Microsoft's standard Identity UI and EF Core stores in `CincyEvents/Program.cs` using `AddDefaultIdentity<IdentityUser>()` and `AddEntityFrameworkStores<ApplicationDbContext>()`; set `RequireConfirmedAccount` to false because email delivery is out of scope, retain standard password validation and cookie protections, and do not add external providers or authentication bypasses.
- [X] T007 Configure authentication and authorization middleware order in `CincyEvents/Program.cs`, placing `UseAuthentication()` before `UseAuthorization()` and mapping Razor Pages with the standard Identity UI routes enabled.
- [X] T008 Configure the SQLite connection in `CincyEvents/appsettings.json` and `CincyEvents/Program.cs` to use `CincyEvents/App_Data/cincy-events.db` outside `wwwroot`; create `App_Data` when needed and keep the database, journal files, and secrets out of commits.
- [X] T009 Configure event and saved-event relationships, required foreign keys, delete behavior, and a unique index on `(UserId, EventId)` in `CincyEvents/Data/ApplicationDbContext.cs`.
- [X] T010 Create and apply the initial EF migration under `CincyEvents/Migrations/` for Identity, event, and saved-event tables.
- [X] T011 Add deterministic sample-event seeding in `CincyEvents/Data/DevelopmentEventSeeder.cs`; run it only in Development and label every sample "Sample event - not live event information"; test that production startup does not seed samples in `CincyEvents.Tests/Data/DevelopmentEventSeederTests.cs`.
- [X] T012 Add an isolated SQLite test-database fixture in `CincyEvents.Tests/Infrastructure/SqliteTestDatabase.cs`; verify the initial migration can be applied without touching the developer database.

**Checkpoint**: The application can create Identity accounts, sign in/out,
persist Identity and feature data in SQLite, and provision clearly labeled
development sample events.

## Phase 3: User Story 1 - Save an Upcoming Event (Priority: P1)

**Goal**: A signed-in user saves a valid upcoming event and receives the
specified confirmation.

**Independent Test**: Register a real test account through Identity, sign in,
save one available upcoming event, and verify "Event Saved." and exactly one
persistent saved relationship for that account.

### Tests for User Story 1

- [X] T013 [P] [US1] Add service tests for upcoming, started, ended, missing, and invalid event IDs in `CincyEvents.Tests/Services/SavedEventsServiceTests.cs`.
- [X] T014 [US1] Add an Identity-backed web integration test that registers/signs in a real test account, saves an upcoming event, displays "Event Saved.", and persists the current user's relationship in `CincyEvents.Tests/Pages/SaveEventPageTests.cs`.

### Implementation for User Story 1

- [X] T015 [US1] Implement upcoming-event validation, current-user association from the authenticated principal, and UTC save timestamp in `CincyEvents/Services/SavedEventsService.cs`; never accept owner ID from form input.
- [X] T016 [US1] Add a minimal non-searching event listing with sample-data disclosure in `CincyEvents/Pages/Events/Index.cshtml` and `CincyEvents/Pages/Events/Index.cshtml.cs`.
- [X] T017 [US1] Add the authenticated POST save handler with antiforgery validation and same-application return-path validation in `CincyEvents/Pages/SavedEvents/Save.cshtml.cs`; show "Event Saved." only after persistence succeeds.
- [X] T018 [US1] Add accessible Save Event forms and status feedback to `CincyEvents/Pages/Events/Index.cshtml`; keep `CincyEvents/Pages/SavedEvents/Save.cshtml` as the focused save-guidance endpoint.

**Checkpoint**: A real Identity user can save one available upcoming event.

## Phase 4: User Story 2 - Register, Sign In, and Require Sign-In to Save (Priority: P1)

**Goal**: Anonymous save attempts are redirected to the standard Identity login
flow without creating a relationship or silently replaying the save after
sign-in.

**Independent Test**: Register a real account through the standard UI, sign
in, sign out, and verify each resulting state. While signed out, submit Save;
verify redirect to `/Identity/Account/Login` and no database row. Sign in
afterward and verify the user must explicitly retry the save.

- [X] T019 [P] [US2] Test successful registration followed by sign-in without email confirmation, invalid registration feedback, successful login, invalid login, and logout through standard Identity UI routes with a real test database in `CincyEvents.Tests/Pages/IdentityAccountFlowTests.cs`.
- [X] T020 [P] [US2] Test anonymous save redirect, no persisted save, explicit retry after sign-in, and rejection of an external return URL in `CincyEvents.Tests/Pages/AnonymousSavePageTests.cs`.
- [X] T021 [US2] Verify anonymous requests to `CincyEvents/Pages/SavedEvents/Save.cshtml.cs` challenge to the configured Identity login route without a custom identity shortcut.
- [X] T022 [US2] Configure the shared navigation for standard Register/Login/Logout actions based on authentication state in `CincyEvents/Pages/Shared/_Layout.cshtml`; use the built-in Identity UI endpoints.

**Checkpoint**: Registration, login, logout, and anonymous save redirection use
Microsoft Identity functionality; an anonymous save is not performed before or
automatically after sign-in.

## Phase 5: User Story 3 - View Saved Events (Priority: P2)

**Goal**: A signed-in user can find only their own saved events with the event
details needed to plan.

**Independent Test**: With two real Identity accounts and saved records for
each, verify account isolation, event details, missing-price display, empty
state, and navigation within two actions.

- [X] T023 [P] [US3] Add tests for user-scoped saved-event listing, event details, missing price, and empty state in `CincyEvents.Tests/Services/SavedEventsListTests.cs`.
- [X] T024 [US3] Extend `CincyEvents/Services/SavedEventsService.cs` to list only the authenticated user's events with title, local date/time, venue, and optional price.
- [X] T025 [US3] Create the authenticated Saved Events page model and query in `CincyEvents/Pages/SavedEvents/Index.cshtml.cs`; scope the query using the server-side principal.
- [X] T026 [US3] Render event details, unavailable-price text, sample labels, and an informative empty state in `CincyEvents/Pages/SavedEvents/Index.cshtml`.
- [X] T027 [US3] Add a Saved Events navigation link in `CincyEvents/Pages/Shared/_Layout.cshtml` and verify it is reachable within two actions.
- [X] T028 [US3] Add two-user ownership-isolation and real-login page tests in `CincyEvents.Tests/Pages/SavedEventsListPageTests.cs`.

**Checkpoint**: A signed-in user can view an empty or populated personal list
without cross-account disclosure.

## Phase 6: User Story 4 - Prevent Duplicates and Remove a Saved Event (Priority: P2)

**Goal**: Repeated saves create no duplicate, and users can remove only their
own saved relationships.

**Independent Test**: Save the same event twice for one signed-in user, confirm
one row and the duplicate message, then remove it and confirm other events and
other users' relationships remain unchanged.

- [X] T029 [P] [US4] Add tests for repeated saves, remove-own, remove-missing, and remove-foreign outcomes in `CincyEvents.Tests/Services/SavedEventsDuplicateAndRemoveTests.cs`.
- [X] T030 [US4] Handle an existing `(UserId, EventId)` pair in `CincyEvents/Services/SavedEventsService.cs` and translate only the expected unique-constraint conflict into the duplicate outcome.
- [X] T031 [US4] Implement removal scoped to both the saved-entry ID and authenticated user ID in `CincyEvents/Services/SavedEventsService.cs`; leave Event and other users' rows intact.
- [X] T032 [US4] Add an authenticated antiforgery-protected Remove POST handler in `CincyEvents/Pages/SavedEvents/Index.cshtml.cs`; return a safe outcome for missing or foreign entries.
- [X] T033 [US4] Add keyboard-operable Remove forms and status feedback in `CincyEvents/Pages/SavedEvents/Index.cshtml`.
- [X] T034 [US4] Add real-Identity integration tests for duplicate feedback, concurrent uniqueness, and cross-user removal isolation in `CincyEvents.Tests/Pages/SavedEventsOwnershipPageTests.cs`.

**Checkpoint**: Repeated/concurrent saves remain unique and removal affects
only the current user's selected saved relationship.

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate the complete feature, account flows, security boundaries,
documentation, and responsive/accessibility behavior before review.

- [X] T035 [P] Verify validation messages, keyboard access, visible focus, and mobile-width layouts in `CincyEvents/Pages/Events/Index.cshtml`, `CincyEvents/Pages/SavedEvents/Save.cshtml`, `CincyEvents/Pages/SavedEvents/Index.cshtml`, and the default Identity UI enabled in `CincyEvents/Program.cs`.
- [X] T036 Run all Identity, Saved Events, SQLite, and Razor Pages integration tests and resolve failures in `CincyEvents.Tests/CincyEvents.Tests.csproj`.
- [X] T037 Run the application build and all tests; record exact commands and any unavailable checks in `specs/001-saved-events/quickstart.md`.
- [X] T038 Update local database and standard Identity setup instructions after implementation in `specs/001-saved-events/quickstart.md`; confirm no database file, credentials, or live-event claims are committed.

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; package, test-project, and ignore-file
  setup can be split across contributors.
- **Foundational (Phase 2)**: Depends on Setup. Configure the Identity EF store
  and Identity UI before applying the migration. The Identity user/context,
  application context relationships, SQLite configuration, and migration must
  be complete before user stories.
- **User Stories (Phases 3-6)**: Depend on the complete foundational phase.
  Stories are ordered by specification priority; they share the saved-event
  service and database.
- **Polish (Phase 7)**: Depends on all desired stories being integrated.

### User Story Dependencies

- **US1 (P1)**: Depends on the Identity sign-in flow, SQLite persistence, and
  sample event catalog from Phase 2.
- **US2 (P1)**: Depends on standard Identity UI and middleware from Phase 2 and
  the save action from US1.
- **US3 (P2)**: Depends on saved records from US1 and authenticated user
  identity from Phase 2.
- **US4 (P2)**: Depends on the unique index from Phase 2 and saved records from
  US1.

### Parallel Opportunities

- In Setup, T002 and T003 can run independently; coordinate T001 with other
  edits to the project file.
- In the foundational phase, model creation (T004), test-project setup (T002),
  and git-ignore changes (T003) touch separate files. Configure the Identity
  user/context before migration work.
- Once the foundation is complete, service tests and page tests can be authored
  independently in separate files. T011's production-seeding test is in a
  separate test file and can be authored alongside the seeder.
- Within US3, view rendering (T026) and navigation (T027) touch different
  files.
- Do not parallelize concurrent edits to `SavedEventsService.cs` (T015, T024,
  T030, T031); integrate those increments sequentially.

## Parallel Example: User Story 3

```text
After foundational tasks and T025 are complete:
Task: Add saved-event detail and empty-state rendering in CincyEvents/Pages/SavedEvents/Index.cshtml
Task: Add the Saved Events navigation link in CincyEvents/Pages/Shared/_Layout.cshtml
Task: Add service-level listing cases in CincyEvents.Tests/Services/SavedEventsListTests.cs
```

## Implementation Strategy

### MVP First

The smallest demonstrable MVP is **US1 (save)** together with **US2
(registration, login/logout, and login protection)**. Complete the Identity and
SQLite foundation first, then save an event using a real account registered
through Microsoft's standard Identity UI. Never represent a bypassed or
hardcoded identity as a working authentication flow.

### Incremental Delivery

1. Complete setup, Identity, and database foundations.
2. Deliver US1 and US2 together so registration and save behavior are
   demonstrable and secure.
3. Add US3 to view personal saved-event details.
4. Add US4 duplicate feedback and owner-scoped removal.
5. Run the full quickstart, accessibility checks, build, and tests before PR.

### Debugging and Review Gate

- Reproduce failures using an isolated SQLite database and generated real
  Identity test accounts; never debug by adding a fixed user ID or bypass.
- Check Identity cookie configuration, migration state, unique-constraint
  exceptions, event start-time UTC conversion, and authenticated principal
  ownership when relevant.
- Run focused tests after each story, then run
  `dotnet test .\CincyEvents\CincyEvents.sln` and the quickstart flows before
  requesting review.

## Notes

- `[P]` means the task can proceed independently on different files after its
  stated prerequisites are met.
- `[US1]` through `[US4]` map to the stories in `spec.md`.
- Every task names the source or test file it changes. No task adds general
  event search, recommendations, or external provider integrations.
