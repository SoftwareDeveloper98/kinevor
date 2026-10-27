# kinevor_checkins_access

Flutter member check-in, live occupancy and visit-history UI for Kinevor.

## Features

- `KinevorCheckInsHome` shell with Check in and My visits tabs.
- `MemberCheckInScreen` with membership context, server eligibility, live
  occupancy, current visit, check-in/check-out actions, last visit and monthly
  membership summary.
- `VisitHistoryScreen` with monthly activity, dated visit cards, source and
  duration, and paged loading.
- Typed `KinevorCheckInsClient`, `CheckInsRepository` and
  `RemoteCheckInsRepository`.
- Kinevor dark theme, Sora typography and configurable `CheckInsStrings`.
- Injectable repository for tests and non-Kinevor hosts.
- Widget, HTTP client and golden tests.

## Install

```yaml
dependencies:
  kinevor_checkins_access:
    git:
      url: https://github.com/SoftwareDeveloper98/kinevor.git
      path: mobile/packages/kinevor_checkins_access
      ref: main
```

## Use

```dart
import 'package:flutter/material.dart';
import 'package:kinevor_checkins_access/kinevor_checkins_access.dart';

final client = KinevorCheckInsClient(
  baseUrl: Uri.parse('https://api.example.com/api/v1'),
  headers: {'Authorization': 'Bearer $accessToken'},
);
final repository = RemoteCheckInsRepository(client);

runApp(
  MaterialApp(
    theme: KinevorCheckInsTheme.dark(),
    home: KinevorCheckInsHome(repository: repository),
  ),
);
```

The package does not authenticate the member. Supply headers or an
`http.Client` from the host's authentication stack. The demo host uses the API's
development-only `X-Demo-Role`, `X-Demo-Member` and `X-Demo-Email` trust headers.

## Screens

```dart
KinevorCheckInsHome(repository: repository)
MemberCheckInScreen(repository: repository)
VisitHistoryScreen(repository: repository)
```

`MemberCheckInScreen` accepts an optional `onOpenHistory` callback. Both screens
accept configurable `CheckInsStrings` for localization, and the history screen
accepts `pageSize`.

## Example

```powershell
cd example
flutter pub get
flutter run -d windows
flutter run -d chrome --dart-define=CHECKINS_API_BASE_URL=http://localhost:5086/api/v1
```

The example targets Maya Thompson's seeded member. On an Android emulator use
`http://10.0.2.2:5086/api/v1`.

## Test

```powershell
flutter pub get
flutter analyze
flutter test
flutter test --update-goldens test\check_ins_golden_test.dart
```

Golden images live in `test/goldens` and are published as the mobile screenshots
in the repository README.

## License

MIT
