import {
  AccessMemberSummary,
  Branch,
  CheckInVisit,
  DailyStats,
  Eligibility,
  Occupancy,
} from './check-in.model';

export const member: AccessMemberSummary = {
  id: '11111111-1111-1111-1111-111111111111',
  memberNumber: 'KZN-2026-00001',
  fullName: 'Maya Thompson',
  email: 'maya.thompson@kinevor.demo',
  status: 'Active',
  membershipStatus: 'Active',
  membershipPlan: 'Unlimited',
  membershipStartsAt: '2026-04-21T00:00:00Z',
  membershipEndsAt: '2027-07-21T00:00:00Z',
  homeBranchId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  homeBranchName: 'Kinevor Shoreditch',
  allClubsAccess: false,
  isInside: false,
  openCheckInId: null,
  checkedInAt: null,
};

export const eligibility: Eligibility = {
  memberId: member.id,
  memberNumber: member.memberNumber,
  fullName: member.fullName,
  memberStatus: member.status,
  membershipStatus: member.membershipStatus,
  membershipPlan: member.membershipPlan,
  membershipStartsAt: member.membershipStartsAt,
  membershipEndsAt: member.membershipEndsAt,
  homeBranchId: member.homeBranchId,
  homeBranchName: member.homeBranchName,
  allClubsAccess: member.allClubsAccess,
  branchId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  branchName: 'Kinevor Shoreditch',
  allowed: true,
  code: 'Access.Allowed',
  message: 'Access granted. Enjoy your session.',
  openCheckInId: null,
  checkedInAt: null,
  evaluatedAt: '2026-10-21T08:00:00Z',
};

export const shoreditch: Branch = {
  id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  name: 'Kinevor Shoreditch',
  code: 'SHR',
  city: 'London',
  capacity: 50,
};

export const camden: Branch = {
  id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
  name: 'Kinevor Camden',
  code: 'CAM',
  city: 'London',
  capacity: 35,
};

export const openVisit: CheckInVisit = {
  id: 'c0000000-0000-0000-0000-000000000001',
  memberId: member.id,
  membershipId: 'a0000000-0000-0000-0000-000000000001',
  memberName: member.fullName,
  memberNumber: member.memberNumber,
  branchId: shoreditch.id,
  branchName: shoreditch.name,
  checkedInAt: '2026-10-21T07:15:00Z',
  checkedOutAt: null,
  durationMinutes: 45,
  isOpen: true,
  source: 'FrontDesk',
  notes: null,
  recordedBy: 'staff-riley',
  recordedByRole: 'Staff',
  accessCode: 'Access.Allowed',
};

export const closedVisit: CheckInVisit = {
  ...openVisit,
  id: 'c0000000-0000-0000-0000-000000009001',
  checkedInAt: '2026-10-19T18:05:00Z',
  checkedOutAt: '2026-10-19T19:20:00Z',
  durationMinutes: 75,
  isOpen: false,
  source: 'MemberApp',
  recordedBy: member.id,
  recordedByRole: 'Member',
};

export const occupancy: Occupancy = {
  branchId: shoreditch.id,
  branchName: shoreditch.name,
  branchCode: shoreditch.code,
  city: shoreditch.city,
  capacity: shoreditch.capacity,
  occupied: 12,
  available: 38,
  utilizationPercent: 24,
  status: 'Comfortable',
  asOf: '2026-10-21T08:00:00Z',
  openVisits: [openVisit],
};

export const stats: DailyStats = {
  date: '2026-10-21',
  branchId: shoreditch.id,
  branchName: shoreditch.name,
  totalCheckIns: 12,
  uniqueMembers: 12,
  completedVisits: 0,
  openVisits: 12,
  averageDurationMinutes: 0,
  peakHour: 7,
  peakHourCheckIns: 5,
};
