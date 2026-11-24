<!--
Sync Impact Report
- Version change: unratified scaffold -> 1.0.0
- Modified principles: none; all principles are established by this initial constitution
- Added sections: Additional Constraints; Development Workflow
- Removed sections: none
- Follow-up TODOs: confirm the original ratification date
-->
# CincyEvents Constitution

## Core Principles

### I. Razor Pages and .NET Alignment
Application features MUST use the existing ASP.NET Core Razor Pages architecture and target the project's supported .NET version. New functionality MUST follow the established page-model, service, model, and shared-layout conventions unless a documented architectural decision approves an exception. This keeps the application cohesive and maintainable.

### II. Secure by Default
Authentication, authorization, input validation, and data-access boundaries MUST be enforced on the server. User-controlled identifiers MUST be checked against the current user's permissions before data is returned or changed. Client-side checks MAY improve usability but MUST NOT replace server-side enforcement. This protects event participation, accounts, and chat data.

### III. Verifiable Changes
Every feature or defect fix MUST have a reproducible validation step appropriate to its scope. Changes MUST be compiled with the target project, and affected user flows, handlers, or service contracts MUST be tested manually or with automated tests. A change MUST NOT be considered complete while known errors remain unexplained. This makes regressions visible before review.

### IV. Consistent Domain Data
Each domain concept MUST have a single authoritative representation. Related pages and handlers MUST reuse shared models, catalogs, and database relationships rather than duplicating business rules or display data. Database changes MUST preserve referential integrity and the existing application's data-access conventions.

### V. Accessible, Focused User Experience
User-facing features MUST provide clear labels, meaningful feedback, keyboard-accessible controls, and responsive presentation using the existing shared layout and styling conventions. Implementations MUST prefer the smallest design that satisfies the requirement and MUST avoid speculative framework, dependency, or infrastructure additions.

## Additional Constraints

- The application MUST target .NET 8 and use ASP.NET Core Razor Pages for web UI features.
- Existing Identity, Entity Framework Core, SQL Server, SignalR, Bootstrap, and shared layout conventions SHOULD be reused before introducing new dependencies.
- Secrets, connection strings containing credentials, and generated build output MUST NOT be committed to source control.
- Real-time endpoints MUST preserve authentication and authorization behavior for browser clients.

## Development Workflow

- Work MUST begin from an agreed specification or a clearly stated defect and MUST remain within that scope unless the change is explicitly documented.
- Changes MUST follow existing project naming, formatting, and folder conventions.
- Before review, contributors MUST build the affected project and report relevant test results, unresolved warnings, and any environment-dependent limitations.
- Reviews MUST check security boundaries, validation, accessibility, data consistency, and whether the implementation adds unnecessary complexity.

## Governance

This constitution governs project specifications, plans, implementation tasks, and reviews. When a proposal conflicts with a principle, the proposal MUST document the conflict, rationale, impact, and approval before implementation. Amendments MUST update the version and amendment date, include a Sync Impact Report, and explain any migration or compliance work required. Versioning follows semantic versioning: MAJOR for incompatible governance changes, MINOR for new or materially expanded principles, and PATCH for clarifications that do not change obligations.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): confirm original project adoption date | **Last Amended**: 2026-11-23