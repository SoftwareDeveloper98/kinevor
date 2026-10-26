/// Failure reported by the Kinevor notifications API.
class NotificationsApiException implements Exception {
  /// Creates an exception with a message and optional HTTP status.
  const NotificationsApiException(this.message, {this.statusCode});

  /// Human-readable failure.
  final String message;

  /// HTTP status code, when a response was received.
  final int? statusCode;

  @override
  String toString() => 'NotificationsApiException($statusCode): $message';
}
