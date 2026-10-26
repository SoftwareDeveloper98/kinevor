import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/check_in_models.dart';
import 'check_ins_api_exception.dart';

/// HTTP client for the Kinevor member check-in API.
///
/// Extra [headers] are attached to every request. The demo uses them for the
/// development-only `X-Demo-Role` and `X-Demo-Member` trust headers, while a
/// production host can supply an authenticated client instead.
class KinevorCheckInsClient {
  /// Creates a client rooted at [baseUrl], for example
  /// `https://api.example.com/api/v1`.
  KinevorCheckInsClient({
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

  /// Loads eligibility, live occupancy and the member's current visit.
  Future<MemberCheckInStatus> fetchStatus() async {
    final data = await _object('GET', 'me/checkins/status');
    return MemberCheckInStatus.fromJson(data);
  }

  /// Loads the member's own visit history, newest first.
  Future<VisitPage> fetchVisits({
    int page = 1,
    int pageSize = 20,
    DateTime? from,
    DateTime? to,
  }) async {
    final data = await _object(
      'GET',
      'me/checkins',
      query: {
        'page': '$page',
        'pageSize': '$pageSize',
        if (from != null) 'from': from.toUtc().toIso8601String(),
        if (to != null) 'to': to.toUtc().toIso8601String(),
      },
    );
    return VisitPage.fromJson(data);
  }

  /// Checks the member in at their home club.
  Future<CheckInVisit> checkIn({String? notes}) async {
    final data = await _object(
      'POST',
      'me/checkins',
      body: {if (notes != null && notes.isNotEmpty) 'notes': notes},
    );
    return CheckInVisit.fromJson(data);
  }

  /// Checks the member out of their current visit.
  Future<CheckInVisit> checkOut() async {
    final data = await _object('POST', 'me/checkins/checkout');
    return CheckInVisit.fromJson(data);
  }

  /// Closes the underlying HTTP client when this instance created it.
  void close() {
    if (_ownsClient) _http.close();
  }

  Uri _endpoint(String path, {Map<String, String>? query}) {
    final base = _baseUrl.toString().replaceFirst(RegExp(r'/+$'), '');
    final uri = Uri.parse('$base/$path');
    return query == null || query.isEmpty
        ? uri
        : uri.replace(queryParameters: query);
  }

  Future<Map<String, dynamic>> _object(
    String method,
    String path, {
    Map<String, dynamic>? body,
    Map<String, String>? query,
  }) async {
    final data = await _send(method, path, body: body, query: query);
    if (data is Map<String, dynamic>) return data;
    if (data is Map) return Map<String, dynamic>.from(data);
    throw const CheckInsApiException(
      'The check-ins response has no data object.',
    );
  }

  Future<Object?> _send(
    String method,
    String path, {
    Map<String, dynamic>? body,
    Map<String, String>? query,
  }) async {
    final request = http.Request(method, _endpoint(path, query: query))
      ..headers.addAll({
        'Accept': 'application/json',
        if (body != null) 'Content-Type': 'application/json',
        ..._headers,
      });
    if (body != null) request.body = jsonEncode(body);

    final http.StreamedResponse streamed;
    try {
      streamed = await _http.send(request);
    } on Exception catch (error) {
      throw CheckInsApiException('Could not reach the check-ins API: $error');
    }

    final response = await http.Response.fromStream(streamed);
    final Object? decoded;
    try {
      decoded = response.body.isEmpty ? null : jsonDecode(response.body);
    } on FormatException {
      throw CheckInsApiException(
        'The check-ins API returned invalid JSON.',
        statusCode: response.statusCode,
      );
    }

    if (decoded is! Map<String, dynamic>) {
      throw CheckInsApiException(
        'The check-ins API returned an unexpected payload.',
        statusCode: response.statusCode,
      );
    }

    final success = decoded['success'] != false;
    if (response.statusCode < 200 || response.statusCode >= 300 || !success) {
      throw CheckInsApiException(
        _errorMessage(decoded) ?? 'The check-ins request failed.',
        statusCode: response.statusCode,
      );
    }

    return decoded['data'];
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
