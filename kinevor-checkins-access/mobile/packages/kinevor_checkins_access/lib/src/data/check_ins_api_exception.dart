/// Error returned by the Kinevor check-ins API or its transport.
class CheckInsApiException implements Exception {
  /// Creates an API error with an optional HTTP [statusCode].
  const CheckInsApiException(this.message, {this.statusCode});

  /// Human-readable error message.
  final String message;

  /// HTTP status code, when a response was received.
  final int? statusCode;

  @override
  String toString() => message;
}
