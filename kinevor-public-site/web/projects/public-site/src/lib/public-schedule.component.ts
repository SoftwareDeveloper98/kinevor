import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { PublicSiteFacade } from './public-site.facade';
import { PUBLIC_PAGE_STYLES } from './public-page.styles';
import { categoryClass, formatTime, formatWeekday, toDateInputValue } from './public-site.utils';

interface DayOption {
  iso: string;
  weekday: string;
  day: string;
  date: Date;
}

function localIso(date: Date): string {
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

function mondayOf(date: Date): Date {
  const result = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  const offset = (result.getDay() + 6) % 7;
  result.setDate(result.getDate() - offset);
  return result;
}

@Component({
  selector: 'kinevor-public-schedule',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (facade.site(); as site) {
      <div class="schedule-page" data-testid="schedule-page">
        <section class="schedule-hero">
          <div class="hero-glow" aria-hidden="true"></div>
          <div class="kx-container hero-grid">
            <div>
              <p class="kx-eyebrow">The timetable</p>
              <h1 class="schedule-title">Find your<br><span>hour.</span></h1>
              <p class="hero-copy">
                Strength, sweat and recovery across all three gyms. Every class is coach-led and capped
                so you get attention, not a number in a crowd.
              </p>
            </div>
            <div class="hero-summary">
              <div class="summary-top">
                <span class="live-dot"></span>
                <span>Next 14 days</span>
              </div>
              <strong>{{ facade.schedule().length }}</strong>
              <span>coach-led sessions<br>on the calendar</span>
              <div class="summary-rule"></div>
              <p>Book up to 7 days ahead in the member app. Doors open 15 minutes before each class.</p>
            </div>
          </div>
        </section>

        <section class="calendar-section">
          <div class="kx-container">
            <div class="week-toolbar">
              <button type="button" class="icon-button" aria-label="Previous week" (click)="shiftWeek(-7)">
                <span class="material-icons">arrow_back</span>
              </button>
              <span class="week-range">{{ weekLabel() }}</span>
              <button type="button" class="icon-button" aria-label="Next week" (click)="shiftWeek(7)">
                <span class="material-icons">arrow_forward</span>
              </button>
              <label class="date-jump">
                <span>Jump to date</span>
                <input type="date" [value]="selectedDate()" (change)="onDateChange($event)">
              </label>
            </div>

            <div class="day-strip">
              @for (day of week(); track day.iso) {
                <button
                  type="button"
                  class="day-card"
                  [class.day-active]="day.iso === selectedDate()"
                  (click)="selectDay(day.iso)"
                >
                  <span class="day-weekday">{{ day.weekday }}</span>
                  <span class="day-number">{{ day.day }}</span>
                  <span class="day-count">{{ countFor(day.iso) }} classes</span>
                </button>
              }
            </div>

            <div class="session-head">
              <div>
                <p class="selected-label">{{ selectedLabel() }}</p>
                <p class="selected-count">{{ selectedSessions().length }} sessions scheduled</p>
              </div>
              <div class="branch-filter" role="group" aria-label="Filter by location">
                <button type="button" [class.filter-active]="branchFilter() === 'all'" (click)="setBranch('all')">
                  All gyms
                </button>
                @for (branch of facade.branches(); track branch.id) {
                  <button
                    type="button"
                    [class.filter-active]="branchFilter() === branch.id"
                    (click)="setBranch(branch.id)"
                  >
                    {{ branch.name }}
                  </button>
                }
              </div>
            </div>

            @if (filteredSessions().length > 0) {
              <div class="session-list">
                @for (session of filteredSessions(); track session.id) {
                  <article class="session-card">
                    <div class="session-time">
                      <strong>{{ formatTime(session.startsAt) }}</strong>
                      <span>{{ session.durationMinutes }} min</span>
                    </div>

                    <div class="session-main">
                      <div class="session-kicker">
                        <span class="category-pill" [class]="'category-pill ' + categoryClass(session.category)">
                          {{ session.category }}
                        </span>
                        <span class="intensity-pill">{{ session.intensity }}</span>
                      </div>
                      <h2>{{ session.name }}</h2>
                      <p>{{ session.description }}</p>
                    </div>

                    <div class="session-meta">
                      <span>
                        <span class="material-icons" aria-hidden="true">person</span>
                        {{ session.trainerName }}
                      </span>
                      <span>
                        <span class="material-icons" aria-hidden="true">location_on</span>
                        {{ session.branchName }} &middot; {{ session.room }}
                      </span>
                    </div>

                    <div class="session-action">
                      <span class="capacity">{{ session.capacity }} places</span>
                      <a
                        class="kx-btn kx-btn-outline"
                        routerLink="/trial"
                        [queryParams]="{ class: session.name, branch: session.branchName }"
                      >
                        Try this class
                      </a>
                    </div>
                  </article>
                }
              </div>
            } @else {
              <div class="empty-day">
                <span class="material-icons" aria-hidden="true">self_improvement</span>
                <h2>No classes on this day.</h2>
                <p>Try another day or clear the location filter. The floor is always open for open-gym training.</p>
                <button type="button" class="kx-btn kx-btn-outline" (click)="setBranch('all')">Show all gyms</button>
              </div>
            }
          </div>
        </section>

        <section class="schedule-help">
          <div class="kx-container help-inner">
            <div>
              <p class="kx-eyebrow">Not sure where to start?</p>
              <h2>Tell us your week.<br>We will find the fit.</h2>
            </div>
            <div class="help-side">
              <p>
                New members get a free class and a floor orientation. A coach will suggest the right
                intensity based on your training history and recovery.
              </p>
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
    .schedule-page { overflow: hidden; }

    .schedule-hero { position: relative; overflow: hidden; padding-block: 72px 64px; background: var(--kx-night); color: var(--kx-cream); }
    .schedule-hero::after { content: 'TIMETABLE'; position: absolute; right: -35px; bottom: -38px; color: rgba(255,255,255,.025); font-size: 200px; font-weight: 850; letter-spacing: -.09em; line-height: .8; pointer-events: none; }
    .hero-glow { position: absolute; top: -250px; left: 12%; width: 580px; height: 580px; border-radius: 999px; background: radial-gradient(circle, rgba(240,79,36,.22), transparent 66%); }

    .hero-grid { position: relative; z-index: 1; display: grid; grid-template-columns: 1.3fr .7fr; gap: 80px; align-items: end; }
    .schedule-title { margin: 20px 0 0; font-size: clamp(62px, 7vw, 98px); font-weight: 850; letter-spacing: -.07em; line-height: .86; }
    .schedule-title span { color: var(--kx-ember); font-family: Georgia, 'Times New Roman', serif; font-style: italic; font-weight: 500; }
    .hero-copy { max-width: 51ch; margin: 27px 0 0; color: rgba(251,246,238,.49); font-size: 12px; line-height: 1.75; }

    .hero-summary { position: relative; padding: 26px; border: 1px solid rgba(255,255,255,.13); border-radius: 21px; background: rgba(255,255,255,.04); backdrop-filter: blur(14px); }
    .summary-top { display: flex; align-items: center; gap: 7px; color: rgba(251,246,238,.35); font-size: 7px; font-weight: 700; letter-spacing: .13em; text-transform: uppercase; }
    .live-dot { width: 6px; height: 6px; border-radius: 999px; background: var(--kx-acid); box-shadow: 0 0 0 4px rgba(217,255,79,.08); }
    .hero-summary strong { display: block; margin-top: 25px; font-size: 68px; font-weight: 820; letter-spacing: -.075em; line-height: .75; }
    .hero-summary > span { display: block; margin-top: 10px; color: rgba(251,246,238,.37); font-size: 9px; line-height: 1.5; }
    .summary-rule { height: 1px; margin: 21px 0 14px; background: rgba(255,255,255,.1); }
    .hero-summary p { margin: 0; color: rgba(251,246,238,.35); font-size: 8px; line-height: 1.65; }

    .calendar-section { padding-block: 24px 92px; background: var(--kx-paper); }

    .week-toolbar { display: flex; align-items: center; gap: 12px; padding: 13px 0 11px; }
    .icon-button { display: inline-grid; width: 31px; height: 31px; place-items: center; border: 1px solid var(--kx-line); border-radius: 999px; background: transparent; color: var(--kx-ink); cursor: pointer; transition: border-color .18s, background .18s; }
    .icon-button:hover { border-color: var(--kx-ink); background: var(--kx-surface); }
    .icon-button .material-icons { font-size: 15px; }
    .week-range { min-width: 190px; color: var(--kx-muted); font-size: 9px; font-weight: 650; letter-spacing: .05em; text-transform: uppercase; }

    .date-jump { display: flex; align-items: center; gap: 9px; margin-left: auto; color: var(--kx-muted); font-size: 8px; font-weight: 650; text-transform: uppercase; letter-spacing: .07em; }
    .date-jump input { padding: 7px 9px; border: 1px solid var(--kx-line); border-radius: 8px; background: var(--kx-surface); color: var(--kx-ink); font: inherit; font-size: 9px; outline: none; }
    .date-jump input:focus { border-color: var(--kx-ember); }

    .day-strip { display: grid; grid-template-columns: repeat(7, 1fr); gap: 5px; border-bottom: 1px solid var(--kx-line); }
    .day-card { display: flex; flex-direction: column; align-items: flex-start; padding: 17px 13px 14px; border: 1px solid transparent; border-bottom: 0; border-radius: 13px 13px 0 0; background: transparent; color: var(--kx-muted); cursor: pointer; text-align: left; transition: background .18s, color .18s; }
    .day-card:hover { background: var(--kx-surface); color: var(--kx-ink); }
    .day-weekday { font-size: 7px; font-weight: 700; letter-spacing: .11em; text-transform: uppercase; }
    .day-number { margin: 4px 0 9px; color: var(--kx-ink); font-size: 27px; font-weight: 780; letter-spacing: -.05em; line-height: 1; }
    .day-count { font-size: 7px; font-weight: 600; }
    .day-active { background: var(--kx-night); color: rgba(251,246,238,.38); }
    .day-active:hover { background: var(--kx-night); color: rgba(251,246,238,.65); }
    .day-active .day-number { color: var(--kx-ember); }
    .day-active .day-weekday { color: var(--kx-acid); }

    .session-head { display: flex; align-items: flex-end; justify-content: space-between; gap: 22px; padding: 34px 0 18px; }
    .selected-label { margin: 0; font-size: 17px; font-weight: 750; letter-spacing: -.025em; }
    .selected-count { margin: 3px 0 0; color: var(--kx-muted); font-size: 8px; }

    .branch-filter { display: flex; flex-wrap: wrap; gap: 3px; padding: 3px; border-radius: 999px; background: var(--kx-paper-deep); }
    .branch-filter button { padding: 7px 11px; border: 0; border-radius: 999px; background: transparent; color: var(--kx-muted); font: inherit; font-size: 8px; font-weight: 650; cursor: pointer; transition: background .18s, color .18s; }
    .branch-filter button:hover { color: var(--kx-ink); }
    .filter-active { background: var(--kx-surface) !important; color: var(--kx-ink) !important; box-shadow: 0 1px 4px rgba(23,20,15,.09); }

    .session-list { border-top: 1px solid var(--kx-line); }
    .session-card { display: grid; grid-template-columns: 95px 1.45fr 1fr 120px; gap: 25px; align-items: center; padding: 25px 4px; border-bottom: 1px solid var(--kx-line); transition: background .18s; }
    .session-card:hover { background: rgba(255,255,255,.4); }

    .session-time strong { display: block; font-size: 18px; font-weight: 750; letter-spacing: -.035em; }
    .session-time span { display: block; margin-top: 3px; color: var(--kx-muted); font-size: 8px; }

    .session-kicker { display: flex; align-items: center; gap: 7px; margin-bottom: 7px; }
    .category-pill, .intensity-pill { padding: 3px 7px; border-radius: 999px; font-size: 7px; font-weight: 700; letter-spacing: .07em; text-transform: uppercase; }
    .category-pill { background: rgba(240,79,36,.1); color: var(--kx-ember); }
    .category-pill.strength { background: rgba(44,63,81,.1); color: #38566d; }
    .category-pill.recovery { background: rgba(76,102,75,.12); color: #4d684b; }
    .category-pill.conditioning { background: rgba(171,118,29,.12); color: #8f6118; }
    .category-pill.combat { background: rgba(104,63,89,.12); color: #70445e; }
    .intensity-pill { border: 1px solid var(--kx-line); color: var(--kx-muted); }

    .session-main h2 { margin: 0; font-size: 20px; font-weight: 760; letter-spacing: -.035em; }
    .session-main p { max-width: 58ch; margin: 6px 0 0; color: var(--kx-muted); font-size: 8px; line-height: 1.55; }

    .session-meta { display: flex; flex-direction: column; gap: 7px; }
    .session-meta span { display: flex; align-items: center; gap: 6px; color: var(--kx-muted); font-size: 8px; }
    .session-meta .material-icons { color: var(--kx-ember); font-size: 14px; }

    .session-action { display: flex; flex-direction: column; align-items: flex-end; gap: 9px; }
    .capacity { color: var(--kx-muted); font-size: 7px; font-weight: 650; letter-spacing: .05em; text-transform: uppercase; }
    .session-action .kx-btn { min-height: 35px; padding-inline: 13px; font-size: 8px; }

    .empty-day { display: flex; flex-direction: column; align-items: center; padding: 70px 24px; border-top: 1px solid var(--kx-line); text-align: center; }
    .empty-day > .material-icons { color: var(--kx-ember); font-size: 38px; }
    .empty-day h2 { margin: 15px 0 6px; font-size: 22px; font-weight: 760; letter-spacing: -.035em; }
    .empty-day p { max-width: 42ch; margin: 0 0 20px; color: var(--kx-muted); font-size: 10px; line-height: 1.65; }

    .schedule-help { padding-block: 70px; background: var(--kx-paper-deep); }
    .help-inner { display: grid; grid-template-columns: 1fr 1fr; gap: 75px; align-items: end; }
    .help-inner h2 { margin: 17px 0 0; font-size: clamp(35px, 4vw, 49px); font-weight: 800; letter-spacing: -.052em; line-height: 1.02; }
    .help-side { display: flex; align-items: center; gap: 28px; }
    .help-side p { max-width: 40ch; margin: 0; color: var(--kx-muted); font-size: 10px; line-height: 1.7; }

    .skeleton-title { width: 55%; height: 240px; }
    .skeleton-grid { height: 400px; }

    @media (max-width: 980px) {
      .hero-grid { grid-template-columns: 1fr; gap: 34px; }
      .hero-summary { max-width: 350px; }
      .session-card { grid-template-columns: 80px 1fr 100px; gap: 20px; }
      .session-meta { display: none; }
      .day-strip { overflow-x: auto; grid-template-columns: repeat(7, minmax(90px, 1fr)); }
    }

    @media (max-width: 760px) {
      .schedule-hero { padding-block: 47px 50px; }
      .schedule-title { font-size: clamp(50px, 13vw, 74px); }
      .session-head { flex-direction: column; align-items: stretch; }
      .branch-filter { align-self: flex-start; max-width: 100%; overflow-x: auto; }
      .branch-filter button { white-space: nowrap; }
      .session-card { grid-template-columns: 68px 1fr; align-items: start; padding-block: 20px; }
      .session-action { grid-column: 2; flex-direction: row; align-items: center; justify-content: space-between; margin-top: 5px; }
      .session-main p { display: none; }
      .help-inner { grid-template-columns: 1fr; gap: 24px; }
      .help-side { flex-direction: column; align-items: flex-start; gap: 17px; }
      .week-toolbar { flex-wrap: wrap; }
      .date-jump { width: 100%; margin-left: 0; justify-content: space-between; }
    }
  `],
})
export class PublicScheduleComponent {
  readonly facade = inject(PublicSiteFacade);
  protected readonly formatTime = formatTime;
  protected readonly formatWeekday = formatWeekday;
  protected readonly categoryClass = categoryClass;

  private readonly now = new Date();
  private defaultDateApplied = false;
  readonly selectedDate = signal(localIso(this.now));
  readonly branchFilter = signal('all');

  constructor() {
    effect(() => {
      const sessions = this.facade.schedule();
      if (this.defaultDateApplied || sessions.length === 0) return;

      this.defaultDateApplied = true;
      const today = this.selectedDate();
      if (sessions.some((session) => toDateInputValue(session.startsAt) === today)) return;

      const next = sessions.find((session) => new Date(session.startsAt).getTime() >= Date.now());
      if (next) {
        this.selectedDate.set(toDateInputValue(next.startsAt));
      }
    });
  }
  readonly week = computed<DayOption[]>(() => {
    const selected = this.parseLocal(this.selectedDate());
    const monday = mondayOf(selected);
    return Array.from({ length: 7 }, (_, index) => {
      const date = new Date(monday);
      date.setDate(monday.getDate() + index);
      const iso = localIso(date);
      return {
        iso,
        weekday: formatWeekday(`${iso}T12:00:00`).slice(0, 3),
        day: `${date.getDate()}`.padStart(2, '0'),
        date,
      };
    });
  });

  readonly selectedSessions = computed(() => {
    const key = this.selectedDate();
    return this.facade.schedule().filter((session) => toDateInputValue(session.startsAt) === key);
  });

  readonly filteredSessions = computed(() => {
    const branch = this.branchFilter();
    return this.selectedSessions().filter((session) => branch === 'all' || session.branchId === branch);
  });

  readonly selectedLabel = computed(() => {
    const selected = this.parseLocal(this.selectedDate());
    return new Intl.DateTimeFormat('en-US', {
      weekday: 'long',
      month: 'long',
      day: 'numeric',
      year: 'numeric',
    }).format(selected);
  });

  readonly weekLabel = computed(() => {
    const days = this.week();
    const first = days[0].date;
    const last = days[6].date;
    const format = new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric' });
    return `${format.format(first)} - ${format.format(last)}, ${last.getFullYear()}`;
  });

  countFor(iso: string): number {
    return this.facade.schedule().filter((session) => toDateInputValue(session.startsAt) === iso).length;
  }

  selectDay(iso: string): void {
    this.selectedDate.set(iso);
    this.branchFilter.set('all');
  }

  setBranch(branchId: string): void {
    this.branchFilter.set(branchId);
  }

  shiftWeek(amount: number): void {
    const selected = this.parseLocal(this.selectedDate());
    selected.setDate(selected.getDate() + amount);
    this.selectedDate.set(localIso(selected));
  }

  onDateChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.value) {
      this.selectedDate.set(input.value);
      this.branchFilter.set('all');
    }
  }

  private parseLocal(iso: string): Date {
    const [year, month, day] = iso.split('-').map(Number);
    return new Date(year, month - 1, day);
  }
}
