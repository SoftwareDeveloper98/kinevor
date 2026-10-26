import 'package:flutter/material.dart';
import 'package:kinevor_identity_access/kinevor_identity_access.dart';

void main() => runApp(const IdentityDemoApp());

class IdentityDemoApp extends StatefulWidget {
  const IdentityDemoApp({super.key});

  @override
  State<IdentityDemoApp> createState() => _IdentityDemoAppState();
}

class _IdentityDemoAppState extends State<IdentityDemoApp> {
  static const _apiBaseUrl = String.fromEnvironment(
    'KINEVOR_API_BASE_URL',
    defaultValue: 'http://localhost:5082/api/v1',
  );

  final _repository = RemoteIdentityAccessRepository(
    KinevorIdentityClient(baseUrl: Uri.parse(_apiBaseUrl)),
  );

  AuthSession? _session;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Kinevor Identity',
      debugShowCheckedModeBanner: false,
      theme: KinevorIdentityTheme.dark(),
      home: AnimatedSwitcher(
        duration: const Duration(milliseconds: 250),
        child: _session == null
            ? LoginScreen(
                key: const ValueKey('login'),
                repository: _repository,
                onSignedIn: (session) => setState(() => _session = session),
              )
            : SessionScreen(
                key: const ValueKey('session'),
                repository: _repository,
                session: _session!,
                onSessionChanged: (session) =>
                    setState(() => _session = session),
                onSignedOut: () => setState(() => _session = null),
              ),
      ),
    );
  }
}
