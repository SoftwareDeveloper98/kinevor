const UNKNOWN_ERROR = 'Something went wrong. Please try again.';

/** Returns a human-readable message from an API or client error. */
export function checkInErrorMessage(error: unknown): string {
  if (!error || typeof error !== 'object') return UNKNOWN_ERROR;

  const candidate = error as {
    readonly error?: { readonly message?: unknown; readonly errors?: unknown };
    readonly message?: unknown;
  };
  const apiError = candidate.error;

  if (apiError?.message && typeof apiError.message === 'string') return apiError.message;
  if (Array.isArray(apiError?.errors)) {
    const first = apiError.errors.find((item) => typeof item === 'string');
    if (typeof first === 'string') return first;
  }
  if (typeof candidate.message === 'string' && candidate.message.length > 0) {
    return candidate.message;
  }
  return UNKNOWN_ERROR;
}

/** Formats an ISO date-time for a compact UK-style display. */
export function formatCheckInDate(value: string | null | undefined): string {
  if (!value) return '—';
  return new Intl.DateTimeFormat('en-GB', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value));
}

/** Formats a date without time. */
export function formatCheckInDay(value: string | null | undefined): string {
  if (!value) return '—';
  return new Intl.DateTimeFormat('en-GB', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
  }).format(new Date(value));
}

/** Formats minutes as `1h 24m`, or `—` when unknown. */
export function formatDuration(minutes: number | null | undefined): string {
  if (minutes === null || minutes === undefined) return '—';
  if (minutes < 60) return `${minutes}m`;
  const hours = Math.floor(minutes / 60);
  const remainder = minutes % 60;
  return remainder === 0 ? `${hours}h` : `${hours}h ${remainder}m`;
}

/** Formats an hour number as a zero-padded 24-hour label. */
export function formatHour(hour: number | null | undefined): string {
  if (hour === null || hour === undefined) return '—';
  return `${String(hour).padStart(2, '0')}:00`;
}

/** Returns initials for an avatar. */
export function initials(fullName: string): string {
  const parts = fullName.trim().split(/\s+/).slice(0, 2);
  return parts.map((part) => part.charAt(0).toUpperCase()).join('') || '?';
}

/** Converts an occupancy utilisation percentage into a CSS-safe width. */
export function utilizationWidth(percent: number | null | undefined): string {
  const value = Math.max(0, Math.min(100, percent ?? 0));
  return `${value}%`;
}
