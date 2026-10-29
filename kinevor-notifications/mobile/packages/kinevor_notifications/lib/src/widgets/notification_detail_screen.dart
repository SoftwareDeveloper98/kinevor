import 'package:flutter/material.dart';

import '../data/notifications_api_exception.dart';
import '../data/notifications_repository.dart';
import '../models/notification_models.dart';
import '../theme/kinevor_notifications_theme.dart';
import '../theme/notifications_strings.dart';

/// Notification detail with message facts and an explicit mark-read action.
class NotificationDetailScreen extends StatefulWidget {
  /// Creates the detail screen.
  const NotificationDetailScreen({
    super.key,
    required this.repository,
    required this.notificationId,
    this.strings = const NotificationsStrings(),
  });

  /// Data source used for the notification.
  final NotificationsRepository repository;

  /// Identifier of the notification to open.
  final String notificationId;

  final NotificationsStrings strings;

  @override
  State<NotificationDetailScreen> createState() =>
      _NotificationDetailScreenState();
}

class _NotificationDetailScreenState extends State<NotificationDetailScreen> {
  NotificationRecord? _record;
  bool _loading = true;
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = _record == null;
      _error = null;
    });
    try {
      final record = await widget.repository.getNotification(
        widget.notificationId,
      );
      if (!mounted) return;
      setState(() {
        _record = record;
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
      error is NotificationsApiException ? error.message : error.toString();

  Future<void> _markRead() async {
    final record = _record;
    if (record == null || _busy || !record.unread) return;
    setState(() => _busy = true);
    try {
      await widget.repository.markRead(record.id);
      if (!mounted) return;
      setState(() {
        _record = record.copyWith(
          unread: false,
          status: 'Read',
          readAt: DateTime.now(),
        );
        _busy = false;
      });
      _showMessage(widget.strings.readSnack);
    } on Exception catch (error) {
      if (!mounted) return;
      setState(() => _busy = false);
      _showMessage(_clean(error));
    }
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    final strings = widget.strings;
    final record = _record;
    return Scaffold(
      appBar: AppBar(
        title: Text(strings.detailTitle),
        actions: [
          IconButton(
            tooltip: strings.refresh,
            onPressed: _busy ? null : _load,
            icon: const Icon(Icons.refresh),
          ),
          const SizedBox(width: 6),
        ],
      ),
      body: SafeArea(
        child: Stack(
          children: [
            if (_loading)
              const Center(child: CircularProgressIndicator())
            else if (record == null)
              _DetailError(
                message: _error ?? strings.loadError,
                retryLabel: strings.retry,
                onRetry: _load,
              )
            else
              RefreshIndicator(
                onRefresh: _load,
                child: SingleChildScrollView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  padding: const EdgeInsets.fromLTRB(20, 4, 20, 32),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      _DetailHero(record: record, strings: strings),
                      const SizedBox(height: 14),
                      _MessageCard(record: record),
                      if (record.unread) ...[
                        const SizedBox(height: 12),
                        FilledButton.icon(
                          key: const ValueKey<String>('detail-mark-read'),
                          onPressed: _busy ? null : _markRead,
                          style: FilledButton.styleFrom(
                            backgroundColor: KinevorNotificationsTheme.accent,
                            foregroundColor: KinevorNotificationsTheme.onAccent,
                            minimumSize: const Size.fromHeight(46),
                          ),
                          icon: const Icon(Icons.done_all, size: 19),
                          label: Text(strings.markRead),
                        ),
                      ],
                      const SizedBox(height: 14),
                      _FactsCard(record: record, strings: strings),
                    ],
                  ),
                ),
              ),
            if (_busy)
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

class _DetailHero extends StatelessWidget {
  const _DetailHero({required this.record, required this.strings});

  final NotificationRecord record;
  final NotificationsStrings strings;

  @override
  Widget build(BuildContext context) {
    final categoryColor = KinevorNotificationsTheme.categoryColor(
      record.category,
    );
    final statusColor = KinevorNotificationsTheme.statusColor(record.status);
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            categoryColor.withValues(alpha: 0.24),
            KinevorNotificationsTheme.surfaceRaised,
          ],
        ),
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: KinevorNotificationsTheme.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 46,
                height: 46,
                decoration: BoxDecoration(
                  color: categoryColor.withValues(alpha: 0.16),
                  borderRadius: BorderRadius.circular(14),
                ),
                alignment: Alignment.center,
                child: Icon(
                  KinevorNotificationsTheme.categoryIcon(record.category),
                  size: 23,
                  color: categoryColor,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      strings.detailEyebrow,
                      style: const TextStyle(
                        fontSize: 9,
                        fontWeight: FontWeight.w800,
                        letterSpacing: 1.4,
                        color: KinevorNotificationsTheme.accent,
                      ),
                    ),
                    const SizedBox(height: 3),
                    Text(
                      strings.categoryLabel(record.category),
                      style: Theme.of(context).textTheme.bodySmall?.copyWith(
                        color: KinevorNotificationsTheme.textMuted,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 8),
              _StatusChip(
                label: strings.statusLabel(record.status),
                color: statusColor,
                icon: KinevorNotificationsTheme.statusIcon(record.status),
              ),
            ],
          ),
          const SizedBox(height: 14),
          Text(record.title, style: Theme.of(context).textTheme.headlineSmall),
          const SizedBox(height: 6),
          Row(
            children: [
              Icon(
                KinevorNotificationsTheme.channelIcon(record.channel),
                size: 14,
                color: KinevorNotificationsTheme.channelColor(record.channel),
              ),
              const SizedBox(width: 6),
              Text(
                '${strings.channelLabel(record.channel)} · '
                '${strings.date(record.createdAt)}',
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                  color: KinevorNotificationsTheme.textMuted,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _StatusChip extends StatelessWidget {
  const _StatusChip({
    required this.label,
    required this.color,
    required this.icon,
  });

  final String label;
  final Color color;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.13),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withValues(alpha: 0.35)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 12, color: color),
          const SizedBox(width: 4),
          Text(
            label.toUpperCase(),
            style: TextStyle(
              fontSize: 8.5,
              fontWeight: FontWeight.w800,
              letterSpacing: 0.5,
              color: color,
            ),
          ),
        ],
      ),
    );
  }
}

class _MessageCard extends StatelessWidget {
  const _MessageCard({required this.record});

  final NotificationRecord record;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(18, 18, 18, 18),
        child: Text(
          record.body,
          style: Theme.of(context).textTheme.bodyLarge?.copyWith(height: 1.6),
        ),
      ),
    );
  }
}

class _FactsCard extends StatelessWidget {
  const _FactsCard({required this.record, required this.strings});

  final NotificationRecord record;
  final NotificationsStrings strings;

  @override
  Widget build(BuildContext context) {
    final facts = <({IconData icon, String label, String value})>[
      (
        icon: KinevorNotificationsTheme.channelIcon(record.channel),
        label: strings.channel,
        value: strings.channelLabel(record.channel),
      ),
      (
        icon: Icons.category_outlined,
        label: strings.category,
        value: strings.categoryLabel(record.category),
      ),
      (
        icon: KinevorNotificationsTheme.statusIcon(record.status),
        label: strings.status,
        value: strings.statusLabel(record.status),
      ),
      (
        icon: Icons.schedule_outlined,
        label: strings.received,
        value: strings.dateTime(record.createdAt),
      ),
      if (record.sentAt != null)
        (
          icon: Icons.send_outlined,
          label: strings.scheduled,
          value: strings.dateTime(record.sentAt!),
        ),
      if (record.recipientEmail != null)
        (
          icon: Icons.alternate_email,
          label: strings.recipient,
          value: record.recipientEmail!,
        ),
      if (record.templateKey != null)
        (
          icon: Icons.description_outlined,
          label: strings.template,
          value: record.templateKey!,
        ),
      if (record.failureReason != null)
        (
          icon: Icons.error_outline,
          label: strings.failureReason,
          value: record.failureReason!,
        ),
    ];

    return Card(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(18, 16, 18, 8),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            for (var index = 0; index < facts.length; index++) ...[
              _FactRow(
                icon: facts[index].icon,
                label: facts[index].label,
                value: facts[index].value,
              ),
              if (index != facts.length - 1) const Divider(),
            ],
          ],
        ),
      ),
    );
  }
}

class _FactRow extends StatelessWidget {
  const _FactRow({
    required this.icon,
    required this.label,
    required this.value,
  });

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 9),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 16, color: KinevorNotificationsTheme.textMuted),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label.toUpperCase(),
                  style: const TextStyle(
                    fontSize: 8.5,
                    fontWeight: FontWeight.w700,
                    letterSpacing: 0.8,
                    color: KinevorNotificationsTheme.textMuted,
                  ),
                ),
                const SizedBox(height: 2),
                Text(value, style: const TextStyle(fontSize: 12.5)),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _DetailError extends StatelessWidget {
  const _DetailError({
    required this.message,
    required this.retryLabel,
    required this.onRetry,
  });

  final String message;
  final String retryLabel;
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
              color: KinevorNotificationsTheme.error,
            ),
            const SizedBox(height: 12),
            Text(
              message,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                color: KinevorNotificationsTheme.textMuted,
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
