const List<String> _weekdays = <String>[
  'Mon',
  'Tue',
  'Wed',
  'Thu',
  'Fri',
  'Sat',
  'Sun',
];

const List<String> _months = <String>[
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

/// Formats a local instant as `HH:mm`.
String formatClock(DateTime value) {
  final hour = value.hour.toString().padLeft(2, '0');
  final minute = value.minute.toString().padLeft(2, '0');
  return '$hour:$minute';
}

/// Formats a local instant as `Wed 21 Oct`.
String formatDay(DateTime value) {
  final weekday = _weekdays[value.weekday - 1];
  return '$weekday ${value.day} ${_months[value.month - 1]}';
}

/// Formats a local instant as `21 Oct 2026`.
String formatDate(DateTime value) =>
    '${value.day} ${_months[value.month - 1]} ${value.year}';

/// Returns a two-letter month abbreviation.
String monthShort(DateTime value) => _months[value.month - 1];

/// Formats minutes as `45m` or `1h 15m`.
String formatMinutes(int? minutes) {
  if (minutes == null) return '—';
  if (minutes < 60) return '${minutes}m';
  final hours = minutes ~/ 60;
  final remainder = minutes % 60;
  return remainder == 0 ? '${hours}h' : '${hours}h ${remainder}m';
}

/// Formats a duration between two instants.
String formatVisitDuration(DateTime from, DateTime? to) {
  final end = to ?? DateTime.now();
  final minutes = end.difference(from).inMinutes;
  return formatMinutes(minutes < 0 ? 0 : minutes);
}
