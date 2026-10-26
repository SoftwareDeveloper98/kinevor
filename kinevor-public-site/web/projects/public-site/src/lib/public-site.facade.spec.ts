import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { PublicSiteFacade } from './public-site.facade';
import {
  BlogPost,
  BlogPostSummary,
  Branch,
  ClassSession,
  Faq,
  Lead,
  LeadCreated,
  LeadRequest,
  MembershipPlan,
  PagedResult,
  PUBLIC_SITE_GATEWAY,
  PublicSite,
  PublicSiteGateway,
  Trainer,
  TrialCreated,
  TrialRequest,
} from './public-site.models';

const branch: Branch = {
  id: 'branch-1',
  name: 'Kinevor Main',
  slug: 'main',
  neighbourhood: 'Tribeca',
  address: '18 Franklin Street',
  city: 'New York',
  phone: '+1 212 555 0101',
  openingHours: 'Mon-Fri 05:30-22:00',
  parking: 'Street parking',
  amenities: ['Showers'],
  capacity: 90,
  latitude: 40.7,
  longitude: -74,
  theme: 'ember',
};

const site: PublicSite = {
  name: 'Kinevor',
  tagline: 'Built to move.',
  description: 'A coach-led gym.',
  story: 'A story.',
  eyebrow: 'New York',
  phone: '+1 212 555 0123',
  email: 'hello@kinevorgym.com',
  address: '18 Franklin Street',
  city: 'New York',
  instagramHandle: '@kinevorgym',
  memberCount: 1200,
  trainerCount: 4,
  weeklyClasses: 24,
  rating: 4.9,
  foundedYear: 2016,
  highlights: ['Coaching'],
  branches: [branch],
};

class FakeGateway implements PublicSiteGateway {
  readonly site = vi.fn(async () => site);
  readonly branches = vi.fn(async () => [branch]);
  readonly branch = vi.fn(async () => branch);
  readonly plans = vi.fn(async (): Promise<MembershipPlan[]> => []);
  readonly trainers = vi.fn(async (): Promise<Trainer[]> => []);
  readonly schedule = vi.fn(async (): Promise<ClassSession[]> => []);
  readonly faqs = vi.fn(async (): Promise<Faq[]> => []);
  readonly posts = vi.fn(async (): Promise<BlogPostSummary[]> => []);
  readonly post = vi.fn(async (): Promise<BlogPost> => {
    throw new Error('Not implemented.');
  });
  readonly createTrial = vi.fn(async (_request: TrialRequest): Promise<TrialCreated> => ({
    id: 'trial-1',
    message: 'Received.',
  }));
  readonly createLead = vi.fn(async (_request: LeadRequest): Promise<LeadCreated> => ({
    id: 'lead-1',
    message: 'Received.',
  }));
  readonly leads = vi.fn(async (): Promise<PagedResult<Lead>> => ({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0,
    hasNext: false,
    hasPrev: false,
  }));
}

describe('PublicSiteFacade', () => {
  let facade: PublicSiteFacade;
  let gateway: FakeGateway;

  beforeEach(() => {
    gateway = new FakeGateway();
    TestBed.configureTestingModule({
      providers: [{ provide: PUBLIC_SITE_GATEWAY, useValue: gateway }],
    });
    facade = TestBed.inject(PublicSiteFacade);
  });

  it('loads all public content once and exposes derived signals', async () => {
    await facade.load();
    await facade.load();

    expect(gateway.site).toHaveBeenCalledTimes(1);
    expect(gateway.plans).toHaveBeenCalledTimes(1);
    expect(gateway.trainers).toHaveBeenCalledTimes(1);
    expect(gateway.schedule).toHaveBeenCalledTimes(1);
    expect(gateway.faqs).toHaveBeenCalledTimes(1);
    expect(gateway.posts).toHaveBeenCalledTimes(1);
    expect(facade.site()).toEqual(site);
    expect(facade.primaryBranch()).toEqual(branch);
    expect(facade.branches()).toEqual([branch]);
    expect(facade.loading()).toBe(false);
    expect(facade.error()).toBeNull();
  });

  it('reloads every collection when forced', async () => {
    await facade.load();
    await facade.load(true);

    expect(gateway.site).toHaveBeenCalledTimes(2);
    expect(gateway.plans).toHaveBeenCalledTimes(2);
    expect(gateway.trainers).toHaveBeenCalledTimes(2);
    expect(gateway.schedule).toHaveBeenCalledTimes(2);
    expect(gateway.faqs).toHaveBeenCalledTimes(2);
    expect(gateway.posts).toHaveBeenCalledTimes(2);
  });

  it('shares an in-flight load between concurrent callers', async () => {
    await Promise.all([facade.load(), facade.load(), facade.load()]);

    expect(gateway.site).toHaveBeenCalledTimes(1);
    expect(gateway.schedule).toHaveBeenCalledTimes(1);
  });

  it('exposes a recoverable error when the content request fails', async () => {
    gateway.site.mockRejectedValueOnce(new Error('Network unavailable.'));

    await facade.load();

    expect(facade.error()).toBe('We could not load the public site. Please try again.');
    expect(facade.loading()).toBe(false);
    expect(facade.site()).toBeNull();
  });
});
