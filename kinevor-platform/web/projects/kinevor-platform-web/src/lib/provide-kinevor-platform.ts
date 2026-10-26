import { EnvironmentProviders, Provider } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { KINEVOR_API_BASE_URL } from './api/api-base-url';
import { AUTH_GATEWAY } from './auth/auth.gateway';
import { HttpAuthGateway } from './auth/http-auth.gateway';
import { authInterceptor } from './auth/auth.interceptor';

/** Options accepted by {@link provideKinevorPlatform}. */
export interface KinevorPlatformOptions {
  /** Base URL of the API, for example `http://localhost:5081/api/v1`. */
  apiBaseUrl?: string;
}

/**
 * Registers the shared platform services used by every Kinevor web module.
 *
 * @param options - API location overrides.
 * @returns Providers to add to the application bootstrap.
 */
export function provideKinevorPlatform(options: KinevorPlatformOptions = {}): Provider[] {
  return [
    { provide: KINEVOR_API_BASE_URL, useValue: options.apiBaseUrl ?? '/api/v1' },
    { provide: AUTH_GATEWAY, useClass: HttpAuthGateway },
  ];
}

/**
 * Configures `HttpClient` with the bearer-token and locale interceptor used by
 * every Kinevor web application.
 *
 * @returns Environment providers for the HTTP client.
 */
export function provideKinevorHttp(): EnvironmentProviders {
  return provideHttpClient(withInterceptors([authInterceptor]));
}
