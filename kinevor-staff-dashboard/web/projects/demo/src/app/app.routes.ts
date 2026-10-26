import { Routes } from '@angular/router';
import { STAFF_DASHBOARD_ROUTES } from '@softwaredeveloper98/kinevor-staff-dashboard-web';

export const appRoutes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '', children: STAFF_DASHBOARD_ROUTES },
  { path: '**', redirectTo: 'dashboard' },
];
