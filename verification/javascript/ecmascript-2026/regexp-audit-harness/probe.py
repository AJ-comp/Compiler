import itertools,json,random,subprocess,hashlib
from pathlib import Path
root=Path.cwd();out=root/'artifacts/ecmascript-2026';cases={}
def add(body,group,flags=('', 'u','v')):
 for f in flags:cases.setdefault((body,f),{'id':f'{group}-{len(cases):05d}','group':group,'body':body,'flags':f})
atoms=['a','.','\\d','\\w','[a]','[^a]','[]','[^]','(a)','(?:a)','(?=a)','(?!a)','(?<=a)','(?<!a)','(?<x>a)','(?i:a)','(?-i:a)','^','$','\\b','\\B','\\1','\\0','\\k<x>','{','}',']','\\p{L}','\\P{ASCII}']
quantifiers=['','*','+','?','*?','+?','??','**','++','?+','{0}','{1}','{01}','{01,0}','{0,01}','{1,}','{1,2}','{2,1}','{,1}','{1,2}?','{1,2}*','{999999999999999999999999,888888888888888888888888}','{999999999999999999999999,}']
for a,q in itertools.product(atoms,quantifiers):add(a+q,'quantifier')
for ch in map(chr,range(128)):add('\\'+ch,'escape')
escapes=['\\x','\\x0','\\x00','\\xgg','\\u','\\u0','\\u0000','\\uD800','\\uDC00','\\uD800\\uDC00','\\u{D800}\\u{DC00}','\\u{0}','\\u{000001}','\\u{110000}','\\u{10FFFF}','\\c','\\c0','\\c_','\\cA','\\c[','\\00','\\01','\\08','\\18','\\118','\\999','\\k','\\k<','\\k<>','\\k<x>','\\p{}','\\p{L}','\\p{gc=L}','\\p{Script=Latin}','\\p{sc=Latn}','\\p{Basic_Emoji}','\\P{Basic_Emoji}','\\p{ASCII=Yes}']
for e in escapes:add(e,'escape-boundary')
class_atoms=['a','z','-','\\-','\\d','\\w','\\b','\\B','\\p{L}','\\P{L}','\\p{Basic_Emoji}','\\q{a}','\\q{ab}','\\q{a|b}','\\q{}','[a]','[^a]','[]','\\c','\\c0','\\x00','\\u{1F600}','\\uD83D\\uDE00','\\u{D83D}\\u{DE00}','&&','--','!','!!','&','\\&']
for a,b in itertools.product(class_atoms,class_atoms):
 for sep in ['','-','&&','--']:
  add('['+a+sep+b+']','class-combination')
  add('[^'+a+sep+b+']','class-combination-negated')
for a in class_atoms:add('['+a+']','class-single');add('[^'+a+']','class-single-negated')
for pre in ['','?i','?m','?s','?-i','?i-','?i-i','?ii','?i-m','?-','?g','?<','?<x>','?<x\\u0061>','?<x\\u{1F600}>']:
 for middle in ['a','a|b','(?<x>a)','(?<x>a)|(?<x>b)']:
  add('('+pre+(':' if pre and not pre.startswith('?<') else '')+middle+')','group-prefix')
names=['(?<x>a)|(?<x>b)','(?:(?<x>a)|(?<x>b))','(?<x>(?<x>a))','(?:(?<x>a)|b)(?:(?<x>c)|d)','(?:(?<x>a)|(?<x>b))*','(?<x>a){0}(?<x>b)','(?<x>a)|((?<x>b)|(?<x>c))','(?<x>a)|(?:(?<x>b)(?<x>c))','(?<x>a)|(?:(?<x>b)|(?<x>c))','(?<x>a)(?!(?<x>b))','(?<x>a)|(?<\\u0078>b)','(?<x>a)\\k<x>','\\k<x>(?<x>a)','\\k<x>','[\\k<x>](?<x>a)','\\k<x>[()]','(?<x>a)\\2','(a)\\01','(a)\\1','\\2(a)(b)','[\\2](a)(b)']
for n in names:add(n,'capture-context')
for term in ['a','-','&','|','(',')','[',']','/','\\d','\\q{a}','\\q{ab}','\\q{}','\\p{Basic_Emoji}']:
 for container in ['[{}]','[[{}]]','[^{}]','[a&&{}]','[{}--a]','[a--{}]','[{}&&a]','[\\q{{ab}}&&{}]']:
  add(container.replace('{}',term),'sets-specific',('v',))
random.seed(20261021)
tokens=['a','b','(',')','[',']','{','}','*','+','?','|','-','&','^','$',':','\\','\\u','\\k','\\1','\\0','\\c','\\p','<','>','i','s','0','1',',']
for i in range(18000):add(''.join(random.choices(tokens,k=random.randrange(1,9))),'random')
array=list(cases.values());payload=''.join(json.dumps(x,ensure_ascii=True)+'\n' for x in array)
(out/'regexp-audit-cases.jsonl').write_text(payload,encoding='utf-8')
node=subprocess.run(['C:/Program Files/nodejs/node.exe',str(out/'regexp-audit-harness/oracle.cjs')],input=payload,text=True,encoding='utf-8',capture_output=True,check=True)
cs=subprocess.run(['C:/Users/AhjLv/.dotnet/dotnet.exe',str(out/'regexp-audit-harness/bin/Release/net8.0/Audit.dll'),str(root/'tools/Janglim.Test262/bin/Release/net8.0/Parse.FrontEnd.Grammars.Ebnf.dll')],input=payload,text=True,encoding='utf-8',capture_output=True,check=True)
a=[json.loads(x) for x in node.stdout.splitlines()];b=[json.loads(x) for x in cs.stdout.splitlines()];assert len(a)==len(b)==len(array),(len(a),len(b),len(array))
diffs=[]
for test,n,c in zip(array,a,b):
 if n['accepted']!=c['accepted']:diffs.append({'id':test['id'],'group':test['group'],'body':test['body'],'flags':test['flags'],'nodeAccepted':n['accepted'],'janglimAccepted':c['accepted'],'nodeError':n['error'],'janglimError':c['error']})
report={'kind':'independent-regexp-pattern-syntax-differential-probes','nodeVersion':subprocess.check_output(['node','--version'],text=True).strip(),'assemblySha256':hashlib.sha256((root/'tools/Janglim.Test262/bin/Release/net8.0/Parse.FrontEnd.Grammars.Ebnf.dll').read_bytes()).hexdigest(),'cases':len(array),'mismatches':len(diffs),'mismatchCases':diffs,'scope':'RegExp Pattern grammar compiled only; no regex is matched, no JS program or Test262 runtime body executed. Node is a cross-check, frozen 2026 specification decides final classification.'}
(out/'regexp-audit.json').write_text(json.dumps(report,ensure_ascii=True,indent=2),encoding='utf-8');print('CASES',len(array),'MISMATCHES',len(diffs));print(json.dumps(diffs[:30],indent=2))
