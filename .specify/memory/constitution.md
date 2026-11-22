# Cincy Events Constitution

## Core Principles

### I. Clean, Maintainable C#

Code MUST be clear, focused, and maintainable. C# and Razor files MUST follow the existing project conventions for naming, indentation, nullable reference types, namespaces, and file organization. Implementations SHOULD prefer simple, readable solutions over unnecessary abstraction. Shared behavior SHOULD be reused rather than duplicated, and public or non-obvious logic SHOULD be documented where its purpose is not clear from the code.

### II. Razor Pages Separation of Concerns

The application MUST use the existing ASP.NET Core Razor Pages architecture. Page models SHOULD handle request processing, validation, and coordination; Razor views SHOULD focus on presentation; and reusable domain or application logic SHOULD be placed in appropriate models or services. Database, authentication, and external-service concerns MUST NOT be embedded directly in view markup. New features MUST remain compatible with the project's .NET 8 target framework.

### III. Secure Authentication and User Data

Authentication and authorization MUST use established ASP.NET Core security mechanisms rather than custom credential or session implementations. Passwords, tokens, connection strings, and other secrets MUST NOT be stored in source control or exposed in responses, logs, or client-side markup.

User-specific data MUST be scoped to the authenticated user and authorized on every read, update, and delete operation. Requests involving another user's data MUST be rejected without revealing sensitive information. Anti-forgery protection, input validation, safe output encoding, and secure cookie configuration MUST be preserved for applicable features.

### IV. Accessibility and Responsive Design

Every user-facing feature MUST be usable with keyboard navigation and readable by assistive technologies. Semantic HTML, logical heading order, visible focus states, descriptive link and control text, associated labels, and sufficient color contrast MUST be used.

Interactivity MUST NOT depend solely on color, hover, or JavaScript. Pages MUST use responsive layouts that remain usable on mobile, tablet, and desktop viewport sizes. Existing Bootstrap resources and project styles SHOULD be reused before introducing custom CSS.

### V. Test, Debug, and Validate

Each feature MUST be validated against its approved specification and acceptance criteria. Changes MUST compile successfully and SHOULD include appropriate unit, integration, or manual tests for the affected behavior.

Testing MUST cover normal paths, invalid input, authorization boundaries, and relevant error conditions. Debugging SHOULD use reproducible steps, breakpoints, logs, and focused verification rather than speculative changes. Build results, significant debugging findings, and known limitations MUST be recorded before review.

### VI. Simplicity and Scope Control

Features MUST remain within the approved specification. The team SHOULD avoid introducing a database, package, service, abstraction, or client-side dependency unless the requirement justifies it.

Existing behavior MUST be preserved unless an approved requirement explicitly changes it. Out-of-scope improvements MUST be proposed separately rather than bundled into feature work.

## Technical Constraints

- The application MUST target .NET 8 unless an approved modernization decision changes the target framework.
- ASP.NET Core Razor Pages and C# are the required application technologies for the current project structure.
- New application code MUST follow the existing directory and paired `.cshtml`/`.cshtml.cs` page conventions.
- Static assets MUST use the existing `wwwroot` structure and asset pipeline.
- Data access MUST use the project's approved data-access boundary when one is introduced; views MUST NOT access a database directly.
- External APIs and new NuGet packages MUST be justified in the plan and reviewed before adoption.
- Secrets and environment-specific configuration MUST be supplied through supported configuration or secret-management mechanisms.

## Development Workflow

1. Work MUST be performed on a feature branch with a focused scope.
2. Before implementation, the feature SHOULD have an approved specification and implementation plan. Tasks SHOULD be explicit enough to verify independently.
3. Commits SHOULD be small, focused, and descriptive. Unrelated formatting or refactoring MUST NOT be mixed into feature changes.
4. A pull request MUST describe the change, reference the relevant requirements, identify tests performed, and disclose known limitations.
5. Pull requests MUST receive peer review before merging. Reviewers SHOULD evaluate correctness, security, accessibility, maintainability, test coverage, and scope compliance.
6. A pull request MUST pass the applicable build and validation checks before merge. Unresolved review comments and failing required checks MUST be addressed or explicitly approved.
7. Direct pushes to protected branches MUST NOT bypass the required review and validation process.

## Quality Gates

A feature is ready for review only when:

- The implementation matches the approved specification and does not introduce unapproved functionality.
- The project builds without compilation or Razor errors.
- Relevant functional, invalid-input, security, accessibility, and responsive behaviors have been tested.
- User-specific operations have authorization checks and do not expose another user's data.
- No secrets, debug artifacts, generated files, or unrelated changes are included.
- The changed files and validation results are documented in the pull request.

## Governance

This constitution defines the project's baseline development standards and applies to new features, bug fixes, and refactoring. When this constitution conflicts with a feature specification, the conflict MUST be identified during planning and resolved explicitly before implementation. Security and privacy requirements MUST NOT be weakened to satisfy convenience or schedule constraints.

Amendments require a documented change, team review, and an explanation of how existing or in-progress work is affected. New rules SHOULD be written as enforceable, testable practices. Feature-specific plans MAY add stricter requirements but MUST NOT silently weaken this constitution.

**Version**: 1.0.0 | **Ratified**: 2026-11-22 | **Last Amended**: 2026-11-22