import 'package:flutter/material.dart';

import '../data/staff_dashboard_api_exception.dart';
import '../data/staff_dashboard_repository.dart';
import '../models/staff_dashboard_models.dart';
import '../theme/kinevor_staff_dashboard_theme.dart';
import '../theme/staff_dashboard_strings.dart';
import 'dashboard_alerts_screen.dart';
import 'dashboard_common.dart';

/// Staff dashboard with KPI grid, trend, activity and top alerts.
class StaffDashboardScreen extends StatefulWidget {
  /// Creates the staff dashboard.
  const StaffDashboardScreen({
    super.key,
    required this.repository,
    this.strings = const StaffDashboardStrings(),
    this.initialBranchId,
  });

  /// Data source used for dashboard, alert and branch reads.
  final StaffDashboardRepository repository;

  /// Localized labels.
  final StaffDashboardStrings strings;

  /// Optional branch selected on first load.
  final String? initialBranchId;

  @override
  State<StaffDashboardScreen> createState() => _StaffDashboardScreenState();
}

class _StaffDashboardScreenState extends State<StaffDashboardScreen> {
  StaffDashboard? _dashboard;
  bool _loading = true;
  String? _error;
  String? _branchId;
  int _days = 7;

  @override
  void initState() {
    super.initState();
    _branchId = widget.initialBranchId;
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final dashboard = await widget.repository.getDashboard(
        branchId: _branchId,
        days: _days,
      );
      if (!mounted) return;
      setState(() {
        _dashboard = dashboard;
        _days = dashboard.days;
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

  Future<void> _selectRange(int days) async {
    if (days == _days) return;
    setState(() => _days = days);
    await _load();
  }

  Future<void> _openAlerts() async {
    await Navigator.of(context).push<void>(
      MaterialPageRoute(
        builder: (_) => DashboardAlertsScreen(
          repository: widget.repository,
          strings: widget.strings,
          initialBranchId: _branchId,
          initialBranches: _dashboard?.branches ?? const [],
        ),
      ),
    );
    if (mounted) await _load();
  }

  int _count(String severity) =>
      _dashboard?.alerts.where((alert) => alert.severity == severity).length ??
      0;

  @override
  Widget build(BuildContext context) {
    final strings = widget.strings;
    final dashboard = _dashboard;
    final critical = _count('Critical');

    return Scaffold(
      appBar: AppBar(
        title: Text(strings.title),
        actions: [
          IconButton(
            tooltip: strings.alertsTitle,
            onPressed: dashboard == null ? null : _openAlerts,
            icon: Badge.count(
              count: critical,
              isLabelVisible: critical > 0,
              backgroundColor: KinevorStaffDashboardTheme.error,
              textColor: KinevorStaffDashboardTheme.onAccent,
              child: const Icon(Icons.notifications_none),
            ),
          ),
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
            if (_loading && dashboard == null)
              const Center(child: CircularProgressIndicator())
            else if (dashboard == null)
              DashboardErrorState(
                message: _error ?? strings.loadError,
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
                      _DashboardHero(
                        dashboard: dashboard,
                        strings: strings,
                        critical: critical,
                        onAlerts: _openAlerts,
                      ),
                      if (_error != null) ...[
                        const SizedBox(height: 10),
                        _InlineError(message: _error!),
                      ],
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          Expanded(
                            child: BranchSelector(
                              branches: dashboard.branches,
                              selectedBranchId: _branchId,
                              onSelected: _selectBranch,
                              strings: strings,
                            ),
                          ),
                          const SizedBox(width: 8),
                          RangeToggle(
                            days: _days,
                            onChanged: _selectRange,
                            strings: strings,
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      _KpiGrid(dashboard: dashboard, strings: strings),
                      const SizedBox(height: 12),
                      _TrendCard(dashboard: dashboard, strings: strings),
                      const SizedBox(height: 12),
                      _AlertsCard(
                        dashboard: dashboard,
                        strings: strings,
                        onOpen: _openAlerts,
                      ),
                      const SizedBox(height: 12),
                      _ActivityCard(dashboard: dashboard, strings: strings),
                    ],
                  ),
                ),
              ),
            if (_loading && dashboard != null)
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

class _DashboardHero extends StatelessWidget {
  const _DashboardHero({
    required this.dashboard,
    required this.strings,
    required this.critical,
    required this.onAlerts,
  });

  final StaffDashboard dashboard;
  final StaffDashboardStrings strings;
  final int critical;
  final VoidCallback onAlerts;

  @override
  Widget build(BuildContext context) {
    return Container(
      key: const ValueKey<String>('dashboard-hero'),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF2B1B18), KinevorStaffDashboardTheme.surfaceRaised],
        ),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: KinevorStaffDashboardTheme.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Row(
            children: [
              _LiveDot(),
              SizedBox(width: 6),
              Text(
                'EXECUTIVE PULSE',
                style: TextStyle(
                  fontSize: 8.5,
                  fontWeight: FontWeight.w800,
                  letterSpacing: 1.3,
                  color: KinevorStaffDashboardTheme.accent,
                ),
              ),
            ],
          ),
          const SizedBox(height: 4),
          Text(
            strings.title,
            style: const TextStyle(
              fontSize: 22,
              fontWeight: FontWeight.w800,
              height: 1.1,
              letterSpacing: -0.5,
            ),
          ),
          const SizedBox(height: 3),
          Text(
            '${dashboard.branchName} - ${dashboard.branchCity}',
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              fontSize: 11,
              color: KinevorStaffDashboardTheme.textMuted,
            ),
          ),
          const SizedBox(height: 9),
          Wrap(
            spacing: 5,
            runSpacing: 5,
            children: [
              _HeroChip(
                icon: Icons.storefront_outlined,
                label: strings.branchChip(dashboard.branches.length),
              ),
              _HeroChip(
                icon: Icons.groups_2_outlined,
                label:
                    '${strings.number(dashboard.summary.activeMembers)} '
                    '${strings.activeMembers.toLowerCase()}',
              ),
              _HeroChip(
                icon: Icons.schedule,
                label:
                    '${strings.updated} '
                    '${strings.time(dashboard.generatedAt)}',
              ),
              if (critical > 0)
                _HeroChip(
                  icon: Icons.priority_high,
                  label: strings.criticalChip(critical),
                  color: KinevorStaffDashboardTheme.error,
                  onTap: onAlerts,
                ),
            ],
          ),
        ],
      ),
    );
  }
}

class _LiveDot extends StatelessWidget {
  const _LiveDot();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 6,
      height: 6,
      decoration: BoxDecoration(
        color: KinevorStaffDashboardTheme.accent,
        shape: BoxShape.circle,
        boxShadow: [
          BoxShadow(
            color: KinevorStaffDashboardTheme.accent.withValues(alpha: 0.35),
            blurRadius: 6,
            spreadRadius: 2,
          ),
        ],
      ),
    );
  }
}

class _HeroChip extends StatelessWidget {
  const _HeroChip({
    required this.icon,
    required this.label,
    this.color = KinevorStaffDashboardTheme.textMuted,
    this.onTap,
  });

  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final chip = Container(
      padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 4),
      decoration: BoxDecoration(
        color: color == KinevorStaffDashboardTheme.textMuted
            ? KinevorStaffDashboardTheme.ink.withValues(alpha: 0.45)
            : color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(999),
        border: Border.all(
          color: color == KinevorStaffDashboardTheme.textMuted
              ? KinevorStaffDashboardTheme.border
              : color.withValues(alpha: 0.32),
        ),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 11, color: color),
          const SizedBox(width: 4),
          Text(
            label,
            style: TextStyle(
              fontSize: 9,
              fontWeight: FontWeight.w700,
              color: color,
            ),
          ),
        ],
      ),
    );

    if (onTap == null) return chip;
    return GestureDetector(onTap: onTap, child: chip);
  }
}

class _InlineError extends StatelessWidget {
  const _InlineError({required this.message});

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

class _KpiGrid extends StatelessWidget {
  const _KpiGrid({required this.dashboard, required this.strings});

  final StaffDashboard dashboard;
  final StaffDashboardStrings strings;

  @override
  Widget build(BuildContext context) {
    final summary = dashboard.summary;
    final cards = [
      _KpiCard(
        label: strings.activeMembers,
        value: strings.number(summary.activeMembers),
        icon: Icons.how_to_reg_outlined,
        accent: KinevorStaffDashboardTheme.warm,
        foot: _DeltaFoot(
          value: summary.comparison.activeMembersPercent,
          strings: strings,
        ),
      ),
      _KpiCard(
        label: strings.revenueMonth,
        value: strings.currency(summary.revenueMonth),
        icon: Icons.payments_outlined,
        accent: KinevorStaffDashboardTheme.cool,
        foot: _DeltaFoot(
          value: summary.comparison.revenuePercent,
          strings: strings,
        ),
      ),
      _KpiCard(
        label: strings.occupancyNow,
        value: strings.percent(summary.occupancyPercent),
        icon: Icons.sensors,
        accent: KinevorStaffDashboardTheme.violet,
        foot: _FootText(
          '${summary.occupancyNow} ${strings.inside} - '
          '${strings.of} ${summary.capacity}',
        ),
      ),
      _KpiCard(
        label: strings.checkInsToday,
        value: strings.number(summary.checkInsToday),
        icon: Icons.login,
        accent: KinevorStaffDashboardTheme.teal,
        foot: _FootText(
          '${strings.number(dashboard.totalCheckIns)} in ${dashboard.days} days',
        ),
      ),
      _KpiCard(
        label: strings.bookingsToday,
        value: strings.number(summary.bookingsToday),
        icon: Icons.event_available_outlined,
        accent: KinevorStaffDashboardTheme.blue,
        foot: _FootText(
          '${strings.number(dashboard.totalBookings)} in ${dashboard.days} days',
        ),
      ),
      _KpiCard(
        label: strings.outstandingInvoices,
        value: strings.currency(summary.outstandingAmount),
        icon: Icons.receipt_long_outlined,
        accent: KinevorStaffDashboardTheme.warning,
        foot: _WarningFoot(
          text:
              '${summary.outstandingInvoices} ${strings.overdue} - '
              '${strings.followUp}',
        ),
      ),
    ];

    return LayoutBuilder(
      builder: (context, constraints) {
        final width = (constraints.maxWidth - 10) / 2;
        return Wrap(
          spacing: 10,
          runSpacing: 10,
          children: [
            for (final card in cards) SizedBox(width: width, child: card),
          ],
        );
      },
    );
  }
}

class _KpiCard extends StatelessWidget {
  const _KpiCard({
    required this.label,
    required this.value,
    required this.icon,
    required this.accent,
    required this.foot,
  });

  final String label;
  final String value;
  final IconData icon;
  final Color accent;
  final Widget foot;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 102,
      padding: const EdgeInsets.fromLTRB(11, 10, 11, 10),
      decoration: BoxDecoration(
        color: KinevorStaffDashboardTheme.surfaceRaised,
        borderRadius: BorderRadius.circular(15),
        border: Border.all(color: KinevorStaffDashboardTheme.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Text(
                  label.toUpperCase(),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 8,
                    fontWeight: FontWeight.w700,
                    letterSpacing: 0.5,
                    color: KinevorStaffDashboardTheme.textMuted,
                  ),
                ),
              ),
              const SizedBox(width: 4),
              Container(
                width: 22,
                height: 22,
                decoration: BoxDecoration(
                  color: accent.withValues(alpha: 0.13),
                  borderRadius: BorderRadius.circular(7),
                ),
                alignment: Alignment.center,
                child: Icon(icon, size: 13, color: accent),
              ),
            ],
          ),
          const Spacer(),
          Text(
            value,
            maxLines: 1,
            style: const TextStyle(
              fontSize: 22,
              fontWeight: FontWeight.w800,
              height: 1,
              letterSpacing: -0.5,
            ),
          ),
          const SizedBox(height: 4),
          foot,
        ],
      ),
    );
  }
}

class _DeltaFoot extends StatelessWidget {
  const _DeltaFoot({required this.value, required this.strings});

  final double value;
  final StaffDashboardStrings strings;

  @override
  Widget build(BuildContext context) {
    final negative = value < 0;
    final color = negative
        ? KinevorStaffDashboardTheme.error
        : KinevorStaffDashboardTheme.success;
    return Row(
      children: [
        Icon(
          negative ? Icons.trending_down : Icons.trending_up,
          size: 11,
          color: color,
        ),
        const SizedBox(width: 2),
        Text(
          strings.delta(value),
          style: TextStyle(
            fontSize: 9,
            fontWeight: FontWeight.w800,
            color: color,
          ),
        ),
        const SizedBox(width: 3),
        Expanded(
          child: Text(
            strings.vsLastMonth,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              fontSize: 8,
              color: KinevorStaffDashboardTheme.textMuted,
            ),
          ),
        ),
      ],
    );
  }
}

class _FootText extends StatelessWidget {
  const _FootText(this.text);

  final String text;

  @override
  Widget build(BuildContext context) {
    return Text(
      text,
      maxLines: 1,
      overflow: TextOverflow.ellipsis,
      style: const TextStyle(
        fontSize: 8.5,
        fontWeight: FontWeight.w600,
        color: KinevorStaffDashboardTheme.textMuted,
      ),
    );
  }
}

class _WarningFoot extends StatelessWidget {
  const _WarningFoot({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        const Icon(
          Icons.notification_important_outlined,
          size: 10,
          color: KinevorStaffDashboardTheme.warning,
        ),
        const SizedBox(width: 2),
        Expanded(
          child: Text(
            text,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              fontSize: 8.5,
              fontWeight: FontWeight.w600,
              color: KinevorStaffDashboardTheme.warning,
            ),
          ),
        ),
      ],
    );
  }
}

class _TrendCard extends StatelessWidget {
  const _TrendCard({required this.dashboard, required this.strings});

  final StaffDashboard dashboard;
  final StaffDashboardStrings strings;

  @override
  Widget build(BuildContext context) {
    final points = dashboard.trend;
    return Card(
      key: const ValueKey<String>('trend-card'),
      child: Padding(
        padding: const EdgeInsets.fromLTRB(14, 14, 14, 12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        strings.trendEyebrow,
                        style: const TextStyle(
                          fontSize: 8,
                          fontWeight: FontWeight.w800,
                          letterSpacing: 1.2,
                          color: KinevorStaffDashboardTheme.accent,
                        ),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        strings.trendTitle,
                        style: const TextStyle(
                          fontSize: 15,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),
                ),
                _Legend(
                  color: KinevorStaffDashboardTheme.warm,
                  label: strings.checkIns,
                ),
                const SizedBox(width: 8),
                _Legend(
                  color: KinevorStaffDashboardTheme.cool,
                  label: strings.bookings,
                ),
              ],
            ),
            const SizedBox(height: 10),
            if (points.isEmpty)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 24),
                child: Text(
                  strings.noTrend,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    fontSize: 11,
                    color: KinevorStaffDashboardTheme.textMuted,
                  ),
                ),
              )
            else ...[
              SizedBox(
                height: 96,
                child: _TrendChart(points: points, days: dashboard.days),
              ),
              if (dashboard.days > 7) ...[
                const SizedBox(height: 4),
                _WindowLabels(points: points),
              ],
              const SizedBox(height: 8),
              Row(
                children: [
                  Expanded(
                    child: _TrendFoot(
                      value: strings.number(dashboard.totalCheckIns),
                      label: strings.checkIns.toLowerCase(),
                    ),
                  ),
                  Expanded(
                    child: _TrendFoot(
                      value: strings.number(dashboard.totalBookings),
                      label: strings.bookings.toLowerCase(),
                    ),
                  ),
                  Expanded(
                    child: _TrendFoot(
                      value: '${dashboard.days}',
                      label: strings.windowSuffix,
                    ),
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _Legend extends StatelessWidget {
  const _Legend({required this.color, required this.label});

  final Color color;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 6,
          height: 6,
          decoration: BoxDecoration(
            color: color,
            borderRadius: BorderRadius.circular(2),
          ),
        ),
        const SizedBox(width: 3),
        Text(
          label,
          style: const TextStyle(
            fontSize: 8,
            color: KinevorStaffDashboardTheme.textMuted,
          ),
        ),
      ],
    );
  }
}

class _TrendChart extends StatelessWidget {
  const _TrendChart({required this.points, required this.days});

  final List<DashboardTrendPoint> points;
  final int days;

  @override
  Widget build(BuildContext context) {
    var max = 1;
    for (final point in points) {
      if (point.checkIns > max) max = point.checkIns;
      if (point.bookings > max) max = point.bookings;
    }

    return Row(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        for (var index = 0; index < points.length; index++)
          Expanded(
            child: Padding(
              padding: EdgeInsets.symmetric(horizontal: days <= 7 ? 2.5 : 0.6),
              child: Column(
                children: [
                  Expanded(
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.end,
                      children: [
                        Expanded(
                          child: _TrendBar(
                            value: points[index].checkIns,
                            max: max,
                            gradient: KinevorStaffDashboardTheme.checkInBars,
                          ),
                        ),
                        const SizedBox(width: 2),
                        Expanded(
                          child: _TrendBar(
                            value: points[index].bookings,
                            max: max,
                            gradient: KinevorStaffDashboardTheme.bookingBars,
                          ),
                        ),
                      ],
                    ),
                  ),
                  if (days <= 7) ...[
                    const SizedBox(height: 3),
                    Text(
                      points[index].label,
                      maxLines: 1,
                      overflow: TextOverflow.clip,
                      style: const TextStyle(
                        fontSize: 7,
                        color: KinevorStaffDashboardTheme.textMuted,
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ),
      ],
    );
  }
}

class _TrendBar extends StatelessWidget {
  const _TrendBar({
    required this.value,
    required this.max,
    required this.gradient,
  });

  final int value;
  final int max;
  final LinearGradient gradient;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final ratio = (value / max).clamp(0.05, 1.0).toDouble();
        return Align(
          alignment: Alignment.bottomCenter,
          child: Container(
            height: constraints.maxHeight * ratio,
            decoration: BoxDecoration(
              gradient: gradient,
              borderRadius: const BorderRadius.vertical(
                top: Radius.circular(2),
              ),
            ),
          ),
        );
      },
    );
  }
}

class _WindowLabels extends StatelessWidget {
  const _WindowLabels({required this.points});

  final List<DashboardTrendPoint> points;

  @override
  Widget build(BuildContext context) {
    final middle = points[points.length ~/ 2];
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        _WindowLabel(points.first.label),
        _WindowLabel(middle.label),
        _WindowLabel(points.last.label),
      ],
    );
  }
}

class _WindowLabel extends StatelessWidget {
  const _WindowLabel(this.label);

  final String label;

  @override
  Widget build(BuildContext context) {
    return Text(
      label,
      style: const TextStyle(
        fontSize: 7,
        color: KinevorStaffDashboardTheme.textMuted,
      ),
    );
  }
}

class _TrendFoot extends StatelessWidget {
  const _TrendFoot({required this.value, required this.label});

  final String value;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Text(
          value,
          style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w800),
        ),
        const SizedBox(width: 3),
        Expanded(
          child: Text(
            label,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              fontSize: 8.5,
              color: KinevorStaffDashboardTheme.textMuted,
            ),
          ),
        ),
      ],
    );
  }
}

class _AlertsCard extends StatelessWidget {
  const _AlertsCard({
    required this.dashboard,
    required this.strings,
    required this.onOpen,
  });

  final StaffDashboard dashboard;
  final StaffDashboardStrings strings;
  final VoidCallback onOpen;

  int _count(String severity) =>
      dashboard.alerts.where((alert) => alert.severity == severity).length;

  @override
  Widget build(BuildContext context) {
    final alerts = dashboard.alerts.take(3).toList();
    return Card(
      key: const ValueKey<String>('alerts-card'),
      child: Padding(
        padding: const EdgeInsets.fromLTRB(14, 14, 14, 14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        strings.alertsEyebrow,
                        style: const TextStyle(
                          fontSize: 8,
                          fontWeight: FontWeight.w800,
                          letterSpacing: 1.2,
                          color: KinevorStaffDashboardTheme.accent,
                        ),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        strings.alertsTitle,
                        style: const TextStyle(
                          fontSize: 15,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),
                ),
                TextButton(
                  key: const ValueKey<String>('view-inbox'),
                  onPressed: onOpen,
                  style: TextButton.styleFrom(
                    minimumSize: const Size(0, 28),
                    padding: const EdgeInsets.symmetric(horizontal: 6),
                    foregroundColor: KinevorStaffDashboardTheme.accent,
                    textStyle: const TextStyle(
                      fontSize: 10,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  child: Text(strings.viewInbox),
                ),
              ],
            ),
            const SizedBox(height: 8),
            SeveritySummary(
              critical: _count('Critical'),
              warning: _count('Warning'),
              info: _count('Info'),
              strings: strings,
            ),
            const SizedBox(height: 10),
            if (alerts.isEmpty)
              Text(
                strings.noAlerts,
                style: const TextStyle(
                  fontSize: 10.5,
                  color: KinevorStaffDashboardTheme.textMuted,
                ),
              )
            else
              for (var index = 0; index < alerts.length; index++) ...[
                AlertCard(
                  key: ValueKey<String>('alert-${alerts[index].id}'),
                  alert: alerts[index],
                  reference: dashboard.generatedAt,
                  strings: strings,
                  compact: true,
                ),
                if (index != alerts.length - 1) const SizedBox(height: 7),
              ],
          ],
        ),
      ),
    );
  }
}

class _ActivityCard extends StatelessWidget {
  const _ActivityCard({required this.dashboard, required this.strings});

  final StaffDashboard dashboard;
  final StaffDashboardStrings strings;

  @override
  Widget build(BuildContext context) {
    final activity = dashboard.activity;
    return Card(
      key: const ValueKey<String>('activity-card'),
      child: Padding(
        padding: const EdgeInsets.fromLTRB(14, 14, 14, 8),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        strings.activityEyebrow,
                        style: const TextStyle(
                          fontSize: 8,
                          fontWeight: FontWeight.w800,
                          letterSpacing: 1.2,
                          color: KinevorStaffDashboardTheme.accent,
                        ),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        strings.activityTitle,
                        style: const TextStyle(
                          fontSize: 15,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),
                ),
                Text(
                  strings.activityCount(activity.length),
                  style: const TextStyle(
                    fontSize: 8.5,
                    color: KinevorStaffDashboardTheme.textMuted,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 6),
            if (activity.isEmpty)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 10),
                child: Text(
                  strings.noActivity,
                  style: const TextStyle(
                    fontSize: 10.5,
                    color: KinevorStaffDashboardTheme.textMuted,
                  ),
                ),
              )
            else
              for (var index = 0; index < activity.length; index++) ...[
                _ActivityRow(activity: activity[index], strings: strings),
                if (index != activity.length - 1) const Divider(),
              ],
          ],
        ),
      ),
    );
  }
}

class _ActivityRow extends StatelessWidget {
  const _ActivityRow({required this.activity, required this.strings});

  final DashboardActivity activity;
  final StaffDashboardStrings strings;

  @override
  Widget build(BuildContext context) {
    final color = KinevorStaffDashboardTheme.activityColor(activity.kind);
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: 30,
            height: 30,
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(9),
            ),
            alignment: Alignment.center,
            child: Icon(
              KinevorStaffDashboardTheme.activityIcon(activity.kind),
              size: 15,
              color: color,
            ),
          ),
          const SizedBox(width: 9),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  activity.title,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 1),
                Text(
                  activity.detail,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 9,
                    color: KinevorStaffDashboardTheme.textMuted,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 6),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                strings.time(activity.occurredAt),
                style: const TextStyle(
                  fontSize: 9.5,
                  fontWeight: FontWeight.w700,
                ),
              ),
              const SizedBox(height: 1),
              SizedBox(
                width: 92,
                child: Text(
                  strings.branchName(activity.branchName),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.end,
                  style: const TextStyle(
                    fontSize: 8,
                    color: KinevorStaffDashboardTheme.textMuted,
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
