const vm=require('node:vm');
for(const input of [
'const f = () => {}\n/a/.test(value);',
'const f = () => {} /a/.test(value);',
'const f = () => {}\n(value);',
'const f = () => {}\n[value];',
'const f = () => {}\n`value`;',
'const f = value => value\n++other;',
'const value = (first,);',
'const value = (...rest);',
'const value = ();',
'const value = ({name = 1});',
'const f = (value = () => {}) => /a/;',
'const f = (value = function () { return /a/; }) => value / divisor;',
'const f = (value = (first, second)) => value;',
'const f = (value = first, second) => value;',
'const f = () => { return () => {}; }; /a/.test(value);'
]){let valid=true,message='';try{new vm.Script('"use strict"; '+input);}catch(e){valid=false;message=e.message;}console.log(JSON.stringify({input,valid,message}));}
