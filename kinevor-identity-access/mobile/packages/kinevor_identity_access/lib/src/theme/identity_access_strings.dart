/// User-facing labels used by the identity widgets.
class IdentityAccessStrings {
  /// Creates a label bundle.
  const IdentityAccessStrings({
    this.brand = 'Kinevor Identity',
    this.welcome = 'Welcome back.',
    this.subtitle = 'Sign in to manage your gym, your team and your members.',
    this.email = 'E-mail address',
    this.password = 'Password',
    this.signIn = 'Sign in',
    this.signingIn = 'Signing in…',
    this.demoAccounts = 'Seeded demo accounts',
    this.demoPassword = 'Password: Kinevor!2026',
    this.sessionTitle = 'Your session',
    this.signedInAs = 'Signed in as',
    this.accountId = 'Account id',
    this.lastSignIn = 'Last sign-in',
    this.secondFactor = 'Second factor',
    this.expiresAt = 'Access expires',
    this.rotate = 'Rotate session',
    this.signOut = 'Sign out',
    this.enabled = 'Enrolled',
    this.disabled = 'Not enrolled',
    this.never = 'First session',
    this.secure = 'JWT access · rotating refresh',
    this.sessionExpired = 'Your session expired. Sign in again.',
  });

  /// Small brand label.
  final String brand;

  /// Login headline.
  final String welcome;

  /// Login supporting copy.
  final String subtitle;

  /// E-mail field label.
  final String email;

  /// Password field label.
  final String password;

  /// Primary login action.
  final String signIn;

  /// Login action while busy.
  final String signingIn;

  /// Demo accounts section title.
  final String demoAccounts;

  /// Demo password hint.
  final String demoPassword;

  /// Session screen title.
  final String sessionTitle;

  /// Signed-in-as label.
  final String signedInAs;

  /// Account id label.
  final String accountId;

  /// Last sign-in label.
  final String lastSignIn;

  /// Second factor label.
  final String secondFactor;

  /// Access expiry label.
  final String expiresAt;

  /// Refresh action.
  final String rotate;

  /// Logout action.
  final String signOut;

  /// MFA enabled label.
  final String enabled;

  /// MFA disabled label.
  final String disabled;

  /// Missing last-login label.
  final String never;

  /// Security footnote.
  final String secure;

  /// Expired session message.
  final String sessionExpired;
}
