import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_identity_access/kinevor_identity_access.dart';

import 'fake_identity_access_repository.dart';

void main() {
  testWidgets('sign in submits the seeded credentials', (tester) async {
    final repository = FakeIdentityAccessRepository();
    AuthSession? signedIn;

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorIdentityTheme.dark(),
        home: LoginScreen(
          repository: repository,
          onSignedIn: (session) => signedIn = session,
        ),
      ),
    );

    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();

    expect(repository.loginCalls, 1);
    expect(repository.lastEmail, 'admin@kinevor.demo');
    expect(signedIn?.accessToken, sampleSession.accessToken);
  });

  testWidgets('demo account chips replace the e-mail address', (tester) async {
    final repository = FakeIdentityAccessRepository();

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorIdentityTheme.dark(),
        home: LoginScreen(repository: repository, onSignedIn: (_) {}),
      ),
    );

    await tester.tap(find.text('Trainer'));
    await tester.pump();

    final email = tester.widget<TextField>(find.byType(TextField).first);
    expect(email.controller?.text, 'trainer@kinevor.demo');
  });

  testWidgets('login errors are shown without leaving the screen', (
    tester,
  ) async {
    final repository = FakeIdentityAccessRepository(
      failure: const IdentityApiException(
        'Invalid e-mail or password.',
        statusCode: 401,
      ),
    );

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorIdentityTheme.dark(),
        home: LoginScreen(repository: repository, onSignedIn: (_) {}),
      ),
    );

    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();

    expect(find.text('Invalid e-mail or password.'), findsOneWidget);
    expect(find.text('Sign in'), findsOneWidget);
  });

  testWidgets('session rotates and signs out', (tester) async {
    final repository = FakeIdentityAccessRepository();
    var signedOut = false;
    AuthSession? rotated;

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorIdentityTheme.dark(),
        home: SessionScreen(
          repository: repository,
          session: sampleSession,
          onSessionChanged: (session) => rotated = session,
          onSignedOut: () => signedOut = true,
        ),
      ),
    );

    expect(find.text('Amara Obi'), findsOneWidget);
    expect(find.text('ADMIN'), findsOneWidget);

    await tester.tap(find.text('Rotate session'));
    await tester.pumpAndSettle();
    expect(repository.refreshCalls, 1);
    expect(rotated?.refreshToken, 'rotated-refresh-token-value-for-demo');

    await tester.pump(const Duration(seconds: 5));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Sign out'));
    await tester.pumpAndSettle();
    expect(repository.logoutCalls, 1);
    expect(signedOut, isTrue);
  });
}
