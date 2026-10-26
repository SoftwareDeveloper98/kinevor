import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthSession } from './auth.model';

/**
 * Authentication use cases the session layer depends on.
 *
 * Implementations own all auth HTTP details, including the refresh cookie.
 */
export interface AuthGateway {
  /**
   * Exchanges credentials for a session.
   *
   * @param email - Email address of the account.
   * @param password - Password of the account.
   * @returns The authenticated session.
   */
  login(email: string, password: string): Observable<AuthSession>;

  /**
   * Restores the cookie-based session of the current browser.
   *
   * @returns The restored session.
   */
  session(): Observable<AuthSession>;

  /**
   * Confirms an e-mail address with the token from the verification e-mail.
   *
   * @param email - Address that was registered.
   * @param token - Verification token from the e-mailed link.
   * @returns Completion of the verification request.
   */
  verifyEmail(email: string, token: string): Observable<void>;

  /**
   * Requests a password-reset link; the server always answers the same way.
   *
   * @param email - Address of the account.
   * @returns Completion of the request.
   */
  forgotPassword(email: string): Observable<void>;

  /**
   * Sets a new password with the single-use reset token.
   *
   * @param email - Address of the account.
   * @param token - Reset token from the e-mailed link.
   * @param newPassword - The new password.
   * @returns Completion of the reset request.
   */
  resetPassword(email: string, token: string, newPassword: string): Observable<void>;

  /**
   * Rotates the refresh token and returns a fresh session.
   *
   * @returns The renewed session.
   */
  refresh(): Observable<AuthSession>;

  /**
   * Revokes the refresh token on the server.
   *
   * @returns Completion of the revocation request.
   */
  logout(): Observable<void>;
}

/** Injection token resolving to the auth data access implementation. */
export const AUTH_GATEWAY = new InjectionToken<AuthGateway>('AuthGateway');
