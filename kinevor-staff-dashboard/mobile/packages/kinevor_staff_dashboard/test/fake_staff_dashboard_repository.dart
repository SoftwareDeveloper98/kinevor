import 'package:kinevor_staff_dashboard/kinevor_staff_dashboard.dart';

class FakeStaffDashboardRepository implements StaffDashboardRepository {
  FakeStaffDashboardRepository({
    this.failure,
    this.failOnce = false,
    this.empty = false,
  });

  final Object? failure;
  final bool failOnce;
  final bool empty;
  bool _failed = false;

  int dashboardCalls = 0;
  int alertsCalls = 0;
  int branchesCalls = 0;
  String? lastBranchId;
  int? lastDays;

  void _check() {
    if (failure == null) return;
    if (failOnce && _failed) return;
    _failed = true;
    throw failure!;
  }

  @override
  Future<StaffDashboard> getDashboard({String? branchId, int days = 7}) async {
    dashboardCalls++;
    lastBranchId = branchId;
    lastDays = days;
    _check();

    final branch = _branch(branchId);
    final trend = empty ? <DashboardTrendPoint>[] : _trend(days);
    final activity = empty
        ? <DashboardActivity>[]
        : sampleActivity
              .where((item) => branchId == null || item.branchId == branchId)
              .toList();
    final alerts = empty
        ? <DashboardAlert>[]
        : sampleAlerts
              .where((item) => branchId == null || item.branchId == branchId)
              .take(6)
              .toList();

    return StaffDashboard(
      generatedAt: sampleGeneratedAt,
      branchId: branchId,
      branchName: branch?.name ?? 'All branches',
      branchCity: branch?.city ?? '3 locations',
      days: days,
      summary: branch == null ? sampleSummary : _summaryFor(branch),
      trend: trend,
      activity: activity,
      alerts: alerts,
      branches: sampleBranches,
    );
  }

  @override
  Future<DashboardAlerts> getAlerts({String? branchId}) async {
    alertsCalls++;
    lastBranchId = branchId;
    _check();

    final branch = _branch(branchId);
    return DashboardAlerts(
      generatedAt: sampleGeneratedAt,
      branchId: branchId,
      branchName: branch?.name ?? 'All branches',
      branchCity: branch?.city ?? '3 locations',
      alerts: empty
          ? const []
          : sampleAlerts
                .where((item) => branchId == null || item.branchId == branchId)
                .toList(),
      branches: sampleBranches,
    );
  }

  @override
  Future<List<DashboardBranch>> getBranches() async {
    branchesCalls++;
    _check();
    return sampleBranches;
  }

  DashboardBranch? _branch(String? branchId) {
    if (branchId == null) return null;
    for (final branch in sampleBranches) {
      if (branch.id == branchId) return branch;
    }
    return null;
  }

  DashboardSummary _summaryFor(DashboardBranch branch) => switch (branch.id) {
    mainBranchId => const DashboardSummary(
      totalMembers: 221,
      activeMembers: 184,
      occupancyNow: 151,
      occupancyPercent: 60.4,
      capacity: 250,
      checkInsToday: 63,
      bookingsToday: 41,
      revenueMonth: 14850,
      outstandingInvoices: 7,
      outstandingAmount: 1240,
      comparison: DashboardComparison(
        activeMembersPercent: 7,
        revenuePercent: 6.8,
      ),
    ),
    downtownBranchId => const DashboardSummary(
      totalMembers: 176,
      activeMembers: 149,
      occupancyNow: 92,
      occupancyPercent: 51.1,
      capacity: 180,
      checkInsToday: 44,
      bookingsToday: 29,
      revenueMonth: 11240,
      outstandingInvoices: 5,
      outstandingAmount: 860,
      comparison: DashboardComparison(
        activeMembersPercent: 5.7,
        revenuePercent: 4.6,
      ),
    ),
    _ => const DashboardSummary(
      totalMembers: 124,
      activeMembers: 96,
      occupancyNow: 58,
      occupancyPercent: 38.7,
      capacity: 150,
      checkInsToday: 21,
      bookingsToday: 14,
      revenueMonth: 7480,
      outstandingInvoices: 3,
      outstandingAmount: 420,
      comparison: DashboardComparison(
        activeMembersPercent: 5.5,
        revenuePercent: 3.9,
      ),
    ),
  };

  List<DashboardTrendPoint> _trend(int days) {
    const months = [
      'Jan',
      'Feb',
      'Mar',
      'Apr',
      'May',
      'Jun',
      'Jul',
      'Aug',
      'Sep',
      'Oct',
      'Nov',
      'Dec',
    ];
    final end = DateTime(2026, 10, 20);
    return List<DashboardTrendPoint>.generate(days, (index) {
      final date = end.subtract(Duration(days: days - 1 - index));
      final checkIns = 110 + (index * 13) % 36;
      return DashboardTrendPoint(
        date: date,
        label: '${months[date.month - 1]} ${date.day}',
        checkIns: checkIns,
        bookings: 64 + (index * 9) % 28,
        revenue: 1020 + index * 17,
      );
    });
  }
}

const mainBranchId = 'a1111111-1111-4111-8111-111111111111';
const downtownBranchId = 'b2222222-2222-4222-8222-222222222222';
const westsideBranchId = 'c3333333-3333-4333-8333-333333333333';

final sampleGeneratedAt = DateTime(2026, 10, 20, 11, 5);

final sampleBranches = [
  const DashboardBranch(
    id: mainBranchId,
    name: 'Kinevor Gym - Main',
    city: 'Manchester',
    capacity: 250,
    activeMembers: 184,
    occupancyNow: 151,
    occupancyPercent: 60.4,
  ),
  const DashboardBranch(
    id: downtownBranchId,
    name: 'Kinevor Gym - Downtown',
    city: 'Manchester',
    capacity: 180,
    activeMembers: 149,
    occupancyNow: 92,
    occupancyPercent: 51.1,
  ),
  const DashboardBranch(
    id: westsideBranchId,
    name: 'Kinevor Gym - Westside',
    city: 'Leeds',
    capacity: 150,
    activeMembers: 96,
    occupancyNow: 58,
    occupancyPercent: 38.7,
  ),
];

const sampleSummary = DashboardSummary(
  totalMembers: 521,
  activeMembers: 429,
  occupancyNow: 301,
  occupancyPercent: 51.9,
  capacity: 580,
  checkInsToday: 128,
  bookingsToday: 84,
  revenueMonth: 33570,
  outstandingInvoices: 15,
  outstandingAmount: 2520,
  comparison: DashboardComparison(
    activeMembersPercent: 6.2,
    revenuePercent: 5.4,
  ),
);

final sampleActivity = [
  _activity(
    'activity-12',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'CheckIn',
    'Liam Chen checked in',
    'Personal training - 11:00',
    DateTime(2026, 10, 20, 10, 41),
  ),
  _activity(
    'activity-11',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'Notification',
    'Class reminder delivered to 24 members',
    'Reformer Pilates - 100% delivered',
    DateTime(2026, 10, 20, 10, 24),
  ),
  _activity(
    'activity-10',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'MemberJoined',
    'Amina Yusuf joined as an annual member',
    'Premium annual - Main',
    DateTime(2026, 10, 20, 10, 7),
  ),
  _activity(
    'activity-9',
    downtownBranchId,
    'Kinevor Gym - Downtown',
    'Manchester',
    'Payment',
    'Payment received from Arjun Patel',
    r'$120.00 personal training pack',
    DateTime(2026, 10, 20, 9, 57),
  ),
  _activity(
    'activity-8',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'Payment',
    'Payment received from Sofia Rossi',
    r'$85.00 monthly membership',
    DateTime(2026, 10, 20, 9, 48),
  ),
  _activity(
    'activity-7',
    westsideBranchId,
    'Kinevor Gym - Westside',
    'Leeds',
    'Notification',
    'New class schedule sent to 82 members',
    'November timetable - 98% delivered',
    DateTime(2026, 10, 20, 9, 44),
  ),
  _activity(
    'activity-6',
    westsideBranchId,
    'Kinevor Gym - Westside',
    'Leeds',
    'MemberJoined',
    'Grace Kim joined as a student member',
    'Student monthly - Westside',
    DateTime(2026, 10, 20, 9, 20),
  ),
  _activity(
    'activity-5',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'ClassBooking',
    'Daniel Okafor booked Reformer Pilates',
    'Today at 18:30 - Studio 2',
    DateTime(2026, 10, 20, 9, 16),
  ),
  _activity(
    'activity-4',
    downtownBranchId,
    'Kinevor Gym - Downtown',
    'Manchester',
    'ClassBooking',
    'Olivia Brown booked Spin 45',
    'Today at 17:15 - Cycle studio',
    DateTime(2026, 10, 20, 9, 12),
  ),
  _activity(
    'activity-3',
    westsideBranchId,
    'Kinevor Gym - Westside',
    'Leeds',
    'ClassBooking',
    'Tom Becker booked Mobility Flow',
    'Today at 19:00 - Studio 1',
    DateTime(2026, 10, 20, 8, 46),
  ),
  _activity(
    'activity-2',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'CheckIn',
    'Maya Thompson checked in',
    'Strength floor - 08:42',
    DateTime(2026, 10, 20, 8, 42),
  ),
  _activity(
    'activity-1',
    downtownBranchId,
    'Kinevor Gym - Downtown',
    'Manchester',
    'Membership',
    'Noah Williams renewed his membership',
    '12-month commitment - Downtown',
    DateTime(2026, 10, 20, 8, 31),
  ),
];

final sampleAlerts = [
  _alert(
    'alert-1',
    downtownBranchId,
    'Kinevor Gym - Downtown',
    'Manchester',
    'OverdueInvoice',
    'Critical',
    '5 overdue invoices need follow-up',
    r'$860 outstanding across 5 members',
    DateTime(2026, 10, 18, 9),
    'Billing - Downtown',
  ),
  _alert(
    'alert-2',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'OverdueInvoice',
    'Critical',
    '7 overdue invoices need follow-up',
    r'$1,240 outstanding across 7 members',
    DateTime(2026, 10, 19, 9),
    'Billing - Main',
  ),
  _alert(
    'alert-3',
    westsideBranchId,
    'Kinevor Gym - Westside',
    'Leeds',
    'OverdueInvoice',
    'Warning',
    '3 overdue invoices need follow-up',
    r'$420 outstanding across 3 members',
    DateTime(2026, 10, 19, 9),
    'Billing - Westside',
  ),
  _alert(
    'alert-4',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'FailedNotification',
    'Warning',
    '2 payment reminders failed',
    'Two members have invalid push tokens',
    DateTime(2026, 10, 20, 9),
    'NOT-0881',
  ),
  _alert(
    'alert-5',
    westsideBranchId,
    'Kinevor Gym - Westside',
    'Leeds',
    'FailedNotification',
    'Warning',
    '1 receipt was not delivered',
    'Retry the e-mail from the billing screen',
    DateTime(2026, 10, 20, 10),
    'NOT-0889',
  ),
  _alert(
    'alert-6',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'ExpiringMembership',
    'Warning',
    '3 memberships expire this week',
    'Maya Thompson, Liam Chen and 1 other',
    DateTime(2026, 10, 22, 9),
    'Memberships - Main',
  ),
  _alert(
    'alert-7',
    downtownBranchId,
    'Kinevor Gym - Downtown',
    'Manchester',
    'ExpiringMembership',
    'Warning',
    '4 memberships expire this week',
    'Renewal calls assigned to the front desk',
    DateTime(2026, 10, 23, 9),
    'Memberships - Downtown',
  ),
  _alert(
    'alert-8',
    downtownBranchId,
    'Kinevor Gym - Downtown',
    'Manchester',
    'FailedNotification',
    'Info',
    '1 class reminder was not delivered',
    'Member has opted out of push notifications',
    DateTime(2026, 10, 20, 8),
    'NOT-0884',
  ),
  _alert(
    'alert-9',
    downtownBranchId,
    'Kinevor Gym - Downtown',
    'Manchester',
    'LowClassFill',
    'Info',
    'Spin 45 is 55% full',
    '11 of 20 bikes booked for 17:15',
    DateTime(2026, 10, 20, 17, 15),
    'CLS-2431',
  ),
  _alert(
    'alert-10',
    mainBranchId,
    'Kinevor Gym - Main',
    'Manchester',
    'LowClassFill',
    'Info',
    'Reformer Pilates is 40% full',
    '8 of 20 seats booked for tonight',
    DateTime(2026, 10, 20, 18, 30),
    'CLS-2418',
  ),
  _alert(
    'alert-11',
    westsideBranchId,
    'Kinevor Gym - Westside',
    'Leeds',
    'LowClassFill',
    'Info',
    'Mobility Flow is 38% full',
    '6 of 16 mats booked for 19:00',
    DateTime(2026, 10, 20, 19),
    'CLS-2444',
  ),
  _alert(
    'alert-12',
    westsideBranchId,
    'Kinevor Gym - Westside',
    'Leeds',
    'ExpiringMembership',
    'Info',
    '2 memberships expire this week',
    'Both members are on monthly plans',
    DateTime(2026, 10, 24, 9),
    'Memberships - Westside',
  ),
];

DashboardActivity _activity(
  String id,
  String branchId,
  String branchName,
  String branchCity,
  String kind,
  String title,
  String detail,
  DateTime occurredAt,
) => DashboardActivity(
  id: id,
  branchId: branchId,
  branchName: branchName,
  branchCity: branchCity,
  kind: kind,
  title: title,
  detail: detail,
  occurredAt: occurredAt,
);

DashboardAlert _alert(
  String id,
  String branchId,
  String branchName,
  String branchCity,
  String type,
  String severity,
  String title,
  String detail,
  DateTime dueAt,
  String reference,
) => DashboardAlert(
  id: id,
  branchId: branchId,
  branchName: branchName,
  branchCity: branchCity,
  type: type,
  severity: severity,
  title: title,
  detail: detail,
  dueAt: dueAt,
  reference: reference,
);
