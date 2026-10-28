const fs=require('fs'), vm=require('vm');
const examples=JSON.parse(fs.readFileSync('artifacts/classes/node-cases.json','utf8'));
const results=examples.map(x=>{let ok=true,error;try{new vm.Script('"use strict";\n'+(x.method.startsWith('Expression_entry')?'('+x.input+')':x.input));}catch(e){ok=false;error=e.message}return {...x,ok,error}});
fs.writeFileSync('artifacts/classes/node-results.json',JSON.stringify(results,null,2));
const bad=results.filter(x=>x.ok!==x.expected); console.log(process.version, results.length,'cases,',bad.length,'mismatches');console.log(JSON.stringify(bad,null,2));
