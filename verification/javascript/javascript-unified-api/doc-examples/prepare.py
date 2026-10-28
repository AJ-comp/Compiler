from pathlib import Path
import re, html, hashlib, json, shutil

root = Path(__file__).resolve().parents[3]
out = Path(__file__).resolve().parent
sources = sorted((root / 'docs/ko').glob('javascript*.html'))
sources += [root / 'README.md', root / 'src/Janglim/README.md', root / 'grammars/javascript/README.md']
entries = []
seen = {}
for source in sources:
    text = source.read_text(encoding='utf-8-sig')
    if source.suffix == '.html':
        sections = re.findall(r'<section\b[^>]*>(.*?)</section>', text, re.S)
        candidates = []
        for section in sections:
            blocks = [html.unescape(re.sub(r'<[^>]+>', '', code)).strip()
                      for code in re.findall(r'<pre\b[^>]*>\s*<code\b[^>]*>(.*?)</code>\s*</pre>', section, re.S)]
            for index, code in enumerate(blocks):
                if code.startswith('using ') and 'JavaScriptGrammar' in code:
                    if 'Parsing(' not in code:
                        code = '\n\n'.join(blocks[index:])
                    candidates.append(code)
    else:
        candidates = [code.strip() for code in re.findall(r'```csharp\s*\n(.*?)```', text, re.S)
                      if code.lstrip().startswith('using ') and 'JavaScriptGrammar' in code]
    for code in candidates:
        normalized = code.replace('\r\n', '\n').strip() + '\n'
        digest = hashlib.sha256(normalized.encode()).hexdigest()
        if digest in seen:
            seen[digest]['sources'].append(str(source.relative_to(root)))
            continue
        entry = {'id': len(entries) + 1, 'sources': [str(source.relative_to(root))], 'sha256': digest,
                 'inputKind': 'Expression' if 'JavaScriptInputKind.Expression' in code else
                              'Module' if 'JavaScriptInputKind.Module' in code else 'Script'}
        entries.append(entry)
        seen[digest] = entry
        (out / f'Example{entry["id"]:02d}.original.txt').write_text(normalized, encoding='utf-8')
        match = re.match(r'((?:using [^;]+;\s*)+)(.*)', normalized, re.S)
        usings, body = match.groups()
        wrapper = (f'namespace DocumentationExample{entry["id"]:02d}\n{{\n{usings}\n'
                   'internal static class Example\n{\npublic static bool Completed;\npublic static void Run()\n{\n'
                   + body + '\n'
                   + 'if (!parsed.Success || diagnostics.Count != 0)\n'
                   + '    throw new InvalidOperationException("Example did not pass parsing and validation.");\n'
                   + 'Completed = true;\n}\n}\n}\n')
        (out / f'Example{entry["id"]:02d}.cs').write_text(wrapper, encoding='utf-8')

selected = []
for kind in ['Script', 'Module', 'Expression']:
    eligible = [entry for entry in entries if entry['inputKind'] == kind and
                'javascript-regexp.html' not in str(entry['sources'])]
    # The non-strict Script example also exercises the two updated constructor options.
    chosen = next((entry for entry in eligible if 'javascript-non-strict.html' in str(entry['sources'])), eligible[0])
    selected.append(chosen)
main = 'using System;\n'
for entry in selected:
    name = f'DocumentationExample{entry["id"]:02d}.Example'
    main += f'{name}.Run();\n'
    main += f'if (!{name}.Completed) throw new InvalidOperationException("Example {entry["id"]} exited before validation.");\n'
    main += f'Console.WriteLine("VERIFIED {entry["inputKind"]} Example{entry["id"]:02d}: parsing=true, diagnostics=0");\n'
(out / 'Program.cs').write_text(main, encoding='utf-8')

library = out / 'lib'
library.mkdir(exist_ok=True)
test_output = root / 'Tests/Parse.FrontEnd.Parsers.Tests/bin/Release/net8.0'
names = ['AJ.Common.dll', 'ConsoleTables.dll', 'Newtonsoft.Json.dll', 'Parse.dll',
         'Parse.FrontEnd.dll', 'Parse.FrontEnd.Grammars.dll', 'Parse.FrontEnd.Grammars.Ebnf.dll',
         'Parse.FrontEnd.Parsers.dll', 'Wcwidth.dll']
references = []
for name in names:
    shutil.copy2(test_output / name, library / name)
    references.append(f'    <Reference Include="{Path(name).stem}"><HintPath>lib/{name}</HintPath></Reference>')
project = '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <TargetFramework>net8.0</TargetFramework>\n    <LangVersion>latest</LangVersion>\n    <Nullable>disable</Nullable>\n  </PropertyGroup>\n  <ItemGroup>\n'
project += '\n'.join(references) + '\n  </ItemGroup>\n</Project>\n'
(out / 'DocumentationExamples.csproj').write_text(project, encoding='utf-8')
manifest = {'uniqueCompleteExamples': len(entries), 'scannedFiles': [str(source.relative_to(root)) for source in sources],
            'examples': entries, 'executedExampleIds': [entry['id'] for entry in selected],
            'notes': ['HTML code entities decoded.', 'Adjacent staged C# blocks joined only inside the same usage section.',
                      'Original snippets preserved in *.original.txt; wrappers append validation assertions.',
                      'All assembly references point to copied Release DLL snapshots; no ProjectReference.']}
(out / 'manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(manifest, ensure_ascii=False, indent=2))
