import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/trainer.dart';
import 'trainers_api_exception.dart';

/// HTTP client for the Kinevor trainers API.
///
/// The client accepts any [http.Client], so hosts can supply their own
/// authenticated client and interceptors. Extra [headers] are attached to
/// every request, which is how the demo sends its `X-Demo-Role` header.
class KinevorTrainersClient {
  /// Creates a client rooted at [baseUrl], for example
  /// `https://api.example.com/api/v1`.
  KinevorTrainersClient({
    required Uri baseUrl,
    http.Client? httpClient,
    Map<String, String> headers = const {},
  }) : _baseUrl = baseUrl,
       _http = httpClient ?? http.Client(),
       _ownsClient = httpClient == null,
       _headers = Map.unmodifiable(headers);

  final Uri _baseUrl;
  final http.Client _http;
  final bool _ownsClient;
  final Map<String, String> _headers;

  /// Loads one page of trainers.
  ///
  /// [search] is matched by the server against name and e-mail.
  Future<TrainersPage> fetchTrainers({
    int page = 1,
    int pageSize = 20,
    String? search,
  }) async {
    final uri = _endpoint('trainers').replace(
      queryParameters: {
        'page': '$page',
        'pageSize': '$pageSize',
        if (search != null && search.isNotEmpty) 'search': search,
      },
    );
    final data = await _get(uri);
    final items = data['items'];
    if (items is! List) {
      throw const TrainersApiException('Trainers response has no items array.');
    }
    return TrainersPage(
      items: [
        for (final item in items)
          if (item is Map<String, dynamic>)
            Trainer.fromJson(item)
          else if (item is Map)
            Trainer.fromJson(Map<String, dynamic>.from(item)),
      ],
      totalCount: (data['totalCount'] as num?)?.toInt() ?? items.length,
      page: (data['page'] as num?)?.toInt() ?? page,
      pageSize: (data['pageSize'] as num?)?.toInt() ?? pageSize,
    );
  }

  /// Loads a single trainer profile.
  ///
  /// Throws [TrainersApiException] when the trainer does not exist.
  Future<Trainer> fetchTrainer(String id) async {
    final data = await _get(_endpoint('trainers/${Uri.encodeComponent(id)}'));
    return Trainer.fromJson(data);
  }

  /// Closes the underlying HTTP client when this instance created it.
  void close() {
    if (_ownsClient) _http.close();
  }

  Uri _endpoint(String path) {
    final base = _baseUrl.toString().replaceFirst(RegExp(r'/+$'), '');
    return Uri.parse('$base/$path');
  }

  Future<Map<String, dynamic>> _get(Uri uri) async {
    final http.Response response;
    try {
      response = await _http.get(
        uri,
        headers: {'Accept': 'application/json', ..._headers},
      );
    } on Exception catch (error) {
      throw TrainersApiException('Could not reach the trainers API: $error');
    }

    final Object? decoded;
    try {
      decoded = response.body.isEmpty ? null : jsonDecode(response.body);
    } on FormatException {
      throw TrainersApiException(
        'The trainers API returned invalid JSON.',
        statusCode: response.statusCode,
      );
    }

    if (decoded is! Map<String, dynamic>) {
      throw TrainersApiException(
        'The trainers API returned an unexpected payload.',
        statusCode: response.statusCode,
      );
    }

    final success = decoded['success'] != false;
    if (response.statusCode < 200 || response.statusCode >= 300 || !success) {
      throw TrainersApiException(
        _errorMessage(decoded) ?? 'The trainers request failed.',
        statusCode: response.statusCode,
      );
    }

    final data = decoded['data'];
    if (data is Map<String, dynamic>) return data;
    if (data is Map) return Map<String, dynamic>.from(data);
    throw TrainersApiException(
      'The trainers response has no data object.',
      statusCode: response.statusCode,
    );
  }

  String? _errorMessage(Map<String, dynamic> body) {
    final message = body['message'];
    if (message is String && message.isNotEmpty) return message;
    final errors = body['errors'];
    if (errors is List) {
      for (final error in errors) {
        if (error is String && error.isNotEmpty) return error;
        if (error is Map && error['message'] is String) {
          return error['message'] as String;
        }
      }
    }
    final detail = body['detail'];
    return detail is String && detail.isNotEmpty ? detail : null;
  }
}
