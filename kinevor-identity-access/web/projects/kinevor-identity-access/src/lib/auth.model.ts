import { InjectionToken } from '@angular/core';

/** Account returned by the identity API. */
export interface UserAccount {
  readonly id: string;
  readonly email: string;
  readonly displayName: string;
  readonly role: 'Admin' | 'Staff' | 'Trainer' | 'Member';
  readonly mfaEnabled: boolean;
  readonly lastLoginAt: string | null;
}

/** Access token, rotating refresh token and the signed-in account. */
export interface AuthSession {
  readonly accessToken: string;
  readonly refreshToken: string;
  readonly expiresAt: string;
  readonly user: UserAccount;
}

/** Credentials submitted by the login form. */
export interface LoginRequest {
  readonly email: string;
  readonly password: string;
}

/** API envelope used by every Kinevor endpoint. */
export interface ApiEnvelope<T> {
  readonly success: boolean;
  readonly data: T;
  readonly message?: string;
  readonly errors?: string[];
}

/** Injection token resolving to the identity gateway implementation. */
export const IDENTITY_ACCESS_GATEWAY = new InjectionToken<IdentityAccessGateway>('IdentityAccessGateway');

/** Base URL of the identity API, for example `http://localhost:5082/api/v1`. */
export const IDENTITY_API_BASE_URL = new InjectionToken<string>('IDENTITY_API_BASE_URL');

/** HTTP contract consumed by the facade. */
export interface IdentityAccessGateway {
  /** Exchanges credentials for a token pair. */
  login(request: LoginRequest): Promise<AuthSession>;
  /** Returns the account for an access token. */
  session(accessToken: string): Promise<UserAccount>;
  /** Rotates a refresh token. */
  refresh(refreshToken: string): Promise<AuthSession>;
  /** Revokes a refresh token. */
  logout(refreshToken: string): Promise<void>;
}

/** Storage key used by the demo facade. */
export const AUTH_SESSION_STORAGE_KEY = 'kinevor.identity.session';
