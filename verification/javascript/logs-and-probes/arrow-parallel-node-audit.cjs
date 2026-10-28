const vm = require('node:vm');
for(const input of [
'const f = value => value / divisor; const g = () => /a/.test(value);',
'const f = () => {}\n/a/.test(value);',
'const f = ({value = `default ${name}`}) => `outer ${() => ({value})}`;',
'const f = () => { return\n(value) => value; };\nconst g = (x = /a/) => x / divisor;',
'/a/g.test(value); const f = () => { return /b/; };'
]) new vm.Script('"use strict"; '+input);
console.log('Parallel and speculative source samples: 5 strict compile-only checks passed.');
