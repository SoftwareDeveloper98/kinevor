import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { AuthSession, IDENTITY_API_BASE_URL } from './auth.model';
import { HttpIdentityAccessGateway } from './http-identity-access.gateway';

const session: AuthSession = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  expiresAt: '2026-10-20T12:15:00Z',
  user: {
    id: 'u1',
    email: 'admin@kinevor.demo',
    displayName: 'Nour Admin',
    role: 'Admin',
    mfaEnabled: false,
    lastLoginAt: '2026-10-20T12:00:00Z',
  },
};

describe('HttpIdentityAccessGateway', () => {
  let gateway: HttpIdentityAccessGateway;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        HttpIdentityAccessGateway,
        { provide: IDENTITY_API_BASE_URL, useValue: 'http://api.test/api/v1/' },
      ],
    });
    gateway = TestBed.inject(HttpIdentityAccessGateway);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('unwraps the login envelope', async () => {
    const promise = gateway.login({ email: 'admin@kinevor.demo', password: 'Kinevor!2026' });

    const request = http.expectOne('http://api.test/api/v1/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'admin@kinevor.demo',
      password: 'Kinevor!2026',
    });
    request.flush({ success: true, data: session, message: 'Signed in.' });

    const result = await promise;
    expect(result.accessToken).toBe('access-token');
    expect(result.user.role).toBe('Admin');
  });

  it('sends the bearer token on session requests', async () => {
    const promise = gateway.session('access-token');

    const request = http.expectOne('http://api.test/api/v1/auth/session');
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-token');
    request.flush({ success: true, data: session.user });

    const account = await promise;
    expect(account.displayName).toBe('Nour Admin');
  });

  it('rotates a refresh token', async () => {
    const promise = gateway.refresh('refresh-token');

    const request = http.expectOne('http://api.test/api/v1/auth/refresh');
    expect(request.request.body).toEqual({ refreshToken: 'refresh-token' });
    request.flush({ success: true, data: { ...session, accessToken: 'new-access' } });

    const result = await promise;
    expect(result.accessToken).toBe('new-access');
  });
});
