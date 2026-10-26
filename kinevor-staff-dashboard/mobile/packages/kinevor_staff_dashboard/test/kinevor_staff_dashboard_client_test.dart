import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:kinevor_staff_dashboard/kinevor_staff_dashboard.dart';

Map<String, dynamic> branchJson() => {
  'id': 'a1111111-1111-4111-8111-111111111111',
  'name': 'Kinevor Gym - Main',
  'city': 'Manchester',
  'capacity': 250,
  'activeMembers': 184,
  'occupancyNow': 151,
  'occupancyPercent': 60.4,
};

Map<String, dynamic> alertJson() => {
  'id': 'alert-1',
  'branchId': 'a1111111-1111-4111-8111-111111111111',
  'branchName': 'Kinevor Gym - Main',
  'branchCity': 'Manchester',
  'type': 'OverdueInvoice',
  'severity': 'Critical',
  'title': '7 overdue invoices need follow-up',
  'detail': r'$1,240 outstanding across 7 members',
  'dueAt': '2026-10-19T09:00:00',
  'reference': 'Billing - Main',
};

Map<String, dynamic> dashboardJson() => {
  'generatedAt': '2026-10-20T11:05:00',
  'branchId': null,
  'branchName': 'All branches',
  'branchCity': '3 locations',
  'days': 7,
  'summary': {
    'totalMembers': 521,
    'activeMembers': 429,
    'occupancyNow': 301,
    'occupancyPercent': 51.9,
    'capacity': 580,
    'checkInsToday': 128,
    'bookingsToday': 84,
    'revenueMonth': 33570,
    'outstandingInvoices': 15,
    'outstandingAmount': 2520,
    'comparison': {'activeMembersPercent': 6.2, 'revenuePercent': 5.4},
  },
  'trend': [
    {
      'date': '2026-10-19T00:00:00',
      'label': 'Oct 19',
      'checkIns': 124,
      'bookings': 77,
      'revenue': 1110,
    },
    {
      'date': '2026-10-20T00:00:00',
      'label': 'Oct 20',
      'checkIns': 128,
      'bookings': 84,
      'revenue': 1119,
    },
  ],
  'activity': [
    {
      'id': 'activity-1',
      'branchId': 'a1111111-1111-4111-8111-111111111111',
      'branchName': 'Kinevor Gym - Main',
      'branchCity': 'Manchester',
      'kind': 'CheckIn',
      'title': 'Maya Thompson checked in',
      'detail': 'Strength floor - 08:42',
      'occurredAt': '2026-10-20T08:42:00',
    },
  ],
  'alerts': [alertJson()],
  'branches': [branchJson()],
};

Map<String, dynamic> alertsJson() => {
  'generatedAt': '2026-10-20T11:05:00',
  'branchId': null,
  'branchName': 'All branches',
  'branchCity': '3 locations',
  'alerts': [alertJson()],
  'branches': [branchJson()],
};

http.Response envelope(Object? data, {int status = 200}) => http.Response(
  jsonEncode({'success': true, 'data': data}),
  status,
  headers: {'content-type': 'application/json'},
);

void main() {
  test('dashboard request sends demo headers and parses the payload', () async {
    late http.Request captured;
    final client = KinevorStaffDashboardClient(
      baseUrl: Uri.parse('http://localhost:5089/api/v1/'),
      headers: const {'X-Demo-Role': 'Staff'},
      httpClient: MockClient((request) async {
        captured = request;
        return envelope(dashboardJson());
      }),
    );

    final dashboard = await client.fetchDashboard(
      branchId: 'a1111111-1111-4111-8111-111111111111',
      days: 30,
    );

    expect(captured.method, 'GET');
    expect(
      captured.url.toString(),
      'http://localhost:5089/api/v1/staff-dashboard'
      '?days=30&branchId=a1111111-1111-4111-8111-111111111111',
    );
    expect(captured.headers['X-Demo-Role'], 'Staff');
    expect(dashboard.branchName, 'All branches');
    expect(dashboard.summary.activeMembers, 429);
    expect(dashboard.summary.comparison.revenuePercent, 5.4);
    expect(dashboard.totalCheckIns, 252);
    expect(dashboard.trend.last.label, 'Oct 20');
    expect(dashboard.activity.single.kind, 'CheckIn');
    expect(dashboard.alerts.single.severity, 'Critical');
    expect(dashboard.branches.single.capacity, 250);
  });

  test('alerts and branches parse their envelopes', () async {
    final client = KinevorStaffDashboardClient(
      baseUrl: Uri.parse('http://localhost:5089/api/v1'),
      httpClient: MockClient((request) async {
        if (request.url.path.endsWith('/alerts')) {
          return envelope(alertsJson());
        }
        return envelope([branchJson()]);
      }),
    );

    final inbox = await client.fetchAlerts();
    final branches = await client.fetchBranches();

    expect(inbox.alerts.single.reference, 'Billing - Main');
    expect(inbox.alerts.single.isOverdue(inbox.generatedAt), isTrue);
    expect(branches.single.name, 'Kinevor Gym - Main');
  });

  test('missing branch ids are omitted from the query', () async {
    late Uri captured;
    final client = KinevorStaffDashboardClient(
      baseUrl: Uri.parse('http://localhost:5089/api/v1'),
      httpClient: MockClient((request) async {
        captured = request.url;
        return envelope(dashboardJson());
      }),
    );

    await client.fetchDashboard();
    await client.fetchAlerts();

    expect(captured.queryParameters, isEmpty);
  });

  test('failed requests surface the API message', () async {
    final client = KinevorStaffDashboardClient(
      baseUrl: Uri.parse('http://localhost:5089/api/v1'),
      httpClient: MockClient(
        (_) async => http.Response(
          jsonEncode({
            'success': false,
            'message': 'The dashboard range must be 7 or 30 days.',
          }),
          400,
        ),
      ),
    );

    await expectLater(
      client.fetchDashboard(days: 14),
      throwsA(
        isA<StaffDashboardApiException>()
            .having((error) => error.statusCode, 'status', 400)
            .having(
              (error) => error.message,
              'message',
              'The dashboard range must be 7 or 30 days.',
            ),
      ),
    );
  });

  test('network and malformed responses are wrapped', () async {
    final offline = KinevorStaffDashboardClient(
      baseUrl: Uri.parse('http://localhost:5089/api/v1'),
      httpClient: MockClient((_) async => throw Exception('offline')),
    );
    final malformed = KinevorStaffDashboardClient(
      baseUrl: Uri.parse('http://localhost:5089/api/v1'),
      httpClient: MockClient((_) async => http.Response('not-json', 200)),
    );

    await expectLater(
      offline.fetchDashboard(),
      throwsA(isA<StaffDashboardApiException>()),
    );
    await expectLater(
      malformed.fetchBranches(),
      throwsA(isA<StaffDashboardApiException>()),
    );
  });
}
