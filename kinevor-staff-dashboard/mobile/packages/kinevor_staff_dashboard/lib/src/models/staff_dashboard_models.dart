/// Trend ranges accepted by the staff dashboard API.
const List<int> dashboardRanges = [7, 30];

/// Alert priorities ordered from most to least urgent.
const List<String> alertSeverities = ['Critical', 'Warning', 'Info'];

/// Branch option shown in the dashboard selector and branch pulse.
class DashboardBranch {
  /// Creates a branch projection.
  const DashboardBranch({
    required this.id,
    required this.name,
    required this.city,
    required this.capacity,
    required this.activeMembers,
    required this.occupancyNow,
    required this.occupancyPercent,
  });

  /// Stable branch identifier.
  final String id;

  /// Branch display name.
  final String name;

  /// City shown beneath the branch name.
  final String city;

  /// Maximum simultaneous occupancy.
  final int capacity;

  /// Members with an active membership.
  final int activeMembers;

  /// People currently inside the branch.
  final int occupancyNow;

  /// Current occupancy as a percentage of [capacity].
  final double occupancyPercent;

  /// Parses a defensive branch payload.
  factory DashboardBranch.fromJson(Map<String, dynamic> json) =>
      DashboardBranch(
        id: json['id']?.toString() ?? '',
        name: json['name']?.toString() ?? '',
        city: json['city']?.toString() ?? '',
        capacity: (json['capacity'] as num?)?.toInt() ?? 0,
        activeMembers: (json['activeMembers'] as num?)?.toInt() ?? 0,
        occupancyNow: (json['occupancyNow'] as num?)?.toInt() ?? 0,
        occupancyPercent: (json['occupancyPercent'] as num?)?.toDouble() ?? 0,
      );
}

/// Period-over-period movement for headline KPIs.
class DashboardComparison {
  /// Creates a comparison projection.
  const DashboardComparison({
    required this.activeMembersPercent,
    required this.revenuePercent,
  });

  /// Active-member movement against the previous month.
  final double activeMembersPercent;

  /// Revenue movement against the previous month.
  final double revenuePercent;

  /// Parses a defensive comparison payload.
  factory DashboardComparison.fromJson(Map<String, dynamic> json) =>
      DashboardComparison(
        activeMembersPercent:
            (json['activeMembersPercent'] as num?)?.toDouble() ?? 0,
        revenuePercent: (json['revenuePercent'] as num?)?.toDouble() ?? 0,
      );
}

/// Aggregated KPI block for one branch or the whole network.
class DashboardSummary {
  /// Creates a KPI summary.
  const DashboardSummary({
    required this.totalMembers,
    required this.activeMembers,
    required this.occupancyNow,
    required this.occupancyPercent,
    required this.capacity,
    required this.checkInsToday,
    required this.bookingsToday,
    required this.revenueMonth,
    required this.outstandingInvoices,
    required this.outstandingAmount,
    required this.comparison,
  });

  /// Members of any lifecycle status.
  final int totalMembers;

  /// Members with an active membership.
  final int activeMembers;

  /// People currently inside the branch or network.
  final int occupancyNow;

  /// Current occupancy as a percentage of [capacity].
  final double occupancyPercent;

  /// Maximum simultaneous occupancy.
  final int capacity;

  /// Check-ins recorded since midnight.
  final int checkInsToday;

  /// Class bookings recorded for today.
  final int bookingsToday;

  /// Revenue collected in the current calendar month.
  final double revenueMonth;

  /// Number of invoices currently overdue.
  final int outstandingInvoices;

  /// Total value of overdue invoices.
  final double outstandingAmount;

  /// Movement against the previous month.
  final DashboardComparison comparison;

  /// Parses a defensive summary payload.
  factory DashboardSummary.fromJson(Map<String, dynamic> json) {
    final comparison = json['comparison'];
    return DashboardSummary(
      totalMembers: (json['totalMembers'] as num?)?.toInt() ?? 0,
      activeMembers: (json['activeMembers'] as num?)?.toInt() ?? 0,
      occupancyNow: (json['occupancyNow'] as num?)?.toInt() ?? 0,
      occupancyPercent: (json['occupancyPercent'] as num?)?.toDouble() ?? 0,
      capacity: (json['capacity'] as num?)?.toInt() ?? 0,
      checkInsToday: (json['checkInsToday'] as num?)?.toInt() ?? 0,
      bookingsToday: (json['bookingsToday'] as num?)?.toInt() ?? 0,
      revenueMonth: (json['revenueMonth'] as num?)?.toDouble() ?? 0,
      outstandingInvoices: (json['outstandingInvoices'] as num?)?.toInt() ?? 0,
      outstandingAmount: (json['outstandingAmount'] as num?)?.toDouble() ?? 0,
      comparison: comparison is Map
          ? DashboardComparison.fromJson(Map<String, dynamic>.from(comparison))
          : const DashboardComparison(
              activeMembersPercent: 0,
              revenuePercent: 0,
            ),
    );
  }
}

/// One day in the selected operational trend.
class DashboardTrendPoint {
  /// Creates a trend point.
  const DashboardTrendPoint({
    required this.date,
    required this.label,
    required this.checkIns,
    required this.bookings,
    required this.revenue,
  });

  /// Day represented by the point.
  final DateTime date;

  /// Short label supplied by the API, for example `Oct 20`.
  final String label;

  /// Check-ins recorded that day.
  final int checkIns;

  /// Class bookings recorded that day.
  final int bookings;

  /// Revenue collected that day.
  final double revenue;

  /// Parses a defensive trend payload.
  factory DashboardTrendPoint.fromJson(Map<String, dynamic> json) =>
      DashboardTrendPoint(
        date: _date(json['date']),
        label: json['label']?.toString() ?? '',
        checkIns: (json['checkIns'] as num?)?.toInt() ?? 0,
        bookings: (json['bookings'] as num?)?.toInt() ?? 0,
        revenue: (json['revenue'] as num?)?.toDouble() ?? 0,
      );
}

/// Recent event shown in the staff activity feed.
class DashboardActivity {
  /// Creates an activity entry.
  const DashboardActivity({
    required this.id,
    required this.branchId,
    required this.branchName,
    required this.branchCity,
    required this.kind,
    required this.title,
    required this.detail,
    required this.occurredAt,
  });

  /// Stable activity identifier.
  final String id;

  /// Branch that produced the event.
  final String branchId;

  /// Branch display name.
  final String branchName;

  /// Branch city.
  final String branchCity;

  /// Event kind, for example `CheckIn` or `Payment`.
  final String kind;

  /// Short event headline.
  final String title;

  /// Supporting detail.
  final String detail;

  /// When the event occurred.
  final DateTime occurredAt;

  /// Parses a defensive activity payload.
  factory DashboardActivity.fromJson(Map<String, dynamic> json) =>
      DashboardActivity(
        id: json['id']?.toString() ?? '',
        branchId: json['branchId']?.toString() ?? '',
        branchName: json['branchName']?.toString() ?? '',
        branchCity: json['branchCity']?.toString() ?? '',
        kind: json['kind']?.toString() ?? '',
        title: json['title']?.toString() ?? '',
        detail: json['detail']?.toString() ?? '',
        occurredAt: _date(json['occurredAt']),
      );
}

/// Operational alert or task requiring staff attention.
class DashboardAlert {
  /// Creates an alert entry.
  const DashboardAlert({
    required this.id,
    required this.branchId,
    required this.branchName,
    required this.branchCity,
    required this.type,
    required this.severity,
    required this.title,
    required this.detail,
    required this.reference,
    this.dueAt,
  });

  /// Stable alert identifier.
  final String id;

  /// Branch that owns the alert.
  final String branchId;

  /// Branch display name.
  final String branchName;

  /// Branch city.
  final String branchCity;

  /// Alert category, for example `OverdueInvoice`.
  final String type;

  /// Priority: `Critical`, `Warning` or `Info`.
  final String severity;

  /// Short alert headline.
  final String title;

  /// Supporting detail.
  final String detail;

  /// When the task is due, when scheduled.
  final DateTime? dueAt;

  /// Reference shown to staff, for example an invoice or class code.
  final String reference;

  /// Whether the task is past due relative to [reference].
  bool isOverdue(DateTime reference) {
    final due = dueAt;
    return due != null && due.isBefore(reference);
  }

  /// Parses a defensive alert payload.
  factory DashboardAlert.fromJson(Map<String, dynamic> json) => DashboardAlert(
    id: json['id']?.toString() ?? '',
    branchId: json['branchId']?.toString() ?? '',
    branchName: json['branchName']?.toString() ?? '',
    branchCity: json['branchCity']?.toString() ?? '',
    type: json['type']?.toString() ?? '',
    severity: json['severity']?.toString() ?? 'Info',
    title: json['title']?.toString() ?? '',
    detail: json['detail']?.toString() ?? '',
    dueAt: _optionalDate(json['dueAt']),
    reference: json['reference']?.toString() ?? '',
  );
}

/// Complete dashboard payload for the selected branch and range.
class StaffDashboard {
  /// Creates a dashboard payload.
  const StaffDashboard({
    required this.generatedAt,
    required this.branchName,
    required this.branchCity,
    required this.days,
    required this.summary,
    required this.trend,
    required this.activity,
    required this.alerts,
    required this.branches,
    this.branchId,
  });

  /// When the payload was assembled.
  final DateTime generatedAt;

  /// Selected branch, or null for the all-branch aggregate.
  final String? branchId;

  /// Selected branch name, or `All branches`.
  final String branchName;

  /// Selected branch city, or a location summary.
  final String branchCity;

  /// Trend range in days.
  final int days;

  /// Headline KPIs.
  final DashboardSummary summary;

  /// Daily operational trend.
  final List<DashboardTrendPoint> trend;

  /// Recent activity across the selected scope.
  final List<DashboardActivity> activity;

  /// Highest-priority alerts for the overview.
  final List<DashboardAlert> alerts;

  /// Branch selector options.
  final List<DashboardBranch> branches;

  /// Sum of check-ins across [trend].
  int get totalCheckIns =>
      trend.fold(0, (total, point) => total + point.checkIns);

  /// Sum of bookings across [trend].
  int get totalBookings =>
      trend.fold(0, (total, point) => total + point.bookings);

  /// Parses a defensive dashboard payload.
  factory StaffDashboard.fromJson(Map<String, dynamic> json) {
    final summary = json['summary'];
    return StaffDashboard(
      generatedAt: _date(json['generatedAt']),
      branchId: _optionalString(json['branchId']),
      branchName: json['branchName']?.toString() ?? 'All branches',
      branchCity: json['branchCity']?.toString() ?? '',
      days: (json['days'] as num?)?.toInt() ?? 7,
      summary: summary is Map
          ? DashboardSummary.fromJson(Map<String, dynamic>.from(summary))
          : DashboardSummary.fromJson(const {}),
      trend: _list(json['trend'], DashboardTrendPoint.fromJson),
      activity: _list(json['activity'], DashboardActivity.fromJson),
      alerts: _list(json['alerts'], DashboardAlert.fromJson),
      branches: _list(json['branches'], DashboardBranch.fromJson),
    );
  }
}

/// Complete alert inbox payload with branch selector context.
class DashboardAlerts {
  /// Creates an alert inbox payload.
  const DashboardAlerts({
    required this.generatedAt,
    required this.branchName,
    required this.branchCity,
    required this.alerts,
    required this.branches,
    this.branchId,
  });

  /// When the inbox was assembled.
  final DateTime generatedAt;

  /// Selected branch, or null for the all-branch aggregate.
  final String? branchId;

  /// Selected branch name, or `All branches`.
  final String branchName;

  /// Selected branch city, or a location summary.
  final String branchCity;

  /// Severity-ordered alerts.
  final List<DashboardAlert> alerts;

  /// Branch selector options.
  final List<DashboardBranch> branches;

  /// Parses a defensive inbox payload.
  factory DashboardAlerts.fromJson(Map<String, dynamic> json) =>
      DashboardAlerts(
        generatedAt: _date(json['generatedAt']),
        branchId: _optionalString(json['branchId']),
        branchName: json['branchName']?.toString() ?? 'All branches',
        branchCity: json['branchCity']?.toString() ?? '',
        alerts: _list(json['alerts'], DashboardAlert.fromJson),
        branches: _list(json['branches'], DashboardBranch.fromJson),
      );
}

DateTime _date(Object? value) =>
    DateTime.tryParse(value?.toString() ?? '') ?? DateTime(1970);

DateTime? _optionalDate(Object? value) {
  final text = value?.toString();
  if (text == null || text.isEmpty) return null;
  return DateTime.tryParse(text);
}

String? _optionalString(Object? value) {
  final text = value?.toString();
  return text == null || text.isEmpty ? null : text;
}

List<T> _list<T>(Object? value, T Function(Map<String, dynamic>) parse) {
  if (value is! List) return const [];
  return [
    for (final item in value)
      if (item is Map) parse(Map<String, dynamic>.from(item)),
  ];
}
