import 'package:kinevor_trainers/kinevor_trainers.dart';

/// Test double for [TrainersRepository] with scripted failures and search logs.
class FakeTrainersRepository implements TrainersRepository {
  /// Creates a fake with optional [trainers] and [error].
  FakeTrainersRepository({
    this.trainers = const [],
    this.error,
    this.detailError,
  });

  /// Trainers returned by [getTrainers] and [getTrainer].
  final List<Trainer> trainers;

  /// When set, [getTrainers] throws this error.
  Object? error;

  /// When set, [getTrainer] throws this error.
  Object? detailError;

  /// Search terms observed by [getTrainers].
  final List<String?> searches = [];

  @override
  Future<TrainersPage> getTrainers({
    int page = 1,
    int pageSize = 20,
    String? search,
  }) async {
    searches.add(search);
    final failure = error;
    if (failure != null) throw failure;
    final filtered = search == null || search.isEmpty
        ? trainers
        : trainers
              .where(
                (t) =>
                    t.fullName.toLowerCase().contains(search.toLowerCase()) ||
                    t.email.toLowerCase().contains(search.toLowerCase()),
              )
              .toList();
    return TrainersPage(
      items: filtered,
      totalCount: filtered.length,
      page: page,
      pageSize: pageSize,
    );
  }

  @override
  Future<Trainer> getTrainer(String id) async {
    final failure = detailError;
    if (failure != null) throw failure;
    return trainers.firstWhere((t) => t.id == id);
  }
}

/// Sample trainer used across widget and golden tests.
const sampleTrainers = [
  Trainer(
    id: 't1',
    fullName: 'Nour Adel',
    email: 'nour.adel@kinevor.demo',
    phone: '+201003456789',
    bio:
        'High-energy cycling and dance coach with a focus on rhythm and endurance.',
    specializations: ['Spinning', 'Zumba', 'Aqua'],
    rating: 4.9,
    totalRatings: 143,
    yearsExperience: 5,
    status: 'Active',
    certifications: ['Spinning Instructor', 'Zumba Licensed'],
  ),
  Trainer(
    id: 't2',
    fullName: 'Karim Fathy',
    email: 'karim.fathy@kinevor.demo',
    phone: '+201004567890',
    bio:
        'Combat sports coach with a decade of ring and conditioning experience.',
    specializations: ['Boxing', 'Kickboxing', 'HIIT'],
    rating: 4.6,
    totalRatings: 87,
    yearsExperience: 10,
    status: 'Active',
    certifications: ['USA Boxing Coach', 'First Aid'],
  ),
];
