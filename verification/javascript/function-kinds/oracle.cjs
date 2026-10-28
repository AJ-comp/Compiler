// Compile JavaScript syntax only. Never evaluate a Script, link a Module,
// construct a function from the parsed result, or call a user body.
const fs = require('node:fs');
const vm = require('node:vm');
const cases = JSON.parse(fs.readFileSync('artifacts/function-kinds/cases.json', 'utf8'));
let mismatches = 0;
const results = cases.map((item, index) => {
  const text = item.goal === 'expression'
    ? '"use strict"; (' + item.text + '\n);'
    : item.goal === 'script' ? '"use strict";\n' + item.text : item.text;
  let actual = true, message = '';
  try {
    if (item.goal === 'module') new vm.SourceTextModule(text);
    else new vm.Script(text);
  } catch (error) { actual = false; message = error.message; }
  if (actual !== item.valid) {
    mismatches++;
    console.log(JSON.stringify({index, ...item, actual, message}));
  }
  return {...item, actual, message};
});
fs.writeFileSync('artifacts/function-kinds/oracle-results.json', JSON.stringify({versions:process.versions, compileOnly:true, cases:results}, null, 2));
console.log('Compared ' + cases.length + '; mismatches ' + mismatches);
process.exitCode = mismatches ? 1 : 0;
