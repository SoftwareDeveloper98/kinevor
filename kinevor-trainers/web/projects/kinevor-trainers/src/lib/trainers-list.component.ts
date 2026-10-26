import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { I18nService, TranslatePipe } from '@softwaredeveloper98/kinevor-platform-web';
import { TrainersFacade } from './trainers.facade';

/**
 * Staff trainer directory page: debounced search, star ratings and paging,
 * driven entirely by {@link TrainersFacade}.
 */
@Component({
  selector: 'kinevor-trainers-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    SlicePipe,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    TranslatePipe,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1 class="page-title">{{ 'trainers.title' | translate }}</h1>
      </div>

      <div class="filters-row">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>{{ 'trainers.search' | translate }}</mat-label>
          <input matInput [(ngModel)]="search" (ngModelChange)="search$.next($event)" />
          <mat-icon matSuffix aria-hidden="true">search</mat-icon>
        </mat-form-field>
      </div>

      @if (loading()) {
        <div class="table-loading" role="status">
          <mat-spinner diameter="48" />
          <span class="sr-only">{{ 'common.loading' | translate }}</span>
        </div>
      }

      @if (!loading()) {
        <div class="trainer-grid">
          @for (t of trainers(); track t.id) {
            <mat-card class="gym-card trainer-card">
              <mat-card-content>
                <div class="trainer-header">
                  <div class="trainer-avatar" aria-hidden="true">{{ t.fullName.charAt(0) }}</div>
                  <div class="trainer-stars" [attr.aria-label]="ratingLabel(t.rating)">
                    @for (s of stars(t.rating); track $index) {
                      <mat-icon class="star" [class.half]="s === 0.5" aria-hidden="true">
                        {{ s === 1 ? 'star' : s === 0.5 ? 'star_half' : 'star_border' }}
                      </mat-icon>
                    }
                  </div>
                </div>
                <div class="trainer-name">{{ t.fullName }}</div>
                <div class="trainer-exp">{{ yearsExp(t.yearsExperience) }}</div>
                <div class="trainer-specs">
                  @for (sp of t.specializations; track sp) {
                    <span class="spec-tag">{{ sp }}</span>
                  }
                </div>
                <p class="trainer-bio">{{ t.bio | slice: 0 : 100 }}{{ t.bio.length > 100 ? '…' : '' }}</p>
                <div class="trainer-contact">
                  <div><mat-icon aria-hidden="true">email</mat-icon>{{ t.email }}</div>
                  <div><mat-icon aria-hidden="true">phone</mat-icon>{{ t.phone }}</div>
                </div>
                <span class="status-chip" [class]="'status-' + t.status.toLowerCase()">
                  {{ ('status.' + t.status.toLowerCase()) | translate }}
                </span>
              </mat-card-content>
            </mat-card>
          }

          @if (trainers().length === 0) {
            <div class="empty-state" role="status" style="grid-column:1/-1">
              <mat-icon aria-hidden="true">sports</mat-icon>
              <p>{{ 'trainers.empty' | translate }}</p>
            </div>
          }
        </div>

        <mat-paginator
          [length]="total()"
          [pageSize]="pageSize"
          [pageSizeOptions]="[9, 18, 36]"
          (page)="onPage($event)"
          style="background:transparent;margin-top:8px"
          [attr.aria-label]="'common.pagination' | translate"
        />
      }
    </div>
  `,
  styles: [`
    .filters-row { display:flex; margin-bottom:20px; }
    .search-field { max-width:400px; width:100%; }
    .trainer-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(300px,1fr)); gap:16px; }
    .trainer-card { overflow:hidden; }
    .trainer-card mat-card-content { padding:24px !important; }
    .trainer-header { display:flex; align-items:center; justify-content:space-between; margin-bottom:12px; }
    .trainer-avatar { width:60px; height:60px; border-radius:50%; background:var(--gradient-brandWarm); color:var(--color-on-accent); font-size:24px; font-weight:800; display:flex; align-items:center; justify-content:center; }
    .trainer-stars { display:flex; }
    .star { font-size:18px; width:18px; height:18px; color:var(--color-status-rating); }
    .trainer-name { font-size:20px; font-weight:700; color:var(--color-text); margin-bottom:2px; }
    .trainer-exp { font-size:12px; color:var(--color-text-muted); margin-bottom:10px; }
    .trainer-specs { display:flex; flex-wrap:wrap; gap:6px; margin-bottom:12px; }
    .spec-tag { background:rgba(230,81,0,.15); color:var(--color-accent-text); font-size:11px; font-weight:600; padding:3px 10px; border-radius:10px; }
    .trainer-bio { font-size:13px; color:var(--color-text-muted); line-height:1.5; margin-bottom:12px; }
    .trainer-contact { margin-bottom:12px; }
    .trainer-contact div { display:flex; align-items:center; gap:6px; font-size:12px; color:var(--color-text-muted); margin-bottom:4px; }
    .trainer-contact mat-icon { font-size:14px; width:14px; height:14px; }
  `],
})
export class TrainersListComponent implements OnInit, OnDestroy {
  private readonly facade = inject(TrainersFacade);
  private readonly destroy$ = new Subject<void>();

  /** Localization helpers used by the card labels. */
  readonly i18n = inject(I18nService);
  /** Debounced search stream. */
  readonly search$ = new Subject<string>();
  /** Current page of trainers. */
  readonly trainers = this.facade.trainers;
  /** Total number of trainers. */
  readonly total = this.facade.total;
  /** Whether a load is in flight. */
  readonly loading = this.facade.loading;

  /** Current search box value. */
  search = '';
  /** Current page size. */
  pageSize = 9;

  /** Wires the debounced search and loads the first page. */
  ngOnInit(): void {
    this.search$
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => this.facade.updateQuery({ page: 1, search: this.search || undefined }));

    this.facade.load();
  }

  /** Releases the search subscription. */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Formats the experience label.
   *
   * @param value - Years of experience.
   * @returns Localized label.
   */
  yearsExp(value: number): string {
    return this.i18n.t('trainers.yearsExp', { years: this.i18n.formatNumber(value) });
  }

  /**
   * Builds the accessible rating label.
   *
   * @param rating - Average rating.
   * @returns Localized label.
   */
  ratingLabel(rating: number): string {
    return this.i18n.t('trainers.ratingLabel', {
      rating: this.i18n.formatNumber(rating, { maximumFractionDigits: 1 }),
    });
  }

  /**
   * Applies a paginator event.
   *
   * @param event - Material page event.
   */
  onPage(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.facade.updateQuery({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }

  /**
   * Converts a rating into five star states.
   *
   * @param rating - Average rating.
   * @returns One entry per star: 1 full, 0.5 half, 0 empty.
   */
  stars(rating: number): number[] {
    return Array.from({ length: 5 }, (_, i) => {
      if (i + 1 <= rating) return 1;
      if (i + 0.5 <= rating) return 0.5;
      return 0;
    });
  }
}
