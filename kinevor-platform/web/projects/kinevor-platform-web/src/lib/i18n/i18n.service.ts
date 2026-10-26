import { Injectable, inject, signal, computed, effect, DOCUMENT } from '@angular/core';
import { Router, UrlSegment, UrlSegmentGroup } from '@angular/router';
import {
  BUNDLES,
  DEFAULT_LOCALE,
  LANGUAGES,
  LOCALES,
  Language,
  Locale,
  isRtlLocale,
  languageOf,
  localeFromTag,
} from './languages';

export { DEFAULT_LOCALE, LANGUAGES, LOCALES, isRtlLocale, languageOf, localeFromTag };
export type { Language, Locale };

/** Local-storage key under which the chosen locale is persisted. */
export const LOCALE_STORAGE_KEY = 'kinevor_lang';

/** Local-storage key under which the chosen numeral system is persisted. */
export const NUMERALS_STORAGE_KEY = 'kinevor_numerals';

/** Numeral system used to render digit runs: western `1,234` or eastern `١٬٢٣٤`. */
export type Numerals = 'western' | 'eastern';

/**
 * Type-guard checking whether a raw value is a supported numeral system.
 *
 * @param value - Candidate value.
 * @returns True when the value is `western` or `eastern`.
 */
export function isNumerals(value: unknown): value is Numerals {
  return value === 'western' || value === 'eastern';
}

function readStoredNumerals(): Numerals | null {
  try {
    const stored = localStorage.getItem(NUMERALS_STORAGE_KEY);
    return isNumerals(stored) ? stored : null;
  } catch { return null; }
}

/** Named interpolation values accepted by {@link I18nService.t}. */
export type I18nParams = Record<string, string | number>;
type Dictionary = Record<string, string>;

/**
 * Type-guard checking whether a raw string is a supported locale.
 *
 * @param value - Candidate value.
 * @returns True when the value is one of the locales in the language registry.
 */
export function isLocale(value: string | null | undefined): value is Locale {
  return typeof value === 'string' && (LOCALES as readonly string[]).includes(value);
}

const LOCALE_PATH = new RegExp(`^/(${LOCALES.join('|')})(?:/|$)`);

function localeFromPath(pathname: string | null | undefined): Locale | null {
  if (!pathname) return null;
  const match = LOCALE_PATH.exec(pathname.split('?')[0]);
  return match && isLocale(match[1]) ? match[1] : null;
}

function detectInitialLocale(): Locale {
  try {
    const fromPath = localeFromPath(typeof location === 'undefined' ? null : location.pathname);
    if (fromPath) return fromPath;
    const param = localeFromTag(new URLSearchParams(location.search).get('lang'));
    if (param) return param;
    const saved = localStorage.getItem(LOCALE_STORAGE_KEY);
    if (isLocale(saved)) return saved;
  } catch {}
  const nav = typeof navigator === 'undefined' ? '' : navigator.language;
  return localeFromTag(nav) ?? DEFAULT_LOCALE;
}

/**
 * Runtime localization service: locale detection, dictionary lookup with
 * interpolation, path prefixing and locale-aware formatting helpers.
 */
@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly document = inject(DOCUMENT);
  private readonly router = inject(Router);

  private readonly _locale = signal<Locale>(detectInitialLocale());
  /** Active locale. */
  readonly locale = this._locale.asReadonly();
  /** True when the active locale is right-to-left. */
  readonly rtl = computed(() => isRtlLocale(this._locale()));
  /** Endonym of the active locale, suitable for the language switcher. */
  readonly languageName = computed(() => languageOf(this._locale()).name);

  private static readonly EASTERN_DIGITS = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
  private readonly storedNumerals = readStoredNumerals();
  private readonly _numerals = signal<Numerals>(this.storedNumerals ?? 'western');
  private readonly _numeralsUserChosen = signal(this.storedNumerals !== null);
  /** Active numeral system for digit runs in formatted output. */
  readonly numerals = this._numerals.asReadonly();

  private readonly _en = signal<Dictionary>(BUNDLES[DEFAULT_LOCALE]);
  private readonly _active = computed<Dictionary>(() => BUNDLES[this._locale()]);
  /** Active dictionary, exposed for change detection in pipes. */
  readonly messages = this._active;

  constructor() {
    effect(() => {
      const loc = this._locale();
      this.document.documentElement.lang = loc;
      this.document.documentElement.dir = isRtlLocale(loc) ? 'rtl' : 'ltr';
      try { localStorage.setItem(LOCALE_STORAGE_KEY, loc); } catch {}
    });
  }

  /**
   * Updates the active locale without navigating.
   *
   * @param locale - Locale to activate.
   */
  setLocale(locale: Locale): void {
    if (this._locale() === locale) return;
    this._locale.set(locale);
  }

  /**
   * Switches locale and rewrites the current route under the new locale segment.
   *
   * @param locale - Locale to activate.
   */
  switchLocale(locale: Locale): void {
    this.setLocale(locale);
    const tree = this.router.parseUrl(this.router.url);
    const primary = tree.root.children['primary'];
    if (!primary) {
      tree.root.children['primary'] = new UrlSegmentGroup([new UrlSegment(locale, {})], {});
    } else if (primary.segments.length > 0 && isLocale(primary.segments[0].path)) {
      primary.segments[0] = new UrlSegment(locale, primary.segments[0].parameters);
    } else {
      primary.segments.unshift(new UrlSegment(locale, {}));
    }
    this.router.navigateByUrl(tree);
  }

  /**
   * Activates a numeral system as an explicit user choice and persists it.
   *
   * @param value - Numeral system to activate.
   */
  setNumerals(value: Numerals): void {
    if (!isNumerals(value)) return;
    this._numeralsUserChosen.set(true);
    this._numerals.set(value);
    try { localStorage.setItem(NUMERALS_STORAGE_KEY, value); } catch {}
  }

  /**
   * Applies the market default from `GET /public/config` when the user has not
   * chosen a numeral system themselves.
   *
   * @param value - Raw `numerals` value served by the public config.
   */
  applySiteDefaultNumerals(value: string | null | undefined): void {
    if (this._numeralsUserChosen() || !isNumerals(value)) return;
    this._numerals.set(value);
  }

  private localizeDigits(text: string): string {
    if (this._numerals() !== 'eastern') return text;
    return text.replace(/[0-9]/g, digit => I18nService.EASTERN_DIGITS[Number(digit)]);
  }

  /**
   * Prefixes an application path with the active locale segment.
   *
   * @param path - Absolute application path.
   * @returns Locale-prefixed path, for example `/en/members`.
   */
  localePath(path: string): string {
    const normalized = path.startsWith('/') ? path : `/${path}`;
    return `/${this._locale()}${normalized === '/' ? '' : normalized}`;
  }

  /**
   * Resolves a translation key with optional named interpolation.
   *
   * @param key - Translation key.
   * @param params - Named interpolation values.
   * @returns Localized string, falling back to English then to the key itself.
   */
  t(key: string, params?: I18nParams): string {
    if (!key) return '';
    const active = this._active();
    let value = active[key] ?? this._en()[key] ?? key;
    if (params) {
      for (const [name, raw] of Object.entries(params)) {
        value = value.split(`{${name}}`).join(String(raw));
      }
    }
    return value;
  }

  /**
   * Checks whether a key exists in the active or English dictionary.
   *
   * @param key - Translation key.
   * @returns True when the key is known.
   */
  has(key: string): boolean {
    return key in this._active() || key in this._en();
  }

  /**
   * Produces a user-facing message for a failed request.
   *
   * @param error - Error object, typically an HTTP failure.
   * @param fallbackKey - Translation key used when nothing more specific exists.
   * @returns Localized error text.
   */
  errorText(error: unknown, fallbackKey = 'error.unknown'): string {
    const err = error as { error?: { code?: unknown; detail?: unknown; message?: unknown } } | null;
    const code = typeof err?.error?.code === 'string' ? err.error.code : '';
    if (code && this.has(`error.${code}`)) return this.t(`error.${code}`);
    const detail = err?.error?.detail ?? err?.error?.message;
    if (typeof detail === 'string' && detail) return detail;
    return this.t(fallbackKey);
  }

  /**
   * Formats a date in the active locale.
   *
   * @param value - Date, timestamp or parseable string.
   * @param opts - Intl date format options.
   * @returns Localized date, or an em dash when the value is unusable.
   */
  formatDate(
    value: string | number | Date | null | undefined,
    opts: Intl.DateTimeFormatOptions = { dateStyle: 'medium' },
  ): string {
    const date = this.toDate(value);
    if (!date) return '—';
    try { return this.localizeDigits(new Intl.DateTimeFormat(this._locale(), opts).format(date)); }
    catch { return this.localizeDigits(date.toISOString()); }
  }

  /**
   * Formats a number in the active locale.
   *
   * @param value - Number to format.
   * @param opts - Intl number format options.
   * @returns Localized number, or an em dash when the value is unusable.
   */
  formatNumber(value: number | null | undefined, opts?: Intl.NumberFormatOptions): string {
    if (value === null || value === undefined || Number.isNaN(value)) return '—';
    try { return this.localizeDigits(new Intl.NumberFormat(this._locale(), opts).format(value)); }
    catch { return this.localizeDigits(String(value)); }
  }

  /**
   * Formats an amount as currency in the active locale.
   *
   * @param value - Amount to format.
   * @param currency - ISO 4217 currency code.
   * @param opts - Intl number format options, style defaults to currency.
   * @returns Localized currency string, or an em dash when the value is unusable.
   */
  formatCurrency(
    value: number | null | undefined,
    currency = 'USD',
    opts: Intl.NumberFormatOptions = { style: 'currency', currencyDisplay: 'symbol' },
  ): string {
    if (value === null || value === undefined || Number.isNaN(value)) return '—';
    try {
      return this.localizeDigits(new Intl.NumberFormat(this._locale(), { ...opts, currency }).format(value));
    } catch { return this.localizeDigits(`${value} ${currency}`); }
  }

  /**
   * Formats a timestamp as localized relative time, for example "2 hours ago".
   *
   * @param value - Date, timestamp or parseable string.
   * @param opts - Intl relative-time format options.
   * @returns Localized relative time, or an em dash when the value is unusable.
   */
  formatRelativeTime(
    value: string | number | Date | null | undefined,
    opts: Intl.RelativeTimeFormatOptions = { numeric: 'auto' },
  ): string {
    const date = this.toDate(value);
    if (!date) return '—';
    const seconds = (date.getTime() - Date.now()) / 1000;
    const abs = Math.abs(seconds);
    const rtf = new Intl.RelativeTimeFormat(this._locale(), opts);
    const format = (amount: number, unit: Intl.RelativeTimeFormatUnit) =>
      this.localizeDigits(rtf.format(Math.round(amount), unit));
    if (abs < 60) return format(seconds, 'second');
    if (abs < 3600) return format(seconds / 60, 'minute');
    if (abs < 86400) return format(seconds / 3600, 'hour');
    if (abs < 259200) return format(seconds / 86400, 'day');
    if (abs < 31536000) return format(seconds / 259200, 'month');
    return format(seconds / 31536000, 'year');
  }

  private toDate(value: string | number | Date | null | undefined): Date | null {
    if (value === null || value === undefined || value === '') return null;
    const date = value instanceof Date ? value : new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
  }
}
