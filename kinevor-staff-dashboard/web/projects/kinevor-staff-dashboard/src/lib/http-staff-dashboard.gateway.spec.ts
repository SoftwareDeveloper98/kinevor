import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { HttpStaffDashboardGateway } from './http-staff-dashboard.gateway';
import {
  DashboardAlerts,
  DashboardBranch,
  STAFF_DASHBOARD_API_BASE_URL,
  StaffDashboard,
} from './staff-dashboard.model';

const branch: DashboardBranch = {
  id: 'a1111111-1111-4111-8111-111111111111',
  name: 'Kinevor Gym - Main',
  city: 'Manchester',
  capacity: 250,
  activeMembers: 184,
  occupancyNow: 63,
  occupancyPercent: 25.2,
};

const dashboard: StaffDashboard = {
  generatedAt: '2026-10-20T10:00:00Z',
  branchId: null,
  branchName: 'All branches',
  branchCity: '3 locations',
  days: 7,
  summary: {
    totalMembers: 521,
    activeMembers: 429,
    occupancyNow: 213,
    occupancyPercent: 36.7,
    capacity: 580,
    checkInsToday: 128,
    bookingsToday: 84,
    revenueMonth: 33570,
    outstandingInvoices: 15,
    outstandingAmount: 2520,
    comparison: { activeMembersPercent: 6.2, revenuePercent: 5.4 },
  },
  trend: [
    {
      date: '2026-10-20T00:00:00Z',
      label: 'Oct 20',
      checkIns: 128,
      bookings: 84,
      revenue: 503.33,
    },
  ],
  activity: [
    {
      id: 'activity-1',
      branchId: branch.id,
      branchName: branch.name,
      branchCity: branch.city,
      kind: 'CheckIn',
      title: 'Maya Thompson checked in',
      detail: 'Strength floor - 08:42',
      occurredAt: '2026-10-20T08:42:00Z',
    },
  ],
  alerts: [
    {
      id: 'alert-1',
      branchId: branch.id,
      branchName: branch.name,
      branchCity: branch.city,
      type: 'OverdueInvoice',
      severity: 'Critical',
      title: '7 overdue invoices need follow-up',
      detail: '$1,240 outstanding across 7 members',
      dueAt: '2026-10-19T00:00:00Z',
      reference: 'Billing - Main',
    },
  ],
  branches: [branch],
};

const inbox: DashboardAlerts = {
  generatedAt: '2026-10-20T10:00:00Z',
  branchId: branch.id,
  branchName: branch.name,
  branchCity: branch.city,
  alerts: dashboard.alerts,
  branches: [branch],
};

describe('HttpStaffDashboardGateway', () => {
  let gateway: HttpStaffDashboardGateway;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        HttpStaffDashboardGateway,
        { provide: STAFF_DASHBOARD_API_BASE_URL, useValue: 'http://api.test/api/v1/' },
      ],
    });
    gateway = TestBed.inject(HttpStaffDashboardGateway);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('unwraps the dashboard for all branches with a range', async () => {
    const promise = gateway.get({ days: 7 });

    const request = http.expectOne((req) => req.url === 'http://api.test/api/v1/staff-dashboard');
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('days')).toBe('7');
    expect(request.request.params.has('branchId')).toBe(false);
    request.flush({ success: true, data: dashboard });

    const result = await promise;
    expect(result.branchName).toBe('All branches');
    expect(result.summary.activeMembers).toBe(429);
    expect(result.branches).toHaveLength(1);
  });

  it('sends branch and range filters', async () => {
    const promise = gateway.get({ branchId: branch.id, days: 30 });

    const request = http.expectOne((req) => req.url === 'http://api.test/api/v1/staff-dashboard');
    expect(request.request.params.get('branchId')).toBe(branch.id);
    expect(request.request.params.get('days')).toBe('30');
    request.flush({ success: true, data: { ...dashboard, days: 30 } });

    expect((await promise).days).toBe(30);
  });

  it('loads the complete alert inbox for a branch', async () => {
    const promise = gateway.alerts(branch.id);

    const request = http.expectOne(
      (req) => req.url === 'http://api.test/api/v1/staff-dashboard/alerts',
    );
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('branchId')).toBe(branch.id);
    request.flush({ success: true, data: inbox });

    const result = await promise;
    expect(result.branchName).toBe('Kinevor Gym - Main');
    expect(result.alerts[0].severity).toBe('Critical');
  });

  it('loads the branch selector options', async () => {
    const promise = gateway.branches();

    const request = http.expectOne('http://api.test/api/v1/staff-dashboard/branches');
    request.flush({ success: true, data: [branch] });

    const result = await promise;
    expect(result).toHaveLength(1);
    expect(result[0].capacity).toBe(250);
  });
});
