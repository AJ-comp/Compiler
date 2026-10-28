const { chromium } = require('C:/Users/AhjLv/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs = require('node:fs');
const path = require('node:path');

(async () => {
  const source = fs.readFileSync('src/FrontEnd/Parse.FrontEnd.Parsers/Collections/CanonicalRelation.cs', 'utf8').split(/\r?\n/);
  const browser = await chromium.launch({ executablePath: 'C:/Users/AhjLv/AppData/Local/ms-playwright/chromium-1243/chrome-win64/chrome.exe', headless: true });
  const checks = [];
  for (const width of [736, 320]) {
    for (const colorScheme of ['light', 'dark']) {
      const page = await browser.newPage({ viewport: { width, height: 1100 }, colorScheme, reducedMotion: 'reduce' });
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      await page.goto('http://127.0.0.1:8796/');
      const frame = page.frames().find(f => f !== page.mainFrame());
      await frame.waitForSelector('svg text');
      const count = await frame.locator('#lookahead-code-memory').getAttribute('data-steps').then(Number);
      for (let step = 0; step < count; step++) {
        await page.waitForTimeout(30);
        const layout = await frame.evaluate(() => {
          const svg = document.querySelector('svg');
          const bounds = svg.getBoundingClientRect();
          const labels = [...svg.querySelectorAll('text')].map(n => ({ text: n.textContent, r: n.getBoundingClientRect().toJSON() }));
          const code = [...document.querySelectorAll('.lcm-line')].map(n => ({ line: Number(n.querySelector('.lcm-number').textContent), code: n.querySelector('.lcm-code-text').textContent }));
          return {
            overflow: document.documentElement.scrollWidth > document.documentElement.clientWidth + 1,
            height: document.documentElement.scrollHeight,
            viewportHeight: document.documentElement.clientHeight,
            outside: labels.filter(({ r }) => r.left < bounds.left - 1 || r.right > bounds.right + 1),
            overlaps: labels.flatMap((a, i) => labels.slice(i + 1).filter(b => a.r.left < b.r.right && b.r.left < a.r.right && a.r.top < b.r.bottom - 1 && b.r.top < a.r.bottom - 1).map(b => [a.text, b.text])),
            title: document.querySelector('[data-title]').textContent,
            code,
            labels: labels.map(x => x.text)
          };
        });
        layout.codeDifferences = layout.code.filter(x => x.line && source[x.line - 1].trim() !== x.code.trim());
        checks.push({ width, colorScheme, step: step + 1, errors: [...errors], ...layout });
        if ([2, 7, 17, 24, count - 1].includes(step) && width === 736) {
          await page.screenshot({ path: path.join(__dirname, `code-memory-${colorScheme}-step${step + 1}.png`), fullPage: true });
        }
        if (step === 17 && width === 320 && colorScheme === 'light') {
          await page.screenshot({ path: path.join(__dirname, 'code-memory-mobile-read.png'), fullPage: true });
        }
        if (step < count - 1) await frame.locator('[data-next]').click();
      }
      await frame.locator('[data-prev]').click();
      const backwards = await frame.locator('[data-count]').textContent();
      if (backwards !== `${count - 1} / ${count}`) throw new Error('Previous-step navigation failed.');
      await page.close();
    }
  }
  fs.writeFileSync(path.join(__dirname, 'code-memory-review.json'), JSON.stringify(checks, null, 2));
  const problems = checks.filter(c => c.overflow || c.outside.length || c.overlaps.length || c.errors.length || c.codeDifferences.length);
  console.log(JSON.stringify({ checked: checks.length, problems }, null, 2));
  await browser.close();
  if (problems.length) process.exitCode = 1;
})();
