import 'package:flutter/material.dart';

/// "Architectural Obsidian" palette and theme used by the check-ins module.
abstract final class KinevorCheckInsTheme {
  /// App background ink.
  static const Color ink = Color(0xFF111317);

  /// Input and inset surface.
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

  /// Text on accent fills.
  static const Color onAccent = Color(0xFFFFFFFF);

  /// Success green used for allowed access and open visits.
  static const Color success = Color(0xFF34D399);

  /// Warning amber used for a busy club.
  static const Color warning = Color(0xFFFBBF24);

  /// Error red used for denied access.
  static const Color error = Color(0xFFFF6B6B);

  /// Info blue.
  static const Color info = Color(0xFF4DAEFF);

  /// Brand gradient used by avatars and the primary action.
  static const LinearGradient brandGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFFA52C00), Color(0xFFFF7A00)],
  );

  /// Success gradient used for the active-visit card.
  static const LinearGradient activeGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFF143D2E), Color(0xFF1E2127)],
  );

  /// Returns the status colour for an occupancy [status].
  static Color occupancyColor(String status) => switch (status.toLowerCase()) {
    'busy' => accent,
    'nearlyfull' => warning,
    'full' => error,
    _ => success,
  };

  /// Builds the dark check-ins theme with Sora typography.
  static ThemeData dark({String fontFamily = 'Sora'}) {
    const scheme = ColorScheme(
      brightness: Brightness.dark,
      primary: accent,
      onPrimary: onAccent,
      secondary: success,
      onSecondary: ink,
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
        letterSpacing: -0.5,
      ),
      headlineSmall: TextStyle(
        fontSize: 19,
        fontWeight: FontWeight.w700,
        height: 1.25,
        letterSpacing: -0.3,
      ),
      titleLarge: TextStyle(
        fontSize: 18,
        fontWeight: FontWeight.w700,
        height: 1.3,
      ),
      titleMedium: TextStyle(
        fontSize: 15,
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
      splashFactory: InkSparkle.splashFactory,
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
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          minimumSize: const Size.fromHeight(54),
          backgroundColor: accent,
          foregroundColor: onAccent,
          textStyle: textTheme.labelLarge?.copyWith(fontSize: 14),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size.fromHeight(50),
          foregroundColor: text,
          side: const BorderSide(color: border),
          textStyle: textTheme.labelLarge,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(15),
          ),
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
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
      ),
      navigationBarTheme: NavigationBarThemeData(
        height: 68,
        backgroundColor: surfaceRaised,
        surfaceTintColor: Colors.transparent,
        indicatorColor: accent.withValues(alpha: 0.14),
        labelTextStyle: WidgetStateProperty.resolveWith((states) {
          final selected = states.contains(WidgetState.selected);
          return textTheme.labelSmall?.copyWith(
            color: selected ? text : textMuted,
            fontWeight: selected ? FontWeight.w700 : FontWeight.w500,
          );
        }),
        iconTheme: WidgetStateProperty.resolveWith((states) {
          final selected = states.contains(WidgetState.selected);
          return IconThemeData(color: selected ? accent : textMuted, size: 23);
        }),
      ),
    );
  }
}
