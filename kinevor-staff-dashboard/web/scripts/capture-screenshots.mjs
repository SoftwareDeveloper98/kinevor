import { chromium } from 'playwright';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const output = resolve(here, '..', '..', 'docs', 'screenshots');
const webUrl = process.env.KINEVOR_WEB_URL ?? 'http://localhost:4207';
const apiUrl = process.env.KINEVOR_API_URL ?? 'http://localhost:5089';
const channel = process.env.PLAYWRIGHT_CHANNEL ?? 'chrome';

const browser = await chromium.launch(channel ? { channel } : {});
const page = await browser.newPage({ viewport: { width: 1440, height: 1040 } });

try {
  await mkdir(output, { recursive: true });

  await page.goto(`${webUrl}/dashboard`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('kinevor-staff-dashboard-overview .kpi-grid', { timeout: 30000 });
  await page.waitForTimeout(900);
  await page.screenshot({
    path: resolve(output, 'web-dashboard.png'),
    fullPage: true,
  });
  console.log('saved web-dashboard.png');

  await page.goto(`${webUrl}/alerts`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('kinevor-staff-dashboard-alerts .alerts-board', { timeout: 30000 });
  await page.waitForTimeout(900);
  await page.screenshot({
    path: resolve(output, 'web-alerts.png'),
    fullPage: true,
  });
  console.log('saved web-alerts.png');

  await page.goto(`${apiUrl}/swagger/index.html`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('.swagger-ui .info', { timeout: 30000 });
  await page.waitForTimeout(900);
  await page.screenshot({
    path: resolve(output, 'api-swagger.png'),
    fullPage: true,
  });
  console.log('saved api-swagger.png');
} finally {
  await browser.close();
}
