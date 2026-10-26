/// Notification categories exposed by the notifications module.
const List<String> notificationCategories = [
  'class_reminders',
  'membership_expiry',
  'offers',
  'receipts',
];

/// Delivery channels exposed by the notifications module.
const List<String> notificationChannels = ['Email', 'Push', 'InApp'];

/// One notification record in the member inbox.
class NotificationRecord {
  /// Creates a notification record.
  const NotificationRecord({
    required this.id,
    required this.memberId,
    required this.title,
    required this.body,
    required this.channel,
    required this.status,
    required this.category,
    required this.unread,
    required this.createdAt,
    this.recipientEmail,
    this.templateKey,
    this.scheduledAt,
    this.sentAt,
    this.readAt,
    this.failureReason,
  });

  /// Stable notification identifier.
  final String id;

  /// Member that owns the notification.
  final String memberId;

  /// Notification title.
  final String title;

  /// Notification body.
  final String body;

  /// Delivery channel: `Email`, `Push` or `InApp`.
  final String channel;

  /// Lifecycle status: `Queued`, `Sent`, `Read` or `Failed`.
  final String status;

  /// Preference category key.
  final String category;

  /// Masked recipient e-mail, when the record used the e-mail channel.
  final String? recipientEmail;

  /// Template key used to render the message, when any.
  final String? templateKey;

  /// Planned delivery time for queued notifications.
  final DateTime? scheduledAt;

  /// When the delivery simulation accepted the record.
  final DateTime? sentAt;

  /// When the member read the record.
  final DateTime? readAt;

  /// Why delivery failed, when it did.
  final String? failureReason;

  /// Whether the record is waiting to be read.
  final bool unread;

  /// Creation timestamp.
  final DateTime createdAt;

  /// Whether delivery failed.
  bool get failed => status.toLowerCase() == 'failed';

  /// Whether the record has been read.
  bool get read => !unread && status.toLowerCase() == 'read';

  /// Returns a copy with the supplied read state.
  NotificationRecord copyWith({
    bool? unread,
    String? status,
    DateTime? readAt,
  }) => NotificationRecord(
    id: id,
    memberId: memberId,
    title: title,
    body: body,
    channel: channel,
    status: status ?? this.status,
    category: category,
    recipientEmail: recipientEmail,
    templateKey: templateKey,
    scheduledAt: scheduledAt,
    sentAt: sentAt,
    readAt: readAt ?? this.readAt,
    failureReason: failureReason,
    unread: unread ?? this.unread,
    createdAt: createdAt,
  );

  /// Parses a defensive notification payload.
  factory NotificationRecord.fromJson(Map<String, dynamic> json) =>
      NotificationRecord(
        id: json['id']?.toString() ?? '',
        memberId: json['memberId']?.toString() ?? '',
        title: json['title']?.toString() ?? '',
        body: json['body']?.toString() ?? '',
        channel: json['channel']?.toString() ?? '',
        status: json['status']?.toString() ?? '',
        category: json['category']?.toString() ?? '',
        recipientEmail: json['recipientEmail']?.toString(),
        templateKey: json['templateKey']?.toString(),
        scheduledAt: DateTime.tryParse(json['scheduledAt']?.toString() ?? ''),
        sentAt: DateTime.tryParse(json['sentAt']?.toString() ?? ''),
        readAt: DateTime.tryParse(json['readAt']?.toString() ?? ''),
        failureReason: json['failureReason']?.toString(),
        unread: json['unread'] == true,
        createdAt:
            DateTime.tryParse(json['createdAt']?.toString() ?? '') ??
            DateTime(1970),
      );
}

/// One page of notifications returned by the inbox endpoint.
class NotificationPage {
  /// Creates a notification page.
  const NotificationPage({
    required this.items,
    required this.totalCount,
    required this.page,
    required this.pageSize,
  });

  /// Notifications on this page.
  final List<NotificationRecord> items;

  /// Total number of matching notifications.
  final int totalCount;

  /// One-based page number.
  final int page;

  /// Maximum page size requested.
  final int pageSize;

  /// Parses a defensive paged payload.
  factory NotificationPage.fromJson(Map<String, dynamic> json) =>
      NotificationPage(
        items: _list(json['items'], NotificationRecord.fromJson),
        totalCount: (json['totalCount'] as num?)?.toInt() ?? 0,
        page: (json['page'] as num?)?.toInt() ?? 1,
        pageSize: (json['pageSize'] as num?)?.toInt() ?? 20,
      );
}

/// Per-member channel flags for one notification category.
class NotificationPreference {
  /// Creates a preference row.
  const NotificationPreference({
    required this.category,
    required this.email,
    required this.push,
    required this.inApp,
  });

  /// Category key, for example `class_reminders`.
  final String category;

  /// Whether e-mail delivery is enabled.
  final bool email;

  /// Whether push delivery is enabled.
  final bool push;

  /// Whether in-app delivery is enabled.
  final bool inApp;

  /// Whether [channel] is enabled for this category.
  bool isEnabled(String channel) => switch (channel) {
    'Email' => email,
    'Push' => push,
    _ => inApp,
  };

  /// Returns a copy with one channel replaced.
  NotificationPreference copyWith({bool? email, bool? push, bool? inApp}) =>
      NotificationPreference(
        category: category,
        email: email ?? this.email,
        push: push ?? this.push,
        inApp: inApp ?? this.inApp,
      );

  /// Parses a defensive preference payload.
  factory NotificationPreference.fromJson(Map<String, dynamic> json) =>
      NotificationPreference(
        category: json['category']?.toString() ?? '',
        email: json['email'] == true,
        push: json['push'] == true,
        inApp: json['inApp'] == true,
      );
}

/// Registered push device with a masked token preview.
class NotificationDevice {
  /// Creates a device projection.
  const NotificationDevice({
    required this.id,
    required this.platform,
    required this.tokenPreview,
    required this.registeredAt,
  });

  /// Stable device identifier.
  final String id;

  /// Platform name: `ios`, `android` or `web`.
  final String platform;

  /// Masked token preview.
  final String tokenPreview;

  /// When the device was registered.
  final DateTime registeredAt;

  /// Parses a defensive device payload.
  factory NotificationDevice.fromJson(Map<String, dynamic> json) =>
      NotificationDevice(
        id: json['id']?.toString() ?? '',
        platform: json['platform']?.toString() ?? '',
        tokenPreview: json['tokenPreview']?.toString() ?? '',
        registeredAt:
            DateTime.tryParse(json['registeredAt']?.toString() ?? '') ??
            DateTime(1970),
      );
}

List<T> _list<T>(Object? value, T Function(Map<String, dynamic>) parse) {
  if (value is! List) return const [];
  return [
    for (final item in value)
      if (item is Map) parse(Map<String, dynamic>.from(item)),
  ];
}
