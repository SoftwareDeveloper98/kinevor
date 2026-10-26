import { Routes } from '@angular/router';
import { LoginComponent } from './login.component';
import { SessionComponent } from './session.component';
import { identitySessionGuard } from './identity-session.guard';

/** Library routes: login and authenticated session views. */
export const IDENTITY_ACCESS_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginComponent },
  { path: 'session', component: SessionComponent, canActivate: [identitySessionGuard] },
];
