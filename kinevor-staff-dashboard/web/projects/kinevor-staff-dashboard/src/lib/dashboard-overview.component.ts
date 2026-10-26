import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { DashboardDays } from './staff-dashboard.model';
import { StaffDashboardFacade } from './staff-dashboard.facade';

@Component({
  selector: 'kinevor-staff-dashboard-overview',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CurrencyPipe,
    DatePipe,
    DecimalPipe,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  template: `
    <div class="page-container dashboard-page">
      <section class="dash-hero">
        <span class="hero-glow hero-glow-a" aria-hidden="true"></span>
        <span class="hero-glow hero-glow-b" aria-hidden="true"></span>

        <div class="hero-main">
          <span class="eyebrow"><span class="live-dot" aria-hidden="true"></span>Executive pulse</span>
          <h1>Staff dashboard</h1>
          <p class="hero-sub">
            A live operating view of memberships, revenue, occupancy and front-desk workload
            across the Kinevor branch network.
          </p>
          <div class="hero-chips">
            <span class="hero-chip"><mat-icon aria-hidden="true">storefront</mat-icon>{{ branches().length }} branches</span>
            <span class="hero-chip"><mat-icon aria-hidden="true">groups</mat-icon>{{ (summary()?.activeMembers ?? 0) | number }} active members</span>
            <span class="hero-chip">
              <mat-icon aria-hidden="true">schedule</mat-icon>
              {{ generatedAt() ? (generatedAt() | date: 'MMM d, HH:mm') : 'Waiting for data' }}
            </span>
            @if (criticalCount() > 0) {
              <a class="hero-chip hero-chip-critical" routerLink="/alerts">
                <mat-icon aria-hidden="true">priority_high</mat-icon>{{ criticalCount() }} critical tasks
              </a>
            }
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

          <div class="range-toggle" role="group" aria-label="Trend range">
            <button type="button" [class.on]="days() === 7" (click)="setRange(7)">7 days</button>
            <button type="button" [class.on]="days() === 30" (click)="setRange(30)">30 days</button>
          </div>

          <button mat-flat-button color="primary" type="button" (click)="refresh()" [disabled]="loading()">
            <mat-icon aria-hidden="true">refresh</mat-icon>
            Refresh
          </button>
        </div>
      </section>

      @if (error(); as message) {
        <p class="notice notice-error" role="alert"><mat-icon aria-hidden="true">error</mat-icon>{{ message }}</p>
      }

      @if (summary(); as kpi) {
        <section class="kpi-grid" aria-label="Key performance indicators">
          <article class="kpi-card kpi-warm">
            <div class="kpi-head">
              <span class="kpi-label">Active members</span>
              <span class="kpi-icon"><mat-icon aria-hidden="true">how_to_reg</mat-icon></span>
            </div>
            <div class="kpi-value">{{ kpi.activeMembers | number }}</div>
            <div class="kpi-foot">
              <span class="delta" [class.negative]="kpi.comparison.activeMembersPercent < 0">
                <mat-icon aria-hidden="true">{{ kpi.comparison.activeMembersPercent < 0 ? 'trending_down' : 'trending_up' }}</mat-icon>
                {{ abs(kpi.comparison.activeMembersPercent) }}%
              </span>
              <span>vs last month</span>
            </div>
          </article>

          <article class="kpi-card kpi-cool">
            <div class="kpi-head">
              <span class="kpi-label">Revenue this month</span>
              <span class="kpi-icon"><mat-icon aria-hidden="true">payments</mat-icon></span>
            </div>
            <div class="kpi-value">{{ kpi.revenueMonth | currency: 'USD' : 'symbol' : '1.0-0' }}</div>
            <div class="kpi-foot">
              <span class="delta" [class.negative]="kpi.comparison.revenuePercent < 0">
                <mat-icon aria-hidden="true">{{ kpi.comparison.revenuePercent < 0 ? 'trending_down' : 'trending_up' }}</mat-icon>
                {{ abs(kpi.comparison.revenuePercent) }}%
              </span>
              <span>vs last month</span>
            </div>
          </article>

          <article class="kpi-card kpi-violet occupancy-card">
            <div class="kpi-head">
              <span class="kpi-label">Occupancy now</span>
              <span class="kpi-icon"><mat-icon aria-hidden="true">sensors</mat-icon></span>
            </div>
            <div class="kpi-value">{{ kpi.occupancyPercent | number: '1.1-1' }}<small>%</small></div>
            <div class="kpi-foot">
              <span>{{ kpi.occupancyNow }} inside</span>
              <span>of {{ kpi.capacity | number }}</span>
            </div>
            <svg class="occupancy-ring" viewBox="0 0 42 42" aria-hidden="true">
              <circle class="ring-track" cx="21" cy="21" r="15.9" pathLength="100"></circle>
              <circle class="ring-fill" cx="21" cy="21" r="15.9" pathLength="100" [attr.stroke-dasharray]="ringDash(kpi.occupancyPercent)"></circle>
            </svg>
          </article>

          <article class="kpi-card kpi-teal">
            <div class="kpi-head">
              <span class="kpi-label">Check-ins today</span>
              <span class="kpi-icon"><mat-icon aria-hidden="true">login</mat-icon></span>
            </div>
            <div class="kpi-value">{{ kpi.checkInsToday | number }}</div>
            <div class="kpi-foot">
              <span>{{ totalCheckIns() | number }} in {{ days() }} days</span>
              <span class="delta"><mat-icon aria-hidden="true">bolt</mat-icon>Live</span>
            </div>
          </article>

          <article class="kpi-card kpi-blue">
            <div class="kpi-head">
              <span class="kpi-label">Bookings today</span>
              <span class="kpi-icon"><mat-icon aria-hidden="true">event_available</mat-icon></span>
            </div>
            <div class="kpi-value">{{ kpi.bookingsToday | number }}</div>
            <div class="kpi-foot">
              <span>{{ totalBookings() | number }} in {{ days() }} days</span>
              <span>{{ bookingFill(kpi) | number: '1.0-0' }}% of visits</span>
            </div>
          </article>

          <article class="kpi-card kpi-amber">
            <div class="kpi-head">
              <span class="kpi-label">Outstanding invoices</span>
              <span class="kpi-icon"><mat-icon aria-hidden="true">receipt_long</mat-icon></span>
            </div>
            <div class="kpi-value">{{ kpi.outstandingAmount | currency: 'USD' : 'symbol' : '1.0-0' }}</div>
            <div class="kpi-foot">
              <span class="delta warning"><mat-icon aria-hidden="true">notification_important</mat-icon>{{ kpi.outstandingInvoices }} overdue</span>
              <span>Needs follow-up</span>
            </div>
          </article>
        </section>

        <div class="dash-grid">
          <section class="panel trend-panel">
            <div class="panel-head">
              <div>
                <span class="panel-eyebrow">Flow</span>
                <h2>Visit &amp; booking trend</h2>
              </div>
              <div class="legend" aria-hidden="true">
                <span><i class="legend-dot visits"></i>Check-ins</span>
                <span><i class="legend-dot bookings"></i>Bookings</span>
              </div>
            </div>

            <div class="trend-chart" role="img" [attr.aria-label]="'Daily check-ins and bookings for the last ' + days() + ' days'">
              <div class="trend-bars">
                @for (point of trend(); track point.date; let i = $index) {
                  <div class="trend-day">
                    <div class="trend-columns">
                      <span
                        class="trend-bar visits"
                        [style.height.%]="barHeight(point.checkIns)"
                        [title]="point.label + ': ' + point.checkIns + ' check-ins'"
                      ></span>
                      <span
                        class="trend-bar bookings"
                        [style.height.%]="barHeight(point.bookings)"
                        [title]="point.label + ': ' + point.bookings + ' bookings'"
                      ></span>
                    </div>
                    <span class="trend-label" [class.quiet]="!showLabel(i)">{{ point.label }}</span>
                  </div>
                }
              </div>
            </div>

            <div class="trend-foot">
              <span><strong>{{ totalCheckIns() | number }}</strong> check-ins</span>
              <span><strong>{{ totalBookings() | number }}</strong> bookings</span>
              <span><strong>{{ days() }}</strong>-day window</span>
            </div>
          </section>

          <aside class="panel branch-panel">
            <div class="panel-head">
              <div>
                <span class="panel-eyebrow">Network</span>
                <h2>Branch pulse</h2>
              </div>
              <span class="muted">{{ branches().length }} locations</span>
            </div>

            <div class="branch-list">
              @for (branch of branches(); track branch.id) {
                <button
                  type="button"
                  class="branch-row"
                  [class.active]="branch.id === branchId()"
                  (click)="changeBranch(branch.id)"
                >
                  <span class="branch-top">
                    <span class="branch-name">{{ branch.name }}</span>
                    <span class="branch-pct">{{ branch.occupancyPercent | number: '1.0-1' }}%</span>
                  </span>
                  <span class="branch-city">{{ branch.city }}</span>
                  <span class="meter"><i [style.width.%]="branch.occupancyPercent"></i></span>
                  <span class="branch-meta">
                    <span>{{ branch.activeMembers | number }} active</span>
                    <span>{{ branch.occupancyNow }}/{{ branch.capacity }} inside</span>
                  </span>
                </button>
              } @empty {
                <p class="muted">No branch data available.</p>
              }
            </div>

            @if (branchId()) {
              <button mat-button type="button" class="clear-branch" (click)="changeBranch('')">
                <mat-icon aria-hidden="true">clear_all</mat-icon>
                Show all branches
              </button>
            }
          </aside>
        </div>

        <div class="dash-grid lower-grid">
          <section class="panel activity-panel">
            <div class="panel-head">
              <div>
                <span class="panel-eyebrow">Front desk</span>
                <h2>Live activity</h2>
              </div>
              <span class="muted">{{ activity().length }} recent events</span>
            </div>

            <ol class="activity-list">
              @for (item of activity(); track item.id) {
                <li class="activity-item">
                  <span class="activity-icon" [class]="'activity-tone-' + activityTone(item.kind)">
                    <mat-icon aria-hidden="true">{{ activityIcon(item.kind) }}</mat-icon>
                  </span>
                  <span class="activity-copy">
                    <strong>{{ item.title }}</strong>
                    <span>{{ item.detail }}</span>
                  </span>
                  <span class="activity-meta">
                    <span class="activity-branch">{{ item.branchName }}</span>
                    <time>{{ item.occurredAt | date: 'HH:mm' }}</time>
                  </span>
                </li>
              } @empty {
                <li class="empty-line">No activity recorded for this branch.</li>
              }
            </ol>
          </section>

          <section class="panel alerts-panel">
            <div class="panel-head">
              <div>
                <span class="panel-eyebrow">Operations inbox</span>
                <h2>Alerts &amp; tasks</h2>
              </div>
              <a class="panel-link" routerLink="/alerts">View inbox <mat-icon aria-hidden="true">arrow_forward</mat-icon></a>
            </div>

            <div class="severity-row">
              <div class="severity-tile critical">
                <span>Critical</span>
                <strong>{{ criticalCount() }}</strong>
              </div>
              <div class="severity-tile warning">
                <span>Warning</span>
                <strong>{{ warningCount() }}</strong>
              </div>
              <div class="severity-tile info">
                <span>Info</span>
                <strong>{{ infoCount() }}</strong>
              </div>
            </div>

            <div class="alert-list">
              @for (alert of topAlerts(); track alert.id) {
                <article class="alert-item" [class]="'severity-' + alert.severity.toLowerCase()">
                  <span class="alert-dot" aria-hidden="true"></span>
                  <div class="alert-copy">
                    <div class="alert-top">
                      <span class="alert-type">{{ alertType(alert.type) }}</span>
                      <span class="severity-chip">{{ alert.severity }}</span>
                    </div>
                    <strong>{{ alert.title }}</strong>
                    <p>{{ alert.detail }}</p>
                    <span class="alert-meta">{{ alert.branchName }} · due {{ alert.dueAt ? (alert.dueAt | date: 'MMM d, HH:mm') : 'anytime' }}</span>
                  </div>
                </article>
              } @empty {
                <p class="empty-line">No open alerts. The floor is clear.</p>
              }
            </div>

            <a mat-stroked-button class="alerts-link" routerLink="/alerts">
              Open alert inbox
              <mat-icon aria-hidden="true">north_east</mat-icon>
            </a>
          </section>
        </div>
      } @else if (loading()) {
        <div class="dashboard-loading" role="status">
          <mat-spinner diameter="52" />
          <span>Assembling the branch pulse…</span>
        </div>
      } @else if (!error()) {
        <div class="empty-state">
          <div class="empty-art"><mat-icon aria-hidden="true">insights</mat-icon></div>
          <h2>No dashboard snapshot yet</h2>
          <p>Seed the demo API or select another branch to begin.</p>
          <button mat-stroked-button type="button" (click)="refresh()">Try again</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .dashboard-page { display:flex; flex-direction:column; gap:16px; }
    .dash-hero { position:relative; overflow:hidden; display:flex; align-items:flex-end; justify-content:space-between; gap:24px; flex-wrap:wrap; padding:28px; border-radius:var(--radius-sheet); background:linear-gradient(135deg, #1b1e24 0%, #15171c 56%, #211a18 100%); border:1px solid var(--color-border); }
    .hero-glow { position:absolute; border-radius:999px; filter:blur(80px); pointer-events:none; }
    .hero-glow-a { width:340px; height:340px; top:-170px; inset-inline-end:80px; background:color-mix(in srgb, var(--color-accent-fill) 26%, transparent); }
    .hero-glow-b { width:260px; height:260px; bottom:-180px; inset-inline-start:35%; background:color-mix(in srgb, var(--color-info) 14%, transparent); }
    .hero-main { position:relative; display:flex; flex-direction:column; gap:8px; flex:1; min-width:320px; }
    .eyebrow { display:inline-flex; align-items:center; gap:8px; font-size:11px; font-weight:700; letter-spacing:.16em; text-transform:uppercase; color:var(--color-accent-text); }
    .live-dot { width:8px; height:8px; border-radius:999px; background:var(--color-accent-fill); box-shadow:0 0 0 4px color-mix(in srgb, var(--color-accent-fill) 18%, transparent); }
    .dash-hero h1 { margin:0; font-size:clamp(30px, 2.4vw, 42px); letter-spacing:-.035em; line-height:1.05; }
    .hero-sub { margin:0; max-width:62ch; color:var(--color-text-muted); font-size:14px; line-height:1.65; }
    .hero-chips { display:flex; gap:8px; flex-wrap:wrap; margin-top:6px; }
    .hero-chip { display:inline-flex; align-items:center; gap:5px; padding:5px 10px; border-radius:999px; background:color-mix(in srgb, var(--color-surface-raised) 80%, transparent); border:1px solid var(--color-border); color:var(--color-text-muted); font-size:11px; font-weight:600; }
    .hero-chip mat-icon { font-size:14px; width:14px; height:14px; color:var(--color-text); }
    .hero-chip-critical { color:var(--color-error); border-color:color-mix(in srgb, var(--color-error) 45%, transparent); background:color-mix(in srgb, var(--color-error) 10%, transparent); }
    .hero-chip-critical mat-icon { color:var(--color-error); }
    .hero-controls { position:relative; display:flex; align-items:center; gap:10px; flex-wrap:wrap; }
    .branch-field { width:230px; margin-bottom:-20px; }
    .range-toggle { display:inline-flex; padding:4px; border-radius:12px; background:var(--color-surface); border:1px solid var(--color-border); }
    .range-toggle button { border:0; background:transparent; color:var(--color-text-muted); font-family:inherit; font-size:12px; font-weight:700; padding:8px 12px; border-radius:9px; cursor:pointer; min-height:36px; }
    .range-toggle button.on { background:var(--color-accent-fill); color:var(--color-on-accent); }
    .kpi-grid { display:grid; grid-template-columns:1fr; gap:12px; }
    .kpi-card { position:relative; overflow:hidden; display:flex; flex-direction:column; gap:10px; padding:18px; border-radius:18px; background:var(--color-surface-raised); border:1px solid var(--color-border); min-height:148px; }
    .kpi-card::before { content:''; position:absolute; inset-inline:0; top:0; height:3px; background:var(--kpi-accent); }
    .kpi-head { display:flex; align-items:flex-start; justify-content:space-between; gap:10px; }
    .kpi-label { font-size:11px; font-weight:700; letter-spacing:.09em; text-transform:uppercase; color:var(--color-text-muted); }
    .kpi-icon { width:36px; height:36px; border-radius:11px; display:inline-flex; align-items:center; justify-content:center; background:color-mix(in srgb, var(--kpi-accent) 14%, transparent); color:var(--kpi-accent); flex:none; }
    .kpi-icon mat-icon { font-size:19px; width:19px; height:19px; }
    .kpi-value { font-size:clamp(26px, 2.5vw, 34px); font-weight:800; letter-spacing:-.035em; line-height:1; color:var(--color-text); font-variant-numeric:tabular-nums; }
    .kpi-value small { font-size:16px; font-weight:700; margin-inline-start:2px; color:var(--color-text-muted); }
    .kpi-foot { display:flex; align-items:center; justify-content:space-between; gap:8px; flex-wrap:wrap; margin-top:auto; font-size:11px; color:var(--color-text-muted); }
    .delta { display:inline-flex; align-items:center; gap:3px; font-weight:700; color:var(--color-success); }
    .delta.negative { color:var(--color-error); }
    .delta.warning { color:var(--color-warning); }
    .delta mat-icon { font-size:14px; width:14px; height:14px; }
    .kpi-warm { --kpi-accent:#FF7A00; }
    .kpi-cool { --kpi-accent:#42A5F5; }
    .kpi-violet { --kpi-accent:#AB47BC; }
    .kpi-teal { --kpi-accent:#34D399; }
    .kpi-blue { --kpi-accent:#60A5FA; }
    .kpi-amber { --kpi-accent:#FBBF24; }
    .occupancy-card { padding-inline-end:74px; }
    .occupancy-ring { position:absolute; inset-inline-end:16px; bottom:16px; width:48px; height:48px; transform:rotate(-90deg); }
    .ring-track { fill:none; stroke:var(--color-border); stroke-width:3.4; }
    .ring-fill { fill:none; stroke:var(--kpi-accent); stroke-width:3.4; stroke-linecap:round; transition:stroke-dasharray .5s ease; }
    .dash-grid { display:grid; grid-template-columns:1fr; gap:12px; align-items:start; }
    .panel { display:flex; flex-direction:column; gap:14px; padding:20px; border-radius:20px; background:var(--color-surface-raised); border:1px solid var(--color-border); min-width:0; }
    .panel-head { display:flex; align-items:flex-start; justify-content:space-between; gap:12px; flex-wrap:wrap; }
    .panel-head h2 { margin:2px 0 0; font-size:17px; letter-spacing:-.02em; }
    .panel-eyebrow { font-size:10px; font-weight:700; letter-spacing:.15em; text-transform:uppercase; color:var(--color-accent-text); }
    .muted { font-size:11px; color:var(--color-text-muted); }
    .legend { display:flex; gap:12px; font-size:11px; color:var(--color-text-muted); }
    .legend span { display:inline-flex; align-items:center; gap:5px; }
    .legend-dot { width:8px; height:8px; border-radius:3px; }
    .legend-dot.visits { background:#FF7A00; }
    .legend-dot.bookings { background:#42A5F5; }
    .trend-chart { height:220px; padding-top:6px; }
    .trend-bars { display:flex; align-items:flex-end; gap:4px; height:100%; }
    .trend-day { display:flex; flex-direction:column; align-items:center; justify-content:flex-end; gap:7px; flex:1; min-width:0; height:100%; }
    .trend-columns { display:flex; align-items:flex-end; justify-content:center; gap:3px; width:100%; height:calc(100% - 24px); }
    .trend-bar { flex:1; max-width:17px; min-height:0; border-radius:4px 4px 2px 2px; transition:height .35s ease, filter .15s ease; }
    .trend-bar:hover { filter:brightness(1.18); }
    .trend-bar.visits { background:var(--gradient-barRevenue); }
    .trend-bar.bookings { background:var(--gradient-barVisits); }
    .trend-label { font-size:9px; color:var(--color-text-muted); white-space:nowrap; }
    .trend-label.quiet { visibility:hidden; }
    .trend-foot { display:flex; gap:16px; flex-wrap:wrap; padding-top:12px; border-top:1px solid var(--color-border); font-size:11px; color:var(--color-text-muted); }
    .trend-foot strong { color:var(--color-text); font-size:13px; }
    .branch-list { display:flex; flex-direction:column; gap:8px; }
    .branch-row { display:flex; flex-direction:column; gap:5px; width:100%; padding:12px; text-align:start; border-radius:12px; border:1px solid transparent; background:var(--color-surface); color:var(--color-text); font-family:inherit; cursor:pointer; transition:border-color .18s ease, background .18s ease; }
    .branch-row:hover, .branch-row.active { border-color:color-mix(in srgb, var(--color-accent-fill) 40%, transparent); background:color-mix(in srgb, var(--color-accent-fill) 6%, var(--color-surface)); }
    .branch-top { display:flex; align-items:center; justify-content:space-between; gap:8px; }
    .branch-name { font-size:13px; font-weight:700; }
    .branch-pct { font-size:12px; font-weight:800; color:var(--color-accent-text); font-variant-numeric:tabular-nums; }
    .branch-city { font-size:11px; color:var(--color-text-muted); }
    .meter { height:6px; border-radius:999px; background:var(--color-border); overflow:hidden; }
    .meter i { display:block; height:100%; border-radius:999px; background:linear-gradient(90deg, var(--color-accent-pressed), var(--color-accent-hover)); }
    .branch-meta { display:flex; justify-content:space-between; gap:8px; font-size:10px; color:var(--color-text-muted); }
    .clear-branch { align-self:flex-start; margin-top:-4px; }
    .lower-grid { grid-template-columns:1fr; }
    .activity-list { list-style:none; margin:0; padding:0; display:flex; flex-direction:column; }
    .activity-item { display:flex; align-items:flex-start; gap:11px; padding:11px 0; border-bottom:1px solid var(--color-border); }
    .activity-item:last-child { border-bottom:0; padding-bottom:0; }
    .activity-icon { width:34px; height:34px; border-radius:10px; display:inline-flex; align-items:center; justify-content:center; flex:none; background:var(--color-surface); color:var(--color-text-muted); }
    .activity-icon mat-icon { font-size:17px; width:17px; height:17px; }
    .activity-tone-checkin { background:color-mix(in srgb, #34D399 14%, transparent); color:#34D399; }
    .activity-tone-payment { background:color-mix(in srgb, #FBBF24 14%, transparent); color:#FBBF24; }
    .activity-tone-member { background:color-mix(in srgb, #FF7A00 14%, transparent); color:#FF7A00; }
    .activity-tone-booking { background:color-mix(in srgb, #60A5FA 14%, transparent); color:#60A5FA; }
    .activity-tone-membership { background:color-mix(in srgb, #AB47BC 14%, transparent); color:#AB47BC; }
    .activity-tone-notification { background:color-mix(in srgb, #22D3EE 14%, transparent); color:#22D3EE; }
    .activity-copy { display:flex; flex-direction:column; gap:2px; flex:1; min-width:0; }
    .activity-copy strong { font-size:12px; font-weight:600; color:var(--color-text); }
    .activity-copy span { font-size:11px; color:var(--color-text-muted); }
    .activity-meta { display:flex; flex-direction:column; align-items:flex-end; gap:2px; flex:none; font-size:10px; color:var(--color-text-muted); }
    .activity-branch { max-width:150px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
    .activity-meta time { font-weight:700; color:var(--color-text); font-variant-numeric:tabular-nums; }
    .severity-row { display:grid; grid-template-columns:repeat(3,1fr); gap:8px; }
    .severity-tile { padding:10px 12px; border-radius:12px; background:var(--color-surface); display:flex; flex-direction:column; gap:2px; border:1px solid transparent; }
    .severity-tile span { font-size:9px; font-weight:700; letter-spacing:.09em; text-transform:uppercase; }
    .severity-tile strong { font-size:21px; line-height:1; font-weight:800; }
    .severity-tile.critical { color:var(--color-error); border-color:color-mix(in srgb, var(--color-error) 30%, transparent); background:color-mix(in srgb, var(--color-error) 8%, var(--color-surface)); }
    .severity-tile.warning { color:var(--color-warning); border-color:color-mix(in srgb, var(--color-warning) 30%, transparent); background:color-mix(in srgb, var(--color-warning) 8%, var(--color-surface)); }
    .severity-tile.info { color:var(--color-info); border-color:color-mix(in srgb, var(--color-info) 30%, transparent); background:color-mix(in srgb, var(--color-info) 8%, var(--color-surface)); }
    .alert-list { display:flex; flex-direction:column; gap:8px; }
    .alert-item { display:flex; gap:10px; padding:11px 12px; border-radius:12px; background:var(--color-surface); border:1px solid transparent; }
    .alert-dot { width:8px; height:8px; border-radius:999px; margin-top:5px; flex:none; }
    .severity-critical { border-color:color-mix(in srgb, var(--color-error) 26%, transparent); }
    .severity-critical .alert-dot { background:var(--color-error); box-shadow:0 0 0 4px color-mix(in srgb, var(--color-error) 12%, transparent); }
    .severity-warning { border-color:color-mix(in srgb, var(--color-warning) 24%, transparent); }
    .severity-warning .alert-dot { background:var(--color-warning); }
    .severity-info { border-color:color-mix(in srgb, var(--color-info) 22%, transparent); }
    .severity-info .alert-dot { background:var(--color-info); }
    .alert-copy { display:flex; flex-direction:column; gap:3px; flex:1; min-width:0; }
    .alert-top { display:flex; align-items:center; gap:6px; flex-wrap:wrap; }
    .alert-type { font-size:9px; font-weight:700; text-transform:uppercase; letter-spacing:.1em; color:var(--color-text-muted); }
    .severity-chip { font-size:9px; font-weight:700; text-transform:uppercase; letter-spacing:.08em; padding:2px 7px; border-radius:999px; background:var(--color-surface-raised); color:var(--color-text-muted); }
    .severity-critical .severity-chip { color:var(--color-error); }
    .severity-warning .severity-chip { color:var(--color-warning); }
    .severity-info .severity-chip { color:var(--color-info); }
    .alert-copy strong { font-size:12px; }
    .alert-copy p { margin:0; font-size:11px; color:var(--color-text-muted); line-height:1.5; }
    .alert-meta { font-size:10px; color:var(--color-text-muted); }
    .panel-link { display:inline-flex; align-items:center; gap:3px; font-size:11px; font-weight:700; color:var(--color-accent-text); }
    .panel-link mat-icon { font-size:14px; width:14px; height:14px; }
    .alerts-link { align-self:flex-start; }
    .alerts-link mat-icon { margin-inline-start:4px; }
    .empty-line { margin:0; padding:12px 0; font-size:12px; color:var(--color-text-muted); }
    .dashboard-loading { display:flex; flex-direction:column; align-items:center; justify-content:center; gap:12px; min-height:320px; color:var(--color-text-muted); }
    .notice { display:flex; align-items:center; gap:8px; }
    @media (min-width: 640px) {
      .kpi-grid { grid-template-columns:repeat(2, minmax(0,1fr)); }
    }
    @media (min-width: 980px) {
      .kpi-grid { grid-template-columns:repeat(3, minmax(0,1fr)); }
      .dash-grid { grid-template-columns:1.55fr .75fr; }
      .lower-grid { grid-template-columns:1.3fr 1fr; }
    }
    @media (max-width: 700px) {
      .dash-hero { padding:22px; }
      .branch-field { width:100%; }
      .hero-controls { width:100%; }
      .trend-label { font-size:8px; }
    }
    @media (prefers-reduced-motion: reduce) {
      .trend-bar, .ring-fill { transition:none; }
    }
  `],
})
export class DashboardOverviewComponent implements OnInit {
  readonly facade = inject(StaffDashboardFacade);

  readonly summary = this.facade.summary;
  readonly trend = this.facade.trend;
  readonly activity = this.facade.activity;
  readonly branches = this.facade.branches;
  readonly branchId = this.facade.branchId;
  readonly days = this.facade.days;
  readonly loading = this.facade.loading;
  readonly error = this.facade.error;
  readonly generatedAt = this.facade.generatedAt;
  readonly criticalCount = this.facade.criticalCount;
  readonly warningCount = this.facade.warningCount;
  readonly infoCount = this.facade.infoCount;
  readonly totalCheckIns = this.facade.totalCheckIns;
  readonly totalBookings = this.facade.totalBookings;
  readonly topAlerts = computed(() => this.facade.alerts().slice(0, 4));

  ngOnInit(): void {
    void this.facade.load();
  }

  changeBranch(branchId: string): void {
    void this.facade.setBranch(branchId || null);
  }

  setRange(days: DashboardDays): void {
    void this.facade.setRange(days);
  }

  refresh(): void {
    void this.facade.refresh();
  }

  barHeight(value: number): number {
    const max = this.facade.maxTrend();
    if (value <= 0) return 0;
    return Math.max(6, Math.round((value / max) * 100));
  }

  showLabel(index: number): boolean {
    if (this.days() === 7) return true;
    return index % 5 === 0 || index === this.trend().length - 1;
  }

  ringDash(percent: number): string {
    const value = Math.max(0, Math.min(100, percent));
    return `${value} ${100 - value}`;
  }

  abs(value: number): number {
    return Math.abs(value);
  }

  bookingFill(kpi: { checkInsToday: number; bookingsToday: number }): number {
    if (kpi.checkInsToday === 0) return 0;
    return Math.round((kpi.bookingsToday / kpi.checkInsToday) * 100);
  }

  activityTone(kind: string): string {
    const tones: Record<string, string> = {
      CheckIn: 'checkin',
      Payment: 'payment',
      MemberJoined: 'member',
      ClassBooking: 'booking',
      Membership: 'membership',
      Notification: 'notification',
    };
    return tones[kind] ?? 'default';
  }

  activityIcon(kind: string): string {
    const icons: Record<string, string> = {
      CheckIn: 'login',
      Payment: 'payments',
      MemberJoined: 'person_add',
      ClassBooking: 'event_available',
      Membership: 'card_membership',
      Notification: 'notifications',
    };
    return icons[kind] ?? 'bolt';
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
