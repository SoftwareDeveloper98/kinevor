import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import {
  DashboardAlerts,
  DashboardBranch,
  DashboardQuery,
  STAFF_DASHBOARD_GATEWAY,
  StaffDashboard,
  StaffDashboardGateway,
} from './staff-dashboard.model';
import { StaffDashboardFacade } from './staff-dashboard.facade';

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
      date: '2026-10-14T00:00:00Z',
      label: 'Oct 14',
      checkIns: 142,
      bookings: 82,
      revenue: 480,
    },
    {
      date: '2026-10-15T00:00:00Z',
      label: 'Oct 15',
      checkIns: 128,
      bookings: 90,
      revenue: 510,
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
    {
      id: 'alert-2',
      branchId: branch.id,
      branchName: branch.name,
      branchCity: branch.city,
      type: 'ExpiringMembership',
      severity: 'Warning',
      title: '3 memberships expire this week',
      detail: 'Maya Thompson, Liam Chen and 1 other',
      dueAt: '2026-10-22T00:00:00Z',
      reference: 'Memberships - Main',
    },
  ],
  branches: [branch],
};

const inbox: DashboardAlerts = {
  generatedAt: '2026-10-20T10:00:00Z',
  branchId: null,
  branchName: 'All branches',
  branchCity: '3 locations',
  alerts: [
    ...dashboard.alerts,
    {
      id: 'alert-3',
      branchId: branch.id,
      branchName: branch.name,
      branchCity: branch.city,
      type: 'LowClassFill',
      severity: 'Info',
      title: 'Reformer Pilates is 40% full',
      detail: '8 of 20 seats booked for tonight',
      dueAt: null,
      reference: 'CLS-2418',
    },
  ],
  branches: [branch],
};

function gatewayMock(): StaffDashboardGateway {
  return {
    get: vi.fn(async () => dashboard),
    alerts: vi.fn(async () => inbox),
    branches: vi.fn(async () => [branch]),
  };
}

function setup() {
  const gateway = gatewayMock();
  TestBed.configureTestingModule({
    providers: [
      StaffDashboardFacade,
      { provide: STAFF_DASHBOARD_GATEWAY, useValue: gateway },
    ],
  });

  return { gateway, facade: TestBed.inject(StaffDashboardFacade) };
}

describe('StaffDashboardFacade', () => {
  it('loads the dashboard and computes operational signals', async () => {
    const { facade, gateway } = setup();

    await facade.load();

    expect(gateway.get).toHaveBeenCalledWith({ branchId: null, days: 7 });
    expect(facade.dashboard()).toEqual(dashboard);
    expect(facade.summary()?.activeMembers).toBe(429);
    expect(facade.trend()).toHaveLength(2);
    expect(facade.activity()).toHaveLength(1);
    expect(facade.branches()).toHaveLength(1);
    expect(facade.branchName()).toBe('All branches');
    expect(facade.totalCheckIns()).toBe(270);
    expect(facade.totalBookings()).toBe(172);
    expect(facade.maxTrend()).toBe(142);
    expect(facade.criticalCount()).toBe(1);
    expect(facade.warningCount()).toBe(1);
    expect(facade.loading()).toBe(false);
  });

  it('filters by branch and reloads the current view', async () => {
    const { facade, gateway } = setup();
    await facade.load();
    gateway.get = vi.fn(async (query: DashboardQuery) => ({
      ...dashboard,
      branchId: query.branchId ?? null,
      branchName: 'Kinevor Gym - Main',
    }));

    await facade.setBranch(branch.id);

    expect(facade.branchId()).toBe(branch.id);
    expect(gateway.get).toHaveBeenLastCalledWith({ branchId: branch.id, days: 7 });
    expect(facade.branchName()).toBe('Kinevor Gym - Main');
  });

  it('switches to the 30-day range', async () => {
    const { facade, gateway } = setup();

    await facade.setRange(30);

    expect(facade.days()).toBe(30);
    expect(gateway.get).toHaveBeenCalledWith({ branchId: null, days: 30 });
  });

  it('loads the full alert inbox and counts every severity', async () => {
    const { facade, gateway } = setup();

    await facade.loadAlerts();

    expect(gateway.alerts).toHaveBeenCalledWith(null);
    expect(facade.alerts()).toHaveLength(3);
    expect(facade.criticalCount()).toBe(1);
    expect(facade.warningCount()).toBe(1);
    expect(facade.infoCount()).toBe(1);
    expect(facade.alertsLoading()).toBe(false);
  });

  it('reloads alerts after a branch change when the inbox is open', async () => {
    const { facade, gateway } = setup();
    await facade.loadAlerts();

    await facade.setBranch(branch.id);

    expect(gateway.get).toHaveBeenCalledWith({ branchId: branch.id, days: 7 });
    expect(gateway.alerts).toHaveBeenLastCalledWith(branch.id);
  });

  it('surfaces API failures as an error message', async () => {
    const { facade, gateway } = setup();
    gateway.get = vi.fn(async () => {
      throw new Error('offline');
    });

    await facade.load();

    expect(facade.error()).toBe('The request failed. Try again.');
    expect(facade.loading()).toBe(false);
  });
});
