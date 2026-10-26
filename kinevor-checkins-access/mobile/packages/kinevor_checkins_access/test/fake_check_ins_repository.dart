import 'package:kinevor_checkins_access/kinevor_checkins_access.dart';

class FakeCheckInsRepository implements CheckInsRepository {
  FakeCheckInsRepository({
    MemberCheckInStatus? status,
    List<CheckInVisit>? visits,
    this.failure,
  }) : _status = status ?? sampleStatus,
       _visits = [...(visits ?? sampleVisits)];

  final Object? failure;
  MemberCheckInStatus _status;
  final List<CheckInVisit> _visits;
  int statusCalls = 0;
  int visitCalls = 0;
  int checkInCalls = 0;
  int checkOutCalls = 0;
  int? lastPage;
  int? lastPageSize;
  DateTime? lastFrom;
  DateTime? lastTo;

  void _check() {
    if (failure != null) throw failure!;
  }

  @override
  Future<MemberCheckInStatus> getStatus() async {
    statusCalls++;
    _check();
    return _status;
  }

  @override
  Future<VisitPage> getVisits({
    int page = 1,
    int pageSize = 20,
    DateTime? from,
    DateTime? to,
  }) async {
    visitCalls++;
    lastPage = page;
    lastPageSize = pageSize;
    lastFrom = from;
    lastTo = to;
    _check();
    final start = (page - 1) * pageSize;
    final end = (start + pageSize).clamp(0, _visits.length);
    final items = start >= _visits.length
        ? <CheckInVisit>[]
        : _visits.sublist(start, end);
    final totalPages = (_visits.length / pageSize).ceil();
    return VisitPage(
      items: items,
      totalCount: _visits.length,
      page: page,
      pageSize: pageSize,
      totalPages: totalPages,
      hasNext: page < totalPages,
      hasPrev: page > 1,
    );
  }

  @override
  Future<CheckInVisit> checkIn({String? notes}) async {
    checkInCalls++;
    _check();
    final visit = sampleOpenVisit;
    _status = _statusWith(openCheckIn: visit);
    return visit;
  }

  @override
  Future<CheckInVisit> checkOut() async {
    checkOutCalls++;
    _check();
    final visit = CheckInVisit(
      id: sampleOpenVisit.id,
      memberName: sampleOpenVisit.memberName,
      memberNumber: sampleOpenVisit.memberNumber,
      branchId: sampleOpenVisit.branchId,
      branchName: sampleOpenVisit.branchName,
      checkedInAt: sampleOpenVisit.checkedInAt,
      checkedOutAt: DateTime(2026, 10, 21, 9, 5),
      durationMinutes: 64,
      isOpen: false,
      source: 'MemberApp',
      notes: null,
      accessCode: 'ALLOWED',
    );
    _status = _statusWith(openCheckIn: null);
    return visit;
  }

  MemberCheckInStatus _statusWith({required CheckInVisit? openCheckIn}) {
    return MemberCheckInStatus(
      memberId: _status.memberId,
      memberNumber: _status.memberNumber,
      fullName: _status.fullName,
      memberStatus: _status.memberStatus,
      membershipStatus: _status.membershipStatus,
      eligibility: _status.eligibility,
      openCheckIn: openCheckIn,
      occupancy: _status.occupancy,
      visitsThisMonth: _status.visitsThisMonth,
      lastVisit: _status.lastVisit,
    );
  }
}

const sampleBranchId = '22222222-2222-2222-2222-222222222222';

final sampleEligibility = CheckInEligibility(
  memberId: '11111111-1111-1111-1111-111111111111',
  memberNumber: 'KZN-2026-00001',
  fullName: 'Maya Thompson',
  memberStatus: 'Active',
  membershipStatus: 'Active',
  membershipPlan: 'Performance',
  membershipStartsAt: _startsAt,
  membershipEndsAt: _endsAt,
  homeBranchName: 'Kinevor Shoreditch',
  allClubsAccess: false,
  branchName: 'Kinevor Shoreditch',
  allowed: true,
  code: 'ALLOWED',
  message: 'Access granted. Enjoy your session.',
  openCheckInId: null,
  checkedInAt: null,
  evaluatedAt: _evaluatedAt,
);

final _startsAt = DateTime.utc(2025, 10, 20);
final _endsAt = DateTime.utc(2026, 10, 20);
final _evaluatedAt = DateTime.utc(2026, 10, 21, 8);

final sampleOccupancy = BranchOccupancy(
  branchId: sampleBranchId,
  branchName: 'Kinevor Shoreditch',
  branchCode: 'SHR',
  city: 'London',
  capacity: 50,
  occupied: 12,
  available: 38,
  utilizationPercent: 24,
  status: 'Comfortable',
  asOf: _evaluatedAt,
  openVisits: [],
);

final sampleLastVisit = CheckInVisit(
  id: 'visit-1',
  memberName: 'Maya Thompson',
  memberNumber: 'KZN-2026-00001',
  branchId: sampleBranchId,
  branchName: 'Kinevor Shoreditch',
  checkedInAt: DateTime(2026, 10, 19, 7, 15),
  checkedOutAt: DateTime(2026, 10, 19, 8, 30),
  durationMinutes: 75,
  isOpen: false,
  source: 'MemberApp',
  notes: null,
  accessCode: 'ALLOWED',
);

final sampleOpenVisit = CheckInVisit(
  id: 'visit-open',
  memberName: 'Maya Thompson',
  memberNumber: 'KZN-2026-00001',
  branchId: sampleBranchId,
  branchName: 'Kinevor Shoreditch',
  checkedInAt: DateTime(2026, 10, 21, 8, 1),
  checkedOutAt: null,
  durationMinutes: null,
  isOpen: true,
  source: 'MemberApp',
  notes: null,
  accessCode: 'ALLOWED',
);

final sampleStatus = MemberCheckInStatus(
  memberId: '11111111-1111-1111-1111-111111111111',
  memberNumber: 'KZN-2026-00001',
  fullName: 'Maya Thompson',
  memberStatus: 'Active',
  membershipStatus: 'Active',
  eligibility: sampleEligibility,
  openCheckIn: null,
  occupancy: sampleOccupancy,
  visitsThisMonth: 6,
  lastVisit: sampleLastVisit,
);

final sampleStatusWithOpenVisit = MemberCheckInStatus(
  memberId: sampleStatus.memberId,
  memberNumber: sampleStatus.memberNumber,
  fullName: sampleStatus.fullName,
  memberStatus: sampleStatus.memberStatus,
  membershipStatus: sampleStatus.membershipStatus,
  eligibility: sampleStatus.eligibility,
  openCheckIn: sampleOpenVisit,
  occupancy: sampleStatus.occupancy,
  visitsThisMonth: sampleStatus.visitsThisMonth,
  lastVisit: sampleStatus.lastVisit,
);

final sampleVisits = [
  sampleLastVisit,
  CheckInVisit(
    id: 'visit-2',
    memberName: 'Maya Thompson',
    memberNumber: 'KZN-2026-00001',
    branchId: sampleBranchId,
    branchName: 'Kinevor Shoreditch',
    checkedInAt: DateTime(2026, 10, 16, 18, 10),
    checkedOutAt: DateTime(2026, 10, 16, 19, 25),
    durationMinutes: 75,
    isOpen: false,
    source: 'FrontDesk',
    notes: null,
    accessCode: 'ALLOWED',
  ),
  CheckInVisit(
    id: 'visit-3',
    memberName: 'Maya Thompson',
    memberNumber: 'KZN-2026-00001',
    branchId: sampleBranchId,
    branchName: 'Kinevor Shoreditch',
    checkedInAt: DateTime(2026, 10, 14, 7, 5),
    checkedOutAt: DateTime(2026, 10, 14, 8, 2),
    durationMinutes: 57,
    isOpen: false,
    source: 'MemberApp',
    notes: null,
    accessCode: 'ALLOWED',
  ),
  CheckInVisit(
    id: 'visit-4',
    memberName: 'Maya Thompson',
    memberNumber: 'KZN-2026-00001',
    branchId: sampleBranchId,
    branchName: 'Kinevor Shoreditch',
    checkedInAt: DateTime(2026, 10, 11, 9, 30),
    checkedOutAt: DateTime(2026, 10, 11, 10, 50),
    durationMinutes: 80,
    isOpen: false,
    source: 'FrontDesk',
    notes: null,
    accessCode: 'ALLOWED',
  ),
  CheckInVisit(
    id: 'visit-5',
    memberName: 'Maya Thompson',
    memberNumber: 'KZN-2026-00001',
    branchId: sampleBranchId,
    branchName: 'Kinevor Shoreditch',
    checkedInAt: DateTime(2026, 10, 8, 18, 40),
    checkedOutAt: DateTime(2026, 10, 8, 19, 45),
    durationMinutes: 65,
    isOpen: false,
    source: 'MemberApp',
    notes: null,
    accessCode: 'ALLOWED',
  ),
  CheckInVisit(
    id: 'visit-6',
    memberName: 'Maya Thompson',
    memberNumber: 'KZN-2026-00001',
    branchId: sampleBranchId,
    branchName: 'Kinevor Shoreditch',
    checkedInAt: DateTime(2026, 10, 5, 7, 20),
    checkedOutAt: DateTime(2026, 10, 5, 8, 35),
    durationMinutes: 75,
    isOpen: false,
    source: 'FrontDesk',
    notes: null,
    accessCode: 'ALLOWED',
  ),
];
