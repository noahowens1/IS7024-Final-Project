# Cincy Events Constitution

## Core Principles

### I. Maintainable C# and Clear Separation
C# code MUST follow Microsoft conventions and use clear, descriptive names, focused
methods, and the repository's existing patterns. Razor Pages MUST handle page
binding and presentation; application rules belong in application services, and
persistence belongs in the data-access layer. Keep each layer small and testable,
and introduce abstractions only when they clarify responsibilities or support a
real integration need. This keeps a four-person team able to understand and safely
extend one another's work.

### II. Secure Authentication and User-Scoped Data
Features that require an account MUST use the application's approved ASP.NET Core
authentication and authorization mechanisms. Every user-specific read, create,
update, and delete MUST be scoped to the authenticated user on the server; a
submitted user identifier MUST NOT be trusted as proof of ownership. Validate
authorization at the operation boundary, protect state-changing requests against
cross-site request forgery, and keep credentials and secrets out of source control.
These controls protect user data as account features are added.

### III. Validated Inputs and Consistent Data
All externally supplied and user-submitted values MUST be validated on the server
before use. Database constraints and application checks MUST work together to
preserve data integrity, including preventing duplicate records where uniqueness
is part of the feature's rules. Invalid or failed operations MUST produce clear,
safe feedback rather than silent success. Event data from current or future
providers MUST be mapped into consistent application-owned fields so integrations
do not leak provider-specific formats throughout the application.

### IV. Accessible, Responsive User Experience
Pages MUST use semantic HTML, correctly associated labels, keyboard-operable
controls, visible focus, and understandable validation and status messages.
Layouts MUST remain usable on common mobile and desktop viewport sizes. Use the
existing Bootstrap and Razor Pages conventions where they meet these requirements;
verify accessibility and responsive behavior when a feature changes the interface.

### V. Tested, Debuggable Changes
Contributors MUST run the relevant automated tests and build, and manually exercise
important user flows when practical, before requesting review. New behavior and
bug fixes MUST include or update focused tests when test infrastructure supports
them; database, authentication, and integration behavior warrant tests at the
appropriate boundary. Diagnose failures and report any unrun checks explicitly
rather than treating them as passed. This helps the team catch regressions without
requiring a heavyweight process.

## Additional Constraints

The application MUST remain compatible with ASP.NET Core Razor Pages and .NET 8
unless the team explicitly approves and coordinates a framework change. Changes
MUST respect the existing project structure and team code, and MUST NOT rework
unrelated features as part of a focused task. Keep event sources behind clear
application-facing boundaries so future API integrations can be added without
coupling page behavior to a particular provider. Update relevant requirements,
design artifacts, or setup instructions when a change materially alters them.

## Development Workflow

Work MUST be done on small, focused feature branches with meaningful names and
commits. A pull request MUST explain the user-visible change, note relevant design
or data impacts, and list the tests and checks performed. Each pull request MUST
receive review from at least one other team member before merging; authors address
review feedback or document the team's agreed resolution. Reviewers check
correctness, security, accessibility where applicable, tests, and consistency with
this constitution. The team may tailor validation to the scope of a change, but
must identify and communicate any checks that could not be completed.

## Governance

This constitution guides feature decisions and code review for the four-person
Cincy Events team. Any team member may propose an amendment in a focused pull
request. The team discusses and agrees to the change before merge, and the pull
request records the rationale and any effects on existing work. Reviewers assess
constitution compliance as part of normal pull-request review; a deviation must
be justified in the pull request and agreed by the team.

Version the constitution using semantic versioning: increment MAJOR for
incompatible changes to established principles, MINOR for new principles or
materially expanded requirements, and PATCH for clarifications that do not change
intent. Update the version and amendment date with every approved change. Keep
implementation proportionate to a student project: prefer the smallest design
that meets the principles, and agree as a team before introducing significant
complexity or a compatibility-breaking change.

**Version**: 1.0.0 | **Ratified**: 2026-11-07 | **Last Amended**: 2026-11-07
