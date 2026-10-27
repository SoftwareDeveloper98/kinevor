# kinevor_trainers

Flutter trainer directory UI and API client for Kinevor.

## Features

- `TrainersDirectory` with search, loading, empty, error and retry states.
- `TrainerDetailPage` with rating, specializations and certifications.
- `TrainersScreen` as a ready-made page with navigation.
- Typed `KinevorTrainersClient`, `TrainersRepository` and `RemoteTrainersRepository`.
- Kinevor dark theme, Sora typography and configurable strings.
- Injectable repository for tests and non-Kinevor hosts.

## Install

```yaml
dependencies:
  kinevor_trainers:
    git:
      url: https://github.com/SoftwareDeveloper98/kinevor.git
      path: mobile/packages/kinevor_trainers
      ref: main
```

## Use

```dart
import 'package:flutter/material.dart';
import 'package:kinevor_trainers/kinevor_trainers.dart';

void main() {
  final client = KinevorTrainersClient(
    baseUrl: Uri.parse('https://api.example.com/api/v1'),
    headers: {'Authorization': 'Bearer $token'},
  );

  runApp(MaterialApp(
    theme: KinevorTrainersTheme.dark(),
    home: TrainersScreen(
      repository: RemoteTrainersRepository(client),
    ),
  ));
}
```

`KinevorTrainersClient` expects the trainer list and detail endpoints described
in the repository README. Pass authentication headers from the host; the package
does not read tokens or global state.

## Example

```powershell
cd example
flutter run -d windows
flutter run -d chrome --web-port 4200 --dart-define=KINEVOR_API_BASE_URL=http://localhost:5081/api/v1
```

The example sends the demo trust headers for the bundled DemoApi. Replace these
with real authentication in production.

## Test

```powershell
flutter pub get
flutter analyze
flutter test
flutter test --update-goldens test\trainers_golden_test.dart
```

Golden images live in `test/goldens` and are published as README screenshots.

## License

MIT
