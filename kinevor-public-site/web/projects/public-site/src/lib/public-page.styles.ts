export const PUBLIC_PAGE_STYLES = `
  :host {
    display: block;
    color: var(--kx-ink, #17140f);
    background: var(--kx-paper, #f7f2e9);
    font-family: var(--kx-font, 'Sora', 'Inter', system-ui, sans-serif);
    line-height: 1.55;
  }

  * { box-sizing: border-box; }

  .kx-container {
    width: min(100% - 48px, 1200px);
    margin-inline: auto;
  }

  .kx-eyebrow {
    display: inline-flex;
    align-items: center;
    gap: 10px;
    margin: 0;
    color: var(--kx-ember, #f04f24);
    font-size: 11px;
    font-weight: 700;
    letter-spacing: .18em;
    text-transform: uppercase;
  }

  .kx-eyebrow::before {
    content: '';
    width: 24px;
    height: 1px;
    background: currentColor;
  }

  .kx-display {
    margin: 0;
    font-size: clamp(46px, 5.2vw, 78px);
    font-weight: 800;
    letter-spacing: -.055em;
    line-height: .98;
  }

  .kx-section-title {
    margin: 0;
    font-size: clamp(32px, 3.5vw, 48px);
    font-weight: 750;
    letter-spacing: -.045em;
    line-height: 1.03;
  }

  .kx-lede {
    margin: 0;
    max-width: 58ch;
    color: var(--kx-muted, #6f675c);
    font-size: 16px;
    line-height: 1.7;
  }

  .kx-btn {
    display: inline-flex;
    min-height: 48px;
    align-items: center;
    justify-content: center;
    gap: 10px;
    padding: 0 22px;
    border: 1px solid transparent;
    border-radius: 999px;
    font: inherit;
    font-size: 13px;
    font-weight: 700;
    letter-spacing: .02em;
    text-decoration: none;
    cursor: pointer;
    transition: transform .2s ease, background .2s ease, border-color .2s ease, color .2s ease;
  }

  .kx-btn:hover { transform: translateY(-2px); }
  .kx-btn .material-icons { font-size: 17px; }

  .kx-btn-ember {
    background: var(--kx-ember, #f04f24);
    color: #fff8ef;
  }

  .kx-btn-ember:hover { background: var(--kx-ember-deep, #cf3e16); }

  .kx-btn-dark {
    background: var(--kx-ink, #17140f);
    color: var(--kx-paper, #f7f2e9);
  }

  .kx-btn-ghost {
    border-color: currentColor;
    background: transparent;
    color: inherit;
  }

  .kx-btn-ghost:hover {
    border-color: var(--kx-ember, #f04f24);
    color: var(--kx-ember, #f04f24);
  }

  .kx-chip {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 5px 11px;
    border: 1px solid var(--kx-line, rgba(23, 20, 15, .14));
    border-radius: 999px;
    background: var(--kx-surface, #fffaf2);
    color: var(--kx-muted, #6f675c);
    font-size: 10px;
    font-weight: 700;
    letter-spacing: .08em;
    text-transform: uppercase;
  }

  .kx-dot {
    width: 7px;
    height: 7px;
    border-radius: 999px;
    background: var(--kx-ember, #f04f24);
    box-shadow: 0 0 0 4px rgba(240, 79, 36, .13);
  }

  .kx-icon {
    display: inline-flex;
    width: 46px;
    height: 46px;
    align-items: center;
    justify-content: center;
    border-radius: 14px;
    background: rgba(240, 79, 36, .1);
    color: var(--kx-ember, #f04f24);
  }

  .kx-icon .material-icons { font-size: 23px; }

  .kx-section {
    padding-block: clamp(72px, 9vw, 118px);
  }

  .kx-section-tight { padding-block: clamp(48px, 6vw, 72px); }

  .kx-skeleton {
    overflow: hidden;
    border-radius: 20px;
    background: linear-gradient(100deg, rgba(23,20,15,.06) 25%, rgba(23,20,15,.1) 50%, rgba(23,20,15,.06) 75%);
    background-size: 200% 100%;
    animation: kx-shimmer 1.5s ease-in-out infinite;
  }

  .kx-loading {
    width: min(100% - 48px, 1200px);
    margin: 48px auto;
    display: grid;
    gap: 20px;
  }

  .kx-rule {
    height: 1px;
    border: 0;
    background: var(--kx-line, rgba(23, 20, 15, .14));
  }

  .kx-link-arrow {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    color: var(--kx-ink, #17140f);
    font-size: 12px;
    font-weight: 750;
    letter-spacing: .02em;
    text-decoration: none;
  }

  .kx-link-arrow:hover { color: var(--kx-ember, #f04f24); }
  .kx-link-arrow .material-icons { font-size: 17px; transition: transform .2s ease; }
  .kx-link-arrow:hover .material-icons { transform: translateX(4px); }

  .material-icons { font-size: 20px; line-height: 1; }

  @keyframes kx-shimmer {
    from { background-position: 200% 0; }
    to { background-position: -200% 0; }
  }

  @media (max-width: 720px) {
    .kx-container, .kx-loading { width: min(100% - 32px, 1200px); }
    .kx-section { padding-block: 64px; }
    .kx-btn { min-height: 46px; padding-inline: 18px; }
  }

  @media (prefers-reduced-motion: reduce) {
    .kx-btn, .kx-link-arrow .material-icons { transition: none; }
    .kx-skeleton { animation: none; }
  }
`;
