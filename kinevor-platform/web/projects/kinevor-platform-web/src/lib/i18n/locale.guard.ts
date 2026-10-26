import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router, UrlSegment, UrlSegmentGroup, UrlTree } from '@angular/router';
import { DEFAULT_LOCALE, I18nService, LOCALE_STORAGE_KEY, Locale, isLocale, localeFromTag } from './i18n.service';

/**
 * Reads the visitor's preferred locale from storage or the browser language.
 *
 * @returns The preferred locale, defaulting to English.
 */
export function detectPreferredLocale(): Locale {
  try {
    const saved = localStorage.getItem(LOCALE_STORAGE_KEY);
    if (isLocale(saved)) return saved;
  } catch {}
  const nav = typeof navigator === 'undefined' ? '' : navigator.language;
  return localeFromTag(nav) ?? DEFAULT_LOCALE;
}

/**
 * Prefixes a URL tree with a locale segment on the primary outlet.
 *
 * @param tree - URL tree to modify.
 * @param locale - Locale segment to ensure at the front.
 * @returns The same tree, mutated in place.
 */
export function withLocalePrefix(tree: UrlTree, locale: Locale): UrlTree {
  const localeSegment = new UrlSegment(locale, {});
  const primary = tree.root.children['primary'];
  if (!primary) {
    tree.root.children['primary'] = new UrlSegmentGroup([localeSegment], {});
  } else if (!(primary.segments.length > 0 && isLocale(primary.segments[0].path))) {
    primary.segments.unshift(localeSegment);
  }
  return tree;
}

/** Matches only routes whose first segment is a supported locale. */
export const localeCanMatchGuard: CanMatchFn = (_route, segments) => isLocale(segments[0]?.path);

/** Activates the `:locale` route param and syncs it into the i18n service. */
export const localeGuard: CanActivateFn = route => {
  const locale = route.paramMap.get('locale');
  if (!isLocale(locale)) return inject(Router).parseUrl(`/${DEFAULT_LOCALE}`);
  inject(I18nService).setLocale(locale);
  return true;
};

/** Redirects locale-less URLs to the same path under the preferred locale. */
export const legacyLocaleRedirectGuard: CanActivateFn = (_route, state) =>
  withLocalePrefix(inject(Router).parseUrl(state.url), detectPreferredLocale());
