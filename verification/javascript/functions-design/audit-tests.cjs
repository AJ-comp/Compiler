const fs = require('node:fs');
const vm = require('node:vm');
const cases = JSON.parse(fs.readFileSync('artifacts/functions-design/test-oracle.json', 'utf8'));
const mismatches = [];
for (const item of cases) {
  let valid = true;
  let error = '';
  try { new vm.Script('"use strict";\n' + item.input); }
  catch (e) { valid = false; error = e.message; }
  if (valid !== item.valid) mismatches.push({...item, actual: valid, error});
}
fs.writeFileSync('artifacts/functions-design/test-oracle-results.json', JSON.stringify({
  runtime: process.version, count: cases.length, mismatches
}, null, 2));
console.log(JSON.stringify({runtime:process.version, count:cases.length, mismatches}, null, 2));
process.exitCode = mismatches.length ? 1 : 0;
