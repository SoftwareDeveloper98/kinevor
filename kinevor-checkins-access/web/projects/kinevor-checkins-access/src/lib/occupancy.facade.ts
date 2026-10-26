import { Injectable, computed, inject, signal } from '@angular/core';
import { Branch, CHECKINS_GATEWAY, DailyStats, Occupancy } from './check-in.model';
import { checkInErrorMessage } from './check-in.utils';

/** Signal facade for the live branch occupancy dashboard. */
@Injectable()
export class OccupancyFacade {
  private readonly gateway = inject(CHECKINS_GATEWAY);

  readonly branches = signal<Branch[]>([]);
  readonly branchId = signal<string | null>(null);
  readonly occupancy = signal<Occupancy | null>(null);
  readonly stats = signal<DailyStats | null>(null);
  readonly loading = signal(false);
  readonly actionLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);
  readonly refreshedAt = signal<Date | null>(null);

  readonly branch = computed(
    () => this.branches().find((item) => item.id === this.branchId()) ?? null,
  );
  readonly openVisits = computed(() => this.occupancy()?.openVisits ?? []);

  /** Loads branches and the preferred branch dashboard. */
  async initialize(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const branches = await this.gateway.branches();
      this.branches.set(branches);
      const preferred = branches.find((item) => item.code === 'SHR') ?? branches.at(0) ?? null;
      this.branchId.set(preferred?.id ?? null);
      await this.loadDashboard();
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  /** Switches branches and refreshes the dashboard. */
  async selectBranch(branchId: string): Promise<void> {
    if (branchId === this.branchId()) return;
    this.branchId.set(branchId);
    await this.refresh();
  }

  /** Reloads occupancy and daily statistics. */
  async refresh(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      await this.loadDashboard();
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  /** Checks out a member from the live visit list. */
  async checkOut(checkInId: string): Promise<boolean> {
    this.actionLoading.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      const visit = await this.gateway.checkOut(checkInId);
      this.notice.set(`${visit.memberName} checked out.`);
      await this.loadDashboard();
      return true;
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
      return false;
    } finally {
      this.actionLoading.set(false);
    }
  }

  /** Dismisses the current notice. */
  clearNotice(): void {
    this.notice.set(null);
  }

  /** Dismisses the current error. */
  clearError(): void {
    this.error.set(null);
  }

  private async loadDashboard(): Promise<void> {
    const [occupancy, stats] = await Promise.all([
      this.gateway.occupancy(this.branchId()),
      this.gateway.stats(this.branchId()),
    ]);
    this.occupancy.set(occupancy);
    this.stats.set(stats);
    this.refreshedAt.set(new Date());
  }
}
