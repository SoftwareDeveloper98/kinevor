import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { formatCheckInDate, formatDuration, initials } from '../check-in.utils';
import { VisitHistoryFacade, VisitHistoryFilters } from '../visit-history.facade';

/** Searchable, filterable and paginated visit history. */
@Component({
  selector: 'kinevor-visit-history',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './visit-history.component.html',
  styleUrl: './visit-history.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VisitHistoryComponent implements OnInit, OnDestroy {
  readonly facade = inject(VisitHistoryFacade);
  readonly initials = initials;
  readonly formatDate = formatCheckInDate;
  readonly formatDuration = formatDuration;

  private searchTimer: ReturnType<typeof setTimeout> | undefined;

  /** Loads branches and visit history. */
  ngOnInit(): void {
    void this.facade.initialize();
  }

  /** Cancels pending search input work. */
  ngOnDestroy(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
  }

  /** Debounces free-text history search. */
  onSearch(event: Event): void {
    const search = (event.target as HTMLInputElement).value;
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      void this.facade.search(search);
    }, 250);
  }

  /** Applies an open-state filter. */
  setStatus(status: VisitHistoryFilters['status']): void {
    void this.facade.setStatus(status);
  }

  /** Applies a branch filter. */
  onBranchChange(event: Event): void {
    void this.facade.setBranch((event.target as HTMLSelectElement).value);
  }

  /** Moves to a specific page. */
  goToPage(page: number): void {
    if (page >= 1 && page <= this.facade.pageCount()) {
      void this.facade.setPage(page);
    }
  }

  /** Reloads history. */
  refresh(): void {
    void this.facade.refresh();
  }

  /** Clears every active filter. */
  clearFilters(): void {
    void this.facade.clearFilters();
  }

  /** Returns the first visible result number. */
  rangeStart(): number {
    const result = this.facade.history();
    return result && result.totalCount > 0 ? (result.page - 1) * result.pageSize + 1 : 0;
  }

  /** Returns the last visible result number. */
  rangeEnd(): number {
    const result = this.facade.history();
    return result ? Math.min(result.page * result.pageSize, result.totalCount) : 0;
  }
}
