import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_staff_dashboard/kinevor_staff_dashboard.dart';

import 'fake_staff_dashboard_repository.dart';

Widget _app(Widget home) =>
    MaterialApp(theme: KinevorStaffDashboardTheme.dark(), home: home);

void main() {
  testWidgets('dashboard renders KPIs, trend, alerts and activity', (
    tester,
  ) async {
    final repository = FakeStaffDashboardRepository();

    await tester.pumpWidget(_app(StaffDashboardScreen(repository: repository)));
    await tester.pumpAndSettle();

    expect(find.text('429'), findsOneWidget);
    expect(find.text(r'$33,570'), findsOneWidget);
    expect(find.text('51.9%'), findsOneWidget);
    expect(find.text('128'), findsOneWidget);
    expect(find.text('84'), findsOneWidget);
    expect(find.text(r'$2,520'), findsOneWidget);
    expect(find.text('Visit & booking trend'), findsOneWidget);
    expect(find.text('Maya Thompson checked in'), findsOneWidget);
    expect(find.text('7 overdue invoices need follow-up'), findsOneWidget);
    expect(find.text('5 overdue invoices need follow-up'), findsOneWidget);
  });

  testWidgets('branch and range controls reload the dashboard', (tester) async {
    final repository = FakeStaffDashboardRepository();

    await tester.pumpWidget(_app(StaffDashboardScreen(repository: repository)));
    await tester.pumpAndSettle();

    await tester.tap(
      find.byKey(const ValueKey<String>('branch-$downtownBranchId')),
    );
    await tester.pumpAndSettle();

    expect(repository.lastBranchId, downtownBranchId);
    expect(find.text('149'), findsOneWidget);
    expect(find.text(r'$11,240'), findsOneWidget);

    await tester.tap(find.byKey(const ValueKey<String>('range-30')));
    await tester.pumpAndSettle();

    expect(repository.lastDays, 30);
    expect(repository.dashboardCalls, 3);
  });

  testWidgets('dashboard opens the alert inbox', (tester) async {
    final repository = FakeStaffDashboardRepository();

    await tester.pumpWidget(_app(StaffDashboardScreen(repository: repository)));
    await tester.pumpAndSettle();

    final inbox = find.byKey(const ValueKey<String>('view-inbox'));
    await tester.ensureVisible(inbox);
    await tester.pumpAndSettle();
    await tester.tap(inbox);
    await tester.pumpAndSettle();

    expect(find.text('OPERATIONS INBOX'), findsOneWidget);
    expect(find.text('5 overdue invoices need follow-up'), findsOneWidget);
    expect(repository.alertsCalls, 1);
  });

  testWidgets('alert inbox filters by severity and branch', (tester) async {
    final repository = FakeStaffDashboardRepository();

    await tester.pumpWidget(
      _app(DashboardAlertsScreen(repository: repository)),
    );
    await tester.pumpAndSettle();

    expect(find.text('5 overdue invoices need follow-up'), findsOneWidget);
    expect(find.text('3 overdue invoices need follow-up'), findsOneWidget);

    await tester.tap(find.byKey(const ValueKey<String>('filter-Warning')));
    await tester.pumpAndSettle();

    expect(find.text('5 overdue invoices need follow-up'), findsNothing);
    expect(find.text('3 overdue invoices need follow-up'), findsOneWidget);

    await tester.tap(find.byKey(const ValueKey<String>('filter-Info')));
    await tester.pumpAndSettle();

    expect(find.text('Spin 45 is 55% full'), findsOneWidget);
    expect(find.text('3 overdue invoices need follow-up'), findsNothing);

    await tester.tap(find.byKey(const ValueKey<String>('filter-All')));
    await tester.pumpAndSettle();
    await tester.tap(
      find.byKey(const ValueKey<String>('branch-$mainBranchId')),
    );
    await tester.pumpAndSettle();

    expect(repository.lastBranchId, mainBranchId);
    expect(find.text('7 overdue invoices need follow-up'), findsOneWidget);
    expect(find.text('5 overdue invoices need follow-up'), findsNothing);
  });

  testWidgets('empty dashboard and inbox surfaces have guidance', (
    tester,
  ) async {
    final repository = FakeStaffDashboardRepository(empty: true);

    await tester.pumpWidget(_app(StaffDashboardScreen(repository: repository)));
    await tester.pumpAndSettle();

    expect(find.text('No trend data for this range.'), findsOneWidget);
    expect(find.text('No open alerts. The floor is clear.'), findsOneWidget);
    expect(find.text('No activity recorded for this branch.'), findsOneWidget);

    await tester.pumpWidget(
      _app(DashboardAlertsScreen(repository: repository)),
    );
    await tester.pumpAndSettle();

    expect(find.text('No alerts in this view'), findsOneWidget);
    expect(find.text('Show all alerts'), findsNothing);
  });

  testWidgets('load failures offer a working retry', (tester) async {
    final repository = FakeStaffDashboardRepository(
      failure: const StaffDashboardApiException('Dashboard unavailable.'),
      failOnce: true,
    );

    await tester.pumpWidget(_app(StaffDashboardScreen(repository: repository)));
    await tester.pumpAndSettle();

    expect(find.text('Dashboard unavailable.'), findsOneWidget);
    expect(find.text('Try again'), findsOneWidget);

    await tester.tap(find.text('Try again'));
    await tester.pumpAndSettle();

    expect(repository.dashboardCalls, 2);
    expect(find.text('429'), findsOneWidget);
  });
}
