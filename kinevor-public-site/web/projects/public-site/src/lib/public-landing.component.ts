import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PublicSiteFacade } from './public-site.facade';
import { PUBLIC_PAGE_STYLES } from './public-page.styles';
import { formatMoney, formatShortDate, formatTime, initials } from './public-site.utils';

@Component({
  selector: 'kinevor-public-landing',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (facade.site(); as site) {
      <div class="landing" data-testid="landing-page">
        <section class="hero" data-testid="landing-hero">
          <div class="hero-noise" aria-hidden="true"></div>
          <div class="kx-container hero-grid">
            <div class="hero-copy">
              <p class="hero-kicker">
                <span class="kx-dot"></span>
                {{ site.eyebrow }}
              </p>

              <h1 class="hero-title">
                <span>TRAIN LIKE</span>
                <span class="hero-title-accent">YOU MEAN IT.</span>
              </h1>

              <p class="hero-lede">{{ site.description }}</p>

              <div class="hero-actions">
                <a class="kx-btn kx-btn-ember" routerLink="/trial">
                  Book your free trial
                  <span class="material-icons" aria-hidden="true">arrow_forward</span>
                </a>
                <a class="kx-btn hero-ghost" routerLink="/schedule">
                  See the timetable
                  <span class="material-icons" aria-hidden="true">calendar_month</span>
                </a>
              </div>

              <div class="hero-proof">
                <div class="proof-faces" aria-hidden="true">
                  <span>JM</span><span>SK</span><span>AR</span><span>+</span>
                </div>
                <div>
                  <div class="proof-stars" aria-label="Rated 4.9 out of 5">
                    @for (star of [1, 2, 3, 4, 5]; track star) {
                      <span class="material-icons" aria-hidden="true">star</span>
                    }
                  </div>
                  <p>Rated {{ site.rating }} by {{ formatCount(site.memberCount) }} local members</p>
                </div>
              </div>
            </div>

            <div class="hero-visual" aria-hidden="true">
              <div class="stage-card">
                <div class="stage-grid"></div>
                <div class="stage-ring ring-one"></div>
                <div class="stage-ring ring-two"></div>
                <div class="stage-bar bar-one"></div>
                <div class="stage-bar bar-two"></div>
                <div class="stage-mark">K</div>
                <span class="stage-vertical">EST. {{ site.foundedYear }} / NEW YORK</span>

                <div class="stage-live">
                  <span class="kx-dot"></span>
                  Open today
                </div>

                <div class="stage-label label-top">
                  <span>Strength floor</span>
                  <strong>Open</strong>
                </div>

                <div class="stage-label label-mid">
                  <span>Classes this week</span>
                  <strong>{{ site.weeklyClasses }}</strong>
                </div>

                <div class="stage-hud">
                  <div class="hud-icon">
                    <span class="material-icons">bolt</span>
                  </div>
                  <div class="hud-copy">
                    <span>Next session</span>
                    <strong>{{ facade.nextClasses()[0]?.name ?? 'Sunrise HIIT' }}</strong>
                    <small>
                      {{ facade.nextClasses()[0] ? formatShortDate(facade.nextClasses()[0].startsAt) : 'Tomorrow' }}
                      at {{ facade.nextClasses()[0] ? formatTime(facade.nextClasses()[0].startsAt) : '7:00 AM' }}
                    </small>
                  </div>
                  <span class="hud-arrow material-icons">north_east</span>
                </div>
              </div>

              <div class="stage-float float-top">
                <span class="material-icons">fitness_center</span>
                <div><strong>{{ formatCount(site.trainerCount) }}</strong><small>coaches</small></div>
              </div>

              <div class="stage-float float-bottom">
                <span class="float-score">{{ site.rating }}</span>
                <div><strong>Member rating</strong><small>Across all locations</small></div>
              </div>
            </div>
          </div>

          <div class="kx-container hero-stats">
            <div class="hero-stat">
              <span class="hero-stat-value">{{ formatCount(site.memberCount) }}</span>
              <span class="hero-stat-label">Members moving</span>
            </div>
            <div class="hero-stat">
              <span class="hero-stat-value">{{ site.trainerCount }}</span>
              <span class="hero-stat-label">Expert coaches</span>
            </div>
            <div class="hero-stat">
              <span class="hero-stat-value">{{ site.weeklyClasses }}</span>
              <span class="hero-stat-label">Classes each week</span>
            </div>
            <div class="hero-stat">
              <span class="hero-stat-value">{{ site.rating }}<small>/5</small></span>
              <span class="hero-stat-label">Average rating</span>
            </div>
          </div>
        </section>

        <section class="kx-section intro-section">
          <div class="kx-container intro-grid">
            <div class="intro-heading">
              <p class="kx-eyebrow">The Kinevor standard</p>
              <h2 class="kx-section-title">
                A serious gym<br>
                without the<br>
                <span class="accent-serif">cold shoulder.</span>
              </h2>
              <div class="intro-stamp" aria-hidden="true">
                <span>K</span>
                <small>NYC<br>{{ site.foundedYear }}</small>
              </div>
            </div>

            <div class="intro-body">
              <p class="intro-lead">{{ site.story }}</p>

              <div class="intro-points">
                @for (highlight of site.highlights; track highlight; let index = $index) {
                  <div class="intro-point">
                    <span class="point-number">0{{ index + 1 }}</span>
                    <span>{{ highlight }}</span>
                  </div>
                }
              </div>

              <div class="intro-actions">
                <a class="kx-link-arrow" routerLink="/trainers">
                  Meet the coaching team
                  <span class="material-icons" aria-hidden="true">arrow_forward</span>
                </a>
                <span class="intro-note">One membership. Three neighbourhoods.</span>
              </div>
            </div>
          </div>
        </section>

        <section class="feature-band">
          <div class="kx-container feature-grid">
            <article class="feature-card">
              <div class="feature-top">
                <span class="feature-icon material-icons" aria-hidden="true">fitness_center</span>
                <span class="feature-index">01</span>
              </div>
              <h3>Train with a plan</h3>
              <p>Every membership includes a coached onboarding and a programme built around your starting point.</p>
            </article>
            <article class="feature-card feature-card-warm">
              <div class="feature-top">
                <span class="feature-icon material-icons" aria-hidden="true">groups</span>
                <span class="feature-index">02</span>
              </div>
              <h3>Find your room</h3>
              <p>Small-group strength, high-energy conditioning and quiet recovery. The timetable has a place for every pace.</p>
            </article>
            <article class="feature-card">
              <div class="feature-top">
                <span class="feature-icon material-icons" aria-hidden="true">spa</span>
                <span class="feature-index">03</span>
              </div>
              <h3>Recover on purpose</h3>
              <p>Sauna, cold plunge and mobility space are part of the programme, not an afterthought.</p>
            </article>
          </div>
        </section>

        <section class="kx-section plans-section" id="memberships">
          <div class="kx-container">
            <div class="section-head">
              <div>
                <p class="kx-eyebrow">Memberships</p>
                <h2 class="kx-section-title">Pick the pace<br>you want to keep.</h2>
              </div>
              <div class="section-side">
                <p class="kx-lede">No joining fee. No long-term lock-in. Just clear access, expert coaching and room to grow.</p>
                <a class="kx-link-arrow" routerLink="/plans">
                  Compare all plans
                  <span class="material-icons" aria-hidden="true">arrow_forward</span>
                </a>
              </div>
            </div>

            <div class="plan-grid">
              @for (plan of facade.featuredPlans(); track plan.id) {
                <article class="plan-card" [class.plan-featured]="plan.isFeatured">
                  @if (plan.badge) {
                    <span class="plan-badge">{{ plan.badge }}</span>
                  }
                  <div class="plan-top">
                    <span class="plan-tier">{{ plan.tier }}</span>
                    <h3>{{ plan.name }}</h3>
                    <p>{{ plan.description }}</p>
                  </div>
                  <div class="plan-price">
                    <span class="plan-amount">{{ formatMoney(plan.price, plan.currency) }}</span>
                    <span class="plan-period">{{ plan.billingLabel }}</span>
                  </div>
                  <ul class="plan-features">
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
                    [class.plan-button]="!plan.isFeatured"
                    routerLink="/trial"
                    [queryParams]="{ plan: plan.slug }"
                  >
                    Start with {{ plan.name }}
                  </a>
                </article>
              }
            </div>

            <div class="plan-assurance">
              <span><span class="material-icons">verified</span> No joining fee</span>
              <span><span class="material-icons">ac_unit</span> Freeze any time</span>
              <span><span class="material-icons">schedule</span> 30 days' notice</span>
              <span><span class="material-icons">location_on</span> All three gyms</span>
            </div>
          </div>
        </section>

        <section class="schedule-band">
          <div class="kx-container schedule-grid">
            <div class="schedule-copy">
              <p class="kx-eyebrow">This week at Kinevor</p>
              <h2 class="kx-section-title">Good training<br>is on the calendar.</h2>
              <p class="schedule-lede">
                Strength, cycling, yoga and recovery sessions across all three locations.
                Book ahead or drop into the floor on your own schedule.
              </p>
              <a class="kx-btn schedule-btn" routerLink="/schedule">
                View full timetable
                <span class="material-icons" aria-hidden="true">arrow_forward</span>
              </a>
              <div class="schedule-branches">
                @for (branch of site.branches; track branch.id) {
                  <span>{{ branch.neighbourhood }}</span>
                }
              </div>
            </div>

            <div class="schedule-list">
              @for (session of facade.nextClasses(); track session.id; let index = $index) {
                <article class="schedule-item">
                  <div class="schedule-date">
                    <span>{{ formatShortDate(session.startsAt).split(',')[0] }}</span>
                    <strong>{{ formatShortDate(session.startsAt).split(',')[1] }}</strong>
                  </div>
                  <div class="schedule-main">
                    <span class="schedule-category">{{ session.category }}</span>
                    <h3>{{ session.name }}</h3>
                    <p>{{ session.trainerName }} &middot; {{ session.branchName }}</p>
                  </div>
                  <div class="schedule-time">
                    <strong>{{ formatTime(session.startsAt) }}</strong>
                    <span>{{ session.durationMinutes }} min</span>
                  </div>
                  <span class="schedule-index">0{{ index + 1 }}</span>
                </article>
              }
            </div>
          </div>
        </section>

        <section class="kx-section coaches-section">
          <div class="kx-container">
            <div class="section-head">
              <div>
                <p class="kx-eyebrow">The coaching floor</p>
                <h2 class="kx-section-title">People who know<br>your name and your numbers.</h2>
              </div>
              <div class="section-side">
                <p class="kx-lede">National certifications, years on the floor and a genuine interest in the person behind the programme.</p>
                <a class="kx-link-arrow" routerLink="/trainers">
                  Meet every coach
                  <span class="material-icons" aria-hidden="true">arrow_forward</span>
                </a>
              </div>
            </div>

            <div class="coach-grid">
              @for (trainer of facade.featuredTrainers(); track trainer.id; let index = $index) {
                <article class="coach-card">
                  <div class="coach-portrait" [class]="'coach-portrait tone-' + (index % 4)">
                    <span class="coach-ring" aria-hidden="true"></span>
                    <span class="coach-initials">{{ initials(trainer.fullName) }}</span>
                    <span class="coach-number">0{{ index + 1 }}</span>
                    <span class="coach-years">{{ trainer.yearsExperience }} yrs</span>
                  </div>
                  <div class="coach-body">
                    <h3>{{ trainer.fullName }}</h3>
                    <span class="coach-role">{{ trainer.roleTitle }}</span>
                    <p>{{ trainer.bio }}</p>
                    <div class="coach-tags">
                      @for (specialty of trainer.specializations; track specialty) {
                        <span>{{ specialty }}</span>
                      }
                    </div>
                  </div>
                </article>
              }
            </div>
          </div>
        </section>

        <section class="kx-section faq-section">
          <div class="kx-container faq-grid">
            <div class="faq-copy">
              <p class="kx-eyebrow">Before you start</p>
              <h2 class="kx-section-title">The honest<br>answers.</h2>
              <p class="kx-lede">Still deciding? Our membership team is at the front desk six days a week. No pressure, no script.</p>
              <a class="kx-btn kx-btn-dark" routerLink="/trial">Talk to a coach</a>
            </div>

            <div class="faq-list">
              @for (faq of facade.faqs(); track faq.id; let index = $index) {
                <details class="faq-item" [open]="index === 0">
                  <summary>
                    <span class="faq-count">0{{ index + 1 }}</span>
                    <span>{{ faq.question }}</span>
                    <span class="material-icons faq-toggle" aria-hidden="true">add</span>
                  </summary>
                  <p>{{ faq.answer }}</p>
                </details>
              }
            </div>
          </div>
        </section>

        <section class="kx-container final-cta">
          <div class="final-cta-visual" aria-hidden="true">
            <span class="cta-word">MOVE</span>
            <span class="cta-ring"></span>
            <span class="cta-ring cta-ring-two"></span>
          </div>
          <div class="final-cta-copy">
            <p class="kx-eyebrow">First session on us</p>
            <h2>Come see what<br>the room feels like.</h2>
            <p>Pick a class, meet a coach, use the floor. No commitment until you know it fits.</p>
            <div class="final-actions">
              <a class="kx-btn kx-btn-dark" routerLink="/trial">
                Book a free trial
                <span class="material-icons" aria-hidden="true">arrow_forward</span>
              </a>
              <a class="final-phone" [href]="'tel:' + site.phone">{{ site.phone }}</a>
            </div>
          </div>
        </section>
      </div>
    } @else {
      <div class="kx-loading" data-testid="landing-loading">
        <div class="kx-skeleton skeleton-hero"></div>
        <div class="kx-skeleton skeleton-row"></div>
      </div>
    }
  `,
  styles: [PUBLIC_PAGE_STYLES, `
    .landing { overflow: hidden; }

    .hero {
      position: relative;
      overflow: hidden;
      background: var(--kx-night);
      color: var(--kx-cream);
      padding-block: 64px 0;
    }

    .hero-noise {
      position: absolute;
      inset: 0;
      opacity: .24;
      background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 180 180' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='.9' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)' opacity='.16'/%3E%3C/svg%3E");
      pointer-events: none;
    }

    .hero::before {
      content: '';
      position: absolute;
      top: -320px;
      right: -140px;
      width: 780px;
      height: 780px;
      border-radius: 999px;
      background: radial-gradient(circle, rgba(240, 79, 36, .25), transparent 64%);
      pointer-events: none;
    }

    .hero-grid {
      position: relative;
      z-index: 1;
      display: grid;
      grid-template-columns: 1.06fr .94fr;
      gap: clamp(40px, 6vw, 92px);
      align-items: center;
      min-height: 610px;
    }

    .hero-copy { padding-bottom: 22px; }

    .hero-kicker {
      display: flex;
      align-items: center;
      gap: 13px;
      margin: 0 0 30px;
      color: rgba(251, 246, 238, .48);
      font-size: 10px;
      font-weight: 650;
      letter-spacing: .2em;
      text-transform: uppercase;
    }

    .hero-title {
      display: flex;
      flex-direction: column;
      margin: 0;
      font-size: clamp(58px, 6.5vw, 96px);
      font-weight: 850;
      letter-spacing: -.065em;
      line-height: .9;
    }

    .hero-title-accent {
      color: var(--kx-ember);
      font-style: italic;
      font-weight: 780;
      letter-spacing: -.06em;
    }

    .hero-lede {
      max-width: 46ch;
      margin: 28px 0 0;
      color: rgba(251, 246, 238, .58);
      font-size: 15px;
      line-height: 1.75;
    }

    .hero-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
      margin-top: 32px;
    }

    .hero-ghost {
      border-color: rgba(251, 246, 238, .24);
      color: var(--kx-cream);
    }

    .hero-ghost:hover {
      border-color: var(--kx-cream);
      color: var(--kx-cream);
      background: rgba(255, 255, 255, .06);
    }

    .hero-proof {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-top: 42px;
      padding-top: 25px;
      border-top: 1px solid rgba(255, 255, 255, .1);
    }

    .proof-faces { display: flex; padding-left: 8px; }

    .proof-faces span {
      display: inline-flex;
      width: 32px;
      height: 32px;
      align-items: center;
      justify-content: center;
      margin-left: -8px;
      border: 2px solid var(--kx-night);
      border-radius: 999px;
      background: #3b3027;
      color: rgba(251, 246, 238, .8);
      font-size: 8px;
      font-weight: 750;
    }

    .proof-faces span:nth-child(2) { background: #56392c; }
    .proof-faces span:nth-child(3) { background: #423d2c; }
    .proof-faces span:nth-child(4) { background: var(--kx-ember); color: #fff; }

    .proof-stars { display: flex; gap: 2px; color: var(--kx-acid); }
    .proof-stars .material-icons { font-size: 12px; }

    .hero-proof p {
      margin: 3px 0 0;
      color: rgba(251, 246, 238, .42);
      font-size: 10px;
    }

    .hero-visual {
      position: relative;
      width: 100%;
      max-width: 520px;
      justify-self: end;
      padding: 18px 16px 22px 0;
    }

    .stage-card {
      position: relative;
      overflow: hidden;
      aspect-ratio: 4 / 4.65;
      border: 1px solid rgba(255, 255, 255, .12);
      border-radius: 30px;
      background:
        radial-gradient(circle at 76% 18%, rgba(240, 79, 36, .36), transparent 32%),
        radial-gradient(circle at 8% 88%, rgba(216, 239, 120, .1), transparent 30%),
        linear-gradient(155deg, #2d231b 0%, #16120f 58%, #0b0a09 100%);
      box-shadow: 0 44px 90px rgba(0, 0, 0, .4);
    }

    .stage-grid {
      position: absolute;
      inset: 0;
      opacity: .26;
      background-image:
        linear-gradient(rgba(255, 255, 255, .12) 1px, transparent 1px),
        linear-gradient(90deg, rgba(255, 255, 255, .12) 1px, transparent 1px);
      background-size: 42px 42px;
      mask-image: linear-gradient(to bottom, #000, transparent 78%);
    }

    .stage-ring {
      position: absolute;
      border: 1px solid rgba(240, 79, 36, .4);
      border-radius: 999px;
    }

    .ring-one {
      width: 420px;
      height: 420px;
      top: 58px;
      left: -70px;
    }

    .ring-two {
      width: 560px;
      height: 560px;
      top: -14px;
      left: -140px;
      border-color: rgba(251, 246, 238, .1);
    }

    .stage-bar {
      position: absolute;
      width: 3px;
      border-radius: 99px;
      background: var(--kx-ember);
      transform: rotate(24deg);
      opacity: .8;
    }

    .bar-one { top: 95px; right: 78px; height: 190px; }
    .bar-two { top: 120px; right: 98px; height: 90px; background: var(--kx-acid); opacity: .55; }

    .stage-mark {
      position: absolute;
      left: 18px;
      bottom: 78px;
      color: rgba(251, 246, 238, .055);
      font-size: 330px;
      font-weight: 850;
      letter-spacing: -.12em;
      line-height: .7;
    }

    .stage-vertical {
      position: absolute;
      top: 28px;
      left: 20px;
      color: rgba(251, 246, 238, .28);
      font-size: 8px;
      font-weight: 700;
      letter-spacing: .2em;
      writing-mode: vertical-rl;
    }

    .stage-live {
      position: absolute;
      top: 24px;
      right: 24px;
      display: inline-flex;
      align-items: center;
      gap: 9px;
      padding: 9px 13px;
      border: 1px solid rgba(255, 255, 255, .13);
      border-radius: 999px;
      background: rgba(18, 16, 14, .55);
      color: rgba(251, 246, 238, .78);
      font-size: 9px;
      font-weight: 650;
      letter-spacing: .06em;
      backdrop-filter: blur(12px);
    }

    .stage-live .kx-dot { width: 6px; height: 6px; box-shadow: none; background: var(--kx-acid); }

    .stage-label {
      position: absolute;
      display: flex;
      flex-direction: column;
      gap: 3px;
      padding: 12px 14px;
      border: 1px solid rgba(255, 255, 255, .1);
      border-radius: 14px;
      background: rgba(18, 16, 14, .45);
      backdrop-filter: blur(10px);
    }

    .stage-label span { color: rgba(251, 246, 238, .38); font-size: 8px; text-transform: uppercase; letter-spacing: .12em; }
    .stage-label strong { color: var(--kx-cream); font-size: 17px; font-weight: 750; }
    .label-top { top: 78px; right: 24px; }
    .label-mid { right: 24px; top: 190px; }
    .label-mid strong { color: var(--kx-ember); font-size: 28px; letter-spacing: -.04em; }

    .stage-hud {
      position: absolute;
      right: 18px;
      bottom: 18px;
      left: 18px;
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 14px;
      border: 1px solid rgba(255, 255, 255, .13);
      border-radius: 17px;
      background: rgba(255, 250, 242, .93);
      color: var(--kx-ink);
      box-shadow: 0 16px 44px rgba(0, 0, 0, .35);
    }

    .hud-icon {
      display: inline-flex;
      width: 38px;
      height: 38px;
      align-items: center;
      justify-content: center;
      flex: none;
      border-radius: 11px;
      background: var(--kx-ember);
      color: #fff8ef;
    }

    .hud-copy { display: flex; flex: 1; flex-direction: column; min-width: 0; }
    .hud-copy span { color: #8b8174; font-size: 8px; font-weight: 700; letter-spacing: .12em; text-transform: uppercase; }
    .hud-copy strong { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: 13px; }
    .hud-copy small { color: #8b8174; font-size: 9px; }
    .hud-arrow { color: #8b8174; font-size: 18px; }

    .stage-float {
      position: absolute;
      z-index: 2;
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 11px 14px;
      border: 1px solid rgba(23, 20, 15, .08);
      border-radius: 15px;
      background: var(--kx-paper);
      color: var(--kx-ink);
      box-shadow: 0 16px 35px rgba(0, 0, 0, .24);
    }

    .stage-float .material-icons { color: var(--kx-ember); font-size: 20px; }
    .stage-float div { display: flex; flex-direction: column; }
    .stage-float strong { font-size: 13px; line-height: 1.2; }
    .stage-float small { color: var(--kx-muted); font-size: 8px; }

    .float-top { top: 80px; left: -26px; }
    .float-bottom { bottom: 84px; left: -34px; }

    .float-score {
      display: inline-flex;
      width: 35px;
      height: 35px;
      align-items: center;
      justify-content: center;
      border-radius: 10px;
      background: var(--kx-night);
      color: var(--kx-acid);
      font-size: 15px;
      font-weight: 800;
    }

    .hero-stats {
      position: relative;
      z-index: 1;
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      margin-top: 28px;
      border-top: 1px solid rgba(255, 255, 255, .11);
    }

    .hero-stat {
      display: flex;
      align-items: baseline;
      gap: 12px;
      padding: 27px 20px 32px;
      border-right: 1px solid rgba(255, 255, 255, .09);
    }

    .hero-stat:first-child { padding-left: 0; }
    .hero-stat:last-child { border-right: 0; }
    .hero-stat-value { color: var(--kx-cream); font-size: 25px; font-weight: 750; letter-spacing: -.04em; }
    .hero-stat-value small { margin-left: 2px; color: rgba(251, 246, 238, .36); font-size: 10px; }
    .hero-stat-label { color: rgba(251, 246, 238, .36); font-size: 9px; font-weight: 550; letter-spacing: .04em; }

    .intro-section { background: var(--kx-paper); }

    .intro-grid {
      display: grid;
      grid-template-columns: .9fr 1.1fr;
      gap: clamp(48px, 8vw, 110px);
      align-items: start;
    }

    .intro-heading { position: relative; }
    .intro-heading .kx-section-title { margin-top: 20px; }

    .accent-serif {
      color: var(--kx-ember);
      font-family: Georgia, 'Times New Roman', serif;
      font-style: italic;
      font-weight: 500;
      letter-spacing: -.055em;
    }

    .intro-stamp {
      display: inline-flex;
      width: 87px;
      height: 87px;
      align-items: center;
      justify-content: center;
      gap: 5px;
      margin-top: 34px;
      border: 1px solid var(--kx-line);
      border-radius: 999px;
      color: var(--kx-muted);
      transform: rotate(-8deg);
    }

    .intro-stamp span { color: var(--kx-ember); font-size: 28px; font-weight: 850; line-height: 1; }
    .intro-stamp small { font-size: 7px; font-weight: 700; letter-spacing: .12em; line-height: 1.5; }

    .intro-lead {
      margin: 0;
      color: var(--kx-ink);
      font-size: clamp(18px, 1.7vw, 22px);
      font-weight: 500;
      line-height: 1.55;
      letter-spacing: -.02em;
    }

    .intro-points {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1px;
      margin-top: 34px;
      background: var(--kx-line);
      border: 1px solid var(--kx-line);
    }

    .intro-point {
      display: flex;
      min-height: 92px;
      flex-direction: column;
      justify-content: space-between;
      gap: 16px;
      padding: 17px;
      background: var(--kx-surface);
      font-size: 11px;
      font-weight: 650;
      line-height: 1.45;
    }

    .point-number { color: var(--kx-ember); font-size: 9px; font-weight: 750; letter-spacing: .12em; }

    .intro-actions {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 20px;
      margin-top: 30px;
      flex-wrap: wrap;
    }

    .intro-note { color: var(--kx-muted); font-size: 9px; }

    .feature-band { background: var(--kx-paper-deep); padding-block: 26px; }

    .feature-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
    }

    .feature-card {
      display: flex;
      min-height: 260px;
      flex-direction: column;
      padding: 27px;
      border: 1px solid var(--kx-line);
      border-radius: 22px;
      background: var(--kx-surface);
    }

    .feature-card-warm { background: #ece1d2; }

    .feature-top {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      margin-bottom: auto;
    }

    .feature-icon {
      display: inline-flex;
      width: 44px;
      height: 44px;
      align-items: center;
      justify-content: center;
      border-radius: 13px;
      background: var(--kx-ink);
      color: var(--kx-ember);
    }

    .feature-index { color: var(--kx-muted); font-size: 9px; font-weight: 700; letter-spacing: .1em; }
    .feature-card h3 { margin: 34px 0 8px; font-size: 19px; font-weight: 750; letter-spacing: -.03em; }
    .feature-card p { margin: 0; color: var(--kx-muted); font-size: 11px; line-height: 1.65; }

    .plans-section { background: var(--kx-paper); }

    .section-head {
      display: grid;
      grid-template-columns: 1fr .74fr;
      gap: 60px;
      align-items: end;
      margin-bottom: 44px;
    }

    .section-head .kx-section-title { margin-top: 18px; }
    .section-side { display: flex; flex-direction: column; gap: 18px; align-items: flex-start; }
    .section-side .kx-lede { font-size: 13px; }

    .plan-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
      align-items: stretch;
    }

    .plan-card {
      position: relative;
      display: flex;
      flex-direction: column;
      padding: 30px 27px 26px;
      border: 1px solid var(--kx-line);
      border-radius: 24px;
      background: var(--kx-surface);
    }

    .plan-featured {
      border-color: var(--kx-night);
      background: var(--kx-night);
      color: var(--kx-cream);
      transform: translateY(-10px);
      box-shadow: 0 28px 55px rgba(23, 20, 15, .16);
    }

    .plan-badge {
      position: absolute;
      top: 18px;
      right: 18px;
      padding: 5px 10px;
      border-radius: 999px;
      background: var(--kx-ember);
      color: #fff8ef;
      font-size: 8px;
      font-weight: 700;
      letter-spacing: .08em;
      text-transform: uppercase;
    }

    .plan-tier {
      color: var(--kx-ember);
      font-size: 9px;
      font-weight: 750;
      letter-spacing: .14em;
      text-transform: uppercase;
    }

    .plan-top h3 {
      margin: 9px 0 10px;
      font-size: 28px;
      font-weight: 780;
      letter-spacing: -.045em;
      line-height: 1.1;
    }

    .plan-top p { min-height: 56px; margin: 0; color: var(--kx-muted); font-size: 11px; line-height: 1.6; }
    .plan-featured .plan-top p { color: rgba(251, 246, 238, .5); }

    .plan-price {
      display: flex;
      align-items: baseline;
      gap: 8px;
      margin: 26px 0 22px;
      padding-bottom: 22px;
      border-bottom: 1px solid var(--kx-line);
    }

    .plan-featured .plan-price { border-color: rgba(255, 255, 255, .14); }
    .plan-amount { font-size: 40px; font-weight: 780; letter-spacing: -.06em; line-height: 1; }
    .plan-period { color: var(--kx-muted); font-size: 9px; }
    .plan-featured .plan-period { color: rgba(251, 246, 238, .4); }

    .plan-features {
      display: flex;
      flex: 1;
      flex-direction: column;
      gap: 11px;
      margin: 0 0 27px;
      padding: 0;
      list-style: none;
    }

    .plan-features li {
      display: flex;
      align-items: flex-start;
      gap: 9px;
      color: var(--kx-muted);
      font-size: 10px;
      line-height: 1.45;
    }

    .plan-featured .plan-features li { color: rgba(251, 246, 238, .62); }
    .plan-features .material-icons { flex: none; color: var(--kx-ember); font-size: 15px; margin-top: 1px; }

    .plan-button {
      border-color: var(--kx-line);
      color: var(--kx-ink);
      background: transparent;
    }

    .plan-button:hover { border-color: var(--kx-ink); }

    .plan-assurance {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      margin-top: 30px;
      padding-top: 24px;
      border-top: 1px solid var(--kx-line);
    }

    .plan-assurance span {
      display: inline-flex;
      align-items: center;
      gap: 9px;
      color: var(--kx-muted);
      font-size: 9px;
      font-weight: 600;
    }

    .plan-assurance .material-icons { color: var(--kx-ember); font-size: 16px; }

    .schedule-band { background: var(--kx-night); color: var(--kx-cream); padding-block: clamp(72px, 8vw, 108px); }

    .schedule-grid {
      display: grid;
      grid-template-columns: .72fr 1.28fr;
      gap: clamp(48px, 8vw, 100px);
      align-items: center;
    }

    .schedule-copy .kx-section-title { margin-top: 18px; }
    .schedule-lede { max-width: 38ch; margin: 22px 0 27px; color: rgba(251, 246, 238, .48); font-size: 12px; line-height: 1.7; }

    .schedule-btn {
      border-color: rgba(251, 246, 238, .22);
      color: var(--kx-cream);
    }

    .schedule-btn:hover { border-color: var(--kx-cream); background: rgba(255, 255, 255, .06); }

    .schedule-branches {
      display: flex;
      gap: 17px;
      margin-top: 35px;
      padding-top: 19px;
      border-top: 1px solid rgba(255, 255, 255, .1);
    }

    .schedule-branches span { color: rgba(251, 246, 238, .28); font-size: 8px; font-weight: 650; letter-spacing: .06em; text-transform: uppercase; }

    .schedule-list { display: flex; flex-direction: column; }

    .schedule-item {
      position: relative;
      display: grid;
      grid-template-columns: 86px 1fr auto;
      gap: 22px;
      align-items: center;
      padding: 22px 10px 22px 0;
      border-top: 1px solid rgba(255, 255, 255, .1);
      transition: padding .2s ease, background .2s ease;
    }

    .schedule-item:last-child { border-bottom: 1px solid rgba(255, 255, 255, .1); }
    .schedule-item:hover { padding-left: 8px; background: rgba(255, 255, 255, .025); }

    .schedule-date { display: flex; flex-direction: column; }
    .schedule-date span { color: var(--kx-ember); font-size: 8px; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; }
    .schedule-date strong { color: var(--kx-cream); font-size: 11px; font-weight: 600; }

    .schedule-main { min-width: 0; }
    .schedule-category { color: rgba(251, 246, 238, .28); font-size: 7px; font-weight: 700; letter-spacing: .14em; text-transform: uppercase; }
    .schedule-main h3 { margin: 3px 0 3px; color: var(--kx-cream); font-size: 17px; font-weight: 700; letter-spacing: -.02em; }
    .schedule-main p { margin: 0; color: rgba(251, 246, 238, .37); font-size: 9px; }

    .schedule-time { display: flex; flex-direction: column; align-items: flex-end; }
    .schedule-time strong { color: var(--kx-cream); font-size: 13px; font-weight: 650; }
    .schedule-time span { color: rgba(251, 246, 238, .32); font-size: 8px; }
    .schedule-index { position: absolute; right: -2px; bottom: 5px; color: rgba(255, 255, 255, .06); font-size: 31px; font-weight: 800; }

    .coaches-section { background: var(--kx-paper); }

    .coach-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; }

    .coach-card {
      overflow: hidden;
      border: 1px solid var(--kx-line);
      border-radius: 22px;
      background: var(--kx-surface);
    }

    .coach-portrait {
      position: relative;
      overflow: hidden;
      display: flex;
      aspect-ratio: 1 / .93;
      align-items: center;
      justify-content: center;
      background: linear-gradient(145deg, #ddc9b1, #8e7056);
    }

    .coach-portrait::after {
      content: '';
      position: absolute;
      inset: 0;
      background-image: linear-gradient(rgba(255,255,255,.1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,.1) 1px, transparent 1px);
      background-size: 34px 34px;
      mask-image: linear-gradient(to bottom, #000, transparent);
    }

    .tone-1 { background: linear-gradient(145deg, #b7c5c2, #506d6a); }
    .tone-2 { background: linear-gradient(145deg, #d8ccae, #77704d); }
    .tone-3 { background: linear-gradient(145deg, #d0b5ae, #804e43); }

    .coach-ring {
      position: absolute;
      width: 210px;
      height: 210px;
      border: 1px solid rgba(255, 255, 255, .25);
      border-radius: 999px;
    }

    .coach-initials {
      position: relative;
      z-index: 1;
      color: rgba(255, 250, 242, .88);
      font-size: 60px;
      font-weight: 800;
      letter-spacing: -.08em;
      text-shadow: 0 8px 30px rgba(0, 0, 0, .18);
    }

    .coach-number { position: absolute; top: 14px; left: 16px; color: rgba(255, 255, 255, .55); font-size: 8px; font-weight: 700; letter-spacing: .1em; }
    .coach-years { position: absolute; right: 14px; bottom: 13px; padding: 5px 9px; border-radius: 999px; background: rgba(18, 16, 14, .6); color: var(--kx-cream); font-size: 8px; font-weight: 650; backdrop-filter: blur(8px); }

    .coach-body { padding: 18px 18px 19px; }
    .coach-body h3 { margin: 0; font-size: 15px; font-weight: 750; letter-spacing: -.025em; }
    .coach-role { display: block; margin-top: 2px; color: var(--kx-ember); font-size: 8px; font-weight: 650; letter-spacing: .03em; }
    .coach-body p { display: -webkit-box; overflow: hidden; margin: 12px 0 14px; color: var(--kx-muted); font-size: 9px; line-height: 1.6; -webkit-box-orient: vertical; -webkit-line-clamp: 3; }
    .coach-tags { display: flex; flex-wrap: wrap; gap: 5px; }
    .coach-tags span { padding: 4px 7px; border: 1px solid var(--kx-line); border-radius: 999px; color: var(--kx-muted); font-size: 7px; font-weight: 600; }

    .faq-section { background: var(--kx-paper-deep); }

    .faq-grid {
      display: grid;
      grid-template-columns: .74fr 1.26fr;
      gap: clamp(50px, 8vw, 110px);
      align-items: start;
    }

    .faq-copy .kx-section-title { margin: 18px 0 22px; }
    .faq-copy .kx-lede { margin-bottom: 28px; font-size: 12px; }

    .faq-list { border-top: 1px solid var(--kx-line); }

    .faq-item { border-bottom: 1px solid var(--kx-line); }

    .faq-item summary {
      display: flex;
      min-height: 76px;
      align-items: center;
      gap: 18px;
      padding: 13px 2px;
      cursor: pointer;
      list-style: none;
      font-size: 13px;
      font-weight: 650;
      letter-spacing: -.01em;
    }

    .faq-item summary::-webkit-details-marker { display: none; }
    .faq-count { color: var(--kx-ember); font-size: 8px; font-weight: 750; letter-spacing: .1em; }
    .faq-toggle { margin-left: auto; color: var(--kx-muted); transition: transform .2s ease; }
    .faq-item[open] .faq-toggle { transform: rotate(45deg); color: var(--kx-ember); }

    .faq-item p {
      max-width: 62ch;
      margin: -3px 0 0 35px;
      padding-bottom: 23px;
      color: var(--kx-muted);
      font-size: 11px;
      line-height: 1.7;
    }

    .final-cta {
      position: relative;
      overflow: hidden;
      display: grid;
      grid-template-columns: .78fr 1.22fr;
      min-height: 370px;
      margin-block: 72px;
      border-radius: 28px;
      background: var(--kx-ember);
      color: #fff8ef;
    }

    .final-cta-visual {
      position: relative;
      overflow: hidden;
      border-right: 1px solid rgba(255, 255, 255, .18);
      background: linear-gradient(145deg, rgba(0, 0, 0, .06), rgba(0, 0, 0, .18));
    }

    .cta-word {
      position: absolute;
      left: 28px;
      bottom: 20px;
      color: rgba(255, 248, 239, .18);
      font-size: 100px;
      font-weight: 850;
      letter-spacing: -.09em;
      line-height: .8;
    }

    .cta-ring { position: absolute; top: 38px; right: 30px; width: 180px; height: 180px; border: 1px solid rgba(255, 248, 239, .28); border-radius: 999px; }
    .cta-ring-two { top: 75px; right: 67px; width: 105px; height: 105px; border-color: rgba(255, 248, 239, .16); }

    .final-cta-copy { padding: clamp(34px, 5vw, 58px); }
    .final-cta .kx-eyebrow { color: rgba(255, 248, 239, .7); }
    .final-cta h2 { margin: 20px 0 15px; font-size: clamp(34px, 4vw, 52px); font-weight: 800; letter-spacing: -.055em; line-height: 1; }
    .final-cta p { max-width: 46ch; margin: 0; color: rgba(255, 248, 239, .72); font-size: 12px; line-height: 1.7; }

    .final-actions { display: flex; align-items: center; gap: 24px; margin-top: 29px; flex-wrap: wrap; }
    .final-phone { color: rgba(255, 248, 239, .8); font-size: 11px; font-weight: 650; text-decoration: none; border-bottom: 1px solid rgba(255, 248, 239, .35); padding-bottom: 4px; }
    .final-phone:hover { color: #fff; }

    .skeleton-hero { height: 650px; }
    .skeleton-row { height: 200px; }

    @media (max-width: 1080px) {
      .hero-grid { gap: 40px; }
      .float-top { left: -8px; }
      .float-bottom { left: -8px; }
      .coach-grid { grid-template-columns: repeat(2, 1fr); }
      .feature-grid { grid-template-columns: 1fr 1fr; }
      .feature-card:last-child { grid-column: 1 / -1; min-height: 190px; }
    }

    @media (max-width: 900px) {
      .hero-grid { grid-template-columns: 1fr; min-height: 0; }
      .hero-copy { padding-top: 18px; }
      .hero-visual { max-width: 560px; justify-self: start; padding-right: 0; margin-top: 14px; }
      .hero-stats { grid-template-columns: 1fr 1fr; }
      .hero-stat { border-bottom: 1px solid rgba(255, 255, 255, .09); }
      .hero-stat:nth-child(2) { border-right: 0; }
      .hero-stat:nth-child(3) { padding-left: 0; border-bottom: 0; }
      .hero-stat:nth-child(4) { border-bottom: 0; }
      .intro-grid, .schedule-grid, .faq-grid { grid-template-columns: 1fr; gap: 44px; }
      .section-head { grid-template-columns: 1fr; gap: 24px; }
      .plan-grid { grid-template-columns: 1fr 1fr; }
      .plan-featured { transform: none; }
      .plan-grid .plan-card:last-child { grid-column: 1 / -1; }
      .plan-assurance { grid-template-columns: 1fr 1fr; gap: 14px; }
      .final-cta { grid-template-columns: 1fr; }
      .final-cta-visual { min-height: 160px; border-right: 0; border-bottom: 1px solid rgba(255, 255, 255, .18); }
      .cta-word { font-size: 74px; }
    }

    @media (max-width: 680px) {
      .hero { padding-top: 34px; }
      .hero-title { font-size: clamp(49px, 15vw, 72px); }
      .hero-visual { padding-top: 4px; }
      .stage-card { border-radius: 22px; aspect-ratio: 4 / 4.8; }
      .stage-mark { font-size: 230px; }
      .float-top { top: 30px; left: -4px; }
      .float-bottom { bottom: 76px; left: -4px; }
      .label-top { top: 22px; right: 16px; }
      .label-mid { display: none; }
      .stage-live { top: 22px; right: 16px; transform: translateY(-52px); }
      .hero-stats { margin-top: 18px; }
      .hero-stat { flex-direction: column; gap: 2px; padding: 18px 12px; }
      .hero-stat:first-child { padding-left: 0; }
      .hero-stat-value { font-size: 22px; }
      .hero-proof { margin-top: 30px; }
      .feature-grid { grid-template-columns: 1fr; }
      .feature-card, .feature-card:last-child { grid-column: auto; min-height: 220px; }
      .intro-points { grid-template-columns: 1fr; }
      .plan-grid { grid-template-columns: 1fr; }
      .plan-grid .plan-card:last-child { grid-column: auto; }
      .plan-assurance { grid-template-columns: 1fr; }
      .schedule-item { grid-template-columns: 66px 1fr; gap: 14px; }
      .schedule-time { grid-column: 2; flex-direction: row; align-items: center; gap: 8px; }
      .coach-grid { grid-template-columns: 1fr; }
      .coach-portrait { aspect-ratio: 1.65 / 1; }
      .faq-item summary { gap: 12px; font-size: 12px; }
      .faq-item p { margin-left: 0; }
      .final-cta { border-radius: 22px; margin-block: 48px; }
      .final-cta-copy { padding: 28px 24px 32px; }
      .final-cta h2 { font-size: 36px; }
    }
  `],
})
export class PublicLandingComponent {
  readonly facade = inject(PublicSiteFacade);
  protected readonly formatMoney = formatMoney;
  protected readonly formatShortDate = formatShortDate;
  protected readonly formatTime = formatTime;
  protected readonly initials = initials;

  formatCount(value: number): string {
    return value.toLocaleString('en-US');
  }
}
