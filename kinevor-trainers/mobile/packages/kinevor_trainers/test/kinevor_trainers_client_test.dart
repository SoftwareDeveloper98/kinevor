import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:kinevor_trainers/kinevor_trainers.dart';

void main() {
  group('KinevorTrainersClient', () {
    test('requests a page with query and demo headers', () async {
      late http.Request captured;
      final client = KinevorTrainersClient(
        baseUrl: Uri.parse('http://localhost:5081/api/v1/'),
        headers: const {'X-Demo-Role': 'Staff'},
        httpClient: MockClient((request) async {
          captured = request;
          return http.Response(
            jsonEncode({
              'success': true,
              'data': {
                'items': [
                  {
                    'id': 't1',
                    'fullName': 'Nour Adel',
                    'email': 'nour.adel@kinevor.demo',
                    'phone': '+201003456789',
                    'bio': 'Coach',
                    'specializations': ['Spinning', 'Zumba'],
                    'rating': 4.9,
                    'totalRatings': 143,
                    'yearsExperience': 5,
                    'status': 'Active',
                    'certifications': ['Spinning Instructor'],
                  },
                ],
                'totalCount': 1,
                'page': 2,
                'pageSize': 10,
              },
            }),
            200,
            headers: {'content-type': 'application/json'},
          );
        }),
      );

      final page = await client.fetchTrainers(
        page: 2,
        pageSize: 10,
        search: 'nour',
      );

      expect(captured.url.path, '/api/v1/trainers');
      expect(captured.url.queryParameters['page'], '2');
      expect(captured.url.queryParameters['pageSize'], '10');
      expect(captured.url.queryParameters['search'], 'nour');
      expect(captured.headers['X-Demo-Role'], 'Staff');
      expect(page.totalCount, 1);
      expect(page.page, 2);
      expect(page.items.single.fullName, 'Nour Adel');
      expect(page.items.single.certifications, ['Spinning Instructor']);
      client.close();
    });

    test('parses a single trainer and encodes the id', () async {
      late Uri captured;
      final client = KinevorTrainersClient(
        baseUrl: Uri.parse('http://localhost:5081/api/v1'),
        httpClient: MockClient((request) async {
          captured = request.url;
          return http.Response(
            jsonEncode({
              'success': true,
              'data': {
                'id': 'a b',
                'fullName': 'Layla Hassan',
                'email': 'layla@example.com',
                'phone': '+201001234567',
                'bio': 'Yoga',
                'specializations': ['Yoga'],
                'rating': 4.8,
                'totalRatings': 126,
                'yearsExperience': 8,
                'status': 'Active',
              },
            }),
            200,
          );
        }),
      );

      final trainer = await client.fetchTrainer('a b');

      expect(captured.path, '/api/v1/trainers/a%20b');
      expect(trainer.fullName, 'Layla Hassan');
      expect(trainer.photoUrl, isNull);
      client.close();
    });

    test('throws a domain exception with the server message', () async {
      final client = KinevorTrainersClient(
        baseUrl: Uri.parse('http://localhost:5081/api/v1'),
        httpClient: MockClient(
          (_) async => http.Response(
            jsonEncode({'success': false, 'message': 'Trainer not found'}),
            404,
          ),
        ),
      );

      await expectLater(
        client.fetchTrainer('missing'),
        throwsA(
          isA<TrainersApiException>()
              .having((e) => e.statusCode, 'statusCode', 404)
              .having((e) => e.message, 'message', 'Trainer not found'),
        ),
      );
      client.close();
    });

    test('rejects malformed JSON', () async {
      final client = KinevorTrainersClient(
        baseUrl: Uri.parse('http://localhost:5081/api/v1'),
        httpClient: MockClient((_) async => http.Response('<html>', 200)),
      );

      await expectLater(
        client.fetchTrainers(),
        throwsA(isA<TrainersApiException>()),
      );
      client.close();
    });
  });

  group('Trainer.fromJson', () {
    test('applies defensive defaults', () {
      final trainer = Trainer.fromJson({'fullName': 'Solo'});
      expect(trainer.fullName, 'Solo');
      expect(trainer.specializations, isEmpty);
      expect(trainer.certifications, isEmpty);
      expect(trainer.rating, 0);
      expect(trainer.initials, 'S');
    });
  });
}
