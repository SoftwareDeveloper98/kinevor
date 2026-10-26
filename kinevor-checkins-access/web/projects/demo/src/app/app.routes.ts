import { Routes } from '@angular/router';
import { CHECKINS_ROUTES } from '@softwaredeveloper98/kinevor-checkins-access-web';

/** Demo routes for the check-ins workspace. */
export const appRoutes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'checkins' },
  ...CHECKINS_ROUTES,
  { path: '**', redirectTo: 'checkins' },
];
