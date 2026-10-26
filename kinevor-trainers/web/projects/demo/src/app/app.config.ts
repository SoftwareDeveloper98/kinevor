import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { provideTrainers } from '@softwaredeveloper98/kinevor-trainers-web';
import { appRoutes } from './app.routes';
import { demoApiInterceptor } from './demo-api.interceptor';

/** Demo application configuration: router, HTTP and trainers feature. */
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideHttpClient(withInterceptors([demoApiInterceptor])),
    provideAnimationsAsync(),
    provideTrainers({ apiBaseUrl: 'http://localhost:5081/api/v1' }),
  ],
};
