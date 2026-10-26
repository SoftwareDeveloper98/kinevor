import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import {
  camden,
  closedVisit,
  eligibility,
  member,
  occupancy,
  openVisit,
  shoreditch,
  stats,
} from './check-in.fixtures';
import { CHECKINS_API_BASE_URL } from './check-in.model';
import { HttpCheckInsGateway } from './http-check-ins.gateway';

describe('HttpCheckInsGateway', () => {
  let gateway: HttpCheckInsGateway;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        HttpCheckInsGateway,
        { provide: CHECKINS_API_BASE_URL, useValue: 'http://api.test/api/v1/' },
      ],
    });
    gateway = TestBed.inject(HttpCheckInsGateway);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('searches access members with paging and unwraps the envelope', async () => {
    const promise = gateway.members({ search: 'maya', page: 2, pageSize: 10 });
    const request = http.expectOne('http://api.test/api/v1/checkins/members?page=2&pageSize=10&search=maya');

    expect(request.request.method).toBe('GET');
    request.flush({
      success: true,
      data: {
        items: [member],
        totalCount: 1,
        page: 2,
        pageSize: 10,
        totalPages: 1,
        hasNext: false,
        hasPrev: true,
      },
    });

    const page = await promise;
    expect(page.totalCount).toBe(1);
    expect(page.items[0].fullName).toBe('Maya Thompson');
  });

  it('evaluates eligibility against the chosen branch', async () => {
    const promise = gateway.eligibility(member.id, camden.id);
    const request = http.expectOne(
      (req) =>
        req.url === `http://api.test/api/v1/checkins/eligibility/${member.id}` &&
        req.params.get('branchId') === camden.id,
    );
    request.flush({ success: true, data: eligibility });

    expect((await promise).allowed).toBe(true);
  });

  it('maps visit-history filters into query parameters', async () => {
    const promise = gateway.history({
      search: 'maya',
      branchId: shoreditch.id,
      status: 'closed',
      from: '2026-10-01',
      to: '2026-10-31',
      page: 2,
      pageSize: 10,
    });
    const request = http.expectOne(
      (req) => req.url === 'http://api.test/api/v1/checkins',
    );

    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('search')).toBe('maya');
    expect(request.request.params.get('branchId')).toBe(shoreditch.id);
    expect(request.request.params.get('status')).toBe('closed');
    expect(request.request.params.get('from')).toBe('2026-10-01');
    expect(request.request.params.get('to')).toBe('2026-10-31');
    request.flush({
      success: true,
      data: {
        items: [closedVisit],
        totalCount: 8,
        page: 2,
        pageSize: 10,
        totalPages: 1,
        hasNext: false,
        hasPrev: true,
      },
    });

    expect((await promise).items[0].isOpen).toBe(false);
  });

  it('reads occupancy, daily stats and branches', async () => {
    const occupancyPromise = gateway.occupancy(shoreditch.id);
    const occupancyRequest = http.expectOne(
      (req) => req.url === 'http://api.test/api/v1/checkins/occupancy',
    );
    expect(occupancyRequest.request.params.get('branchId')).toBe(shoreditch.id);
    occupancyRequest.flush({ success: true, data: occupancy });
    expect((await occupancyPromise).occupied).toBe(12);

    const statsPromise = gateway.stats(shoreditch.id, '2026-10-21');
    const statsRequest = http.expectOne(
      (req) => req.url === 'http://api.test/api/v1/checkins/stats',
    );
    expect(statsRequest.request.params.get('date')).toBe('2026-10-21');
    statsRequest.flush({ success: true, data: stats });
    expect((await statsPromise).peakHour).toBe(7);

    const branchesPromise = gateway.branches();
    const branchesRequest = http.expectOne('http://api.test/api/v1/checkins/branches');
    branchesRequest.flush({ success: true, data: [shoreditch, camden] });
    expect((await branchesPromise).length).toBe(2);
  });

  it('posts front-desk check-ins and check-outs', async () => {
    const checkInPromise = gateway.checkIn({
      memberId: member.id,
      branchId: shoreditch.id,
      notes: 'Guest pass',
    });
    const checkInRequest = http.expectOne('http://api.test/api/v1/checkins');
    expect(checkInRequest.request.method).toBe('POST');
    expect(checkInRequest.request.body).toEqual({
      memberId: member.id,
      branchId: shoreditch.id,
      notes: 'Guest pass',
    });
    checkInRequest.flush({ success: true, data: openVisit });
    expect((await checkInPromise).isOpen).toBe(true);

    const checkOutPromise = gateway.checkOut(openVisit.id);
    const checkOutRequest = http.expectOne(
      `http://api.test/api/v1/checkins/${openVisit.id}/checkout`,
    );
    expect(checkOutRequest.request.method).toBe('POST');
    checkOutRequest.flush({ success: true, data: { ...openVisit, isOpen: false } });
    expect((await checkOutPromise).isOpen).toBe(false);
  });

  it('uses the member self-service endpoints', async () => {
    const statusPromise = gateway.myStatus();
    const statusRequest = http.expectOne('http://api.test/api/v1/me/checkins/status');
    statusRequest.flush({
      success: true,
      data: {
        memberId: member.id,
        memberNumber: member.memberNumber,
        fullName: member.fullName,
        memberStatus: member.status,
        membershipStatus: member.membershipStatus,
        eligibility,
        openCheckIn: null,
        occupancy,
        visitsThisMonth: 6,
        lastVisit: closedVisit,
      },
    });
    expect((await statusPromise).visitsThisMonth).toBe(6);

    const visitsPromise = gateway.myVisits(2, 5);
    const visitsRequest = http.expectOne(
      (req) => req.url === 'http://api.test/api/v1/me/checkins',
    );
    expect(visitsRequest.request.params.get('page')).toBe('2');
    expect(visitsRequest.request.params.get('pageSize')).toBe('5');
    visitsRequest.flush({
      success: true,
      data: {
        items: [closedVisit],
        totalCount: 6,
        page: 2,
        pageSize: 5,
        totalPages: 2,
        hasNext: false,
        hasPrev: true,
      },
    });
    expect((await visitsPromise).totalCount).toBe(6);

    const selfCheckInPromise = gateway.selfCheckIn('Morning session');
    const selfCheckInRequest = http.expectOne('http://api.test/api/v1/me/checkins');
    expect(selfCheckInRequest.request.method).toBe('POST');
    expect(selfCheckInRequest.request.body).toEqual({ notes: 'Morning session' });
    selfCheckInRequest.flush({ success: true, data: openVisit });
    expect((await selfCheckInPromise).memberId).toBe(member.id);

    const selfCheckOutPromise = gateway.selfCheckOut();
    const selfCheckOutRequest = http.expectOne(
      'http://api.test/api/v1/me/checkins/checkout',
    );
    expect(selfCheckOutRequest.request.method).toBe('POST');
    selfCheckOutRequest.flush({ success: true, data: { ...openVisit, isOpen: false } });
    expect((await selfCheckOutPromise).isOpen).toBe(false);
  });
});
