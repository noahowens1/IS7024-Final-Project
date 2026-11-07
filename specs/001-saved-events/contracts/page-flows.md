# Saved Events Page Contracts

These are user-facing Razor Pages interactions, not a public API. All
state-changing forms use POST and antiforgery validation. ASP.NET Core
authorization challenges unauthenticated requests to the configured Identity
login page. No page accepts a submitted user ID as ownership proof.

## Standard Identity account flows

| Page/action | Access | Input | Outcome |
|---|---|---|---|
| `GET /Identity/Account/Register` | Anonymous | None | Display Microsoft's standard registration form. |
| `POST /Identity/Account/Register` | Anonymous | Standard Identity registration fields | Validate using Identity's configured password/account rules; persist a valid account and permit sign-in without email confirmation because email delivery is out of scope; show validation feedback for invalid input. |
| `GET /Identity/Account/Login` | Anonymous | Optional validated local return path | Display Microsoft's standard login form. |
| `POST /Identity/Account/Login` | Anonymous | Standard Identity credentials | Sign in a valid registered user; reject invalid credentials without disclosing whether a username or password was incorrect. |
| `POST /Identity/Account/Logout` | Authenticated | Antiforgery token | End the authenticated session; subsequent protected requests challenge to login. |

## Event listing and save

| Page/action | Access | Input | Outcome |
|---|---|---|---|
| `GET /Events` | Anonymous and signed-in | None | Display only available upcoming events. Development sample entries carry a visible "Sample event - not live event information" label. |
| `POST /SavedEvents/Save` | Authenticated | Event ID | Save for the current principal if the event exists and is still upcoming; redirect to the referring event listing/details context with "Event Saved." |
| `POST /SavedEvents/Save` duplicate | Authenticated | Event ID | Leave one relationship and show "This event is already saved." |
| `POST /SavedEvents/Save` invalid/stale | Authenticated | Event ID | Create no relationship; show safe, understandable feedback. |
| `POST /SavedEvents/Save` anonymous | Anonymous request challenged | Event ID | Redirect to login; do not create a relationship and do not auto-submit after login. |

## Saved list and removal

| Page/action | Access | Input | Outcome |
|---|---|---|---|
| `GET /SavedEvents` | Authenticated | None | Display only the current user's saved events, including title, local date/time where known, venue/location, and optional price. Show a clear empty state if none exist. |
| `POST /SavedEvents?handler=Remove` | Authenticated | Saved relationship ID | Remove only a relationship owned by the current user, then redirect to the saved list with a status message. |
| Remove missing/foreign relationship | Authenticated | Saved relationship ID | Do not reveal another user's data or change any other relationship; return a safe not-found or no-op outcome consistent with the final page UX. |
| `GET /SavedEvents` anonymous | Anonymous request challenged | None | Redirect to login. |

## Common requirements

- Navigation exposes a Saved Events link that is reachable within two user
  actions.
- Event catalog pages may be viewed anonymously; saving and personal saved-list
  operations require authentication.
- Forms use semantic labels and keyboard-accessible controls; success, duplicate,
  and validation feedback is visible and announced appropriately.
- Prices that are absent are described as unavailable; sample status is never
  implied to be live or verified.
- The final return destination after a login challenge must be a validated
  same-application local path; an untrusted external return URL is rejected.
