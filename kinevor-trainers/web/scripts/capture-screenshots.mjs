import { chromium } from 'playwright';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const output = resolve(here, '..', '..', 'docs', 'screenshots');
const webUrl = process.env.KINEVOR_WEB_URL ?? 'http://localhost:4200';
const apiUrl = process.env.KINEVOR_API_URL ?? 'http://localhost:5081';
const channel = process.env.PLAYWRIGHT_CHANNEL ?? 'chrome';

const browser = await chromium.launch(channel ? { channel } : {});
const page = await browser.newPage({ viewport: { width: 1440, height: 1100 } });

async function shot(name, path, selector) {
  await page.goto(`${webUrl}${path}`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector(selector, { timeout: 20000 });
  await page.waitForTimeout(600);
  await page.screenshot({ path: resolve(output, name), fullPage: true });
  console.log(`saved ${name}`);
}

try {
  await mkdir(output, { recursive: true });

  const response = await fetch(`${apiUrl}/api/v1/trainers?page=1&pageSize=1`, {
    headers: { 'X-Demo-Role': 'Staff' },
  });
  if (!response.ok) throw new Error(`Trainers API returned ${response.status}`);
  const payload = await response.json();
  const trainerId = payload?.data?.items?.[0]?.id;
  if (!trainerId) throw new Error('Trainers API returned no trainers');

  await shot('web-staff-directory.png', '/trainers', '.trainer-card');
  await shot('web-public-directory.png', '/public', '.trainer-card');
  await shot('web-trainer-profile.png', `/trainers/${trainerId}`, '.trainer-hero');

  await page.goto(`${apiUrl}/swagger/index.html`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('.swagger-ui .info', { timeout: 20000 });
  await page.waitForTimeout(600);
  await page.screenshot({ path: resolve(output, 'api-swagger.png'), fullPage: true });
  console.log('saved api-swagger.png');
} finally {
  await browser.close();
}
