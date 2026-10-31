# @softwaredeveloper98/kinevor-checkins-access-web

Angular UI for the Kinevor Check-ins & Access module: the front-desk member
lookup and check-in workflow, live occupancy dashboard, visit history, and the
typed gateway/facades behind them.

## Registry access (optional)

This package is built inside the monorepo. If you publish it to a private registry, configure the scope and token before installing:

```ini
@softwaredeveloper98:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken=${KINEVOR_PACKAGES_TOKEN}
```

## Install

```bash
npm install @softwaredeveloper98/kinevor-checkins-access-web
```

The package expects Angular 22, Angular Material 22, RxJS 7.8 and the Kinevor
platform web package.

## Provide the feature

```ts
import { ApplicationConfig } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import {
  CHECKINS_ROUTES,
  provideCheckInsAccess,
} from '@softwaredeveloper98/kinevor-checkins-access-web';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideCheckInsAccess('http://localhost:5086/api/v1'),
    provideRouter([...CHECKINS_ROUTES]),
  ],
};
```

The host owns authentication. Every request must carry the API's bearer token
(or the standalone demo's `X-Demo-Role` / `X-Demo-User` headers).
`provideCheckInsAccess` registers `HttpCheckInsGateway`, `CheckInsFacade`,
`OccupancyFacade` and `VisitHistoryFacade`. Supply an alternative transport by
overriding the `CHECKINS_GATEWAY` injection token with a `CheckInsGateway`
implementation.

## Routes

| Route | Screen |
| --- | --- |
| `/checkins` | Member search, eligibility and front-desk check-in/out |
| `/checkins/occupancy` | Live capacity, daily traffic and open visits |
| `/checkins/history` | Searchable visit history with live status |

Import `CHECKINS_ROUTES` into the host router or mount the standalone
`FrontDeskComponent`, `OccupancyDashboardComponent` and `VisitHistoryComponent`
directly.

## Data behaviour

- Eligibility is evaluated by the API for the selected branch; the client never
  grants access locally.
- A member with an open visit is shown as inside and can be checked out rather
  than checked in again.
- Occupancy and open-visit lists refresh after every visit mutation.
- History supports member search, branch, open/completed status and pagination.
- Denied check-ins surface the API's reason message and remain visible until
  dismissed.

## Development

```bash
npm run install:local   # link the local platform web package
npm test                # gateway and facade unit tests
npm run build:all       # build the library and demo
npm start               # demo on http://localhost:4205
```

The demo host lives in `projects/demo` and expects the Check-ins API on
`http://localhost:5086`.

## License

MIT
