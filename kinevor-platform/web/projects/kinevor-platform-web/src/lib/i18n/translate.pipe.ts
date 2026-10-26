import { Pipe, PipeTransform, inject } from '@angular/core';
import { I18nParams, I18nService } from './i18n.service';

/**
 * Impure pipe that resolves a translation key to the active-locale string via
 * {@link I18nService}. Returns an empty string for nullish keys.
 */
@Pipe({ name: 'translate', standalone: true, pure: false })
export class TranslatePipe implements PipeTransform {
  private readonly i18n = inject(I18nService);

  /**
   * Translates a key, optionally interpolating named parameters.
   *
   * @param key - Translation key, or nullish to yield an empty string.
   * @param params - Named interpolation values.
   * @returns The localized string.
   */
  transform(key: string | null | undefined, params?: I18nParams): string {
    return this.i18n.t(key ?? '', params);
  }
}
