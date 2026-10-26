import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { provideCheckInsAccess } from '@softwaredeveloper98/kinevor-checkins-access-web';
import { appRoutes } from './app.routes';
import { demoApiInterceptor } from './demo-api.interceptor';

/** Demo application configuration: router, demo identity and the check-ins feature. */
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([demoApiInterceptor])),
    provideCheckInsAccess('http://localhost:5086/api/v1'),
  ],
};
