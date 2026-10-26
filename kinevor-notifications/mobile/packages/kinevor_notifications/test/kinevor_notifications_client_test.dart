import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:kinevor_notifications/kinevor_notifications.dart';

Map<String, dynamic> notificationJson({
  String id = 'notification-1',
  bool unread = true,
}) => {
  'id': id,
  'memberId': '11111111-1111-1111-1111-111111111111',
  'title': 'Class reminder: Strength Circuit',
  'body': 'Hi Maya, your class starts tomorrow at 18:30.',
  'channel': 'InApp',
  'status': 'Sent',
  'category': 'class_reminders',
  'recipientEmail': null,
  'templateKey': 'class_reminder',
  'scheduledAt': null,
  'sentAt': '2026-10-20T09:31:00',
  'readAt': null,
  'failureReason': null,
  'unread': unread,
  'createdAt': '2026-10-20T09:30:00',
};

http.Response envelope(Object? data, {int status = 200}) => http.Response(
  jsonEncode({'success': true, 'data': data}),
  status,
  headers: {'content-type': 'application/json'},
);

void main() {
  test('inbox request sends filters and demo headers', () async {
    late http.Request captured;
    final client = KinevorNotificationsClient(
      baseUrl: Uri.parse('http://localhost:5088/api/v1/'),
      headers: const {'X-Demo-Role': 'Member'},
      httpClient: MockClient((request) async {
        captured = request;
        return envelope({
          'items': [notificationJson()],
          'totalCount': 1,
          'page': 2,
          'pageSize': 10,
        });
      }),
    );

    final page = await client.fetchNotifications(
      category: 'offers',
      channel: 'InApp',
      unreadOnly: true,
      page: 2,
      pageSize: 10,
    );

    expect(captured.method, 'GET');
    expect(captured.url.path, '/api/v1/me/notifications');
    expect(captured.url.queryParameters, {
      'category': 'offers',
      'channel': 'InApp',
      'unreadOnly': 'true',
      'page': '2',
      'pageSize': '10',
    });
    expect(captured.headers['X-Demo-Role'], 'Member');
    expect(page.totalCount, 1);
    expect(page.items.single.title, 'Class reminder: Strength Circuit');
    expect(page.items.single.unread, isTrue);
  });

  test('unread count and mark-all-read parse their payloads', () async {
    final requests = <http.Request>[];
    final client = KinevorNotificationsClient(
      baseUrl: Uri.parse('http://localhost:5088/api/v1'),
      httpClient: MockClient((request) async {
        requests.add(request);
        if (request.url.path.endsWith('unread-count')) {
          return envelope({'unread': 4});
        }
        return envelope({'marked': 4});
      }),
    );

    final count = await client.fetchUnreadCount();
    final marked = await client.markAllRead();

    expect(count, 4);
    expect(marked, 4);
    expect(requests[0].url.path, '/api/v1/me/notifications/unread-count');
    expect(requests[1].method, 'POST');
    expect(requests[1].url.path, '/api/v1/me/notifications/read-all');
  });

  test('detail and mark-read use the notification id', () async {
    final requests = <http.Request>[];
    final client = KinevorNotificationsClient(
      baseUrl: Uri.parse('http://localhost:5088/api/v1'),
      httpClient: MockClient((request) async {
        requests.add(request);
        if (request.method == 'POST') {
          return envelope(null);
        }
        return envelope(notificationJson());
      }),
    );

    final record = await client.fetchNotification('notification-1');
    await client.markRead('notification-1');

    expect(record.id, 'notification-1');
    expect(record.failed, isFalse);
    expect(requests[0].method, 'GET');
    expect(requests[0].url.path, '/api/v1/me/notifications/notification-1');
    expect(requests[1].method, 'POST');
    expect(
      requests[1].url.path,
      '/api/v1/me/notifications/notification-1/read',
    );
  });

  test('preferences use GET and PATCH with channel flags', () async {
    final requests = <http.Request>[];
    final client = KinevorNotificationsClient(
      baseUrl: Uri.parse('http://localhost:5088/api/v1'),
      httpClient: MockClient((request) async {
        requests.add(request);
        return envelope([
          {
            'category': 'offers',
            'email': request.method == 'PATCH',
            'push': true,
            'inApp': true,
          },
        ]);
      }),
    );

    final initial = await client.fetchPreferences();
    final updated = await client.updatePreference(
      category: 'offers',
      email: true,
      push: true,
      inApp: true,
    );

    expect(initial.single.email, isFalse);
    expect(updated.single.email, isTrue);
    expect(requests[0].method, 'GET');
    expect(requests[0].url.path, '/api/v1/me/notification-preferences');
    expect(requests[1].method, 'PATCH');
    expect(requests[1].url.path, '/api/v1/me/notification-preferences/offers');
    expect(jsonDecode(requests[1].body), {
      'email': true,
      'push': true,
      'inApp': true,
    });
  });

  test('devices can be listed, registered and removed', () async {
    final requests = <http.Request>[];
    final client = KinevorNotificationsClient(
      baseUrl: Uri.parse('http://localhost:5088/api/v1'),
      httpClient: MockClient((request) async {
        requests.add(request);
        if (request.method == 'POST') {
          return envelope({
            'id': 'device-2',
            'platform': 'web',
            'tokenPreview': 'demo-w...0002',
            'registeredAt': '2026-10-20T12:00:00',
          });
        }
        return envelope([
          {
            'id': 'device-1',
            'platform': 'ios',
            'tokenPreview': 'demo-i...0001',
            'registeredAt': '2026-07-22T12:00:00',
          },
        ]);
      }),
    );

    final devices = await client.fetchDevices();
    final created = await client.registerDevice(
      token: 'demo-web-device-token-0002',
      platform: 'web',
    );
    await client.unregisterDevice('device-2');

    expect(devices.single.platform, 'ios');
    expect(created.platform, 'web');
    expect(requests[0].method, 'GET');
    expect(requests[1].method, 'POST');
    expect(jsonDecode(requests[1].body), {
      'token': 'demo-web-device-token-0002',
      'platform': 'web',
    });
    expect(requests[2].method, 'DELETE');
    expect(requests[2].url.path, '/api/v1/me/devices/device-2');
  });

  test('failed requests surface the API message', () async {
    final client = KinevorNotificationsClient(
      baseUrl: Uri.parse('http://localhost:5088/api/v1'),
      httpClient: MockClient(
        (_) async => http.Response(
          jsonEncode({
            'success': false,
            'message': 'No linked member profile.',
          }),
          403,
        ),
      ),
    );

    await expectLater(
      client.fetchUnreadCount(),
      throwsA(
        isA<NotificationsApiException>()
            .having((error) => error.statusCode, 'status', 403)
            .having(
              (error) => error.message,
              'message',
              'No linked member profile.',
            ),
      ),
    );
  });

  test('network failures are wrapped', () async {
    final client = KinevorNotificationsClient(
      baseUrl: Uri.parse('http://localhost:5088/api/v1'),
      httpClient: MockClient((_) async => throw Exception('offline')),
    );

    await expectLater(
      client.fetchNotifications(),
      throwsA(isA<NotificationsApiException>()),
    );
  });
}
