// Compile syntax only. No source program, regexp matching, or user input is run.
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');
const cases = JSON.parse(fs.readFileSync(path.join(__dirname, 'standard-audit-cases.json'), 'utf8'));
const results = cases.map(item => {
  let constructorValid = false, literalValid = false, constructorError, literalError;
  try { new RegExp(item.body, item.flags); constructorValid = true; }
  catch (error) { constructorError = error.message; }
  const source = '"use strict"; const expression = /' + item.body + '/' + item.flags + ';';
  try { new vm.Script(source); literalValid = true; }
  catch (error) { literalError = error.message; }
  return { ...item, constructorValid, literalValid, constructorError, literalError,
    expected: item.specValid ?? constructorValid,
    oracleException: item.specValid !== undefined && item.specValid !== constructorValid };
});
const report = { versions: process.versions, compileOnly: true, cases: results };
fs.writeFileSync(path.join(__dirname, 'standard-audit-oracle.json'), JSON.stringify(report, null, 2));
const exceptions = results.filter(item => item.oracleException);
console.log(JSON.stringify({ total: results.length,
  valid: results.filter(item => item.expected).length,
  invalid: results.filter(item => !item.expected).length,
  specOverrides: exceptions.length,
  literalConstructorDifferences: results.filter(item => item.constructorValid !== item.literalValid).length,
  exceptions: exceptions.map(({ body, flags, note }) => ({ body, flags, note })) }, null, 2));
