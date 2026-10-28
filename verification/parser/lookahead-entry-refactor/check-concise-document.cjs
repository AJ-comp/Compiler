const {chromium}=require('C:/Users/AhjLv/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('node:fs');
const path=require('node:path');
const {pathToFileURL}=require('node:url');
(async()=>{
  const browser=await chromium.launch({executablePath:'C:/Users/AhjLv/AppData/Local/ms-playwright/chromium-1243/chrome-win64/chrome.exe',headless:true});
  const checks=[];
  for(const width of [1100,736,390,320])for(const colorScheme of ['light','dark']){
    const page=await browser.newPage({viewport:{width,height:1000},colorScheme});
    const errors=[];page.on('pageerror',e=>errors.push(e.message));
    await page.goto(pathToFileURL(path.resolve('docs/ko/lalr-lookahead-propagation.html')).href);
    const check=await page.evaluate(()=>({
      width:innerWidth,
      overflow:document.documentElement.scrollWidth>document.documentElement.clientWidth+1,
      controls:document.querySelectorAll('button,[role="tab"],details,[hidden]').length,
      sections:document.querySelectorAll('main section').length,
      text:document.querySelector('main').innerText,
      code:document.querySelector('[data-core-source]').textContent
    }));
    if(width===736&&colorScheme==='light')await page.screenshot({path:path.join(__dirname,'concise-document.png'),fullPage:true});
    if(width===320&&colorScheme==='light')await page.screenshot({path:path.join(__dirname,'concise-document-mobile.png'),fullPage:true});
    checks.push({...check,colorScheme,errors});await page.close();
  }
  await browser.close();
  const source=fs.readFileSync('src/FrontEnd/Parse.FrontEnd.Parsers/Collections/CanonicalRelation.cs','utf8').split(/\r?\n/);
  const start=source.findIndex(s=>s.includes('int before = target.LookAhead.Count;'));
  const end=source.findIndex((s,i)=>i>=start&&s.includes('pending.Enqueue(target);'))+2;
  const expected=source.slice(start,end).filter(s=>!s.trim().startsWith('//')).join('').replace(/\s/g,'');
  for(const check of checks){check.codeMatches=check.code.replace(/\s/g,'')===expected;check.textLength=check.text.length;delete check.text;delete check.code;}
  fs.writeFileSync(path.join(__dirname,'concise-document-review.json'),JSON.stringify(checks,null,2));
  const problems=checks.filter(c=>c.overflow||c.controls||c.sections!==2||c.errors.length||!c.codeMatches);
  console.log(JSON.stringify({checked:checks.length,textLength:checks[0].textLength,problems},null,2));
  if(problems.length)process.exitCode=1;
})();
