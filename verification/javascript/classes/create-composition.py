import json,pathlib
out=[]; groups={}
def add(group,source):
 if source not in seen:
  seen.add(source);out.append(source);groups[group]=groups.get(group,0)+1
seen=set()
# Property names interact with contextual static/get/set and with slash goals.
names=['value','static','get','set','async','function','if','class',"'name'","'constructor'","'prototype'","['name']",'[value / divisor]','#value']
for name in names:
 for element in [f'{name}(){{return /a/;}}',f'static {name}(){{return value / divisor;}}',f'get {name}(){{return /a/;}}',f'set {name}(value){{value / divisor;}}',f'{name}=/a/;',f'static {name}=()=>/a/;',f'static\n{name}(){{return /a/;}}',f'get\n{name}(){{return /a/;}}']:
  add('names',f'class C {{ {element} }}')
# A newline is allowed within an accessor/static modifier and can also separate fields.
for head in ['static','get','set','async','function','if','class','[key]','#value',"'name'"]:
 for tail in ['value;','value=/a/;','value(){return /a/;}','{}','static value=/a/;','get value(){return /a/;}','[key]=/a/;','/a/.test(value);']:
  add('prefix-newline',f'class C {{ {head}\n{tail} }}')
# Both class declaration and class expression heritage exercise independent nested braces.
heritage=['Base','object.Base','factory()','(ready?First:Second)','(function(){return Base;})()','class {}','(class {})','({Base}).Base','(class {static get Base(){return /a/;}}).Base','new Factory()','object?.Base','(value / divisor)','tag`base`','(()=>Base)()']
bodies=['','method(){return /a/;}','field=/a/;','static { /a/.test(value); }']
for base in heritage:
 for body in bodies:
  add('heritage',f'class C extends {base} {{ {body} }} /z/.test(value);')
  add('heritage',f'const C=class extends {base} {{ {body} }} / divisor;')
# The closing class brace has different roles in declarations and expressions.
heads=['class C {}','const C=class {}','(class {})','new class {}','const C=()=>class {}','const C=class extends Base {method(){}}','class C extends Base {static {}}','const C=function(){return class {}; }']
tails=['(argument);','[key];','.name;','?.name;','`text`;','/a/.test(value);','/ divisor;','+value;','other();','; /a/.test(value);']
for head in heads:
 for tail in tails:add('class-suffix',head+'\n'+tail)
# Test field-initializer ASI against calls, computed fields, templates and block boundaries.
values=['/a/','value / divisor','()=>{}','value=>value','function(){}','class {}','class Named {}','(class {})','({value:1})','tag`raw\\xZ`','`text ${/a/.source}`','()=>class {}']
field_tails=['(argument);','[key];','[key]=1;','`text`;','/a/.test(value);','+1;','method(){return /a/;}','static { /a/.test(value); }','other=/b/;',';other=/b/;']
for value in values:
 for tail in field_tails:add('field-suffix',f'class C {{ field={value}\n{tail} }}')
# Declaration order, getter/setter matching and lexical nested-class private capture.
declarations=['#x;','#x(){}','get #x(){return 1;}','set #x(value){}','get #x(){} set #x(value){}','static #x;','#x;#x;','#\\u0078;','#y;','#x; static #x;']
uses=['method(){return this.#x;}','method(){return object?.#x;}','method(){return #x in object;}','method(){delete this.#x;}','method(){return ()=>this.#x;}','method(){return function(object){return object.#x;};}','method(){return class {method(object){return object.#x;}};}','field=object.#x;','static {object.#x;}','method(){return class extends object.#x {#x;};}']
for declaration in declarations:
 for use in uses:add('private',f'class C {{ {declaration} {use} }}')
# Nested contexts reset method permissions but preserve lexical arrow permissions.
inner=['class Inner {[new.target]=1;}','class Inner {[arguments]=1;}','class Inner {[super.value](){}}','class Inner {field=arguments;}','class Inner {field=()=>arguments;}','class Inner {field=function(){return arguments;};}','class Inner {method(){return super.value;}}','class Inner {constructor(){super();}}','class Inner extends Base {constructor(){super();}}','class Inner {static {return;}}']
wrappers=['{code}','function outer(){{{code}}}','const outer=()=>{{{code}}};','class Outer {{method(){{{code}}}}}','class Outer extends Base {{constructor(){{{code}}}}}','class Outer {{static {{{code}}}}}','while(ready){{{code}}}','class Outer {{field=()=>{{{code}}};}}']
for item in inner:
 for wrapper in wrappers:add('scope-reset',wrapper.format(code=item))
root=pathlib.Path('artifacts/classes');root.mkdir(parents=True,exist_ok=True)
extra=root/'composition-extra-inputs.json'
if extra.exists():
 for source in json.loads(extra.read_text(encoding='utf-8')):add('modifier-and-nested-regressions',source)
(root/'composition-inputs.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf-8')
(root/'composition-groups.json').write_text(json.dumps(groups,indent=2),encoding='utf-8')
print('inputs',len(out));print(groups)
