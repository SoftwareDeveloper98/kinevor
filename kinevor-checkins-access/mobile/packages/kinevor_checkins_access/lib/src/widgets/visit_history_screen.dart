import 'package:flutter/material.dart';

import '../data/check_ins_repository.dart';
import '../models/check_in_models.dart';
import '../theme/check_ins_strings.dart';
import '../theme/kinevor_check_ins_theme.dart';
import 'visit_formatters.dart';

/// Member visit history with monthly summary and paging.
class VisitHistoryScreen extends StatefulWidget {
  /// Creates the history screen for [repository].
  const VisitHistoryScreen({
    required this.repository,
    this.strings = const CheckInsStrings(),
    this.pageSize = 8,
    super.key,
  });

  /// Data source for visits and the monthly summary.
  final CheckInsRepository repository;

  /// Localized labels.
  final CheckInsStrings strings;

  /// Number of visits requested per page.
  final int pageSize;

  @override
  State<VisitHistoryScreen> createState() => _VisitHistoryScreenState();
}

class _VisitHistoryScreenState extends State<VisitHistoryScreen> {
  final List<CheckInVisit> _visits = [];
  MemberCheckInStatus? _status;
  String? _error;
  bool _loading = true;
  bool _loadingMore = false;
  int _page = 1;
  int _totalPages = 0;
  int _totalCount = 0;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final page = await widget.repository.getVisits(
        page: 1,
        pageSize: widget.pageSize,
      );
      final status = await widget.repository.getStatus();
      if (!mounted) return;
      setState(() {
        _visits
          ..clear()
          ..addAll(page.items);
        _status = status;
        _page = page.page;
        _totalPages = page.totalPages;
        _totalCount = page.totalCount;
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

  Future<void> _loadMore() async {
    if (_loadingMore || _page >= _totalPages) return;
    setState(() => _loadingMore = true);
    try {
      final page = await widget.repository.getVisits(
        page: _page + 1,
        pageSize: widget.pageSize,
      );
      if (!mounted) return;
      setState(() {
        _visits.addAll(page.items);
        _page = page.page;
        _totalPages = page.totalPages;
        _totalCount = page.totalCount;
      });
    } on Exception catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context)
          ..hideCurrentSnackBar()
          ..showSnackBar(SnackBar(content: Text(error.toString())));
      }
    } finally {
      if (mounted) setState(() => _loadingMore = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: KinevorCheckInsTheme.ink,
      appBar: AppBar(
        title: Text(widget.strings.historyTitle),
        actions: [
          IconButton(
            onPressed: _loading ? null : _load,
            tooltip: widget.strings.refresh,
            icon: const Icon(Icons.refresh),
          ),
          const SizedBox(width: 6),
        ],
      ),
      body: SafeArea(top: false, child: _body()),
    );
  }

  Widget _body() {
    if (_loading && _visits.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_visits.isEmpty) {
      return _EmptyHistory(
        title: widget.strings.noVisits,
        detail: widget.strings.noVisitsDetail,
        error: _error,
        actionLabel: widget.strings.retry,
        onRetry: _load,
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(18, 4, 18, 28),
        children: [
          _ActivityHeader(
            status: _status,
            totalCount: _totalCount,
            strings: widget.strings,
          ),
          const SizedBox(height: 18),
          Row(
            children: [
              Text(
                widget.strings.allVisits,
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const Spacer(),
              Text(
                widget.strings.visitsCount(_totalCount),
                style: Theme.of(context).textTheme.labelSmall?.copyWith(
                  color: KinevorCheckInsTheme.textMuted,
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          for (final visit in _visits) ...[
            _VisitTile(visit: visit, strings: widget.strings),
            const SizedBox(height: 10),
          ],
          if (_page < _totalPages)
            OutlinedButton(
              onPressed: _loadingMore ? null : _loadMore,
              child: _loadingMore
                  ? const SizedBox.square(
                      dimension: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : Text(widget.strings.loadMore),
            ),
        ],
      ),
    );
  }
}

class _ActivityHeader extends StatelessWidget {
  const _ActivityHeader({
    required this.status,
    required this.totalCount,
    required this.strings,
  });

  final MemberCheckInStatus? status;
  final int totalCount;
  final CheckInsStrings strings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final count = status?.visitsThisMonth ?? totalCount;

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
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  strings.yourActivity.toUpperCase(),
                  style: theme.textTheme.labelSmall?.copyWith(
                    color: KinevorCheckInsTheme.accent,
                    letterSpacing: 1.6,
                  ),
                ),
                const SizedBox(height: 7),
                Row(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text(
                      '$count',
                      style: theme.textTheme.headlineMedium?.copyWith(
                        fontSize: 34,
                        fontWeight: FontWeight.w800,
                        height: 1,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Padding(
                      padding: const EdgeInsets.only(bottom: 2),
                      child: Text(
                        strings.visitsThisMonth,
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: KinevorCheckInsTheme.textMuted,
                        ),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(width: 12),
          Container(
            width: 48,
            height: 48,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: KinevorCheckInsTheme.accent.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(15),
            ),
            child: const Icon(
              Icons.calendar_month_outlined,
              color: KinevorCheckInsTheme.accent,
              size: 23,
            ),
          ),
        ],
      ),
    );
  }
}

class _VisitTile extends StatelessWidget {
  const _VisitTile({required this.visit, required this.strings});

  final CheckInVisit visit;
  final CheckInsStrings strings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final statusColor = visit.isOpen
        ? KinevorCheckInsTheme.success
        : KinevorCheckInsTheme.textMuted;
    final duration = visit.isOpen
        ? formatVisitDuration(visit.checkedInAt, null)
        : formatMinutes(visit.durationMinutes);
    final time = visit.isOpen
        ? '${strings.since} ${formatClock(visit.checkedInAt)}'
        : strings.timeRange(
            formatClock(visit.checkedInAt),
            formatClock(visit.checkedOutAt ?? visit.checkedInAt),
          );

    return Container(
      padding: const EdgeInsets.all(13),
      decoration: BoxDecoration(
        color: KinevorCheckInsTheme.surfaceRaised,
        borderRadius: BorderRadius.circular(17),
        border: Border.all(color: KinevorCheckInsTheme.border),
      ),
      child: Row(
        children: [
          Container(
            width: 48,
            height: 56,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: KinevorCheckInsTheme.surface,
              borderRadius: BorderRadius.circular(13),
              border: Border.all(color: KinevorCheckInsTheme.border),
            ),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(
                  '${visit.checkedInAt.day}',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.w800,
                    height: 1.1,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  monthShort(visit.checkedInAt).toUpperCase(),
                  style: theme.textTheme.labelSmall?.copyWith(
                    color: KinevorCheckInsTheme.textMuted,
                    fontSize: 9,
                    letterSpacing: 0.8,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        visit.branchName,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: theme.textTheme.titleMedium,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 8,
                        vertical: 3,
                      ),
                      decoration: BoxDecoration(
                        color: statusColor.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(99),
                      ),
                      child: Text(
                        visit.isOpen ? strings.inside : strings.completed,
                        style: theme.textTheme.labelSmall?.copyWith(
                          color: statusColor,
                          fontSize: 9,
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 4),
                Text(
                  time,
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: KinevorCheckInsTheme.textMuted,
                  ),
                ),
                const SizedBox(height: 5),
                Row(
                  children: [
                    Icon(
                      visit.source == 'MemberApp'
                          ? Icons.phone_iphone
                          : Icons.storefront_outlined,
                      size: 13,
                      color: KinevorCheckInsTheme.textMuted,
                    ),
                    const SizedBox(width: 4),
                    Text(
                      visit.source == 'MemberApp'
                          ? strings.memberApp
                          : strings.frontDesk,
                      style: theme.textTheme.labelSmall?.copyWith(
                        color: KinevorCheckInsTheme.textMuted,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Container(
                      width: 3,
                      height: 3,
                      decoration: const BoxDecoration(
                        color: KinevorCheckInsTheme.textMuted,
                        shape: BoxShape.circle,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      duration,
                      style: theme.textTheme.labelSmall?.copyWith(
                        color: KinevorCheckInsTheme.textMuted,
                        fontWeight: FontWeight.w500,
                      ),
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

class _EmptyHistory extends StatelessWidget {
  const _EmptyHistory({
    required this.title,
    required this.detail,
    required this.error,
    required this.actionLabel,
    required this.onRetry,
  });

  final String title;
  final String detail;
  final String? error;
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
            Icon(
              error == null
                  ? Icons.history_toggle_off
                  : Icons.cloud_off_outlined,
              size: 38,
              color: KinevorCheckInsTheme.textMuted,
            ),
            const SizedBox(height: 12),
            Text(title, style: theme.textTheme.titleLarge),
            const SizedBox(height: 5),
            Text(
              error ?? detail,
              textAlign: TextAlign.center,
              style: theme.textTheme.bodySmall?.copyWith(
                color: KinevorCheckInsTheme.textMuted,
              ),
            ),
            const SizedBox(height: 16),
            OutlinedButton(onPressed: onRetry, child: Text(actionLabel)),
          ],
        ),
      ),
    );
  }
}
