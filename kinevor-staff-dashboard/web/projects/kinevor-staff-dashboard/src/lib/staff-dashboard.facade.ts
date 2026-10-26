import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import {
  DashboardAlerts,
  DashboardDays,
  STAFF_DASHBOARD_GATEWAY,
  StaffDashboard,
} from './staff-dashboard.model';

@Injectable()
export class StaffDashboardFacade {
  private readonly gateway = inject(STAFF_DASHBOARD_GATEWAY);

  private readonly dashboardState = signal<StaffDashboard | null>(null);
  private readonly alertsState = signal<DashboardAlerts | null>(null);
  private readonly loadingState = signal(false);
  private readonly alertsLoadingState = signal(false);
  private readonly errorState = signal<string | null>(null);
  private readonly branchState = signal<string | null>(null);
  private readonly rangeState = signal<DashboardDays>(7);

  readonly dashboard = this.dashboardState.asReadonly();
  readonly alertsInbox = this.alertsState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly alertsLoading = this.alertsLoadingState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly branchId = this.branchState.asReadonly();
  readonly days = this.rangeState.asReadonly();

  readonly summary = computed(() => this.dashboardState()?.summary ?? null);
  readonly trend = computed(() => this.dashboardState()?.trend ?? []);
  readonly activity = computed(() => this.dashboardState()?.activity ?? []);
  readonly dashboardAlerts = computed(() => this.dashboardState()?.alerts ?? []);
  readonly alerts = computed(
    () => this.alertsState()?.alerts ?? this.dashboardState()?.alerts ?? [],
  );
  readonly branches = computed(
    () => this.dashboardState()?.branches ?? this.alertsState()?.branches ?? [],
  );
  readonly branchName = computed(
    () => this.dashboardState()?.branchName ?? this.alertsState()?.branchName ?? 'All branches',
  );
  readonly generatedAt = computed(
    () => this.dashboardState()?.generatedAt ?? this.alertsState()?.generatedAt ?? null,
  );

  readonly criticalCount = computed(
    () => this.alerts().filter((alert) => alert.severity === 'Critical').length,
  );
  readonly warningCount = computed(
    () => this.alerts().filter((alert) => alert.severity === 'Warning').length,
  );
  readonly infoCount = computed(
    () => this.alerts().filter((alert) => alert.severity === 'Info').length,
  );
  readonly totalCheckIns = computed(() =>
    this.trend().reduce((total, point) => total + point.checkIns, 0),
  );
  readonly totalBookings = computed(() =>
    this.trend().reduce((total, point) => total + point.bookings, 0),
  );
  readonly maxTrend = computed(() =>
    this.trend().reduce(
      (max, point) => Math.max(max, point.checkIns, point.bookings),
      1,
    ),
  );

  async load(): Promise<void> {
    this.loadingState.set(true);
    this.errorState.set(null);

    try {
      const dashboard = await this.gateway.get({
        branchId: this.branchState(),
        days: this.rangeState(),
      });
      this.dashboardState.set(dashboard);
    } catch (error) {
      this.errorState.set(this.messageFor(error));
    } finally {
      this.loadingState.set(false);
    }
  }

  async loadAlerts(): Promise<void> {
    this.alertsLoadingState.set(true);
    this.errorState.set(null);

    try {
      const inbox = await this.gateway.alerts(this.branchState());
      this.alertsState.set(inbox);
    } catch (error) {
      this.errorState.set(this.messageFor(error));
    } finally {
      this.alertsLoadingState.set(false);
    }
  }

  async setBranch(branchId: string | null): Promise<void> {
    this.branchState.set(branchId || null);
    await this.load();
    if (this.alertsState()) await this.loadAlerts();
  }

  async setRange(days: DashboardDays): Promise<void> {
    this.rangeState.set(days);
    await this.load();
  }

  async refresh(): Promise<void> {
    await this.load();
    if (this.alertsState()) await this.loadAlerts();
  }

  async refreshAlerts(): Promise<void> {
    await this.loadAlerts();
  }

  private messageFor(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'The staff dashboard API is not reachable. Start the demo host on port 5089.';
      }
      if (error.status === 404) return 'The selected branch or dashboard snapshot could not be found.';
      if (error.status === 400) {
        return (error.error as { message?: string } | null)?.message ?? 'The dashboard range is invalid.';
      }
      return (error.error as { message?: string } | null)?.message ?? 'The request failed. Try again.';
    }

    return 'The request failed. Try again.';
  }
}
