import re,json,hashlib,base64,subprocess
from pathlib import Path
root=Path.cwd();out=root/'artifacts/ecmascript-2026';testroot=out/'test262/test/built-ins/RegExp'
if not testroot.exists():
 p=list((out/'test262').glob('*/test/built-ins/RegExp'));testroot=p[0] if p else testroot
lit=r'''(?:"(?:[^"\\]|\\[\s\S])*"|'(?:[^'\\]|\\[\s\S])*')'''
pattern=re.compile(r'(?<![\w$.])(?:new\s+)?RegExp\s*\(\s*(?P<body>'+lit+r')\s*(?:(?P<end>\))|,\s*(?P<flags>'+lit+r')\s*\))')
def decode(s):
 q=s[0];s=s[1:-1];i=0;chars=[]
 while i<len(s):
  c=s[i];i+=1
  if c!='\\':chars.append(c);continue
  if i==len(s):raise ValueError('trailing slash')
  c=s[i];i+=1
  simple={'b':'\b','f':'\f','n':'\n','r':'\r','t':'\t','v':'\v'}
  if c in simple:chars.append(simple[c]);continue
  if c in '\r\n\u2028\u2029':
   if c=='\r' and i<len(s) and s[i]=='\n':i+=1
   continue
  if c=='x':chars.append(chr(int(s[i:i+2],16)));i+=2;continue
  if c=='u':
   if s[i]=='{':j=s.index('}',i);chars.append(chr(int(s[i+1:j],16)));i=j+1
   else:chars.append(chr(int(s[i:i+4],16)));i+=4
   continue
  if '0'<=c<='7':
   digs=c;n=2 if c<='3' else 1
   while n and i<len(s) and '0'<=s[i]<='7':digs+=s[i];i+=1;n-=1
   chars.append(chr(int(digs,8)));continue
  chars.append(c)
 return ''.join(chars)
cases=[];skipped=[]
for file in sorted(testroot.rglob('*.js')):
 text=file.read_text(encoding='utf-8');path=file.relative_to(out/'test262/test').as_posix()
 for match in pattern.finditer(text):
  try:
   body=decode(match['body']);flags=decode(match['flags']) if match['flags'] else ''
   if set(flags)-set('dgimsuvy') or len(flags)!=len(set(flags)) or 'u' in flags and 'v' in flags:continue
   # This first pass keeps all raw source tokens and reports unsupported JSON
   # surrogate transport rather than silently normalizing the pattern.
   if any(0xD800<=ord(c)<=0xDFFF for c in body):
    skipped.append({'path':path,'offset':match.start(),'reason':'requires-raw-UTF16-transport'});continue
   cases.append({'id':f'constructor-{len(cases):05d}','body':body,'flags':flags,'path':path,'offset':match.start(),'constructorSource':match.group(),'sourceSha256':hashlib.sha256(file.read_bytes()).hexdigest()})
  except (ValueError,IndexError) as e:skipped.append({'path':path,'offset':match.start(),'reason':str(e)})
payload=''.join(json.dumps(t,ensure_ascii=True)+'\n' for t in cases)
(out/'regexp-constructor-cases.jsonl').write_text(payload,encoding='utf-8')
node=subprocess.run(['node',str(out/'regexp-audit-harness/oracle.cjs')],input=payload,text=True,encoding='utf-8',capture_output=True,check=True)
cs=subprocess.run(['C:/Users/AhjLv/.dotnet/dotnet.exe',str(out/'regexp-audit-harness/bin/Release/net8.0/Audit.dll'),str(root/'tools/Janglim.Test262/bin/Release/net8.0/Parse.FrontEnd.Grammars.Ebnf.dll')],input=payload,text=True,encoding='utf-8',capture_output=True,check=True)
a=[json.loads(x) for x in node.stdout.splitlines()];b=[json.loads(x) for x in cs.stdout.splitlines()];assert len(a)==len(b)==len(cases)
diffs=[]
for t,n,c in zip(cases,a,b):
 if n['accepted']!=c['accepted']:diffs.append({**t,'nodeAccepted':n['accepted'],'janglimAccepted':c['accepted'],'nodeError':n['error'],'janglimError':c['error']})
report={'kind':'test262-regexp-static-constructor-arguments','cases':len(cases),'files':len(set(t['path'] for t in cases)),'mismatches':len(diffs),'mismatchCases':diffs,'skippedCalls':skipped,'scope':'Static string literal pattern/flags arguments only; no computed strings, flags errors, RegExp matches, assertions, eval, or JS runtime programs executed.'}
(out/'regexp-constructor-audit.json').write_text(json.dumps(report,ensure_ascii=True,indent=2),encoding='utf-8');print('CASES',len(cases),'FILES',report['files'],'MISMATCHES',len(diffs),'SKIPPED',len(skipped));print(json.dumps(diffs[:30],indent=2))
