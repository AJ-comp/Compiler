const fs = require('node:fs');
const vm = require('node:vm');
const cases = JSON.parse(fs.readFileSync('artifacts/regexp/integration.json','utf8'));
const results = cases.map(item => {
  try {
    if(item.goal === 'Module') new vm.SourceTextModule(item.input);
    else new vm.Script('"use strict";\n' + item.input);
    return {...item, actual: true};
  } catch(error) {
    if (!(error instanceof SyntaxError)) throw error;
    return {...item, actual: false, error: error.message};
  }
});
fs.writeFileSync('artifacts/regexp/integration-node.json', JSON.stringify(results,null,2));
console.log(JSON.stringify(results.filter(item=>item.expected!==item.actual), null, 2));
console.log('COUNT', results.length, 'MISMATCH', results.filter(item=>item.expected!==item.actual).length);
