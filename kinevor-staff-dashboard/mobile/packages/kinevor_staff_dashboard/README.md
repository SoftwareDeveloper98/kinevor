# kinevor_staff_dashboard

Flutter staff dashboard, trend and alert inbox UI and API client for Kinevor.

## Features

- `StaffDashboardScreen` with the executive pulse hero, branch scope, 7/30-day
  range switch, six KPI cards, check-in and booking trend, critical alerts and
  the latest activity feed.
- `DashboardAlertsScreen` with severity tiles, severity filters, branch scope
  and the full operational task list.
- Typed `KinevorStaffDashboardClient`, `StaffDashboardRepository` and
  `RemoteStaffDashboardRepository`.
- Kinevor dark theme, Sora typography and configurable `StaffDashboardStrings`.
- Pull-to-refresh, loading, empty, error and retry states throughout.
- Injectable repository for tests and non-Kinevor hosts.

## Install

```yaml
dependencies:
  kinevor_staff_dashboard:
    git:
      url: https://github.com/SoftwareDeveloper98/kinevor-staff-dashboard.git
      path: mobile/packages/kinevor_staff_dashboard
      ref: v0.1.0
```

## Use

```dart
import 'package:flutter/material.dart';
import 'package:kinevor_staff_dashboard/kinevor_staff_dashboard.dart';

final repository = RemoteStaffDashboardRepository(
  KinevorStaffDashboardClient(
    baseUrl: Uri.parse('https://api.example.com/api/v1'),
    headers: {'Authorization': 'Bearer $accessToken'},
  ),
);

runApp(
  MaterialApp(
    theme: KinevorStaffDashboardTheme.dark(),
    home: StaffDashboardScreen(repository: repository),
  ),
);
```

The package does not authenticate staff. Supply an `http.Client` or headers from
the host's authentication stack. The demo host uses the API's development-only
`X-Demo-Role` and `X-Demo-User` trust headers.

## Screens

```dart
StaffDashboardScreen(repository: repository)
DashboardAlertsScreen(repository: repository)
```

The dashboard app bar opens the alert inbox. Both screens expose configurable
`StaffDashboardStrings` for localization. Pass `initialBranchId` to open a
specific branch or leave it null to start with all branches.

## Data

The client calls:

```text
GET /staff-dashboard?branchId=...&days=7|30
GET /staff-dashboard/alerts?branchId=...
GET /staff-dashboard/branches
```

The trend chart uses only check-ins and bookings. Revenue remains available on
`DashboardTrendPoint` and in the monthly KPI. Alerts are read-only; the package
does not add resolve or follow-up actions that the API does not expose.

## Example

```powershell
cd example
flutter pub get
flutter run -d chrome --dart-define=KINEVOR_API_BASE_URL=http://localhost:5089/api/v1
```

The demo trusts the seeded staff account. On an Android emulator use
`http://10.0.2.2:5089/api/v1`.

## Test

```powershell
flutter pub get
flutter analyze
flutter test
flutter test --update-goldens test\staff_dashboard_golden_test.dart
```

Golden images live in `test/goldens` and are published alongside the dashboard
screenshots in the repository README.

## License

MIT
