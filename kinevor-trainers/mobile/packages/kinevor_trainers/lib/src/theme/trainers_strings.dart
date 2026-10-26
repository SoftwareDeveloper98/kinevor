/// User-facing labels used by the trainers widgets.
///
/// Extend or instantiate with localized values to translate the module
/// without forking the widgets.
class TrainersStrings {
  /// Creates a label bundle.
  const TrainersStrings({
    this.title = 'Trainers',
    this.searchHint = 'Search trainer',
    this.empty = 'No trainers found',
    this.error = 'Could not load trainers',
    this.retry = 'Try again',
    this.about = 'About',
    this.specializations = 'Specializations',
    this.certifications = 'Certifications',
    this.contact = 'Contact',
    this.statsExperience = 'Years experience',
    this.statsRating = 'Avg rating',
    this.statsReviews = 'Reviews',
    this.notFound = 'Trainer not found',
    this.back = 'Back',
  });

  /// App-bar and section title.
  final String title;

  /// Search field hint.
  final String searchHint;

  /// Empty-directory message.
  final String empty;

  /// Load-failure message.
  final String error;

  /// Retry action label.
  final String retry;

  /// About card title.
  final String about;

  /// Specializations section title.
  final String specializations;

  /// Certifications section title.
  final String certifications;

  /// Contact section title.
  final String contact;

  /// Years-of-experience stat label.
  final String statsExperience;

  /// Rating stat label.
  final String statsRating;

  /// Reviews stat label.
  final String statsReviews;

  /// Missing-profile message.
  final String notFound;

  /// Back navigation label.
  final String back;

  /// Formats the experience line under a trainer name.
  String yearsExperience(int years) => '$years years experience';

  /// Formats the review count under a rating.
  String reviews(int count) => '$count reviews';

  /// Formats the accessible rating label.
  String rating(double rating) =>
      'Rating: ${rating.toStringAsFixed(1)} out of 5';

  /// Capitalizes a status value for display.
  String status(String status) {
    if (status.isEmpty) return '';
    return '${status[0].toUpperCase()}${status.substring(1).toLowerCase()}';
  }
}
