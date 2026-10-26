import { InjectionToken } from '@angular/core';

/** Compact access projection used by the front-desk lookup. */
export interface AccessMemberSummary {
  readonly id: string;
  readonly memberNumber: string;
  readonly fullName: string;
  readonly email: string | null;
  readonly status: string;
  readonly membershipStatus: string;
  readonly membershipPlan: string;
  readonly membershipStartsAt: string;
  readonly membershipEndsAt: string;
  readonly homeBranchId: string | null;
  readonly homeBranchName: string | null;
  readonly allClubsAccess: boolean;
  readonly isInside: boolean;
  readonly openCheckInId: string | null;
  readonly checkedInAt: string | null;
}

/** Access decision with the member and branch context that produced it. */
export interface Eligibility {
  readonly memberId: string;
  readonly memberNumber: string;
  readonly fullName: string;
  readonly memberStatus: string;
  readonly membershipStatus: string;
  readonly membershipPlan: string;
  readonly membershipStartsAt: string;
  readonly membershipEndsAt: string;
  readonly homeBranchId: string | null;
  readonly homeBranchName: string | null;
  readonly allClubsAccess: boolean;
  readonly branchId: string | null;
  readonly branchName: string | null;
  readonly allowed: boolean;
  readonly code: string;
  readonly message: string;
  readonly openCheckInId: string | null;
  readonly checkedInAt: string | null;
  readonly evaluatedAt: string;
}

/** One member visit. */
export interface CheckInVisit {
  readonly id: string;
  readonly memberId: string;
  readonly membershipId: string;
  readonly memberName: string;
  readonly memberNumber: string;
  readonly branchId: string;
  readonly branchName: string;
  readonly checkedInAt: string;
  readonly checkedOutAt: string | null;
  readonly durationMinutes: number | null;
  readonly isOpen: boolean;
  readonly source: string;
  readonly notes: string | null;
  readonly recordedBy: string;
  readonly recordedByRole: string;
  readonly accessCode: string;
}

/** Branch and its safe occupancy capacity. */
export interface Branch {
  readonly id: string;
  readonly name: string;
  readonly code: string;
  readonly city: string;
  readonly capacity: number;
}

/** Point-in-time occupancy snapshot for a branch. */
export interface Occupancy {
  readonly branchId: string;
  readonly branchName: string;
  readonly branchCode: string;
  readonly city: string;
  readonly capacity: number;
  readonly occupied: number;
  readonly available: number;
  readonly utilizationPercent: number;
  readonly status: string;
  readonly asOf: string;
  readonly openVisits: CheckInVisit[];
}

/** Aggregated visit statistics for one UTC day. */
export interface DailyStats {
  readonly date: string;
  readonly branchId: string | null;
  readonly branchName: string | null;
  readonly totalCheckIns: number;
  readonly uniqueMembers: number;
  readonly completedVisits: number;
  readonly openVisits: number;
  readonly averageDurationMinutes: number;
  readonly peakHour: number | null;
  readonly peakHourCheckIns: number;
}

/** Append-only access audit projection. */
export interface AccessAudit {
  readonly id: string;
  readonly memberId: string;
  readonly memberNumber: string;
  readonly memberName: string;
  readonly branchId: string | null;
  readonly checkInId: string | null;
  readonly action: string;
  readonly outcome: string;
  readonly code: string;
  readonly message: string;
  readonly actorId: string;
  readonly actorRole: string;
  readonly source: string;
  readonly occurredAt: string;
}

/** Member self-service status with eligibility, open visit and occupancy. */
export interface MemberCheckInStatus {
  readonly memberId: string;
  readonly memberNumber: string;
  readonly fullName: string;
  readonly memberStatus: string;
  readonly membershipStatus: string;
  readonly eligibility: Eligibility;
  readonly openCheckIn: CheckInVisit | null;
  readonly occupancy: Occupancy | null;
  readonly visitsThisMonth: number;
  readonly lastVisit: CheckInVisit | null;
}

/** Page of results returned by the API. */
export interface PagedResult<T> {
  readonly items: T[];
  readonly totalCount: number;
  readonly page: number;
  readonly pageSize: number;
  readonly totalPages: number;
  readonly hasNext: boolean;
  readonly hasPrev: boolean;
}

/** Standard Kinevor response envelope. */
export interface ApiEnvelope<T> {
  readonly success: boolean;
  readonly data: T;
  readonly message?: string | null;
  readonly errors?: string[] | null;
}

/** Front-desk member search. */
export interface MemberListQuery {
  readonly search?: string;
  readonly page?: number;
  readonly pageSize?: number;
}

/** Visit-history filters. */
export interface CheckInHistoryQuery {
  readonly search?: string;
  readonly memberId?: string;
  readonly branchId?: string;
  readonly from?: string;
  readonly to?: string;
  readonly status?: 'open' | 'closed';
  readonly page?: number;
  readonly pageSize?: number;
}

/** Front-desk check-in payload. */
export interface CreateCheckInRequest {
  readonly memberId: string;
  readonly branchId?: string | null;
  readonly notes?: string | null;
}

/** Injector token for the check-ins gateway. */
export const CHECKINS_GATEWAY = new InjectionToken<CheckInsGateway>('CheckInsGateway');

/** Base URL of the check-ins API, for example `http://localhost:5086/api/v1`. */
export const CHECKINS_API_BASE_URL = new InjectionToken<string>('CHECKINS_API_BASE_URL');

/** HTTP contract consumed by the check-in facades. */
export interface CheckInsGateway {
  /** Searches access member projections. */
  members(query: MemberListQuery): Promise<PagedResult<AccessMemberSummary>>;
  /** Evaluates eligibility at a branch. */
  eligibility(memberId: string, branchId?: string | null): Promise<Eligibility>;
  /** Searches visit history. */
  history(query: CheckInHistoryQuery): Promise<PagedResult<CheckInVisit>>;
  /** Reads daily statistics. */
  stats(branchId?: string | null, date?: string | null): Promise<DailyStats>;
  /** Reads a live occupancy snapshot. */
  occupancy(branchId?: string | null): Promise<Occupancy>;
  /** Lists branches. */
  branches(): Promise<Branch[]>;
  /** Searches the access audit log. */
  audit(memberId?: string | null, page?: number, pageSize?: number): Promise<PagedResult<AccessAudit>>;
  /** Records a front-desk check-in. */
  checkIn(request: CreateCheckInRequest): Promise<CheckInVisit>;
  /** Closes a visit from the front desk. */
  checkOut(checkInId: string): Promise<CheckInVisit>;
  /** Reads the authenticated member's check-in status. */
  myStatus(): Promise<MemberCheckInStatus>;
  /** Lists the authenticated member's visits. */
  myVisits(page?: number, pageSize?: number): Promise<PagedResult<CheckInVisit>>;
  /** Checks the authenticated member in at their home branch. */
  selfCheckIn(notes?: string | null): Promise<CheckInVisit>;
  /** Checks the authenticated member out. */
  selfCheckOut(): Promise<CheckInVisit>;
}

/** Visit sources accepted by the API. */
export const CHECK_IN_SOURCES = ['FrontDesk', 'MemberApp'] as const;

/** Occupancy statuses returned by the API. */
export const OCCUPANCY_STATUSES = ['Comfortable', 'Busy', 'NearlyFull', 'Full'] as const;
