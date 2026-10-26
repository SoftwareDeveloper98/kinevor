import '../models/check_in_models.dart';
import 'kinevor_check_ins_client.dart';

/// Data source for the member check-in and visit-history screens.
abstract interface class CheckInsRepository {
  /// Loads eligibility, live occupancy and the current visit.
  Future<MemberCheckInStatus> getStatus();

  /// Loads the member's own visits.
  Future<VisitPage> getVisits({
    int page = 1,
    int pageSize = 20,
    DateTime? from,
    DateTime? to,
  });

  /// Checks the member in at their home club.
  Future<CheckInVisit> checkIn({String? notes});

  /// Checks the member out of their current visit.
  Future<CheckInVisit> checkOut();
}

/// [CheckInsRepository] backed by [KinevorCheckInsClient].
class RemoteCheckInsRepository implements CheckInsRepository {
  /// Creates a repository over [client].
  const RemoteCheckInsRepository(this.client);

  /// HTTP client used for every call.
  final KinevorCheckInsClient client;

  @override
  Future<MemberCheckInStatus> getStatus() => client.fetchStatus();

  @override
  Future<VisitPage> getVisits({
    int page = 1,
    int pageSize = 20,
    DateTime? from,
    DateTime? to,
  }) => client.fetchVisits(page: page, pageSize: pageSize, from: from, to: to);

  @override
  Future<CheckInVisit> checkIn({String? notes}) => client.checkIn(notes: notes);

  @override
  Future<CheckInVisit> checkOut() => client.checkOut();
}
