# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Flutter `kinevor_staff_dashboard` package with the branch-scoped staff
  dashboard, 7/30-day trend, KPI grid, activity feed and alert inbox.
- Typed staff dashboard HTTP client, repository abstraction and configurable
  strings plus the Kinevor dark theme and Sora typography.
- Flutter example host for the staff DemoApi with development-only demo headers.
- Flutter client, screen-state, interaction and 390x844 golden coverage.
- Mobile dashboard and alert inbox screenshots in `docs/screenshots`.
- Windows and web runners for the Flutter example.

## [0.1.0] - 2026-10-20

### Added

- Standalone staff dashboard backend with branch snapshots, KPI aggregation,
  7/30-day trends, activity feeds, operational alerts, EF Core persistence,
  SQLite demo host and platform authorization policies.
- Dashboard and alert inbox endpoints with all-branch and single-branch views,
  severity ordering and branch selector options.
- Angular library with standalone overview and alerts components, typed HTTP
  gateway, signal-based facade, injectable provider and routes.
- Angular demo application with the Kinevor visual system, Sora typography,
  Material icons, demo role interceptor and branch/range controls.
- Vitest coverage for the HTTP gateway and dashboard facade.
- Playwright screenshot automation for the dashboard, alert inbox and Swagger UI.
- Real web and API screenshots under `docs/screenshots`.
- NuGet packing, GitHub Actions CI and tag-based release workflows.
- MIT license and package documentation.
