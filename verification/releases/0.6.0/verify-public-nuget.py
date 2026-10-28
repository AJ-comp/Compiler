from pathlib import Path
import argparse, json, hashlib, urllib.request, urllib.error, zipfile, xml.etree.ElementTree as ET
from datetime import datetime, timezone

parser=argparse.ArgumentParser()
parser.add_argument('--ci-package',type=Path,required=True)
parser.add_argument('--commit',default='efe182f49d3ff265e42e93a33bc144a1811c4c9c')
args=parser.parse_args()
base=Path(__file__).resolve().parent
dest=base/'public-nuget'
dest.mkdir(exist_ok=True)
url='https://api.nuget.org/v3-flatcontainer/janglim/0.6.0/janglim.0.6.0.nupkg'
try:
    with urllib.request.urlopen(url,timeout=60) as response:
        content=response.read()
except urllib.error.HTTPError as error:
    event={'url':url,'status':error.code,'checkedAt':datetime.now(timezone.utc).isoformat()}
    with (dest/'availability.jsonl').open('a',encoding='utf-8') as log: log.write(json.dumps(event)+'\n')
    print(json.dumps(event))
    raise SystemExit(2)
(dest/'Janglim.0.6.0.nupkg').write_bytes(content)
sha=lambda b:hashlib.sha256(b).hexdigest()
ci_content=args.ci_package.read_bytes()
expected_dlls={'AJ.Common.dll','Janglim.dll','Parse.dll','Parse.FrontEnd.dll','Parse.FrontEnd.Grammars.dll','Parse.FrontEnd.Parsers.dll','Parse.FrontEnd.Grammars.Ebnf.dll','Parse.FrontEnd.ErrorHandler.dll','Parse.FrontEnd.Support.dll'}
with zipfile.ZipFile(dest/'Janglim.0.6.0.nupkg') as z:
    dlls=[n for n in z.namelist() if n.startswith('lib/netstandard2.1/') and n.endswith('.dll') and not n.endswith('.resources.dll')]
    # Extract only known binaries needed for the separate reflection check.
    for name in dlls:
        if Path(name).name not in expected_dlls: raise ValueError('Unexpected engine binary: '+name)
        (dest/Path(name).name).write_bytes(z.read(name))
    root=ET.fromstring(z.read('Janglim.nuspec'))
    metadata=next(e for e in root if e.tag.endswith('metadata'))
    values={e.tag.rsplit('}',1)[-1]:e for e in metadata}
    version=values['version'].text
    commit=values['repository'].attrib.get('commit')
    readme=z.read('README.md').decode('utf-8-sig')
    license=z.read('licenses/Unicode-LICENSE.txt').decode('utf-8-sig')
    with zipfile.ZipFile(args.ci_package) as ci:
        readme_matches_ci=z.read('README.md')==ci.read('README.md')
        unicode_matches_ci=z.read('licenses/Unicode-LICENSE.txt')==ci.read('licenses/Unicode-LICENSE.txt')
        ci_names=set(ci.namelist())- {'.signature.p7s'}
        public_names=set(z.namelist())- {'.signature.p7s'}
        changed=[name for name in sorted(ci_names & public_names) if z.read(name)!=ci.read(name)]
        added=sorted(public_names-ci_names)
        removed=sorted(ci_names-public_names)
        payload_matches=not changed and not added and not removed
        signatures={ 'public':'.signature.p7s' in z.namelist(), 'ci':'.signature.p7s' in ci.namelist() }
    result={'url':url,'version':version,'repositoryCommit':commit,'expectedCommit':args.commit,
        'publicSha256':sha(content),'ciPackage':str(args.ci_package.resolve()),'ciSha256':sha(ci_content),
        'archiveMatchesCi':content==ci_content,'payloadMatchesCi':payload_matches,
        'payloadEntryCount':len(public_names),'payloadChanged':changed,'payloadAdded':added,'payloadRemoved':removed,
        'signatureEntries':signatures,'signatureVerification':'Run dotnet nuget verify --all separately.',
        'engineDlls':dlls,
        'readmeMetadata':values['readme'].text,'licenseMetadata':values['license'].text,
        'readmeVersionPresent':'0.6.0' in readme,'readmeMatchesCi':readme_matches_ci,
        'unicodeLicensePresent':'UNICODE' in license.upper() and len(license)>500,'unicodeLicenseMatchesCi':unicode_matches_ci,
        'passed':version=='0.6.0' and commit==args.commit and payload_matches and signatures['public'] and
            {Path(n).name for n in dlls}==expected_dlls and values['readme'].text=='README.md' and values['license'].text=='MIT' and
            readme_matches_ci and unicode_matches_ci and '0.6.0' in readme and 'UNICODE' in license.upper()}
    (dest/'package-verification.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
    print(json.dumps(result,indent=2))
    if not result['passed']: raise SystemExit(1)
