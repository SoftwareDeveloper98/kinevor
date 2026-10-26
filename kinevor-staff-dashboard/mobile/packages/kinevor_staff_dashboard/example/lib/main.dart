import 'package:flutter/material.dart';
import 'package:kinevor_staff_dashboard/kinevor_staff_dashboard.dart';

const String _apiBaseUrl = String.fromEnvironment(
  'KINEVOR_API_BASE_URL',
  defaultValue: 'http://localhost:5089/api/v1',
);

void main() {
  final client = KinevorStaffDashboardClient(
    baseUrl: Uri.parse(_apiBaseUrl),
    headers: const {
      'X-Demo-Role': 'Staff',
      'X-Demo-Email': 'staff.demo@kinevor.local',
      'X-Demo-User': 'staff-demo',
    },
  );
  runApp(
    StaffDashboardDemoApp(repository: RemoteStaffDashboardRepository(client)),
  );
}

/// Demo application hosting [StaffDashboardScreen] against the staff DemoApi.
class StaffDashboardDemoApp extends StatelessWidget {
  /// Creates the demo app.
  const StaffDashboardDemoApp({super.key, required this.repository});

  /// Data source used by the dashboard screens.
  final StaffDashboardRepository repository;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Kinevor Staff Dashboard',
      debugShowCheckedModeBanner: false,
      theme: KinevorStaffDashboardTheme.dark(),
      home: StaffDashboardScreen(repository: repository),
    );
  }
}
