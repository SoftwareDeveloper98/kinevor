import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:kinevor_identity_access/kinevor_identity_access.dart';

void main() {
  test('login posts credentials and parses the session envelope', () async {
    late http.Request captured;
    final client = KinevorIdentityClient(
      baseUrl: Uri.parse('http://localhost:5082/api/v1/'),
      httpClient: MockClient((request) async {
        captured = request;
        return http.Response(
          jsonEncode({
            'success': true,
            'data': {
              'accessToken': 'access-1',
              'refreshToken': 'refresh-1',
              'expiresAt': '2026-10-20T18:45:00Z',
              'user': {
                'id': 'account-1',
                'email': 'admin@kinevor.demo',
                'displayName': 'Amara Obi',
                'role': 'Admin',
                'mfaEnabled': true,
              },
            },
          }),
          200,
          headers: {'content-type': 'application/json'},
        );
      }),
    );

    final session = await client.login(
      email: 'admin@kinevor.demo',
      password: 'Kinevor!2026',
    );

    expect(captured.method, 'POST');
    expect(captured.url.toString(), 'http://localhost:5082/api/v1/auth/login');
    expect(jsonDecode(captured.body), {
      'email': 'admin@kinevor.demo',
      'password': 'Kinevor!2026',
    });
    expect(session.accessToken, 'access-1');
    expect(session.user.displayName, 'Amara Obi');
    expect(session.user.role, 'Admin');
    expect(session.user.initials, 'AO');
  });

  test('session sends the bearer token', () async {
    late http.Request captured;
    final client = KinevorIdentityClient(
      baseUrl: Uri.parse('http://localhost:5082/api/v1'),
      httpClient: MockClient((request) async {
        captured = request;
        return http.Response(
          jsonEncode({
            'success': true,
            'data': {
              'id': 'account-1',
              'email': 'member@kinevor.demo',
              'displayName': 'Noah Mensah',
              'role': 'Member',
              'mfaEnabled': false,
            },
          }),
          200,
        );
      }),
    );

    final account = await client.session('access-token');

    expect(captured.url.path, '/api/v1/auth/session');
    expect(captured.headers['Authorization'], 'Bearer access-token');
    expect(account.email, 'member@kinevor.demo');
    expect(account.mfaEnabled, isFalse);
  });

  test('failed login surfaces the API message', () async {
    final client = KinevorIdentityClient(
      baseUrl: Uri.parse('http://localhost:5082/api/v1'),
      httpClient: MockClient(
        (_) async => http.Response(
          jsonEncode({
            'success': false,
            'message': 'Invalid e-mail or password.',
          }),
          401,
        ),
      ),
    );

    await expectLater(
      client.login(email: 'nobody@kinevor.demo', password: 'wrong'),
      throwsA(
        isA<IdentityApiException>()
            .having((error) => error.statusCode, 'status', 401)
            .having(
              (error) => error.message,
              'message',
              'Invalid e-mail or password.',
            ),
      ),
    );
  });

  test('network failures are wrapped', () async {
    final client = KinevorIdentityClient(
      baseUrl: Uri.parse('http://localhost:5082/api/v1'),
      httpClient: MockClient((_) async => throw Exception('offline')),
    );

    await expectLater(
      client.login(email: 'admin@kinevor.demo', password: 'Kinevor!2026'),
      throwsA(isA<IdentityApiException>()),
    );
  });
}
