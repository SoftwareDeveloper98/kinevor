import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthSession, IDENTITY_ACCESS_GATEWAY, IdentityAccessGateway } from './auth.model';
import { IdentityAccessFacade } from './identity-access.facade';

const session: AuthSession = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  expiresAt: new Date(Date.now() + 900_000).toISOString(),
  user: {
    id: 'u1',
    email: 'member@kinevor.demo',
    displayName: 'Youssef Member',
    role: 'Member',
    mfaEnabled: false,
    lastLoginAt: null,
  },
};

function setup() {
  const gateway: IdentityAccessGateway = {
    login: vi.fn(async () => session),
    session: vi.fn(async () => session.user),
    refresh: vi.fn(async () => ({ ...session, accessToken: 'rotated' })),
    logout: vi.fn(async () => undefined),
  };

  TestBed.configureTestingModule({
    providers: [IdentityAccessFacade, { provide: IDENTITY_ACCESS_GATEWAY, useValue: gateway }],
  });

  return { facade: TestBed.inject(IdentityAccessFacade), gateway };
}

describe('IdentityAccessFacade', () => {
  beforeEach(() => { try { globalThis.localStorage?.clear(); } catch { /* storage unavailable under Node */ } });

  it('stores a successful login and exposes the user', async () => {
    const { facade } = setup();

    expect(await facade.login('member@kinevor.demo', 'Kinevor!2026')).toBe(true);
    expect(facade.user()?.displayName).toBe('Youssef Member');
    expect(facade.isAuthenticated()).toBe(true);
  });

  it('reports invalid credentials without storing a session', async () => {
    const { facade, gateway } = setup();
    gateway.login = vi.fn(async () => {
      throw new Error('invalid');
    });

    expect(await facade.login('member@kinevor.demo', 'wrong')).toBe(false);
    expect(facade.user()).toBeNull();
    expect(facade.error()).toBe('The request failed. Try again.');
  });

  it('rotates the token pair', async () => {
    const { facade } = setup();
    await facade.login('member@kinevor.demo', 'Kinevor!2026');

    expect(await facade.rotate()).toBe(true);
    expect(facade.session()?.accessToken).toBe('rotated');
  });

  it('clears state and revokes on logout', async () => {
    const { facade, gateway } = setup();
    await facade.login('member@kinevor.demo', 'Kinevor!2026');

    await facade.logout();

    expect(gateway.logout).toHaveBeenCalledWith('refresh-token');
    expect(facade.user()).toBeNull();
  });
});
