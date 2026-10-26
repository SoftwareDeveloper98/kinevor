import 'package:flutter/material.dart';

/// "Architectural Obsidian" palette and theme used by the staff dashboard.
abstract final class KinevorStaffDashboardTheme {
  /// Bundled Sora family, namespaced by Flutter for package assets.
  static const String soraFontFamily = 'packages/kinevor_staff_dashboard/Sora';

  /// App background ink.
  static const Color ink = Color(0xFF111317);

  /// Raised surface.
  static const Color surface = Color(0xFF16181D);

  /// Card and sheet surface.
  static const Color surfaceRaised = Color(0xFF1E2127);

  /// Hairline border.
  static const Color border = Color(0xFF2A2E36);

  /// Primary text.
  static const Color text = Color(0xFFF4F5F7);

  /// Secondary text.
  static const Color textMuted = Color(0xFF8E949E);

  /// Brand crimson accent.
  static const Color accent = Color(0xFFFF453A);

  /// Accent pressed state.
  static const Color accentPressed = Color(0xFFE0342A);

  /// Text on accent fills.
  static const Color onAccent = Color(0xFFFFFFFF);

  /// Success green.
  static const Color success = Color(0xFF34D399);

  /// Warning amber.
  static const Color warning = Color(0xFFFBBF24);

  /// Error red.
  static const Color error = Color(0xFFFF6B6B);

  /// Info blue.
  static const Color info = Color(0xFF4DAEFF);

  /// Warm KPI accent.
  static const Color warm = Color(0xFFFF7A00);

  /// Cool KPI accent.
  static const Color cool = Color(0xFF42A5F5);

  /// Violet KPI accent.
  static const Color violet = Color(0xFFAB47BC);

  /// Teal KPI accent.
  static const Color teal = Color(0xFF34D399);

  /// Blue KPI accent.
  static const Color blue = Color(0xFF60A5FA);

  /// Cyan activity accent.
  static const Color cyan = Color(0xFF22D3EE);

  /// Brand gradient used by the hero.
  static const LinearGradient brandGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [accent, Color(0xFFFF8A65)],
  );

  /// Check-in bar gradient.
  static const LinearGradient checkInBars = LinearGradient(
    begin: Alignment.bottomCenter,
    end: Alignment.topCenter,
    colors: [Color(0xFFB3341A), warm],
  );

  /// Booking bar gradient.
  static const LinearGradient bookingBars = LinearGradient(
    begin: Alignment.bottomCenter,
    end: Alignment.topCenter,
    colors: [Color(0xFF1C4E80), cool],
  );

  /// Color assigned to an alert [severity].
  static Color severityColor(String severity) =>
      switch (severity.toLowerCase()) {
        'critical' => error,
        'warning' => warning,
        'info' => info,
        _ => textMuted,
      };

  /// Icon assigned to an alert [severity].
  static IconData severityIcon(String severity) =>
      switch (severity.toLowerCase()) {
        'critical' => Icons.priority_high,
        'warning' => Icons.warning_amber_outlined,
        'info' => Icons.info_outline,
        _ => Icons.report_problem_outlined,
      };

  /// Icon assigned to an alert [type].
  static IconData alertTypeIcon(String type) => switch (type) {
    'OverdueInvoice' => Icons.receipt_long_outlined,
    'ExpiringMembership' => Icons.event_busy_outlined,
    'LowClassFill' => Icons.groups_outlined,
    'FailedNotification' => Icons.notification_important_outlined,
    _ => Icons.report_problem_outlined,
  };

  /// Icon assigned to an activity [kind].
  static IconData activityIcon(String kind) => switch (kind) {
    'CheckIn' => Icons.login,
    'Payment' => Icons.payments_outlined,
    'MemberJoined' => Icons.person_add_alt_1_outlined,
    'ClassBooking' => Icons.event_available_outlined,
    'Membership' => Icons.card_membership_outlined,
    'Notification' => Icons.notifications_none,
    _ => Icons.bolt_outlined,
  };

  /// Color assigned to an activity [kind].
  static Color activityColor(String kind) => switch (kind) {
    'CheckIn' => success,
    'Payment' => warning,
    'MemberJoined' => warm,
    'ClassBooking' => blue,
    'Membership' => violet,
    'Notification' => cyan,
    _ => textMuted,
  };

  /// Builds the dark staff dashboard theme with Sora typography.
  static ThemeData dark({String fontFamily = soraFontFamily}) {
    const scheme = ColorScheme(
      brightness: Brightness.dark,
      primary: accent,
      onPrimary: onAccent,
      secondary: accent,
      onSecondary: onAccent,
      error: error,
      onError: onAccent,
      surface: surface,
      onSurface: text,
      onSurfaceVariant: textMuted,
      surfaceContainerHighest: surfaceRaised,
      surfaceContainerHigh: surfaceRaised,
      surfaceContainerLow: surface,
      surfaceContainerLowest: ink,
      surfaceContainer: surfaceRaised,
      outline: border,
      outlineVariant: border,
    );

    final textTheme = const TextTheme(
      headlineMedium: TextStyle(
        fontSize: 24,
        fontWeight: FontWeight.w800,
        height: 1.2,
      ),
      headlineSmall: TextStyle(
        fontSize: 19,
        fontWeight: FontWeight.w700,
        height: 1.25,
      ),
      titleLarge: TextStyle(
        fontSize: 17,
        fontWeight: FontWeight.w700,
        height: 1.3,
      ),
      titleMedium: TextStyle(
        fontSize: 14,
        fontWeight: FontWeight.w600,
        height: 1.3,
      ),
      bodyLarge: TextStyle(fontSize: 15, height: 1.5),
      bodyMedium: TextStyle(fontSize: 14, height: 1.5),
      bodySmall: TextStyle(fontSize: 12, height: 1.45),
      labelLarge: TextStyle(fontSize: 13, fontWeight: FontWeight.w700),
      labelSmall: TextStyle(fontSize: 11, fontWeight: FontWeight.w600),
    ).apply(fontFamily: fontFamily, bodyColor: text, displayColor: text);

    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.dark,
      fontFamily: fontFamily,
      colorScheme: scheme,
      scaffoldBackgroundColor: ink,
      canvasColor: ink,
      textTheme: textTheme,
      appBarTheme: AppBarTheme(
        backgroundColor: ink,
        foregroundColor: text,
        elevation: 0,
        centerTitle: false,
        surfaceTintColor: Colors.transparent,
        titleTextStyle: textTheme.headlineSmall?.copyWith(
          fontWeight: FontWeight.w800,
        ),
      ),
      cardTheme: CardThemeData(
        color: surfaceRaised,
        elevation: 0,
        margin: EdgeInsets.zero,
        clipBehavior: Clip.antiAlias,
        surfaceTintColor: Colors.transparent,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
          side: const BorderSide(color: border),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: surface,
        isDense: true,
        contentPadding: const EdgeInsets.symmetric(
          horizontal: 16,
          vertical: 14,
        ),
        hintStyle: textTheme.bodyMedium?.copyWith(color: textMuted),
        prefixIconColor: textMuted,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: BorderSide.none,
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: border),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: accent, width: 2),
        ),
      ),
      progressIndicatorTheme: const ProgressIndicatorThemeData(color: accent),
      dividerTheme: const DividerThemeData(
        color: border,
        space: 1,
        thickness: 1,
      ),
      snackBarTheme: SnackBarThemeData(
        backgroundColor: surfaceRaised,
        contentTextStyle: textTheme.bodyMedium?.copyWith(color: text),
        behavior: SnackBarBehavior.floating,
      ),
      bottomSheetTheme: const BottomSheetThemeData(
        backgroundColor: surfaceRaised,
        surfaceTintColor: Colors.transparent,
        showDragHandle: true,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
        ),
      ),
    );
  }
}
