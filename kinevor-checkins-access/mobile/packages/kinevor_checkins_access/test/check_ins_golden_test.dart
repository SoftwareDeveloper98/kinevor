import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_checkins_access/kinevor_checkins_access.dart';

import 'fake_check_ins_repository.dart';

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

Widget _goldenApp(int index) => MaterialApp(
  theme: KinevorCheckInsTheme.dark(),
  home: KinevorCheckInsHome(
    repository: FakeCheckInsRepository(),
    initialIndex: index,
  ),
);

void main() {
  setUpAll(_loadFonts);

  testWidgets('member check-in golden', (tester) async {
    await tester.binding.setSurfaceSize(const Size(390, 844));
    addTearDown(() => tester.binding.setSurfaceSize(null));

    await tester.pumpWidget(_goldenApp(0));
    await tester.pumpAndSettle();

    await expectLater(
      find.byType(KinevorCheckInsHome),
      matchesGoldenFile('goldens/member_check_in.png'),
    );
  });

  testWidgets('member history golden', (tester) async {
    await tester.binding.setSurfaceSize(const Size(390, 844));
    addTearDown(() => tester.binding.setSurfaceSize(null));

    await tester.pumpWidget(_goldenApp(1));
    await tester.pumpAndSettle();

    await expectLater(
      find.byType(KinevorCheckInsHome),
      matchesGoldenFile('goldens/member_history.png'),
    );
  });
}
