import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_checkins_access/kinevor_checkins_access.dart';

import 'fake_check_ins_repository.dart';

Widget _app({required CheckInsRepository repository, int initialIndex = 0}) =>
    MaterialApp(
      theme: KinevorCheckInsTheme.dark(),
      home: KinevorCheckInsHome(
        repository: repository,
        initialIndex: initialIndex,
      ),
    );

Future<void> _pumpApp(
  WidgetTester tester, {
  required CheckInsRepository repository,
  int initialIndex = 0,
}) async {
  await tester.binding.setSurfaceSize(const Size(390, 844));
  addTearDown(() => tester.binding.setSurfaceSize(null));
  await tester.pumpWidget(
    _app(repository: repository, initialIndex: initialIndex),
  );
  await tester.pumpAndSettle();
}

void main() {
  testWidgets('check-in screen shows access, occupancy and last visit', (
    tester,
  ) async {
    await _pumpApp(tester, repository: FakeCheckInsRepository());

    expect(find.text('Welcome back, Maya'), findsOneWidget);
    expect(find.text("You're cleared to train"), findsOneWidget);
    expect(find.text('24%'), findsOneWidget);
    expect(find.text('12 of 50 inside'), findsOneWidget);
    expect(find.text('38 spaces free'), findsOneWidget);
    expect(find.text('Check in at Kinevor Shoreditch'), findsOneWidget);
    expect(find.text('Last visit'), findsOneWidget);
    expect(find.textContaining('KZN-2026-00001'), findsOneWidget);
  });

  testWidgets('check-in action refreshes into an active visit', (tester) async {
    final repository = FakeCheckInsRepository();
    await _pumpApp(tester, repository: repository);

    await tester.tap(find.byKey(const ValueKey('check-in-button')));
    await tester.pumpAndSettle();

    expect(repository.checkInCalls, 1);
    expect(find.text("You're checked in"), findsOneWidget);
    expect(find.byKey(const ValueKey('check-out-button')), findsOneWidget);
    expect(find.textContaining('Have a great session'), findsOneWidget);
  });

  testWidgets('check-out action closes the active visit', (tester) async {
    final repository = FakeCheckInsRepository(
      status: sampleStatusWithOpenVisit,
    );
    await _pumpApp(tester, repository: repository);

    expect(find.text("You're checked in"), findsOneWidget);
    await tester.tap(find.byKey(const ValueKey('check-out-button')));
    await tester.pumpAndSettle();

    expect(repository.checkOutCalls, 1);
    expect(find.text("You're checked in"), findsNothing);
    expect(find.byKey(const ValueKey('check-in-button')), findsOneWidget);
  });

  testWidgets('history screen shows monthly total and completed visits', (
    tester,
  ) async {
    await _pumpApp(
      tester,
      repository: FakeCheckInsRepository(),
      initialIndex: 1,
    );

    expect(find.text('YOUR ACTIVITY'), findsOneWidget);
    expect(find.text('6'), findsOneWidget);
    expect(find.text('All visits'), findsOneWidget);
    expect(find.text('6 visits'), findsOneWidget);
    expect(find.text('Kinevor Shoreditch'), findsWidgets);
    expect(find.text('1h 15m'), findsWidgets);
    expect(find.text('Front desk'), findsWidgets);
  });

  testWidgets('history tab is reachable from the check-in app bar', (
    tester,
  ) async {
    await _pumpApp(tester, repository: FakeCheckInsRepository());

    await tester.tap(
      find.descendant(
        of: find.byType(AppBar),
        matching: find.byIcon(Icons.history),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('All visits'), findsOneWidget);
    expect(find.text('Your activity'.toUpperCase()), findsOneWidget);
  });

  testWidgets('load failures offer a retry', (tester) async {
    await _pumpApp(
      tester,
      repository: FakeCheckInsRepository(
        failure: const CheckInsApiException('The API is offline.'),
      ),
    );

    expect(find.text('The API is offline.'), findsOneWidget);
    expect(find.text('Try again'), findsOneWidget);
  });
}
