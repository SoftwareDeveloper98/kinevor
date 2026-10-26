import { HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { IdentityAccessFacade } from './identity-access.facade';

/** Redirects anonymous visitors to the login page. */
export const identitySessionGuard: CanActivateFn = () => {
  const facade = inject(IdentityAccessFacade);
  const router = inject(Router);
  return facade.isAuthenticated() ? true : router.createUrlTree(['/login']);
};

/** Exported for tests that assert status handling. */
export function isUnauthorized(error: unknown): boolean {
  return error instanceof HttpErrorResponse && error.status === 401;
}
