import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { I18nService, TranslatePipe } from '@softwaredeveloper98/kinevor-platform-web';
import { TrainerSummary } from './trainer.model';
import { TRAINERS_GATEWAY } from './trainers.gateway';

/**
 * Public trainer directory with debounced search and paging. Uses
 * {@link TRAINERS_GATEWAY} directly so it does not share staff page state.
 */
@Component({
  selector: 'kinevor-public-trainers',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    SlicePipe,
    MatCardModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    TranslatePipe,
  ],
  template: `
    <div class="pub-page">
      <h1 class="pub-page-title">{{ 'trainers.title' | translate }}</h1>
      <p class="pub-page-sub">{{ 'public.trainers.subtitle' | translate }}</p>

      <mat-form-field appearance="outline" class="search-field">
        <mat-label>{{ 'trainers.search' | translate }}</mat-label>
        <input matInput [(ngModel)]="search" (ngModelChange)="search$.next($event)" />
        <mat-icon matSuffix aria-hidden="true">search</mat-icon>
      </mat-form-field>

      @if (loading()) {
        <div class="table-loading" role="status">
          <mat-spinner diameter="48" />
          <span class="sr-only">{{ 'common.loading' | translate }}</span>
        </div>
      } @else {
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
                <p class="trainer-bio">{{ t.bio | slice: 0 : 110 }}{{ t.bio.length > 110 ? '…' : '' }}</p>
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
    .pub-page { max-width:1200px; margin:0 auto; padding:48px 24px; }
    .pub-page-title { font-size:32px; font-weight:900; color:var(--color-text); margin:0 0 8px; }
    .pub-page-sub { font-size:15px; color:var(--color-text-muted); margin:0 0 24px; }
    .search-field { max-width:400px; width:100%; margin-bottom:20px; }
    .trainer-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(300px,1fr)); gap:16px; }
    .trainer-card mat-card-content { padding:24px !important; }
    .trainer-header { display:flex; align-items:center; justify-content:space-between; margin-bottom:12px; }
    .trainer-avatar { width:60px; height:60px; border-radius:50%; background:var(--gradient-brandWarm); color:var(--color-on-accent); font-size:24px; font-weight:800; display:flex; align-items:center; justify-content:center; }
    .trainer-stars { display:flex; }
    .star { font-size:18px; width:18px; height:18px; color:var(--color-status-rating); }
    .trainer-name { font-size:20px; font-weight:700; color:var(--color-text); }
    .trainer-exp { font-size:12px; color:var(--color-text-muted); margin-bottom:10px; }
    .trainer-specs { display:flex; flex-wrap:wrap; gap:6px; margin-bottom:12px; }
    .spec-tag { background:rgba(230,81,0,.15); color:var(--color-accent-text); font-size:11px; font-weight:600; padding:3px 10px; border-radius:10px; }
    .trainer-bio { font-size:13px; color:var(--color-text-muted); line-height:1.5; margin:0; }
  `],
})
export class PublicTrainersComponent implements OnInit, OnDestroy {
  private readonly gateway = inject(TRAINERS_GATEWAY);
  private readonly destroy$ = new Subject<void>();

  /** Localization helpers used by the card labels. */
  readonly i18n = inject(I18nService);
  /** Debounced search stream. */
  readonly search$ = new Subject<string>();
  /** Current page of trainers. */
  readonly trainers = signal<TrainerSummary[]>([]);
  /** Total number of trainers. */
  readonly total = signal(0);
  /** Whether a load is in flight. */
  readonly loading = signal(true);

  /** Current search box value. */
  search = '';
  /** Current page number. */
  page = 1;
  /** Current page size. */
  pageSize = 9;

  /** Wires the debounced search and loads the first page. */
  ngOnInit(): void {
    this.search$
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.page = 1;
        this.load();
      });

    this.load();
  }

  /** Releases the search subscription. */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** Loads the current page. */
  load(): void {
    this.loading.set(true);
    this.gateway
      .list({ page: this.page, pageSize: this.pageSize, search: this.search || undefined })
      .subscribe({
        next: page => {
          this.trainers.set(page.items);
          this.total.set(page.totalCount);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
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
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.load();
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
