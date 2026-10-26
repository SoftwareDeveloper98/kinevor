import { Injectable, signal, computed, effect, Injector, inject, DOCUMENT } from '@angular/core';

/** User-selectable theme modes. */
export type ThemeMode = 'light' | 'dark' | 'system';
/** Theme actually applied after resolving `system`. */
export type ResolvedTheme = 'light' | 'dark';

/** Local-storage key under which the theme mode is persisted. */
export const THEME_STORAGE_KEY = 'kinevor_theme';

function readStoredMode(): ThemeMode {
  try {
    const saved = localStorage.getItem(THEME_STORAGE_KEY);
    if (saved === 'light' || saved === 'dark' || saved === 'system') return saved;
  } catch {}
  return 'system';
}

function prefersDark(): boolean {
  return typeof matchMedia !== 'undefined' && matchMedia('(prefers-color-scheme: dark)').matches;
}

/**
 * Theme state: light, dark or system, applied to the document root via the
 * `data-theme` attribute and persisted in local storage.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly injector = inject(Injector);
  private readonly _mode = signal<ThemeMode>(readStoredMode());
  /** Selected theme mode. */
  readonly mode = this._mode.asReadonly();

  private readonly _systemDark = signal<boolean>(prefersDark());
  /** Theme actually applied, after resolving `system` against the OS preference. */
  readonly resolved = computed<ResolvedTheme>(() => {
    const mode = this._mode();
    if (mode === 'system') return this._systemDark() ? 'dark' : 'light';
    return mode;
  });

  constructor() {
    const document = inject(DOCUMENT);
    effect(() => {
      const theme = this.resolved();
      const mode = this._mode();
      try { localStorage.setItem(THEME_STORAGE_KEY, mode); } catch {}
      document.documentElement?.setAttribute('data-theme', theme);
    }, { injector: this.injector });

    if (typeof matchMedia !== 'undefined') {
      const mq = matchMedia('(prefers-color-scheme: dark)');
      const onChange = () => this._systemDark.set(mq.matches);
      mq.addEventListener('change', onChange);
    }
  }

  /**
   * Selects the theme mode.
   *
   * @param mode - Mode to apply.
   */
  setMode(mode: ThemeMode): void {
    this._mode.set(mode);
  }

  /**
   * Cycles light → dark → system → light.
   */
  cycle(): void {
    const next: Record<ThemeMode, ThemeMode> = { light: 'dark', dark: 'system', system: 'light' };
    this.setMode(next[this._mode()]);
  }
}
