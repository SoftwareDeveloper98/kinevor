import registry from './data/languages.json';
import enBundle from './data/en.json';
import arBundle from './data/ar.json';

/** Supported UI locales. */
export type Locale = 'en' | 'ar';

/** Metadata describing one supported UI language. */
export interface Language {
  /** BCP-47 language code used in routes, storage and bundle file names. */
  readonly code: Locale;
  /** Endonym shown in language menus, for example `العربية`. */
  readonly name: string;
  /** Compact label shown in space-constrained switchers, for example `EN`. */
  readonly short: string;
  /** Open Graph `og:locale` value, for example `en_US`. */
  readonly ogLocale: string;
  /** Text direction of the language. */
  readonly dir: 'ltr' | 'rtl';
}

interface LanguageEntry {
  readonly code: string;
  readonly name: string;
  readonly short: string;
  readonly ogLocale: string;
  readonly dir: string;
}

type Dictionary = Record<string, string>;

/**
 * All supported languages in default order. Metadata lives in
 * `src/lib/i18n/data/languages.json`, so language switchers and SEO tags stay in sync.
 */
export const LANGUAGES: readonly Language[] = (registry.languages as readonly LanguageEntry[]).map(entry => ({
  code: entry.code as Locale,
  name: entry.name,
  short: entry.short,
  ogLocale: entry.ogLocale,
  dir: entry.dir === 'rtl' ? 'rtl' : 'ltr',
}));

/** Locale used when detection finds nothing better. */
export const DEFAULT_LOCALE = registry.default as Locale;

/** All supported locales in default order. */
export const LOCALES: readonly Locale[] = LANGUAGES.map(language => language.code);

/** Translation bundles keyed by locale; every locale must ship a bundle. */
export const BUNDLES: Record<Locale, Dictionary> = {
  en: enBundle as Dictionary,
  ar: arBundle as Dictionary,
};

/**
 * Resolves the metadata of a supported locale.
 *
 * @param code - Supported locale code.
 * @returns Metadata of the locale.
 */
export function languageOf(code: Locale): Language {
  return LANGUAGES.find(language => language.code === code) ?? LANGUAGES[0];
}

/**
 * Normalizes a BCP-47 tag such as `ar-EG` or `en_US` to a supported locale.
 *
 * @param tag - Raw language tag from a browser, URL, or storage.
 * @returns The matching supported locale, or null when none matches.
 */
export function localeFromTag(tag: string | null | undefined): Locale | null {
  if (!tag) return null;
  const base = tag.toLowerCase().replace('_', '-').split('-')[0];
  return (LOCALES as readonly string[]).includes(base) ? (base as Locale) : null;
}

/**
 * Base language subtags written right-to-left that are not part of the shipped
 * registry. Keeping the list here (rather than in a consuming component) makes
 * direction handling data-driven and testable when new languages are added.
 */
const RTL_FALLBACK_LANGUAGES = new Set(['ar', 'he', 'fa', 'ur', 'yi', 'dv', 'ps', 'sd', 'ug']);

/**
 * Determines whether text in a locale flows right-to-left. Supported languages
 * use their registry direction; other tags fall back to a known RTL language set.
 *
 * @param locale - BCP-47 locale or language tag, for example `ar`, `ar-EG` or `en`.
 * @returns True when the locale's base language is written right-to-left.
 */
export function isRtlLocale(locale: string | null | undefined): boolean {
  if (!locale) return false;
  const base = locale.toLowerCase().split('-')[0];
  const known = LANGUAGES.find(language => language.code === base);
  if (known) return known.dir === 'rtl';
  return RTL_FALLBACK_LANGUAGES.has(base);
}
