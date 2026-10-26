import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kinevor_staff_dashboard/kinevor_staff_dashboard.dart';

import 'fake_staff_dashboard_repository.dart';

Future<void> _loadFonts() async {
  final bytes = await File('assets/fonts/Sora-Variable.ttf').readAsBytes();
  final sora = FontLoader(KinevorStaffDashboardTheme.soraFontFamily)
    ..addFont(Future<ByteData>.value(ByteData.sublistView(bytes)));
  await sora.load();

  final material = await rootBundle.load('fonts/MaterialIcons-Regular.otf');
  final icons = FontLoader('MaterialIcons')
    ..addFont(Future<ByteData>.value(material));
  await icons.load();
}

void main() {
  setUpAll(_loadFonts);

  testWidgets('staff dashboard golden', (tester) async {
    await tester.binding.setSurfaceSize(const Size(390, 844));
    addTearDown(() => tester.binding.setSurfaceSize(null));

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorStaffDashboardTheme.dark(),
        home: StaffDashboardScreen(repository: FakeStaffDashboardRepository()),
      ),
    );
    await tester.pumpAndSettle();

    await expectLater(
      find.byType(StaffDashboardScreen),
      matchesGoldenFile('goldens/staff_dashboard.png'),
    );
  });

  testWidgets('alert inbox golden', (tester) async {
    await tester.binding.setSurfaceSize(const Size(390, 844));
    addTearDown(() => tester.binding.setSurfaceSize(null));

    await tester.pumpWidget(
      MaterialApp(
        theme: KinevorStaffDashboardTheme.dark(),
        home: DashboardAlertsScreen(repository: FakeStaffDashboardRepository()),
      ),
    );
    await tester.pumpAndSettle();

    await expectLater(
      find.byType(DashboardAlertsScreen),
      matchesGoldenFile('goldens/staff_alerts.png'),
    );
  });
}
