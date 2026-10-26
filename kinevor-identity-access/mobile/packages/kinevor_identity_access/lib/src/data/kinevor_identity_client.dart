import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/user_account.dart';
import 'identity_api_exception.dart';

/// HTTP client for the Kinevor identity and access API.
///
/// Supply [httpClient] to plug in retries, tracing or platform-specific
/// transports. The client does not persist tokens; hosts decide where the
/// session lives.
class KinevorIdentityClient {
  /// Creates a client rooted at [baseUrl], for example
  /// `https://api.example.com/api/v1`.
  KinevorIdentityClient({
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

  /// Exchanges credentials for a token pair.
  Future<AuthSession> login({
    required String email,
    required String password,
  }) async {
    final data = await _send(
      'POST',
      'auth/login',
      body: {'email': email, 'password': password},
    );
    return AuthSession.fromJson(data);
  }

  /// Loads the account for [accessToken].
  Future<UserAccount> session(String accessToken) async {
    final data = await _send('GET', 'auth/session', accessToken: accessToken);
    return UserAccount.fromJson(data);
  }

  /// Rotates [refreshToken] and returns a new pair.
  Future<AuthSession> refresh(String refreshToken) async {
    final data = await _send(
      'POST',
      'auth/refresh',
      body: {'refreshToken': refreshToken},
    );
    return AuthSession.fromJson(data);
  }

  /// Revokes [refreshToken].
  Future<void> logout(String refreshToken) async {
    await _send('POST', 'auth/logout', body: {'refreshToken': refreshToken});
  }

  /// Closes the underlying HTTP client when this instance created it.
  void close() {
    if (_ownsClient) _http.close();
  }

  Uri _endpoint(String path) {
    final base = _baseUrl.toString().replaceFirst(RegExp(r'/+$'), '');
    return Uri.parse('$base/$path');
  }

  Future<Map<String, dynamic>> _send(
    String method,
    String path, {
    Map<String, dynamic>? body,
    String? accessToken,
  }) async {
    final uri = _endpoint(path);
    final request = http.Request(method, uri)
      ..headers.addAll({
        'Accept': 'application/json',
        if (body != null) 'Content-Type': 'application/json',
        if (accessToken != null) 'Authorization': 'Bearer $accessToken',
        ..._headers,
      });
    if (body != null) request.body = jsonEncode(body);

    final http.StreamedResponse streamed;
    try {
      streamed = await _http.send(request);
    } on Exception catch (error) {
      throw IdentityApiException('Could not reach the identity API: $error');
    }

    final response = await http.Response.fromStream(streamed);
    final Object? decoded;
    try {
      decoded = response.body.isEmpty ? null : jsonDecode(response.body);
    } on FormatException {
      throw IdentityApiException(
        'The identity API returned invalid JSON.',
        statusCode: response.statusCode,
      );
    }

    if (decoded is! Map<String, dynamic>) {
      throw IdentityApiException(
        'The identity API returned an unexpected payload.',
        statusCode: response.statusCode,
      );
    }

    final success = decoded['success'] != false;
    if (response.statusCode < 200 || response.statusCode >= 300 || !success) {
      throw IdentityApiException(
        _errorMessage(decoded) ?? 'The identity request failed.',
        statusCode: response.statusCode,
      );
    }

    final data = decoded['data'];
    if (data is Map<String, dynamic>) return data;
    if (data is Map) return Map<String, dynamic>.from(data);
    return const {};
  }

  String? _errorMessage(Map<String, dynamic> body) {
    final message = body['message'];
    if (message is String && message.isNotEmpty) return message;
    final errors = body['errors'];
    if (errors is List) {
      for (final error in errors) {
        if (error is String && error.isNotEmpty) return error;
      }
    }
    return null;
  }
}
