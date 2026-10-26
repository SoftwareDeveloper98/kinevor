import 'package:kinevor_notifications/kinevor_notifications.dart';

class FakeNotificationsRepository implements NotificationsRepository {
  FakeNotificationsRepository({this.failure});

  final Object? failure;
  int notificationCalls = 0;
  int unreadCountCalls = 0;
  int detailCalls = 0;
  int markReadCalls = 0;
  int markAllReadCalls = 0;
  int preferenceCalls = 0;
  int updatePreferenceCalls = 0;
  int deviceCalls = 0;
  int registerDeviceCalls = 0;
  int unregisterDeviceCalls = 0;
  String? lastCategory;
  String? lastChannel;
  bool? lastUnreadOnly;
  String? lastMarkedReadId;
  String? lastPreferenceCategory;
  bool? lastPreferenceEmail;
  bool? lastPreferencePush;
  bool? lastPreferenceInApp;
  String? lastRegisteredToken;
  String? lastRegisteredPlatform;
  String? lastUnregisteredDeviceId;

  final List<NotificationRecord> _notifications = [...sampleNotifications];
  final List<NotificationPreference> _preferences = [...samplePreferences];
  final List<NotificationDevice> _devices = [...sampleDevices];

  void _check() {
    if (failure != null) throw failure!;
  }

  @override
  Future<NotificationPage> getNotifications({
    String? category,
    String? channel,
    bool unreadOnly = false,
    int page = 1,
    int pageSize = 50,
  }) async {
    notificationCalls++;
    lastCategory = category;
    lastChannel = channel;
    lastUnreadOnly = unreadOnly;
    _check();
    final filtered = _notifications.where((record) {
      if (unreadOnly && !record.unread) return false;
      if (category != null && record.category != category) return false;
      if (channel != null &&
          record.channel.toLowerCase() != channel.toLowerCase()) {
        return false;
      }
      return true;
    }).toList();
    return NotificationPage(
      items: filtered,
      totalCount: filtered.length,
      page: page,
      pageSize: pageSize,
    );
  }

  @override
  Future<int> getUnreadCount() async {
    unreadCountCalls++;
    _check();
    return _notifications.where((record) => record.unread).length;
  }

  @override
  Future<NotificationRecord> getNotification(String id) async {
    detailCalls++;
    _check();
    for (final record in _notifications) {
      if (record.id == id) return record;
    }
    throw const NotificationsApiException(
      'Notification was not found.',
      statusCode: 404,
    );
  }

  @override
  Future<void> markRead(String id) async {
    markReadCalls++;
    lastMarkedReadId = id;
    _check();
    final index = _notifications.indexWhere((record) => record.id == id);
    if (index < 0) {
      throw const NotificationsApiException(
        'Notification was not found.',
        statusCode: 404,
      );
    }
    _notifications[index] = _notifications[index].copyWith(
      unread: false,
      status: 'Read',
      readAt: DateTime(2026, 10, 20, 12),
    );
  }

  @override
  Future<int> markAllRead() async {
    markAllReadCalls++;
    _check();
    var marked = 0;
    for (var index = 0; index < _notifications.length; index++) {
      if (!_notifications[index].unread) continue;
      _notifications[index] = _notifications[index].copyWith(
        unread: false,
        status: 'Read',
        readAt: DateTime(2026, 10, 20, 12),
      );
      marked++;
    }
    return marked;
  }

  @override
  Future<List<NotificationPreference>> getPreferences() async {
    preferenceCalls++;
    _check();
    return [..._preferences];
  }

  @override
  Future<List<NotificationPreference>> updatePreference({
    required String category,
    required bool email,
    required bool push,
    required bool inApp,
  }) async {
    updatePreferenceCalls++;
    lastPreferenceCategory = category;
    lastPreferenceEmail = email;
    lastPreferencePush = push;
    lastPreferenceInApp = inApp;
    _check();
    final index = _preferences.indexWhere(
      (preference) => preference.category == category,
    );
    if (index >= 0) {
      _preferences[index] = NotificationPreference(
        category: category,
        email: email,
        push: push,
        inApp: inApp,
      );
    }
    return [..._preferences];
  }

  @override
  Future<List<NotificationDevice>> getDevices() async {
    deviceCalls++;
    _check();
    return [..._devices];
  }

  @override
  Future<NotificationDevice> registerDevice({
    required String token,
    required String platform,
  }) async {
    registerDeviceCalls++;
    lastRegisteredToken = token;
    lastRegisteredPlatform = platform;
    _check();
    final device = NotificationDevice(
      id: 'device-${_devices.length + 1}',
      platform: platform,
      tokenPreview:
          '${token.substring(0, 6)}...${token.substring(token.length - 4)}',
      registeredAt: DateTime(2026, 10, 20),
    );
    _devices.add(device);
    return device;
  }

  @override
  Future<void> unregisterDevice(String id) async {
    unregisterDeviceCalls++;
    lastUnregisteredDeviceId = id;
    _check();
    _devices.removeWhere((device) => device.id == id);
  }
}

final sampleNotifications = [
  NotificationRecord(
    id: 'notification-1',
    memberId: '11111111-1111-1111-1111-111111111111',
    title: 'Class reminder: Strength Circuit',
    body:
        'Hi Maya, your Strength Circuit class starts tomorrow at 18:30. '
        'Bring a water bottle and arrive five minutes early.',
    channel: 'InApp',
    status: 'Sent',
    category: 'class_reminders',
    unread: true,
    createdAt: DateTime(2026, 10, 20, 9, 30),
    sentAt: DateTime(2026, 10, 20, 9, 31),
    templateKey: 'class_reminder',
  ),
  NotificationRecord(
    id: 'notification-2',
    memberId: '11111111-1111-1111-1111-111111111111',
    title: 'Membership expiring soon',
    body:
        'Hi Maya, your membership expires in 5 days. '
        'Renew to keep your access.',
    channel: 'InApp',
    status: 'Read',
    category: 'membership_expiry',
    unread: false,
    createdAt: DateTime(2026, 10, 18, 8, 15),
    sentAt: DateTime(2026, 10, 18, 8, 16),
    readAt: DateTime(2026, 10, 18, 10, 5),
  ),
  NotificationRecord(
    id: 'notification-3',
    memberId: '11111111-1111-1111-1111-111111111111',
    title: 'Member-only offer: 20% off personal training',
    body: 'Book a personal training block before Sunday and save 20%.',
    channel: 'InApp',
    status: 'Sent',
    category: 'offers',
    unread: true,
    createdAt: DateTime(2026, 10, 19, 14, 0),
    sentAt: DateTime(2026, 10, 19, 14, 1),
    templateKey: 'offer_announcement',
  ),
  NotificationRecord(
    id: 'notification-4',
    memberId: '11111111-1111-1111-1111-111111111111',
    title: 'Receipt for October membership',
    body: 'Hi Maya, we received GBP 49.00 for your October membership.',
    channel: 'Email',
    status: 'Sent',
    category: 'receipts',
    recipientEmail: 'm***@kinevor.demo',
    unread: true,
    createdAt: DateTime(2026, 10, 17, 16, 45),
    sentAt: DateTime(2026, 10, 17, 16, 46),
    templateKey: 'payment_receipt',
  ),
  NotificationRecord(
    id: 'notification-5',
    memberId: '11111111-1111-1111-1111-111111111111',
    title: 'Class starts in one hour',
    body: 'Your Strength Circuit class starts at 18:30. See you soon.',
    channel: 'Push',
    status: 'Sent',
    category: 'class_reminders',
    unread: true,
    createdAt: DateTime(2026, 10, 20, 17, 30),
    sentAt: DateTime(2026, 10, 20, 17, 31),
  ),
];

final samplePreferences = [
  const NotificationPreference(
    category: 'class_reminders',
    email: true,
    push: true,
    inApp: true,
  ),
  const NotificationPreference(
    category: 'membership_expiry',
    email: true,
    push: true,
    inApp: true,
  ),
  const NotificationPreference(
    category: 'offers',
    email: false,
    push: true,
    inApp: true,
  ),
  const NotificationPreference(
    category: 'receipts',
    email: true,
    push: false,
    inApp: true,
  ),
];

final sampleDevices = [
  NotificationDevice(
    id: 'device-1',
    platform: 'ios',
    tokenPreview: 'demo-i...0001',
    registeredAt: DateTime(2026, 7, 22),
  ),
];
