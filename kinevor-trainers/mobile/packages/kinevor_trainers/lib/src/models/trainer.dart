/// Trainer employed by the gym.
class Trainer {
  /// Creates a trainer profile.
  const Trainer({
    required this.id,
    required this.fullName,
    required this.email,
    required this.phone,
    required this.bio,
    required this.specializations,
    required this.rating,
    required this.totalRatings,
    required this.yearsExperience,
    required this.status,
    this.photoUrl,
    this.certifications = const [],
  });

  /// Stable identifier of the trainer.
  final String id;

  /// Full display name.
  final String fullName;

  /// Email address.
  final String email;

  /// Phone number in international format.
  final String phone;

  /// Short biographical text shown on profile cards.
  final String bio;

  /// Specialization labels, for example `Yoga` or `HIIT`.
  final List<String> specializations;

  /// Average class rating from 1 to 5.
  final double rating;

  /// Number of ratings contributing to the average.
  final int totalRatings;

  /// Years of coaching experience.
  final int yearsExperience;

  /// Employment status, for example `Active`.
  final String status;

  /// Absolute or relative URL of the profile photo, when any.
  final String? photoUrl;

  /// Certification names displayed on the profile.
  final List<String> certifications;

  /// First letters of the first two name parts, used when no photo exists.
  String get initials {
    final parts = fullName
        .trim()
        .split(RegExp(r'\s+'))
        .where((p) => p.isNotEmpty);
    if (parts.isEmpty) return '?';
    final letters = parts.take(2).map((p) => p[0].toUpperCase()).join();
    return letters;
  }

  /// Parses a trainer JSON object defensively so partial payloads never
  /// break list rendering.
  factory Trainer.fromJson(Map<String, dynamic> json) {
    final rawSpecializations = json['specializations'];
    final rawCertifications = json['certifications'];
    return Trainer(
      id: json['id']?.toString() ?? '',
      fullName: json['fullName']?.toString() ?? '',
      email: json['email']?.toString() ?? '',
      phone: json['phone']?.toString() ?? '',
      bio: json['bio']?.toString() ?? '',
      specializations: rawSpecializations is List
          ? [for (final item in rawSpecializations) item.toString()]
          : const [],
      rating: (json['rating'] as num?)?.toDouble() ?? 0,
      totalRatings: (json['totalRatings'] as num?)?.toInt() ?? 0,
      yearsExperience: (json['yearsExperience'] as num?)?.toInt() ?? 0,
      status: json['status']?.toString() ?? '',
      photoUrl: json['photoUrl']?.toString(),
      certifications: rawCertifications is List
          ? [for (final item in rawCertifications) item.toString()]
          : const [],
    );
  }
}

/// One page of the trainer directory.
class TrainersPage {
  /// Creates a directory page.
  const TrainersPage({
    required this.items,
    required this.totalCount,
    required this.page,
    required this.pageSize,
  });

  /// Trainers in this page.
  final List<Trainer> items;

  /// Total trainers matching the query.
  final int totalCount;

  /// One-based page number.
  final int page;

  /// Requested page size.
  final int pageSize;

  /// Whether another page can be requested.
  bool get hasNext => page * pageSize < totalCount;
}
