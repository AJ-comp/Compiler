const { chromium } = require('C:/Users/AhjLv/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs = require('node:fs');
const path = require('node:path');
(async () => {
  const source = fs.readFileSync('src/FrontEnd/Parse.FrontEnd.Parsers/Collections/CanonicalRelation.cs', 'utf8').split(/\r?\n/);
  const browser = await chromium.launch({ executablePath: 'C:/Users/AhjLv/AppData/Local/ms-playwright/chromium-1243/chrome-win64/chrome.exe', headless: true });
  const checks = [];
  for (const width of [736, 480, 320]) {
    for (const colorScheme of ['light', 'dark']) {
      const page = await browser.newPage({ viewport: { width, height: 1050 }, colorScheme, reducedMotion: 'reduce' });
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      await page.goto('http://127.0.0.1:8798/');
      const frame = page.frames().find(frame => frame !== page.mainFrame());
      await frame.waitForSelector('svg text');
      const count = Number(await frame.locator('#lookahead-worklist-code').getAttribute('data-steps'));
      for (let step = 0; step < count; step++) {
        await page.waitForTimeout(18);
        const check = await frame.evaluate(() => {
          const root = document.getElementById('lookahead-worklist-code');
          const svg = root.querySelector('svg');
          const bounds = svg.getBoundingClientRect();
          const labels = [...svg.querySelectorAll('text')].map(node => ({ text: node.textContent, r: node.getBoundingClientRect().toJSON() }));
          const code = [...root.querySelectorAll('.lw-line')].map(node => ({ line: Number(node.querySelector('.lw-line-number').textContent), code: node.querySelector('.lw-code-text').textContent }));
          return {
            step: Number(root.dataset.step),
            title: root.querySelector('[data-scene]').textContent,
            height: document.documentElement.scrollHeight,
            overflow: document.documentElement.scrollWidth > document.documentElement.clientWidth + 1,
            outside: labels.filter(({r}) => r.left < bounds.left - 1 || r.right > bounds.right + 1),
            overlaps: labels.flatMap((a, i) => labels.slice(i + 1).filter(b => a.r.left < b.r.right && b.r.left < a.r.right && a.r.top < b.r.bottom - 1 && b.r.top < a.r.bottom - 1).map(b => [a.text, b.text])),
            labels: labels.map(node => node.text),
            code
          };
        });
        check.codeDifferences = check.code.filter(line => source[line.line - 1].trim() !== line.code.trim());
        checks.push({width, colorScheme, errors: [...errors], ...check});
        if (width === 736 && [0, 21, 23, 26, 35, 37].includes(step)) {
          await page.screenshot({path: path.join(__dirname, `worklist-${colorScheme}-step${step + 1}.png`), fullPage: true});
        }
        if (width === 320 && colorScheme === 'light' && [22, 36].includes(step)) {
          await page.screenshot({path: path.join(__dirname, `worklist-mobile-step${step + 1}.png`), fullPage: true});
        }
        if (step < count - 1) await frame.locator('[data-next]').click();
      }
      await frame.locator('[data-prev]').click();
      if (await frame.locator('[data-count]').textContent() !== `${count - 1} / ${count}`) throw new Error('Previous navigation failed.');
      await page.close();
    }
  }
  fs.writeFileSync(path.join(__dirname, 'worklist-visual-review.json'), JSON.stringify(checks, null, 2));
  const problems = checks.filter(check => check.errors.length || check.overflow || check.outside.length || check.overlaps.length || check.codeDifferences.length);
  console.log(JSON.stringify({checked: checks.length, problems}, null, 2));
  await browser.close();
  if (problems.length) process.exitCode = 1;
})();
