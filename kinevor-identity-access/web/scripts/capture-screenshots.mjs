import { chromium } from 'playwright';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const output = resolve(here, '..', '..', 'docs', 'screenshots');
const webUrl = process.env.KINEVOR_WEB_URL ?? 'http://localhost:4201';
const apiUrl = process.env.KINEVOR_API_URL ?? 'http://localhost:5082';
const channel = process.env.PLAYWRIGHT_CHANNEL ?? 'chrome';

const browser = await chromium.launch(channel ? { channel } : {});
const page = await browser.newPage({ viewport: { width: 1440, height: 1040 } });

try {
  await mkdir(output, { recursive: true });

  await page.goto(`${webUrl}/login`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('kinevor-login .auth-card', { timeout: 30000 });
  await page.waitForTimeout(800);
  await page.screenshot({
    path: resolve(output, 'web-login.png'),
    fullPage: true,
  });
  console.log('saved web-login.png');

  await page.locator('button.submit').click();
  await page.waitForURL('**/session', { timeout: 20000 });
  await page.waitForSelector('kinevor-session .session-grid', { timeout: 20000 });
  await page.waitForTimeout(800);
  await page.screenshot({
    path: resolve(output, 'web-session.png'),
    fullPage: true,
  });
  console.log('saved web-session.png');

  await page.goto(`${apiUrl}/swagger/index.html`, {
    waitUntil: 'domcontentloaded',
  });
  await page.waitForSelector('.swagger-ui .info', { timeout: 30000 });
  await page.waitForTimeout(800);
  await page.screenshot({
    path: resolve(output, 'api-swagger.png'),
    fullPage: true,
  });
  console.log('saved api-swagger.png');
} finally {
  await browser.close();
}
