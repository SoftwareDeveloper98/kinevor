# @softwaredeveloper98/kinevor-platform-web

Shared Angular 21 services for Kinevor feature modules: API response envelopes,
authentication, internationalization, notices and theme state.

## Install

Inside the monorepo, module workspaces bind to this build with
`npm run install:local`. To install from a registry instead, configure its scope:

```ini
@softwaredeveloper98:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken=${KINEVOR_PACKAGES_TOKEN}
```

```bash
npm install @softwaredeveloper98/kinevor-platform-web
```

## Provide the platform

```ts
import {
  provideKinevorHttp,
  provideKinevorPlatform,
} from '@softwaredeveloper98/kinevor-platform-web';

export const appConfig = {
  providers: [
    provideKinevorHttp(),
    provideKinevorPlatform({ apiBaseUrl: '/api/v1' }),
  ],
};
```

The package requires Angular 21, Angular Material 21 and RxJS 7.8 as peer
dependencies. Authentication storage and API hosts remain application concerns.
