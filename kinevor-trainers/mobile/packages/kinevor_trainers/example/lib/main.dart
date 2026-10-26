import 'package:flutter/material.dart';
import 'package:kinevor_trainers/kinevor_trainers.dart';

const String _apiBaseUrl = String.fromEnvironment(
  'KINEVOR_API_BASE_URL',
  defaultValue: 'http://localhost:5081/api/v1',
);

void main() {
  final client = KinevorTrainersClient(
    baseUrl: Uri.parse(_apiBaseUrl),
    headers: const {
      'X-Demo-Role': 'Staff',
      'X-Demo-Email': 'staff.demo@kinevor.local',
    },
  );
  runApp(KinevorTrainersDemoApp(repository: RemoteTrainersRepository(client)));
}

/// Demo application hosting [TrainersScreen] against the trainers DemoApi.
class KinevorTrainersDemoApp extends StatelessWidget {
  /// Creates the demo app.
  const KinevorTrainersDemoApp({super.key, required this.repository});

  /// Data source used by the directory.
  final TrainersRepository repository;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Kinevor Trainers',
      debugShowCheckedModeBanner: false,
      theme: KinevorTrainersTheme.dark(),
      home: TrainersScreen(
        repository: repository,
        actions: [
          IconButton(
            tooltip: _apiBaseUrl,
            onPressed: () {},
            icon: const Icon(Icons.link),
          ),
        ],
      ),
    );
  }
}
