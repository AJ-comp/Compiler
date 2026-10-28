const fs = require('node:fs');
const vm = require('node:vm');
const file = fs.readFileSync('Tests/Parse.FrontEnd.Parsers.Tests/JavaScript/JavaScriptArrowFunctionTests.cs', 'utf8');
let count=0; const mismatches=[];
for (const section of file.split(/public sealed class /).slice(1)) {
 const expression=section.startsWith('JavaScriptArrowExpressionTests');
 for (const block of section.split('[Theory]').slice(1)) {
  const method=/public void\s+(\w+)/.exec(block)?.[1];
  if(!method)continue;
  const expected=/=> _fixture\.Success/.test(block.split('public static')[0]) || method.startsWith('A_new_statement_after');
  for(const match of block.matchAll(/\[InlineData\("((?:\\.|[^"\\])*)"/g)) {
   const input=JSON.parse('"'+match[1]+'"');
   let actual=true,message='';
   try { new vm.Script('"use strict"; '+(expression?'('+input+');':input)); }
   catch(e){actual=false;message=e.message;}
   count++;
   if(actual!==expected)mismatches.push({input,expected,actual,message,method});
  }
 }
}
for(const name of ['eval','arguments','let','yield','static','implements','interface','package','private','protected','public','enum']) {
 for(const input of [`const f = ${name} => 1;`,`const f = (${name}) => 1;`,`const f = ([${name}]) => 1;`,`const f = ({name: ${name}}) => 1;`,`const f = (...${name}) => 1;`]){
  let actual=true;try{new vm.Script('"use strict"; '+input);}catch(e){actual=false;}
  count++;if(actual)mismatches.push({input,expected:false,actual});
 }
}
console.log(JSON.stringify({node:process.version,count,mismatches},null,2));

