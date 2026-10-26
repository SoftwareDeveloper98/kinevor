# Kinevor Trainers Web

Angular 21 library and demo for the Kinevor trainer directory.

## Packages

- `projects/kinevor-trainers` - publishable `@softwaredeveloper98/kinevor-trainers-web`
- `projects/demo` - standalone demo with staff, public and profile routes

The library depends on `@softwaredeveloper98/kinevor-platform-web` for the API
client primitives, platform services and Material styling contract.

## Install

Authenticate the GitHub Packages scope:

```text
@softwaredeveloper98:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken=${NODE_AUTH_TOKEN}
```

Then install both Kinevor packages:

```powershell
npm install @softwaredeveloper98/kinevor-platform-web @softwaredeveloper98/kinevor-trainers-web
```

## Use

```ts
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideKinevorPlatform } from '@softwaredeveloper98/kinevor-platform-web';
import { provideTrainers, TRAINERS_ROUTES } from '@softwaredeveloper98/kinevor-trainers-web';

export const appConfig = {
  providers: [
    provideHttpClient(),
    provideKinevorPlatform({ apiBaseUrl: '/api/v1' }),
    provideTrainers(),
    provideRouter(TRAINERS_ROUTES),
  ],
};
```

The package exports:

- `TrainersListComponent`, `TrainerDetailComponent`, `PublicTrainersComponent`
- `TrainersFacade`, `TrainersGateway`, `TRAINERS_GATEWAY`, `HttpTrainersGateway`
- `provideTrainers()`, `TRAINERS_ROUTES`, `PUBLIC_TRAINERS_ROUTES`
- `TrainerSummary` and the public API barrel

Override `TRAINERS_GATEWAY` in a host to provide a non-HTTP data source.

## Local development

The demo consumes the platform package. If it is not published, build the
sibling platform web workspace and install it locally:

```powershell
cd ..\..\kinevor-platform\web
npm install
npm run build
cd ..\..\kinevor-trainers\web
npm run install:local
```

`install:local` temporarily edits the dependency to a `file:` specifier,
installs, restores `package.json`, and removes `package-lock.json`.

```powershell
npm start
npm run build
npm run build:demo
npm test
```

The demo runs at `http://localhost:4200` and expects the trainers DemoApi at
`http://localhost:5081`. The demo interceptor adds `X-Demo-Role: Staff`.

## Screenshots

```powershell
npm run capture:screenshots
```

Set `KINEVOR_WEB_URL`, `KINEVOR_API_URL` and `PLAYWRIGHT_CHANNEL` to override the
defaults. The script writes to `../docs/screenshots`.
