import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { CHECKINS_API_BASE_URL, CHECKINS_GATEWAY } from './check-in.model';
import { CheckInsFacade } from './check-ins.facade';
import { HttpCheckInsGateway } from './http-check-ins.gateway';
import { OccupancyFacade } from './occupancy.facade';
import { VisitHistoryFacade } from './visit-history.facade';

/** Provides the check-ins gateway and facades for a host application. */
export function provideCheckInsAccess(apiBaseUrl: string): EnvironmentProviders {
  return makeEnvironmentProviders([
    { provide: CHECKINS_API_BASE_URL, useValue: apiBaseUrl },
    { provide: CHECKINS_GATEWAY, useClass: HttpCheckInsGateway },
    CheckInsFacade,
    OccupancyFacade,
    VisitHistoryFacade,
  ]);
}
