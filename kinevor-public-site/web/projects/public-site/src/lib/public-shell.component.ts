import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { PublicSiteFacade } from './public-site.facade';

@Component({
  selector: 'kinevor-public-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="site-header">
      <div class="header-inner">
        <a class="brand" routerLink="/" aria-label="Kinevor Gym home" (click)="closeMenu()">
          <img class="brand-logo" src="kinevor-logo.png" width="52" height="52" alt="Kinevor Gym">
        </a>

        <nav class="desktop-nav" aria-label="Main navigation">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">
            Home
          </a>
          <a routerLink="/plans" routerLinkActive="active">Memberships</a>
          <a routerLink="/trainers" routerLinkActive="active">Coaches</a>
          <a routerLink="/schedule" routerLinkActive="active">Timetable</a>
        </nav>

        <div class="header-actions">
          <a class="header-phone" [href]="'tel:' + (facade.site()?.phone ?? '')">
            {{ facade.site()?.phone ?? '+1 (212) 555-0123' }}
          </a>
          <a class="kx-btn kx-btn-ember header-cta" routerLink="/trial" (click)="closeMenu()">
            Book a trial
            <span class="material-icons" aria-hidden="true">arrow_outward</span>
          </a>
          <button
            type="button"
            class="menu-button"
            [attr.aria-expanded]="menuOpen()"
            aria-controls="mobile-menu"
            aria-label="Toggle navigation"
            (click)="toggleMenu()"
          >
            <span class="material-icons" aria-hidden="true">{{ menuOpen() ? 'close' : 'menu' }}</span>
          </button>
        </div>
      </div>

      @if (menuOpen()) {
        <nav id="mobile-menu" class="mobile-nav" aria-label="Mobile navigation">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }" (click)="closeMenu()">Home</a>
          <a routerLink="/plans" routerLinkActive="active" (click)="closeMenu()">Memberships</a>
          <a routerLink="/trainers" routerLinkActive="active" (click)="closeMenu()">Coaches</a>
          <a routerLink="/schedule" routerLinkActive="active" (click)="closeMenu()">Timetable</a>
          <a routerLink="/trial" routerLinkActive="active" (click)="closeMenu()">Book a free trial</a>
        </nav>
      }
    </header>

    @if (facade.error(); as error) {
      <div class="load-error" role="alert">
        <div class="load-error-inner">
          <span class="material-icons" aria-hidden="true">cloud_off</span>
          <p>{{ error }}</p>
          <button type="button" (click)="retry()">Try again</button>
        </div>
      </div>
    }

    <main id="main-content">
      <router-outlet />
    </main>

    <footer class="site-footer">
      <div class="footer-inner">
        <div class="footer-brand">
          <a class="footer-lockup" routerLink="/" aria-label="Kinevor Gym home">
            <img class="footer-logo" src="kinevor-logo.png" width="52" height="52" alt="Kinevor Gym">
          </a>
          <p>{{ facade.site()?.tagline ?? 'Built to move. Made to belong.' }}</p>
          <a class="footer-cta" routerLink="/trial">
            Start with a free session
            <span class="material-icons" aria-hidden="true">arrow_forward</span>
          </a>
        </div>

        <div class="footer-column">
          <span class="footer-label">Explore</span>
          <a routerLink="/plans">Memberships</a>
          <a routerLink="/trainers">Coaches</a>
          <a routerLink="/schedule">Timetable</a>
          <a routerLink="/trial">Free trial</a>
        </div>

        <div class="footer-column">
          <span class="footer-label">Visit</span>
          @for (branch of facade.branches(); track branch.id) {
            <span>{{ branch.name }}</span>
            <small>{{ branch.neighbourhood }}</small>
          } @empty {
            <span>Kinevor Main</span>
            <small>Tribeca, New York</small>
          }
        </div>

        <div class="footer-column">
          <span class="footer-label">Contact</span>
          <a [href]="'mailto:' + (facade.site()?.email ?? 'hello@kinevorgym.com')">
            {{ facade.site()?.email ?? 'hello@kinevorgym.com' }}
          </a>
          <a [href]="'tel:' + (facade.site()?.phone ?? '')">
            {{ facade.site()?.phone ?? '+1 (212) 555-0123' }}
          </a>
          <span>{{ facade.site()?.instagramHandle ?? '@kinevorgym' }}</span>
        </div>
      </div>

      <div class="footer-bottom">
        <span>&copy; {{ year }} Kinevor Gym. All rights reserved.</span>
        <span>Standalone public-site demo. Content is seeded locally.</span>
      </div>
    </footer>
  `,
  styles: [`
    :host {
      --kx-ink: #17140f;
      --kx-paper: #f7f2e9;
      --kx-paper-deep: #eee5d8;
      --kx-surface: #fffaf2;
      --kx-muted: #6f675c;
      --kx-line: rgba(23, 20, 15, .14);
      --kx-ember: #f04f24;
      --kx-ember-deep: #cf3e16;
      --kx-acid: #d8ef78;
      --kx-night: #12100e;
      --kx-night-soft: #1d1915;
      --kx-cream: #fbf6ee;
      --kx-font: 'Sora', 'Inter', system-ui, sans-serif;
      display: block;
      min-height: 100vh;
      background: var(--kx-paper);
      color: var(--kx-ink);
      font-family: var(--kx-font);
    }

    * { box-sizing: border-box; }

    .site-header {
      position: sticky;
      top: 0;
      z-index: 50;
      background: color-mix(in srgb, var(--kx-night) 94%, transparent);
      color: var(--kx-cream);
      border-bottom: 1px solid rgba(255, 255, 255, .08);
      backdrop-filter: blur(16px);
    }

    .header-inner {
      width: min(100% - 48px, 1280px);
      height: 76px;
      margin-inline: auto;
      display: flex;
      align-items: center;
      gap: 28px;
    }

    .brand {
      display: inline-flex;
      align-items: center;
      gap: 11px;
      color: var(--kx-cream);
      text-decoration: none;
      flex: none;
    }

    .brand-logo, .footer-logo {
      width: 52px;
      height: 52px;
      object-fit: cover;
      border-radius: 12px;
      box-shadow: 0 7px 18px rgba(0, 0, 0, .28);
    }

    .load-error {
      position: relative;
      z-index: 49;
      border-bottom: 1px solid rgba(240, 79, 36, .22);
      background: #281b16;
      color: var(--kx-cream);
    }

    .load-error-inner {
      display: flex;
      width: min(100% - 48px, 1280px);
      min-height: 46px;
      align-items: center;
      gap: 10px;
      margin-inline: auto;
    }

    .load-error .material-icons { color: var(--kx-ember); font-size: 18px; }
    .load-error p { margin: 0; color: rgba(251, 246, 238, .72); font-size: 10px; }
    .load-error button { margin-left: auto; padding: 0; border: 0; border-bottom: 1px solid rgba(251, 246, 238, .4); background: none; color: var(--kx-cream); font: inherit; font-size: 9px; font-weight: 700; cursor: pointer; }

    .desktop-nav {
      display: flex;
      align-items: center;
      gap: 6px;
      margin-inline-start: 18px;
    }

    .desktop-nav a {
      position: relative;
      padding: 9px 13px;
      border-radius: 999px;
      color: rgba(251, 246, 238, .62);
      font-size: 12px;
      font-weight: 600;
      text-decoration: none;
      transition: color .2s ease, background .2s ease;
    }

    .desktop-nav a:hover { color: var(--kx-cream); background: rgba(255, 255, 255, .06); }

    .desktop-nav a.active { color: var(--kx-cream); }

    .desktop-nav a.active::after {
      content: '';
      position: absolute;
      left: 50%;
      bottom: 2px;
      width: 4px;
      height: 4px;
      border-radius: 999px;
      background: var(--kx-ember);
      transform: translateX(-50%);
    }

    .header-actions {
      margin-inline-start: auto;
      display: flex;
      align-items: center;
      gap: 18px;
    }

    .header-phone {
      color: rgba(251, 246, 238, .58);
      font-size: 11px;
      font-weight: 600;
      text-decoration: none;
      white-space: nowrap;
    }

    .header-phone:hover { color: var(--kx-cream); }

    .kx-btn {
      display: inline-flex;
      min-height: 43px;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 0 18px;
      border: 1px solid transparent;
      border-radius: 999px;
      font: inherit;
      font-size: 12px;
      font-weight: 700;
      text-decoration: none;
      cursor: pointer;
      transition: transform .2s ease, background .2s ease;
    }

    .kx-btn:hover { transform: translateY(-2px); }

    .kx-btn-ember { background: var(--kx-ember); color: #fff8ef; }
    .kx-btn-ember:hover { background: var(--kx-ember-deep); }
    .kx-btn .material-icons { font-size: 16px; }

    .menu-button {
      display: none;
      width: 42px;
      height: 42px;
      align-items: center;
      justify-content: center;
      border: 1px solid rgba(255, 255, 255, .16);
      border-radius: 999px;
      background: transparent;
      color: var(--kx-cream);
      cursor: pointer;
    }

    .mobile-nav {
      display: none;
      padding: 8px 24px 24px;
      border-top: 1px solid rgba(255, 255, 255, .08);
      background: var(--kx-night);
    }

    .mobile-nav a {
      display: flex;
      min-height: 48px;
      align-items: center;
      border-bottom: 1px solid rgba(255, 255, 255, .08);
      color: var(--kx-cream);
      font-size: 14px;
      font-weight: 600;
      text-decoration: none;
    }

    .mobile-nav a.active { color: var(--kx-ember); }

    .site-footer {
      background: var(--kx-night);
      color: var(--kx-cream);
      border-top: 1px solid rgba(255, 255, 255, .08);
    }

    .footer-inner {
      width: min(100% - 48px, 1200px);
      margin-inline: auto;
      display: grid;
      grid-template-columns: 1.4fr .7fr .8fr .9fr;
      gap: 42px;
      padding-block: 70px 50px;
    }

    .footer-lockup { display: inline-flex; align-items: center; }

    .footer-brand p {
      max-width: 30ch;
      margin: 18px 0 24px;
      color: rgba(251, 246, 238, .55);
      font-size: 13px;
      line-height: 1.7;
    }

    .footer-cta {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      color: var(--kx-cream);
      font-size: 12px;
      font-weight: 700;
      text-decoration: none;
      border-bottom: 1px solid rgba(251, 246, 238, .3);
      padding-bottom: 5px;
    }

    .footer-cta:hover { color: var(--kx-ember); border-color: var(--kx-ember); }
    .footer-cta .material-icons { font-size: 16px; }

    .footer-column { display: flex; flex-direction: column; gap: 10px; }

    .footer-label {
      margin-bottom: 7px;
      color: rgba(251, 246, 238, .36);
      font-size: 10px;
      font-weight: 700;
      letter-spacing: .16em;
      text-transform: uppercase;
    }

    .footer-column a, .footer-column span {
      color: rgba(251, 246, 238, .72);
      font-size: 12px;
      text-decoration: none;
      line-height: 1.5;
    }

    .footer-column a:hover { color: var(--kx-ember); }

    .footer-column small {
      margin-top: -6px;
      color: rgba(251, 246, 238, .34);
      font-size: 10px;
    }

    .footer-bottom {
      width: min(100% - 48px, 1200px);
      margin-inline: auto;
      display: flex;
      justify-content: space-between;
      gap: 20px;
      padding-block: 22px;
      border-top: 1px solid rgba(255, 255, 255, .08);
      color: rgba(251, 246, 238, .3);
      font-size: 10px;
    }

    @media (max-width: 980px) {
      .desktop-nav { display: none; }
      .header-phone { display: none; }
      .menu-button { display: inline-flex; }
      .mobile-nav { display: block; }
      .footer-inner { grid-template-columns: 1.3fr 1fr 1fr; }
    }

    @media (max-width: 720px) {
      .header-inner, .footer-inner, .footer-bottom { width: min(100% - 32px, 1200px); }
      .load-error-inner { width: min(100% - 32px, 1200px); min-height: 42px; }
      .header-cta { display: none; }
      .footer-inner { grid-template-columns: 1fr 1fr; gap: 30px; }
      .footer-brand { grid-column: 1 / -1; }
      .footer-bottom { flex-direction: column; }
    }

    @media (max-width: 440px) {
      .footer-inner { grid-template-columns: 1fr; }
    }
  `],
})
export class PublicSiteShellComponent {
  readonly facade = inject(PublicSiteFacade);
  readonly menuOpen = signal(false);
  readonly year = new Date().getFullYear();

  constructor() {
    void this.facade.load();
  }

  toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  retry(): void {
    void this.facade.load(true);
  }
}
