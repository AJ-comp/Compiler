const fs = require('node:fs');
const vm = require('node:vm');
const samples = new Set();
const atoms = ['a', 'b', '1', '0n', 'true', 'null', 'this', '"s"', '/x/g', '[]', '{}', '(a)', 'f()', 'a.b', 'a?.b'];
for (const a of atoms) {
  samples.add(a);
  for (const op of ['+', '-', '*', '/', '%', '**', '==', '===', '<', 'in', 'instanceof', '<<', '>>>', '&', '|', '^', '&&', '||', '??', '=', '+=', '&&=', '??=', ','])
    for (const b of ['b', '2', '/a/', '(b=c)']) samples.add(`${a} ${op} ${b}`);
  for (const prefix of ['+', '-', '!', '~', 'typeof ', 'void ', 'delete ', '++', '--', 'new ']) samples.add(prefix + a);
  for (const suffix of ['++', '--', '.x', '[b]', '(b)', '?.x', '?.[b]', '?.(b)']) samples.add(a + suffix);
}
for (const text of [
  'a??b||c', 'a||b??c', '(a??b)||c', 'a??(b||c)', '-2**2', '2**-2', '(-2)**2',
  'a?.3:0', 'a?.b=2', '(a?.b).c=2', 'new a?.b()', 'new (a?.b)()',
  'delete (a,b)', 'delete ((a))', 'a\n++', 'a/*\n*/++', 'a/**/++',
  'a/ /b/.source', '/a/ / b', 'a=/a/g', 'a/=/x/.test(s)',
  '{__proto__:1,a:{__proto__:2}}', '{__proto__:1,__proto__:2}', '{["__proto__"]:1,__proto__:2}',
  '{"__pr\\u006fto__":1,__proto__:2}', '{"\\u{000000000000000000000000005f}_proto__":1,__proto__:2}',
  '\\u0061wait+1', '\\u0073tatic', 'obj.\\u0073tatic',
  '[,]', '[,,]', '[a,,b,]', '[...a,]', 'f(...a,)', '({a, ...b,})', '({a:1,b:2})',
  '0..toString()', '1.toString()', '1.e2', '1.e', '0_1', '1_2.3_4e+5_6', '00n',
  '"\\u{0000000000000000000000000041}"', '"\\8"', '"\\q"', '"\\u{110000}"'
]) samples.add(text);
const cases = [...samples].map(input => {
  let expected = true;
  try { new vm.Script('"use strict"; (' + input + '\n)'); } catch (e) { if (e instanceof SyntaxError) expected = false; else throw e; }
  return { input, expected };
});
fs.writeFileSync(process.argv[2], JSON.stringify(cases));
console.log(JSON.stringify({ node: process.version, unicode: process.versions.unicode, cases: cases.length }));
