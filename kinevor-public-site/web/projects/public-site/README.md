# @softwaredeveloper98/kinevor-public-site-web

Angular 21 public website for Kinevor gyms. The library ships a typed gateway,
facade, provider, routes and five standalone screens: landing, plans, trainers,
timetable and free-trial capture.

## GitHub Packages access

This package is private. Configure the scope and token before installing:

```ini
@softwaredeveloper98:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken=${KINEVOR_PACKAGES_TOKEN}
```

## Install

```powershell
npm install @softwaredeveloper98/kinevor-public-site-web
```

The host also needs `@angular/router`, `@angular/forms` and `provideHttpClient()`.

## Wire up

```ts
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import {
  PUBLIC_SITE_ROUTES,
  providePublicSite,
} from '@softwaredeveloper98/kinevor-public-site-web';

export const appConfig = {
  providers: [
    provideHttpClient(),
    provideRouter(PUBLIC_SITE_ROUTES),
    providePublicSite({ apiBaseUrl: 'https://api.example.com/api/v1' }),
  ],
};
```

`PUBLIC_SITE_ROUTES` renders the complete branded shell at `/`, with `/plans`,
`/trainers`, `/schedule` and `/trial`.

## Override the gateway

Provide your own implementation of `PublicSiteGateway` under the
`PUBLIC_SITE_GATEWAY` token. The default `HttpPublicSiteGateway` unwraps the
Kinevor `ApiResponse<T>` envelope and targets the public-site endpoints.

## Design

The components are standalone, use signals and OnPush change detection, and
depend on CSS custom properties with safe fallbacks. The demo application adds
the Kinevor Sora typeface. No remote images are required; the visual system is
built from local CSS gradients, grids and typographic compositions.
