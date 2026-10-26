# Kinevor — Architecture

## Overview

Kinevor is a modular monorepo: a **shared platform kernel** and **ten vertical
feature modules**. The kernel owns cross-cutting contracts and building blocks;
each module owns one business capability end-to-end (domain → application → API →
web → mobile).

```
┌─────────────────────────────────────────────────────────────────┐
│ kinevor-platform (shared kernel)                                  │
│   Kinevor.Platform.Domain      entities, results, value objects   │
│   Kinevor.Platform.Application repository/UoW contracts, paging   │
│   Kinevor.Platform.Api         ApiResponse<T>, roles, policies    │
│   @softwaredeveloper98/kinevor-platform-web  Angular primitives   │
└─────────────────────────────────────────────────────────────────┘
        ▲                            ▲
        │ consumes                   │ consumes
┌───────┴───────┐            ┌───────┴───────┐
│ kinevor-*     │  ...       │ kinevor-*     │
│ vertical      │            │ vertical      │
│ module        │            │ module        │
└───────────────┘            └───────────────┘
```

## Platform kernel

`kinevor-platform` has no product UI of its own. It supplies:

- **NuGet** `Kinevor.Platform.Domain` — entities, aggregates, `Result<T>`/error
  types, value objects, enums and specifications.
- **NuGet** `Kinevor.Platform.Application` — repository and unit-of-work
  contracts, current user, branch context, paging and typed exceptions.
- **NuGet** `Kinevor.Platform.Api` — the `ApiResponse<T>` envelope, role
  constants and authorization-policy extensions.
- **npm** `@softwaredeveloper98/kinevor-platform-web` — API envelope, auth
  gateway/interceptor/guard, i18n, notices and theme services.

Backend projects target `net9.0`; the Angular library targets Angular 21.

## Module anatomy

Each module follows the same shape:

```text
kinevor-<module>/
  backend/
    src/Kinevor.Modules.<Module>/                domain, application, API
    src/Kinevor.Modules.<Module>.EntityFrameworkCore/   (where applicable)
    samples/Kinevor.Modules.<Module>.DemoApi/    runnable demo host
    tests/Kinevor.Modules.<Module>.Tests/
    Kinevor.Modules.<Module>.slnx
    nuget.config                                  local + GitHub Packages feeds
  web/
    projects/kinevor-<module>/                     Angular library
    projects/demo/                                 demo host
    scripts/                                       local install + screenshots
  mobile/
    packages/kinevor_<module>/                     Flutter package
    packages/kinevor_<module>/example/             runnable demo
  docs/
    screenshots/
```

A module is a **vertical slice**: it owns its domain rules, its HTTP API, its
Angular package and its Flutter package, and depends on the platform kernel only
for shared contracts — never on another module.

## Backend conventions

- **Clean Architecture** — `Domain` has zero dependencies; `Application` depends
  on `Domain`; infrastructure depends on `Domain` + `Application`; the API wires
  them together.
- **CQRS** — reads and writes go through MediatR commands and queries; behavior
  is composed with pipeline behaviors (logging, validation).
- **Errors** — expected failures use `Result<T>` or typed exceptions mapped to
  RFC 7807 problem-details with stable error codes; success payloads use the
  `ApiResponse<T>` envelope.
- **Ports** — repository, unit-of-work, encryption, audit, branch-context,
  token, payment and notification interfaces are defined in `Application` and
  implemented in infrastructure.
- **Idempotency & webhooks** — mutating endpoints accept an `Idempotency-Key`;
  webhooks are HMAC-verified, deduplicated per provider event id and replay-safe.

## Web conventions (Angular 21)

- **Zero NgModules** — `bootstrapApplication`, standalone components, `inject()`.
- **Signals** — `signal()`/`computed()` replace `BehaviorSubject`/`AsyncPipe`;
  `@if`/`@for` replace `*ngIf`/`*ngFor`.
- **Facades + gateways** — components call feature facades only; data access goes
  through `InjectionToken` gateways with HTTP implementations.
- **Functional guards/interceptors** — `CanActivateFn`, `HttpInterceptorFn`.

## Mobile conventions (Flutter)

- **BLoC + Clean Architecture** — per feature: `domain/` (entities, abstract
  repositories, use cases), `data/` (models, datasources, repository impls),
  `presentation/` (bloc, pages, widgets).
- **`Either<Failure, T>`** — errors as values through a sealed `Failure`
  hierarchy; a single `ApiClient` owns 401 → refresh → retry-once.
- **`get_it` composition root** — the only construction site (`core/di/injection.dart`).

## Package dependencies & publishing order

Modules reference the platform packages by version (`0.1.0`). Build order:

1. `kinevor-platform` — pack NuGet + build the Angular package (or publish).
2. Each module — restore `Kinevor.Platform.*` from the local feed
   (`kinevor-platform/backend/artifacts/packages`) or GitHub Packages, and install
   the Angular package locally (`npm run install:local`) or from the registry.

The root CI builds and tests the platform kernel. Module CI (build/test per
ecosystem) requires the platform packages to be published first; each module's
`PUBLISHING.md` documents the release checklist and consumer setup.
