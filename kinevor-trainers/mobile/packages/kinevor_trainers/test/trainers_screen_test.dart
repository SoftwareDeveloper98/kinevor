import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_trainers/kinevor_trainers.dart';

import 'fake_trainers_repository.dart';

Widget _app(FakeTrainersRepository repository) => MaterialApp(
  theme: KinevorTrainersTheme.dark(),
  home: TrainersScreen(repository: repository),
);

void main() {
  testWidgets('renders trainers from the repository', (tester) async {
    await tester.pumpWidget(
      _app(FakeTrainersRepository(trainers: sampleTrainers)),
    );
    await tester.pumpAndSettle();

    expect(find.text('Nour Adel'), findsOneWidget);
    expect(find.text('Karim Fathy'), findsOneWidget);
    expect(find.text('5 years experience'), findsOneWidget);
  });

  testWidgets('shows the empty state', (tester) async {
    await tester.pumpWidget(_app(FakeTrainersRepository()));
    await tester.pumpAndSettle();

    expect(find.text('No trainers found'), findsOneWidget);
  });

  testWidgets('shows an error and retries', (tester) async {
    final repository = FakeTrainersRepository(
      trainers: sampleTrainers,
      error: const TrainersApiException('Service unavailable'),
    );
    await tester.pumpWidget(_app(repository));
    await tester.pumpAndSettle();

    expect(find.text('Service unavailable'), findsOneWidget);

    repository.error = null;
    await tester.tap(find.text('Try again'));
    await tester.pumpAndSettle();

    expect(find.text('Nour Adel'), findsOneWidget);
  });

  testWidgets('debounces search and sends the term', (tester) async {
    final repository = FakeTrainersRepository(trainers: sampleTrainers);
    await tester.pumpWidget(_app(repository));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextField), 'karim');
    await tester.pump(const Duration(milliseconds: 400));
    await tester.pumpAndSettle();

    expect(repository.searches.last, 'karim');
    expect(find.text('Karim Fathy'), findsOneWidget);
    expect(find.text('Nour Adel'), findsNothing);
  });

  testWidgets('opens the trainer profile', (tester) async {
    final repository = FakeTrainersRepository(trainers: sampleTrainers);
    await tester.pumpWidget(_app(repository));
    await tester.pumpAndSettle();

    await tester.tap(find.text('Nour Adel'));
    await tester.pumpAndSettle();

    expect(find.text('About'), findsOneWidget);
    expect(
      find.text(
        'High-energy cycling and dance coach with a focus on rhythm and endurance.',
      ),
      findsOneWidget,
    );

    await tester.scrollUntilVisible(
      find.text('Spinning Instructor'),
      250,
      scrollable: find.byType(Scrollable).last,
    );
    expect(find.text('Spinning Instructor'), findsOneWidget);
  });
}
