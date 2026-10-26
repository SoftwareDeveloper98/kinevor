import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:kinevor_checkins_access/kinevor_checkins_access.dart';

Map<String, dynamic> eligibilityJson() => {
  'memberId': 'member-1',
  'memberNumber': 'KZN-2026-00001',
  'fullName': 'Maya Thompson',
  'memberStatus': 'Active',
  'membershipStatus': 'Active',
  'membershipPlan': 'Performance',
  'membershipStartsAt': '2025-10-20T00:00:00Z',
  'membershipEndsAt': '2026-10-20T00:00:00Z',
  'homeBranchName': 'Kinevor Shoreditch',
  'allClubsAccess': false,
  'branchName': 'Kinevor Shoreditch',
  'allowed': true,
  'code': 'ALLOWED',
  'message': 'Access granted. Enjoy your session.',
  'openCheckInId': null,
  'checkedInAt': null,
  'evaluatedAt': '2026-10-21T08:00:00Z',
};

Map<String, dynamic> visitJson({String id = 'visit-1', bool isOpen = false}) =>
    {
      'id': id,
      'memberName': 'Maya Thompson',
      'memberNumber': 'KZN-2026-00001',
      'branchId': 'branch-1',
      'branchName': 'Kinevor Shoreditch',
      'checkedInAt': '2026-10-21T07:15:00Z',
      'checkedOutAt': isOpen ? null : '2026-10-21T08:30:00Z',
      'durationMinutes': isOpen ? null : 75,
      'isOpen': isOpen,
      'source': 'MemberApp',
      'notes': null,
      'accessCode': 'ALLOWED',
    };

Map<String, dynamic> statusJson() => {
  'memberId': 'member-1',
  'memberNumber': 'KZN-2026-00001',
  'fullName': 'Maya Thompson',
  'memberStatus': 'Active',
  'membershipStatus': 'Active',
  'eligibility': eligibilityJson(),
  'openCheckIn': null,
  'occupancy': {
    'branchId': 'branch-1',
    'branchName': 'Kinevor Shoreditch',
    'branchCode': 'SHR',
    'city': 'London',
    'capacity': 50,
    'occupied': 12,
    'available': 38,
    'utilizationPercent': 24.4,
    'status': 'Comfortable',
    'asOf': '2026-10-21T08:00:00Z',
    'openVisits': <Object?>[],
  },
  'visitsThisMonth': 6,
  'lastVisit': visitJson(),
};

http.Response envelope(Object? data, {int status = 200}) => http.Response(
  jsonEncode({'success': true, 'data': data}),
  status,
  headers: {'content-type': 'application/json'},
);

void main() {
  test('status request sends demo headers and parses nested data', () async {
    late http.Request captured;
    final client = KinevorCheckInsClient(
      baseUrl: Uri.parse('http://localhost:5086/api/v1/'),
      headers: const {'X-Demo-Role': 'Member', 'X-Demo-Member': 'member-1'},
      httpClient: MockClient((request) async {
        captured = request;
        return envelope(statusJson());
      }),
    );

    final status = await client.fetchStatus();

    expect(captured.method, 'GET');
    expect(
      captured.url.toString(),
      'http://localhost:5086/api/v1/me/checkins/status',
    );
    expect(captured.headers['X-Demo-Role'], 'Member');
    expect(captured.headers['X-Demo-Member'], 'member-1');
    expect(status.fullName, 'Maya Thompson');
    expect(status.initials, 'MT');
    expect(status.eligibility.allowed, isTrue);
    expect(status.occupancy?.occupied, 12);
    expect(status.occupancy?.roundedUtilization, 24);
    expect(status.visitsThisMonth, 6);
    expect(status.lastVisit?.branchName, 'Kinevor Shoreditch');
  });

  test('visit history sends paging and date filters', () async {
    late http.Request captured;
    final client = KinevorCheckInsClient(
      baseUrl: Uri.parse('http://localhost:5086/api/v1'),
      httpClient: MockClient((request) async {
        captured = request;
        return envelope({
          'items': [visitJson(), visitJson(id: 'visit-2', isOpen: true)],
          'totalCount': 12,
          'page': 2,
          'pageSize': 5,
          'totalPages': 3,
          'hasNext': true,
          'hasPrev': true,
        });
      }),
    );

    final page = await client.fetchVisits(
      page: 2,
      pageSize: 5,
      from: DateTime.utc(2026, 10, 1),
      to: DateTime.utc(2026, 10, 21, 12),
    );

    expect(captured.method, 'GET');
    expect(captured.url.path, '/api/v1/me/checkins');
    expect(captured.url.queryParameters['page'], '2');
    expect(captured.url.queryParameters['pageSize'], '5');
    expect(captured.url.queryParameters['from'], '2026-10-01T00:00:00.000Z');
    expect(page.items, hasLength(2));
    expect(page.items.last.isOpen, isTrue);
    expect(page.totalCount, 12);
    expect(page.hasNext, isTrue);
  });

  test('check-in and check-out use POST and parse the visit', () async {
    final requests = <http.Request>[];
    final client = KinevorCheckInsClient(
      baseUrl: Uri.parse('http://localhost:5086/api/v1'),
      httpClient: MockClient((request) async {
        requests.add(request);
        return envelope(
          visitJson(
            id: request.url.path.endsWith('/checkout')
                ? 'visit-closed'
                : 'visit-open',
            isOpen: !request.url.path.endsWith('/checkout'),
          ),
        );
      }),
    );

    final checkedIn = await client.checkIn(notes: 'Evening session');
    final checkedOut = await client.checkOut();

    expect(requests[0].method, 'POST');
    expect(requests[0].url.path, '/api/v1/me/checkins');
    expect(jsonDecode(requests[0].body), {'notes': 'Evening session'});
    expect(requests[1].method, 'POST');
    expect(requests[1].url.path, '/api/v1/me/checkins/checkout');
    expect(checkedIn.isOpen, isTrue);
    expect(checkedOut.id, 'visit-closed');
    expect(checkedOut.isOpen, isFalse);
  });

  test('API errors surface their message and status', () async {
    final client = KinevorCheckInsClient(
      baseUrl: Uri.parse('http://localhost:5086/api/v1'),
      httpClient: MockClient(
        (_) async => http.Response(
          jsonEncode({
            'success': false,
            'message': 'You already have an open visit.',
          }),
          409,
        ),
      ),
    );

    await expectLater(
      client.checkIn(),
      throwsA(
        isA<CheckInsApiException>()
            .having((error) => error.statusCode, 'status', 409)
            .having(
              (error) => error.message,
              'message',
              'You already have an open visit.',
            ),
      ),
    );
  });

  test('validation errors use the first detail message', () async {
    final client = KinevorCheckInsClient(
      baseUrl: Uri.parse('http://localhost:5086/api/v1'),
      httpClient: MockClient(
        (_) async => http.Response(
          jsonEncode({
            'success': false,
            'errors': [
              {'message': 'Notes must be 280 characters or fewer.'},
            ],
          }),
          400,
        ),
      ),
    );

    await expectLater(
      client.checkIn(),
      throwsA(
        isA<CheckInsApiException>().having(
          (error) => error.message,
          'message',
          'Notes must be 280 characters or fewer.',
        ),
      ),
    );
  });

  test('network and malformed payload failures are wrapped', () async {
    final offline = KinevorCheckInsClient(
      baseUrl: Uri.parse('http://localhost:5086/api/v1'),
      httpClient: MockClient((_) async => throw Exception('offline')),
    );
    final malformed = KinevorCheckInsClient(
      baseUrl: Uri.parse('http://localhost:5086/api/v1'),
      httpClient: MockClient((_) async => http.Response('<html>', 502)),
    );

    await expectLater(
      offline.fetchStatus(),
      throwsA(isA<CheckInsApiException>()),
    );
    await expectLater(
      malformed.fetchStatus(),
      throwsA(isA<CheckInsApiException>()),
    );
  });
}
