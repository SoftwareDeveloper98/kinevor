import { Routes } from '@angular/router';
import { IDENTITY_ACCESS_ROUTES } from '@softwaredeveloper98/kinevor-identity-access-web';

/** Demo routes: login and authenticated session. */
export const appRoutes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '', children: IDENTITY_ACCESS_ROUTES },
  { path: '**', redirectTo: 'login' },
];
