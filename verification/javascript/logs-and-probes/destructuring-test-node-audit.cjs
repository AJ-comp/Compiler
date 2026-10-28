const fs = require('fs');
const vm = require('vm');
const file = fs.readFileSync('Tests/Parse.FrontEnd.Parsers.Tests/JavaScript/JavaScriptDestructuringTests.cs', 'utf8');
let count=0,mismatches=[];
for (const block of file.split('[Theory]').slice(1)) {
 const method = /public void\s+(\w+)/.exec(block)?.[1];
 if (!method) continue;
 const expected = method.includes('Invalid_leaf') ? false : /=> _fixture\.Success/.test(block);
 for (const match of block.matchAll(/\[InlineData\("((?:\\.|[^"\\])*)"/g)) {
  const input = JSON.parse('"'+match[1]+'"');
  let actual=true, message='';
  try { new vm.Script('"use strict"; ('+input+');'); } catch(e) {actual=false;message=e.message;}
  count++;
  if(actual!==expected)mismatches.push({input,expected,actual,message,method});
 }
}
console.log(JSON.stringify({count,mismatches},null,2));
