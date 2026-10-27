# kinevor_notifications

Flutter member notification inbox, detail, preferences and device UI for Kinevor.

## Features

- `NotificationInboxScreen` with category and channel filters, unread-only
  filtering, an unread summary and mark-all-read.
- `NotificationDetailScreen` with the delivered message, metadata, status and an
  explicit mark-read action.
- `NotificationPreferencesScreen` with per-category Email, Push and In-app
  switches plus push-device registration and removal.
- Typed `KinevorNotificationsClient`, `NotificationsRepository` and
  `RemoteNotificationsRepository`.
- Kinevor dark theme, Sora typography and configurable `NotificationsStrings`.
- Injectable repository for tests and non-Kinevor hosts.
- Widget and golden tests for the inbox, detail and preferences screens.

## Install

```yaml
dependencies:
  kinevor_notifications:
    git:
      url: https://github.com/SoftwareDeveloper98/kinevor.git
      path: mobile/packages/kinevor_notifications
      ref: main
```

## Use

```dart
import 'package:flutter/material.dart';
import 'package:kinevor_notifications/kinevor_notifications.dart';

final repository = RemoteNotificationsRepository(
  KinevorNotificationsClient(
    baseUrl: Uri.parse('https://api.example.com/api/v1'),
    headers: {'Authorization': 'Bearer $accessToken'},
  ),
);

runApp(MaterialApp(
  theme: KinevorNotificationsTheme.dark(),
  home: NotificationInboxScreen(repository: repository),
));
```

The package does not authenticate the member. Supply an `http.Client` or headers
from the host's authentication stack. The demo host uses the API's
development-only `X-Demo-Role` and `X-Demo-Member` trust headers.

## Screens

```dart
NotificationInboxScreen(repository: repository)
NotificationDetailScreen(repository: repository, notificationId: id)
NotificationPreferencesScreen(repository: repository)
```

The inbox opens detail and preferences routes. All screens expose configurable
`NotificationsStrings` for localization.

## Example

```powershell
cd example
flutter pub get
flutter run -d windows
flutter run -d chrome --web-port=4203 --dart-define=KINEVOR_API_BASE_URL=http://localhost:5088/api/v1
```

The demo targets Maya Thompson's seeded notifications at
`http://localhost:5088/api/v1`. On an Android emulator use
`http://10.0.2.2:5088/api/v1`.

## Test

```powershell
flutter pub get
flutter analyze
flutter test
flutter test --update-goldens test\notifications_golden_test.dart
```

Golden images live in `test/goldens` and are published as README screenshots.

## License

MIT
