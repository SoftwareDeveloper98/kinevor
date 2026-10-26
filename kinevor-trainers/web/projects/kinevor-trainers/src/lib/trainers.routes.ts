import { Routes } from '@angular/router';

/** Staff trainer routes, mounted by the host under a path such as `/trainers`. */
export const TRAINERS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./trainers-list.component').then(m => m.TrainersListComponent),
  },
  {
    path: ':id',
    loadComponent: () => import('./trainer-detail.component').then(m => m.TrainerDetailComponent),
  },
];

/** Public trainer directory route, mounted by the host under a public path. */
export const PUBLIC_TRAINERS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./public-trainers.component').then(m => m.PublicTrainersComponent),
  },
];
