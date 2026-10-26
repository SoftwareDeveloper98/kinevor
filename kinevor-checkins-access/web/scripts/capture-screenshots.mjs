import { chromium } from 'playwright';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const output = resolve(here, '..', '..', 'docs', 'screenshots');
const webUrl = process.env.KINEVOR_WEB_URL ?? 'http://localhost:4205';
const apiUrl = process.env.KINEVOR_API_URL ?? 'http://localhost:5086';
const channel = process.env.PLAYWRIGHT_CHANNEL ?? 'chrome';

const browser = await chromium.launch(channel ? { channel } : {});
const page = await browser.newPage({ viewport: { width: 1440, height: 1040 } });

async function capture(path, selector, fullPage = false) {
  await page.waitForSelector(selector, { timeout: 30000 });
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(500);
  await page.screenshot({ path: resolve(output, path), fullPage });
  console.log(`saved ${path}`);
}

try {
  await mkdir(output, { recursive: true });

  await page.goto(`${webUrl}/checkins`, { waitUntil: 'domcontentloaded' });
  await capture('web-front-desk.png', 'kinevor-front-desk .member-row');

  await page.goto(`${webUrl}/checkins/occupancy`, { waitUntil: 'domcontentloaded' });
  await capture('web-occupancy.png', 'kinevor-occupancy-dashboard .visit-table tbody tr');

  await page.goto(`${webUrl}/checkins/history`, { waitUntil: 'domcontentloaded' });
  await capture('web-history.png', 'kinevor-visit-history .visit-table tbody tr');

  await page.goto(`${apiUrl}/swagger/index.html`, { waitUntil: 'domcontentloaded' });
  await capture('api-swagger.png', '.swagger-ui .info', true);
} finally {
  await browser.close();
}
