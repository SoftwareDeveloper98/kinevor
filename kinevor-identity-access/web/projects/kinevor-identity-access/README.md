# @softwaredeveloper98/kinevor-identity-access-web

## GitHub Packages access

This package is private. Configure the scope and token before installing:

```ini
@softwaredeveloper98:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken=${KINEVOR_PACKAGES_TOKEN}
```


Angular 21 identity and access components for Kinevor.

## What is included

- Split-screen login with seeded-account shortcuts and inline errors.
- Authenticated session view with account facts, token metadata, rotation and
  sign-out.
- `IdentityAccessFacade` for session state and `HttpIdentityAccessGateway` for
  the API.
- `identitySessionGuard`, `provideIdentityAccess()` and `IDENTITY_ACCESS_ROUTES`.
- Best-effort session persistence with `AUTH_SESSION_STORAGE_KEY`.

## Peer dependencies

- `@angular/common`, `@angular/core`, `@angular/forms`, `@angular/material`,
  `@angular/router` 21.x
- `@softwaredeveloper98/kinevor-platform-web` 0.1.x
- `rxjs` 7.8.x

## Setup

```ts
import { provideRouter } from '@angular/router';
import { provideKinevorPlatform } from '@softwaredeveloper98/kinevor-platform-web';
import {
  IDENTITY_ACCESS_ROUTES,
  provideIdentityAccess,
} from '@softwaredeveloper98/kinevor-identity-access-web';

export const appConfig = {
  providers: [
    provideKinevorPlatform({ apiBaseUrl: '/api/v1' }),
    provideIdentityAccess({ apiBaseUrl: '/api/v1' }),
    provideRouter(IDENTITY_ACCESS_ROUTES),
  ],
};
```

The host supplies the visual system, localization and any additional session
storage policy through the platform package. The API must expose the auth
endpoints described in the repository README.

## Replace the data source

```ts
import {
  IDENTITY_ACCESS_GATEWAY,
  provideIdentityAccess,
} from '@softwaredeveloper98/kinevor-identity-access-web';

providers: [
  provideIdentityAccess({ apiBaseUrl: '/api/v1' }),
  { provide: IDENTITY_ACCESS_GATEWAY, useClass: MyIdentityGateway },
]
```

`IDENTITY_ACCESS_GATEWAY` is the seam for Storybook, SSR, token brokers or
non-Kinevor hosts. `IDENTITY_API_BASE_URL` configures the default HTTP gateway.

## Routes

`IDENTITY_ACCESS_ROUTES` registers `login` and `session` paths. The session route
is protected by `identitySessionGuard`, which verifies the stored access token
before activation.

## License

MIT
