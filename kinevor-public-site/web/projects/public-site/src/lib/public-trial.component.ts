import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PUBLIC_SITE_GATEWAY, TrialCreated } from './public-site.models';
import { PublicSiteFacade } from './public-site.facade';
import { PUBLIC_PAGE_STYLES } from './public-page.styles';
import { formatShortDate, formatTime, toDateInputValue } from './public-site.utils';

@Component({
  selector: 'kinevor-public-trial',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (facade.site(); as site) {
      <div class="trial-page" data-testid="trial-page">
        <section class="trial-shell">
          <div class="kx-container trial-grid">
            <aside class="trial-aside">
              <div class="aside-orbit" aria-hidden="true"></div>
              <p class="kx-eyebrow">Your first session</p>
              <h1>Start here.<br><span>On us.</span></h1>
              <p class="aside-copy">
                One free class, one proper introduction to the floor, and a coach who learns how you
                move before recommending what comes next.
              </p>

              <div class="aside-benefits">
                <div>
                  <span class="benefit-index">01</span>
                  <div>
                    <strong>Pick a class or just come in</strong>
                    <p>Book a session from the timetable or let us recommend one for your first visit.</p>
                  </div>
                </div>
                <div>
                  <span class="benefit-index">02</span>
                  <div>
                    <strong>Meet a coach, not a salesperson</strong>
                    <p>A short movement screen and a conversation about what you want to change.</p>
                  </div>
                </div>
                <div>
                  <span class="benefit-index">03</span>
                  <div>
                    <strong>Leave with a clear next step</strong>
                    <p>No pressure and no lock-in. You decide if Kinevor is the right room for you.</p>
                  </div>
                </div>
              </div>

              <div class="aside-footer">
                <span class="material-icons" aria-hidden="true">schedule</span>
                <p>We reply within one business day. Prefer to talk? Call
                  <a [href]="'tel:' + site.phone">{{ site.phone }}</a>.
                </p>
              </div>
            </aside>

            <div class="trial-form-panel">
              @if (created(); as result) {
                <div class="trial-success" data-testid="trial-success">
                  <span class="success-ring">
                    <span class="material-icons" aria-hidden="true">check</span>
                  </span>
                  <p class="kx-eyebrow">Request received</p>
                  <h2>You are on<br>the list.</h2>
                  <p class="success-copy">
                    Thanks, {{ submittedName() }}. A Kinevor coach will email you within one business day
                    to confirm your first session and answer any questions.
                  </p>
                  <div class="reference">
                    <span>Reference</span>
                    <strong>{{ reference(result) }}</strong>
                  </div>
                  <div class="success-actions">
                    <a class="kx-btn kx-btn-ember" routerLink="/schedule">
                      Explore the timetable
                      <span class="material-icons" aria-hidden="true">arrow_forward</span>
                    </a>
                    <button type="button" class="text-button" (click)="startAnother()">Send another request</button>
                  </div>
                </div>
              } @else {
                <div class="form-heading">
                  <div>
                    <p class="kx-eyebrow">Free trial request</p>
                    <h2>Tell us a little<br>about you.</h2>
                  </div>
                  <span class="form-step">01 / 01</span>
                </div>

                <form [formGroup]="form" (ngSubmit)="submit()" data-testid="trial-form" novalidate>
                  <div class="field-row">
                    <label class="field">
                      <span>First name</span>
                      <input type="text" formControlName="firstName" autocomplete="given-name" placeholder="Alex" data-testid="trial-first-name">
                      @if (invalid('firstName')) {
                        <small>Please enter your first name.</small>
                      }
                    </label>
                    <label class="field">
                      <span>Last name</span>
                      <input type="text" formControlName="lastName" autocomplete="family-name" placeholder="Morgan" data-testid="trial-last-name">
                      @if (invalid('lastName')) {
                        <small>Please enter your last name.</small>
                      }
                    </label>
                  </div>

                  <div class="field-row">
                    <label class="field">
                      <span>Email</span>
                      <input type="email" formControlName="email" autocomplete="email" placeholder="alex@example.com" data-testid="trial-email">
                      @if (invalid('email')) {
                        <small>Enter a valid email address.</small>
                      }
                    </label>
                    <label class="field">
                      <span>Phone</span>
                      <input type="tel" formControlName="phone" autocomplete="tel" placeholder="+1 212 555 0140" data-testid="trial-phone">
                      @if (invalid('phone')) {
                        <small>Enter a phone number so we can confirm.</small>
                      }
                    </label>
                  </div>

                  <div class="field-row">
                    <label class="field">
                      <span>Preferred day</span>
                      <input type="date" formControlName="preferredDate" [min]="today" data-testid="trial-date">
                    </label>
                    <label class="field">
                      <span>Home gym</span>
                      <select formControlName="branchId" (change)="branchChanged()" data-testid="trial-branch">
                        <option value="">No preference</option>
                        @for (branch of facade.branches(); track branch.id) {
                          <option [value]="branch.id">{{ branch.name }} - {{ branch.neighbourhood }}</option>
                        }
                      </select>
                    </label>
                  </div>

                  <label class="field">
                    <span>Class you would like to try <em>Optional</em></span>
                    <select formControlName="classSessionId" data-testid="trial-class">
                      <option value="">Let a coach recommend one</option>
                      @for (session of availableSessions(); track session.id) {
                        <option [value]="session.id">
                          {{ session.name }} &middot; {{ formatShortDate(session.startsAt) }} at {{ formatTime(session.startsAt) }}
                        </option>
                      }
                    </select>
                  </label>

                  <label class="field">
                    <span>What brings you in?</span>
                    <select formControlName="goal" data-testid="trial-goal">
                      <option value="Build strength and muscle">Build strength and muscle</option>
                      <option value="Improve general fitness">Improve general fitness</option>
                      <option value="Lose fat sustainably">Lose fat sustainably</option>
                      <option value="Move and feel better">Move and feel better</option>
                      <option value="Sport-specific performance">Sport-specific performance</option>
                      <option value="Return from injury">Return from injury</option>
                      <option value="Not sure yet">Not sure yet</option>
                    </select>
                  </label>

                  <label class="field">
                    <span>Anything a coach should know? <em>Optional</em></span>
                    <textarea formControlName="notes" rows="3" placeholder="Injuries, experience level, or what a good session looks like for you." data-testid="trial-notes"></textarea>
                  </label>

                  <label class="consent">
                    <input type="checkbox" formControlName="consentToContact" data-testid="trial-consent">
                    <span class="check-box"><span class="material-icons" aria-hidden="true">check</span></span>
                    <span>I agree to be contacted about my trial request. No marketing lists, no sharing my details.</span>
                  </label>
                  @if (invalid('consentToContact')) {
                    <p class="consent-error">Please confirm we can contact you about this request.</p>
                  }

                  @if (errorMessage(); as message) {
                    <div class="form-error" role="alert">
                      <span class="material-icons" aria-hidden="true">error_outline</span>
                      {{ message }}
                    </div>
                  }

                  <button type="submit" class="submit-button" [disabled]="submitting()" data-testid="trial-submit">
                    @if (submitting()) {
                      <span class="spinner" aria-hidden="true"></span>
                      Sending request
                    } @else {
                      Book my free trial
                      <span class="material-icons" aria-hidden="true">arrow_forward</span>
                    }
                  </button>
                  <p class="form-footnote">
                    No card required. Your first session is free, including equipment and a floor orientation.
                  </p>
                </form>
              }
            </div>
          </div>
        </section>

        <section class="trial-bottom">
          <div class="kx-container trial-bottom-inner">
            <div class="bottom-mark" aria-hidden="true">K</div>
            <div>
              <p class="kx-eyebrow">Three gyms. One standard.</p>
              <h2>{{ site.branches.length }} locations, and a coach<br>who knows your name.</h2>
            </div>
            <div class="bottom-branches">
              @for (branch of site.branches; track branch.id) {
                <div>
                  <strong>{{ branch.name }}</strong>
                  <span>{{ branch.neighbourhood }}</span>
                  <span>{{ branch.phone }}</span>
                </div>
              }
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
    .trial-page { overflow: hidden; background: var(--kx-paper); }

    .trial-shell { padding-block: 22px; }

    .trial-grid {
      display: grid;
      grid-template-columns: .82fr 1.18fr;
      min-height: 760px;
      overflow: hidden;
      border-radius: 26px;
      box-shadow: 0 24px 70px rgba(25, 21, 15, .11);
    }

    .trial-aside {
      position: relative;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      padding: 48px 41px 37px;
      background: var(--kx-night);
      color: var(--kx-cream);
    }

    .trial-aside::before {
      content: '';
      position: absolute;
      left: -180px;
      bottom: -230px;
      width: 560px;
      height: 560px;
      border-radius: 999px;
      background: radial-gradient(circle, rgba(240,79,36,.26), transparent 65%);
    }

    .aside-orbit { position: absolute; top: 90px; right: -170px; width: 390px; height: 390px; border: 1px solid rgba(255,255,255,.08); border-radius: 999px; }
    .aside-orbit::after { content: ''; position: absolute; inset: 58px; border: 1px solid rgba(217,255,79,.12); border-radius: inherit; }

    .trial-aside > * { position: relative; z-index: 1; }
    .trial-aside h1 { margin: 21px 0 0; font-size: clamp(49px, 5.5vw, 70px); font-weight: 840; letter-spacing: -.066em; line-height: .88; }
    .trial-aside h1 span { color: var(--kx-ember); font-family: Georgia, 'Times New Roman', serif; font-style: italic; font-weight: 500; }
    .aside-copy { max-width: 40ch; margin: 24px 0 0; color: rgba(251,246,238,.46); font-size: 11px; line-height: 1.72; }

    .aside-benefits { display: flex; flex-direction: column; gap: 0; margin-top: 35px; border-top: 1px solid rgba(255,255,255,.11); }
    .aside-benefits > div { display: grid; grid-template-columns: 30px 1fr; gap: 12px; padding: 17px 0; border-bottom: 1px solid rgba(255,255,255,.1); }
    .benefit-index { padding-top: 1px; color: var(--kx-acid); font-size: 8px; font-weight: 750; letter-spacing: .09em; }
    .aside-benefits strong { display: block; font-size: 11px; font-weight: 600; letter-spacing: -.01em; }
    .aside-benefits p { margin: 5px 0 0; color: rgba(251,246,238,.34); font-size: 8px; line-height: 1.6; }

    .aside-footer { display: flex; gap: 10px; align-items: flex-start; margin-top: auto; padding-top: 32px; }
    .aside-footer .material-icons { color: var(--kx-ember); font-size: 16px; }
    .aside-footer p { margin: 0; color: rgba(251,246,238,.35); font-size: 8px; line-height: 1.6; }
    .aside-footer a { color: var(--kx-cream); text-decoration: none; border-bottom: 1px solid rgba(255,255,255,.22); }

    .trial-form-panel { display: flex; flex-direction: column; justify-content: center; padding: 48px clamp(28px, 5vw, 70px); background: var(--kx-surface); }

    .form-heading { display: flex; align-items: flex-start; justify-content: space-between; gap: 26px; margin-bottom: 30px; }
    .form-heading h2 { margin: 14px 0 0; font-size: clamp(34px, 3.6vw, 47px); font-weight: 800; letter-spacing: -.057em; line-height: 1.01; }
    .form-step { padding-top: 3px; color: var(--kx-muted); font-size: 8px; font-weight: 700; letter-spacing: .1em; white-space: nowrap; }

    form { display: flex; flex-direction: column; gap: 16px; }
    .field-row { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; }
    .field { display: flex; min-width: 0; flex-direction: column; gap: 7px; }
    .field > span { display: flex; justify-content: space-between; gap: 10px; color: var(--kx-ink); font-size: 8px; font-weight: 700; letter-spacing: .075em; text-transform: uppercase; }
    .field em { color: var(--kx-muted); font-size: 7px; font-style: normal; font-weight: 500; letter-spacing: .03em; text-transform: none; }

    .field input, .field select, .field textarea {
      width: 100%;
      border: 1px solid var(--kx-line);
      border-radius: 9px;
      background: var(--kx-paper);
      color: var(--kx-ink);
      font: inherit;
      font-size: 11px;
      outline: none;
      transition: border-color .18s, box-shadow .18s, background .18s;
    }

    .field input, .field select { height: 43px; padding: 0 12px; }
    .field textarea { resize: vertical; min-height: 76px; padding: 11px 12px; line-height: 1.5; }
    .field select { appearance: none; background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='10' height='6' viewBox='0 0 10 6'%3E%3Cpath d='M1 1l4 4 4-4' fill='none' stroke='%23757168' stroke-width='1.3' stroke-linecap='round'/%3E%3C/svg%3E"); background-repeat: no-repeat; background-position: right 12px center; padding-right: 32px; }
    .field input::placeholder, .field textarea::placeholder { color: #a49d91; }
    .field input:focus, .field select:focus, .field textarea:focus { border-color: var(--kx-ember); background: #fffdf9; box-shadow: 0 0 0 3px rgba(240,79,36,.07); }
    .field small { color: #b5482e; font-size: 8px; }

    .consent { display: flex; align-items: flex-start; gap: 9px; margin-top: 1px; color: var(--kx-muted); font-size: 8px; line-height: 1.55; cursor: pointer; }
    .consent input { position: absolute; width: 1px; height: 1px; opacity: 0; pointer-events: none; }
    .check-box { display: inline-grid; flex: none; width: 16px; height: 16px; place-items: center; border: 1px solid var(--kx-line); border-radius: 4px; background: var(--kx-paper); transition: background .18s, border-color .18s; }
    .check-box .material-icons { color: transparent; font-size: 12px; transition: color .18s; }
    .consent input:checked + .check-box { border-color: var(--kx-night); background: var(--kx-night); }
    .consent input:checked + .check-box .material-icons { color: var(--kx-acid); }
    .consent input:focus-visible + .check-box { outline: 2px solid var(--kx-ember); outline-offset: 2px; }
    .consent-error { margin: -8px 0 0; color: #b5482e; font-size: 8px; }

    .form-error { display: flex; align-items: flex-start; gap: 8px; padding: 10px 11px; border: 1px solid rgba(181,72,46,.22); border-radius: 8px; background: rgba(181,72,46,.05); color: #8d3a27; font-size: 9px; line-height: 1.5; }
    .form-error .material-icons { font-size: 15px; }

    .submit-button {
      display: flex;
      min-height: 48px;
      align-items: center;
      justify-content: center;
      gap: 9px;
      margin-top: 4px;
      border: 0;
      border-radius: 9px;
      background: var(--kx-ember);
      color: #fff9f1;
      font: inherit;
      font-size: 11px;
      font-weight: 700;
      cursor: pointer;
      transition: background .18s, transform .18s;
    }

    .submit-button:hover:not(:disabled) { background: var(--kx-ember-deep); transform: translateY(-1px); }
    .submit-button:disabled { opacity: .6; cursor: wait; }
    .submit-button .material-icons { font-size: 17px; }
    .form-footnote { margin: -3px 0 0; color: var(--kx-muted); font-size: 7px; text-align: center; }

    .spinner { width: 14px; height: 14px; border: 2px solid rgba(255,255,255,.35); border-top-color: #fff; border-radius: 999px; animation: spin .7s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }

    .trial-success { max-width: 440px; margin: auto; }
    .success-ring { display: grid; width: 55px; height: 55px; margin-bottom: 24px; place-items: center; border-radius: 999px; background: rgba(61,97,65,.1); color: #436b48; }
    .success-ring .material-icons { font-size: 28px; }
    .trial-success h2 { margin: 14px 0 0; font-size: clamp(40px, 4.5vw, 55px); font-weight: 820; letter-spacing: -.06em; line-height: .96; }
    .success-copy { max-width: 38ch; margin: 20px 0 0; color: var(--kx-muted); font-size: 11px; line-height: 1.7; }
    .reference { display: flex; align-items: center; justify-content: space-between; margin-top: 27px; padding: 12px 14px; border: 1px solid var(--kx-line); border-radius: 9px; background: var(--kx-paper); }
    .reference span { color: var(--kx-muted); font-size: 7px; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; }
    .reference strong { font-family: ui-monospace, SFMono-Regular, Menlo, monospace; font-size: 10px; letter-spacing: .05em; }
    .success-actions { display: flex; flex-direction: column; align-items: flex-start; gap: 15px; margin-top: 27px; }
    .text-button { padding: 0; border: 0; border-bottom: 1px solid var(--kx-line); background: none; color: var(--kx-muted); font: inherit; font-size: 9px; cursor: pointer; }

    .trial-bottom { padding-block: 43px 55px; }
    .trial-bottom-inner { display: grid; grid-template-columns: auto .8fr 1.2fr; gap: 30px; align-items: center; border-top: 1px solid var(--kx-line); padding-top: 33px; }
    .bottom-mark { display: grid; width: 59px; height: 59px; place-items: center; border-radius: 999px; background: var(--kx-night); color: var(--kx-cream); font-size: 25px; font-weight: 830; letter-spacing: -.08em; }
    .trial-bottom h2 { margin: 13px 0 0; font-size: clamp(25px, 2.6vw, 33px); font-weight: 790; letter-spacing: -.047em; line-height: 1.06; }
    .bottom-branches { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; }
    .bottom-branches div { display: flex; flex-direction: column; gap: 3px; padding-left: 12px; border-left: 1px solid var(--kx-line); }
    .bottom-branches strong { font-size: 10px; font-weight: 750; }
    .bottom-branches span { color: var(--kx-muted); font-size: 8px; }

    .skeleton-title { width: 55%; height: 240px; }
    .skeleton-grid { height: 480px; }

    @media (max-width: 1000px) {
      .trial-grid { grid-template-columns: 1fr; }
      .trial-aside { min-height: 0; padding: 39px 30px; }
      .aside-benefits { display: grid; grid-template-columns: repeat(3, 1fr); gap: 18px; border-top: 0; margin-top: 26px; }
      .aside-benefits > div { grid-template-columns: 1fr; gap: 7px; padding: 0; border: 0; }
      .aside-footer { display: none; }
      .trial-aside h1 { font-size: clamp(48px, 8vw, 64px); }
      .trial-form-panel { padding-block: 38px; }
    }

    @media (max-width: 720px) {
      .trial-shell { padding-block: 10px; }
      .trial-grid { border-radius: 18px; }
      .field-row { grid-template-columns: 1fr; }
      .form-heading { margin-bottom: 24px; }
      .aside-benefits { grid-template-columns: 1fr; }
      .aside-benefits > div { grid-template-columns: 24px 1fr; }
      .trial-bottom-inner { grid-template-columns: auto 1fr; }
      .bottom-branches { grid-column: 1 / -1; }
    }

    @media (max-width: 520px) {
      .trial-aside { padding: 30px 22px; }
      .trial-form-panel { padding: 30px 19px; }
      .trial-bottom { padding-bottom: 38px; }
      .bottom-branches { grid-template-columns: 1fr; gap: 12px; }
      .bottom-branches div { padding-left: 9px; }
      .form-step { display: none; }
    }
  `],
})
export class PublicTrialComponent {
  readonly facade = inject(PublicSiteFacade);
  private readonly gateway = inject(PUBLIC_SITE_GATEWAY);
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly formatShortDate = formatShortDate;
  protected readonly formatTime = formatTime;
  protected readonly today = toDateInputValue(new Date().toISOString());

  readonly submitting = signal(false);
  readonly created = signal<TrialCreated | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly submittedName = signal('');
  readonly branchId = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
    phone: ['', [Validators.required, Validators.maxLength(40)]],
    preferredDate: [''],
    branchId: [''],
    classSessionId: [''],
    goal: ['Not sure yet', [Validators.maxLength(200)]],
    notes: ['', [Validators.maxLength(1000)]],
    consentToContact: [false, [Validators.requiredTrue]],
  });

  readonly availableSessions = computed(() => {
    const branch = this.branchId();
    return this.facade.schedule().filter((session) => !branch || session.branchId === branch);
  });

  private queryApplied = false;

  constructor() {
    effect(() => {
      if (!this.facade.site() || this.queryApplied) return;

      this.queryApplied = true;
      const params = this.route.snapshot.queryParamMap;
      const branchName = params.get('branch');
      const className = params.get('class');
      const planSlug = params.get('plan');

      if (branchName) {
        const branch = this.facade.branches().find((item) => item.name === branchName);
        if (branch) {
          this.form.controls.branchId.setValue(branch.id);
          this.branchId.set(branch.id);
        }
      }

      if (className) {
        const session = this.availableSessions().find((item) => item.name === className);
        if (session) this.form.controls.classSessionId.setValue(session.id);
      }

      if (planSlug) {
        const plan = this.facade.plans().find((item) => item.slug === planSlug);
        if (plan) {
          this.form.controls.notes.setValue(`Interested in the ${plan.name} membership.`);
        }
      }
    });
  }

  invalid(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || control.dirty);
  }

  branchChanged(): void {
    const branch = this.form.controls.branchId.value;
    this.branchId.set(branch);
    const selected = this.form.controls.classSessionId.value;
    if (selected && !this.availableSessions().some((session) => session.id === selected)) {
      this.form.controls.classSessionId.setValue('');
    }
  }

  async submit(): Promise<void> {
    this.errorMessage.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);

    try {
      const preferredDate = value.preferredDate
        ? new Date(`${value.preferredDate}T12:00:00`).toISOString()
        : null;

      const result = await this.gateway.createTrial({
        firstName: value.firstName.trim(),
        lastName: value.lastName.trim(),
        email: value.email.trim(),
        phone: value.phone.trim(),
        preferredDate,
        classSessionId: value.classSessionId || null,
        branchId: value.branchId || null,
        goal: value.goal || null,
        notes: value.notes.trim() || null,
        consentToContact: value.consentToContact,
      });

      this.submittedName.set(value.firstName.trim());
      this.created.set(result);
    } catch {
      this.errorMessage.set(
        'We could not send your request. Check your details and try again, or call the gym and we will sort it out.',
      );
    } finally {
      this.submitting.set(false);
    }
  }

  reference(result: TrialCreated): string {
    return `KV-${result.id.slice(0, 8).toUpperCase()}`;
  }

  startAnother(): void {
    this.form.reset({
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      preferredDate: '',
      branchId: '',
      classSessionId: '',
      goal: 'Not sure yet',
      notes: '',
      consentToContact: false,
    });
    this.branchId.set('');
    this.created.set(null);
    this.errorMessage.set(null);
  }
}
