# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-10-20

### Added

- Public-site .NET module with site content, branches, membership plans,
  trainers, class timetable, FAQs, articles, free trials and website enquiries.
- Anonymous public content API, trial and enquiry capture, and a staff/admin
  lead inbox secured by the platform `StaffOrAdmin` policy.
- EF Core persistence with SQLite support, JSON collection converters and
  audited timestamps.
- Runnable demo API with Swagger, health checks, CORS, an HTTP lead inbox demo
  role and deterministic content seeded relative to the current date.
- Domain tests and HTTP contract tests covering content, schedule filtering,
  trial validation, duplicate prevention and lead access.
- Angular 22 standalone component library with a typed HTTP gateway, shared
  signal facade, provider, route collection and five responsive screens:
  landing, memberships, coaches, timetable and free trial.
- Reference demo application with local Sora typography, a complete visual
  system and no remote image dependencies.
- Playwright screenshot capture with overflow checks for the landing page,
  memberships, timetable, trial form and Swagger UI.
- GitHub Actions CI and documented tag-based release steps for NuGet, npm and release
  screenshots.
