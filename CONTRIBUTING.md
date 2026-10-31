# Contributing

Thank you for looking at Kinevor. This is a portfolio reference implementation,
not a product with a public roadmap, but focused fixes and documentation
improvements are welcome.

## Before you start

1. Read [ARCHITECTURE.md](ARCHITECTURE.md) so changes stay inside the owning
   module.
2. Read [SECURITY.md](SECURITY.md). Do not open a public issue for a
   vulnerability.
3. Keep modules independent. Do not add a project reference from one
   `kinevor-*` service to another.

## Development setup

Follow the [Quick start](README.md#quick-start) in the root README:

1. Pack the platform NuGet packages and build the Angular platform library.
2. Run the module you are changing (`dotnet run`, `npm start`, or
   `flutter run`).
3. Use the seeded demo accounts documented in each module README.

## Making a change

- One concern per pull request (one module, or a shared platform contract).
- Match the existing layout: domain / application / API on the backend;
  gateway / facade / component on the web; client / repository / screen on
  mobile.
- Do not commit `.env` files, SQLite databases, `bin/`, `obj/`, `node_modules/`,
  `dist/`, or signing keys.
- Demo authentication headers and seeded passwords are intentional. Do not
  “fix” them into production auth in a drive-by change.

## Tests

Run the suites that cover what you touched:

```bash
dotnet test <module>/backend/*.slnx -c Release
```

```bash
cd <module>/web
npm run install:local
npm test
```

```bash
cd <module>/mobile/packages/<package>
flutter test
```

CI on `main` builds the platform, then runs backend, web and mobile matrices.
A change is not ready if those jobs fail.

## Pull requests

- Use a short title that says why the change exists.
- Link the issue if there is one.
- Include screenshots when you change a visible screen.
- Be ready to discuss the service boundary: if two modules need the same data,
  prefer duplicating a read model over coupling them.

By contributing you agree that your work is licensed under the MIT License in
[LICENSE](LICENSE).
