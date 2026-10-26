import 'package:flutter/material.dart';

import '../models/staff_dashboard_models.dart';
import '../theme/kinevor_staff_dashboard_theme.dart';
import '../theme/staff_dashboard_strings.dart';

/// Compact horizontal branch selector.
class BranchSelector extends StatelessWidget {
  /// Creates a branch selector.
  const BranchSelector({
    super.key,
    required this.branches,
    required this.selectedBranchId,
    required this.onSelected,
    required this.strings,
  });

  /// Branch options.
  final List<DashboardBranch> branches;

  /// Selected branch, or null for all branches.
  final String? selectedBranchId;

  /// Called when the user selects a branch.
  final ValueChanged<String?> onSelected;

  /// Localized labels.
  final StaffDashboardStrings strings;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 34,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        physics: const BouncingScrollPhysics(),
        itemCount: branches.length + 1,
        separatorBuilder: (_, _) => const SizedBox(width: 5),
        itemBuilder: (context, index) {
          final branch = index == 0 ? null : branches[index - 1];
          final id = branch?.id;
          final selected = id == selectedBranchId;
          return _BranchChip(
            key: ValueKey<String>('branch-${id ?? 'all'}'),
            label: branch == null
                ? strings.allBranches
                : strings.branchName(branch.name),
            selected: selected,
            onTap: () => onSelected(id),
          );
        },
      ),
    );
  }
}

class _BranchChip extends StatelessWidget {
  const _BranchChip({
    super.key,
    required this.label,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final color = selected
        ? KinevorStaffDashboardTheme.accent
        : KinevorStaffDashboardTheme.textMuted;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 10),
        alignment: Alignment.center,
        decoration: BoxDecoration(
          color: selected
              ? KinevorStaffDashboardTheme.accent.withValues(alpha: 0.14)
              : KinevorStaffDashboardTheme.surfaceRaised,
          borderRadius: BorderRadius.circular(999),
          border: Border.all(
            color: selected
                ? KinevorStaffDashboardTheme.accent.withValues(alpha: 0.55)
                : KinevorStaffDashboardTheme.border,
          ),
        ),
        child: Text(
          label,
          maxLines: 1,
          style: TextStyle(
            fontSize: 11,
            fontWeight: FontWeight.w700,
            color: color,
          ),
        ),
      ),
    );
  }
}

/// Seven- and thirty-day range switch.
class RangeToggle extends StatelessWidget {
  /// Creates a range toggle.
  const RangeToggle({
    super.key,
    required this.days,
    required this.onChanged,
    required this.strings,
  });

  /// Active range in days.
  final int days;

  /// Called with the selected range.
  final ValueChanged<int> onChanged;

  /// Localized labels.
  final StaffDashboardStrings strings;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(3),
      decoration: BoxDecoration(
        color: KinevorStaffDashboardTheme.surfaceRaised,
        borderRadius: BorderRadius.circular(11),
        border: Border.all(color: KinevorStaffDashboardTheme.border),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          _RangeButton(
            key: const ValueKey<String>('range-7'),
            label: strings.range7,
            selected: days == 7,
            onTap: () => onChanged(7),
          ),
          _RangeButton(
            key: const ValueKey<String>('range-30'),
            label: strings.range30,
            selected: days == 30,
            onTap: () => onChanged(30),
          ),
        ],
      ),
    );
  }
}

class _RangeButton extends StatelessWidget {
  const _RangeButton({
    super.key,
    required this.label,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        height: 26,
        padding: const EdgeInsets.symmetric(horizontal: 8),
        alignment: Alignment.center,
        decoration: BoxDecoration(
          color: selected
              ? KinevorStaffDashboardTheme.accent
              : Colors.transparent,
          borderRadius: BorderRadius.circular(8),
        ),
        child: Text(
          label,
          style: TextStyle(
            fontSize: 10,
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

/// Three-tile alert severity summary with optional filtering.
class SeveritySummary extends StatelessWidget {
  /// Creates a severity summary.
  const SeveritySummary({
    super.key,
    required this.critical,
    required this.warning,
    required this.info,
    required this.strings,
    this.selected,
    this.onSelected,
  });

  /// Number of critical alerts.
  final int critical;

  /// Number of warning alerts.
  final int warning;

  /// Number of info alerts.
  final int info;

  /// Localized labels.
  final StaffDashboardStrings strings;

  /// Selected severity filter.
  final String? selected;

  /// Called when a tile is tapped.
  final ValueChanged<String>? onSelected;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: _SeverityTile(
            severity: 'Critical',
            label: strings.critical,
            hint: strings.actToday,
            count: critical,
            strings: strings,
            selected: selected == 'Critical',
            onTap: onSelected,
          ),
        ),
        const SizedBox(width: 7),
        Expanded(
          child: _SeverityTile(
            severity: 'Warning',
            label: strings.warning,
            hint: strings.thisShift,
            count: warning,
            strings: strings,
            selected: selected == 'Warning',
            onTap: onSelected,
          ),
        ),
        const SizedBox(width: 7),
        Expanded(
          child: _SeverityTile(
            severity: 'Info',
            label: strings.info,
            hint: strings.followUpHint,
            count: info,
            strings: strings,
            selected: selected == 'Info',
            onTap: onSelected,
          ),
        ),
      ],
    );
  }
}

class _SeverityTile extends StatelessWidget {
  const _SeverityTile({
    required this.severity,
    required this.label,
    required this.hint,
    required this.count,
    required this.strings,
    required this.selected,
    required this.onTap,
  });

  final String severity;
  final String label;
  final String hint;
  final int count;
  final StaffDashboardStrings strings;
  final bool selected;
  final ValueChanged<String>? onTap;

  @override
  Widget build(BuildContext context) {
    final color = KinevorStaffDashboardTheme.severityColor(severity);
    final content = Container(
      padding: const EdgeInsets.fromLTRB(10, 8, 10, 8),
      decoration: BoxDecoration(
        color: color.withValues(alpha: selected ? 0.16 : 0.08),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: color.withValues(alpha: selected ? 0.55 : 0.26),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                KinevorStaffDashboardTheme.severityIcon(severity),
                size: 12,
                color: color,
              ),
              const SizedBox(width: 4),
              Expanded(
                child: Text(
                  label.toUpperCase(),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    fontSize: 8,
                    fontWeight: FontWeight.w800,
                    letterSpacing: 0.6,
                    color: color,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 3),
          Text(
            strings.number(count),
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.w800,
              height: 1,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            hint,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              fontSize: 8.5,
              color: KinevorStaffDashboardTheme.textMuted,
            ),
          ),
        ],
      ),
    );

    if (onTap == null) return content;
    return GestureDetector(onTap: () => onTap!(severity), child: content);
  }
}

/// One alert or task row shared by the overview and inbox.
class AlertCard extends StatelessWidget {
  /// Creates an alert card.
  const AlertCard({
    super.key,
    required this.alert,
    required this.reference,
    required this.strings,
    this.compact = false,
  });

  /// Alert to render.
  final DashboardAlert alert;

  /// Timestamp used to decide whether the task is overdue.
  final DateTime reference;

  /// Localized labels.
  final StaffDashboardStrings strings;

  /// Whether to render the denser overview variant.
  final bool compact;

  @override
  Widget build(BuildContext context) {
    final color = KinevorStaffDashboardTheme.severityColor(alert.severity);
    final overdue = alert.isOverdue(reference);
    final due = alert.dueAt;
    final dueText = due == null
        ? '${strings.due} ${strings.anytime}'
        : overdue
        ? '${strings.overdueStatus} - ${strings.dateTime(due)}'
        : '${strings.due} ${strings.dateTime(due)}';

    return Container(
      padding: EdgeInsets.all(compact ? 10 : 12),
      decoration: BoxDecoration(
        color: KinevorStaffDashboardTheme.surface,
        borderRadius: BorderRadius.circular(13),
        border: Border.all(color: color.withValues(alpha: 0.24)),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: compact ? 30 : 34,
            height: compact ? 30 : 34,
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(9),
            ),
            alignment: Alignment.center,
            child: Icon(
              KinevorStaffDashboardTheme.alertTypeIcon(alert.type),
              size: compact ? 15 : 17,
              color: color,
            ),
          ),
          const SizedBox(width: 9),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Wrap(
                  spacing: 5,
                  runSpacing: 3,
                  crossAxisAlignment: WrapCrossAlignment.center,
                  children: [
                    _Tag(
                      label: strings.severityLabel(alert.severity),
                      color: color,
                      filled: true,
                    ),
                    _Tag(
                      label: strings.alertTypeLabel(alert.type),
                      color: KinevorStaffDashboardTheme.textMuted,
                    ),
                    if (!compact && alert.reference.isNotEmpty)
                      _Tag(
                        label: alert.reference,
                        color: KinevorStaffDashboardTheme.textMuted,
                        monospaced: true,
                      ),
                  ],
                ),
                const SizedBox(height: 3),
                Text(
                  alert.title,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    fontSize: compact ? 11.5 : 13,
                    fontWeight: FontWeight.w700,
                    height: 1.25,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  alert.detail,
                  maxLines: compact ? 1 : 2,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    fontSize: compact ? 9.5 : 10.5,
                    color: KinevorStaffDashboardTheme.textMuted,
                    height: 1.35,
                  ),
                ),
                const SizedBox(height: 4),
                Wrap(
                  spacing: 10,
                  runSpacing: 2,
                  children: [
                    _MetaText(
                      icon: Icons.storefront_outlined,
                      text: compact
                          ? strings.branchName(alert.branchName)
                          : '${alert.branchName} - ${alert.branchCity}',
                    ),
                    _MetaText(
                      icon: Icons.schedule,
                      text: dueText,
                      color: overdue
                          ? KinevorStaffDashboardTheme.error
                          : KinevorStaffDashboardTheme.textMuted,
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _Tag extends StatelessWidget {
  const _Tag({
    required this.label,
    required this.color,
    this.filled = false,
    this.monospaced = false,
  });

  final String label;
  final Color color;
  final bool filled;
  final bool monospaced;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: filled
            ? color.withValues(alpha: 0.13)
            : KinevorStaffDashboardTheme.ink.withValues(alpha: 0.5),
        borderRadius: BorderRadius.circular(999),
      ),
      child: Text(
        monospaced ? label : label.toUpperCase(),
        style: TextStyle(
          fontSize: 7.5,
          fontWeight: FontWeight.w800,
          letterSpacing: monospaced ? 0 : 0.5,
          color: color,
        ),
      ),
    );
  }
}

class _MetaText extends StatelessWidget {
  const _MetaText({
    required this.icon,
    required this.text,
    this.color = KinevorStaffDashboardTheme.textMuted,
  });

  final IconData icon;
  final String text;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 10, color: color),
        const SizedBox(width: 3),
        Text(
          text,
          style: TextStyle(
            fontSize: 8.5,
            fontWeight: FontWeight.w600,
            color: color,
          ),
        ),
      ],
    );
  }
}

/// Full-screen error with a retry action.
class DashboardErrorState extends StatelessWidget {
  /// Creates an error state.
  const DashboardErrorState({
    super.key,
    required this.message,
    required this.retryLabel,
    required this.onRetry,
  });

  /// Human-readable failure.
  final String message;

  /// Retry action label.
  final String retryLabel;

  /// Retry callback.
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(28),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(
              Icons.error_outline,
              size: 34,
              color: KinevorStaffDashboardTheme.error,
            ),
            const SizedBox(height: 12),
            Text(
              message,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                color: KinevorStaffDashboardTheme.textMuted,
              ),
            ),
            const SizedBox(height: 14),
            FilledButton(onPressed: onRetry, child: Text(retryLabel)),
          ],
        ),
      ),
    );
  }
}

/// Card used when a dashboard section has no rows.
class DashboardEmptyState extends StatelessWidget {
  /// Creates an empty state card.
  const DashboardEmptyState({
    super.key,
    required this.icon,
    required this.title,
    required this.message,
    this.actionLabel,
    this.onAction,
  });

  /// Illustration icon.
  final IconData icon;

  /// Empty-state headline.
  final String title;

  /// Supporting message.
  final String message;

  /// Optional action label.
  final String? actionLabel;

  /// Optional action callback.
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            Container(
              width: 46,
              height: 46,
              decoration: BoxDecoration(
                color: KinevorStaffDashboardTheme.success.withValues(
                  alpha: 0.1,
                ),
                borderRadius: BorderRadius.circular(14),
              ),
              alignment: Alignment.center,
              child: Icon(
                icon,
                size: 24,
                color: KinevorStaffDashboardTheme.success,
              ),
            ),
            const SizedBox(height: 10),
            Text(title, style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 4),
            Text(
              message,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                color: KinevorStaffDashboardTheme.textMuted,
              ),
            ),
            if (actionLabel != null && onAction != null) ...[
              const SizedBox(height: 12),
              OutlinedButton(onPressed: onAction, child: Text(actionLabel!)),
            ],
          ],
        ),
      ),
    );
  }
}
