import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PublicSiteFacade } from './public-site.facade';
import { PUBLIC_PAGE_STYLES } from './public-page.styles';
import { formatMoney } from './public-site.utils';

@Component({
  selector: 'kinevor-public-plans',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (facade.site(); as site) {
      <div class="plans-page" data-testid="plans-page">
        <section class="page-hero">
          <div class="hero-orbit" aria-hidden="true"></div>
          <div class="kx-container page-hero-grid">
            <div>
              <p class="kx-eyebrow">Memberships</p>
              <h1 class="page-title">
                Choose your<br>
                <span>training life.</span>
              </h1>
            </div>
            <div class="hero-side">
              <p>{{ site.description }}</p>
              <div class="hero-side-meta">
                <span class="kx-chip"><span class="kx-dot"></span> No joining fee</span>
                <span class="kx-chip">Cancel with 30 days' notice</span>
              </div>
            </div>
          </div>
        </section>

        <section class="plans-body">
          <div class="kx-container">
            <div class="plan-intro">
              <span>Four ways in. One standard of care.</span>
              <span>All prices in USD</span>
            </div>

            <div class="plan-grid">
              @for (plan of facade.plans(); track plan.id; let index = $index) {
                <article class="plan-card" [class.plan-featured]="plan.isFeatured">
                  <div class="plan-head">
                    @if (plan.badge) {
                      <span class="plan-badge">{{ plan.badge }}</span>
                    }
                    <span class="plan-count">0{{ index + 1 }}</span>
                    <span class="plan-tier">{{ plan.tier }}</span>
                    <h2>{{ plan.name }}</h2>
                    <p>{{ plan.description }}</p>
                  </div>

                  <div class="plan-price">
                    <span class="amount">{{ formatMoney(plan.price, plan.currency) }}</span>
                    <span class="period">{{ plan.billingLabel }}</span>
                  </div>

                  <ul>
                    @for (feature of plan.features; track feature) {
                      <li>
                        <span class="material-icons" aria-hidden="true">check</span>
                        {{ feature }}
                      </li>
                    }
                  </ul>

                  <a
                    class="kx-btn"
                    [class.kx-btn-ember]="plan.isFeatured"
                    [class.kx-btn-outline]="!plan.isFeatured"
                    routerLink="/trial"
                    [queryParams]="{ plan: plan.slug }"
                  >
                    Choose {{ plan.name }}
                    <span class="material-icons" aria-hidden="true">arrow_forward</span>
                  </a>
                </article>
              }
            </div>

            <div class="plan-note">
              <span class="material-icons" aria-hidden="true">info</span>
              <p>
                Monthly plans can be frozen for up to three months each year. Annual plans are billed once
                and include the best effective rate. A coach will confirm the right fit before you commit.
              </p>
            </div>

            <div class="included-head">
              <p class="kx-eyebrow">Included in every plan</p>
              <h2 class="kx-section-title">The fundamentals<br>are never an add-on.</h2>
            </div>

            <div class="included-grid">
              <article>
                <span class="included-number">01</span>
                <span class="material-icons" aria-hidden="true">badge</span>
                <h3>A coached start</h3>
                <p>A 30-minute onboarding with a coach, a movement screen and a plan you can actually follow.</p>
              </article>
              <article>
                <span class="included-number">02</span>
                <span class="material-icons" aria-hidden="true">apartment</span>
                <h3>Every location</h3>
                <p>One membership unlocks Main, Downtown and Westside, from early mornings to late evenings.</p>
              </article>
              <article>
                <span class="included-number">03</span>
                <span class="material-icons" aria-hidden="true">apps</span>
                <h3>The member app</h3>
                <p>Book classes, track visits, freeze your plan and message the coaching team from your phone.</p>
              </article>
              <article>
                <span class="included-number">04</span>
                <span class="material-icons" aria-hidden="true">lock_open</span>
                <h3>No lock-in</h3>
                <p>Stay because it works, not because a contract says so. Monthly plans need 30 days' notice.</p>
              </article>
            </div>
          </div>
        </section>

        <section class="plans-cta">
          <div class="kx-container plans-cta-inner">
            <div>
              <p class="kx-eyebrow">Not sure yet?</p>
              <h2>Try the room before<br>you pick a plan.</h2>
            </div>
            <div class="plans-cta-actions">
              <p>Your first session is free, including a class and a floor walkthrough with a coach.</p>
              <a class="kx-btn kx-btn-ember" routerLink="/trial">
                Book a free trial
                <span class="material-icons" aria-hidden="true">arrow_forward</span>
              </a>
            </div>
          </div>
        </section>
      </div>
    } @else {
      <div class="kx-loading">
        <div class="kx-skeleton skeleton-title"></div>
        <div class="kx-skeleton skeleton-grid"></div>
      </div>
    }
  `,
  styles: [PUBLIC_PAGE_STYLES, `
    .plans-page { overflow: hidden; }

    .page-hero {
      position: relative;
      overflow: hidden;
      background: var(--kx-night);
      color: var(--kx-cream);
      padding-block: 72px 66px;
    }

    .page-hero::before {
      content: '';
      position: absolute;
      top: -280px;
      right: 8%;
      width: 600px;
      height: 600px;
      border-radius: 999px;
      background: radial-gradient(circle, rgba(240, 79, 36, .23), transparent 67%);
    }

    .hero-orbit {
      position: absolute;
      top: -120px;
      right: 8%;
      width: 480px;
      height: 480px;
      border: 1px solid rgba(255, 255, 255, .08);
      border-radius: 999px;
    }

    .hero-orbit::after {
      content: '';
      position: absolute;
      inset: 70px;
      border: 1px solid rgba(240, 79, 36, .25);
      border-radius: inherit;
    }

    .page-hero-grid {
      position: relative;
      display: grid;
      grid-template-columns: 1.1fr .9fr;
      gap: 60px;
      align-items: end;
    }

    .page-title {
      margin: 19px 0 0;
      font-size: clamp(55px, 6.4vw, 92px);
      font-weight: 840;
      letter-spacing: -.067em;
      line-height: .9;
    }

    .page-title span {
      color: var(--kx-ember);
      font-family: Georgia, 'Times New Roman', serif;
      font-style: italic;
      font-weight: 500;
    }

    .hero-side { padding-bottom: 3px; }
    .hero-side > p { max-width: 43ch; margin: 0 0 24px; color: rgba(251, 246, 238, .5); font-size: 12px; line-height: 1.75; }

    .hero-side-meta { display: flex; flex-wrap: wrap; gap: 8px; }

    .hero-side-meta .kx-chip {
      border-color: rgba(255, 255, 255, .13);
      background: rgba(255, 255, 255, .045);
      color: rgba(251, 246, 238, .62);
    }

    .plans-body { padding-block: 52px 92px; background: var(--kx-paper); }

    .plan-intro {
      display: flex;
      justify-content: space-between;
      gap: 20px;
      margin-bottom: 22px;
      color: var(--kx-muted);
      font-size: 9px;
      font-weight: 650;
      letter-spacing: .06em;
      text-transform: uppercase;
    }

    .plan-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; align-items: stretch; }

    .plan-card {
      display: flex;
      min-width: 0;
      flex-direction: column;
      padding: 24px 21px 21px;
      border: 1px solid var(--kx-line);
      border-radius: 22px;
      background: var(--kx-surface);
    }

    .plan-featured {
      border-color: var(--kx-night);
      background: var(--kx-night);
      color: var(--kx-cream);
      box-shadow: 0 22px 40px rgba(23, 20, 15, .13);
    }

    .plan-head { position: relative; }
    .plan-count { display: block; color: var(--kx-line); font-size: 32px; font-weight: 800; letter-spacing: -.06em; line-height: 1; }
    .plan-featured .plan-count { color: rgba(255, 255, 255, .11); }
    .plan-tier { display: block; margin-top: 17px; color: var(--kx-ember); font-size: 8px; font-weight: 750; letter-spacing: .14em; text-transform: uppercase; }

    .plan-badge {
      position: absolute;
      top: -6px;
      right: 0;
      padding: 4px 8px;
      border-radius: 999px;
      background: var(--kx-ember);
      color: #fff8ef;
      font-size: 7px;
      font-weight: 750;
      letter-spacing: .07em;
      text-transform: uppercase;
    }

    .plan-head h2 { margin: 7px 0 8px; font-size: 25px; font-weight: 780; letter-spacing: -.045em; line-height: 1.1; }
    .plan-head p { min-height: 76px; margin: 0; color: var(--kx-muted); font-size: 9px; line-height: 1.65; }
    .plan-featured .plan-head p { color: rgba(251, 246, 238, .48); }

    .plan-price { display: flex; align-items: baseline; gap: 6px; margin: 19px 0 18px; padding-block: 17px; border-block: 1px solid var(--kx-line); }
    .plan-featured .plan-price { border-color: rgba(255, 255, 255, .14); }
    .amount { font-size: 31px; font-weight: 780; letter-spacing: -.055em; line-height: 1; }
    .period { color: var(--kx-muted); font-size: 8px; }
    .plan-featured .period { color: rgba(251, 246, 238, .4); }

    .plan-card ul { display: flex; flex: 1; flex-direction: column; gap: 10px; margin: 0 0 23px; padding: 0; list-style: none; }
    .plan-card li { display: flex; align-items: flex-start; gap: 8px; color: var(--kx-muted); font-size: 9px; line-height: 1.5; }
    .plan-featured li { color: rgba(251, 246, 238, .62); }
    .plan-card li .material-icons { flex: none; margin-top: 1px; color: var(--kx-ember); font-size: 14px; }

    .kx-btn { min-height: 44px; padding-inline: 12px; font-size: 10px; }

    .kx-btn-outline { border-color: var(--kx-line); color: var(--kx-ink); }
    .kx-btn-outline:hover { border-color: var(--kx-ink); }

    .plan-note {
      display: flex;
      gap: 11px;
      max-width: 720px;
      margin-top: 24px;
      padding: 15px 17px;
      border: 1px solid var(--kx-line);
      border-radius: 13px;
      background: var(--kx-surface);
    }

    .plan-note .material-icons { flex: none; color: var(--kx-ember); font-size: 17px; }
    .plan-note p { margin: 0; color: var(--kx-muted); font-size: 9px; line-height: 1.7; }

    .included-head { margin-top: 92px; }
    .included-head .kx-section-title { margin-top: 18px; }

    .included-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 0;
      margin-top: 36px;
      border-block: 1px solid var(--kx-line);
    }

    .included-grid article {
      position: relative;
      min-height: 260px;
      padding: 28px 25px 26px;
      border-right: 1px solid var(--kx-line);
    }

    .included-grid article:last-child { border-right: 0; }
    .included-number { position: absolute; top: 25px; right: 22px; color: var(--kx-muted); font-size: 8px; font-weight: 700; letter-spacing: .1em; }
    .included-grid .material-icons { display: block; margin-bottom: 44px; color: var(--kx-ember); font-size: 27px; }
    .included-grid h3 { margin: 0 0 8px; font-size: 15px; font-weight: 750; letter-spacing: -.02em; }
    .included-grid p { margin: 0; color: var(--kx-muted); font-size: 9px; line-height: 1.65; }

    .plans-cta { background: var(--kx-paper-deep); padding-block: 58px; }
    .plans-cta-inner { display: grid; grid-template-columns: 1.05fr .95fr; gap: 60px; align-items: center; }
    .plans-cta h2 { margin: 17px 0 0; font-size: clamp(34px, 3.8vw, 48px); font-weight: 800; letter-spacing: -.05em; line-height: 1.02; }
    .plans-cta-actions { display: flex; align-items: center; gap: 30px; }
    .plans-cta-actions p { max-width: 32ch; margin: 0; color: var(--kx-muted); font-size: 10px; line-height: 1.65; }

    .skeleton-title { width: 54%; height: 220px; }
    .skeleton-grid { height: 420px; }

    @media (max-width: 1080px) {
      .plan-grid { grid-template-columns: 1fr 1fr; }
      .included-grid { grid-template-columns: 1fr 1fr; }
      .included-grid article:nth-child(2) { border-right: 0; }
      .included-grid article:nth-child(-n+2) { border-bottom: 1px solid var(--kx-line); }
    }

    @media (max-width: 800px) {
      .page-hero-grid, .plans-cta-inner { grid-template-columns: 1fr; gap: 30px; }
      .page-title { font-size: clamp(52px, 12vw, 76px); }
      .plan-grid { grid-template-columns: 1fr; }
      .plan-head p { min-height: 0; }
      .plan-card li { font-size: 10px; }
      .plan-note { margin-top: 20px; }
      .plans-cta-actions { align-items: flex-start; flex-direction: column; gap: 18px; }
    }

    @media (max-width: 560px) {
      .page-hero { padding-block: 48px; }
      .included-grid { grid-template-columns: 1fr; }
      .included-grid article { border-right: 0; border-bottom: 1px solid var(--kx-line); min-height: 210px; }
      .included-grid article:last-child { border-bottom: 0; }
      .included-grid .material-icons { margin-bottom: 28px; }
      .plan-intro { flex-direction: column; }
    }
  `],
})
export class PublicPlansComponent {
  readonly facade = inject(PublicSiteFacade);
  protected readonly formatMoney = formatMoney;
}
