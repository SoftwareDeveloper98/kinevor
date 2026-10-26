import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { provideIdentityAccess } from '@softwaredeveloper98/kinevor-identity-access-web';
import { appRoutes } from './app.routes';

/** Demo application configuration: router and identity feature. */
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideAnimationsAsync(),
    provideIdentityAccess({ apiBaseUrl: 'http://localhost:5082/api/v1' }),
  ],
};
