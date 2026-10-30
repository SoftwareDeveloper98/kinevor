# Changelog

All notable changes to Kinevor are documented here. The repository uses a single
`0.x` line while the platform contracts and module packages evolve together.

## Unreleased

### Security

- Demo APIs now refuse to start outside the Development environment.
- Removed the committed JWT signing key. The identity demo generates an
  ephemeral key when `Jwt:SigningKey` is not configured.
- Login verifies unknown e-mails against a dummy hash, so response timing does
  not reveal which accounts exist.

### Added

- CodeQL analysis for C# and TypeScript.
- Dependabot updates for NuGet, npm, pub and GitHub Actions.
- Flutter format and `flutter analyze` checks in the mobile CI matrix.

## 0.1.0 — 2026-10-27

### Added

- Shared `kinevor-platform` kernel: domain primitives, application contracts,
  API response envelope and authorization policies.
- `@softwaredeveloper98/kinevor-platform-web` Angular library with API
  envelopes, authentication, i18n/RTL, notices and theming.
- Ten business services, each with its own .NET API, SQLite database, tests and
  demo host:
  - identity & access
  - members
  - memberships
  - classes & bookings
  - check-ins & access
  - payments & billing
  - notifications
  - trainers
  - staff dashboard
  - public site
- Nine Angular libraries with runnable demo applications.
- Nine Flutter packages with runnable examples.
- 57 application screenshots covering web, mobile and API surfaces.
- Monorepo CI: platform build/pack plus backend, web and mobile module matrices.
- Per-module READMEs, changelogs and publishing guides.
- Root architecture, security and project documentation.

### Verified

- 208 .NET tests, 99 Angular tests and 128 Flutter tests pass.
- Platform NuGet packing and Angular package build succeed from a clean checkout.
- Every module restores from the local platform feed and builds independently.
