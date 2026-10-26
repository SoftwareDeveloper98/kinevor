import 'package:flutter/material.dart';
import 'package:kinevor_checkins_access/kinevor_checkins_access.dart';

const String _apiBaseUrl = String.fromEnvironment(
  'CHECKINS_API_BASE_URL',
  defaultValue: 'http://localhost:5086/api/v1',
);

const String _demoMemberId = '10000000-0000-0000-0000-000000000001';
const String _demoMemberEmail = 'maya.thompson@kinevor.demo';

void main() {
  runApp(const CheckInsExampleApp());
}

/// Demo host for the member check-in experience.
class CheckInsExampleApp extends StatefulWidget {
  /// Creates the example application.
  const CheckInsExampleApp({super.key});

  @override
  State<CheckInsExampleApp> createState() => _CheckInsExampleAppState();
}

class _CheckInsExampleAppState extends State<CheckInsExampleApp> {
  late final KinevorCheckInsClient _client;
  late final CheckInsRepository _repository;

  @override
  void initState() {
    super.initState();
    _client = KinevorCheckInsClient(
      baseUrl: Uri.parse(_apiBaseUrl),
      headers: const {
        'X-Demo-Role': 'Member',
        'X-Demo-Member': _demoMemberId,
        'X-Demo-Email': _demoMemberEmail,
      },
    );
    _repository = RemoteCheckInsRepository(_client);
  }

  @override
  void dispose() {
    _client.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Kinevor Check-ins',
      debugShowCheckedModeBanner: false,
      theme: KinevorCheckInsTheme.dark(),
      home: KinevorCheckInsHome(repository: _repository),
    );
  }
}
