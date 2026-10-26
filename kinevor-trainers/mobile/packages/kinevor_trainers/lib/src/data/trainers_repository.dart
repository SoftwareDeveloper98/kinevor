import '../models/trainer.dart';
import 'kinevor_trainers_client.dart';

/// Data source for trainer directory screens.
abstract interface class TrainersRepository {
  /// Loads one page of trainers, optionally filtered by [search].
  Future<TrainersPage> getTrainers({
    int page = 1,
    int pageSize = 20,
    String? search,
  });

  /// Loads a single trainer profile.
  Future<Trainer> getTrainer(String id);
}

/// [TrainersRepository] backed by [KinevorTrainersClient].
class RemoteTrainersRepository implements TrainersRepository {
  /// Creates a repository over [client].
  const RemoteTrainersRepository(this.client);

  /// HTTP client used for all calls.
  final KinevorTrainersClient client;

  @override
  Future<TrainersPage> getTrainers({
    int page = 1,
    int pageSize = 20,
    String? search,
  }) => client.fetchTrainers(page: page, pageSize: pageSize, search: search);

  @override
  Future<Trainer> getTrainer(String id) => client.fetchTrainer(id);
}
