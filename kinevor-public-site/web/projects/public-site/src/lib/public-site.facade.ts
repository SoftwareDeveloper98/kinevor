import { Injectable, computed, inject, signal } from '@angular/core';
import {
  BlogPostSummary,
  Branch,
  ClassSession,
  Faq,
  MembershipPlan,
  PublicSite,
  PUBLIC_SITE_GATEWAY,
  Trainer,
} from './public-site.models';

@Injectable({ providedIn: 'root' })
export class PublicSiteFacade {
  private readonly gateway = inject(PUBLIC_SITE_GATEWAY);
  private readonly inFlight = signal<Promise<void> | null>(null);
  private loaded = false;

  readonly site = signal<PublicSite | null>(null);
  readonly plans = signal<MembershipPlan[]>([]);
  readonly trainers = signal<Trainer[]>([]);
  readonly schedule = signal<ClassSession[]>([]);
  readonly faqs = signal<Faq[]>([]);
  readonly posts = signal<BlogPostSummary[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly primaryBranch = computed(() => this.site()?.branches[0] ?? null);
  readonly featuredPlans = computed(() => this.plans().slice(0, 3));
  readonly nextClasses = computed(() => this.schedule().slice(0, 3));
  readonly featuredTrainers = computed(() => this.trainers().slice(0, 4));
  readonly faqCategories = computed(() =>
    [...new Set(this.faqs().map((faq) => faq.category))],
  );
  readonly branches = computed<Branch[]>(() => this.site()?.branches ?? []);

  async load(force = false): Promise<void> {
    if (this.loaded && !force) return;

    const current = this.inFlight();
    if (current) return current;

    const request = this.fetchAll(force);
    this.inFlight.set(request);
    try {
      await request;
    } finally {
      this.inFlight.set(null);
    }
  }

  async refreshSchedule(): Promise<void> {
    try {
      const schedule = await this.gateway.schedule();
      this.schedule.set(schedule);
    } catch {
      this.error.set('The timetable is taking a moment. Please try again.');
    }
  }

  clearError(): void {
    this.error.set(null);
  }

  private async fetchAll(force: boolean): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const [site, plans, trainers, schedule, faqs, posts] = await Promise.all([
        this.gateway.site(),
        this.gateway.plans(),
        this.gateway.trainers(),
        this.gateway.schedule(),
        this.gateway.faqs(),
        this.gateway.posts(),
      ]);

      this.site.set(site);
      this.plans.set(plans);
      this.trainers.set(trainers);
      this.schedule.set(schedule);
      this.faqs.set(faqs);
      this.posts.set(posts);
      this.loaded = true;
    } catch {
      if (force) {
        this.loaded = false;
      }
      this.error.set('We could not load the public site. Please try again.');
    } finally {
      this.loading.set(false);
    }
  }
}
