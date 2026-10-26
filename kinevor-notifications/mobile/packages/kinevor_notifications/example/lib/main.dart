import 'package:flutter/material.dart';
import 'package:kinevor_notifications/kinevor_notifications.dart';

const String _apiBaseUrl = String.fromEnvironment(
  'KINEVOR_API_BASE_URL',
  defaultValue: 'http://localhost:5088/api/v1',
);

void main() {
  final client = KinevorNotificationsClient(
    baseUrl: Uri.parse(_apiBaseUrl),
    headers: const {
      'X-Demo-Role': 'Member',
      'X-Demo-Email': 'maya.thompson@kinevor.demo',
      'X-Demo-Member': '11111111-1111-1111-1111-111111111111',
    },
  );
  runApp(
    NotificationsDemoApp(repository: RemoteNotificationsRepository(client)),
  );
}

/// Demo application hosting [NotificationInboxScreen] against the DemoApi.
class NotificationsDemoApp extends StatelessWidget {
  /// Creates the demo app.
  const NotificationsDemoApp({super.key, required this.repository});

  /// Data source used by the notification screens.
  final NotificationsRepository repository;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Kinevor Notifications',
      debugShowCheckedModeBanner: false,
      theme: KinevorNotificationsTheme.dark(),
      home: NotificationInboxScreen(repository: repository),
    );
  }
}
