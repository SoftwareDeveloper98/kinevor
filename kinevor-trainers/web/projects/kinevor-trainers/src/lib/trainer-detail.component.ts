import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { I18nService, TranslatePipe } from '@softwaredeveloper98/kinevor-platform-web';
import { TrainersFacade } from './trainers.facade';

/**
 * Staff trainer profile page: hero, stats, bio, specializations and
 * certifications, driven by {@link TrainersFacade}. Redirects back to the
 * directory when the trainer is missing.
 */
@Component({
  selector: 'kinevor-trainer-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    TranslatePipe,
  ],
  template: `
    @if (loading()) {
      <div class="table-loading" role="status">
        <mat-spinner diameter="56" />
        <span class="sr-only">{{ 'common.loading' | translate }}</span>
      </div>
    }

    @if (!loading() && trainer(); as t) {
      <div class="page-container">
        <div class="trainer-hero">
          <button
            mat-icon-button
            routerLink="/trainers"
            class="flip-rtl"
            [attr.aria-label]="'common.back' | translate"
          >
            <mat-icon aria-hidden="true">arrow_back</mat-icon>
          </button>

          <div class="hero-avatar" aria-hidden="true">{{ t.fullName.charAt(0) }}</div>

          <div class="hero-info">
            <h1>{{ t.fullName }}</h1>
            <div class="hero-meta">
              <span class="status-chip" [class]="'status-' + t.status.toLowerCase()">
                {{ ('status.' + t.status.toLowerCase()) | translate }}
              </span>
              <span class="exp-badge">
                <mat-icon aria-hidden="true">work</mat-icon>
                {{ yearsExp(t.yearsExperience) }}
              </span>
            </div>
            <div class="rating-row" [attr.aria-label]="ratingLabel(t.rating)">
              @for (i of [1, 2, 3, 4, 5]; track i) {
                <mat-icon class="star" [class.filled]="i <= t.rating" aria-hidden="true">star</mat-icon>
              }
              <span class="rating-val">{{ i18n.formatNumber(t.rating, { minimumFractionDigits: 1, maximumFractionDigits: 1 }) }}</span>
              <span class="rating-count">({{ reviews(t.totalRatings) }})</span>
            </div>
          </div>
        </div>

        <div class="stats-row">
          <div class="stat-card">
            <mat-icon style="color:var(--color-accent-text)" aria-hidden="true">sports</mat-icon>
            <div class="sv">{{ i18n.formatNumber(t.specializations.length) }}</div>
            <div class="sl">{{ 'trainers.statsSpecializations' | translate }}</div>
          </div>
          <div class="stat-card">
            <mat-icon style="color:var(--color-status-paid)" aria-hidden="true">verified</mat-icon>
            <div class="sv">{{ i18n.formatNumber(t.certifications?.length ?? 0) }}</div>
            <div class="sl">{{ 'trainers.statsCertifications' | translate }}</div>
          </div>
          <div class="stat-card">
            <mat-icon style="color:var(--color-status-rating)" aria-hidden="true">star</mat-icon>
            <div class="sv">{{ i18n.formatNumber(t.rating, { minimumFractionDigits: 1, maximumFractionDigits: 1 }) }}</div>
            <div class="sl">{{ 'trainers.statsAvgRating' | translate }}</div>
          </div>
        </div>

        <div class="content-grid">
          <mat-card class="gym-card">
            <mat-card-header>
              <mat-card-title>{{ 'trainers.about' | translate }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p class="bio">{{ t.bio }}</p>

              <div class="contact-row">
                <mat-icon aria-hidden="true">email</mat-icon>
                <span>{{ t.email }}</span>
              </div>
              <div class="contact-row">
                <mat-icon aria-hidden="true">phone</mat-icon>
                <span>{{ t.phone }}</span>
              </div>

              <h3 class="section-subtitle">{{ 'trainers.specializations' | translate }}</h3>
              <div class="chips-row">
                @for (s of t.specializations; track s) {
                  <span class="spec-chip">{{ s }}</span>
                }
              </div>

              <h3 class="section-subtitle">{{ 'trainers.certifications' | translate }}</h3>
              <div class="certs-list">
                @for (c of t.certifications ?? []; track c) {
                  <div class="cert-item">
                    <mat-icon aria-hidden="true">verified</mat-icon>
                    <span>{{ c }}</span>
                  </div>
                }
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="gym-card">
            <mat-card-header>
              <mat-card-title>{{ 'trainers.statsAvgRating' | translate }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="score-block">
                <div class="score">{{ i18n.formatNumber(t.rating, { minimumFractionDigits: 1, maximumFractionDigits: 1 }) }}</div>
                <div class="score-stars" [attr.aria-label]="ratingLabel(t.rating)">
                  @for (i of [1, 2, 3, 4, 5]; track i) {
                    <mat-icon class="star" [class.filled]="i <= t.rating" aria-hidden="true">star</mat-icon>
                  }
                </div>
                <p class="reviews">{{ reviews(t.totalRatings) }}</p>
              </div>

              <div class="fact-row">
                <span>{{ 'common.status' | translate }}</span>
                <strong>{{ ('status.' + t.status.toLowerCase()) | translate }}</strong>
              </div>
              <div class="fact-row">
                <span>{{ 'trainers.statsCertifications' | translate }}</span>
                <strong>{{ i18n.formatNumber(t.certifications?.length ?? 0) }}</strong>
              </div>
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    }
  `,
  styles: [`
    .trainer-hero { display:flex; align-items:center; gap:20px; background:var(--color-surface-raised); border-radius:16px; padding:24px; margin-bottom:20px; border:1px solid var(--color-border); flex-wrap:wrap; }
    .hero-avatar { width:80px; height:80px; border-radius:50%; background:var(--gradient-brandWarm); color:var(--color-on-accent); font-size:32px; font-weight:900; display:flex; align-items:center; justify-content:center; flex-shrink:0; box-shadow:0 0 30px rgba(191,54,12,.3); }
    .hero-info { flex:1; }
    .hero-info h1 { font-size:26px; font-weight:800; color:var(--color-text); margin:0 0 8px; }
    .hero-meta { display:flex; gap:10px; align-items:center; margin-bottom:8px; flex-wrap:wrap; }
    .exp-badge { display:flex; align-items:center; gap:4px; font-size:13px; color:var(--color-text-muted); }
    .exp-badge mat-icon { font-size:16px; width:16px; height:16px; }
    .rating-row { display:flex; align-items:center; gap:3px; }
    .star { font-size:18px; width:18px; height:18px; color:var(--color-text-muted); }
    .star.filled { color:var(--color-status-rating); }
    .rating-val { margin-inline-start:6px; font-weight:700; color:var(--color-text); }
    .rating-count { font-size:12px; color:var(--color-text-muted); margin-inline-start:4px; }
    .stats-row { display:grid; grid-template-columns:repeat(auto-fill,minmax(160px,1fr)); gap:12px; margin-bottom:20px; }
    .stat-card { background:var(--color-surface-raised); border:1px solid var(--color-border); border-radius:14px; padding:16px; display:flex; flex-direction:column; align-items:center; gap:6px; }
    .sv { font-size:22px; font-weight:800; color:var(--color-text); }
    .sl { font-size:11px; color:var(--color-text-muted); text-align:center; }
    .content-grid { display:grid; grid-template-columns:1fr 360px; gap:16px; }
    @media(max-width:900px) { .content-grid { grid-template-columns:1fr; } }
    .bio { font-size:14px; color:var(--color-text-muted); line-height:1.7; margin-bottom:16px; }
    .contact-row { display:flex; align-items:center; gap:10px; font-size:13px; color:var(--color-text-muted); margin-bottom:8px; }
    .contact-row mat-icon { font-size:16px; width:16px; height:16px; color:var(--color-text-muted); }
    .section-subtitle { font-size:12px; font-weight:700; color:var(--color-accent-text); text-transform:uppercase; letter-spacing:1px; margin:16px 0 10px; }
    .chips-row { display:flex; flex-wrap:wrap; gap:8px; }
    .spec-chip { background:rgba(230,81,0,.15); color:var(--color-accent-text); font-size:12px; font-weight:600; padding:4px 12px; border-radius:20px; }
    .certs-list { display:flex; flex-direction:column; gap:8px; }
    .cert-item { display:flex; align-items:center; gap:8px; font-size:13px; color:var(--color-text-muted); }
    .cert-item mat-icon { font-size:16px; width:16px; height:16px; color:var(--color-status-paid); }
    .score-block { text-align:center; padding:12px 0 20px; border-bottom:1px solid var(--color-border); margin-bottom:16px; }
    .score { font-size:48px; font-weight:900; color:var(--color-text); line-height:1; }
    .score-stars { display:flex; justify-content:center; margin:8px 0 4px; }
    .score-stars .star { font-size:22px; width:22px; height:22px; }
    .reviews { font-size:13px; color:var(--color-text-muted); }
    .fact-row { display:flex; justify-content:space-between; gap:12px; padding:9px 0; font-size:13px; color:var(--color-text-muted); border-bottom:1px solid var(--color-border); }
    .fact-row:last-child { border-bottom:0; }
    .fact-row strong { color:var(--color-text); font-weight:600; }
  `],
})
export class TrainerDetailComponent implements OnInit {
  private readonly facade = inject(TrainersFacade);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  /** Localization helpers used by the template. */
  readonly i18n = inject(I18nService);
  /** Whether the profile is loading. */
  readonly loading = this.facade.detailLoading;
  /** Loaded trainer profile. */
  readonly trainer = this.facade.detail;

  /** Loads the trainer selected by the route, returning to the list when missing. */
  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      void this.router.navigate(['/trainers']);
      return;
    }

    this.facade
      .loadDetail(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ error: () => void this.router.navigate(['/trainers']) });
  }

  /**
   * Formats the short experience label.
   *
   * @param value - Years of experience.
   * @returns Localized label.
   */
  yearsExp(value: number): string {
    return this.i18n.t('trainers.yearsExpShort', { years: this.i18n.formatNumber(value) });
  }

  /**
   * Formats the review count.
   *
   * @param count - Number of ratings.
   * @returns Localized label.
   */
  reviews(count: number): string {
    return this.i18n.t('trainers.reviews', { count: this.i18n.formatNumber(count) });
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
}
