import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_identity_access/kinevor_identity_access.dart';

class FakeIdentityAccessRepository implements IdentityAccessRepository {
  FakeIdentityAccessRepository({this.failure});

  final Object? failure;
  int loginCalls = 0;
  int refreshCalls = 0;
  int logoutCalls = 0;
  String? lastEmail;

  @override
  Future<AuthSession> login(String email, String password) async {
    loginCalls++;
    lastEmail = email;
    if (failure != null) throw failure!;
    return sampleSession;
  }

  @override
  Future<void> logout(String refreshToken) async {
    logoutCalls++;
    if (failure != null) throw failure!;
  }

  @override
  Future<AuthSession> refresh(String refreshToken) async {
    refreshCalls++;
    if (failure != null) throw failure!;
    return AuthSession(
      accessToken: 'rotated-access-token-value-for-demo',
      refreshToken: 'rotated-refresh-token-value-for-demo',
      expiresAt: DateTime.utc(2026, 11, 1, 9, 30),
      user: sampleSession.user,
    );
  }

  @override
  Future<UserAccount> session(String accessToken) async {
    if (failure != null) throw failure!;
    return sampleSession.user;
  }
}

final sampleSession = AuthSession(
  accessToken:
      'eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhY2NvdW50LWFkbWluIn0.demo-signature',
  refreshToken: 'refresh-token-value-for-demo',
  expiresAt: DateTime.utc(2026, 10, 20, 18, 45),
  user: const UserAccount(
    id: '3f4b5e6c-account-admin',
    email: 'admin@kinevor.demo',
    displayName: 'Amara Obi',
    role: 'Admin',
    mfaEnabled: true,
    lastLoginAt: null,
  ),
);
