import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, EMPTY, finalize, shareReplay, timeout } from 'rxjs';
import { AuthSession, AuthUser } from './auth.model';
import { AUTH_GATEWAY } from './auth.gateway';
import { I18nService } from '../i18n/i18n.service';

const PROFILE_KEY = 'kinevor_profile';

/**
 * Session state for the signed-in user.
 *
 * Delegates all HTTP work to {@link AUTH_GATEWAY}; keeps access-token,
 * auto-refresh scheduling and the cached profile used after a reload.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly gateway = inject(AUTH_GATEWAY);
  private readonly router = inject(Router);
  private readonly i18n = inject(I18nService);

  private readonly _user = signal<AuthUser | null>(loadProfile());
  private readonly _accessToken = signal<string | null>(null);
  private refreshTimer: ReturnType<typeof setTimeout> | null = null;
  private refreshInFlight: Observable<unknown> | null = null;

  /** Currently signed-in user, or null when signed out. */
  readonly currentUser = this._user.asReadonly();
  /** Whether a user session is active. */
  readonly isAuthenticated = computed(() => !!this._user());
  /** Role names of the signed-in user. */
  readonly roles = computed(() => this._user()?.roles ?? []);

  /**
   * Signs in with email and password and adopts the returned session.
   *
   * @param email - Email address of the account.
   * @param password - Password of the account.
   * @returns The authenticated session.
   */
  login(email: string, password: string): Observable<AuthSession> {
    return this.gateway.login(email, password).pipe(tap(session => this.adopt(session)));
  }

  /**
   * Confirms an e-mail address with the token from the verification e-mail.
   *
   * @param email - Address that was registered.
   * @param token - Verification token from the e-mailed link.
   * @returns Completion of the verification request.
   */
  verifyEmail(email: string, token: string): Observable<void> {
    return this.gateway.verifyEmail(email, token);
  }

  /**
   * Requests a password-reset link; the server always answers the same way.
   *
   * @param email - Address of the account.
   * @returns Completion of the request.
   */
  forgotPassword(email: string): Observable<void> {
    return this.gateway.forgotPassword(email);
  }

  /**
   * Sets a new password with the single-use reset token.
   *
   * @param email - Address of the account.
   * @param token - Reset token from the e-mailed link.
   * @param newPassword - The new password.
   * @returns Completion of the reset request.
   */
  resetPassword(email: string, token: string, newPassword: string): Observable<void> {
    return this.gateway.resetPassword(email, token, newPassword);
  }

  /**
   * Restores the cookie-based session after an application reload.
   *
   * @returns The restored session, or completes empty when none exists.
   */
  restoreSession(): Observable<unknown> {
    return this.gateway.session().pipe(
      timeout(5000),
      tap(session => {
        this.adopt(session);
        if (!this._accessToken()) this.refreshToken().subscribe({ error: () => undefined });
      }),
      catchError(() => EMPTY),
    );
  }

  /**
   * Rotates the refresh token, sharing a single in-flight request.
   *
   * @returns The renewed session, or completes empty when signed out.
   */
  refreshToken(): Observable<unknown> {
    if (!this._user()) return EMPTY;
    if (!this.refreshInFlight) {
      this.refreshInFlight = this.gateway.refresh().pipe(
        tap(session => this.adopt(session)),
        catchError(() => {
          this.clearSession();
          return throwError(() => new Error('refresh failed'));
        }),
        finalize(() => { this.refreshInFlight = null; }),
        shareReplay({ bufferSize: 1, refCount: false }),
      );
    }
    return this.refreshInFlight;
  }

  /**
   * Revokes the session server-side, clears local state and returns to login.
   */
  logout(): void {
    this.gateway.logout().subscribe({ error: () => undefined });
    this.clearSession();
    this.router.navigate([this.i18n.localePath('/login')]);
  }

  /**
   * Returns the in-memory access token, if one was issued.
   *
   * @returns The bearer token, or null on cookie-based sessions.
   */
  getAccessToken(): string | null {
    return this._accessToken();
  }

  /**
   * Checks whether the signed-in user holds any of the given roles.
   *
   * @param roles - Role names to test against.
   * @returns True when at least one role matches.
   */
  hasAnyRole(roles: readonly string[]): boolean {
    const own = this.roles();
    return roles.some(role => own.includes(role));
  }

  private adopt(session: AuthSession): void {
    const user: AuthUser = session.user ?? {
      id: session.id ?? '',
      fullName: session.fullName ?? '',
      email: session.email ?? '',
      roles: session.roles ?? [],
    };
    this._accessToken.set(session.accessToken ?? null);
    this._user.set(user);
    storeProfile(user);
    this.scheduleRefresh(session.expiresAt);
  }

  private scheduleRefresh(expiresAt?: string): void {
    if (this.refreshTimer) clearTimeout(this.refreshTimer);
    this.refreshTimer = null;
    if (!expiresAt) return;
    const delay = new Date(expiresAt).getTime() - Date.now() - 60_000;
    this.refreshTimer = setTimeout(() => {
      this.refreshToken().subscribe({ error: () => undefined });
    }, Math.max(delay, 5_000));
  }

  private clearSession(): void {
    if (this.refreshTimer) clearTimeout(this.refreshTimer);
    this.refreshTimer = null;
    this.refreshInFlight = null;
    this._accessToken.set(null);
    this._user.set(null);
    try { sessionStorage.removeItem(PROFILE_KEY); } catch {}
  }
}

function storeProfile(user: AuthUser): void {
  try {
    sessionStorage.setItem(PROFILE_KEY, JSON.stringify(user));
  } catch {}
}

function loadProfile(): AuthUser | null {
  try {
    const raw = sessionStorage.getItem(PROFILE_KEY);
    if (!raw) return null;
    const user = JSON.parse(raw) as AuthUser;
    return user && user.id ? user : null;
  } catch { return null; }
}
