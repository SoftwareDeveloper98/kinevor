# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-10-20

### Added

- Standalone notifications backend with the `Notification` aggregate,
  preferences, device tokens, templates and e-mail suppressions, MediatR
  commands and queries, EF Core persistence, SQLite demo host and platform
  authorization policies.
- Member inbox with category, channel and unread filters, unread counts, detail
  projections, single mark-read and mark-all-read.
- Notification preferences for four categories and three channels, plus
  push-device registration, re-assignment and removal.
- Staff ledger, compose, broadcast, scheduled dispatch, template and category
  catalogues and suppression management.
- Deterministic delivery simulation with scheduled sends and suppressed-address
  failures.
- Flutter package with API client, repository, notification inbox, detail and
  preferences screens, device management, theme and strings.
- Flutter example application targeting Maya Thompson's seeded inbox.
- Flutter widget tests, client tests and golden images.
- Mobile and Swagger screenshots under `docs/screenshots`.
- NuGet packing, GitHub Actions CI and documented tag-based release steps.
- MIT license and package documentation.
