/// Account returned by the Kinevor identity API.
class UserAccount {
  /// Creates an account projection.
  const UserAccount({
    required this.id,
    required this.email,
    required this.displayName,
    required this.role,
    required this.mfaEnabled,
    this.lastLoginAt,
  });

  /// Stable account identifier.
  final String id;

  /// Login e-mail address.
  final String email;

  /// Display name.
  final String displayName;

  /// Platform role: Admin, Staff, Trainer or Member.
  final String role;

  /// Whether a second factor is enrolled.
  final bool mfaEnabled;

  /// Last successful login timestamp.
  final DateTime? lastLoginAt;

  /// Up to two uppercase initials for avatars.
  String get initials {
    final parts = displayName
        .trim()
        .split(RegExp(r'\s+'))
        .where((part) => part.isNotEmpty);
    if (parts.isEmpty) return '?';
    return parts.take(2).map((part) => part[0].toUpperCase()).join();
  }

  /// Parses a defensive account payload.
  factory UserAccount.fromJson(Map<String, dynamic> json) => UserAccount(
    id: json['id']?.toString() ?? '',
    email: json['email']?.toString() ?? '',
    displayName: json['displayName']?.toString() ?? '',
    role: json['role']?.toString() ?? 'Member',
    mfaEnabled: json['mfaEnabled'] == true,
    lastLoginAt: DateTime.tryParse(json['lastLoginAt']?.toString() ?? ''),
  );
}

/// Access token, rotating refresh token and the signed-in account.
class AuthSession {
  /// Creates a session.
  const AuthSession({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresAt,
    required this.user,
  });

  /// JWT access token.
  final String accessToken;

  /// Opaque single-use refresh token.
  final String refreshToken;

  /// UTC access-token expiry.
  final DateTime expiresAt;

  /// Signed-in account.
  final UserAccount user;

  /// Whether the access token has expired at [now].
  bool isExpired({DateTime? now}) =>
      !expiresAt.isAfter(now ?? DateTime.now().toUtc());

  /// Parses a defensive session payload.
  factory AuthSession.fromJson(Map<String, dynamic> json) {
    final user = json['user'];
    return AuthSession(
      accessToken: json['accessToken']?.toString() ?? '',
      refreshToken: json['refreshToken']?.toString() ?? '',
      expiresAt:
          DateTime.tryParse(json['expiresAt']?.toString() ?? '')?.toUtc() ??
          DateTime.now().toUtc(),
      user: user is Map
          ? UserAccount.fromJson(Map<String, dynamic>.from(user))
          : const UserAccount(
              id: '',
              email: '',
              displayName: '',
              role: 'Member',
              mfaEnabled: false,
            ),
    );
  }
}
