// Independent syntax-composition audit. vm only compiles these strings;
// it never evaluates code, links modules, invokes getters, or runs a pattern.
const fs = require('node:fs');
const vm = require('node:vm');
const cases = [];
const seen = new Set();
function add(input, goal, category) {
  const key = goal + '\0' + input;
  if (seen.has(key)) return;
  seen.add(key);
  cases.push({input, goal, category});
}
const kinds = [
  ['normal', (p,b) => `function named(${p}) { ${b} }`],
  ['async', (p,b) => `async function named(${p}) { ${b} }`],
  ['generator', (p,b) => `function* named(${p}) { ${b} }`],
  ['async-generator', (p,b) => `async function* named(${p}) { ${b} }`],
  ['arrow', (p,b) => `(${p}) => { ${b} }`],
  ['async-arrow', (p,b) => `async (${p}) => { ${b} }`]
];
const outers = [
  ['Script', e => [`const value = ${e};`, 'Script']],
  ['Module', e => [`export default ${e};`, 'Module']],
  ['async', e => [`async function outer() { const value = ${e}; }`, 'Script']],
  ['generator', e => [`function* outer() { const value = ${e}; }`, 'Script']],
  ['async-generator', e => [`async function* outer() { const value = ${e}; }`, 'Script']],
  ['Expression', e => [e, 'Expression']]
];
const bodies = [
  '', 'return value;', 'return await task;', 'yield value;', 'yield* values;',
  'yield await task;', 'yield* await task;', 'await (yield value);',
  'return new.target;', 'return super.value;', 'super();',
  'for await (const item of source) { consume(item); }',
  'const f = () => await;', 'const f = (await) => await;',
  'const f = async () => await task;',
  'yield\n/regexp/;', 'await\n/regexp/;',
  'return `${await task}:${yield value}`;'
];
const parameters = [
  'await', 'yield', '\\u0061wait', '\\u0079ield', '{await}', '{yield: value}',
  'value = await task', 'value = yield task', 'value = () => await',
  'value = (await) => 1', 'value = async () => await task',
  'value = class await {}', 'value = class { field = await; }',
  'value = ({ [await task]() {} })', 'value = class extends (yield Base) {}',
  'first, first'
];
for (const [kind, make] of kinds) {
  for (const [outer, wrap] of outers) {
    for (const body of bodies) {
      const [input, goal] = wrap(make('', body));
      add(input, goal, `body:${kind}:${outer}`);
    }
    for (const parameter of parameters) {
      const [input, goal] = wrap(make(parameter, 'return value;'));
      add(input, goal, `parameter:${kind}:${outer}`);
    }
  }
}

// Names in a declaration use the surrounding Yield/Await grammar parameters;
// expression names have the function-kind-specific grammar parameters.
const names = ['await', 'yield', 'async', '\\u0061wait', '\\u0079ield', 'eval', 'arguments'];
const declarationKinds = ['function', 'async function', 'function*', 'async function*'];
const declarationOuters = [
  ['Script', text => [text, 'Script']],
  ['Module', text => [text, 'Module']],
  ['async', text => [`async function outer() { ${text} }`, 'Script']],
  ['generator', text => [`function* outer() { ${text} }`, 'Script']],
  ['async-generator', text => [`async function* outer() { ${text} }`, 'Script']]
];
for (const kind of declarationKinds) {
  for (const name of names) {
    for (const [outer, wrap] of declarationOuters) {
      const [input, goal] = wrap(`${kind} ${name}() {}`);
      add(input, goal, `declaration-name:${kind}:${outer}`);
    }
  }
}

// A computed method key is evaluated syntactically in the surrounding scope,
// not the new method body. Test both object and class forms.
const methodPrefixes = ['', 'async ', '*', 'async *'];
const keys = ['await key', 'yield key', 'await', 'yield', 'new.target', 'super.key'];
for (const prefix of methodPrefixes) {
  for (const key of keys) {
    for (const [outer, wrap] of outers.slice(0, 5)) {
      for (const form of ['object', 'class']) {
        const expression = form === 'object'
          ? `({ ${prefix}[${key}]() {} })` : `class { ${prefix}[${key}]() {} }`;
        const [input, goal] = wrap(expression);
        add(input, goal, `computed-key:${form}:${prefix}:${outer}`);
      }
    }
  }
}

// ASI and lexical-goal composition. These are deliberately real newlines,
// line terminators inside comments, and Unicode line separators.
for (const trivia of [' ', '\n', '\r\n', '/*same*/', '/*\n*/', '\u2028', '\u2029']) {
  for (const tail of [
    'function f() { await task; }', 'function* f() { yield await task; }',
    '(value) => await value;', 'value => await value;',
    '(value) / divisor;', 'value: while (ready) break value;'
  ]) {
    add('async' + trivia + tail, 'Script', 'async-prefix-trivia');
  }
}
for (const term of ['value', '* values', '/a/', '+ value', '- value', '++value', '--value', '(value)', '[value]', '`text`', 'await task']) {
  for (const trivia of [' ', '\n', '/*\n*/']) {
    add(`function* f() { yield${trivia}${term}; }`, 'Script', 'yield-ASI');
    add(`async function* f() { yield${trivia}${term}; }`, 'Script', 'async-yield-ASI');
  }
}

// Export-default function declarations are distinct from export-default
// expressions and preserve the no-LineTerminator-here async restriction.
for (const kind of ['async function', 'function*', 'async function*']) {
  for (const name of ['', 'named', 'await', 'yield']) {
    for (const body of ['', 'return await task;', 'yield value;', 'yield await task;']) {
      add(`export default ${kind} ${name}() { ${body} }`, 'Module', 'default-export');
    }
  }
}

const targeted = [
  'class A { static field = await; }',
  'class A { static field = () => await; }',
  'class A { static field = (await) => 1; }',
  'class A { static field = ({[await](){}}); }',
  'class A { static field = ({m(){return await;}}); }',
  'class A { static field = class { field = await; }; }',
  'class A { static { class B { field = await; } } }',
  'const f = async (a = () => await) => a;',
  'const f = async (a = (await) => 1) => a;',
  'const f = async (a = class await {}) => a;',
  'const f = async (a = class { field = await; }) => a;',
  'for (let f = async () => key in object;;) {}',
  'for (let f = (async () => key in object);;) {}',
  'function* f() { for (let i = yield key in source;;) {} }',
  'function* f() { for (let i = (yield (key in source));;) {} }',
  'async function f() { for (let i = await key in source;;) {} }',
  'async function f() { for (let i = (await (key in source));;) {} }',
  'async\nfunction f() {} /regexp/.test(value);',
  'function* f() {} /regexp/.test(value);',
  'async function* f() {} /regexp/.test(value);',
  'const f = async function* () {} / divisor;',
  'const f = async () => /regexp/;',
  'const object = { async run() {} } / divisor;',
  'async: while (ready) { break async; }',
  'async(value); async(value, other); async({run(){}});',
  'const object = {\\u0061sync run(){}};',
  'const object = {async \\u0072un(){await task;}};',
  'const object = {\\u0067et value(){}};',
  'const object = {get \\u0076alue(){}};'
];
for (const input of targeted) add(input, 'Script', 'targeted-boundary');

for (const item of cases) {
  let compiled = item.input;
  if (item.goal === 'Script') compiled = '"use strict";\n' + compiled;
  if (item.goal === 'Expression') compiled = '"use strict"; (' + compiled + '\n);';
  item.expected = true;
  try {
    if (item.goal === 'Module') new vm.SourceTextModule(compiled);
    else new vm.Script(compiled);
  } catch (error) {
    item.expected = false;
    item.nodeMessage = error.message;
  }
}
fs.writeFileSync('artifacts/function-kinds/matrix.json', JSON.stringify(cases.map(({input,goal,expected}) => ({input,goal,expected})), null, 2));
fs.writeFileSync('artifacts/function-kinds/matrix-audit.json', JSON.stringify({compileOnly:true,versions:process.versions,cases}, null, 2));
const counts = cases.reduce((result, item) => {
  const key = item.goal + ':' + item.expected;
  result[key] = (result[key] || 0) + 1;
  return result;
}, {});
console.log(JSON.stringify({total:cases.length,counts}));
for (const item of cases.filter(x => x.category === 'targeted-boundary')) console.log(JSON.stringify(item));
