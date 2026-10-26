import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { IdentityAccessFacade } from './identity-access.facade';

/** Split-screen login page for the identity module. */
@Component({
  selector: 'kinevor-login',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <section class="auth-shell">
      <aside class="auth-story" aria-label="Kinevor identity overview">
        <div class="story-glow story-glow-a" aria-hidden="true"></div>
        <div class="story-glow story-glow-b" aria-hidden="true"></div>

        <div class="story-content">
          <span class="eyebrow"><span class="dot" aria-hidden="true"></span> Identity &amp; Access</span>
          <h1 class="story-title">One secure door<br /><span>to every Kinevor app.</span></h1>
          <p class="story-lead">
            Short-lived access tokens, rotating refresh tokens and role-aware sessions.
            Built for staff consoles, member apps and public booking journeys.
          </p>

          <div class="story-stats">
            <div class="story-stat">
              <span class="stat-value">15 min</span>
              <span class="stat-label">Access token</span>
            </div>
            <div class="story-stat">
              <span class="stat-value">30 days</span>
              <span class="stat-label">Refresh window</span>
            </div>
            <div class="story-stat">
              <span class="stat-value">4 roles</span>
              <span class="stat-label">Policy ready</span>
            </div>
          </div>

          <ul class="story-list">
            <li><mat-icon aria-hidden="true">verified_user</mat-icon> PBKDF2 password hashing</li>
            <li><mat-icon aria-hidden="true">autorenew</mat-icon> Single-use refresh rotation</li>
            <li><mat-icon aria-hidden="true">shield</mat-icon> Admin, Staff, Trainer and Member policies</li>
          </ul>
        </div>

        <div class="story-foot">
          <span class="logo-mark" aria-hidden="true">K</span>
          <span>Kinevor Gym Platform</span>
          <span class="demo-pill">Demo host</span>
        </div>
      </aside>

      <main class="auth-panel">
        <mat-card class="auth-card">
          <mat-card-content>
            <div class="card-head">
              <span class="eyebrow">Welcome back</span>
              <h2>Sign in to Kinevor</h2>
              <p>Use a seeded demo account or your own identity.</p>
            </div>

            <form (ngSubmit)="submit()" #form="ngForm" class="auth-form">
              <mat-form-field appearance="outline">
                <mat-label>E-mail address</mat-label>
                <input
                  matInput
                  name="email"
                  type="email"
                  autocomplete="username"
                  [(ngModel)]="email"
                  required
                />
                <mat-icon matSuffix aria-hidden="true">mail</mat-icon>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Password</mat-label>
                <input
                  matInput
                  name="password"
                  [type]="showPassword ? 'text' : 'password'"
                  autocomplete="current-password"
                  [(ngModel)]="password"
                  required
                />
                <button
                  mat-icon-button
                  matSuffix
                  type="button"
                  (click)="showPassword = !showPassword"
                  [attr.aria-label]="showPassword ? 'Hide password' : 'Show password'"
                >
                  <mat-icon aria-hidden="true">{{ showPassword ? 'visibility_off' : 'visibility' }}</mat-icon>
                </button>
              </mat-form-field>

              @if (facade.error(); as error) {
                <p class="form-error" role="alert"><mat-icon aria-hidden="true">error</mat-icon>{{ error }}</p>
              }

              <button
                mat-flat-button
                color="primary"
                type="submit"
                class="submit"
                [disabled]="facade.loading() || !email || !password"
              >
                @if (facade.loading()) {
                  <mat-spinner diameter="20" class="btn-spinner" />
                }
                Sign in
                <mat-icon aria-hidden="true">arrow_forward</mat-icon>
              </button>
            </form>

            <div class="demo-accounts">
              <span class="demo-title">Seeded accounts</span>
              @for (account of accounts; track account.email) {
                <button type="button" class="demo-account" (click)="useAccount(account.email)">
                  <span class="role-dot" [class]="'role-' + account.role.toLowerCase()"></span>
                  <span class="account-copy">
                    <strong>{{ account.role }}</strong>
                    <small>{{ account.email }}</small>
                  </span>
                  <mat-icon aria-hidden="true">north_east</mat-icon>
                </button>
              }
              <p class="demo-password">Password <code>Kinevor!2026</code> for every account.</p>
            </div>
          </mat-card-content>
        </mat-card>
      </main>
    </section>
  `,
  styles: [`
    :host { display:block; }
    .auth-shell { display:grid; grid-template-columns:1fr; min-height:calc(100vh - 64px); background:var(--color-bg); }
    .auth-story { position:relative; overflow:hidden; display:flex; flex-direction:column; justify-content:space-between; gap:32px; padding:48px 40px; background:linear-gradient(160deg, color-mix(in srgb, var(--color-accent-fill) 18%, var(--color-ink)) 0%, var(--color-ink) 62%); border-bottom:1px solid var(--color-border); }
    .story-glow { position:absolute; border-radius:999px; filter:blur(100px); pointer-events:none; }
    .story-glow-a { width:360px; height:360px; top:-120px; inset-inline-end:-80px; background:color-mix(in srgb, var(--color-accent-fill) 34%, transparent); }
    .story-glow-b { width:300px; height:300px; bottom:-120px; inset-inline-start:-60px; background:color-mix(in srgb, var(--color-info) 20%, transparent); }
    .story-content { position:relative; display:flex; flex-direction:column; gap:20px; max-width:560px; }
    .eyebrow { display:inline-flex; align-items:center; gap:8px; font-size:11px; font-weight:700; letter-spacing:.16em; text-transform:uppercase; color:var(--color-accent-text); }
    .dot { width:8px; height:8px; border-radius:999px; background:var(--color-accent-fill); box-shadow:0 0 0 5px color-mix(in srgb, var(--color-accent-fill) 18%, transparent); }
    .story-title { font-size:clamp(2.4rem, 4.5vw, 4.1rem); line-height:1.03; letter-spacing:-.045em; font-weight:800; color:var(--color-on-accent); margin:0; }
    .story-title span { color:var(--color-accent-text); }
    .story-lead { max-width:48ch; margin:0; color:color-mix(in srgb, var(--color-on-accent) 72%, transparent); line-height:1.7; font-size:15px; }
    .story-stats { display:grid; grid-template-columns:repeat(3, minmax(0,1fr)); gap:10px; margin-top:8px; }
    .story-stat { padding:16px; border-radius:14px; background:color-mix(in srgb, var(--color-on-accent) 6%, transparent); border:1px solid color-mix(in srgb, var(--color-on-accent) 12%, transparent); display:flex; flex-direction:column; gap:4px; }
    .stat-value { font-size:22px; font-weight:800; color:var(--color-on-accent); }
    .stat-label { font-size:11px; text-transform:uppercase; letter-spacing:.1em; color:color-mix(in srgb, var(--color-on-accent) 58%, transparent); }
    .story-list { list-style:none; margin:8px 0 0; padding:0; display:flex; flex-direction:column; gap:10px; }
    .story-list li { display:flex; align-items:center; gap:10px; font-size:13px; color:color-mix(in srgb, var(--color-on-accent) 78%, transparent); }
    .story-list mat-icon { font-size:18px; width:18px; height:18px; color:var(--color-accent-text); }
    .story-foot { position:relative; display:flex; align-items:center; gap:10px; font-size:12px; font-weight:600; color:color-mix(in srgb, var(--color-on-accent) 62%, transparent); }
    .logo-mark { width:30px; height:30px; border-radius:9px; background:var(--gradient-brandWarm); color:var(--color-on-accent); display:inline-flex; align-items:center; justify-content:center; font-weight:900; }
    .demo-pill { margin-inline-start:auto; font-size:10px; text-transform:uppercase; letter-spacing:.14em; padding:4px 10px; border-radius:999px; border:1px solid color-mix(in srgb, var(--color-on-accent) 24%, transparent); }
    .auth-panel { display:flex; align-items:center; justify-content:center; padding:40px 20px; }
    .auth-card { width:100%; max-width:470px; }
    .auth-card mat-card-content { padding:32px !important; }
    .card-head { display:flex; flex-direction:column; gap:6px; margin-bottom:24px; }
    .card-head h2 { margin:0; font-size:28px; letter-spacing:-.03em; color:var(--color-text); }
    .card-head p { margin:0; color:var(--color-text-muted); font-size:14px; }
    .auth-form { display:flex; flex-direction:column; gap:6px; }
    .auth-form mat-form-field { width:100%; }
    .submit { width:100%; height:52px; margin-top:8px; font-size:15px; display:inline-flex; align-items:center; justify-content:center; gap:8px; }
    .form-error { display:flex; align-items:center; gap:6px; color:var(--color-error); font-size:13px; margin:0 0 4px; }
    .form-error mat-icon { font-size:17px; width:17px; height:17px; }
    .demo-accounts { margin-top:28px; padding-top:20px; border-top:1px solid var(--color-border); display:flex; flex-direction:column; gap:8px; }
    .demo-title { font-size:11px; font-weight:700; letter-spacing:.12em; text-transform:uppercase; color:var(--color-text-muted); margin-bottom:2px; }
    .demo-account { display:flex; align-items:center; gap:10px; width:100%; padding:9px 12px; border-radius:10px; border:1px solid var(--color-border); background:var(--color-surface); color:var(--color-text); cursor:pointer; font:inherit; text-align:start; transition:border-color .18s ease, background .18s ease; }
    .demo-account:hover { border-color:var(--color-accent-text); background:var(--color-accent-soft); }
    .role-dot { width:8px; height:8px; border-radius:999px; flex:none; background:var(--color-text-muted); }
    .role-admin { background:var(--color-accent-fill); }
    .role-staff { background:var(--color-info); }
    .role-trainer { background:var(--color-warning); }
    .role-member { background:var(--color-success); }
    .account-copy { display:flex; flex-direction:column; gap:1px; flex:1; min-width:0; }
    .account-copy strong { font-size:13px; }
    .account-copy small { font-size:12px; color:var(--color-text-muted); }
    .demo-account mat-icon { font-size:15px; width:15px; height:15px; color:var(--color-text-muted); }
    .demo-password { margin:6px 0 0; font-size:12px; color:var(--color-text-muted); }
    .demo-password code { color:var(--color-accent-text); background:var(--color-accent-soft); padding:2px 6px; border-radius:5px; }
    @media (min-width: 940px) { .auth-shell { grid-template-columns:1.12fr .88fr; } .auth-story { border-bottom:none; border-inline-end:1px solid var(--color-border); padding:56px; } .auth-panel { padding:56px; } }
    @media (max-width: 520px) { .story-stats { grid-template-columns:1fr; } .auth-panel { padding:24px 14px; } .auth-card mat-card-content { padding:24px !important; } }
  `],
})
export class LoginComponent {
  /** Facade that owns the session state. */
  readonly facade = inject(IdentityAccessFacade);
  private readonly router = inject(Router);

  /** Seeded accounts shown below the form. */
  readonly accounts = [
    { role: 'Admin', email: 'admin@kinevor.demo' },
    { role: 'Staff', email: 'staff@kinevor.demo' },
    { role: 'Trainer', email: 'trainer@kinevor.demo' },
    { role: 'Member', email: 'member@kinevor.demo' },
  ];

  /** Current e-mail input. */
  email = 'admin@kinevor.demo';
  /** Current password input. */
  password = 'Kinevor!2026';
  /** Whether the password is rendered in clear text. */
  showPassword = false;

  /** Submits credentials and navigates to the session view on success. */
  async submit(): Promise<void> {
    if (!this.email || !this.password || this.facade.loading()) return;

    if (await this.facade.login(this.email.trim(), this.password)) {
      await this.router.navigateByUrl('/session');
    }
  }

  /** Prefills the form with a seeded account. */
  useAccount(email: string): void {
    this.email = email;
    this.password = 'Kinevor!2026';
  }
}
