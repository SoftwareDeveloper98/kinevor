import 'package:flutter/material.dart';

import '../data/check_ins_repository.dart';
import '../theme/check_ins_strings.dart';
import '../theme/kinevor_check_ins_theme.dart';
import 'member_check_in_screen.dart';
import 'visit_history_screen.dart';

/// Member-facing shell with Check in and My visits tabs.
class KinevorCheckInsHome extends StatefulWidget {
  /// Creates the member check-in home.
  const KinevorCheckInsHome({
    required this.repository,
    this.strings = const CheckInsStrings(),
    this.initialIndex = 0,
    super.key,
  });

  /// Data source used by both tabs.
  final CheckInsRepository repository;

  /// Localized labels.
  final CheckInsStrings strings;

  /// Initial tab index, 0 for check-in and 1 for history.
  final int initialIndex;

  @override
  State<KinevorCheckInsHome> createState() => _KinevorCheckInsHomeState();
}

class _KinevorCheckInsHomeState extends State<KinevorCheckInsHome> {
  late int _index = widget.initialIndex.clamp(0, 1);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: KinevorCheckInsTheme.ink,
      body: _index == 0
          ? MemberCheckInScreen(
              repository: widget.repository,
              strings: widget.strings,
              onOpenHistory: () => setState(() => _index = 1),
            )
          : VisitHistoryScreen(
              repository: widget.repository,
              strings: widget.strings,
            ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (value) => setState(() => _index = value),
        destinations: [
          NavigationDestination(
            icon: const Icon(Icons.login_outlined),
            selectedIcon: const Icon(Icons.login),
            label: widget.strings.checkInTitle,
          ),
          NavigationDestination(
            icon: const Icon(Icons.history_outlined),
            selectedIcon: const Icon(Icons.history),
            label: widget.strings.historyTitle,
          ),
        ],
      ),
    );
  }
}
