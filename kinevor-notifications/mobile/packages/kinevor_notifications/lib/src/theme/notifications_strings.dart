/// User-facing labels used by the notifications widgets.
class NotificationsStrings {
  const NotificationsStrings({
    this.brand = 'Kinevor Notifications',
    this.inboxTitle = 'Inbox',
    this.inboxEyebrow = 'MEMBER INBOX',
    this.inboxSubtitle = 'Reminders, offers and receipts in one place.',
    this.detailTitle = 'Notification',
    this.detailEyebrow = 'MESSAGE DETAIL',
    this.preferencesTitle = 'Notification preferences',
    this.preferencesEyebrow = 'DELIVERY SETTINGS',
    this.preferencesSubtitle = 'Choose how each category reaches you.',
    this.all = 'All',
    this.unread = 'Unread',
    this.categoryFilter = 'Category',
    this.channelFilter = 'Channel',
    this.allCategories = 'All categories',
    this.allChannels = 'All channels',
    this.clearFilters = 'Clear filters',
    this.markAllRead = 'Mark all read',
    this.markRead = 'Mark as read',
    this.read = 'Read',
    this.readSnack = 'Notification marked read.',
    this.allReadSnack = 'Notifications marked read.',
    this.noNotifications = 'No notifications here yet.',
    this.noNotificationsHint =
        'New reminders, offers and receipts will appear in this inbox.',
    this.loadError = 'Could not load your notifications.',
    this.preferenceLoadError = 'Could not load your preferences.',
    this.retry = 'Try again',
    this.email = 'Email',
    this.push = 'Push',
    this.inApp = 'In-app',
    this.preferenceUpdated = 'Preference updated.',
    this.devicesTitle = 'Devices',
    this.devicesNote =
        'Register a device token to receive push notifications on this app.',
    this.noDevices = 'No push devices registered.',
    this.registerDevice = 'Register this device',
    this.removeDevice = 'Remove device',
    this.deviceRegistered = 'Device registered.',
    this.deviceRemoved = 'Device removed.',
    this.registeredOn = 'Registered',
    this.recipient = 'Recipient',
    this.template = 'Template',
    this.received = 'Created',
    this.scheduled = 'Scheduled',
    this.failureReason = 'Failure reason',
    this.status = 'Status',
    this.category = 'Category',
    this.channel = 'Channel',
    this.refresh = 'Refresh',
    this.preferencesTooltip = 'Notification preferences',
    this.actionFailed = 'The request could not be completed.',
    this.demoDevicePlatform = 'web',
    this.demoDeviceToken = 'demo-web-device-token',
  });

  /// Small brand label.
  final String brand;

  /// Inbox screen title.
  final String inboxTitle;

  /// Eyebrow above the unread summary.
  final String inboxEyebrow;

  /// Inbox subtitle.
  final String inboxSubtitle;

  /// Detail screen title.
  final String detailTitle;

  /// Detail eyebrow.
  final String detailEyebrow;

  /// Preferences screen title.
  final String preferencesTitle;

  /// Preferences eyebrow.
  final String preferencesEyebrow;

  /// Preferences subtitle.
  final String preferencesSubtitle;

  /// All-items filter label.
  final String all;

  /// Unread-only filter label.
  final String unread;

  /// Category filter label.
  final String categoryFilter;

  /// Channel filter label.
  final String channelFilter;

  /// Empty category selection label.
  final String allCategories;

  /// Empty channel selection label.
  final String allChannels;

  /// Clear-filters action.
  final String clearFilters;

  /// Mark-all-read action.
  final String markAllRead;

  /// Mark-one-read action.
  final String markRead;

  /// Read state label.
  final String read;

  /// Mark-read success message.
  final String readSnack;

  /// Mark-all-read success message.
  final String allReadSnack;

  /// Empty inbox message.
  final String noNotifications;

  /// Empty inbox hint.
  final String noNotificationsHint;

  /// Inbox load failure.
  final String loadError;

  /// Preferences load failure.
  final String preferenceLoadError;

  /// Retry action.
  final String retry;

  /// E-mail channel label.
  final String email;

  /// Push channel label.
  final String push;

  /// In-app channel label.
  final String inApp;

  /// Preference update success message.
  final String preferenceUpdated;

  /// Device section title.
  final String devicesTitle;

  /// Device section note.
  final String devicesNote;

  /// Empty device message.
  final String noDevices;

  /// Device registration action.
  final String registerDevice;

  /// Device removal tooltip.
  final String removeDevice;

  /// Device registration success message.
  final String deviceRegistered;

  /// Device removal success message.
  final String deviceRemoved;

  /// Device registration date label.
  final String registeredOn;

  /// Recipient fact label.
  final String recipient;

  /// Template fact label.
  final String template;

  /// Creation fact label.
  final String received;

  /// Scheduled fact label.
  final String scheduled;

  /// Failure fact label.
  final String failureReason;

  /// Status fact label.
  final String status;

  /// Category fact label.
  final String category;

  /// Channel fact label.
  final String channel;

  /// Refresh tooltip.
  final String refresh;

  /// Preferences navigation tooltip.
  final String preferencesTooltip;

  /// Generic action failure.
  final String actionFailed;

  /// Platform used by the demo registration action.
  final String demoDevicePlatform;

  /// Token prefix used by the demo registration action.
  final String demoDeviceToken;

  /// Formats a full date.
  String date(DateTime value) =>
      '${_months[value.month - 1]} ${value.day}, ${value.year}';

  /// Formats a date and time.
  String dateTime(DateTime value) {
    final local = value.toLocal();
    return '${date(local)} · ${_two(local.hour)}:${_two(local.minute)}';
  }

  /// Formats a month and day.
  String monthDay(DateTime value) => '${_months[value.month - 1]} ${value.day}';

  /// Formats a notification count.
  String notificationCount(int count) =>
      count == 1 ? '1 notification' : '$count notifications';

  /// Formats the unread summary.
  String unreadSummary(int count) => switch (count) {
    0 => 'You are all caught up.',
    1 => '1 unread update',
    _ => '$count unread updates',
  };

  /// Formats the visible result count.
  String showingCount(int count) =>
      count == 1 ? 'Showing 1 message' : 'Showing $count messages';

  /// Human label for a category key.
  String categoryLabel(String key) => switch (key) {
    'class_reminders' => 'Class reminders',
    'membership_expiry' => 'Membership',
    'offers' => 'Offers',
    'receipts' => 'Receipts',
    _ => key.replaceAll('_', ' '),
  };

  /// Description for a category key.
  String categoryDescription(String key) => switch (key) {
    'class_reminders' => 'Bookings, reminders and room changes',
    'membership_expiry' => 'Expiry and renewal reminders',
    'offers' => 'Offers, news and re-engagement',
    'receipts' => 'Payments and receipts',
    _ => '',
  };

  /// Human label for a delivery channel.
  String channelLabel(String channel) => switch (channel.toLowerCase()) {
    'email' => email,
    'push' => push,
    'inapp' => inApp,
    _ => channel,
  };

  /// Description for a delivery channel.
  String channelDescription(String channel) => switch (channel.toLowerCase()) {
    'email' => 'Inbox delivery',
    'push' => 'Device notifications',
    'inapp' => 'Kinevor inbox',
    _ => '',
  };

  /// Human label for a lifecycle status.
  String statusLabel(String status) => switch (status.toLowerCase()) {
    'queued' => 'Queued',
    'sent' => 'Delivered',
    'read' => 'Read',
    'failed' => 'Failed',
    _ => status,
  };

  /// Human label for a device platform.
  String platformLabel(String platform) => switch (platform.toLowerCase()) {
    'ios' => 'iPhone',
    'android' => 'Android',
    'web' => 'Web browser',
    _ => platform.isEmpty ? 'Device' : platform,
  };

  static String _two(int value) => value.toString().padLeft(2, '0');

  static const List<String> _months = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'May',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Oct',
    'Nov',
    'Dec',
  ];
}
