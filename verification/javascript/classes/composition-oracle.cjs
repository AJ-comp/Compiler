const fs=require('fs'),vm=require('vm');
const inputs=JSON.parse(fs.readFileSync('artifacts/classes/composition-inputs.json','utf8'));
const results=inputs.map(input=>{let success=true,error;try{new vm.Script('"use strict";\n'+input);}catch(e){success=false;error=e.message}return {input,success,error}});
fs.writeFileSync('artifacts/classes/composition-node.json',JSON.stringify(results,null,2));
console.log(JSON.stringify({version:process.version,total:results.length,accepted:results.filter(x=>x.success).length,rejected:results.filter(x=>!x.success).length}));
