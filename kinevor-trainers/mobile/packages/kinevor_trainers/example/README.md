# Kinevor Trainers example

Demo host for the `kinevor_trainers` Flutter package.

Start the trainers DemoApi from `backend/`:

```powershell
dotnet run --project samples/Kinevor.Modules.Trainers.DemoApi -c Release --urls http://localhost:5081
```

Run the example on Windows or the web:

```powershell
flutter run -d windows
flutter run -d chrome --web-port 4200 --dart-define=KINEVOR_API_BASE_URL=http://localhost:5081/api/v1
```

The DemoApi allows CORS from `http://localhost:4200`, so use `--web-port 4200`
when running in a browser. On an Android emulator, replace `localhost` with
`10.0.2.2`.

The demo sends the trust headers `X-Demo-Role: Staff` and
`X-Demo-Email: staff.demo@kinevor.local`; a real host should supply an
authenticated HTTP client instead.
