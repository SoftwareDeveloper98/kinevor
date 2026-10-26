import { Routes } from '@angular/router';
import { DashboardAlertsComponent } from './dashboard-alerts.component';
import { DashboardOverviewComponent } from './dashboard-overview.component';

export const STAFF_DASHBOARD_ROUTES: Routes = [
  { path: 'dashboard', component: DashboardOverviewComponent },
  { path: 'alerts', component: DashboardAlertsComponent },
];
