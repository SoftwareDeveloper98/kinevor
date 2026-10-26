import 'package:flutter/material.dart';

import '../data/notifications_api_exception.dart';
import '../data/notifications_repository.dart';
import '../models/notification_models.dart';
import '../theme/kinevor_notifications_theme.dart';
import '../theme/notifications_strings.dart';
import 'notification_detail_screen.dart';
import 'notification_preferences_screen.dart';

/// Filterable member notification inbox with unread state and mark-all-read.
class NotificationInboxScreen extends StatefulWidget {
  /// Creates the inbox screen.
  const NotificationInboxScreen({
    super.key,
    required this.repository,
    this.strings = const NotificationsStrings(),
  });

  /// Data source used for notifications.
  final NotificationsRepository repository;

  /// Localized labels.
  final NotificationsStrings strings;

  @override
  State<NotificationInboxScreen> createState() =>
      _NotificationInboxScreenState();
}

class _NotificationInboxScreenState extends State<NotificationInboxScreen> {
  List<NotificationRecord> _items = const [];
  int _unreadCount = 0;
  bool _loading = true;
  bool _busy = false;
  String? _error;
  String? _category;
  String? _channel;
  bool _unreadOnly = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = _items.isEmpty;
      _error = null;
    });
    try {
      final page = await widget.repository.getNotifications(
        category: _category,
        channel: _channel,
        unreadOnly: _unreadOnly,
      );
      final unread = await widget.repository.getUnreadCount();
      if (!mounted) return;
      setState(() {
        _items = page.items;
        _unreadCount = unread;
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

  Future<void> _markAllRead() async {
    if (_busy) return;
    setState(() => _busy = true);
    try {
      await widget.repository.markAllRead();
      await _load();
      if (!mounted) return;
      setState(() => _busy = false);
      _showMessage(widget.strings.allReadSnack);
    } on Exception catch (error) {
      if (!mounted) return;
      setState(() => _busy = false);
      _showMessage(_clean(error));
    }
  }

  Future<void> _openDetail(NotificationRecord record) async {
    await Navigator.of(context).push<void>(
      MaterialPageRoute<void>(
        builder: (_) => NotificationDetailScreen(
          repository: widget.repository,
          strings: widget.strings,
          notificationId: record.id,
        ),
      ),
    );
    if (mounted) await _load();
  }

  Future<void> _openPreferences() async {
    await Navigator.of(context).push<void>(
      MaterialPageRoute<void>(
        builder: (_) => NotificationPreferencesScreen(
          repository: widget.repository,
          strings: widget.strings,
        ),
      ),
    );
  }

  Future<void> _pickCategory() async {
    final selection = await _pickOption(
      title: widget.strings.categoryFilter,
      allLabel: widget.strings.allCategories,
      options: [
        for (final category in notificationCategories)
          (value: category, label: widget.strings.categoryLabel(category)),
      ],
      selected: _category,
    );
    if (selection == null) return;
    setState(() => _category = selection.isEmpty ? null : selection);
    await _load();
  }

  Future<void> _pickChannel() async {
    final selection = await _pickOption(
      title: widget.strings.channelFilter,
      allLabel: widget.strings.allChannels,
      options: [
        for (final channel in notificationChannels)
          (value: channel, label: widget.strings.channelLabel(channel)),
      ],
      selected: _channel,
    );
    if (selection == null) return;
    setState(() => _channel = selection.isEmpty ? null : selection);
    await _load();
  }

  Future<String?> _pickOption({
    required String title,
    required String allLabel,
    required List<({String value, String label})> options,
    required String? selected,
  }) {
    return showModalBottomSheet<String>(
      context: context,
      builder: (sheetContext) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 4, 20, 8),
              child: Align(
                alignment: Alignment.centerLeft,
                child: Text(
                  title,
                  style: Theme.of(sheetContext).textTheme.titleLarge,
                ),
              ),
            ),
            ListTile(
              leading: const Icon(Icons.clear_all),
              title: Text(allLabel),
              trailing: selected == null
                  ? const Icon(
                      Icons.check,
                      color: KinevorNotificationsTheme.accent,
                    )
                  : null,
              onTap: () => Navigator.of(sheetContext).pop(''),
            ),
            for (final option in options)
              ListTile(
                leading: Icon(
                  title == widget.strings.categoryFilter
                      ? KinevorNotificationsTheme.categoryIcon(option.value)
                      : KinevorNotificationsTheme.channelIcon(option.value),
                ),
                title: Text(option.label),
                trailing: selected == option.value
                    ? const Icon(
                        Icons.check,
                        color: KinevorNotificationsTheme.accent,
                      )
                    : null,
                onTap: () => Navigator.of(sheetContext).pop(option.value),
              ),
            const SizedBox(height: 8),
          ],
        ),
      ),
    );
  }

  Future<void> _clearFilters() async {
    setState(() {
      _category = null;
      _channel = null;
      _unreadOnly = false;
    });
    await _load();
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(message)));
  }

  bool get _hasFilters => _category != null || _channel != null || _unreadOnly;

  @override
  Widget build(BuildContext context) {
    final strings = widget.strings;
    return Scaffold(
      appBar: AppBar(
        title: Text(strings.inboxTitle),
        actions: [
          IconButton(
            tooltip: strings.preferencesTooltip,
            onPressed: _openPreferences,
            icon: const Icon(Icons.tune),
          ),
          const SizedBox(width: 6),
        ],
      ),
      body: SafeArea(
        child: Stack(
          children: [
            if (_loading && _items.isEmpty)
              const Center(child: CircularProgressIndicator())
            else if (_error != null && _items.isEmpty)
              _InboxError(
                message: _error!,
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
                      _InboxHero(
                        strings: strings,
                        unreadCount: _unreadCount,
                        total: _items.length,
                        busy: _busy,
                        onMarkAllRead: _markAllRead,
                      ),
                      const SizedBox(height: 12),
                      _FilterCard(
                        strings: strings,
                        unreadOnly: _unreadOnly,
                        category: _category,
                        channel: _channel,
                        hasFilters: _hasFilters,
                        onUnreadChanged: (value) {
                          setState(() => _unreadOnly = value);
                          _load();
                        },
                        onPickCategory: _pickCategory,
                        onPickChannel: _pickChannel,
                        onClear: _clearFilters,
                      ),
                      const SizedBox(height: 14),
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              strings.showingCount(_items.length),
                              style: Theme.of(context).textTheme.bodySmall
                                  ?.copyWith(
                                    color: KinevorNotificationsTheme.textMuted,
                                  ),
                            ),
                          ),
                          if (_unreadCount > 0)
                            Text(
                              strings.unreadSummary(_unreadCount),
                              style: const TextStyle(
                                fontSize: 10,
                                fontWeight: FontWeight.w700,
                                color: KinevorNotificationsTheme.accent,
                              ),
                            ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      if (_items.isEmpty)
                        _EmptyState(strings: strings)
                      else
                        for (final record in _items) ...[
                          _NotificationCard(
                            record: record,
                            strings: strings,
                            onTap: () => _openDetail(record),
                          ),
                          const SizedBox(height: 10),
                        ],
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

class _InboxHero extends StatelessWidget {
  const _InboxHero({
    required this.strings,
    required this.unreadCount,
    required this.total,
    required this.busy,
    required this.onMarkAllRead,
  });

  final NotificationsStrings strings;
  final int unreadCount;
  final int total;
  final bool busy;
  final VoidCallback onMarkAllRead;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            KinevorNotificationsTheme.accent.withValues(alpha: 0.24),
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
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 52,
                height: 52,
                decoration: BoxDecoration(
                  gradient: KinevorNotificationsTheme.brandGradient,
                  borderRadius: BorderRadius.circular(16),
                  boxShadow: [
                    BoxShadow(
                      color: KinevorNotificationsTheme.accent.withValues(
                        alpha: 0.3,
                      ),
                      blurRadius: 18,
                      offset: const Offset(0, 6),
                    ),
                  ],
                ),
                alignment: Alignment.center,
                child: const Icon(
                  Icons.mark_email_unread_outlined,
                  size: 25,
                  color: KinevorNotificationsTheme.onAccent,
                ),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      strings.inboxEyebrow,
                      style: const TextStyle(
                        fontSize: 9,
                        fontWeight: FontWeight.w800,
                        letterSpacing: 1.4,
                        color: KinevorNotificationsTheme.accent,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      strings.unreadSummary(unreadCount),
                      style: Theme.of(context).textTheme.headlineSmall,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 3),
                    Text(
                      strings.inboxSubtitle,
                      maxLines: 2,
                      style: Theme.of(context).textTheme.bodySmall?.copyWith(
                        color: KinevorNotificationsTheme.textMuted,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 10,
                  vertical: 7,
                ),
                decoration: BoxDecoration(
                  color: KinevorNotificationsTheme.ink.withValues(alpha: 0.5),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Column(
                  children: [
                    Text(
                      '$unreadCount',
                      style: const TextStyle(
                        fontSize: 19,
                        fontWeight: FontWeight.w800,
                        height: 1,
                      ),
                    ),
                    const SizedBox(height: 3),
                    Text(
                      strings.unread.toUpperCase(),
                      style: const TextStyle(
                        fontSize: 7,
                        fontWeight: FontWeight.w800,
                        letterSpacing: 0.6,
                        color: KinevorNotificationsTheme.textMuted,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: Text(
                  strings.notificationCount(total),
                  style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: KinevorNotificationsTheme.textMuted,
                  ),
                ),
              ),
              if (unreadCount > 0)
                TextButton.icon(
                  onPressed: busy ? null : onMarkAllRead,
                  style: TextButton.styleFrom(
                    foregroundColor: KinevorNotificationsTheme.accent,
                    padding: const EdgeInsets.symmetric(horizontal: 8),
                    minimumSize: const Size(0, 32),
                    tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                  ),
                  icon: const Icon(Icons.done_all, size: 16),
                  label: Text(
                    strings.markAllRead,
                    style: const TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.w700,
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

class _FilterCard extends StatelessWidget {
  const _FilterCard({
    required this.strings,
    required this.unreadOnly,
    required this.category,
    required this.channel,
    required this.hasFilters,
    required this.onUnreadChanged,
    required this.onPickCategory,
    required this.onPickChannel,
    required this.onClear,
  });

  final NotificationsStrings strings;
  final bool unreadOnly;
  final String? category;
  final String? channel;
  final bool hasFilters;
  final ValueChanged<bool> onUnreadChanged;
  final VoidCallback onPickCategory;
  final VoidCallback onPickChannel;
  final VoidCallback onClear;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(14, 12, 14, 12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                ChoiceChip(
                  key: const ValueKey<String>('inbox-filter-all'),
                  label: Text(strings.all),
                  selected: !unreadOnly,
                  onSelected: (_) => onUnreadChanged(false),
                  selectedColor: KinevorNotificationsTheme.accent.withValues(
                    alpha: 0.18,
                  ),
                  labelStyle: TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.w700,
                    color: !unreadOnly
                        ? KinevorNotificationsTheme.accent
                        : KinevorNotificationsTheme.text,
                  ),
                  side: BorderSide(
                    color: !unreadOnly
                        ? KinevorNotificationsTheme.accent.withValues(
                            alpha: 0.5,
                          )
                        : KinevorNotificationsTheme.border,
                  ),
                  visualDensity: VisualDensity.compact,
                ),
                const SizedBox(width: 8),
                ChoiceChip(
                  key: const ValueKey<String>('inbox-filter-unread'),
                  label: Text(strings.unread),
                  selected: unreadOnly,
                  onSelected: (_) => onUnreadChanged(true),
                  selectedColor: KinevorNotificationsTheme.accent.withValues(
                    alpha: 0.18,
                  ),
                  labelStyle: TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.w700,
                    color: unreadOnly
                        ? KinevorNotificationsTheme.accent
                        : KinevorNotificationsTheme.text,
                  ),
                  side: BorderSide(
                    color: unreadOnly
                        ? KinevorNotificationsTheme.accent.withValues(
                            alpha: 0.5,
                          )
                        : KinevorNotificationsTheme.border,
                  ),
                  visualDensity: VisualDensity.compact,
                ),
                const Spacer(),
                if (hasFilters)
                  TextButton(
                    onPressed: onClear,
                    style: TextButton.styleFrom(
                      foregroundColor: KinevorNotificationsTheme.textMuted,
                      padding: const EdgeInsets.symmetric(horizontal: 6),
                      minimumSize: const Size(0, 30),
                      tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    ),
                    child: Text(
                      strings.clearFilters,
                      style: const TextStyle(fontSize: 10),
                    ),
                  ),
              ],
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                Expanded(
                  child: _FilterButton(
                    icon: Icons.category_outlined,
                    label: category == null
                        ? strings.allCategories
                        : strings.categoryLabel(category!),
                    onTap: onPickCategory,
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: _FilterButton(
                    icon: Icons.send_outlined,
                    label: channel == null
                        ? strings.allChannels
                        : strings.channelLabel(channel!),
                    onTap: onPickChannel,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _FilterButton extends StatelessWidget {
  const _FilterButton({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  final IconData icon;
  final String label;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return OutlinedButton.icon(
      onPressed: onTap,
      style: OutlinedButton.styleFrom(
        minimumSize: const Size.fromHeight(38),
        foregroundColor: KinevorNotificationsTheme.text,
        padding: const EdgeInsets.symmetric(horizontal: 10),
        side: const BorderSide(color: KinevorNotificationsTheme.border),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(11)),
      ),
      icon: Icon(icon, size: 15),
      label: Text(
        label,
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
        style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w600),
      ),
    );
  }
}

class _NotificationCard extends StatelessWidget {
  const _NotificationCard({
    required this.record,
    required this.strings,
    required this.onTap,
  });

  final NotificationRecord record;
  final NotificationsStrings strings;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final categoryColor = KinevorNotificationsTheme.categoryColor(
      record.category,
    );
    return Card(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(18),
        side: BorderSide(
          color: record.unread
              ? KinevorNotificationsTheme.accent.withValues(alpha: 0.45)
              : KinevorNotificationsTheme.border,
        ),
      ),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(18),
        child: Padding(
          padding: const EdgeInsets.fromLTRB(14, 13, 12, 13),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: categoryColor.withValues(alpha: 0.13),
                  borderRadius: BorderRadius.circular(11),
                ),
                alignment: Alignment.center,
                child: Icon(
                  KinevorNotificationsTheme.categoryIcon(record.category),
                  size: 20,
                  color: categoryColor,
                ),
              ),
              const SizedBox(width: 11),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Expanded(
                          child: Text(
                            record.title,
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                            style: TextStyle(
                              fontSize: 13.5,
                              height: 1.3,
                              fontWeight: record.unread
                                  ? FontWeight.w700
                                  : FontWeight.w600,
                            ),
                          ),
                        ),
                        if (record.unread) ...[
                          const SizedBox(width: 7),
                          Container(
                            width: 8,
                            height: 8,
                            margin: const EdgeInsets.only(top: 4),
                            decoration: const BoxDecoration(
                              color: KinevorNotificationsTheme.accent,
                              shape: BoxShape.circle,
                            ),
                          ),
                        ],
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      record.body,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        fontSize: 11,
                        height: 1.4,
                        color: KinevorNotificationsTheme.textMuted,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        Flexible(
                          child: Wrap(
                            spacing: 5,
                            runSpacing: 4,
                            children: [
                              _MiniPill(
                                label: strings.categoryLabel(record.category),
                                color: categoryColor,
                              ),
                              _MiniPill(
                                label: strings.channelLabel(record.channel),
                                color: KinevorNotificationsTheme.channelColor(
                                  record.channel,
                                ),
                              ),
                              if (record.failed)
                                _MiniPill(
                                  label: strings
                                      .statusLabel(record.status)
                                      .toUpperCase(),
                                  color: KinevorNotificationsTheme.error,
                                ),
                            ],
                          ),
                        ),
                        const SizedBox(width: 8),
                        Text(
                          strings.monthDay(record.createdAt),
                          style: const TextStyle(
                            fontSize: 9.5,
                            color: KinevorNotificationsTheme.textMuted,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 2),
              const Padding(
                padding: EdgeInsets.only(top: 9),
                child: Icon(
                  Icons.chevron_right,
                  size: 18,
                  color: KinevorNotificationsTheme.textMuted,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _MiniPill extends StatelessWidget {
  const _MiniPill({required this.label, required this.color});

  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 3),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withValues(alpha: 0.28)),
      ),
      child: Text(
        label,
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
        style: TextStyle(
          fontSize: 8.5,
          fontWeight: FontWeight.w700,
          color: color,
        ),
      ),
    );
  }
}

class _EmptyState extends StatelessWidget {
  const _EmptyState({required this.strings});

  final NotificationsStrings strings;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          children: [
            Container(
              width: 54,
              height: 54,
              decoration: BoxDecoration(
                color: KinevorNotificationsTheme.accent.withValues(alpha: 0.1),
                shape: BoxShape.circle,
              ),
              alignment: Alignment.center,
              child: const Icon(
                Icons.notifications_off_outlined,
                size: 26,
                color: KinevorNotificationsTheme.accent,
              ),
            ),
            const SizedBox(height: 12),
            Text(
              strings.noNotifications,
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w700),
            ),
            const SizedBox(height: 5),
            Text(
              strings.noNotificationsHint,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                color: KinevorNotificationsTheme.textMuted,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _InboxError extends StatelessWidget {
  const _InboxError({
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
