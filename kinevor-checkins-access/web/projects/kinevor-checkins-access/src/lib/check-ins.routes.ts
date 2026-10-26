import { Routes } from '@angular/router';
import { FrontDeskComponent } from './front-desk/front-desk.component';
import { OccupancyDashboardComponent } from './occupancy-dashboard/occupancy-dashboard.component';
import { VisitHistoryComponent } from './visit-history/visit-history.component';

/** Front-desk, occupancy and visit-history routes. Mount at the application root. */
export const CHECKINS_ROUTES: Routes = [
  { path: 'checkins', component: FrontDeskComponent, title: 'Member check-in' },
  { path: 'checkins/occupancy', component: OccupancyDashboardComponent, title: 'Occupancy' },
  { path: 'checkins/history', component: VisitHistoryComponent, title: 'Visit history' },
];
