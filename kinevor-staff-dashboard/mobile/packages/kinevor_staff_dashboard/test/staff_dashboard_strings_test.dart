import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_staff_dashboard/kinevor_staff_dashboard.dart';

void main() {
  const strings = StaffDashboardStrings();

  test('formats counts, currency and percentages', () {
    expect(strings.number(33570), '33,570');
    expect(strings.number(-1234), '-1,234');
    expect(strings.currency(33570), r'$33,570');
    expect(strings.percent(51.94), '51.9%');
    expect(strings.delta(-6.24), '6.2%');
  });

  test('formats UTC timestamps in the device time zone', () {
    final value = DateTime.utc(2026, 10, 20, 10, 5);
    final local = value.toLocal();

    expect(
      strings.time(value),
      '${local.hour.toString().padLeft(2, '0')}:'
      '${local.minute.toString().padLeft(2, '0')}',
    );
    expect(strings.monthDay(value), endsWith('${local.day}'));
    expect(strings.date(value), endsWith('${local.day}, ${local.year}'));
    expect(
      strings.dateTime(value),
      '${strings.monthDay(value)}, ${strings.time(value)}',
    );
  });

  test('formats singular and plural staff labels', () {
    expect(strings.openTasks(1), '1 open task');
    expect(strings.openTasks(4), '4 open tasks');
    expect(strings.shown(12), '12 shown');
    expect(strings.criticalChip(1), '1 critical task');
    expect(strings.criticalChip(2), '2 critical tasks');
    expect(strings.branchChip(1), '1 branch');
    expect(strings.activityCount(1), '1 recent event');
  });

  test('maps alert, activity and branch labels', () {
    expect(strings.alertTypeLabel('OverdueInvoice'), 'Billing');
    expect(strings.activityKindLabel('MemberJoined'), 'New member');
    expect(strings.severityLabel('WARNING'), 'Warning');
    expect(strings.branchName('Kinevor Gym - Main'), 'Main');
  });
}
