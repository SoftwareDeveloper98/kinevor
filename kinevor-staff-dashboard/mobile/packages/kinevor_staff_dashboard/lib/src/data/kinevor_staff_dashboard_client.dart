import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/staff_dashboard_models.dart';
import 'staff_dashboard_api_exception.dart';

/// HTTP client for the Kinevor staff dashboard API.
///
/// Extra [headers] are attached to every request. The demo uses them for the
/// development-only `X-Demo-Role`, `X-Demo-Email` and `X-Demo-User` trust
/// headers, while a production host can supply an authenticated client instead.
class KinevorStaffDashboardClient {
  /// Creates a client rooted at [baseUrl], for example
  /// `https://api.example.com/api/v1`.
  KinevorStaffDashboardClient({
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

  /// Loads the dashboard for a branch and trend range.
  Future<StaffDashboard> fetchDashboard({
    String? branchId,
    int days = 7,
  }) async {
    final data = await _object(
      'GET',
      'staff-dashboard',
      query: {
        'days': '$days',
        if (branchId != null && branchId.isNotEmpty) 'branchId': branchId,
      },
    );
    return StaffDashboard.fromJson(data);
  }

  /// Loads the complete operational alert inbox.
  Future<DashboardAlerts> fetchAlerts({String? branchId}) async {
    final data = await _object(
      'GET',
      'staff-dashboard/alerts',
      query: {
        if (branchId != null && branchId.isNotEmpty) 'branchId': branchId,
      },
    );
    return DashboardAlerts.fromJson(data);
  }

  /// Loads the branches available in the dashboard selector.
  Future<List<DashboardBranch>> fetchBranches() async {
    final data = await _list('GET', 'staff-dashboard/branches');
    return [
      for (final item in data)
        if (item is Map)
          DashboardBranch.fromJson(Map<String, dynamic>.from(item)),
    ];
  }

  /// Closes the underlying HTTP client when this instance created it.
  void close() {
    if (_ownsClient) _http.close();
  }

  Uri _endpoint(String path, {Map<String, String>? query}) {
    final base = _baseUrl.toString().replaceFirst(RegExp(r'/+$'), '');
    final uri = Uri.parse('$base/$path');
    if (query == null || query.isEmpty) return uri;
    return uri.replace(queryParameters: query);
  }

  Future<Map<String, dynamic>> _object(
    String method,
    String path, {
    Map<String, String>? query,
  }) async {
    final data = await _send(method, path, query: query);
    if (data is Map<String, dynamic>) return data;
    if (data is Map) return Map<String, dynamic>.from(data);
    throw const StaffDashboardApiException(
      'The staff dashboard response has no data object.',
    );
  }

  Future<List<Object?>> _list(
    String method,
    String path, {
    Map<String, String>? query,
  }) async {
    final data = await _send(method, path, query: query);
    if (data is List) return data;
    throw const StaffDashboardApiException(
      'The staff dashboard response has no data array.',
    );
  }

  Future<Object?> _send(
    String method,
    String path, {
    Map<String, String>? query,
  }) async {
    final request = http.Request(method, _endpoint(path, query: query))
      ..headers.addAll({'Accept': 'application/json', ..._headers});

    final http.StreamedResponse streamed;
    try {
      streamed = await _http.send(request);
    } on Exception catch (error) {
      throw StaffDashboardApiException(
        'Could not reach the staff dashboard API: $error',
      );
    }

    final response = await http.Response.fromStream(streamed);
    final Object? decoded;
    try {
      decoded = response.body.isEmpty ? null : jsonDecode(response.body);
    } on FormatException {
      throw StaffDashboardApiException(
        'The staff dashboard API returned invalid JSON.',
        statusCode: response.statusCode,
      );
    }

    if (decoded is! Map<String, dynamic>) {
      throw StaffDashboardApiException(
        'The staff dashboard API returned an unexpected payload.',
        statusCode: response.statusCode,
      );
    }

    final success = decoded['success'] != false;
    if (response.statusCode < 200 || response.statusCode >= 300 || !success) {
      throw StaffDashboardApiException(
        _errorMessage(decoded) ?? 'The staff dashboard request failed.',
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
