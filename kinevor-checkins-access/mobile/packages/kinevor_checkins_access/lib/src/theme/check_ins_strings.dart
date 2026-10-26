/// Localizable labels used by the check-in screens.
class CheckInsStrings {
  /// Creates the default English strings.
  const CheckInsStrings();

  /// Check-in tab title.
  String get checkInTitle => 'Check in';

  /// Visit-history tab title.
  String get historyTitle => 'My visits';

  /// Eyebrow above the member greeting.
  String get memberAccess => 'Member access';

  /// Heading on the check-in screen.
  String get welcomeBack => 'Welcome back';

  /// Supporting sentence on the check-in screen.
  String get readyToTrain => 'Your club is ready when you are.';

  /// Allowed eligibility heading.
  String get clearedToTrain => "You're cleared to train";

  /// Denied eligibility heading.
  String get accessBlocked => 'Access currently blocked';

  /// Live occupancy card title.
  String get liveOccupancy => 'Live club occupancy';

  /// Occupancy status label.
  String get comfortable => 'Comfortable';

  /// Label for an open visit.
  String get checkedIn => "You're checked in";

  /// Check-in action.
  String get checkIn => 'Check in';

  /// Disabled action when access is denied.
  String get accessUnavailable => 'Check-in unavailable';

  /// Check-out action.
  String get checkOut => 'Check out';

  /// Label shown before the check-in instant.
  String get since => 'Since';

  /// Last-visit card heading.
  String get lastVisit => 'Last visit';

  /// Membership summary heading.
  String get membership => 'Membership';

  /// Monthly visit count label.
  String get visitsThisMonth => 'Visits this month';

  /// History summary heading.
  String get yourActivity => 'Your activity';

  /// History list heading.
  String get allVisits => 'All visits';

  /// Empty history message.
  String get noVisits => 'No visits yet';

  /// Empty history supporting message.
  String get noVisitsDetail =>
      'Your check-ins will appear here as soon as you enter a club.';

  /// Generic load error.
  String get loadError => "Couldn't load your access status.";

  /// History load error.
  String get historyError => "Couldn't load your visit history.";

  /// Retry action.
  String get retry => 'Try again';

  /// Refresh tooltip.
  String get refresh => 'Refresh';

  /// Load-more action.
  String get loadMore => 'Load more visits';

  /// Front-desk source label.
  String get frontDesk => 'Front desk';

  /// Member-app source label.
  String get memberApp => 'Member app';

  /// Completed status.
  String get completed => 'Completed';

  /// Open status.
  String get inside => 'Inside';

  /// Check-in success message.
  String checkedInAt(String branch) =>
      'Checked in at $branch. Have a great session.';

  /// Check-out success message.
  String get checkedOut => 'Checked out. See you next time.';

  /// Greeting with the member's first name.
  String welcome(String firstName) => 'Welcome back, $firstName';

  /// Cleared-access support text.
  String clearedAt(String branch) =>
      'Access granted for $branch. Tap below to start your visit.';

  /// Occupied vs capacity label.
  String occupancyOf(int occupied, int capacity) =>
      '$occupied of $capacity inside';

  /// Available space label.
  String spacesFree(int available) => '$available spaces free';

  /// Check-in button label for a club.
  String checkInAt(String branch) => 'Check in at $branch';

  /// Open-visit club and time sentence.
  String sinceAt(String time, String branch) => 'Since $time · $branch';

  /// History summary value.
  String visitsCount(int count) => '$count visits';

  /// Page indicator.
  String pageOf(int page, int totalPages) => 'Page $page of $totalPages';

  /// Two-up time range.
  String timeRange(String from, String to) => '$from – $to';
}
