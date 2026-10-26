# @softwaredeveloper98/kinevor-staff-dashboard-web

Standalone Angular 21 library for the Kinevor staff dashboard. It ships the
executive overview and alert inbox, a typed HTTP gateway, a signal-based facade,
an injectable provider and ready-made routes.

## GitHub Packages access

This package is private. Configure the scope and token before installing:

```ini
@softwaredeveloper98:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken=${KINEVOR_PACKAGES_TOKEN}
```

## Install

```powershell
npm install @softwaredeveloper98/kinevor-staff-dashboard-web
```

The package expects `@angular/common`, `@angular/core`, `@angular/material`,
`@angular/router`, `rxjs` and `@softwaredeveloper98/kinevor-platform-web` as peer
dependencies.

## Register

```ts
import { provideHttpClient } from '@angular/common/http';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import {
  provideStaffDashboard,
  STAFF_DASHBOARD_ROUTES,
} from '@softwaredeveloper98/kinevor-staff-dashboard-web';

import { AppComponent } from './app/app.component';

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(),
    provideStaffDashboard({ apiBaseUrl: 'http://localhost:5089/api/v1' }),
    provideRouter([{ path: '', children: STAFF_DASHBOARD_ROUTES }]),
  ],
});
```

`provideStaffDashboard` registers `HttpStaffDashboardGateway` and
`StaffDashboardFacade`. Pass a custom `gateway` class when the host needs a
different transport:

```ts
provideStaffDashboard({
  apiBaseUrl: '/api/v1',
  gateway: MyStaffDashboardGateway,
});
```

When `apiBaseUrl` is omitted, the host must provide the
`STAFF_DASHBOARD_API_BASE_URL` token itself.

## Routes

| Path | Component | Purpose |
| --- | --- | --- |
| `/dashboard` | `DashboardOverviewComponent` | KPIs, 7/30-day trend, branch pulse, activity and top alerts |
| `/alerts` | `DashboardAlertsComponent` | Severity-filtered operational alert inbox |

The routes are exported as `STAFF_DASHBOARD_ROUTES`; mount them under the host's
authenticated staff shell.

## API contract

The library calls the platform `ApiResponse<T>` envelope and expects:

| Method | Route | Result |
| --- | --- | --- |
| `GET` | `/staff-dashboard?branchId=&days=7` | Summary, trend, activity, top alerts and branch options |
| `GET` | `/staff-dashboard/alerts?branchId=` | Complete alert inbox and branch options |
| `GET` | `/staff-dashboard/branches` | Branch selector options |

`days` accepts `7` or `30`. Omitting `branchId` requests the all-branch
aggregate. The host is responsible for authentication, authorization and branch
claims.

The gateway can be replaced through the `StaffDashboardGateway` interface.
Models and tokens are exported from the package root.

## Styling

The components use the Kinevor visual tokens (`--color-*`, `--radius-*`,
`--gradient-*`) and the platform `page-container` helper. Load the Kinevor
platform styles and Sora font in the host, or copy the token setup from the demo
application in this repository. The demo is the reference integration.

## Development

From the workspace root:

```powershell
cd web
npm run install:local
npm test
npm run build:all
npm start
```

`npm run install:local` points the workspace at the sibling platform build at
`../../kinevor-platform/web/dist/kinevor-platform-web`, installs dependencies and
then restores `package.json`.

The demo host runs at `http://localhost:4207` and expects the API at
`http://localhost:5089/api/v1`.
