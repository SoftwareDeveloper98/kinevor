import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { AlertSeverity, DashboardAlert } from './staff-dashboard.model';
import { StaffDashboardFacade } from './staff-dashboard.facade';

type SeverityFilter = 'All' | AlertSeverity;

@Component({
  selector: 'kinevor-staff-dashboard-alerts',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  template: `
    <div class="page-container alerts-page">
      <section class="alerts-hero">
        <span class="hero-glow" aria-hidden="true"></span>
        <div class="hero-copy">
          <a class="back-link" routerLink="/dashboard">
            <mat-icon aria-hidden="true">arrow_back</mat-icon>
            Dashboard
          </a>
          <span class="eyebrow">Operations inbox</span>
          <h1>Alerts &amp; tasks</h1>
          <p>
            Every overdue renewal, collection risk, low-fill class and failed delivery in one
            severity-ordered queue.
          </p>
          <div class="hero-chips">
            <span class="hero-chip"><mat-icon aria-hidden="true">storefront</mat-icon>{{ branches().length }} branches</span>
            <span class="hero-chip"><mat-icon aria-hidden="true">task_alt</mat-icon>{{ alerts().length }} open tasks</span>
            <span class="hero-chip"><mat-icon aria-hidden="true">schedule</mat-icon>{{ generatedAt() ? (generatedAt() | date: 'MMM d, HH:mm') : 'Waiting for data' }}</span>
          </div>
        </div>

        <div class="hero-controls">
          <mat-form-field appearance="outline" class="branch-field">
            <mat-label>Branch</mat-label>
            <mat-select [value]="branchId() ?? ''" (selectionChange)="changeBranch($event.value)">
              <mat-option value="">All branches</mat-option>
              @for (branch of branches(); track branch.id) {
                <mat-option [value]="branch.id">{{ branch.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <button mat-flat-button color="primary" type="button" (click)="refresh()" [disabled]="loading()">
            <mat-icon aria-hidden="true">refresh</mat-icon>
            Refresh inbox
          </button>
        </div>
      </section>

      @if (error(); as message) {
        <p class="notice notice-error" role="alert"><mat-icon aria-hidden="true">error</mat-icon>{{ message }}</p>
      }

      <section class="severity-summary" aria-label="Alerts by severity">
        <button type="button" class="severity-card critical" [class.selected]="severity() === 'Critical'" (click)="filter('Critical')">
          <span class="severity-icon"><mat-icon aria-hidden="true">emergency</mat-icon></span>
          <span class="severity-copy"><span>Critical</span><strong>{{ criticalCount() }}</strong></span>
          <span class="severity-hint">Act today</span>
        </button>
        <button type="button" class="severity-card warning" [class.selected]="severity() === 'Warning'" (click)="filter('Warning')">
          <span class="severity-icon"><mat-icon aria-hidden="true">warning</mat-icon></span>
          <span class="severity-copy"><span>Warning</span><strong>{{ warningCount() }}</strong></span>
          <span class="severity-hint">This shift</span>
        </button>
        <button type="button" class="severity-card info" [class.selected]="severity() === 'Info'" (click)="filter('Info')">
          <span class="severity-icon"><mat-icon aria-hidden="true">info</mat-icon></span>
          <span class="severity-copy"><span>Info</span><strong>{{ infoCount() }}</strong></span>
          <span class="severity-hint">Follow up</span>
        </button>
      </section>

      <div class="filter-bar">
        <div class="filter-pills" role="group" aria-label="Filter by severity">
          @for (option of severityOptions; track option.value) {
            <button
              type="button"
              [class.on]="severity() === option.value"
              (click)="filter(option.value)"
            >
              {{ option.label }}
              <span>{{ countFor(option.value) }}</span>
            </button>
          }
        </div>
        <span class="queue-note">{{ branchName() }} · {{ visibleAlerts().length }} shown</span>
      </div>

      @if (loading()) {
        <div class="dashboard-loading" role="status">
          <mat-spinner diameter="48" />
          <span>Loading the alert inbox…</span>
        </div>
      } @else {
        <div class="alerts-board">
          @for (alert of visibleAlerts(); track alert.id) {
            <article class="alert-row" [class]="'severity-' + alert.severity.toLowerCase()">
              <span class="alert-icon"><mat-icon aria-hidden="true">{{ alertIcon(alert.type) }}</mat-icon></span>

              <div class="alert-main">
                <div class="alert-tags">
                  <span class="severity-chip">{{ alert.severity }}</span>
                  <span class="type-chip">{{ alertType(alert.type) }}</span>
                  <span class="ref-chip">{{ alert.reference }}</span>
                </div>
                <h2>{{ alert.title }}</h2>
                <p>{{ alert.detail }}</p>
                <div class="alert-meta">
                  <span><mat-icon aria-hidden="true">storefront</mat-icon>{{ alert.branchName }} · {{ alert.branchCity }}</span>
                  <span [class.overdue]="isOverdue(alert.dueAt)">
                    <mat-icon aria-hidden="true">schedule</mat-icon>
                    Due {{ alert.dueAt ? (alert.dueAt | date: 'MMM d, HH:mm') : 'anytime' }}
                  </span>
                </div>
              </div>

              <span class="alert-state" [class.overdue]="isOverdue(alert.dueAt)">
                {{ isOverdue(alert.dueAt) ? 'Overdue' : 'Open' }}
              </span>
            </article>
          } @empty {
            <div class="empty-state">
              <div class="empty-art"><mat-icon aria-hidden="true">task_alt</mat-icon></div>
              <h2>No alerts in this view</h2>
              <p>Try another severity or branch. A clear queue is a good queue.</p>
              <button mat-stroked-button type="button" (click)="clearFilters()">Show all alerts</button>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .alerts-page { display:flex; flex-direction:column; gap:16px; }
    .alerts-hero { position:relative; overflow:hidden; display:flex; align-items:flex-end; justify-content:space-between; gap:24px; flex-wrap:wrap; padding:28px; border-radius:var(--radius-sheet); background:linear-gradient(135deg, #201a1a 0%, #17181d 58%, #1c1f26 100%); border:1px solid var(--color-border); }
    .hero-glow { position:absolute; width:360px; height:360px; top:-210px; inset-inline-end:100px; border-radius:999px; filter:blur(85px); background:color-mix(in srgb, var(--color-error) 22%, transparent); pointer-events:none; }
    .hero-copy { position:relative; display:flex; flex-direction:column; gap:7px; flex:1; min-width:320px; }
    .back-link { display:inline-flex; align-items:center; gap:4px; font-size:11px; font-weight:700; color:var(--color-text-muted); width:max-content; }
    .back-link mat-icon { font-size:15px; width:15px; height:15px; }
    .eyebrow { font-size:11px; font-weight:700; letter-spacing:.16em; text-transform:uppercase; color:var(--color-accent-text); }
    .alerts-hero h1 { margin:0; font-size:clamp(30px, 2.4vw, 42px); letter-spacing:-.035em; line-height:1.05; }
    .alerts-hero p { margin:0; max-width:62ch; color:var(--color-text-muted); font-size:14px; line-height:1.65; }
    .hero-chips { display:flex; gap:8px; flex-wrap:wrap; margin-top:5px; }
    .hero-chip { display:inline-flex; align-items:center; gap:5px; padding:5px 10px; border-radius:999px; background:color-mix(in srgb, var(--color-surface-raised) 80%, transparent); border:1px solid var(--color-border); color:var(--color-text-muted); font-size:11px; font-weight:600; }
    .hero-chip mat-icon { font-size:14px; width:14px; height:14px; color:var(--color-text); }
    .hero-controls { position:relative; display:flex; align-items:center; gap:10px; flex-wrap:wrap; }
    .branch-field { width:230px; margin-bottom:-20px; }
    .severity-summary { display:grid; grid-template-columns:1fr; gap:10px; }
    .severity-card { display:flex; align-items:center; gap:12px; padding:14px 16px; border-radius:16px; text-align:start; font-family:inherit; cursor:pointer; border:1px solid var(--color-border); background:var(--color-surface-raised); color:var(--color-text); transition:border-color .18s ease, transform .18s ease; }
    .severity-card:hover { transform:translateY(-2px); }
    .severity-icon { width:38px; height:38px; border-radius:11px; display:inline-flex; align-items:center; justify-content:center; flex:none; }
    .severity-icon mat-icon { font-size:20px; width:20px; height:20px; }
    .severity-copy { display:flex; flex-direction:column; gap:1px; flex:1; }
    .severity-copy span { font-size:9px; font-weight:700; letter-spacing:.1em; text-transform:uppercase; color:var(--color-text-muted); }
    .severity-copy strong { font-size:24px; line-height:1; font-weight:800; }
    .severity-hint { font-size:10px; font-weight:600; color:var(--color-text-muted); }
    .severity-card.critical .severity-icon { background:color-mix(in srgb, var(--color-error) 14%, transparent); color:var(--color-error); }
    .severity-card.critical strong { color:var(--color-error); }
    .severity-card.warning .severity-icon { background:color-mix(in srgb, var(--color-warning) 14%, transparent); color:var(--color-warning); }
    .severity-card.warning strong { color:var(--color-warning); }
    .severity-card.info .severity-icon { background:color-mix(in srgb, var(--color-info) 14%, transparent); color:var(--color-info); }
    .severity-card.info strong { color:var(--color-info); }
    .severity-card.selected { border-color:color-mix(in srgb, var(--color-accent-fill) 46%, transparent); box-shadow:0 0 0 1px color-mix(in srgb, var(--color-accent-fill) 24%, transparent) inset; }
    .filter-bar { display:flex; align-items:center; justify-content:space-between; gap:12px; flex-wrap:wrap; }
    .filter-pills { display:inline-flex; gap:4px; padding:4px; border-radius:12px; background:var(--color-surface-raised); border:1px solid var(--color-border); }
    .filter-pills button { display:inline-flex; align-items:center; gap:6px; border:0; background:transparent; color:var(--color-text-muted); font-family:inherit; font-size:12px; font-weight:600; padding:7px 11px; border-radius:9px; cursor:pointer; min-height:34px; }
    .filter-pills button span { font-size:10px; padding:1px 6px; border-radius:999px; background:var(--color-surface); }
    .filter-pills button.on { background:var(--color-accent-fill); color:var(--color-on-accent); }
    .filter-pills button.on span { background:color-mix(in srgb, var(--color-on-accent) 18%, transparent); color:var(--color-on-accent); }
    .queue-note { font-size:11px; color:var(--color-text-muted); }
    .alerts-board { display:flex; flex-direction:column; gap:10px; }
    .alert-row { display:flex; align-items:flex-start; gap:14px; padding:16px 18px; border-radius:16px; background:var(--color-surface-raised); border:1px solid var(--color-border); border-inline-start:3px solid var(--color-border); }
    .alert-icon { width:40px; height:40px; border-radius:12px; display:inline-flex; align-items:center; justify-content:center; flex:none; background:var(--color-surface); color:var(--color-text-muted); }
    .alert-icon mat-icon { font-size:20px; width:20px; height:20px; }
    .alert-main { display:flex; flex-direction:column; gap:5px; flex:1; min-width:0; }
    .alert-tags { display:flex; gap:6px; flex-wrap:wrap; }
    .severity-chip, .type-chip, .ref-chip { font-size:9px; font-weight:700; text-transform:uppercase; letter-spacing:.08em; padding:2px 8px; border-radius:999px; background:var(--color-surface); }
    .severity-chip { color:var(--color-text-muted); }
    .type-chip { color:var(--color-text-muted); }
    .ref-chip { font-family:ui-monospace, SFMono-Regular, Consolas, monospace; letter-spacing:0; text-transform:none; color:var(--color-text-muted); }
    .alert-main h2 { margin:2px 0 0; font-size:15px; letter-spacing:-.015em; }
    .alert-main p { margin:0; font-size:12px; line-height:1.55; color:var(--color-text-muted); }
    .alert-meta { display:flex; gap:14px; flex-wrap:wrap; margin-top:2px; font-size:10px; color:var(--color-text-muted); }
    .alert-meta span { display:inline-flex; align-items:center; gap:4px; }
    .alert-meta mat-icon { font-size:13px; width:13px; height:13px; }
    .alert-meta .overdue { color:var(--color-error); font-weight:700; }
    .alert-state { flex:none; padding:4px 10px; border-radius:999px; font-size:9px; font-weight:800; text-transform:uppercase; letter-spacing:.08em; background:var(--color-surface); color:var(--color-text-muted); }
    .alert-state.overdue { background:color-mix(in srgb, var(--color-error) 12%, transparent); color:var(--color-error); }
    .severity-critical { border-inline-start-color:var(--color-error); }
    .severity-critical .alert-icon { background:color-mix(in srgb, var(--color-error) 12%, transparent); color:var(--color-error); }
    .severity-critical .severity-chip { background:color-mix(in srgb, var(--color-error) 12%, transparent); color:var(--color-error); }
    .severity-warning { border-inline-start-color:var(--color-warning); }
    .severity-warning .alert-icon { background:color-mix(in srgb, var(--color-warning) 12%, transparent); color:var(--color-warning); }
    .severity-warning .severity-chip { background:color-mix(in srgb, var(--color-warning) 12%, transparent); color:var(--color-warning); }
    .severity-info { border-inline-start-color:var(--color-info); }
    .severity-info .alert-icon { background:color-mix(in srgb, var(--color-info) 12%, transparent); color:var(--color-info); }
    .severity-info .severity-chip { background:color-mix(in srgb, var(--color-info) 12%, transparent); color:var(--color-info); }
    .dashboard-loading { display:flex; flex-direction:column; align-items:center; justify-content:center; gap:12px; min-height:320px; color:var(--color-text-muted); }
    .empty-state { background:var(--color-surface-raised); border:1px solid var(--color-border); border-radius:var(--radius-card); }
    .notice { display:flex; align-items:center; gap:8px; }
    @media (min-width: 720px) {
      .severity-summary { grid-template-columns:repeat(3, minmax(0,1fr)); }
      .branch-field { width:230px; }
    }
    @media (max-width: 700px) {
      .alerts-hero { padding:22px; }
      .hero-controls, .branch-field { width:100%; }
      .alert-row { flex-wrap:wrap; }
      .alert-state { margin-inline-start:54px; }
    }
  `],
})
export class DashboardAlertsComponent implements OnInit {
  readonly facade = inject(StaffDashboardFacade);

  readonly alerts = this.facade.alerts;
  readonly branches = this.facade.branches;
  readonly branchId = this.facade.branchId;
  readonly branchName = this.facade.branchName;
  readonly loading = this.facade.alertsLoading;
  readonly error = this.facade.error;
  readonly generatedAt = this.facade.generatedAt;
  readonly criticalCount = this.facade.criticalCount;
  readonly warningCount = this.facade.warningCount;
  readonly infoCount = this.facade.infoCount;

  readonly severity = signal<SeverityFilter>('All');
  readonly severityOptions: readonly { value: SeverityFilter; label: string }[] = [
    { value: 'All', label: 'All' },
    { value: 'Critical', label: 'Critical' },
    { value: 'Warning', label: 'Warning' },
    { value: 'Info', label: 'Info' },
  ];

  readonly visibleAlerts = computed(() => {
    const filter = this.severity();
    const alerts = this.facade.alerts();
    return filter === 'All' ? alerts : alerts.filter((alert) => alert.severity === filter);
  });

  ngOnInit(): void {
    void this.facade.loadAlerts();
  }

  changeBranch(branchId: string): void {
    void this.facade.setBranch(branchId || null);
  }

  refresh(): void {
    void this.facade.refreshAlerts();
  }

  filter(value: SeverityFilter): void {
    this.severity.set(value);
  }

  clearFilters(): void {
    this.severity.set('All');
  }

  countFor(value: SeverityFilter): number {
    if (value === 'Critical') return this.criticalCount();
    if (value === 'Warning') return this.warningCount();
    if (value === 'Info') return this.infoCount();
    return this.alerts().length;
  }

  isOverdue(dueAt: string | null): boolean {
    return dueAt !== null && new Date(dueAt).getTime() < Date.now();
  }

  alertIcon(type: string): string {
    const icons: Record<string, string> = {
      OverdueInvoice: 'receipt_long',
      ExpiringMembership: 'event_busy',
      LowClassFill: 'groups',
      FailedNotification: 'notification_important',
    };
    return icons[type] ?? 'report_problem';
  }

  alertType(type: string): string {
    const labels: Record<string, string> = {
      OverdueInvoice: 'Billing',
      ExpiringMembership: 'Retention',
      LowClassFill: 'Classes',
      FailedNotification: 'Notifications',
    };
    return labels[type] ?? type;
  }
}
