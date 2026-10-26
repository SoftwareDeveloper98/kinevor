import 'package:flutter/material.dart';

import '../data/trainers_repository.dart';
import '../models/trainer.dart';
import '../theme/trainers_strings.dart';
import 'trainer_detail_page.dart';
import 'trainers_directory.dart';

/// Drop-in trainers screen with an app bar and the directory body.
class TrainersScreen extends StatelessWidget {
  /// Creates a trainers screen bound to [repository].
  const TrainersScreen({
    super.key,
    required this.repository,
    this.strings = const TrainersStrings(),
    this.onTrainerTap,
    this.actions,
  });

  /// Data source for the directory.
  final TrainersRepository repository;

  /// Localized labels.
  final TrainersStrings strings;

  /// Optional tap handler; defaults to pushing [TrainerDetailPage].
  final ValueChanged<Trainer>? onTrainerTap;

  /// Optional app-bar actions.
  final List<Widget>? actions;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(strings.title), actions: actions),
      body: TrainersDirectory(
        repository: repository,
        strings: strings,
        onTrainerTap: (trainer) {
          if (onTrainerTap != null) {
            onTrainerTap!(trainer);
            return;
          }
          Navigator.of(context).push(
            MaterialPageRoute<void>(
              builder: (_) => TrainerDetailPage(
                repository: repository,
                trainerId: trainer.id,
                initialTrainer: trainer,
                strings: strings,
              ),
            ),
          );
        },
      ),
    );
  }
}
