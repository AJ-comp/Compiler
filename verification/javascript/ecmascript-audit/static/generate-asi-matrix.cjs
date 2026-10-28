const fs = require('fs');
const vm = require('vm');
const crypto = require('crypto');
const output = 'artifacts/ecmascript-audit/static/asi-matrix.jsonl';
const script = body => body;
const ordinary = body => `function outer() {\n${body}\n}`;
const generator = body => `function* outer() {\n${body}\n}`;
const loop = body => `outer: while (false) {\n${body}\n}`;
const prefixes = [
 ['identifier','value',script],['let-identifier','let',script],['yield-identifier','yield',script],
 ['var-let','var let',script],['var-yield','var yield',script],
 ['typeof-let','typeof let',script],['typeof-yield','typeof yield',script],
 ['binary-let','value + let',script],['binary-yield','value + yield',script],
 ['postfix-value','value++',script],['postfix-let','let++',script],['postfix-yield','yield++',script],
 ['postfix-decrement','value--',script],['call','value()',script],['member','object.value',script],
 ['parenthesized','(value)',script],['assign-let','let = value',script],
 ['return-bare','return',ordinary],['return-value','return value',ordinary],
 ['break','break',loop],['continue','continue',loop],['generator-yield','yield',generator],
 ['generator-yield-value','yield value',generator],['if-let','if (false) let',script],
 ['while-let','while (false) let',script],['label-let','label: let',script],
 ['for-let','for (; false;) let',script],['if-return','if (false) return',ordinary],
 ['if-break','if (false) break',loop],['if-continue','if (false) continue',loop],
 ['async-identifier','async',script],['var-initializer','var value = 1',script],
 ['let-declaration','let value = 1',script],['async-call','async(value)',script],
 ['function-expression','value = function() {}',script],['class-expression','value = class {}',script]
];
const declarations = [
 ['class','class Next {}'],['function','function next() {}'],
 ['async-function','async function next() {}'],['generator','function* next() {}']
];
const trivias = [['lf','\n'],['crlf','\r\n'],['multiline-comment',' /*\n*/ ']];
const tails = [['regexp','/ab/.test(text);'],['division','value / divisor;']];
const cases = [];
function add(path, source, mode='NonStrict') {
 if (mode==='Strict') source='"use strict";\n'+source;
 let expectAccept, nodeDiagnostic;
 try { new vm.Script(source); expectAccept=true; }
 catch (e) { expectAccept=false; nodeDiagnostic=e.name+': '+e.message; }
 const sourceSha256=crypto.createHash('sha256').update(source,'utf8').digest('hex');
 cases.push({id:path, path:'independent/asi-matrix/'+path,variant:mode==='Strict'?'strict':'non-strict',inputKind:'Script',mode,webCompatibility:true,expectAccept,source,sourceSha256,nodeDiagnostic});
}
for (const [p,prefix,wrap] of prefixes)
 for (const [d,decl] of declarations)
  for (const [t,trivia] of trivias)
   for (const [s,tail] of tails)
    add(`${p}/${d}/${t}/${s}`,wrap(prefix+trivia+decl+'\n'+tail));
for (const [p,prefix,wrap] of prefixes)
 for (const [s,tail] of tails)
  add(`${p}/async-generator/lf/${s}`,wrap(prefix+'\nasync function* next() {}\n'+tail));
for (const [p,prefix,wrap] of [
 ['let-body-block','if (false) let\n{}',script],
 ['let-body-labelled-block','label: let\n{}',script],
 ['let-body-loop-block','while (false) let\n{}',script],
 ['let-binding-object','let\n{value} = source;',script],
 ['let-binding-array','let\n[value] = source;',script],
 ['let-invalid-empty-pattern','let\n{}',script]
]) for(const [s,tail]of tails) add(`${p}/special/lf/${s}`,wrap(prefix+'\n'+tail));
for(const [p,prefix]of [['let','let'],['yield','yield'],['var-let','var let'],['var-yield','var yield'],['typeof-let','typeof let'],['typeof-yield','typeof yield']])
 for(const [d,decl]of declarations.slice(0,2))for(const [s,tail]of tails)
  add(`strict-${p}/${d}/lf/${s}`,prefix+'\n'+decl+'\n'+tail,'Strict');
for(const [p,prefix]of [['invalid-return','return'],['invalid-break','break'],['invalid-continue','continue'],['invalid-let-binding','let name']])
 for(const [s,tail]of tails) add(`${p}/class/no-newline/${s}`,prefix+' class Next {}\n'+tail);
fs.writeFileSync(output,cases.map(c=>JSON.stringify(c)).join('\n')+'\n');
const valid=cases.filter(c=>c.expectAccept).length;
fs.writeFileSync(output+'.node-summary.json',JSON.stringify({schemaVersion:1,syntaxOnly:true,nodeVersion:process.version,input:output,total:cases.length,expectAccept:valid,expectReject:cases.length-valid,execution:'vm.Script constructor only; no source execution'},null,2)+'\n');
console.log(JSON.stringify({total:cases.length,expectAccept:valid,expectReject:cases.length-valid}));
