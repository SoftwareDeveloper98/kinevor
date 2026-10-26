import 'package:flutter/material.dart';

import '../data/staff_dashboard_api_exception.dart';
import '../data/staff_dashboard_repository.dart';
import '../models/staff_dashboard_models.dart';
import '../theme/kinevor_staff_dashboard_theme.dart';
import '../theme/staff_dashboard_strings.dart';
import 'dashboard_common.dart';

/// Complete severity-ordered operational alert inbox.
class DashboardAlertsScreen extends StatefulWidget {
  /// Creates the alert inbox.
  const DashboardAlertsScreen({
    super.key,
    required this.repository,
    this.strings = const StaffDashboardStrings(),
    this.initialBranchId,
    this.initialBranches = const [],
  });

  /// Data source used for alert and branch reads.
  final StaffDashboardRepository repository;

  /// Localized labels.
  final StaffDashboardStrings strings;

  /// Optional branch selected on first load.
  final String? initialBranchId;

  /// Branch options supplied by the dashboard that opened the inbox.
  final List<DashboardBranch> initialBranches;

  @override
  State<DashboardAlertsScreen> createState() => _DashboardAlertsScreenState();
}

class _DashboardAlertsScreenState extends State<DashboardAlertsScreen> {
  DashboardAlerts? _inbox;
  List<DashboardBranch> _branches = const [];
  bool _loading = true;
  String? _error;
  String? _branchId;
  String _filter = 'All';

  @override
  void initState() {
    super.initState();
    _branchId = widget.initialBranchId;
    _branches = widget.initialBranches;
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final inbox = await widget.repository.getAlerts(branchId: _branchId);
      if (!mounted) return;
      setState(() {
        _inbox = inbox;
        if (inbox.branches.isNotEmpty) _branches = inbox.branches;
        _loading = false;
      });
    } on Exception catch (error) {
      if (!mounted) return;
      setState(() {
        _error = _clean(error);
        _loading = false;
      });
    }
  }

  String _clean(Object error) =>
      error is StaffDashboardApiException ? error.message : error.toString();

  Future<void> _selectBranch(String? branchId) async {
    if (branchId == _branchId) return;
    setState(() => _branchId = branchId);
    await _load();
  }

  List<DashboardAlert> get _visibleAlerts {
    final alerts = _inbox?.alerts ?? const <DashboardAlert>[];
    if (_filter == 'All') return alerts;
    return alerts.where((alert) => alert.severity == _filter).toList();
  }

  int _count(String severity) =>
      _inbox?.alerts.where((alert) => alert.severity == severity).length ?? 0;

  @override
  Widget build(BuildContext context) {
    final strings = widget.strings;
    final inbox = _inbox;
    final visible = _visibleAlerts;

    return Scaffold(
      appBar: AppBar(
        title: Text(strings.alertsTitle),
        actions: [
          IconButton(
            tooltip: strings.refresh,
            onPressed: _loading ? null : _load,
            icon: const Icon(Icons.refresh),
          ),
          const SizedBox(width: 4),
        ],
      ),
      body: SafeArea(
        child: Stack(
          children: [
            if (_loading && inbox == null)
              const Center(child: CircularProgressIndicator())
            else if (inbox == null)
              DashboardErrorState(
                message: _error ?? strings.loadAlertsError,
                retryLabel: strings.retry,
                onRetry: _load,
              )
            else
              RefreshIndicator(
                onRefresh: _load,
                child: SingleChildScrollView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  padding: const EdgeInsets.fromLTRB(16, 4, 16, 30),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      _AlertsHero(inbox: inbox, strings: strings),
                      if (_error != null) ...[
                        const SizedBox(height: 10),
                        _AlertsError(message: _error!),
                      ],
                      const SizedBox(height: 10),
                      BranchSelector(
                        branches: _branches,
                        selectedBranchId: _branchId,
                        onSelected: _selectBranch,
                        strings: strings,
                      ),
                      const SizedBox(height: 10),
                      SeveritySummary(
                        critical: _count('Critical'),
                        warning: _count('Warning'),
                        info: _count('Info'),
                        strings: strings,
                        selected: _filter == 'All' ? null : _filter,
                        onSelected: (value) => setState(() => _filter = value),
                      ),
                      const SizedBox(height: 10),
                      _FilterBar(
                        filter: _filter,
                        strings: strings,
                        countFor: (value) => value == 'All'
                            ? inbox.alerts.length
                            : _count(value),
                        onChanged: (value) => setState(() => _filter = value),
                      ),
                      const SizedBox(height: 7),
                      Text(
                        '${inbox.branchName} - ${strings.shown(visible.length)}',
                        style: const TextStyle(
                          fontSize: 8.5,
                          color: KinevorStaffDashboardTheme.textMuted,
                        ),
                      ),
                      const SizedBox(height: 8),
                      if (visible.isEmpty)
                        DashboardEmptyState(
                          key: const ValueKey<String>('alerts-empty'),
                          icon: Icons.task_alt,
                          title: strings.noAlertsInView,
                          message: strings.noAlertsHint,
                          actionLabel: _filter == 'All'
                              ? null
                              : strings.showAllAlerts,
                          onAction: _filter == 'All'
                              ? null
                              : () => setState(() => _filter = 'All'),
                        )
                      else
                        for (
                          var index = 0;
                          index < visible.length;
                          index++
                        ) ...[
                          AlertCard(
                            key: ValueKey<String>('alert-${visible[index].id}'),
                            alert: visible[index],
                            reference: inbox.generatedAt,
                            strings: strings,
                          ),
                          if (index != visible.length - 1)
                            const SizedBox(height: 8),
                        ],
                    ],
                  ),
                ),
              ),
            if (_loading && inbox != null)
              const Positioned(
                top: 0,
                left: 0,
                right: 0,
                child: LinearProgressIndicator(minHeight: 2),
              ),
          ],
        ),
      ),
    );
  }
}

class _AlertsHero extends StatelessWidget {
  const _AlertsHero({required this.inbox, required this.strings});

  final DashboardAlerts inbox;
  final StaffDashboardStrings strings;

  @override
  Widget build(BuildContext context) {
    return Container(
      key: const ValueKey<String>('alerts-hero'),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF2A1A1C), KinevorStaffDashboardTheme.surfaceRaised],
        ),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: KinevorStaffDashboardTheme.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            strings.alertsEyebrow,
            style: const TextStyle(
              fontSize: 8.5,
              fontWeight: FontWeight.w800,
              letterSpacing: 1.3,
              color: KinevorStaffDashboardTheme.accent,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            strings.alertsTitle,
            style: const TextStyle(
              fontSize: 22,
              fontWeight: FontWeight.w800,
              height: 1.1,
              letterSpacing: -0.5,
            ),
          ),
          const SizedBox(height: 3),
          Text(
            '${inbox.branchName} - ${inbox.branchCity}',
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              fontSize: 11,
              color: KinevorStaffDashboardTheme.textMuted,
            ),
          ),
          const SizedBox(height: 8),
          Wrap(
            spacing: 5,
            runSpacing: 5,
            children: [
              _HeroPill(
                icon: Icons.task_alt,
                label: strings.openTasks(inbox.alerts.length),
              ),
              _HeroPill(
                icon: Icons.storefront_outlined,
                label: strings.branchChip(inbox.branches.length),
              ),
              _HeroPill(
                icon: Icons.schedule,
                label:
                    '${strings.updated} '
                    '${strings.time(inbox.generatedAt)}',
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _HeroPill extends StatelessWidget {
  const _HeroPill({required this.icon, required this.label});

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 4),
      decoration: BoxDecoration(
        color: KinevorStaffDashboardTheme.ink.withValues(alpha: 0.45),
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: KinevorStaffDashboardTheme.border),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 11, color: KinevorStaffDashboardTheme.textMuted),
          const SizedBox(width: 4),
          Text(
            label,
            style: const TextStyle(
              fontSize: 9,
              fontWeight: FontWeight.w700,
              color: KinevorStaffDashboardTheme.textMuted,
            ),
          ),
        ],
      ),
    );
  }
}

class _AlertsError extends StatelessWidget {
  const _AlertsError({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
      decoration: BoxDecoration(
        color: KinevorStaffDashboardTheme.error.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(10),
        border: Border.all(
          color: KinevorStaffDashboardTheme.error.withValues(alpha: 0.28),
        ),
      ),
      child: Row(
        children: [
          const Icon(
            Icons.error_outline,
            size: 14,
            color: KinevorStaffDashboardTheme.error,
          ),
          const SizedBox(width: 6),
          Expanded(
            child: Text(
              message,
              style: const TextStyle(
                fontSize: 10,
                color: KinevorStaffDashboardTheme.error,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _FilterBar extends StatelessWidget {
  const _FilterBar({
    required this.filter,
    required this.strings,
    required this.countFor,
    required this.onChanged,
  });

  final String filter;
  final StaffDashboardStrings strings;
  final int Function(String) countFor;
  final ValueChanged<String> onChanged;

  @override
  Widget build(BuildContext context) {
    final options = [
      (value: 'All', label: strings.all),
      (value: 'Critical', label: strings.critical),
      (value: 'Warning', label: strings.warning),
      (value: 'Info', label: strings.info),
    ];

    return Container(
      padding: const EdgeInsets.all(3),
      decoration: BoxDecoration(
        color: KinevorStaffDashboardTheme.surfaceRaised,
        borderRadius: BorderRadius.circular(11),
        border: Border.all(color: KinevorStaffDashboardTheme.border),
      ),
      child: Row(
        children: [
          for (var index = 0; index < options.length; index++) ...[
            if (index > 0) const SizedBox(width: 3),
            Expanded(
              child: _FilterPill(
                key: ValueKey<String>('filter-${options[index].value}'),
                label: options[index].label,
                count: countFor(options[index].value),
                selected: filter == options[index].value,
                onTap: () => onChanged(options[index].value),
              ),
            ),
          ],
        ],
      ),
    );
  }
}

class _FilterPill extends StatelessWidget {
  const _FilterPill({
    super.key,
    required this.label,
    required this.count,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final int count;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        height: 28,
        alignment: Alignment.center,
        decoration: BoxDecoration(
          color: selected
              ? KinevorStaffDashboardTheme.accent
              : Colors.transparent,
          borderRadius: BorderRadius.circular(8),
        ),
        child: Text(
          '$label $count',
          maxLines: 1,
          style: TextStyle(
            fontSize: 9,
            fontWeight: FontWeight.w700,
            color: selected
                ? KinevorStaffDashboardTheme.onAccent
                : KinevorStaffDashboardTheme.textMuted,
          ),
        ),
      ),
    );
  }
}
