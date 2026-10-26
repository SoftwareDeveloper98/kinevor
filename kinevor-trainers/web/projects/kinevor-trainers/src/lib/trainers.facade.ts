import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, catchError, map, tap, throwError } from 'rxjs';
import { NoticeService } from '@softwaredeveloper98/kinevor-platform-web';
import { TrainerSummary } from './trainer.model';
import { TRAINERS_GATEWAY, TrainerListQuery } from './trainers.gateway';

/** Trainers feature state exposed to the staff pages and pickers. */
export interface TrainersState {
  /** Current page of trainers. */
  items: TrainerSummary[];
  /** Total trainer count matching the query. */
  total: number;
  /** Whether a load is in flight. */
  loading: boolean;
  /** Currently viewed trainer, when on the detail page. */
  detail: TrainerSummary | null;
  /** Whether the detail load is in flight. */
  detailLoading: boolean;
}

/**
 * Staff trainer directory state and use cases. Pages and dialogs call the
 * actions and read the signals; all HTTP details live in {@link TRAINERS_GATEWAY}.
 */
@Injectable({ providedIn: 'root' })
export class TrainersFacade {
  private readonly gateway = inject(TRAINERS_GATEWAY);
  private readonly notice = inject(NoticeService);

  private readonly _state = signal<TrainersState>({
    items: [],
    total: 0,
    loading: false,
    detail: null,
    detailLoading: false,
  });
  private readonly _query = signal<TrainerListQuery>({ page: 1, pageSize: 9 });

  /** Current page of trainers. */
  readonly trainers = computed(() => this._state().items);
  /** Total trainer count matching the query. */
  readonly total = computed(() => this._state().total);
  /** Whether a load is in flight. */
  readonly loading = computed(() => this._state().loading);
  /** Currently viewed trainer, when on the detail page. */
  readonly detail = computed(() => this._state().detail);
  /** Whether the detail load is in flight. */
  readonly detailLoading = computed(() => this._state().detailLoading);
  /** Current paging and search query. */
  readonly query = this._query.asReadonly();

  /** Loads the trainer page described by the current query. */
  load(): void {
    this._state.update(s => ({ ...s, loading: true }));
    this.gateway.list(this._query()).subscribe({
      next: page => this._state.update(s => ({
        ...s,
        items: page.items,
        total: page.totalCount,
        loading: false,
      })),
      error: () => {
        this._state.update(s => ({ ...s, loading: false }));
        this.notice.error(null, 'error.unknown');
      },
    });
  }

  /**
   * Updates the query and reloads.
   *
   * @param patch - Query fields to change.
   */
  updateQuery(patch: Partial<TrainerListQuery>): void {
    this._query.update(q => ({ ...q, ...patch }));
    this.load();
  }

  /**
   * Loads one trainer profile into facade state.
   *
   * @param id - Trainer identifier.
   * @returns Completion of the load, or an error when the trainer is missing.
   */
  loadDetail(id: string): Observable<void> {
    this._state.update(s => ({ ...s, detailLoading: true, detail: null }));

    return this.gateway.byId(id).pipe(
      tap(detail => this._state.update(s => ({ ...s, detail, detailLoading: false }))),
      map(() => undefined),
      catchError(error => {
        this._state.update(s => ({ ...s, detailLoading: false }));
        return throwError(() => error);
      }),
    );
  }

  /**
   * Loads a single trainer profile.
   *
   * @param id - Trainer identifier.
   * @returns The trainer record.
   */
  byId(id: string): Observable<TrainerSummary> {
    return this.gateway.byId(id);
  }

  /**
   * Loads a large page of trainers for pickers that need the full directory.
   *
   * @param pageSize - Maximum number of trainers to return.
   * @returns The requested page of trainers.
   */
  forPicker(pageSize = 100): Observable<TrainerSummary[]> {
    return new Observable<TrainerSummary[]>(subscriber => {
      this.gateway.list({ page: 1, pageSize }).subscribe({
        next: page => {
          subscriber.next(page.items);
          subscriber.complete();
        },
        error: error => subscriber.error(error),
      });
    });
  }
}
