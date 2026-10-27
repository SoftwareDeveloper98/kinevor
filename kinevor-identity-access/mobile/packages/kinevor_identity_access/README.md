# kinevor_identity_access

Flutter identity and access UI and API client for Kinevor.

## Features

- `LoginScreen` with seeded-account shortcuts, password visibility, loading and
  inline API errors.
- `SessionScreen` with account facts, token metadata, rotation and sign-out.
- Typed `KinevorIdentityClient`, `IdentityAccessRepository` and
  `RemoteIdentityAccessRepository`.
- Kinevor dark theme, Sora typography and configurable strings.
- Injectable repository for tests and non-Kinevor hosts.

## Install

```yaml
dependencies:
  kinevor_identity_access:
    git:
      url: https://github.com/SoftwareDeveloper98/kinevor.git
      path: mobile/packages/kinevor_identity_access
      ref: main
```

## Use

```dart
import 'package:flutter/material.dart';
import 'package:kinevor_identity_access/kinevor_identity_access.dart';

final repository = RemoteIdentityAccessRepository(
  KinevorIdentityClient(
    baseUrl: Uri.parse('https://api.example.com/api/v1'),
  ),
);

runApp(MaterialApp(
  theme: KinevorIdentityTheme.dark(),
  home: LoginScreen(
    repository: repository,
    onSignedIn: (session) {
      // Persist the token pair with the host's secure storage.
    },
  ),
));
```

`KinevorIdentityClient` expects the login, refresh, logout and session endpoints
described in the repository README. The package does not persist tokens or read
global state; hosts decide where sessions live.

## Session screen

```dart
SessionScreen(
  repository: repository,
  session: session,
  onSessionChanged: (rotated) => save(rotated),
  onSignedOut: () => clear(),
)
```

## Example

```powershell
cd example
flutter run -d windows
flutter run -d chrome --web-port 4221 --dart-define=KINEVOR_API_BASE_URL=http://localhost:5082/api/v1
```

## Test

```powershell
flutter pub get
flutter analyze
flutter test
flutter test --update-goldens test\identity_golden_test.dart
```

Golden images live in `test/goldens` and are published as README screenshots.

## License

MIT
