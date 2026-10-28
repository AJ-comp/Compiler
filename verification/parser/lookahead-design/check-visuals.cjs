const { chromium } = require('C:/Users/AhjLv/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs = require('node:fs');
const path = require('node:path');

(async () => {
  const browser = await chromium.launch({ executablePath: 'C:/Users/AhjLv/AppData/Local/ms-playwright/chromium-1243/chrome-win64/chrome.exe', headless: true });
  const findings = [];
  for (const [name, port, count] of [['old-cache', 8793, 6], ['new-worklist', 8794, 7]]) {
    for (const width of [736, 320]) {
      for (const colorScheme of ['light', 'dark']) {
        const page = await browser.newPage({ viewport: { width, height: 1200 }, colorScheme, reducedMotion: 'reduce' });
        const errors = [];
        page.on('pageerror', e => errors.push(e.message));
        await page.goto(`http://127.0.0.1:${port}/`);
        const frame = page.frames().find(f => f !== page.mainFrame());
        await frame.waitForSelector('svg text');
        for (let step = 0; step < count; step++) {
          await page.waitForTimeout(50);
          const layout = await frame.evaluate(() => {
            const svg = document.querySelector('svg');
            const bounds = svg.getBoundingClientRect();
            const texts = [...svg.querySelectorAll('text')].map(t => ({ text: t.textContent, rect: t.getBoundingClientRect().toJSON() }));
            return {
              overflow: document.documentElement.scrollWidth > document.documentElement.clientWidth + 1,
              outside: texts.filter(({ rect }) => rect.left < bounds.left - 1 || rect.right > bounds.right + 1),
              overlaps: texts.flatMap((a, i) => texts.slice(i + 1).filter(b => a.rect.left < b.rect.right && b.rect.left < a.rect.right && a.rect.top < b.rect.bottom - 1 && b.rect.top < a.rect.bottom - 1).map(b => [a.text, b.text])),
              stage: document.querySelector('[data-stage]').textContent,
              lastAction: [...svg.querySelectorAll('text')].map(t => t.textContent).find(t => t.startsWith('ACTION'))
            };
          });
          findings.push({ name, width, colorScheme, step: step + 1, errors: [...errors], ...layout });
          if ((step === count - 1 || step === 1) && width === 736) {
            await page.screenshot({ path: path.join(__dirname, `${name}-${colorScheme}-step${step + 1}.png`), fullPage: true });
          }
          if (step === count - 1 && width === 320 && colorScheme === 'light') {
            await page.screenshot({ path: path.join(__dirname, `${name}-mobile.png`), fullPage: true });
          }
          if (step < count - 1) await frame.locator('[data-next]').click();
        }
        await page.close();
      }
    }
  }
  fs.writeFileSync(path.join(__dirname, 'visual-review.json'), JSON.stringify(findings, null, 2));
  const problems = findings.filter(x => x.overflow || x.outside.length || x.overlaps.length || x.errors.length);
  console.log(JSON.stringify({ checked: findings.length, problems }, null, 2));
  await browser.close();
  if (problems.length) process.exitCode = 1;
})();
