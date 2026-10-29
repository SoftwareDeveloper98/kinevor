import 'package:flutter/material.dart';

import '../data/notifications_api_exception.dart';
import '../data/notifications_repository.dart';
import '../models/notification_models.dart';
import '../theme/kinevor_notifications_theme.dart';
import '../theme/notifications_strings.dart';

/// Category and channel preferences plus a push device registration demo.
class NotificationPreferencesScreen extends StatefulWidget {
  /// Creates the preferences screen.
  const NotificationPreferencesScreen({
    super.key,
    required this.repository,
    this.strings = const NotificationsStrings(),
  });

  /// Data source used for preferences and devices.
  final NotificationsRepository repository;

  final NotificationsStrings strings;

  @override
  State<NotificationPreferencesScreen> createState() =>
      _NotificationPreferencesScreenState();
}

class _NotificationPreferencesScreenState
    extends State<NotificationPreferencesScreen> {
  List<NotificationPreference> _preferences = const [];
  List<NotificationDevice> _devices = const [];
  bool _loading = true;
  bool _busy = false;
  String? _savingCategory;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = _preferences.isEmpty;
      _error = null;
    });
    try {
      final preferences = await widget.repository.getPreferences();
      final devices = await widget.repository.getDevices();
      if (!mounted) return;
      setState(() {
        _preferences = preferences;
        _devices = devices;
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

  NotificationPreference _preference(String category) {
    for (final preference in _preferences) {
      if (preference.category == category) return preference;
    }
    return NotificationPreference(
      category: category,
      email: true,
      push: true,
      inApp: true,
    );
  }

  Future<void> _toggle(
    NotificationPreference preference,
    String channel,
    bool value,
  ) async {
    if (_savingCategory != null) return;
    final updated = switch (channel) {
      'Email' => preference.copyWith(email: value),
      'Push' => preference.copyWith(push: value),
      _ => preference.copyWith(inApp: value),
    };

    setState(() => _savingCategory = preference.category);
    try {
      final preferences = await widget.repository.updatePreference(
        category: updated.category,
        email: updated.email,
        push: updated.push,
        inApp: updated.inApp,
      );
      if (!mounted) return;
      setState(() {
        _preferences = preferences;
        _savingCategory = null;
      });
      _showMessage(widget.strings.preferenceUpdated);
    } on Exception catch (error) {
      if (!mounted) return;
      setState(() => _savingCategory = null);
      _showMessage(_clean(error));
    }
  }

  Future<void> _registerDevice() async {
    if (_busy) return;
    setState(() => _busy = true);
    try {
      final token =
          '${widget.strings.demoDeviceToken}-'
          '${DateTime.now().millisecondsSinceEpoch}';
      await widget.repository.registerDevice(
        token: token,
        platform: widget.strings.demoDevicePlatform,
      );
      final devices = await widget.repository.getDevices();
      if (!mounted) return;
      setState(() {
        _devices = devices;
        _busy = false;
      });
      _showMessage(widget.strings.deviceRegistered);
    } on Exception catch (error) {
      if (!mounted) return;
      setState(() => _busy = false);
      _showMessage(_clean(error));
    }
  }

  Future<void> _removeDevice(NotificationDevice device) async {
    if (_busy) return;
    setState(() => _busy = true);
    try {
      await widget.repository.unregisterDevice(device.id);
      final devices = await widget.repository.getDevices();
      if (!mounted) return;
      setState(() {
        _devices = devices;
        _busy = false;
      });
      _showMessage(widget.strings.deviceRemoved);
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
    return Scaffold(
      appBar: AppBar(
        title: Text(strings.preferencesTitle),
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
            else if (_error != null && _preferences.isEmpty)
              _PreferencesError(
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
                      _PreferencesHero(strings: strings),
                      const SizedBox(height: 12),
                      for (final category in notificationCategories) ...[
                        _PreferenceCard(
                          category: category,
                          preference: _preference(category),
                          strings: strings,
                          saving: _savingCategory == category,
                          enabled: _savingCategory == null,
                          onToggle: _toggle,
                        ),
                        const SizedBox(height: 10),
                      ],
                      const SizedBox(height: 2),
                      _DeviceCard(
                        devices: _devices,
                        strings: strings,
                        busy: _busy,
                        onRegister: _registerDevice,
                        onRemove: _removeDevice,
                      ),
                    ],
                  ),
                ),
              ),
            if (_busy || _savingCategory != null)
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

class _PreferencesHero extends StatelessWidget {
  const _PreferencesHero({required this.strings});

  final NotificationsStrings strings;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            KinevorNotificationsTheme.success.withValues(alpha: 0.2),
            KinevorNotificationsTheme.surfaceRaised,
          ],
        ),
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: KinevorNotificationsTheme.border),
      ),
      child: Row(
        children: [
          Container(
            width: 46,
            height: 46,
            decoration: BoxDecoration(
              color: KinevorNotificationsTheme.success.withValues(alpha: 0.14),
              borderRadius: BorderRadius.circular(14),
            ),
            alignment: Alignment.center,
            child: const Icon(
              Icons.tune,
              size: 24,
              color: KinevorNotificationsTheme.success,
            ),
          ),
          const SizedBox(width: 13),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  strings.preferencesEyebrow,
                  style: const TextStyle(
                    fontSize: 9,
                    fontWeight: FontWeight.w800,
                    letterSpacing: 1.4,
                    color: KinevorNotificationsTheme.success,
                  ),
                ),
                const SizedBox(height: 3),
                Text(
                  strings.preferencesSubtitle,
                  style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: KinevorNotificationsTheme.textMuted,
                  ),
                ),
                const SizedBox(height: 6),
                Wrap(
                  spacing: 8,
                  runSpacing: 4,
                  children: [
                    for (final channel in notificationChannels)
                      Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            KinevorNotificationsTheme.channelIcon(channel),
                            size: 11,
                            color: KinevorNotificationsTheme.channelColor(
                              channel,
                            ),
                          ),
                          const SizedBox(width: 3),
                          Text(
                            strings.channelLabel(channel),
                            style: const TextStyle(
                              fontSize: 9,
                              fontWeight: FontWeight.w600,
                              color: KinevorNotificationsTheme.textMuted,
                            ),
                          ),
                        ],
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

class _PreferenceCard extends StatelessWidget {
  const _PreferenceCard({
    required this.category,
    required this.preference,
    required this.strings,
    required this.saving,
    required this.enabled,
    required this.onToggle,
  });

  final String category;
  final NotificationPreference preference;
  final NotificationsStrings strings;
  final bool saving;
  final bool enabled;
  final void Function(NotificationPreference, String, bool) onToggle;

  @override
  Widget build(BuildContext context) {
    final color = KinevorNotificationsTheme.categoryColor(category);
    return Card(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(14, 12, 14, 12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Container(
                  width: 34,
                  height: 34,
                  decoration: BoxDecoration(
                    color: color.withValues(alpha: 0.13),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  alignment: Alignment.center,
                  child: Icon(
                    KinevorNotificationsTheme.categoryIcon(category),
                    size: 17,
                    color: color,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        strings.categoryLabel(category),
                        style: const TextStyle(
                          fontSize: 13,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 1),
                      Text(
                        strings.categoryDescription(category),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          fontSize: 10,
                          color: KinevorNotificationsTheme.textMuted,
                        ),
                      ),
                    ],
                  ),
                ),
                if (saving)
                  const SizedBox(
                    width: 14,
                    height: 14,
                    child: CircularProgressIndicator(strokeWidth: 1.6),
                  ),
              ],
            ),
            const SizedBox(height: 9),
            Row(
              children: [
                for (
                  var index = 0;
                  index < notificationChannels.length;
                  index++
                ) ...[
                  if (index > 0) const SizedBox(width: 7),
                  Expanded(
                    child: _ChannelChip(
                      category: category,
                      channel: notificationChannels[index],
                      label: strings.channelLabel(notificationChannels[index]),
                      selected: preference.isEnabled(
                        notificationChannels[index],
                      ),
                      enabled: enabled,
                      onChanged: (value) => onToggle(
                        preference,
                        notificationChannels[index],
                        value,
                      ),
                    ),
                  ),
                ],
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _ChannelChip extends StatelessWidget {
  const _ChannelChip({
    required this.category,
    required this.channel,
    required this.label,
    required this.selected,
    required this.enabled,
    required this.onChanged,
  });

  final String category;
  final String channel;
  final String label;
  final bool selected;
  final bool enabled;
  final ValueChanged<bool> onChanged;

  @override
  Widget build(BuildContext context) {
    final color = KinevorNotificationsTheme.channelColor(channel);
    return ChoiceChip(
      key: ValueKey<String>('preference-$category-${channel.toLowerCase()}'),
      label: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            selected ? Icons.check_circle : Icons.circle_outlined,
            size: 13,
            color: selected ? color : KinevorNotificationsTheme.textMuted,
          ),
          const SizedBox(width: 4),
          Flexible(
            child: Text(
              label,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                fontSize: 10,
                fontWeight: FontWeight.w700,
                color: selected ? color : KinevorNotificationsTheme.textMuted,
              ),
            ),
          ),
        ],
      ),
      selected: selected,
      onSelected: enabled ? onChanged : null,
      showCheckmark: false,
      selectedColor: color.withValues(alpha: 0.14),
      backgroundColor: KinevorNotificationsTheme.surface,
      disabledColor: KinevorNotificationsTheme.surface,
      side: BorderSide(
        color: selected
            ? color.withValues(alpha: 0.45)
            : KinevorNotificationsTheme.border,
      ),
      padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 5),
      visualDensity: VisualDensity.compact,
      materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
    );
  }
}

class _DeviceCard extends StatelessWidget {
  const _DeviceCard({
    required this.devices,
    required this.strings,
    required this.busy,
    required this.onRegister,
    required this.onRemove,
  });

  final List<NotificationDevice> devices;
  final NotificationsStrings strings;
  final bool busy;
  final VoidCallback onRegister;
  final ValueChanged<NotificationDevice> onRemove;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(16, 15, 16, 14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Container(
                  width: 34,
                  height: 34,
                  decoration: BoxDecoration(
                    color: KinevorNotificationsTheme.info.withValues(
                      alpha: 0.13,
                    ),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  alignment: Alignment.center,
                  child: const Icon(
                    Icons.devices_other,
                    size: 17,
                    color: KinevorNotificationsTheme.info,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        strings.devicesTitle,
                        style: const TextStyle(
                          fontSize: 13,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 1),
                      Text(
                        strings.devicesNote,
                        maxLines: 2,
                        style: const TextStyle(
                          fontSize: 10,
                          height: 1.35,
                          color: KinevorNotificationsTheme.textMuted,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            if (devices.isEmpty)
              Text(
                strings.noDevices,
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                  color: KinevorNotificationsTheme.textMuted,
                ),
              )
            else
              for (var index = 0; index < devices.length; index++) ...[
                _DeviceRow(
                  device: devices[index],
                  strings: strings,
                  enabled: !busy,
                  onRemove: () => onRemove(devices[index]),
                ),
                if (index != devices.length - 1) const Divider(),
              ],
            const SizedBox(height: 8),
            OutlinedButton.icon(
              onPressed: busy ? null : onRegister,
              style: OutlinedButton.styleFrom(
                minimumSize: const Size.fromHeight(40),
                foregroundColor: KinevorNotificationsTheme.text,
                side: const BorderSide(color: KinevorNotificationsTheme.border),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(11),
                ),
              ),
              icon: const Icon(Icons.add, size: 17),
              label: Text(
                strings.registerDevice,
                style: const TextStyle(fontSize: 11),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _DeviceRow extends StatelessWidget {
  const _DeviceRow({
    required this.device,
    required this.strings,
    required this.enabled,
    required this.onRemove,
  });

  final NotificationDevice device;
  final NotificationsStrings strings;
  final bool enabled;
  final VoidCallback onRemove;

  @override
  Widget build(BuildContext context) {
    final icon = switch (device.platform.toLowerCase()) {
      'ios' => Icons.phone_iphone,
      'android' => Icons.phone_android,
      _ => Icons.language,
    };
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        children: [
          Container(
            width: 30,
            height: 30,
            decoration: BoxDecoration(
              color: KinevorNotificationsTheme.accent.withValues(alpha: 0.11),
              borderRadius: BorderRadius.circular(9),
            ),
            alignment: Alignment.center,
            child: Icon(
              icon,
              size: 15,
              color: KinevorNotificationsTheme.accent,
            ),
          ),
          const SizedBox(width: 9),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  strings.platformLabel(device.platform),
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 1),
                Text(
                  '${device.tokenPreview} · '
                  '${strings.registeredOn} ${strings.monthDay(device.registeredAt)}',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 9.5,
                    color: KinevorNotificationsTheme.textMuted,
                  ),
                ),
              ],
            ),
          ),
          IconButton(
            tooltip: strings.removeDevice,
            onPressed: enabled ? onRemove : null,
            visualDensity: VisualDensity.compact,
            icon: const Icon(
              Icons.delete_outline,
              size: 18,
              color: KinevorNotificationsTheme.error,
            ),
          ),
        ],
      ),
    );
  }
}

class _PreferencesError extends StatelessWidget {
  const _PreferencesError({
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
