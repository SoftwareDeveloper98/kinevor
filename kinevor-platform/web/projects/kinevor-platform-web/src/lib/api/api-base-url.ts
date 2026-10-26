import { InjectionToken } from '@angular/core';

/** Base URL of the Kinevor API, without a trailing slash. */
export const KINEVOR_API_BASE_URL = new InjectionToken<string>('KINEVOR_API_BASE_URL', {
  factory: () => '/api/v1',
});
