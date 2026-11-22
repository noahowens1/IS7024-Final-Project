# Research: Saved Events

## Decisions

### Local persistence: EF Core with SQLite

- **Decision**: Use the EF Core SQLite provider for the local application
  database. Store Identity records, event records, and saved-event relationships
  in the same configured database at `CincyEvents/App_Data/cincy-events.db`,
  outside the web root.
- **Rationale**: The project has no database today, and the feature needs
  durable user/event relationships and database-enforced uniqueness. SQLite is
  low-friction for a local student project while remaining supported by EF Core.
  Keep the provider behind the application's data context so a later team
  database choice does not change saved-event page behavior.
- **Alternatives considered**: In-memory lists do not survive restart or
  enforce concurrent uniqueness. A remote database adds setup and secret
  management unrelated to local feature development. A different provider can
  be selected by the team later if deployment requirements demand it.

### Real sign-in: ASP.NET Core Identity

- **Decision**: Configure ASP.NET Core Identity with persisted EF Core user
  stores and Microsoft's standard Identity UI for registration, login, and
  logout. Use its cookie authentication and login challenge for protected
  saved-event operations.
- **Rationale**: No authentication exists in the scaffold. Identity provides
  established account and principal handling; no demo identity, hardcoded user
  key, or authentication bypass is acceptable. The standard UI and EF store
  provide the minimum account flows this feature needs without building custom
  authentication. Tests provision generated users in an isolated real Identity
  store and exercise the real sign-in pipeline. Password recovery, external
  providers, email delivery/confirmation, and profile management remain out of
  scope; newly registered accounts can sign in without an email-confirmation
  dependency.
- **Alternatives considered**: A custom cookie or ad-hoc session identity
  duplicates security-sensitive framework behavior. External sign-in requires
  provider configuration and credentials not present in the project.

### Save ownership and duplicate handling

- **Decision**: Obtain the owner ID from the authenticated Identity principal,
  scope list and removal operations to that ID, and define a unique constraint
  over `(UserId, EventId)` in persistent storage. Check for an existing pair
  before insert for clear feedback, while treating the unique constraint as
  the final concurrency guard.
- **Rationale**: An application check alone can race when two saves arrive
  together. A database uniqueness constraint guarantees the required invariant.
  Returning a duplicate result for the constraint conflict preserves the
  specified user message without exposing other users' entries.
- **Alternatives considered**: A client-provided user ID is untrusted. A
  page-only duplicate check cannot prevent concurrent duplicate inserts.

### Minimal event source for local acceptance

- **Decision**: Add a small, deterministic development-only catalog of sample
  events, with an explicit "Sample event - not live event information" label.
  Do not seed sample catalog entries in production. Keep the event source
  behind a narrow application-facing boundary.
- **Rationale**: The existing project has no event model or event provider, but
  save and list flows need selectable events to demonstrate and test. Visible
  labeling prevents sample data from being mistaken for a verified local event.
  A small catalog is sufficient without implementing discovery.
- **Alternatives considered**: A live provider integration is explicitly out
  of scope and needs credentials, provider mapping, and maintenance. Fabricated
  success responses or unmarked sample events would mislead users.

### Testing approach

- **Decision**: Add an xUnit test project. Test save eligibility, ownership, and
  duplicate outcomes; use SQLite-backed integration tests for relationships
  and unique-index behavior; exercise successful and invalid registration,
  successful and invalid login, logout, and Saved Events Razor Pages using
  generated real Identity accounts and an isolated test database.
- **Rationale**: The main risks cross service, database, and authentication
  boundaries. SQLite integration tests verify the same relational constraint
  the application uses. Real account creation and sign-in exercise the
  configured authentication pipeline without shipping a fake authentication
  path.
- **Alternatives considered**: Mock-only persistence tests cannot validate
  provider constraints or migrations. A test authentication bypass would not
  verify the required login redirect or real identity ownership.

## Integration Dependencies

- Microsoft's standard ASP.NET Core Identity UI and EF Core stores provide
  registration, login, logout, persistent accounts, and cookie authentication
  within this feature. No external authentication foundation is required.
- EF Core SQLite provider, EF design-time tooling, an initial migration, and a
  configured writable local database path are required.
- A persistent event catalog is required for saved records to resolve event
  details. For the feature branch, the only planned catalog population is
  development-only sample data.
- The application has no automated test project currently; test project and
  web-host test dependencies must be added by implementation tasks.
- No event API keys, provider integration, or external weather service is
  required.

## Open Decisions

No blocking requirements clarification remains. The local database file is
`CincyEvents/App_Data/cincy-events.db` and is git-ignored. This feature includes
its minimum standard Identity account flows; email confirmation, external
providers, and broader account management are not included.
