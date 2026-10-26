import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

/** Demo shell for the check-ins and access workspace. */
@Component({
  selector: 'demo-root',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatButtonModule, MatIconModule],
  template: `
    <a class="skip-link" href="#main-content">Skip to content</a>

    <header class="demo-topbar">
      <a class="demo-brand" routerLink="/checkins">
        <span class="demo-logo" aria-hidden="true">K</span>
        <span class="brand-copy">
          <span>Kinevor<span class="demo-brand-accent">Access</span></span>
          <small>Front desk operations</small>
        </span>
      </a>

      <nav class="demo-nav" aria-label="Demo sections">
        <a mat-button routerLink="/checkins" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">
          <mat-icon aria-hidden="true">badge</mat-icon>
          Front desk
        </a>
        <a mat-button routerLink="/checkins/occupancy" routerLinkActive="active">
          <mat-icon aria-hidden="true">groups</mat-icon>
          Occupancy
        </a>
        <a mat-button routerLink="/checkins/history" routerLinkActive="active">
          <mat-icon aria-hidden="true">history</mat-icon>
          Visit history
        </a>
      </nav>

      <div class="demo-actor">
        <span class="actor-avatar" aria-hidden="true">RC</span>
        <span class="actor-copy">
          <strong>Riley Chen</strong>
          <small>Front desk · Staff</small>
        </span>
      </div>
      <span class="demo-badge">Demo</span>
    </header>

    <main id="main-content">
      <router-outlet />
    </main>
  `,
  styles: [
    `
      :host {
        display: block;
        min-height: 100vh;
      }

      .demo-topbar {
        position: sticky;
        top: 0;
        z-index: 20;
        display: flex;
        align-items: center;
        gap: 26px;
        height: 66px;
        padding: 0 28px;
        border-bottom: 1px solid var(--color-border);
        background: rgba(22, 24, 29, 0.92);
        backdrop-filter: blur(16px);
      }

      .demo-brand {
        display: flex;
        align-items: center;
        gap: 10px;
        min-width: 190px;
        color: var(--color-text);
        text-decoration: none;
      }

      .demo-logo {
        display: grid;
        place-items: center;
        width: 34px;
        height: 34px;
        border-radius: 10px;
        background: var(--gradient-brandWarm);
        color: var(--color-on-accent);
        font-size: 16px;
        font-weight: 800;
        box-shadow: 0 5px 18px rgba(255, 69, 58, 0.18);
      }

      .brand-copy {
        display: grid;
        gap: 1px;
        font-size: 15px;
        font-weight: 700;
        letter-spacing: -0.03em;
      }

      .demo-brand-accent {
        margin-left: 3px;
        color: var(--color-accent-text);
      }

      .brand-copy small {
        color: var(--color-text-muted);
        font-size: 8.5px;
        font-weight: 500;
        letter-spacing: 0.06em;
        text-transform: uppercase;
      }

      .demo-nav {
        display: flex;
        gap: 2px;
        margin: 0 auto;
        padding: 3px;
        border: 1px solid var(--color-border);
        border-radius: 11px;
        background: rgba(0, 0, 0, 0.13);
      }

      .demo-nav a {
        height: 32px;
        padding: 0 12px;
        border-radius: 7px;
        color: var(--color-text-muted);
        font-size: 11px;
        font-weight: 600;
        line-height: 32px;
      }

      .demo-nav a.active {
        background: var(--color-surface-raised);
        color: var(--color-text);
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.25);
      }

      .demo-nav mat-icon {
        width: 16px;
        height: 16px;
        margin-right: 5px;
        font-size: 16px;
        vertical-align: -3px;
      }

      .demo-actor {
        display: flex;
        align-items: center;
        gap: 9px;
        padding-left: 18px;
        border-left: 1px solid var(--color-border);
      }

      .actor-avatar {
        display: grid;
        place-items: center;
        width: 30px;
        height: 30px;
        border: 1px solid var(--color-border);
        border-radius: 9px;
        background: var(--color-surface-raised);
        color: var(--color-text-muted);
        font-size: 9px;
        font-weight: 700;
      }

      .actor-copy {
        display: grid;
        gap: 1px;
      }

      .actor-copy strong {
        color: var(--color-text);
        font-size: 10.5px;
        font-weight: 600;
        white-space: nowrap;
      }

      .actor-copy small {
        color: var(--color-text-muted);
        font-size: 8.5px;
        white-space: nowrap;
      }

      .demo-badge {
        padding: 4px 9px;
        border: 1px solid rgba(255, 69, 58, 0.35);
        border-radius: var(--radius-pill);
        color: var(--color-accent-text);
        font-size: 8.5px;
        font-weight: 700;
        letter-spacing: 0.09em;
        text-transform: uppercase;
      }

      main {
        position: relative;
        min-height: calc(100vh - 66px);
      }

      main::before {
        position: fixed;
        inset: 0;
        z-index: -1;
        background:
          radial-gradient(760px 380px at 92% -6%, rgba(255, 69, 58, 0.055), transparent 65%),
          radial-gradient(620px 320px at 4% 24%, rgba(77, 174, 255, 0.035), transparent 62%);
        content: '';
        pointer-events: none;
      }

      @media (max-width: 1080px) {
        .actor-copy {
          display: none;
        }

        .demo-brand {
          min-width: 0;
        }
      }

      @media (max-width: 860px) {
        .demo-topbar {
          gap: 12px;
          padding: 0 14px;
        }

        .brand-copy small,
        .demo-actor {
          display: none;
        }

        .demo-nav a {
          padding: 0 8px;
          font-size: 0;
        }

        .demo-nav mat-icon {
          margin: 0;
        }

        .demo-badge {
          margin-left: auto;
        }
      }
    `,
  ],
})
export class AppComponent {}
