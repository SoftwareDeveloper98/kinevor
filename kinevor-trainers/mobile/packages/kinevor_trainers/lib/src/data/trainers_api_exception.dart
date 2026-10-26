/// Error raised when the trainers API is unreachable or rejects a request.
class TrainersApiException implements Exception {
  /// Creates an API exception.
  const TrainersApiException(this.message, {this.statusCode});

  /// Human-readable error description.
  final String message;

  /// HTTP status code, when the failure came from a response.
  final int? statusCode;

  @override
  String toString() => 'TrainersApiException($statusCode): $message';
}
