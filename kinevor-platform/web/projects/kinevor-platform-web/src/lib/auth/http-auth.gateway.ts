import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, unwrapEnvelope, unwrapOk } from '../api/api-envelope';
import { KINEVOR_API_BASE_URL } from '../api/api-base-url';
import { AuthSession } from './auth.model';
import { AuthGateway } from './auth.gateway';

/** HTTP implementation of {@link AuthGateway} using the cookie-based auth endpoints. */
@Injectable()
export class HttpAuthGateway implements AuthGateway {
  private readonly http = inject(HttpClient);
  private readonly api = inject(KINEVOR_API_BASE_URL);

  /** {@inheritDoc AuthGateway.login} */
  login(email: string, password: string): Observable<AuthSession> {
    return this.http
      .post<ApiResponse<AuthSession>>(`${this.api}/auth/login`, { email, password }, { withCredentials: true })
      .pipe(map(unwrapEnvelope));
  }

  /** {@inheritDoc AuthGateway.session} */
  session(): Observable<AuthSession> {
    return this.http
      .get<ApiResponse<AuthSession>>(`${this.api}/auth/session`, { withCredentials: true })
      .pipe(map(unwrapEnvelope));
  }

  /** {@inheritDoc AuthGateway.verifyEmail} */
  verifyEmail(email: string, token: string): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.api}/auth/verify-email`, { email, token }, { withCredentials: true })
      .pipe(map(unwrapOk));
  }

  /** {@inheritDoc AuthGateway.forgotPassword} */
  forgotPassword(email: string): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.api}/auth/forgot-password`, { email }, { withCredentials: true })
      .pipe(map(unwrapOk));
  }

  /** {@inheritDoc AuthGateway.resetPassword} */
  resetPassword(email: string, token: string, newPassword: string): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.api}/auth/reset-password`, { email, token, newPassword }, { withCredentials: true })
      .pipe(map(unwrapOk));
  }

  /** {@inheritDoc AuthGateway.refresh} */
  refresh(): Observable<AuthSession> {
    return this.http
      .post<ApiResponse<AuthSession>>(`${this.api}/auth/refresh`, {}, { withCredentials: true })
      .pipe(map(unwrapEnvelope));
  }

  /** {@inheritDoc AuthGateway.logout} */
  logout(): Observable<void> {
    return this.http
      .post<ApiResponse<void>>(`${this.api}/auth/logout`, {}, { withCredentials: true })
      .pipe(map(() => undefined));
  }
}
