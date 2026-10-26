/// Failure reported by the Kinevor staff dashboard API.
class StaffDashboardApiException implements Exception {
  /// Creates an exception with a message and optional HTTP status.
  const StaffDashboardApiException(this.message, {this.statusCode});

  /// Human-readable failure.
  final String message;

  /// HTTP status code, when a response was received.
  final int? statusCode;

  @override
  String toString() => 'StaffDashboardApiException($statusCode): $message';
}
