import json
from pathlib import Path

root = Path.cwd()
task = Path((root / 'artifacts/typescript/current-class-extensions-dir.txt').read_text(encoding='utf-8-sig').strip())
cases = []
def add(group, expected, *sources):
    for source in sources:
        cases.append({'id': f'{group}-{len(cases)+1:03}', 'group': group, 'expected': expected, 'source': source, 'kind': 'ts'})

add('fields', True,
    'class C { public value: number; }',
    'class C { private value = 1; }',
    'class C { protected value?: string; }',
    'class C { readonly value: number = 1; }',
    'class C { public readonly value: number = 1; }',
    'class C { protected static readonly value: number = 1; }',
    'class C { value!: number; }',
    'class C { private value!: Box<User>; }',
    'class C { private readonly ["value"]: number = 1; }',
    'class C { readonly #value: number = 1; }',
    'class C { declare value: number; }',
    'class C { declare readonly value: number; }',
    'class C { accessor value: number = 1; }',
    'class C { public static accessor value = 1; }',
    'class C { accessor #value = 1; }',
    'class C { [key: string]: unknown; }',
    'class C { static readonly [key: string]: unknown; }',
    'class C { value?: number = 1; }',
)
add('methods', True,
    'class C { public run(value: number): number { return value; } }',
    'class C { private run<T>(value: T): T { return value; } }',
    'class C { protected static run(): void {} }',
    'class C extends B { override run(): void {} }',
    'class C extends B { public override run(): void {} }',
    'class C extends B { public static override run(): void {} }',
    'class C { public async run(): Promise<void> { await work(); } }',
    'class C { protected *run(): Iterable<number> { yield 1; } }',
    'class C { private async *run(): AsyncIterable<number> { yield await work(); } }',
    'class C { public get value(): number { return 1; } public set value(next: number) {} }',
    'class C { protected static get value(): number { return 1; } }',
    'class C { run?(): void {} }',
    'class C { public run?(): void {} }',
    'class C { public constructor() {} }',
    'class C { protected constructor() {} }',
    'class C { private constructor(value: number) {} }',
)
add('heritage', True,
    'abstract class C {}',
    'abstract class C<T> {}',
    'abstract class C { abstract value: number; abstract run(): void; }',
    'abstract class C { public abstract run<T>(value: T): T; }',
    'abstract class C { protected abstract readonly value: number; }',
    'abstract class C { abstract get value(): number; abstract set value(next: number); }',
    'abstract class C extends B { abstract override run(): void; }',
    'class C implements Named {}',
    'class C implements Named, Printable<number> {}',
    'class C<T extends Named = Named> extends Base<T> implements Named, Printable<T> {}',
    'const C = class<T> extends Base<T> implements Named {};',
    'const C = class Local<T> implements Named {};',
    'class C extends Base<T> {}',
    'class C extends ns.Base<Box<T>> {}',
    'class C extends (factory()) implements Named {}',
    'class C<T> { run<U extends T>(value: U): U { return value; } }',
)
add('parameters', True,
    'class C { constructor(public value: number) {} }',
    'class C { constructor(private value: number) {} }',
    'class C { constructor(protected value: number) {} }',
    'class C { constructor(readonly value: number) {} }',
    'class C { constructor(public readonly value: number) {} }',
    'class C { constructor(private readonly value = 1) {} }',
    'class C { constructor(protected readonly value?: string) {} }',
    'class C { constructor(public value: number = 1, private name?: string) {} }',
    'class C { constructor(public value: Box<User>) { this.value = value; } }',
    'class C extends B { constructor(public override value: number) { super(); } }',
    'class C extends B { constructor(public override readonly value: number) { super(); } }',
    'class C { constructor(public value = /ab/.test(text)) {} }',
    'class C { constructor(public value = () => 1) {} }',
    'class C { constructor(readonly value = function() { return 1; }) {} }',
)
add('signatures', True,
    'class C { run(value: string): string; run(value: number): number; run(value: unknown): unknown { return value; } }',
    'class C { constructor(value: string); constructor(value: number); constructor(value: unknown) {} }',
    'class C { constructor(value: number); constructor(public value: number) {} }',
    'class C { public run(value: string): string; public run(value: unknown): unknown { return value; } }',
    'class C { static run(value: string): string; static run(value: unknown): unknown { return value; } }',
    'declare class C { value: number; readonly name: string; run(value: number): void; constructor(value: number); }',
    'declare abstract class C { abstract run(): void; }',
    'declare class C<T> extends Base<T> implements Named { protected value: T; }',
    'declare class C { get value(): number; set value(next: number); }',
    'declare class C { readonly value = 1; }',
    'declare class C { private readonly value = "ready"; }',
    'declare class C { private constructor(); }',
)
add('contextual', True,
    'class C { public() {} private() {} protected() {} }',
    'class C { readonly() {} abstract() {} override() {} }',
    'class C { declare() {} accessor() {} }',
    'class C { public: number; private: number; protected: number; }',
    'class C { readonly = 1; abstract = 2; override = 3; }',
    'class C { declare = 1; accessor = 2; }',
    'class C { readonly; value: number; }',
    'class C { public\nvalue: number; }',
    'class C { readonly\nvalue: number; }',
    'class C { static public() {} }',
    'class C { public static() {} }',
    'class C { get public(): number { return 1; } }',
    'class C { public /* comment */ readonly value: number; }',
    'abstract\nclass C {}',
)
add('runtime', True,
    'class C { public run() { return /ab/.test(text); } } const result = value / 2;',
    'class C { public value: Box<T> = left > right; }',
    'class C { public value = { count: 1 }; } /ab/.test(text);',
    'class C { public #method() {} }', # Classified below by authoritative checker.
    'class C { #value = 1; readonly other = this.#value; public get() { return this.#value; } }',
)

add('modifier-errors', False,
    'class C { public private value: number; }',
    'class C { public public value: number; }',
    'class C { static static value: number; }',
    'class C { readonly readonly value: number; }',
    'class C extends B { override override value: number; }',
    'class C { static public value: number; }',
    'class C { readonly public value: number; }',
    'class C { readonly static value: number; }',
    'class C extends B { override public value: number; }',
    'class C extends B { readonly override value: number; }',
    'class C extends B { override static value: number; }',
    'class C extends B { async override run() {} }',
    'class C { readonly run(): void {} }',
    'class C { readonly get value(): number { return 1; } }',
    'class C { public #value: number; }',
    'class C { private #run() {} }',
    'class C { declare #value: number; }',
    'class C extends B { declare override value: number; }',
    'class C { readonly accessor value = 1; }',
    'class C { declare accessor value: number; }',
    'class C { accessor run() {} }',
    'class C { accessor value?: number; }',
)
add('abstract-errors', False,
    'class C { abstract value: number; }',
    'class C { abstract run(): void; }',
    'abstract class C { abstract abstract run(): void; }',
    'abstract class C { private abstract run(): void; }',
    'abstract class C { abstract private value: number; }',
    'abstract class C { static abstract run(): void; }',
    'abstract class C { abstract static value: number; }',
    'abstract class C { async abstract run(): void; }',
    'abstract class C { abstract run(): void {} }',
    'abstract class C { abstract value: number = 1; }',
    'abstract class C { abstract #value: number; }',
    'const C = abstract class {};',
)
add('field-errors', False,
    'class C { value!; }',
    'class C { value!: number = 1; }',
    'class C { static value!: number; }',
    'abstract class C { abstract value!: number; }',
    'declare class C { value!: number; }',
    'class C { declare value!: number; }',
    'class C { value?!: number; }',
    'class C { value!?: number; }',
    'class C { [key: string]; }',
    'class C { public [key: string]: unknown; }',
    'class C { [key?: string]: unknown; }',
    'class C { constructor: number; }',
)
add('parameter-errors', False,
    'class C { constructor(public private value: number) {} }',
    'class C { constructor(readonly public value: number) {} }',
    'class C { constructor(public ...values: number[]) {} }',
    'class C { constructor(readonly { value }: { value: number }) {} }',
    'class C { constructor(public [value]: number[]) {} }',
    'class C { constructor(public constructor: number) {} }',
    'class C { run(public value: number) {} }',
    'function f(readonly value: number) {}',
    'class C { constructor(public this: C) {} }',
    'class C { constructor(static value: number) {} }',
    'class C { constructor(abstract value: number) {} }',
    'class C { constructor(public value?: number, next: number) {} }',
    'declare class C { constructor(public value: number); }',
    'class C { constructor(public value: number); constructor(value: number) {} }',
)
add('signature-errors', False,
    'class C { constructor<T>() {} }',
    'class C { constructor(): void {} }',
    'class C { readonly constructor() {} }',
    'class C { override constructor() {} }',
    'class C { declare run(): void; }',
    'declare class C { run(): void {} }',
    'declare class C { value: number = 1; }',
    'declare class C { readonly value: number = 1; }',
    'declare class C { async run(): Promise<void>; }',
    'class C { get value?(): number { return 1; } }',
    'class C { set value(next?: number) {} }',
)
add('heritage-errors', False,
    'class C implements {}',
    'class C implements A, {}',
    'class C extends A, B {}',
    'class C extends A extends B {}',
    'class C implements A implements B {}',
    'class C implements A extends B {}',
    'class C extends Base<> {}',
    'class C implements Named<> {}',
)
add('runtime-errors', False,
    'class C { public run() { return missing; } private #value = 1; }', # Classified below: semantic and private grammar.
    'class C { public run() { super(); } }',
    'class C { public value = arguments; }',
    'class C { public run() { return this.#missing; } }',
    'class C { public run() { let name; let name; } }',
    'class C { public run(value: number, value: number) {} }',
)
add('private-optional', True,
    'class C { #x?: number }',
    'class C { #m?(): void; }',
)
add('constructor-optional', False,
    'class C { constructor?() {} }',
)

# Accessibility modifiers apply to ordinary properties only, including methods.
cases[next(i for i,c in enumerate(cases) if c['source']=='class C { public #method() {} }')]['expected'] = False

task.mkdir(parents=True, exist_ok=True)
(task/'class-extension-cases.json').write_text(json.dumps(cases, indent=2), encoding='utf-8')
(task/'class-extension-cases.jsonl').write_text(''.join(json.dumps(c)+'\n' for c in cases), encoding='utf-8')
case_dir = task/'native-class-cases'
case_dir.mkdir(exist_ok=True)
for case in cases:
    (case_dir/(case['id']+'.ts')).write_text('export {};\n'+case['source']+'\n', encoding='utf-8')
(task/'native-class-tsconfig.json').write_text(json.dumps({'compilerOptions':{'target':'esnext','noEmit':True,'skipLibCheck':True},'include':['native-class-cases/*.ts']},indent=2),encoding='utf-8')

names = {
 'fields': 'Accepts_accessibility_readonly_optional_definite_and_accessor_fields',
 'methods': 'Accepts_typed_methods_accessors_and_constructor_visibility',
 'heritage': 'Accepts_abstract_classes_implements_and_generic_heritage',
 'parameters': 'Accepts_constructor_parameter_properties_without_type_checking',
 'signatures': 'Accepts_overload_and_ambient_signatures',
 'contextual': 'Modifier_spellings_keep_their_name_and_line_break_roles',
 'runtime': 'Class_extensions_preserve_runtime_lexical_and_private_name_contexts',
 'modifier-errors': 'Rejects_duplicate_misordered_and_inapplicable_modifiers',
 'abstract-errors': 'Rejects_invalid_abstract_member_contexts_and_implementations',
 'field-errors': 'Rejects_invalid_definite_optional_and_index_fields',
 'parameter-errors': 'Rejects_invalid_parameter_properties',
 'signature-errors': 'Rejects_invalid_constructor_accessor_and_ambient_signatures',
 'heritage-errors': 'Rejects_invalid_extends_and_implements_lists',
 'runtime-errors': 'Class_extensions_do_not_hide_existing_JavaScript_early_errors',
 'private-optional': 'Private_fields_and_signature_methods_can_be_optional',
 'constructor-optional': 'Constructors_cannot_be_optional',
}
header='''using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

// Syntax cases are compared with the pinned Microsoft TypeScript 7.0.2 parser
// and grammar checks. Unresolved type names and assignability are intentionally
// outside the language grammar's acceptance criteria.
[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptClassExtensions")]
public sealed class TypeScriptClassExtensionTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;
'''
blocks=[]
for group,name in names.items():
    selected=[c for c in cases if c['group']==group]
    for expected in (True,False):
        rows=[c for c in selected if c['expected']==expected]
        if not rows: continue
        suffix='' if all(c['expected']==expected for c in selected) else ('_valid' if expected else '_invalid')
        lines=['','    [Theory]']
        lines += ['    [InlineData('+json.dumps(c['source'],ensure_ascii=True)+')]' for c in rows]
        lines += ['    public void '+name+suffix+'(string source)', '        => fixture.'+('Accept' if expected else 'Reject')+'(source);']
        blocks.append('\n'.join(lines))
fact='''

    [Fact]
    public void Validation_preserves_the_original_class_syntax_tokens_positions_and_parent_links()
    {
        const string source = "interface Named { id: number; } " +
            "abstract class Base<T> { protected abstract run(value: T): T; } " +
            "class Model extends Base<number> implements Named { " +
            "constructor(public readonly id: number) { super(); } " +
            "protected override run(value: number): number { return value; } }";
        var parsed = fixture.Parser.Parsing(fixture.Language.CreateTokenSource(source));
        Assert.True(parsed.Success, source + Environment.NewLine + fixture.Parser.FiredException + Environment.NewLine +
            string.Join(Environment.NewLine, parsed.AllErrors.Select(error => error.Message)));
        Assert.Empty(parsed.AllErrors);
        var tree = Assert.IsType<ParseTreeNonTerminal>(parsed.ToParseTree);
        var nodes = Descendants(tree).ToArray();
        var parents = nodes.Select(node => node.Parent).ToArray();
        var tokens = tree.AllTokens.ToArray();

        Assert.Empty(fixture.Language.Validate(tree));
        Assert.Empty(fixture.Language.Validate(tree));

        Assert.Equal(tokens, tree.AllTokens);
        Assert.Contains(tokens, token => token.Input == "implements");
        Assert.Contains(tokens, token => token.Input == "readonly");
        Assert.Contains(tokens, token => token.Input == "override");
        for (int i = 0; i < nodes.Length; i++) Assert.Same(parents[i], nodes[i].Parent);
        foreach (var token in tokens)
            Assert.Equal(token.Input, source.Substring(token.StartIndex, token.EndIndex - token.StartIndex + 1));
    }

    private static IEnumerable<ParseTreeSymbol> Descendants(ParseTreeSymbol root)
    {
        var pending = new Stack<ParseTreeSymbol>();
        pending.Push(root);
        while (pending.Count != 0)
        {
            var symbol = pending.Pop();
            yield return symbol;
            if (symbol is not ParseTreeNonTerminal node) continue;
            foreach (var child in node.Items) pending.Push(child);
        }
    }
}
'''
test_file=root/'Tests/Parse.FrontEnd.Parsers.Tests/TypeScript/TypeScriptClassExtensionTests.cs'
test_file.write_text(header+'\n'.join(blocks)+fact,encoding='utf-8',newline='\r\n')
print(json.dumps({'task':str(task),'count':len(cases),'valid':sum(c['expected'] for c in cases),'invalid':sum(not c['expected'] for c in cases)}))
