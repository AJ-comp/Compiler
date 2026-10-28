# Reuse only the independently written string decoder and common paths.
import runpy,json,subprocess,re
from pathlib import Path
ns=runpy.run_path('artifacts/ecmascript-2026/regexp-audit-harness/constructors.py');decode=ns['decode'];lit=ns['lit'];out=ns['out'];root=ns['root'];testroot=ns['testroot'];dedup={};skipped=0
for file in sorted(testroot.rglob('*.js')):
 text=file.read_text(encoding='utf-8');end=text.find('---*/');text=text[end+5:] if end>=0 else text
 for match in re.finditer(lit,text):
  try:
   body=decode(match.group())
   # Merge adjacent surrogate pairs into one Python scalar. Re-encoding the
   # resulting string yields exactly the original UTF-16 code-unit sequence.
   body=body.encode('utf-16-le','surrogatepass').decode('utf-16-le')
  except (UnicodeDecodeError,ValueError,IndexError):skipped+=1;continue
  if len(body)>5000:continue
  for flags in ('','u','v'):
   if (body,flags) in dedup:continue
   dedup[body,flags]={'id':f'string-{len(dedup):06d}','body':body,'flags':flags,'path':file.relative_to(out/'test262/test').as_posix(),'offsetAfterMetadata':match.start()}
cases=list(dedup.values());payload=''.join(json.dumps(t,ensure_ascii=True)+'\n' for t in cases)
(out/'regexp-string-probes.jsonl').write_text(payload,encoding='utf-8')
node=subprocess.run(['node',str(out/'regexp-audit-harness/oracle.cjs')],input=payload,text=True,encoding='utf-8',capture_output=True,check=True)
cs=subprocess.run(['C:/Users/AhjLv/.dotnet/dotnet.exe',str(out/'regexp-audit-harness/bin/Release/net8.0/Audit.dll'),str(root/'tools/Janglim.Test262/bin/Release/net8.0/Parse.FrontEnd.Grammars.Ebnf.dll')],input=payload,text=True,encoding='utf-8',capture_output=True,check=True)
a=[json.loads(x) for x in filter(None,node.stdout.split("\n"))];b=[json.loads(x) for x in filter(None,cs.stdout.split("\n"))];assert len(a)==len(b)==len(cases)
diffs=[]
for t,n,c in zip(cases,a,b):
 if n['accepted']!=c['accepted']:diffs.append({**t,'nodeAccepted':n['accepted'],'janglimAccepted':c['accepted'],'nodeError':n['error'],'janglimError':c['error']})
report={'kind':'test262-regexp-arbitrary-source-string-probes','cases':len(cases),'mismatches':len(diffs),'mismatchCases':diffs,'skippedLiterals':skipped,'scope':'Arbitrary string-literal contents from RegExp test sources compiled as Patterns in normal/u/v modes. This supplements direct constructor extraction, but these are probes, not independently expected Test262 test results.'}
(out/'regexp-string-audit.json').write_text(json.dumps(report,ensure_ascii=True,indent=2),encoding='utf-8');print('STRING CASES',len(cases),'MISMATCHES',len(diffs),'SKIPPED',skipped);print(json.dumps(diffs[:30],indent=2))
