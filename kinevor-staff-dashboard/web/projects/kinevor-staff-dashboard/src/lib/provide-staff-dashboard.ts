import { EnvironmentProviders, Provider, Type, makeEnvironmentProviders } from '@angular/core';
import { HttpStaffDashboardGateway } from './http-staff-dashboard.gateway';
import {
  STAFF_DASHBOARD_API_BASE_URL,
  STAFF_DASHBOARD_GATEWAY,
  StaffDashboardGateway,
} from './staff-dashboard.model';
import { StaffDashboardFacade } from './staff-dashboard.facade';

export interface StaffDashboardOptions {
  readonly apiBaseUrl?: string;
  readonly gateway?: Type<StaffDashboardGateway>;
}

export function provideStaffDashboard(
  options: StaffDashboardOptions = {},
): EnvironmentProviders {
  const providers: (Provider | EnvironmentProviders)[] = [
    { provide: STAFF_DASHBOARD_GATEWAY, useClass: options.gateway ?? HttpStaffDashboardGateway },
    StaffDashboardFacade,
  ];

  if (options.apiBaseUrl) {
    providers.push({ provide: STAFF_DASHBOARD_API_BASE_URL, useValue: options.apiBaseUrl });
  }

  return makeEnvironmentProviders(providers);
}
