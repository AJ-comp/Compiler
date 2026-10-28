const {chromium}=require('C:/Users/AhjLv/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('node:fs');
const path=require('node:path');
const {pathToFileURL}=require('node:url');
(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Users/AhjLv/AppData/Local/ms-playwright/chromium-1243/chrome-win64/chrome.exe',headless:true});
 const checks=[];
 const overviewOnly=process.argv.includes('--overview-only');
 const scenes=Array.from({length:6},(_,i)=>`requeue-${String(i+1).padStart(2,'0')}`).flatMap(n=>[n,n+'-mobile']);
 for(const name of ['full-graph',...(overviewOnly?[]:scenes)])for(const colorScheme of ['light','dark']){
   const mobile=name.includes('mobile');
   const page=await browser.newPage({viewport:{width:name==='full-graph'?1540:mobile?400:1000,height:1600},colorScheme,deviceScaleFactor:2});
   await page.goto(pathToFileURL(path.resolve(`docs/images/lalr-lookahead-${name}.svg`)).href);
   await page.evaluate(()=>document.fonts.ready);
   const result=await page.evaluate(()=>{
     const svg=document.querySelector('svg');const b=svg.getBoundingClientRect();
     const labels=[...svg.querySelectorAll('text')].map(n=>({text:n.textContent,r:n.getBoundingClientRect().toJSON()}));
     return {outside:labels.filter(({r})=>r.left<b.left-1||r.right>b.right+1||r.top<b.top-1||r.bottom>b.bottom+1),
       overlaps:labels.flatMap((a,i)=>labels.slice(i+1).filter(c=>a.r.left<c.r.right-1&&c.r.left<a.r.right-1&&a.r.top<c.r.bottom-1&&c.r.top<a.r.bottom-1).map(c=>[a.text,c.text]))};
   });
   checks.push({name,colorScheme,...result});
   if(!mobile&&colorScheme==='light')await page.locator('svg').screenshot({path:path.resolve(`docs/images/lalr-lookahead-${name}.png`)});
   if(mobile&&colorScheme==='light')await page.locator('svg').screenshot({path:path.join(__dirname,`lookahead-${name}.png`)});
   await page.close();
 }
 const source=fs.readFileSync('src/FrontEnd/Parse.FrontEnd.Parsers/Collections/CanonicalRelation.cs','utf8').split(/\r?\n/);
 const start=source.findIndex(l=>l.includes('if (target.LookAhead.Count > before && !target.IsQueued)'));
 const expected=source.slice(start,start+5).join('').replace(/\s/g,'');
 for(const width of [1800,1100,736,390,320])for(const colorScheme of ['light','dark']){
   const page=await browser.newPage({viewport:{width,height:1000},colorScheme});
   const errors=[];page.on('pageerror',e=>errors.push(e.message));
   await page.goto(pathToFileURL(path.resolve('docs/ko/lalr-lookahead-propagation.html')).href);
   const result=await page.evaluate(()=>({
     overflow:document.documentElement.scrollWidth>document.documentElement.clientWidth+1,
     controls:document.querySelectorAll('button,[role="tab"],details,[hidden]').length,
     imageCount:document.images.length,
     images:[...document.images].map(i=>({loaded:i.complete&&!!i.naturalWidth,src:i.currentSrc})),
     graphEntries:document.querySelectorAll('img[src*="full-graph"]').length,
     graphFits:document.querySelector('.graph-scroll').getBoundingClientRect().right<=innerWidth,
     code:document.querySelector('[data-requeue-source]').textContent
   }));
   result.codeMatches=result.code.replace(/\s/g,'')===expected;delete result.code;
   checks.push({width,colorScheme,errors,...result});
   if(width===1800&&colorScheme==='light'){
     await page.locator('.overview').screenshot({path:path.join(__dirname,'full-graph-document.png')});
   }
   if(width===390&&colorScheme==='light')await page.locator('.graph-scroll').screenshot({path:path.join(__dirname,'full-graph-mobile-scroll.png')});
   await page.close();
 }
 await browser.close();
 fs.writeFileSync(path.join(__dirname,overviewOnly?'full-graph-review.json':'illustrated-lookahead-review.json'),JSON.stringify(checks,null,2));
 const problems=checks.filter(c=>c.outside?.length||c.overlaps?.length||c.overflow||c.controls||c.errors?.length||c.codeMatches===false||c.imageCount===0||c.graphEntries===0||c.graphFits===false||c.images?.some(i=>!i.loaded));
 console.log(JSON.stringify({checked:checks.length,problems},null,2));
 if(problems.length)process.exitCode=1;
})();
