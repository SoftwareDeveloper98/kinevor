import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_trainers/kinevor_trainers.dart';

import 'fake_trainers_repository.dart';

Future<void> _loadFonts() async {
  final bytes = await File('assets/fonts/Sora-Variable.ttf').readAsBytes();
  final sora = FontLoader('Sora')
    ..addFont(Future<ByteData>.value(ByteData.sublistView(bytes)));
  await sora.load();

  final material = await rootBundle.load('fonts/MaterialIcons-Regular.otf');
  final icons = FontLoader('MaterialIcons')
    ..addFont(Future<ByteData>.value(material));
  await icons.load();
}

void main() {
  setUpAll(_loadFonts);

  testWidgets('trainers directory golden', (tester) async {
    await tester.binding.setSurfaceSize(const Size(390, 844));
    addTearDown(() => tester.binding.setSurfaceSize(null));

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorTrainersTheme.dark(),
        home: TrainersScreen(
          repository: FakeTrainersRepository(trainers: sampleTrainers),
        ),
      ),
    );
    await tester.pumpAndSettle();

    await expectLater(
      find.byType(TrainersScreen),
      matchesGoldenFile('goldens/trainers_directory.png'),
    );
  });

  testWidgets('trainer profile golden', (tester) async {
    await tester.binding.setSurfaceSize(const Size(390, 844));
    addTearDown(() => tester.binding.setSurfaceSize(null));

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorTrainersTheme.dark(),
        home: TrainerDetailPage(
          repository: FakeTrainersRepository(trainers: sampleTrainers),
          trainerId: sampleTrainers.first.id,
          initialTrainer: sampleTrainers.first,
        ),
      ),
    );
    await tester.pumpAndSettle();

    await expectLater(
      find.byType(TrainerDetailPage),
      matchesGoldenFile('goldens/trainer_profile.png'),
    );
  });
}
