import '../models/notification_models.dart';
import 'kinevor_notifications_client.dart';

/// Data source for the member notification screens.
abstract interface class NotificationsRepository {
  /// Loads a page of notifications for the authenticated member.
  Future<NotificationPage> getNotifications({
    String? category,
    String? channel,
    bool unreadOnly = false,
    int page = 1,
    int pageSize = 50,
  });

  /// Loads the unread notification count.
  Future<int> getUnreadCount();

  /// Loads one notification belonging to the authenticated member.
  Future<NotificationRecord> getNotification(String id);

  /// Marks one notification as read.
  Future<void> markRead(String id);

  /// Marks every delivered notification as read.
  Future<int> markAllRead();

  /// Loads channel preferences for every category.
  Future<List<NotificationPreference>> getPreferences();

  /// Updates the channel flags for one category.
  Future<List<NotificationPreference>> updatePreference({
    required String category,
    required bool email,
    required bool push,
    required bool inApp,
  });

  /// Loads registered push devices.
  Future<List<NotificationDevice>> getDevices();

  /// Registers or re-assigns a push device token.
  Future<NotificationDevice> registerDevice({
    required String token,
    required String platform,
  });

  /// Unregisters a device.
  Future<void> unregisterDevice(String id);
}

/// [NotificationsRepository] backed by [KinevorNotificationsClient].
class RemoteNotificationsRepository implements NotificationsRepository {
  /// Creates a repository over [client].
  const RemoteNotificationsRepository(this.client);

  /// HTTP client used for every call.
  final KinevorNotificationsClient client;

  @override
  Future<NotificationPage> getNotifications({
    String? category,
    String? channel,
    bool unreadOnly = false,
    int page = 1,
    int pageSize = 50,
  }) => client.fetchNotifications(
    category: category,
    channel: channel,
    unreadOnly: unreadOnly,
    page: page,
    pageSize: pageSize,
  );

  @override
  Future<int> getUnreadCount() => client.fetchUnreadCount();

  @override
  Future<NotificationRecord> getNotification(String id) =>
      client.fetchNotification(id);

  @override
  Future<void> markRead(String id) => client.markRead(id);

  @override
  Future<int> markAllRead() => client.markAllRead();

  @override
  Future<List<NotificationPreference>> getPreferences() =>
      client.fetchPreferences();

  @override
  Future<List<NotificationPreference>> updatePreference({
    required String category,
    required bool email,
    required bool push,
    required bool inApp,
  }) => client.updatePreference(
    category: category,
    email: email,
    push: push,
    inApp: inApp,
  );

  @override
  Future<List<NotificationDevice>> getDevices() => client.fetchDevices();

  @override
  Future<NotificationDevice> registerDevice({
    required String token,
    required String platform,
  }) => client.registerDevice(token: token, platform: platform);

  @override
  Future<void> unregisterDevice(String id) => client.unregisterDevice(id);
}
