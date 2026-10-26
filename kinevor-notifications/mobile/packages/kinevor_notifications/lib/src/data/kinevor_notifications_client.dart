import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/notification_models.dart';
import 'notifications_api_exception.dart';

/// HTTP client for the Kinevor member notification API.
///
/// Extra [headers] are attached to every request. The demo uses them for the
/// development-only `X-Demo-Role` and `X-Demo-Member` trust headers, while a
/// production host can supply an authenticated client instead.
class KinevorNotificationsClient {
  /// Creates a client rooted at [baseUrl], for example
  /// `https://api.example.com/api/v1`.
  KinevorNotificationsClient({
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

  /// Loads a page of the authenticated member's notifications.
  Future<NotificationPage> fetchNotifications({
    String? category,
    String? channel,
    bool unreadOnly = false,
    int page = 1,
    int pageSize = 50,
  }) async {
    final query = <String, String>{
      if (category != null && category.isNotEmpty) 'category': category,
      if (channel != null && channel.isNotEmpty) 'channel': channel,
      if (unreadOnly) 'unreadOnly': 'true',
      'page': '$page',
      'pageSize': '$pageSize',
    };
    final data = await _object('GET', 'me/notifications', query: query);
    return NotificationPage.fromJson(data);
  }

  /// Loads the authenticated member's unread notification count.
  Future<int> fetchUnreadCount() async {
    final data = await _object('GET', 'me/notifications/unread-count');
    return (data['unread'] as num?)?.toInt() ?? 0;
  }

  /// Loads one notification belonging to the authenticated member.
  Future<NotificationRecord> fetchNotification(String id) async {
    final data = await _object(
      'GET',
      'me/notifications/${Uri.encodeComponent(id)}',
    );
    return NotificationRecord.fromJson(data);
  }

  /// Marks one notification as read.
  Future<void> markRead(String id) async {
    await _send('POST', 'me/notifications/${Uri.encodeComponent(id)}/read');
  }

  /// Marks every delivered notification as read and returns how many changed.
  Future<int> markAllRead() async {
    final data = await _object('POST', 'me/notifications/read-all');
    return (data['marked'] as num?)?.toInt() ?? 0;
  }

  /// Loads the channel preferences for every category.
  Future<List<NotificationPreference>> fetchPreferences() async {
    final data = await _list('GET', 'me/notification-preferences');
    return [
      for (final item in data)
        NotificationPreference.fromJson(Map<String, dynamic>.from(item as Map)),
    ];
  }

  /// Updates the three channel flags for one category.
  Future<List<NotificationPreference>> updatePreference({
    required String category,
    required bool email,
    required bool push,
    required bool inApp,
  }) async {
    final data = await _list(
      'PATCH',
      'me/notification-preferences/${Uri.encodeComponent(category)}',
      body: {'email': email, 'push': push, 'inApp': inApp},
    );
    return [
      for (final item in data)
        NotificationPreference.fromJson(Map<String, dynamic>.from(item as Map)),
    ];
  }

  /// Loads registered push devices.
  Future<List<NotificationDevice>> fetchDevices() async {
    final data = await _list('GET', 'me/devices');
    return [
      for (final item in data)
        NotificationDevice.fromJson(Map<String, dynamic>.from(item as Map)),
    ];
  }

  /// Registers or re-assigns a push device token.
  Future<NotificationDevice> registerDevice({
    required String token,
    required String platform,
  }) async {
    final data = await _object(
      'POST',
      'me/devices',
      body: {'token': token, 'platform': platform},
    );
    return NotificationDevice.fromJson(data);
  }

  /// Unregisters one of the authenticated member's devices.
  Future<void> unregisterDevice(String id) async {
    await _send('DELETE', 'me/devices/${Uri.encodeComponent(id)}');
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
    throw const NotificationsApiException(
      'The notifications response has no data object.',
    );
  }

  Future<List<Object?>> _list(
    String method,
    String path, {
    Map<String, dynamic>? body,
  }) async {
    final data = await _send(method, path, body: body);
    if (data is List) return data;
    throw const NotificationsApiException(
      'The notifications response has no data array.',
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
      throw NotificationsApiException(
        'Could not reach the notifications API: $error',
      );
    }

    final response = await http.Response.fromStream(streamed);
    final Object? decoded;
    try {
      decoded = response.body.isEmpty ? null : jsonDecode(response.body);
    } on FormatException {
      throw NotificationsApiException(
        'The notifications API returned invalid JSON.',
        statusCode: response.statusCode,
      );
    }

    if (decoded is! Map<String, dynamic>) {
      throw NotificationsApiException(
        'The notifications API returned an unexpected payload.',
        statusCode: response.statusCode,
      );
    }

    final success = decoded['success'] != false;
    if (response.statusCode < 200 || response.statusCode >= 300 || !success) {
      throw NotificationsApiException(
        _errorMessage(decoded) ?? 'The notifications request failed.',
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
