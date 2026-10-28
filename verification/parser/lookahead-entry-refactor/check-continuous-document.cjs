const { chromium } = require('C:/Users/AhjLv/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs = require('node:fs');
const path = require('node:path');
const { pathToFileURL } = require('node:url');
(async () => {
  const source = fs.readFileSync('src/FrontEnd/Parse.FrontEnd.Parsers/Collections/CanonicalRelation.cs','utf8').split(/\r?\n/);
  const document = path.resolve('docs/ko/lalr-lookahead-propagation.html');
  const browser = await chromium.launch({executablePath:'C:/Users/AhjLv/AppData/Local/ms-playwright/chromium-1243/chrome-win64/chrome.exe',headless:true});
  const checks=[];
  for (const width of [1100,736,390,320]) {
    for (const colorScheme of ['light','dark']) {
      const page=await browser.newPage({viewport:{width,height:1000},colorScheme});
      const errors=[];
      page.on('pageerror',e=>errors.push(e.message));
      await page.goto(pathToFileURL(document).href);
      await page.waitForTimeout(60);
      const result=await page.evaluate(()=>{
        const svg=document.getElementById('same-set');
        const b=svg.getBoundingClientRect();
        const labels=[...svg.querySelectorAll('text')].map(n=>({text:n.textContent,box:n.getBoundingClientRect().toJSON()}));
        return {
          width:innerWidth,
          height:document.documentElement.scrollHeight,
          overflow:document.documentElement.scrollWidth>document.documentElement.clientWidth+1,
          controls:document.querySelectorAll('button,[role="tab"],details,[hidden]').length,
          sections:document.querySelectorAll('main section').length,
          emptyCode:[...document.querySelectorAll('[data-source]')].some(n=>!n.textContent.trim()),
          source:[...document.querySelectorAll('[data-source]')].map(n=>({range:n.dataset.source,text:n.textContent})),
          outside:labels.filter(({box:r})=>r.left<b.left-1||r.right>b.right+1),
          overlaps:labels.flatMap((a,i)=>labels.slice(i+1).filter(c=>a.box.left<c.box.right&&c.box.left<a.box.right&&a.box.top<c.box.bottom-1&&c.box.top<a.box.bottom-1).map(c=>[a.text,c.text])),
          missingImages:[...document.images].filter(i=>!i.complete||!i.naturalWidth).length
        };
      });
      const dedent=text=>{
        const lines=text.split('\n');
        const indent=Math.min(...lines.filter(x=>x.trim()).map(x=>x.match(/^ */)[0].length));
        return lines.map(x=>x.trim()?x.slice(indent):'').join('\n');
      };
      result.codeDifferences=result.source.filter(s=>{
        const [first,last]=s.range.split('-').map(Number);
        return dedent(source.slice(first-1,last).join('\n'))!==s.text;
      });
      delete result.source;
      checks.push({...result,colorScheme,errors});
      if (colorScheme==='light'&&[1100,390].includes(width)) {
        await page.locator('#storage').screenshot({path:path.join(__dirname,`continuous-storage-${width}.png`)});
        await page.locator('#propagation').screenshot({path:path.join(__dirname,`continuous-propagation-${width}.png`)});
        await page.locator('#late-arrival').screenshot({path:path.join(__dirname,`continuous-late-${width}.png`)});
      }
      await page.close();
    }
  }
  await browser.close();
  fs.writeFileSync(path.join(__dirname,'continuous-document-review.json'),JSON.stringify(checks,null,2));
  const problems=checks.filter(c=>c.overflow||c.controls||c.sections!==9||c.emptyCode||c.outside.length||c.overlaps.length||c.codeDifferences.length||c.errors.length||c.missingImages);
  console.log(JSON.stringify({checked:checks.length,problems},null,2));
  if(problems.length)process.exitCode=1;
})();
