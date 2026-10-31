import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideHttpClient, withFetch } from '@angular/common/http';
import {
  provideRouter,
  withInMemoryScrolling,
} from '@angular/router';
import {
  providePublicSite,
} from '@softwaredeveloper98/kinevor-public-site-web';
import { appRoutes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // View transitions are intentionally omitted: in Angular 22 they can stall
    // initial and client-side navigation, leaving the router outlet empty.
    provideRouter(
      appRoutes,
      withInMemoryScrolling({
        scrollPositionRestoration: 'top',
        anchorScrolling: 'enabled',
      }),
    ),
    provideHttpClient(withFetch()),
    providePublicSite({
      apiBaseUrl: 'http://localhost:5090/api/v1',
    }),
  ],
};
