# Saved Events Quickstart

## Prerequisites

- .NET 8 SDK.
- A writable local development environment.
- The configured SQLite database at `CincyEvents/App_Data/cincy-events.db`,
  created by the application when missing.
- The feature's ASP.NET Core Identity UI and persistent store configured with
  the initial database migration.
- The feature migration applied to the local database. The application also
  applies pending migrations at startup.
- Development sample events available and visibly marked as sample data. They
  are seeded only when the application environment is Development.

Do not use a hardcoded account, bypass authentication, or claim that sample
events are live. Register a real local development account through the standard
Identity UI; do not add a test-only identity provider.

## Restore, migrate, test, and run

From the repository root:

```powershell
dotnet restore .\CincyEvents\CincyEvents.sln
dotnet ef database update --project .\CincyEvents\CincyEvents.csproj --startup-project .\CincyEvents\CincyEvents.csproj
dotnet build .\CincyEvents\CincyEvents.sln
dotnet test .\CincyEvents\CincyEvents.sln
dotnet run --project .\CincyEvents\CincyEvents.csproj
```

Use EF Core tooling 8.0.22 with the EF Core 8 packages. If `dotnet-ef` is not
installed, install it with:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.22
```

The database is stored at
`CincyEvents/App_Data/cincy-events.db`, outside `wwwroot`; its parent directory
is created by the application. Keep the database and its `-shm`/`-wal` journal
files untracked; do not commit secrets.

Open the application URL shown by `dotnet run`, then use the Register and Log
in links. Logout uses the standard Identity POST action. Upcoming sample event
records are labeled "Sample event - not live event information."

## Acceptance scenarios

1. Open `/Events` while signed out. Confirm upcoming sample entries are marked
   as sample information. Submit Save for an event; confirm redirect to login
   and confirm no row was created.
2. Register a real development account at `/Identity/Account/Register`, confirm
   it can sign in without an email-confirmation step, and save an upcoming
   sample event; confirm "Event Saved." and find the event on `/SavedEvents`.
3. Submit invalid registration details and invalid login credentials; confirm
   Identity rejects them with validation feedback and does not sign in an
   invalid account.
4. Submit the same Save action again. Confirm "This event is already saved."
   and one relationship remains.
5. Save a second event, remove the first, and confirm the second remains.
6. Register/sign in as a second account. Confirm the first account's saved
   events are not visible and cannot be removed by the second account.
7. Sign out. Confirm a subsequent Save attempt redirects to login and does not
   create a saved relationship.
8. Confirm a started/ended event cannot be saved, a missing price is shown as
   unavailable, and an empty saved list has an informative empty state.
9. Verify every state-changing form rejects missing/invalid antiforgery tokens,
   and check keyboard navigation, visible focus, and mobile-width layout.

## Automated coverage expected

- Unit tests: upcoming-event rule, invalid event handling, duplicate outcome,
  and user-scoped service behavior.
- SQLite integration tests: migration application, user/event foreign keys,
  unique `(UserId, EventId)` enforcement, and concurrent/repeated saves.
- Web integration tests: real Identity registration/sign-in/sign-out, anonymous
  login redirect without a save, saved-list isolation, remove ownership, status
  messages, and empty state.
- Build and test both projects before requesting review; record any unavailable
  prerequisites or checks explicitly.

## Verification completed

- Applied `InitialIdentityAndSavedEvents` to a clean local SQLite database with
  `dotnet-ef` 8.0.22.
- `dotnet build .\CincyEvents\CincyEvents.sln --no-restore`: succeeded with
  zero warnings and zero errors.
- `dotnet test .\CincyEvents\CincyEvents.sln --no-restore --no-build`: 22
  passed, 0 failed.
- Local smoke check: `/Events` and `/Identity/Account/Register` returned 200;
  anonymous `/SavedEvents` redirected to `/Identity/Account/Login`.
- Rendered-page check confirmed sample disclosure, Cincinnati local event time,
  mobile navigation, no horizontal overflow at 390px, and keyboard focus
  styling.
