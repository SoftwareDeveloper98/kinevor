/// Failure reported by the Kinevor identity API.
class IdentityApiException implements Exception {
  /// Creates an exception with a message and optional HTTP status.
  const IdentityApiException(this.message, {this.statusCode});

  /// Human-readable failure.
  final String message;

  /// HTTP status code, when a response was received.
  final int? statusCode;

  @override
  String toString() => 'IdentityApiException($statusCode): $message';
}
