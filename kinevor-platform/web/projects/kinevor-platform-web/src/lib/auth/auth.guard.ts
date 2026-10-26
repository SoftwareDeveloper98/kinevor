import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { I18nService } from '../i18n/i18n.service';

/**
 * Route guard allowing only authenticated users; redirects to the localized
 * login page otherwise.
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.isAuthenticated() ? true : inject(Router).createUrlTree([inject(I18nService).localePath('/login')]);
};

/**
 * Route guard requiring any of the roles listed in `route.data.roles`.
 * Unauthenticated users go to login; authenticated users without a required
 * role are sent to `route.data.redirect` or home.
 */
export const roleGuard: CanActivateFn = route => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const i18n = inject(I18nService);
  const required = route.data?.['roles'] as string[] | undefined;
  if (!auth.isAuthenticated()) return router.createUrlTree([i18n.localePath('/login')]);
  if (!required || required.length === 0) return true;
  if (auth.hasAnyRole(required)) return true;
  const redirect = typeof route.data?.['redirect'] === 'string' ? route.data['redirect'] : '/';
  return router.createUrlTree([i18n.localePath(redirect)]);
};
