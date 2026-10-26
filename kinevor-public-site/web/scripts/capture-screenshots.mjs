import { chromium } from 'playwright';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const output = resolve(here, '..', '..', 'docs', 'screenshots');
const webUrl = (process.env.KINEVOR_WEB_URL ?? 'http://localhost:4208').replace(/\/$/, '');
const apiUrl = (process.env.KINEVOR_API_URL ?? 'http://localhost:5090').replace(/\/$/, '');
const channel = process.env.PLAYWRIGHT_CHANNEL ?? 'chrome';

const browser = await chromium.launch(channel ? { channel } : {});
const page = await browser.newPage({ viewport: { width: 1440, height: 1040 } });
page.setDefaultTimeout(30000);

const pageErrors = [];
page.on('pageerror', (error) => pageErrors.push(error.message));

async function prepare() {
  await page.evaluate(async () => {
    await document.fonts.ready;
  });
  await page.waitForTimeout(600);
}

async function assertNoOverflow(name) {
  const overflow = await page.evaluate(() => {
    const root = document.documentElement;
    return Math.max(root.scrollWidth, document.body.scrollWidth) - root.clientWidth;
  });

  if (overflow > 1) {
    throw new Error(`${name} has ${overflow}px of horizontal overflow.`);
  }
}

async function capture(name, path, selector) {
  await page.goto(`${webUrl}${path}`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector(selector);
  await prepare();
  await assertNoOverflow(name);
  await page.screenshot({ path: resolve(output, `${name}.png`), fullPage: true });
  console.log(`saved ${name}.png`);
}

try {
  await mkdir(output, { recursive: true });

  await capture('web-landing', '/', '[data-testid="landing-page"]');
  await capture('web-plans', '/plans', '[data-testid="plans-page"]');
  await capture('web-schedule', '/schedule', '[data-testid="schedule-page"]');

  await page.goto(`${webUrl}/trial`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('[data-testid="trial-form"]');
  await page.getByTestId('trial-first-name').fill('Jordan');
  await page.getByTestId('trial-last-name').fill('Ellis');
  await page.getByTestId('trial-email').fill('jordan.ellis@example.com');
  await page.getByTestId('trial-phone').fill('+1 212 555 0188');
  await page.getByTestId('trial-date').fill(new Date(Date.now() + 86400000).toISOString().slice(0, 10));
  await page.getByTestId('trial-notes').fill('New to structured strength training. Looking for a coach-led start.');
  await page.locator('label.consent').click();
  if (!(await page.getByTestId('trial-consent').isChecked())) {
    throw new Error('The trial consent checkbox did not toggle.');
  }
  await prepare();
  await assertNoOverflow('web-trial');
  await page.screenshot({ path: resolve(output, 'web-trial.png'), fullPage: true });
  console.log('saved web-trial.png');

  await page.goto(`${apiUrl}/swagger/index.html`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('.swagger-ui .info');
  await prepare();
  await page.screenshot({ path: resolve(output, 'api-swagger.png'), fullPage: true });
  console.log('saved api-swagger.png');

  if (pageErrors.length > 0) {
    throw new Error(`Browser errors detected:\n${pageErrors.join('\n')}`);
  }
} finally {
  await browser.close();
}
