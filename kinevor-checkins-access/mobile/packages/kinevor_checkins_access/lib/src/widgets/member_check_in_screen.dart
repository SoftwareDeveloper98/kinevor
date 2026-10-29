import 'package:flutter/material.dart';

import '../data/check_ins_repository.dart';
import '../models/check_in_models.dart';
import '../theme/check_ins_strings.dart';
import '../theme/kinevor_check_ins_theme.dart';
import 'visit_formatters.dart';

/// Member self-service screen with access status, occupancy and visits.
class MemberCheckInScreen extends StatefulWidget {
  /// Creates the check-in screen for [repository].
  const MemberCheckInScreen({
    required this.repository,
    this.strings = const CheckInsStrings(),
    this.onOpenHistory,
    super.key,
  });

  /// Data source for status and visit actions.
  final CheckInsRepository repository;

  final CheckInsStrings strings;

  /// Optional callback that opens the visit-history tab.
  final VoidCallback? onOpenHistory;

  @override
  State<MemberCheckInScreen> createState() => _MemberCheckInScreenState();
}

class _MemberCheckInScreenState extends State<MemberCheckInScreen> {
  MemberCheckInStatus? _status;
  String? _error;
  bool _loading = true;
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load({bool showSpinner = true}) async {
    if (showSpinner) {
      setState(() {
        _loading = true;
        _error = null;
      });
    }
    try {
      final status = await widget.repository.getStatus();
      if (!mounted) return;
      setState(() {
        _status = status;
        _error = null;
        _loading = false;
      });
    } on Exception catch (error) {
      if (!mounted) return;
      setState(() {
        _error = error.toString();
        _loading = false;
      });
    }
  }

  Future<void> _checkIn() => _mutate(
    () => widget.repository.checkIn(),
    (visit) => widget.strings.checkedInAt(visit.branchName),
  );

  Future<void> _checkOut() => _mutate(
    () => widget.repository.checkOut(),
    (_) => widget.strings.checkedOut,
  );

  Future<void> _mutate(
    Future<CheckInVisit> Function() action,
    String Function(CheckInVisit visit) successMessage,
  ) async {
    setState(() => _busy = true);
    try {
      final visit = await action();
      final status = await widget.repository.getStatus();
      if (!mounted) return;
      setState(() => _status = status);
      _showMessage(successMessage(visit));
    } on Exception catch (error) {
      if (mounted) _showMessage(error.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  void _showMessage(String message) {
    if (!mounted) return;
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: KinevorCheckInsTheme.ink,
      appBar: AppBar(
        title: Text(widget.strings.checkInTitle),
        actions: [
          if (widget.onOpenHistory != null)
            IconButton(
              onPressed: widget.onOpenHistory,
              tooltip: widget.strings.historyTitle,
              icon: const Icon(Icons.history),
            ),
          IconButton(
            onPressed: _busy ? null : () => _load(showSpinner: false),
            tooltip: widget.strings.refresh,
            icon: const Icon(Icons.refresh),
          ),
          const SizedBox(width: 6),
        ],
      ),
      body: SafeArea(top: false, child: _body(context)),
    );
  }

  Widget _body(BuildContext context) {
    final theme = Theme.of(context);
    final status = _status;

    if (_loading && status == null) {
      return const Center(child: CircularProgressIndicator());
    }
    if (status == null) {
      return _ErrorState(
        message: _error ?? widget.strings.loadError,
        actionLabel: widget.strings.retry,
        onRetry: _load,
      );
    }

    return RefreshIndicator(
      onRefresh: () => _load(showSpinner: false),
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(18, 4, 18, 28),
        children: [
          _ProfileHeader(status: status, strings: widget.strings),
          const SizedBox(height: 12),
          _EligibilityBanner(
            eligibility: status.eligibility,
            strings: widget.strings,
          ),
          const SizedBox(height: 12),
          if (status.occupancy != null) ...[
            _OccupancyCard(
              occupancy: status.occupancy!,
              strings: widget.strings,
            ),
            const SizedBox(height: 14),
          ],
          if (status.openCheckIn != null)
            _ActiveVisitCard(
              visit: status.openCheckIn!,
              strings: widget.strings,
              busy: _busy,
              onCheckOut: _checkOut,
            )
          else
            FilledButton.icon(
              key: const ValueKey('check-in-button'),
              onPressed: status.eligibility.allowed && !_busy ? _checkIn : null,
              icon: _busy
                  ? const SizedBox.square(
                      dimension: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.login, size: 20),
              label: Text(
                status.eligibility.allowed
                    ? widget.strings.checkInAt(
                        status.eligibility.branchName ?? 'your club',
                      )
                    : widget.strings.accessUnavailable,
              ),
            ),
          if (status.lastVisit != null) ...[
            const SizedBox(height: 18),
            _LastVisitRow(visit: status.lastVisit!, strings: widget.strings),
          ],
          const SizedBox(height: 14),
          _MembershipRow(status: status, strings: widget.strings),
          if (_error != null) ...[
            const SizedBox(height: 14),
            Text(
              _error!,
              style: theme.textTheme.bodySmall?.copyWith(
                color: KinevorCheckInsTheme.error,
              ),
            ),
          ],
        ],
      ),
    );
  }
}

class _ProfileHeader extends StatelessWidget {
  const _ProfileHeader({required this.status, required this.strings});

  final MemberCheckInStatus status;
  final CheckInsStrings strings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final firstName = status.fullName.trim().split(RegExp(r'\s+')).first;

    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF2A160F), KinevorCheckInsTheme.surfaceRaised],
        ),
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: KinevorCheckInsTheme.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  strings.memberAccess.toUpperCase(),
                  style: theme.textTheme.labelSmall?.copyWith(
                    color: KinevorCheckInsTheme.accent,
                    letterSpacing: 1.6,
                  ),
                ),
              ),
              Container(
                width: 46,
                height: 46,
                alignment: Alignment.center,
                decoration: const BoxDecoration(
                  gradient: KinevorCheckInsTheme.brandGradient,
                  shape: BoxShape.circle,
                ),
                child: Text(
                  status.initials,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.w800,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            strings.welcome(firstName),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: theme.textTheme.headlineMedium,
          ),
          const SizedBox(height: 4),
          Text(
            '${status.memberNumber} · ${status.eligibility.membershipPlan}',
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: theme.textTheme.bodySmall?.copyWith(
              color: KinevorCheckInsTheme.textMuted,
            ),
          ),
        ],
      ),
    );
  }
}

class _EligibilityBanner extends StatelessWidget {
  const _EligibilityBanner({required this.eligibility, required this.strings});

  final CheckInEligibility eligibility;
  final CheckInsStrings strings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final color = eligibility.allowed
        ? KinevorCheckInsTheme.success
        : KinevorCheckInsTheme.error;

    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: color.withValues(alpha: 0.28)),
      ),
      child: Row(
        children: [
          Container(
            width: 40,
            height: 40,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.14),
              shape: BoxShape.circle,
            ),
            child: Icon(
              eligibility.allowed
                  ? Icons.verified_user_outlined
                  : Icons.block_outlined,
              color: color,
              size: 21,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  eligibility.allowed
                      ? strings.clearedToTrain
                      : strings.accessBlocked,
                  style: theme.textTheme.titleMedium?.copyWith(color: color),
                ),
                const SizedBox(height: 2),
                Text(
                  eligibility.message,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: KinevorCheckInsTheme.textMuted,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _OccupancyCard extends StatelessWidget {
  const _OccupancyCard({required this.occupancy, required this.strings});

  final BranchOccupancy occupancy;
  final CheckInsStrings strings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final color = KinevorCheckInsTheme.occupancyColor(occupancy.status);
    final percent = occupancy.utilizationPercent.clamp(0, 100) / 100;

    return Container(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 15),
      decoration: BoxDecoration(
        color: KinevorCheckInsTheme.surfaceRaised,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: KinevorCheckInsTheme.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Row(
                  children: [
                    const Icon(
                      Icons.groups_2_outlined,
                      size: 17,
                      color: KinevorCheckInsTheme.textMuted,
                    ),
                    const SizedBox(width: 7),
                    Flexible(
                      child: Text(
                        strings.liveOccupancy,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: KinevorCheckInsTheme.textMuted,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              _StatusPill(
                label: occupancy.status == 'Comfortable'
                    ? strings.comfortable
                    : occupancy.status,
                color: color,
              ),
            ],
          ),
          const SizedBox(height: 9),
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                '${occupancy.roundedUtilization}%',
                style: theme.textTheme.headlineMedium?.copyWith(
                  fontSize: 30,
                  fontWeight: FontWeight.w800,
                  height: 1,
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Padding(
                  padding: const EdgeInsets.only(bottom: 2),
                  child: Text(
                    strings.occupancyOf(occupancy.occupied, occupancy.capacity),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: KinevorCheckInsTheme.textMuted,
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          ClipRRect(
            borderRadius: BorderRadius.circular(99),
            child: LinearProgressIndicator(
              value: percent,
              minHeight: 7,
              color: color,
              backgroundColor: KinevorCheckInsTheme.border,
            ),
          ),
          const SizedBox(height: 9),
          Row(
            children: [
              Text(
                strings.spacesFree(occupancy.available),
                style: theme.textTheme.labelSmall?.copyWith(color: color),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  '${occupancy.branchName} · ${occupancy.city}',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.end,
                  style: theme.textTheme.labelSmall?.copyWith(
                    color: KinevorCheckInsTheme.textMuted,
                    fontWeight: FontWeight.w500,
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

class _ActiveVisitCard extends StatelessWidget {
  const _ActiveVisitCard({
    required this.visit,
    required this.strings,
    required this.busy,
    required this.onCheckOut,
  });

  final CheckInVisit visit;
  final CheckInsStrings strings;
  final bool busy;
  final VoidCallback onCheckOut;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: KinevorCheckInsTheme.activeGradient,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(
          color: KinevorCheckInsTheme.success.withValues(alpha: 0.34),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 38,
                height: 38,
                alignment: Alignment.center,
                decoration: BoxDecoration(
                  color: KinevorCheckInsTheme.success.withValues(alpha: 0.14),
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  Icons.fitness_center,
                  color: KinevorCheckInsTheme.success,
                  size: 19,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      strings.checkedIn,
                      style: theme.textTheme.titleMedium?.copyWith(
                        color: KinevorCheckInsTheme.success,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      strings.sinceAt(
                        formatClock(visit.checkedInAt),
                        visit.branchName,
                      ),
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: KinevorCheckInsTheme.textMuted,
                      ),
                    ),
                  ],
                ),
              ),
              Text(
                formatVisitDuration(visit.checkedInAt, visit.checkedOutAt),
                style: theme.textTheme.titleMedium,
              ),
            ],
          ),
          const SizedBox(height: 14),
          OutlinedButton.icon(
            key: const ValueKey('check-out-button'),
            onPressed: busy ? null : onCheckOut,
            style: OutlinedButton.styleFrom(
              minimumSize: const Size.fromHeight(44),
              foregroundColor: KinevorCheckInsTheme.text,
              side: BorderSide(
                color: KinevorCheckInsTheme.success.withValues(alpha: 0.4),
              ),
            ),
            icon: busy
                ? const SizedBox.square(
                    dimension: 16,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Icon(Icons.logout, size: 18),
            label: Text(strings.checkOut),
          ),
        ],
      ),
    );
  }
}

class _LastVisitRow extends StatelessWidget {
  const _LastVisitRow({required this.visit, required this.strings});

  final CheckInVisit visit;
  final CheckInsStrings strings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Row(
      children: [
        Container(
          width: 38,
          height: 38,
          alignment: Alignment.center,
          decoration: BoxDecoration(
            color: KinevorCheckInsTheme.surfaceRaised,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: KinevorCheckInsTheme.border),
          ),
          child: const Icon(
            Icons.history,
            size: 18,
            color: KinevorCheckInsTheme.textMuted,
          ),
        ),
        const SizedBox(width: 11),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                strings.lastVisit,
                style: theme.textTheme.labelSmall?.copyWith(
                  color: KinevorCheckInsTheme.textMuted,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                '${visit.branchName} · ${formatDay(visit.checkedInAt)}',
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: theme.textTheme.bodyMedium?.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ),
        Text(
          formatMinutes(visit.durationMinutes),
          style: theme.textTheme.bodySmall?.copyWith(
            color: KinevorCheckInsTheme.textMuted,
          ),
        ),
      ],
    );
  }
}

class _MembershipRow extends StatelessWidget {
  const _MembershipRow({required this.status, required this.strings});

  final MemberCheckInStatus status;
  final CheckInsStrings strings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 11),
      decoration: BoxDecoration(
        color: KinevorCheckInsTheme.surfaceRaised,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: KinevorCheckInsTheme.border),
      ),
      child: Row(
        children: [
          Container(
            width: 36,
            height: 36,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: KinevorCheckInsTheme.accent.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(11),
            ),
            child: const Icon(
              Icons.workspace_premium_outlined,
              size: 18,
              color: KinevorCheckInsTheme.accent,
            ),
          ),
          const SizedBox(width: 11),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  strings.membership,
                  style: theme.textTheme.labelSmall?.copyWith(
                    color: KinevorCheckInsTheme.textMuted,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  '${status.eligibility.membershipPlan} · ${status.membershipStatus}',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: theme.textTheme.bodyMedium?.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 8),
          Text(
            strings.visitsCount(status.visitsThisMonth),
            style: theme.textTheme.labelSmall?.copyWith(
              color: KinevorCheckInsTheme.textMuted,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}

class _StatusPill extends StatelessWidget {
  const _StatusPill({required this.label, required this.color});

  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(99),
      ),
      child: Text(
        label,
        style: Theme.of(
          context,
        ).textTheme.labelSmall?.copyWith(color: color, fontSize: 10),
      ),
    );
  }
}

class _ErrorState extends StatelessWidget {
  const _ErrorState({
    required this.message,
    required this.actionLabel,
    required this.onRetry,
  });

  final String message;
  final String actionLabel;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(28),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(
              Icons.cloud_off_outlined,
              size: 36,
              color: KinevorCheckInsTheme.textMuted,
            ),
            const SizedBox(height: 12),
            Text(
              message,
              textAlign: TextAlign.center,
              style: theme.textTheme.bodyMedium,
            ),
            const SizedBox(height: 16),
            OutlinedButton(onPressed: onRetry, child: Text(actionLabel)),
          ],
        ),
      ),
    );
  }
}
