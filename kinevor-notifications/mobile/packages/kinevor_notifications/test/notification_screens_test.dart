import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_notifications/kinevor_notifications.dart';

import 'fake_notifications_repository.dart';

void main() {
  testWidgets('inbox renders unread summary and notifications', (tester) async {
    final repository = FakeNotificationsRepository();

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorNotificationsTheme.dark(),
        home: NotificationInboxScreen(repository: repository),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('4 unread updates'), findsWidgets);
    expect(find.text('5 notifications'), findsOneWidget);
    expect(find.text('Class reminder: Strength Circuit'), findsOneWidget);
    expect(
      find.text('Member-only offer: 20% off personal training'),
      findsOneWidget,
    );
    expect(repository.notificationCalls, 1);
  });

  testWidgets('inbox unread filter requests only unread notifications', (
    tester,
  ) async {
    final repository = FakeNotificationsRepository();

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorNotificationsTheme.dark(),
        home: NotificationInboxScreen(repository: repository),
      ),
    );
    await tester.pumpAndSettle();

    await tester.tap(find.byKey(const ValueKey<String>('inbox-filter-unread')));
    await tester.pumpAndSettle();

    expect(repository.lastUnreadOnly, isTrue);
    expect(find.text('Showing 4 messages'), findsOneWidget);
  });

  testWidgets('inbox marks every notification read', (tester) async {
    final repository = FakeNotificationsRepository();

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorNotificationsTheme.dark(),
        home: NotificationInboxScreen(repository: repository),
      ),
    );
    await tester.pumpAndSettle();

    await tester.tap(find.text('Mark all read'));
    await tester.pumpAndSettle();

    expect(repository.markAllReadCalls, 1);
    expect(find.text('You are all caught up.'), findsOneWidget);
    expect(find.text('Mark all read'), findsNothing);
  });

  testWidgets('detail opens and marks the notification read', (tester) async {
    final repository = FakeNotificationsRepository();

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorNotificationsTheme.dark(),
        home: NotificationInboxScreen(repository: repository),
      ),
    );
    await tester.pumpAndSettle();

    await tester.tap(find.text('Class reminder: Strength Circuit'));
    await tester.pumpAndSettle();

    expect(find.text('MESSAGE DETAIL'), findsOneWidget);
    expect(
      find.byKey(const ValueKey<String>('detail-mark-read')),
      findsOneWidget,
    );

    await tester.tap(find.byKey(const ValueKey<String>('detail-mark-read')));
    await tester.pumpAndSettle();

    expect(repository.markReadCalls, 1);
    expect(repository.lastMarkedReadId, 'notification-1');
    expect(
      find.byKey(const ValueKey<String>('detail-mark-read')),
      findsNothing,
    );
    expect(find.text('Notification marked read.'), findsOneWidget);
  });

  testWidgets('preferences toggle a category channel', (tester) async {
    final repository = FakeNotificationsRepository();

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorNotificationsTheme.dark(),
        home: NotificationPreferencesScreen(repository: repository),
      ),
    );
    await tester.pumpAndSettle();

    final chip = find.byKey(const ValueKey<String>('preference-offers-email'));
    await tester.ensureVisible(chip);
    await tester.pumpAndSettle();

    expect(tester.widget<ChoiceChip>(chip).selected, isFalse);
    await tester.tap(chip);
    await tester.pumpAndSettle();

    expect(repository.updatePreferenceCalls, 1);
    expect(repository.lastPreferenceCategory, 'offers');
    expect(repository.lastPreferenceEmail, isTrue);
    expect(tester.widget<ChoiceChip>(chip).selected, isTrue);
  });

  testWidgets('preferences register a demo device', (tester) async {
    final repository = FakeNotificationsRepository();

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorNotificationsTheme.dark(),
        home: NotificationPreferencesScreen(repository: repository),
      ),
    );
    await tester.pumpAndSettle();

    final register = find.text('Register this device');
    await tester.ensureVisible(register);
    await tester.pumpAndSettle();
    await tester.tap(register);
    await tester.pumpAndSettle();

    expect(repository.registerDeviceCalls, 1);
    expect(repository.lastRegisteredPlatform, 'web');
    expect(repository.lastRegisteredToken, contains('demo-web-device-token'));
    expect(find.text('Web browser'), findsOneWidget);
    expect(find.text('Device registered.'), findsOneWidget);
  });

  testWidgets('inbox load failures show a retry action', (tester) async {
    final repository = FakeNotificationsRepository(
      failure: const NotificationsApiException(
        'Notifications API unavailable.',
      ),
    );

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorNotificationsTheme.dark(),
        home: NotificationInboxScreen(repository: repository),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Notifications API unavailable.'), findsOneWidget);
    expect(find.text('Try again'), findsOneWidget);
  });
}
