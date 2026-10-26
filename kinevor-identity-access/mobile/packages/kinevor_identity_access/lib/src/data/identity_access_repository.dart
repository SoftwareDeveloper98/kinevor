import '../models/user_account.dart';
import 'kinevor_identity_client.dart';

/// Data access used by the identity widgets.
abstract interface class IdentityAccessRepository {
  /// Exchanges credentials for a token pair.
  Future<AuthSession> login(String email, String password);

  /// Loads the account for an access token.
  Future<UserAccount> session(String accessToken);

  /// Rotates a refresh token.
  Future<AuthSession> refresh(String refreshToken);

  /// Revokes a refresh token.
  Future<void> logout(String refreshToken);
}

/// Repository backed by [KinevorIdentityClient].
class RemoteIdentityAccessRepository implements IdentityAccessRepository {
  /// Creates a repository over [client].
  const RemoteIdentityAccessRepository(this.client);

  /// HTTP client used for every call.
  final KinevorIdentityClient client;

  @override
  Future<AuthSession> login(String email, String password) =>
      client.login(email: email, password: password);

  @override
  Future<UserAccount> session(String accessToken) =>
      client.session(accessToken);

  @override
  Future<AuthSession> refresh(String refreshToken) =>
      client.refresh(refreshToken);

  @override
  Future<void> logout(String refreshToken) => client.logout(refreshToken);
}
