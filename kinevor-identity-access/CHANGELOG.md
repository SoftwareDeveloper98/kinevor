# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-10-20

### Added

- Standalone identity backend with `UserAccount` and `RefreshToken` aggregates,
  MediatR login/refresh/logout/session/account handlers, PBKDF2 hashing, HS256
  JWT issuance, EF Core persistence, SQLite demo host and platform policies.
- Angular library with split-screen login, authenticated session view, typed HTTP
  gateway, facade, session guard, providers and routes.
- Angular demo application with seeded accounts, the Kinevor visual system, Sora
  typography and Material icons.
- Flutter package with API client, repository, login and session screens, theme
  and strings.
- Flutter example application for Windows and web.
- Real web, mobile and Swagger screenshots under `docs/screenshots`.
- NuGet packing, GitHub Actions CI and tag-based release workflows.
- MIT license and package documentation.
