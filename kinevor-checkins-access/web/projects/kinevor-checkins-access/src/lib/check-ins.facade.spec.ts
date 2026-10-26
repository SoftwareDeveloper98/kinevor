import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
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
import { CheckInsFacade } from './check-ins.facade';
import {
  CHECKINS_GATEWAY,
  CheckInHistoryQuery,
  CheckInVisit,
  CheckInsGateway,
  MemberCheckInStatus,
  MemberListQuery,
} from './check-in.model';
import { OccupancyFacade } from './occupancy.facade';
import { VisitHistoryFacade } from './visit-history.facade';

function gatewayMock() {
  const historyPage = {
    items: [openVisit],
    totalCount: 23,
    page: 1,
    pageSize: 10,
    totalPages: 3,
    hasNext: true,
    hasPrev: false,
  };
  const myStatus: MemberCheckInStatus = {
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
  };
  const mocks = {
    members: vi.fn(async (_query: MemberListQuery) => ({
      items: [member],
      totalCount: 19,
      page: 1,
      pageSize: 6,
      totalPages: 4,
      hasNext: true,
      hasPrev: false,
    })),
    eligibility: vi.fn(async (_memberId: string, _branchId?: string | null) => eligibility),
    history: vi.fn(async (_query: CheckInHistoryQuery) => historyPage),
    stats: vi.fn(async (_branchId?: string | null, _date?: string | null) => stats),
    occupancy: vi.fn(async (_branchId?: string | null) => occupancy),
    branches: vi.fn(async () => [camden, shoreditch]),
    audit: vi.fn(async () => ({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 25,
      totalPages: 1,
      hasNext: false,
      hasPrev: false,
    })),
    checkIn: vi.fn(async (_request: { memberId: string }) => openVisit),
    checkOut: vi.fn(async (_checkInId: string): Promise<CheckInVisit> => closedVisit),
    myStatus: vi.fn(async () => myStatus),
    myVisits: vi.fn(async () => historyPage),
    selfCheckIn: vi.fn(async (_notes?: string | null) => openVisit),
    selfCheckOut: vi.fn(async () => closedVisit),
  };

  return { mocks, gateway: mocks as unknown as CheckInsGateway };
}

function setup() {
  const { mocks, gateway } = gatewayMock();
  TestBed.configureTestingModule({
    providers: [
      CheckInsFacade,
      OccupancyFacade,
      VisitHistoryFacade,
      { provide: CHECKINS_GATEWAY, useValue: gateway },
    ],
  });

  return {
    mocks,
    facade: TestBed.inject(CheckInsFacade),
    occupancyFacade: TestBed.inject(OccupancyFacade),
    historyFacade: TestBed.inject(VisitHistoryFacade),
  };
}

describe('CheckInsFacade', () => {
  it('loads the preferred branch, lookup, occupancy and eligibility', async () => {
    const { facade, mocks } = setup();

    await facade.initialize();

    expect(facade.branchId()).toBe(shoreditch.id);
    expect(facade.branch()?.code).toBe('SHR');
    expect(facade.members()).toEqual([member]);
    expect(facade.memberTotal()).toBe(19);
    expect(facade.memberPageCount()).toBe(4);
    expect(facade.selectedMemberId()).toBe(member.id);
    expect(facade.eligibility()?.allowed).toBe(true);
    expect(mocks.occupancy).toHaveBeenCalledWith(shoreditch.id);
    expect(mocks.stats).toHaveBeenCalledWith(shoreditch.id);
    expect(mocks.history).toHaveBeenCalledWith({
      branchId: shoreditch.id,
      page: 1,
      pageSize: 6,
    });
    expect(facade.loading()).toBe(false);
  });

  it('switches branch and clears the selected member', async () => {
    const { facade, mocks } = setup();
    await facade.initialize();

    await facade.selectBranch(camden.id);

    expect(facade.branchId()).toBe(camden.id);
    expect(facade.selectedMemberId()).toBeNull();
    expect(facade.eligibility()).toBeNull();
    expect(mocks.occupancy).toHaveBeenLastCalledWith(camden.id);
  });

  it('searches and pages the member directory', async () => {
    const { facade, mocks } = setup();
    await facade.initialize();

    await facade.searchMembers('maya');
    expect(facade.memberSearch()).toBe('maya');
    expect(facade.memberPage()).toBe(1);
    expect(mocks.members).toHaveBeenLastCalledWith({
      search: 'maya',
      page: 1,
      pageSize: 6,
    });

    await facade.setMemberPage(2);
    expect(mocks.members).toHaveBeenLastCalledWith({
      search: 'maya',
      page: 2,
      pageSize: 6,
    });
  });

  it('checks the selected member in and refreshes the workspace', async () => {
    const { facade, mocks } = setup();
    await facade.initialize();

    const checkedIn = await facade.checkInSelected();

    expect(checkedIn).toBe(true);
    expect(mocks.checkIn).toHaveBeenCalledWith({
      memberId: member.id,
      branchId: shoreditch.id,
      notes: null,
    });
    expect(facade.notice()).toContain('Maya Thompson checked in');
    expect(mocks.eligibility).toHaveBeenCalledTimes(2);
    expect(facade.actionLoading()).toBe(false);
  });

  it('checks a visit out and reports the result', async () => {
    const { facade, mocks } = setup();
    await facade.initialize();

    const checkedOut = await facade.checkOut(openVisit.id);

    expect(checkedOut).toBe(true);
    expect(mocks.checkOut).toHaveBeenCalledWith(openVisit.id);
    expect(facade.notice()).toContain('Maya Thompson checked out');
  });

  it('surfaces API messages and resets loading state', async () => {
    const { facade, mocks } = setup();
    mocks.checkIn.mockRejectedValueOnce({ error: { message: 'Member already inside.' } });
    await facade.initialize();

    const checkedIn = await facade.checkInSelected();

    expect(checkedIn).toBe(false);
    expect(facade.error()).toBe('Member already inside.');
    expect(facade.actionLoading()).toBe(false);
  });

  it('reports the first API validation error when no message exists', async () => {
    const { facade, mocks } = setup();
    mocks.members.mockRejectedValueOnce({ error: { errors: ['Branch is required.'] } });

    await facade.initialize();

    expect(facade.error()).toBe('Branch is required.');
    expect(facade.loading()).toBe(false);
  });
});

describe('OccupancyFacade', () => {
  it('loads the preferred branch dashboard and refreshes after checkout', async () => {
    const { occupancyFacade, mocks } = setup();

    await occupancyFacade.initialize();

    expect(occupancyFacade.branchId()).toBe(shoreditch.id);
    expect(occupancyFacade.occupancy()?.occupied).toBe(12);
    expect(occupancyFacade.openVisits()[0].memberName).toBe('Maya Thompson');
    expect(occupancyFacade.refreshedAt()).not.toBeNull();

    await occupancyFacade.checkOut(openVisit.id);

    expect(mocks.checkOut).toHaveBeenCalledWith(openVisit.id);
    expect(occupancyFacade.notice()).toContain('checked out');
    expect(mocks.occupancy).toHaveBeenCalledTimes(2);
  });

  it('shows API failures on the dashboard', async () => {
    const { occupancyFacade, mocks } = setup();
    mocks.occupancy.mockRejectedValueOnce(new Error('offline'));

    await occupancyFacade.initialize();

    expect(occupancyFacade.error()).toBe('offline');
    expect(occupancyFacade.loading()).toBe(false);
  });

  it('switches club without reloading the same branch', async () => {
    const { occupancyFacade, mocks } = setup();
    await occupancyFacade.initialize();
    const calls = mocks.occupancy.mock.calls.length;

    await occupancyFacade.selectBranch(shoreditch.id);
    expect(mocks.occupancy.mock.calls.length).toBe(calls);

    await occupancyFacade.selectBranch(camden.id);
    expect(occupancyFacade.branchId()).toBe(camden.id);
    expect(mocks.occupancy).toHaveBeenLastCalledWith(camden.id);
  });
});

describe('VisitHistoryFacade', () => {
  it('loads all visits and combines filters', async () => {
    const { historyFacade, mocks } = setup();

    await historyFacade.initialize();

    expect(historyFacade.branches().length).toBe(2);
    expect(historyFacade.history()?.totalCount).toBe(23);
    expect(mocks.history).toHaveBeenLastCalledWith({
      search: undefined,
      status: undefined,
      branchId: undefined,
      page: 1,
      pageSize: 10,
    });

    await historyFacade.search('maya');
    await historyFacade.setStatus('closed');
    await historyFacade.setBranch(shoreditch.id);
    await historyFacade.setPage(2);

    expect(historyFacade.filters()).toEqual({
      search: 'maya',
      status: 'closed',
      branchId: shoreditch.id,
    });
    expect(historyFacade.page()).toBe(2);
    expect(mocks.history).toHaveBeenLastCalledWith({
      search: 'maya',
      status: 'closed',
      branchId: shoreditch.id,
      page: 2,
      pageSize: 10,
    });
  });

  it('clears filters and returns to the first page', async () => {
    const { historyFacade, mocks } = setup();
    await historyFacade.initialize();
    await historyFacade.search('maya');
    await historyFacade.setStatus('open');
    await historyFacade.setPage(2);

    await historyFacade.clearFilters();

    expect(historyFacade.filters()).toEqual({ search: '', status: '', branchId: '' });
    expect(historyFacade.page()).toBe(1);
    expect(mocks.history).toHaveBeenLastCalledWith({
      search: undefined,
      status: undefined,
      branchId: undefined,
      page: 1,
      pageSize: 10,
    });
  });

  it('reports history failures without leaving the loading state on', async () => {
    const { historyFacade, mocks } = setup();
    mocks.history.mockRejectedValueOnce({ error: { message: 'Access log unavailable.' } });

    await historyFacade.initialize();

    expect(historyFacade.error()).toBe('Access log unavailable.');
    expect(historyFacade.loading()).toBe(false);
  });
});
