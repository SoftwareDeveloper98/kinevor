import { Routes } from '@angular/router';
import { PublicLandingComponent } from './public-landing.component';
import { PublicPlansComponent } from './public-plans.component';
import { PublicScheduleComponent } from './public-schedule.component';
import { PublicSiteShellComponent } from './public-shell.component';
import { PublicTrainersComponent } from './public-trainers.component';
import { PublicTrialComponent } from './public-trial.component';

export const PUBLIC_SITE_ROUTES: Routes = [
  {
    path: '',
    component: PublicSiteShellComponent,
    children: [
      {
        path: '',
        component: PublicLandingComponent,
        title: 'Kinevor - Strength has a home',
      },
      {
        path: 'plans',
        component: PublicPlansComponent,
        title: 'Memberships - Kinevor',
      },
      {
        path: 'trainers',
        component: PublicTrainersComponent,
        title: 'Our coaches - Kinevor',
      },
      {
        path: 'schedule',
        component: PublicScheduleComponent,
        title: 'Class timetable - Kinevor',
      },
      {
        path: 'trial',
        component: PublicTrialComponent,
        title: 'Free trial - Kinevor',
      },
      {
        path: '**',
        redirectTo: '',
      },
    ],
  },
];
