# Kinevor staff dashboard example

Demo host for `kinevor_staff_dashboard`, wired to the staff dashboard DemoApi.

```powershell
flutter pub get
flutter run -d windows --dart-define=KINEVOR_API_BASE_URL=http://localhost:5089/api/v1
```

Run `flutter run -d chrome` for the web build. On an Android emulator use
`http://10.0.2.2:5089/api/v1` instead of localhost. The demo trusts the API's
development-only `X-Demo-Role` and `X-Demo-User` headers, so start the DemoApi
first and keep the default staff seed data.
