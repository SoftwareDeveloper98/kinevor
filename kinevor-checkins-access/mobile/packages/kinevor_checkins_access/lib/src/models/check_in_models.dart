/// Access decision for a member at one club.
class CheckInEligibility {
  /// Creates an access decision.
  const CheckInEligibility({
    required this.memberId,
    required this.memberNumber,
    required this.fullName,
    required this.memberStatus,
    required this.membershipStatus,
    required this.membershipPlan,
    required this.membershipStartsAt,
    required this.membershipEndsAt,
    required this.homeBranchName,
    required this.allClubsAccess,
    required this.branchName,
    required this.allowed,
    required this.code,
    required this.message,
    required this.openCheckInId,
    required this.checkedInAt,
    required this.evaluatedAt,
  });

  /// Parses an eligibility JSON object.
  factory CheckInEligibility.fromJson(Map<String, dynamic> json) {
    return CheckInEligibility(
      memberId: _string(json['memberId']),
      memberNumber: _string(json['memberNumber']),
      fullName: _string(json['fullName']),
      memberStatus: _string(json['memberStatus']),
      membershipStatus: _string(json['membershipStatus']),
      membershipPlan: _string(json['membershipPlan']),
      membershipStartsAt: _date(json['membershipStartsAt']),
      membershipEndsAt: _date(json['membershipEndsAt']),
      homeBranchName: _stringOrNull(json['homeBranchName']),
      allClubsAccess: _bool(json['allClubsAccess']),
      branchName: _stringOrNull(json['branchName']),
      allowed: _bool(json['allowed']),
      code: _string(json['code']),
      message: _string(json['message']),
      openCheckInId: _stringOrNull(json['openCheckInId']),
      checkedInAt: _dateOrNull(json['checkedInAt']),
      evaluatedAt: _date(json['evaluatedAt']),
    );
  }

  /// External member id.
  final String memberId;

  /// Human-readable member number.
  final String memberNumber;

  /// Member's full name.
  final String fullName;

  /// Member account status.
  final String memberStatus;

  /// Membership status.
  final String membershipStatus;

  /// Membership plan name.
  final String membershipPlan;

  /// Membership start instant.
  final DateTime membershipStartsAt;

  /// Membership expiry instant.
  final DateTime membershipEndsAt;

  /// Home club name, when linked.
  final String? homeBranchName;

  /// Whether the member can use every club.
  final bool allClubsAccess;

  /// Club evaluated for this decision.
  final String? branchName;

  /// Whether access is allowed.
  final bool allowed;

  /// Stable access decision code.
  final String code;

  /// Human-readable decision message.
  final String message;

  /// Id of the member's open visit, when one exists.
  final String? openCheckInId;

  /// Start of the open visit, when one exists.
  final DateTime? checkedInAt;

  /// Instant the decision was evaluated.
  final DateTime evaluatedAt;

  /// Two-letter initials used by avatars.
  String get initials => FullNameInitials.of(fullName);
}

/// A member visit, open or completed.
class CheckInVisit {
  /// Creates a visit.
  const CheckInVisit({
    required this.id,
    required this.memberName,
    required this.memberNumber,
    required this.branchId,
    required this.branchName,
    required this.checkedInAt,
    required this.checkedOutAt,
    required this.durationMinutes,
    required this.isOpen,
    required this.source,
    required this.notes,
    required this.accessCode,
  });

  /// Parses a visit JSON object.
  factory CheckInVisit.fromJson(Map<String, dynamic> json) {
    return CheckInVisit(
      id: _string(json['id']),
      memberName: _string(json['memberName']),
      memberNumber: _string(json['memberNumber']),
      branchId: _string(json['branchId']),
      branchName: _string(json['branchName']),
      checkedInAt: _date(json['checkedInAt']),
      checkedOutAt: _dateOrNull(json['checkedOutAt']),
      durationMinutes: _intOrNull(json['durationMinutes']),
      isOpen: _bool(json['isOpen']),
      source: _string(json['source']),
      notes: _stringOrNull(json['notes']),
      accessCode: _string(json['accessCode']),
    );
  }

  /// Visit id.
  final String id;

  /// Member display name.
  final String memberName;

  /// Member number.
  final String memberNumber;

  /// Club id.
  final String branchId;

  /// Club display name.
  final String branchName;

  /// Check-in instant.
  final DateTime checkedInAt;

  /// Check-out instant, when completed.
  final DateTime? checkedOutAt;

  /// Visit duration in whole minutes.
  final int? durationMinutes;

  /// Whether the visit is still open.
  final bool isOpen;

  /// Visit source: `FrontDesk` or `MemberApp`.
  final String source;

  /// Optional visit notes.
  final String? notes;

  /// Access decision code recorded with the visit.
  final String accessCode;

  /// Two-letter initials used by avatars.
  String get initials => FullNameInitials.of(memberName);

  /// Friendly source label.
  String get sourceLabel => source == 'MemberApp' ? 'Member app' : 'Front desk';
}

/// Live occupancy and safe capacity for one club.
class BranchOccupancy {
  /// Creates an occupancy snapshot.
  const BranchOccupancy({
    required this.branchId,
    required this.branchName,
    required this.branchCode,
    required this.city,
    required this.capacity,
    required this.occupied,
    required this.available,
    required this.utilizationPercent,
    required this.status,
    required this.asOf,
    required this.openVisits,
  });

  /// Parses an occupancy JSON object.
  factory BranchOccupancy.fromJson(Map<String, dynamic> json) {
    final visits = json['openVisits'];
    return BranchOccupancy(
      branchId: _string(json['branchId']),
      branchName: _string(json['branchName']),
      branchCode: _string(json['branchCode']),
      city: _string(json['city']),
      capacity: _int(json['capacity']),
      occupied: _int(json['occupied']),
      available: _int(json['available']),
      utilizationPercent: _double(json['utilizationPercent']),
      status: _string(json['status']),
      asOf: _date(json['asOf']),
      openVisits: visits is List
          ? [
              for (final visit in visits)
                CheckInVisit.fromJson(Map<String, dynamic>.from(visit as Map)),
            ]
          : const [],
    );
  }

  /// Club id.
  final String branchId;

  /// Club display name.
  final String branchName;

  /// Short club code.
  final String branchCode;

  /// City.
  final String city;

  /// Safe capacity.
  final int capacity;

  /// Members currently inside.
  final int occupied;

  /// Safe spaces still available.
  final int available;

  /// Occupancy percentage from 0 to 100.
  final double utilizationPercent;

  /// Comfortable, Busy, NearlyFull or Full.
  final String status;

  /// Snapshot instant.
  final DateTime asOf;

  /// Open visits included in the snapshot.
  final List<CheckInVisit> openVisits;

  /// Rounded occupancy percentage.
  int get roundedUtilization => utilizationPercent.round();
}

/// Everything the member check-in screen needs in one response.
class MemberCheckInStatus {
  /// Creates a member status snapshot.
  const MemberCheckInStatus({
    required this.memberId,
    required this.memberNumber,
    required this.fullName,
    required this.memberStatus,
    required this.membershipStatus,
    required this.eligibility,
    required this.openCheckIn,
    required this.occupancy,
    required this.visitsThisMonth,
    required this.lastVisit,
  });

  /// Parses a status JSON object.
  factory MemberCheckInStatus.fromJson(Map<String, dynamic> json) {
    final open = json['openCheckIn'];
    final occupancy = json['occupancy'];
    final last = json['lastVisit'];
    return MemberCheckInStatus(
      memberId: _string(json['memberId']),
      memberNumber: _string(json['memberNumber']),
      fullName: _string(json['fullName']),
      memberStatus: _string(json['memberStatus']),
      membershipStatus: _string(json['membershipStatus']),
      eligibility: CheckInEligibility.fromJson(
        Map<String, dynamic>.from(json['eligibility'] as Map),
      ),
      openCheckIn: open is Map
          ? CheckInVisit.fromJson(Map<String, dynamic>.from(open))
          : null,
      occupancy: occupancy is Map
          ? BranchOccupancy.fromJson(Map<String, dynamic>.from(occupancy))
          : null,
      visitsThisMonth: _int(json['visitsThisMonth']),
      lastVisit: last is Map
          ? CheckInVisit.fromJson(Map<String, dynamic>.from(last))
          : null,
    );
  }

  /// Member id.
  final String memberId;

  /// Member number.
  final String memberNumber;

  /// Member display name.
  final String fullName;

  /// Member account status.
  final String memberStatus;

  /// Membership status.
  final String membershipStatus;

  /// Access decision at the member's home club.
  final CheckInEligibility eligibility;

  /// Current open visit, when checked in.
  final CheckInVisit? openCheckIn;

  /// Live occupancy for the eligible club.
  final BranchOccupancy? occupancy;

  /// Completed visits this calendar month.
  final int visitsThisMonth;

  /// Most recent completed visit.
  final CheckInVisit? lastVisit;

  /// Two-letter initials used by avatars.
  String get initials => FullNameInitials.of(fullName);

  /// Whether the member is currently inside.
  bool get isInside => openCheckIn != null;
}

/// Page of member visits.
class VisitPage {
  /// Creates a visit page.
  const VisitPage({
    required this.items,
    required this.totalCount,
    required this.page,
    required this.pageSize,
    required this.totalPages,
    required this.hasNext,
    required this.hasPrev,
  });

  /// Parses a paged visit JSON object.
  factory VisitPage.fromJson(Map<String, dynamic> json) {
    final items = json['items'];
    return VisitPage(
      items: items is List
          ? [
              for (final item in items)
                CheckInVisit.fromJson(Map<String, dynamic>.from(item as Map)),
            ]
          : const [],
      totalCount: _int(json['totalCount']),
      page: _int(json['page']),
      pageSize: _int(json['pageSize']),
      totalPages: _int(json['totalPages']),
      hasNext: _bool(json['hasNext']),
      hasPrev: _bool(json['hasPrev']),
    );
  }

  /// Visits in this page.
  final List<CheckInVisit> items;

  /// Total matching visits.
  final int totalCount;

  /// Current page number.
  final int page;

  /// Page size.
  final int pageSize;

  /// Total pages.
  final int totalPages;

  /// Whether another page exists.
  final bool hasNext;

  /// Whether a previous page exists.
  final bool hasPrev;
}

/// Shared initials helper for member names.
abstract final class FullNameInitials {
  /// Returns up to two initials for [fullName].
  static String of(String fullName) {
    final parts = fullName
        .trim()
        .split(RegExp(r'\s+'))
        .where((part) => part.isNotEmpty)
        .take(2);
    final initials = parts.map((part) => part[0].toUpperCase()).join();
    return initials.isEmpty ? '?' : initials;
  }
}

String _string(Object? value) => value is String ? value : '';

String? _stringOrNull(Object? value) => value is String ? value : null;

bool _bool(Object? value) => value is bool && value;

int _int(Object? value) => value is num ? value.toInt() : 0;

int? _intOrNull(Object? value) => value is num ? value.toInt() : null;

double _double(Object? value) => value is num ? value.toDouble() : 0;

DateTime _date(Object? value) =>
    DateTime.parse(value is String ? value : '1970-01-01T00:00:00Z').toLocal();

DateTime? _dateOrNull(Object? value) =>
    value is String ? DateTime.parse(value).toLocal() : null;
