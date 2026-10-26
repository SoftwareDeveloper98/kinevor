import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_notifications/kinevor_notifications.dart';

import 'fake_notifications_repository.dart';

Future<void> _loadFonts() async {
  final bytes = await File('assets/fonts/Sora-Variable.ttf').readAsBytes();
  final sora = FontLoader('Sora')
    ..addFont(Future<ByteData>.value(ByteData.sublistView(bytes)));
  await sora.load();

  final material = await rootBundle.load('fonts/MaterialIcons-Regular.otf');
  final icons = FontLoader('MaterialIcons')
    ..addFont(Future<ByteData>.value(material));
  await icons.load();
}

Future<void> _pumpScreen(WidgetTester tester, Widget screen) async {
  await tester.binding.setSurfaceSize(const Size(390, 844));
  addTearDown(() => tester.binding.setSurfaceSize(null));

  await tester.pumpWidget(
    MaterialApp(theme: KinevorNotificationsTheme.dark(), home: screen),
  );
  await tester.pumpAndSettle();
}

void main() {
  setUpAll(_loadFonts);

  testWidgets('inbox golden', (tester) async {
    await _pumpScreen(
      tester,
      NotificationInboxScreen(repository: FakeNotificationsRepository()),
    );

    await expectLater(
      find.byType(NotificationInboxScreen),
      matchesGoldenFile('goldens/notification_inbox.png'),
    );
  });

  testWidgets('notification detail golden', (tester) async {
    await _pumpScreen(
      tester,
      NotificationDetailScreen(
        repository: FakeNotificationsRepository(),
        notificationId: 'notification-1',
      ),
    );

    await expectLater(
      find.byType(NotificationDetailScreen),
      matchesGoldenFile('goldens/notification_detail.png'),
    );
  });

  testWidgets('preferences golden', (tester) async {
    await _pumpScreen(
      tester,
      NotificationPreferencesScreen(repository: FakeNotificationsRepository()),
    );

    await expectLater(
      find.byType(NotificationPreferencesScreen),
      matchesGoldenFile('goldens/notification_preferences.png'),
    );
  });
}
