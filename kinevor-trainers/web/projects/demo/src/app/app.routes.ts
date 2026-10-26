import { Routes } from '@angular/router';
import { PUBLIC_TRAINERS_ROUTES, TRAINERS_ROUTES } from '@softwaredeveloper98/kinevor-trainers-web';

/** Demo routes: staff directory and public directory. */
export const appRoutes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'trainers' },
  { path: 'trainers', children: TRAINERS_ROUTES },
  { path: 'public', children: PUBLIC_TRAINERS_ROUTES },
  { path: '**', redirectTo: 'trainers' },
];
