import { Injectable, computed, inject, signal } from '@angular/core';
import {
  Branch,
  CHECKINS_GATEWAY,
  CheckInHistoryQuery,
  CheckInVisit,
  PagedResult,
} from './check-in.model';
import { checkInErrorMessage } from './check-in.utils';

/** Filter state for the visit-history table. */
export interface VisitHistoryFilters {
  readonly search: string;
  readonly status: '' | 'open' | 'closed';
  readonly branchId: string;
}

const EMPTY_FILTERS: VisitHistoryFilters = { search: '', status: '', branchId: '' };

/** Signal facade for paginated visit history and filters. */
@Injectable()
export class VisitHistoryFacade {
  private readonly gateway = inject(CHECKINS_GATEWAY);

  readonly branches = signal<Branch[]>([]);
  readonly filters = signal<VisitHistoryFilters>(EMPTY_FILTERS);
  readonly history = signal<PagedResult<CheckInVisit> | null>(null);
  readonly page = signal(1);
  readonly pageSize = 10;
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly hasFilters = computed(() => {
    const filters = this.filters();
    return filters.search.length > 0 || filters.status.length > 0 || filters.branchId.length > 0;
  });
  readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.history()?.totalCount ?? 0) / this.pageSize)),
  );

  /** Loads branch filter options and the first page of visits. */
  async initialize(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      this.branches.set(await this.gateway.branches());
      await this.loadHistory();
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  /** Applies a free-text search. */
  async search(search: string): Promise<void> {
    this.filters.update((filters) => ({ ...filters, search }));
    this.page.set(1);
    await this.load();
  }

  /** Filters by open or closed visits. */
  async setStatus(status: VisitHistoryFilters['status']): Promise<void> {
    this.filters.update((filters) => ({ ...filters, status }));
    this.page.set(1);
    await this.load();
  }

  /** Filters by branch, or all branches when empty. */
  async setBranch(branchId: string): Promise<void> {
    this.filters.update((filters) => ({ ...filters, branchId }));
    this.page.set(1);
    await this.load();
  }

  /** Moves to a specific page. */
  async setPage(page: number): Promise<void> {
    this.page.set(page);
    await this.load();
  }

  /** Reloads the current page. */
  async refresh(): Promise<void> {
    await this.load();
  }

  /** Clears every filter. */
  async clearFilters(): Promise<void> {
    this.filters.set(EMPTY_FILTERS);
    this.page.set(1);
    await this.load();
  }

  /** Dismisses the current error. */
  clearError(): void {
    this.error.set(null);
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      await this.loadHistory();
    } catch (error) {
      this.error.set(checkInErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  private async loadHistory(): Promise<void> {
    const filters = this.filters();
    const query: CheckInHistoryQuery = {
      search: filters.search || undefined,
      status: filters.status || undefined,
      branchId: filters.branchId || undefined,
      page: this.page(),
      pageSize: this.pageSize,
    };
    this.history.set(await this.gateway.history(query));
  }
}
