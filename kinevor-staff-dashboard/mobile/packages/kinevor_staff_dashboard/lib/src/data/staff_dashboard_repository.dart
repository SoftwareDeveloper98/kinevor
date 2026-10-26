import '../models/staff_dashboard_models.dart';
import 'kinevor_staff_dashboard_client.dart';

/// Data source for the staff dashboard screens.
abstract interface class StaffDashboardRepository {
  /// Loads the dashboard for a branch and trend range.
  Future<StaffDashboard> getDashboard({String? branchId, int days = 7});

  /// Loads the complete operational alert inbox.
  Future<DashboardAlerts> getAlerts({String? branchId});

  /// Loads the branches available in the dashboard selector.
  Future<List<DashboardBranch>> getBranches();
}

/// [StaffDashboardRepository] backed by [KinevorStaffDashboardClient].
class RemoteStaffDashboardRepository implements StaffDashboardRepository {
  /// Creates a repository over [client].
  const RemoteStaffDashboardRepository(this.client);

  /// HTTP client used for every call.
  final KinevorStaffDashboardClient client;

  @override
  Future<StaffDashboard> getDashboard({String? branchId, int days = 7}) =>
      client.fetchDashboard(branchId: branchId, days: days);

  @override
  Future<DashboardAlerts> getAlerts({String? branchId}) =>
      client.fetchAlerts(branchId: branchId);

  @override
  Future<List<DashboardBranch>> getBranches() => client.fetchBranches();
}
