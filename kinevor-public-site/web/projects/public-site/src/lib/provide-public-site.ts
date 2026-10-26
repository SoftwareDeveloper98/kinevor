import {
  EnvironmentProviders,
  makeEnvironmentProviders,
} from '@angular/core';
import { HttpPublicSiteGateway } from './http-public-site.gateway';
import {
  PUBLIC_SITE_API_BASE_URL,
  PUBLIC_SITE_GATEWAY,
  PublicSiteOptions,
} from './public-site.models';

export function providePublicSite(options: PublicSiteOptions): EnvironmentProviders {
  return makeEnvironmentProviders([
    { provide: PUBLIC_SITE_API_BASE_URL, useValue: options.apiBaseUrl },
    { provide: PUBLIC_SITE_GATEWAY, useClass: HttpPublicSiteGateway },
  ]);
}
