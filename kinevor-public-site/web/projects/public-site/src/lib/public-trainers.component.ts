import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PublicSiteFacade } from './public-site.facade';
import { PUBLIC_PAGE_STYLES } from './public-page.styles';
import { initials } from './public-site.utils';

@Component({
  selector: 'kinevor-public-trainers',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (facade.site(); as site) {
      <div class="trainers-page" data-testid="trainers-page">
        <section class="trainer-hero">
          <div class="hero-grid-lines" aria-hidden="true"></div>
          <div class="kx-container trainer-hero-grid">
            <div class="hero-copy">
              <p class="kx-eyebrow">Coaching at Kinevor</p>
              <h1 class="page-title">
                Experts in<br>
                movement.<br>
                <span>Students of people.</span>
              </h1>
              <p class="hero-lede">
                Our coaches do not hand out generic programmes. They watch, listen, adjust and
                celebrate the small wins that become big ones.
              </p>
              <div class="hero-actions">
                <a class="kx-btn kx-btn-ember" routerLink="/trial">
                  Train with us
                  <span class="material-icons" aria-hidden="true">arrow_forward</span>
                </a>
                <a class="kx-btn hero-ghost" routerLink="/schedule">See class times</a>
              </div>
            </div>

            <div class="hero-aside" aria-hidden="true">
              <span class="aside-label">The coaching floor / NYC</span>
              <div class="aside-stack">
                <span class="stack-number">{{ site.trainerCount }}</span>
                <span class="stack-caption">certified coaches<br>across three gyms</span>
              </div>
              <div class="aside-metrics">
                <div><strong>11</strong><small>avg. years</small></div>
                <div><strong>L2+</strong><small>certified</small></div>
                <div><strong>1:1</strong><small>onboarding</small></div>
              </div>
            </div>
          </div>
        </section>

        <section class="coaches">
          <div class="kx-container">
            <div class="coaches-head">
              <span>Featured coaches</span>
              <span>Four disciplines, one language</span>
            </div>

            <div class="coach-list">
              @for (trainer of facade.trainers(); track trainer.id; let index = $index) {
                <article class="coach-row">
                  <div class="coach-visual" [class]="'coach-visual tone-' + (index % 4)">
                    <span class="visual-grid" aria-hidden="true"></span>
                    <span class="visual-ring" aria-hidden="true"></span>
                    <span class="coach-initials">{{ initials(trainer.fullName) }}</span>
                    <span class="coach-index">0{{ index + 1 }}</span>
                    <span class="coach-experience">{{ trainer.yearsExperience }} years coaching</span>
                  </div>

                  <div class="coach-content">
                    <div class="coach-title-row">
                      <div>
                        <span class="coach-role">{{ trainer.roleTitle }}</span>
                        <h2>{{ trainer.fullName }}</h2>
                      </div>
                      <span class="coach-branch">
                        <span class="material-icons" aria-hidden="true">location_on</span>
                        {{ trainer.branchName ?? 'All locations' }}
                      </span>
                    </div>

                    <p class="coach-bio">{{ trainer.bio }}</p>

                    <div class="coach-detail-grid">
                      <div>
                        <span class="detail-label">Specialities</span>
                        <div class="detail-tags">
                          @for (specialty of trainer.specializations; track specialty) {
                            <span>{{ specialty }}</span>
                          }
                        </div>
                      </div>
                      <div>
                        <span class="detail-label">Certifications</span>
                        <div class="cert-list">
                          @for (certification of trainer.certifications; track certification) {
                            <span><span class="material-icons" aria-hidden="true">verified</span>{{ certification }}</span>
                          }
                        </div>
                      </div>
                    </div>
                  </div>
                </article>
              }
            </div>
          </div>
        </section>

        <section class="method">
          <div class="kx-container">
            <div class="method-head">
              <p class="kx-eyebrow">How coaching works</p>
              <h2 class="kx-section-title">A relationship,<br>not a template.</h2>
              <p class="kx-lede">Your first month follows a deliberate rhythm. It stays personal even when the room is full.</p>
            </div>

            <div class="method-grid">
              <article>
                <span class="method-number">01</span>
                <h3>Listen first</h3>
                <p>A movement screen and a proper conversation about history, goals and the week you actually live.</p>
              </article>
              <article>
                <span class="method-number">02</span>
                <h3>Build the base</h3>
                <p>Technique, capacity and confidence come before load. Your coach makes the early sessions feel repeatable.</p>
              </article>
              <article>
                <span class="method-number">03</span>
                <h3>Keep adjusting</h3>
                <p>Monthly check-ins and quarterly reviews keep the plan honest as sleep, work and life change.</p>
              </article>
            </div>

            <div class="method-quote">
              <span class="quote-mark" aria-hidden="true">&ldquo;</span>
              <blockquote>
                The best session is the one you leave feeling more capable than when you walked in.
              </blockquote>
              <span class="quote-credit">Kinevor coaching standard</span>
            </div>
          </div>
        </section>

        <section class="trainer-cta">
          <div class="kx-container trainer-cta-inner">
            <div>
              <p class="kx-eyebrow">Start with a conversation</p>
              <h2>Your first session<br>comes with a coach.</h2>
            </div>
            <div class="cta-side">
              <p>Tell us what you want to change. We will match you with the right coach and class, then meet you at the door.</p>
              <a class="kx-btn kx-btn-dark" routerLink="/trial">
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
    .trainers-page { overflow: hidden; }

    .trainer-hero {
      position: relative;
      overflow: hidden;
      background: var(--kx-night);
      color: var(--kx-cream);
      padding-block: 72px 78px;
    }

    .trainer-hero::before {
      content: '';
      position: absolute;
      bottom: -270px;
      left: -170px;
      width: 640px;
      height: 640px;
      border-radius: 999px;
      background: radial-gradient(circle, rgba(240, 79, 36, .18), transparent 66%);
    }

    .hero-grid-lines {
      position: absolute;
      inset: 0;
      opacity: .16;
      background-image:
        linear-gradient(rgba(255, 255, 255, .2) 1px, transparent 1px),
        linear-gradient(90deg, rgba(255, 255, 255, .2) 1px, transparent 1px);
      background-size: 58px 58px;
      mask-image: linear-gradient(to right, #000, transparent 72%);
    }

    .trainer-hero-grid {
      position: relative;
      display: grid;
      grid-template-columns: 1.22fr .78fr;
      gap: clamp(50px, 8vw, 120px);
      align-items: end;
    }

    .page-title {
      margin: 21px 0 0;
      font-size: clamp(52px, 6.2vw, 88px);
      font-weight: 840;
      letter-spacing: -.067em;
      line-height: .92;
    }

    .page-title span {
      color: var(--kx-ember);
      font-family: Georgia, 'Times New Roman', serif;
      font-style: italic;
      font-weight: 500;
    }

    .hero-lede { max-width: 48ch; margin: 26px 0 0; color: rgba(251, 246, 238, .52); font-size: 12px; line-height: 1.75; }
    .hero-actions { display: flex; gap: 10px; flex-wrap: wrap; margin-top: 30px; }
    .hero-ghost { border-color: rgba(251, 246, 238, .22); color: var(--kx-cream); }
    .hero-ghost:hover { border-color: var(--kx-cream); color: var(--kx-cream); }

    .hero-aside {
      padding: 24px;
      border: 1px solid rgba(255, 255, 255, .12);
      border-radius: 22px;
      background: rgba(255, 255, 255, .035);
      backdrop-filter: blur(12px);
    }

    .aside-label { display: block; color: rgba(251, 246, 238, .3); font-size: 7px; font-weight: 700; letter-spacing: .17em; text-transform: uppercase; }
    .aside-stack { display: flex; align-items: flex-end; gap: 13px; margin-top: 27px; padding-bottom: 23px; border-bottom: 1px solid rgba(255, 255, 255, .1); }
    .stack-number { color: var(--kx-cream); font-size: 70px; font-weight: 820; letter-spacing: -.08em; line-height: .76; }
    .stack-caption { color: rgba(251, 246, 238, .38); font-size: 9px; line-height: 1.5; }

    .aside-metrics { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; margin-top: 20px; }
    .aside-metrics div { display: flex; flex-direction: column; gap: 2px; }
    .aside-metrics strong { color: var(--kx-acid); font-size: 16px; font-weight: 750; }
    .aside-metrics small { color: rgba(251, 246, 238, .32); font-size: 7px; }

    .coaches { padding-block: 42px 96px; background: var(--kx-paper); }

    .coaches-head {
      display: flex;
      justify-content: space-between;
      gap: 20px;
      padding-bottom: 17px;
      border-bottom: 1px solid var(--kx-line);
      color: var(--kx-muted);
      font-size: 9px;
      font-weight: 650;
      letter-spacing: .07em;
      text-transform: uppercase;
    }

    .coach-linear-list { display: flex; flex-direction: column; }
    .coach-row { display: grid; grid-template-columns: 310px 1fr; gap: 48px; padding-block: 40px; border-bottom: 1px solid var(--kx-line); }

    .coach-visual {
      position: relative;
      overflow: hidden;
      display: flex;
      min-height: 310px;
      align-items: center;
      justify-content: center;
      border-radius: 23px;
      background: linear-gradient(145deg, #ddc9b1, #8e7056);
    }

    .tone-1 { background: linear-gradient(145deg, #b7c5c2, #506d6a); }
    .tone-2 { background: linear-gradient(145deg, #d8ccae, #77704d); }
    .tone-3 { background: linear-gradient(145deg, #d0b5ae, #804e43); }

    .visual-grid { position: absolute; inset: 0; opacity: .9; background-image: linear-gradient(rgba(255,255,255,.1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,.1) 1px, transparent 1px); background-size: 38px 38px; }
    .visual-ring { position: absolute; width: 250px; height: 250px; border: 1px solid rgba(255, 255, 255, .28); border-radius: 999px; }
    .coach-initials { position: relative; z-index: 1; color: rgba(255, 250, 242, .9); font-size: 78px; font-weight: 800; letter-spacing: -.09em; text-shadow: 0 8px 34px rgba(0,0,0,.15); }
    .coach-index { position: absolute; top: 16px; left: 18px; color: rgba(255,255,255,.52); font-size: 8px; font-weight: 700; letter-spacing: .11em; }
    .coach-experience { position: absolute; right: 14px; bottom: 14px; padding: 5px 9px; border-radius: 999px; background: rgba(18,16,14,.6); color: var(--kx-cream); font-size: 7px; font-weight: 650; backdrop-filter: blur(8px); }

    .coach-content { display: flex; flex-direction: column; justify-content: center; }
    .coach-title-row { display: flex; align-items: flex-start; justify-content: space-between; gap: 20px; }
    .coach-role { color: var(--kx-ember); font-size: 8px; font-weight: 700; letter-spacing: .12em; text-transform: uppercase; }
    .coach-title-row h2 { margin: 6px 0 0; font-size: clamp(29px, 3vw, 39px); font-weight: 780; letter-spacing: -.05em; line-height: 1.05; }
    .coach-branch { display: inline-flex; align-items: center; gap: 5px; padding: 6px 10px; border: 1px solid var(--kx-line); border-radius: 999px; color: var(--kx-muted); font-size: 8px; white-space: nowrap; }
    .coach-branch .material-icons { color: var(--kx-ember); font-size: 13px; }

    .coach-bio { max-width: 65ch; margin: 20px 0 26px; color: var(--kx-muted); font-size: 11px; line-height: 1.75; }

    .coach-detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 26px; padding-top: 20px; border-top: 1px solid var(--kx-line); }
    .detail-label { display: block; margin-bottom: 9px; color: var(--kx-muted); font-size: 7px; font-weight: 700; letter-spacing: .12em; text-transform: uppercase; }
    .detail-tags { display: flex; flex-wrap: wrap; gap: 5px; }
    .detail-tags span { padding: 5px 9px; border-radius: 999px; background: var(--kx-paper-deep); color: var(--kx-ink); font-size: 8px; font-weight: 600; }
    .cert-list { display: flex; flex-direction: column; gap: 5px; }
    .cert-list span { display: inline-flex; align-items: center; gap: 6px; color: var(--kx-muted); font-size: 8px; }
    .cert-list .material-icons { color: var(--kx-ember); font-size: 13px; }

    .method { padding-block: 90px; background: var(--kx-paper-deep); }
    .method-head { display: grid; grid-template-columns: .9fr .55fr; gap: 60px; align-items: end; }
    .method-head .kx-section-title { margin-top: 18px; }
    .method-head .kx-lede { font-size: 11px; }

    .method-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 14px; margin-top: 44px; }
    .method-grid article { min-height: 235px; padding: 24px; border: 1px solid var(--kx-line); border-radius: 18px; background: var(--kx-surface); }
    .method-number { display: block; color: var(--kx-ember); font-size: 8px; font-weight: 750; letter-spacing: .12em; }
    .method-grid h3 { margin: 68px 0 9px; font-size: 18px; font-weight: 750; letter-spacing: -.025em; }
    .method-grid p { margin: 0; color: var(--kx-muted); font-size: 9px; line-height: 1.65; }

    .method-quote { display: grid; grid-template-columns: auto 1fr auto; gap: 24px; align-items: center; margin-top: 40px; padding: 31px 35px; border-radius: 20px; background: var(--kx-ink); color: var(--kx-cream); }
    .quote-mark { color: var(--kx-ember); font-family: Georgia, serif; font-size: 62px; line-height: .5; }
    .method-quote blockquote { max-width: 46ch; margin: 0; font-size: 15px; font-weight: 500; letter-spacing: -.015em; line-height: 1.5; }
    .quote-credit { color: rgba(251, 246, 238, .3); font-size: 8px; font-weight: 650; letter-spacing: .06em; text-transform: uppercase; white-space: nowrap; }

    .trainer-cta { padding-block: 76px; background: var(--kx-paper); }
    .trainer-cta-inner { display: grid; grid-template-columns: 1fr 1fr; gap: 70px; align-items: end; }
    .trainer-cta h2 { margin: 17px 0 0; font-size: clamp(35px, 4vw, 50px); font-weight: 800; letter-spacing: -.054em; line-height: 1.02; }
    .cta-side { display: flex; align-items: center; gap: 30px; }
    .cta-side p { max-width: 36ch; margin: 0; color: var(--kx-muted); font-size: 10px; line-height: 1.7; }

    .skeleton-title { width: 50%; height: 250px; }
    .skeleton-grid { height: 460px; }

    @media (max-width: 1000px) {
      .trainer-hero-grid { grid-template-columns: 1fr; gap: 38px; }
      .hero-aside { max-width: 420px; }
      .coach-row { grid-template-columns: 240px 1fr; gap: 30px; }
      .coach-visual { min-height: 280px; }
      .coach-detail-grid { grid-template-columns: 1fr; gap: 14px; }
    }

    @media (max-width: 750px) {
      .trainer-hero { padding-block: 48px 54px; }
      .page-title { font-size: clamp(45px, 12vw, 66px); }
      .coach-row { grid-template-columns: 1fr; gap: 24px; padding-block: 28px; }
      .coach-visual { min-height: 250px; }
      .coach-initials { font-size: 66px; }
      .coach-title-row { flex-direction: column; gap: 10px; }
      .method-head, .trainer-cta-inner { grid-template-columns: 1fr; gap: 28px; }
      .method-grid { grid-template-columns: 1fr; }
      .method-grid article { min-height: 180px; }
      .method-grid h3 { margin-top: 38px; }
      .method-quote { grid-template-columns: 1fr; gap: 12px; }
      .quote-mark { font-size: 46px; }
      .cta-side { flex-direction: column; align-items: flex-start; gap: 18px; }
    }

    @media (max-width: 560px) {
      .hero-actions .kx-btn { width: 100%; }
      .coaches { padding-bottom: 64px; }
      .coaches-head { flex-direction: column; gap: 5px; }
      .coach-detail-grid { padding-top: 16px; }
    }
  `],
})
export class PublicTrainersComponent {
  readonly facade = inject(PublicSiteFacade);
  protected readonly initials = initials;
}
