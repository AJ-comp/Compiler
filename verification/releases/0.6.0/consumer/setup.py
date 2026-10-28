from pathlib import Path
import re, json, zipfile, hashlib, xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[3]
base = Path(__file__).resolve().parent
package = base.parent / 'packages' / 'Janglim.0.6.0.nupkg'
base.mkdir(parents=True, exist_ok=True)
project = '''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><PackageReference Include="Janglim" Version="0.6.0" /></ItemGroup>
</Project>
'''
(base/'NuGet.Config').write_text('''<configuration><packageSources><clear/><add key="release" value="../packages"/><add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources><config><add key="globalPackagesFolder" value=".nuget"/></config></configuration>''',encoding='utf-8')
examples = []
for file, prefix in [('README.md','readme'),('src/Janglim/README.md','package-readme')]:
    text = (root/file).read_text(encoding='utf-8-sig')
    blocks = re.findall(r'```csharp\r?\n([\s\S]*?)\r?\n```',text)
    selected = [('ebnf',next(b for b in blocks if 'using System;' in b and 'EbnfGrammarReader.Read' in b)),
                ('javascript',next(b for b in blocks if 'using System;' in b and 'new JavaScriptGrammar()' in b)),
                ('typescript',next(b for b in blocks if 'using System;' in b and 'new TypeScriptGrammar()' in b))]
    for kind, source in selected:
        name = prefix+'-'+kind
        dest = base/name
        dest.mkdir(exist_ok=True)
        (dest/(name+'.csproj')).write_text(project,encoding='utf-8')
        (dest/'Program.cs').write_text(source+'\n',encoding='utf-8')
        examples.append({'id':name,'source':file,'kind':kind,'sha256':hashlib.sha256(source.encode()).hexdigest()})
(base/'examples.json').write_text(json.dumps(examples,indent=2),encoding='utf-8')
(base/'smoke').mkdir(exist_ok=True)
(base/'smoke'/'smoke.csproj').write_text(project,encoding='utf-8')
with zipfile.ZipFile(package) as z:
    names=z.namelist()
    dlls=[n for n in names if n.startswith('lib/netstandard2.1/') and n.endswith('.dll') and not n.endswith('.resources.dll')]
    ns={'n':'http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd'}
    metadata=ET.fromstring(z.read('Janglim.nuspec')).find('n:metadata',ns)
    def get(name):
        e=metadata.find('n:'+name,ns)
        return e.text if e is not None else None
    report={'package':str(package),'sha256':hashlib.sha256(package.read_bytes()).hexdigest(),'dllCount':len(dlls),'dlls':dlls,
            'id':get('id'),'version':get('version'),'description':get('description'),'releaseNotes':get('releaseNotes'),
            'readme':get('readme'),'license':get('license'),'tags':get('tags'),
            'readmeMatchesSource':z.read('README.md')==(root/'src/Janglim/README.md').read_bytes(),
            'unicodeLicenseMatchesSource':z.read('licenses/Unicode-LICENSE.txt')==(root/'grammars/javascript/unicode/LICENSE.txt').read_bytes()}
    report['passed']=len(dlls)==9 and report['version']=='0.6.0' and report['readme']=='README.md' and report['license']=='MIT' and report['readmeMatchesSource'] and report['unicodeLicenseMatchesSource'] and 'TypeScript' in report['description'] and '0.6.0' in report['releaseNotes']
    (base/'package-metadata.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    if not report['passed']: raise RuntimeError('package metadata verification failed')
print(json.dumps({'examples':len(examples),'package':report['passed'],'dlls':len(dlls)}))
