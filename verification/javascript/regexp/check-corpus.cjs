const fs = require('node:fs');
const vm = require('node:vm');
const corpus = JSON.parse(fs.readFileSync('artifacts/regexp/corpus.json','utf8'));
const results = corpus.map(item => {
  try {
    new vm.Script('"use strict"; (' + item.literal + ');');
    return {...item, actual: true};
  } catch(error) {
    if (!(error instanceof SyntaxError)) throw error;
    return {...item, actual: false, error: error.message};
  }
});
fs.writeFileSync('artifacts/regexp/corpus-node.json', JSON.stringify(results,null,2));
console.log(JSON.stringify(results.filter(item=>item.expected!==item.actual), null, 2));
console.log('COUNT', results.length, 'MISMATCH', results.filter(item=>item.expected!==item.actual).length);
