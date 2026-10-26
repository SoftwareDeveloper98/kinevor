import { Injectable, inject } from '@angular/core';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { I18nService } from '../i18n/i18n.service';

/**
 * Thin wrapper around {@link MatSnackBar} that centralizes snack message
 * presentation with the localized close action. On the server platform every
 * call is a no-op so SSR never touches the DOM.
 */
@Injectable({ providedIn: 'root' })
export class NoticeService {
  private readonly snack = inject(MatSnackBar);
  private readonly i18n = inject(I18nService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  /**
   * Shows a localized success snack message.
   *
   * @param messageKey - Translation key of the message body.
   * @param durationMs - Visible duration in milliseconds.
   */
  success(messageKey: string, durationMs = 3000): void {
    if (!this.isBrowser) return;
    this.snack.open(this.i18n.t(messageKey), this.i18n.t('common.close'), { duration: durationMs });
  }

  /**
   * Shows an error snack message, preferring a server problem detail when present.
   *
   * @param error - Error object, typically from a failed HTTP call.
   * @param fallbackKey - Translation key used when the error carries no message.
   * @param durationMs - Visible duration in milliseconds.
   */
  error(error: unknown, fallbackKey: string, durationMs = 4000): void {
    if (!this.isBrowser) return;
    this.snack.open(this.i18n.errorText(error, fallbackKey), this.i18n.t('common.close'), { duration: durationMs });
  }
}
