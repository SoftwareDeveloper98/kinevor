import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'demo-root',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatButtonModule, MatIconModule],
  template: `
    <header class="demo-topbar">
      <a class="demo-brand" routerLink="/dashboard">
        <span class="demo-logo" aria-hidden="true">K</span>
        <span>Kinevor<span class="demo-brand-accent">Staff</span></span>
      </a>

      <nav class="demo-nav" aria-label="Demo sections">
        <a mat-button routerLink="/dashboard" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">
          <mat-icon aria-hidden="true">space_dashboard</mat-icon>
          Dashboard
        </a>
        <a mat-button routerLink="/alerts" routerLinkActive="active">
          <mat-icon aria-hidden="true">notifications_active</mat-icon>
          Alerts
        </a>
      </nav>

      <span class="demo-live"><span class="dot" aria-hidden="true"></span>Live demo</span>
      <span class="demo-actor">Staff demo · Alex Morgan</span>
      <span class="demo-badge">Demo</span>
    </header>

    <main>
      <router-outlet />
    </main>
  `,
  styles: [`
    .demo-topbar { display:flex; align-items:center; gap:18px; padding:0 28px; height:64px; background:var(--color-surface-raised); border-bottom:1px solid var(--color-border); position:sticky; top:0; z-index:20; }
    .demo-topbar::before { content:''; position:absolute; inset-inline:0; top:0; height:2px; background:var(--gradient-brandWarm); }
    .demo-brand { display:flex; align-items:center; gap:10px; text-decoration:none; color:var(--color-text); font-weight:800; font-size:16px; letter-spacing:-.3px; }
    .demo-brand-accent { color:var(--color-accent-text); margin-inline-start:4px; }
    .demo-logo { width:34px; height:34px; border-radius:10px; background:var(--gradient-brandWarm); color:var(--color-on-accent); display:flex; align-items:center; justify-content:center; font-weight:900; }
    .demo-nav { display:flex; gap:4px; margin-inline-start:auto; }
    .demo-nav a.active { color:var(--color-accent-text); }
    .demo-nav mat-icon { margin-inline-end:6px; }
    .demo-live { display:inline-flex; align-items:center; gap:6px; font-size:11px; color:var(--color-text-muted); }
    .demo-live .dot { width:7px; height:7px; border-radius:999px; background:var(--color-success); box-shadow:0 0 0 3px color-mix(in srgb, var(--color-success) 16%, transparent); }
    .demo-actor { font-size:11px; color:var(--color-text-muted); white-space:nowrap; }
    .demo-badge { font-size:11px; font-weight:700; text-transform:uppercase; letter-spacing:1px; color:var(--color-accent-text); border:1px solid var(--color-accent-text); border-radius:20px; padding:4px 10px; }
    @media(max-width:980px) { .demo-actor { display:none; } }
    @media(max-width:760px) { .demo-topbar { padding:0 14px; gap:10px; } .demo-live { display:none; } .demo-nav a { font-size:0; min-width:0; padding:8px; } .demo-nav mat-icon { margin:0; } .demo-badge { display:none; } }
  `],
})
export class AppComponent {}
