import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { HttpPublicSiteGateway } from './http-public-site.gateway';
import {
  ApiEnvelope,
  PUBLIC_SITE_API_BASE_URL,
  PublicSite,
  TrialCreated,
  TrialRequest,
} from './public-site.models';

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
  branches: [
    {
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
    },
  ],
};

describe('HttpPublicSiteGateway', () => {
  let gateway: HttpPublicSiteGateway;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        HttpPublicSiteGateway,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: PUBLIC_SITE_API_BASE_URL, useValue: 'http://localhost:5090/api/v1/' },
      ],
    });

    gateway = TestBed.inject(HttpPublicSiteGateway);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('unwraps the site envelope and normalizes the base URL', async () => {
    const result = gateway.site();

    const request = http.expectOne('http://localhost:5090/api/v1/public/site');
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, data: site } satisfies ApiEnvelope<PublicSite>);

    await expect(result).resolves.toEqual(site);
  });

  it('sends schedule filters as query parameters', async () => {
    const result = gateway.schedule({
      from: '2026-10-20',
      to: '2026-11-03',
      branchId: 'branch-1',
    });

    const request = http.expectOne(
      (candidate) =>
        candidate.url === 'http://localhost:5090/api/v1/public/schedule'
        && candidate.params.get('from') === '2026-10-20'
        && candidate.params.get('to') === '2026-11-03'
        && candidate.params.get('branchId') === 'branch-1',
    );
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, data: [] } satisfies ApiEnvelope<never[]>);

    await expect(result).resolves.toEqual([]);
  });

  it('posts a trial request and returns the created identifier', async () => {
    const payload: TrialRequest = {
      firstName: 'Jordan',
      lastName: 'Ellis',
      email: 'jordan@example.com',
      phone: '+1 212 555 0188',
      preferredDate: null,
      classSessionId: null,
      branchId: null,
      goal: 'Build strength',
      notes: null,
      consentToContact: true,
    };
    const created: TrialCreated = { id: 'trial-1', message: 'Received.' };
    const result = gateway.createTrial(payload);

    const request = http.expectOne('http://localhost:5090/api/v1/public/trials');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush({ success: true, data: created } satisfies ApiEnvelope<TrialCreated>);

    await expect(result).resolves.toEqual(created);
  });

  it('rejects a failed API envelope with its message', async () => {
    const result = gateway.faqs();

    const request = http.expectOne('http://localhost:5090/api/v1/public/faqs');
    request.flush({
      success: false,
      data: null,
      message: 'Content is unavailable.',
    } satisfies ApiEnvelope<never[]>);

    await expect(result).rejects.toThrow('Content is unavailable.');
  });
});
