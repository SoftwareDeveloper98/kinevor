import { Pipe, PipeTransform, inject } from '@angular/core';
import { I18nService } from './i18n.service';

/**
 * Impure pipe that prefixes an application path (or router command array) with
 * the active locale segment, producing locale-aware navigation targets.
 */
@Pipe({ name: 'localePath', standalone: true, pure: false })
export class LocalePathPipe implements PipeTransform {
  private readonly i18n = inject(I18nService);

  /**
   * Prefixes a path string or router command array with the current locale.
   *
   * @param value - Absolute app path or router commands.
   * @returns The locale-prefixed path or commands.
   */
  transform(value: string | readonly unknown[]): string | unknown[] {
    if (typeof value === 'string') return this.i18n.localePath(value);
    if (Array.isArray(value)) {
      return [this.i18n.locale(), ...value.map(part =>
        typeof part === 'string' ? part.replace(/^\//, '') : part)];
    }
    return value as unknown[];
  }
}
