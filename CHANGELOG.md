# Changelog

All notable changes to Kinevor are documented here. The repository uses a single
`0.x` line while the platform contracts and module packages evolve together.

## 0.1.0 — 2026-10-30

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
- CodeQL analysis for C# and TypeScript.
- Dependabot updates for NuGet, npm, pub and GitHub Actions.
- Flutter format and `flutter analyze` checks in the mobile CI matrix.
- Per-module READMEs, changelogs and publishing guides.
- Root architecture, security, contributing and code-of-conduct documentation.
- GitHub issue templates for bugs and feature requests.
- Kinevor Gym brand mark on the README, public site and demo hosts.

### Changed

- Backend projects target `net10.0` with EF Core 10 and ASP.NET Core 10 packages.
- Angular workspaces and libraries target Angular 22, Angular Material 22 and
  TypeScript 6.
- Demo apps use a 256 px logo asset; the full-size brand image remains in
  `docs/branding/` for the README and social preview.
- Removed obsolete `RollForward=LatestMajor` settings from demo and test hosts
  now that every backend targets `net10.0`.
- Removed the deprecated TypeScript `baseUrl` option; workspace path mappings
  are now relative and no longer rely on a suppression.

### Security

- Demo APIs refuse to start outside the Development environment.
- Removed the committed JWT signing key. The identity demo generates an
  ephemeral key when `Jwt:SigningKey` is not configured.
- Login verifies unknown e-mails against a dummy hash, so response timing does
  not reveal which accounts exist.
- Pinned SQLitePCLRaw 2.1.14 (SQLite 3.54.0) to clear the high-severity
  advisory GHSA-2m69-gcr7-jv3q in the SQLite-backed demo hosts and tests.

### Verified

- 208 .NET tests, 99 Angular tests and 128 Flutter tests pass.
- Platform NuGet packing and Angular package build succeed from a clean checkout.
- Every module restores from the local platform feed and builds independently.
