import { EnvironmentProviders, Provider, makeEnvironmentProviders } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { IDENTITY_ACCESS_GATEWAY, IDENTITY_API_BASE_URL } from './auth.model';
import { HttpIdentityAccessGateway } from './http-identity-access.gateway';
import { IdentityAccessFacade } from './identity-access.facade';

/** Configuration accepted by {@link provideIdentityAccess}. */
export interface IdentityAccessOptions {
  /** Base URL of the identity API, including `/api/v1`. */
  readonly apiBaseUrl: string;
}

/** Registers HTTP, the identity gateway and the facade. */
export function provideIdentityAccess(options: IdentityAccessOptions): EnvironmentProviders {
  const providers: (Provider | EnvironmentProviders)[] = [
    provideHttpClient(),
    { provide: IDENTITY_API_BASE_URL, useValue: options.apiBaseUrl },
    { provide: IDENTITY_ACCESS_GATEWAY, useClass: HttpIdentityAccessGateway },
    IdentityAccessFacade,
  ];

  return makeEnvironmentProviders(providers);
}
