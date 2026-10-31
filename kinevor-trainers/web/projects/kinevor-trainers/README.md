# @softwaredeveloper98/kinevor-trainers-web

## Registry access (optional)

This package is built inside the monorepo. If you publish it to a private registry, configure the scope and token before installing:

```ini
@softwaredeveloper98:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken=${KINEVOR_PACKAGES_TOKEN}
```


Angular 22 trainer directory components for Kinevor.

## What is included

- Staff trainer list with search and paging.
- Trainer profile with contact, bio, specializations and certifications.
- Public directory built on the same read model.
- `TrainersFacade` for list/detail state and `HttpTrainersGateway` for the API.
- `provideTrainers()`, `TRAINERS_ROUTES` and `PUBLIC_TRAINERS_ROUTES`.

## Peer dependencies

- `@angular/common`, `@angular/core`, `@angular/forms`, `@angular/material`,
  `@angular/router` 21.x
- `@softwaredeveloper98/kinevor-platform-web` 0.1.x
- `rxjs` 7.8.x

## Setup

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

The host supplies authentication and localization through the platform package.
The API must expose the trainer read endpoints under the configured base URL.

## Replace the data source

```ts
providers: [
  provideTrainers(),
  { provide: TRAINERS_GATEWAY, useClass: MyTrainersGateway },
]
```

`TRAINERS_GATEWAY` is the seam for Storybook, SSR, caching or non-Kinevor hosts.

## License

MIT
