import { DatePipe, NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { IdentityAccessFacade } from './identity-access.facade';

/** Authenticated account view with token metadata and session actions. */
@Component({
  selector: 'kinevor-session',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    NgClass,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <span class="eyebrow">Authenticated session</span>
          <h1 class="page-title">Your Kinevor account</h1>
        </div>
        <span class="status-chip status-active"><span class="live-dot" aria-hidden="true"></span> Active</span>
      </div>

      @if (facade.loading() && !facade.user()) {
        <div class="table-loading" role="status"><mat-spinner diameter="48" /></div>
      }

      @if (facade.user(); as user) {
        <div class="session-grid">
          <mat-card class="gym-card profile-card">
            <mat-card-content>
              <div class="profile-head">
                <div class="avatar" aria-hidden="true">{{ initials(user.displayName) }}</div>
                <div class="profile-copy">
                  <h2>{{ user.displayName }}</h2>
                  <p>{{ user.email }}</p>
                </div>
                <span class="role-chip" [ngClass]="'role-' + user.role.toLowerCase()">{{ user.role }}</span>
              </div>

              <mat-divider />

              <dl class="profile-facts">
                <div><dt>Account id</dt><dd class="mono">{{ user.id }}</dd></div>
                <div><dt>Last sign-in</dt><dd>{{ user.lastLoginAt ? (user.lastLoginAt | date: 'medium') : 'First session' }}</dd></div>
                <div><dt>Second factor</dt><dd>{{ user.mfaEnabled ? 'Enrolled' : 'Not enrolled' }}</dd></div>
                <div><dt>Policy</dt><dd>{{ policyFor(user.role) }}</dd></div>
              </dl>

              <div class="profile-actions">
                <button mat-flat-button color="primary" (click)="rotate()" [disabled]="facade.loading()">
                  <mat-icon aria-hidden="true">autorenew</mat-icon>
                  Rotate session
                </button>
                <button mat-stroked-button type="button" (click)="logout()">
                  <mat-icon aria-hidden="true">logout</mat-icon>
                  Sign out
                </button>
              </div>
            </mat-card-content>
          </mat-card>

          <div class="side-stack">
            <mat-card class="gym-card token-card">
              <mat-card-content>
                <span class="eyebrow">Token pair</span>
                <h3>Short access, long refresh</h3>
                <div class="token-row">
                  <span class="token-label">Access expires</span>
                  <strong>{{ facade.session()?.expiresAt | date: 'mediumTime' }}</strong>
                </div>
                <div class="token-row">
                  <span class="token-label">Refresh lifetime</span>
                  <strong>30 days</strong>
                </div>
                <div class="token-preview">
                  <span>Bearer</span>
                  <code>{{ tokenPreview() }}</code>
                </div>
                <p class="muted-note">The access token is sent as an <code>Authorization: Bearer</code> header. Refresh tokens are single use and rotate on every exchange.</p>
              </mat-card-content>
            </mat-card>

            <mat-card class="gym-card guard-card">
              <mat-card-content>
                <div class="guard-head">
                  <span class="kx-icon-badge" aria-hidden="true"><mat-icon>shield</mat-icon></span>
                  <div>
                    <h3>Role-aware by default</h3>
                    <p>Every endpoint is protected by the shared Kinevor policies.</p>
                  </div>
                </div>
                <div class="guard-chips">
                  @for (role of roles; track role) {
                    <span class="chip" [class.chip-outline]="role !== user.role">{{ role }}</span>
                  }
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </div>

        @if (facade.error(); as error) {
          <p class="notice notice-error" role="alert">{{ error }}</p>
        }
      }
    </div>
  `,
  styles: [`
    .page-header > div { display:flex; flex-direction:column; gap:6px; }
    .eyebrow { font-size:11px; font-weight:700; letter-spacing:.14em; text-transform:uppercase; color:var(--color-accent-text); }
    .live-dot { width:7px; height:7px; border-radius:999px; background:currentColor; animation:live 2s ease-out infinite; }
    @keyframes live { 0%,100% { box-shadow:0 0 0 0 color-mix(in srgb, currentColor 55%, transparent); } 50% { box-shadow:0 0 0 5px transparent; } }
    .session-grid { display:grid; grid-template-columns:1fr; gap:16px; align-items:start; }
    .profile-card mat-card-content { padding:28px !important; }
    .profile-head { display:flex; align-items:center; gap:16px; flex-wrap:wrap; margin-bottom:20px; }
    .avatar { width:68px; height:68px; border-radius:20px; background:var(--gradient-brandWarm); color:var(--color-on-accent); display:flex; align-items:center; justify-content:center; font-size:24px; font-weight:800; letter-spacing:-.02em; flex:none; }
    .profile-copy { flex:1; min-width:180px; }
    .profile-copy h2 { margin:0; font-size:24px; letter-spacing:-.03em; color:var(--color-text); }
    .profile-copy p { margin:3px 0 0; color:var(--color-text-muted); font-size:14px; }
    .role-chip { padding:6px 14px; border-radius:999px; font-size:12px; font-weight:700; text-transform:uppercase; letter-spacing:.08em; background:var(--color-accent-soft); color:var(--color-accent-text); }
    .role-admin { background:var(--color-accent-soft); color:var(--color-accent-text); }
    .role-staff { background:color-mix(in srgb, var(--color-info) 15%, transparent); color:var(--color-info); }
    .role-trainer { background:color-mix(in srgb, var(--color-warning) 15%, transparent); color:var(--color-warning); }
    .role-member { background:color-mix(in srgb, var(--color-success) 15%, transparent); color:var(--color-success); }
    .profile-facts { display:grid; grid-template-columns:1fr; gap:12px; margin:20px 0; }
    .profile-facts div { display:flex; flex-direction:column; gap:3px; }
    .profile-facts dt { font-size:11px; font-weight:600; letter-spacing:.08em; text-transform:uppercase; color:var(--color-text-muted); }
    .profile-facts dd { margin:0; font-size:14px; color:var(--color-text); overflow-wrap:anywhere; }
    .mono { font-family:ui-monospace, SFMono-Regular, Consolas, monospace; font-size:12px; color:var(--color-text-muted); }
    .profile-actions { display:flex; gap:10px; flex-wrap:wrap; padding-top:4px; }
    .side-stack { display:flex; flex-direction:column; gap:16px; }
    .token-card mat-card-content, .guard-card mat-card-content { padding:24px !important; }
    h3 { margin:8px 0 14px; font-size:18px; letter-spacing:-.02em; color:var(--color-text); }
    .token-row { display:flex; justify-content:space-between; gap:12px; padding:9px 0; border-bottom:1px solid var(--color-border); font-size:13px; }
    .token-row:last-of-type { border-bottom:none; }
    .token-label { color:var(--color-text-muted); }
    .token-preview { margin-top:12px; padding:12px; border-radius:10px; background:var(--color-surface); display:flex; flex-direction:column; gap:5px; }
    .token-preview span { font-size:10px; font-weight:700; letter-spacing:.12em; text-transform:uppercase; color:var(--color-accent-text); }
    .token-preview code { font-family:ui-monospace, SFMono-Regular, Consolas, monospace; font-size:12px; color:var(--color-text); overflow-wrap:anywhere; }
    .muted-note { margin:12px 0 0; }
    .guard-head { display:flex; gap:12px; align-items:flex-start; margin-bottom:16px; }
    .guard-head h3 { margin:0 0 3px; }
    .guard-head p { margin:0; font-size:13px; color:var(--color-text-muted); }
    .guard-chips { display:flex; gap:8px; flex-wrap:wrap; }
    @media (min-width: 900px) { .session-grid { grid-template-columns:1.2fr .8fr; } .profile-facts { grid-template-columns:1fr 1fr; } }
  `],
})
export class SessionComponent implements OnInit {
  /** Facade that owns the session state. */
  readonly facade = inject(IdentityAccessFacade);
  private readonly router = inject(Router);

  /** Roles rendered in the guard card. */
  readonly roles = ['Admin', 'Staff', 'Trainer', 'Member'];

  /** Refreshes account data on entry and redirects if the session is gone. */
  ngOnInit(): void {
    void this.load();
  }

  /**
   * Returns the initials of a display name.
   *
   * @param name - Display name.
   * @returns Up to two uppercase initials.
   */
  initials(name: string): string {
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part.charAt(0).toUpperCase())
      .join('');
  }

  /**
   * Returns the policy assigned to a role.
   *
   * @param role - Platform role.
   * @returns Human-readable policy name.
   */
  policyFor(role: string): string {
    return role === 'Admin'
      ? 'AdminOnly'
      : role === 'Staff'
        ? 'StaffOrAdmin'
        : role === 'Trainer'
          ? 'TrainerOrAbove'
          : 'MemberOrAbove';
  }

  /** Returns a truncated access-token preview. */
  tokenPreview(): string {
    const token = this.facade.session()?.accessToken ?? '';
    return token.length > 42 ? `${token.slice(0, 42)}...` : token;
  }

  /** Rotates the refresh token and renews the access token. */
  async rotate(): Promise<void> {
    await this.facade.rotate();
  }

  /** Signs out and returns to the login page. */
  async logout(): Promise<void> {
    await this.facade.logout();
    await this.router.navigateByUrl('/login');
  }

  private async load(): Promise<void> {
    await this.facade.loadSession();
    if (!this.facade.isAuthenticated()) {
      await this.router.navigateByUrl('/login');
    }
  }
}
