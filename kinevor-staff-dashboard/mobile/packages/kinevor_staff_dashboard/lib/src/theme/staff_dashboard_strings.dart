/// User-facing labels used by the staff dashboard widgets.
class StaffDashboardStrings {
  /// Creates a label bundle.
  const StaffDashboardStrings({
    this.brand = 'Kinevor Staff',
    this.title = 'Staff dashboard',
    this.eyebrow = 'EXECUTIVE PULSE',
    this.subtitle =
        'Memberships, revenue, occupancy and front-desk workload in one live view.',
    this.allBranches = 'All branches',
    this.range7 = '7 days',
    this.range30 = '30 days',
    this.updated = 'Updated',
    this.activeMembers = 'Active members',
    this.revenueMonth = 'Revenue this month',
    this.occupancyNow = 'Occupancy now',
    this.checkInsToday = 'Check-ins today',
    this.bookingsToday = 'Bookings today',
    this.outstandingInvoices = 'Outstanding invoices',
    this.vsLastMonth = 'vs last month',
    this.inside = 'inside',
    this.of = 'of',
    this.overdue = 'overdue',
    this.followUp = 'Needs follow-up',
    this.trendTitle = 'Visit & booking trend',
    this.trendEyebrow = 'FLOW',
    this.checkIns = 'Check-ins',
    this.bookings = 'Bookings',
    this.alertsTitle = 'Alerts & tasks',
    this.alertsEyebrow = 'OPERATIONS INBOX',
    this.alertsSubtitle =
        'Every collection risk, retention task, low-fill class and failed delivery in one severity-ordered queue.',
    this.viewInbox = 'View inbox',
    this.openInbox = 'Open alert inbox',
    this.critical = 'Critical',
    this.warning = 'Warning',
    this.info = 'Info',
    this.all = 'All',
    this.actToday = 'Act today',
    this.thisShift = 'This shift',
    this.followUpHint = 'Follow up',
    this.openStatus = 'Open',
    this.overdueStatus = 'Overdue',
    this.due = 'Due',
    this.anytime = 'anytime',
    this.noAlerts = 'No open alerts. The floor is clear.',
    this.noAlertsInView = 'No alerts in this view',
    this.noAlertsHint =
        'Try another severity or branch. A clear queue is a good queue.',
    this.showAllAlerts = 'Show all alerts',
    this.activityTitle = 'Live activity',
    this.activityEyebrow = 'FRONT DESK',
    this.noActivity = 'No activity recorded for this branch.',
    this.noTrend = 'No trend data for this range.',
    this.loadError = 'Could not load the staff dashboard.',
    this.loadAlertsError = 'Could not load the alert inbox.',
    this.retry = 'Try again',
    this.refresh = 'Refresh',
    this.actionFailed = 'The request could not be completed.',
    this.criticalTasks = 'critical tasks',
    this.windowSuffix = 'day window',
    this.shownSuffix = 'shown',
  });

  /// Small brand label.
  final String brand;

  /// Dashboard screen title.
  final String title;

  /// Dashboard eyebrow.
  final String eyebrow;

  /// Dashboard introduction.
  final String subtitle;

  /// All-branch selector label.
  final String allBranches;

  /// Seven-day range label.
  final String range7;

  /// Thirty-day range label.
  final String range30;

  /// Updated-at label.
  final String updated;

  /// Active-member KPI label.
  final String activeMembers;

  /// Monthly revenue KPI label.
  final String revenueMonth;

  /// Occupancy KPI label.
  final String occupancyNow;

  /// Today's check-ins KPI label.
  final String checkInsToday;

  /// Today's bookings KPI label.
  final String bookingsToday;

  /// Outstanding invoices KPI label.
  final String outstandingInvoices;

  /// Comparison footnote.
  final String vsLastMonth;

  /// Occupancy inside note.
  final String inside;

  /// Capacity note connector.
  final String of;

  /// Overdue footnote.
  final String overdue;

  /// Follow-up footnote.
  final String followUp;

  /// Trend card title.
  final String trendTitle;

  /// Trend eyebrow.
  final String trendEyebrow;

  /// Check-in legend label.
  final String checkIns;

  /// Booking legend label.
  final String bookings;

  /// Alert screen title.
  final String alertsTitle;

  /// Alert eyebrow.
  final String alertsEyebrow;

  /// Alert screen introduction.
  final String alertsSubtitle;

  /// Inbox navigation label.
  final String viewInbox;

  /// Inbox call to action.
  final String openInbox;

  /// Critical severity label.
  final String critical;

  /// Warning severity label.
  final String warning;

  /// Info severity label.
  final String info;

  /// All-filter label.
  final String all;

  /// Critical hint.
  final String actToday;

  /// Warning hint.
  final String thisShift;

  /// Info hint.
  final String followUpHint;

  /// Open status label.
  final String openStatus;

  /// Overdue status label.
  final String overdueStatus;

  /// Due label.
  final String due;

  /// Missing due date label.
  final String anytime;

  /// Empty dashboard alert message.
  final String noAlerts;

  /// Empty inbox message.
  final String noAlertsInView;

  /// Empty inbox hint.
  final String noAlertsHint;

  /// Reset filter action.
  final String showAllAlerts;

  /// Activity card title.
  final String activityTitle;

  /// Activity eyebrow.
  final String activityEyebrow;

  /// Empty activity message.
  final String noActivity;

  /// Empty trend message.
  final String noTrend;

  /// Dashboard load failure.
  final String loadError;

  /// Inbox load failure.
  final String loadAlertsError;

  /// Retry action.
  final String retry;

  /// Refresh action.
  final String refresh;

  /// Generic action failure.
  final String actionFailed;

  /// Critical task chip suffix.
  final String criticalTasks;

  /// Trend footer window suffix.
  final String windowSuffix;

  /// Visible alert count suffix.
  final String shownSuffix;

  /// Formats an open task count.
  String openTasks(int count) =>
      count == 1 ? '1 open task' : '$count open tasks';

  /// Formats a visible row count.
  String shown(int count) => '$count $shownSuffix';

  /// Formats a full date in the device time zone.
  String date(DateTime value) {
    final local = value.toLocal();
    return '${_months[local.month - 1]} ${local.day}, ${local.year}';
  }

  /// Formats a month and day in the device time zone.
  String monthDay(DateTime value) {
    final local = value.toLocal();
    return '${_months[local.month - 1]} ${local.day}';
  }

  /// Formats a time as HH:mm in the device time zone.
  String time(DateTime value) {
    final local = value.toLocal();
    return '${local.hour.toString().padLeft(2, '0')}:'
        '${local.minute.toString().padLeft(2, '0')}';
  }

  /// Formats a month, day and time.
  String dateTime(DateTime value) => '${monthDay(value)}, ${time(value)}';

  /// Formats an integer with thousands separators.
  String number(num value) {
    final rounded = value.round();
    final digits = rounded.abs().toString();
    final buffer = StringBuffer();
    for (var index = 0; index < digits.length; index++) {
      if (index > 0 && (digits.length - index) % 3 == 0) buffer.write(',');
      buffer.write(digits[index]);
    }
    return rounded < 0 ? '-$buffer' : buffer.toString();
  }

  /// Formats a whole-dollar currency amount.
  String currency(num value) => '\$${number(value.round())}';

  /// Formats a percentage with one decimal place.
  String percent(num value) => '${value.toStringAsFixed(1)}%';

  /// Formats a signed comparison percentage without the sign.
  String delta(num value) => percent(value.abs());

  /// Formats a check-in count.
  String checkInsTotal(num count) => '${number(count)} check-ins';

  /// Formats a booking count.
  String bookingsTotal(num count) => '${number(count)} bookings';

  /// Formats a trend window.
  String dayWindow(int days) => '$days-day window';

  /// Formats an activity count.
  String activityCount(int count) =>
      count == 1 ? '1 recent event' : '$count recent events';

  /// Formats a branch count chip.
  String branchChip(int count) => count == 1 ? '1 branch' : '$count branches';

  /// Formats a critical task chip.
  String criticalChip(int count) =>
      '$count ${count == 1 ? 'critical task' : criticalTasks}';

  /// Human label for an activity kind.
  String activityKindLabel(String kind) => switch (kind) {
    'CheckIn' => 'Check-in',
    'Payment' => 'Payment',
    'MemberJoined' => 'New member',
    'ClassBooking' => 'Class booking',
    'Membership' => 'Membership',
    'Notification' => 'Notification',
    _ => kind,
  };

  /// Human label for an alert type.
  String alertTypeLabel(String type) => switch (type) {
    'OverdueInvoice' => 'Billing',
    'ExpiringMembership' => 'Retention',
    'LowClassFill' => 'Classes',
    'FailedNotification' => 'Notifications',
    _ => type,
  };

  /// Human label for an alert severity.
  String severityLabel(String severity) => switch (severity.toLowerCase()) {
    'critical' => critical,
    'warning' => warning,
    'info' => info,
    _ => severity,
  };

  /// Short branch name used in compact chips.
  String branchName(String name) =>
      name.replaceFirst(RegExp(r'^Kinevor Gym\s*-\s*'), '').trim();

  static const List<String> _months = [
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
}
