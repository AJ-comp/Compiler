const vm = require('vm');
let count=0,unexpected=[];
for(const target of ['1','true','null','this','a?.member','a?.[key]','f()','new C()','tag`text`','`text`','eval','arguments','\\u0065val','\\u0061rguments']) {
 for(const input of [`[${target}] = values`,`[...${target}] = values`,`({name: ${target}} = source)`,`({...${target}} = source)`]) {
  let accepted=true; try{new vm.Script('"use strict"; ('+input+');');}catch{accepted=false;}
  count++;if(accepted)unexpected.push(input);
 }
}
for(const op of ['+=','-=','*=','/=','%=','**=','<<=','>>=','>>>=','&=','^=','|=','&&=','||=','??='])for(const input of [`[a] ${op} values`,`({a} ${op} source)`]){try{new vm.Script('"use strict"; ('+input+');');unexpected.push(input);}catch{}count++;}
for(const pattern of ['[a]','({a})'])for(const op of ['++','--'])for(const input of [op+pattern,pattern+op]){try{new vm.Script('"use strict"; ('+input+');');unexpected.push(input);}catch{}count++;}
console.log(JSON.stringify({count,unexpected},null,2));
