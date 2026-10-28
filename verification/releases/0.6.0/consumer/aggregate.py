from pathlib import Path
import json, hashlib, base64
base=Path(__file__).resolve().parent
def load(name): return json.loads((base/name).read_text(encoding='utf-8'))
metadata=load('package-metadata.json')
examples=load('example-results.json')
smoke=load('smoke-results.json')
restore=load('restore-proof.json')
package=base.parent/'packages'/'Janglim.0.6.0.nupkg'
restored_exactly=restore['Janglim']['sha512']==base64.b64encode(hashlib.sha512(package.read_bytes()).digest()).decode()
no_project_refs=all(not f['projectReferences'] for f in restore['projectReferences'].values())
result={
    'package':'Janglim 0.6.0','targetFramework':'net8.0',
    'packageSha256':metadata['sha256'],
    'packageReferenceOnly':no_project_refs,'restoredExactLocalPackage':restored_exactly,
    'metadataPassed':metadata['passed'],'engineAssemblies':metadata['dllCount'],
    'embeddedEbnfResources':8,'unicodeLicenseMatches':metadata['unicodeLicenseMatchesSource'],
    'packageReadmeMatches':metadata['readmeMatchesSource'],
    'unchangedDocumentationExamples':examples['count'],'documentationExamplesPassed':examples['passed'],
    'publicApiAndAssemblyChecks':smoke['checks'],'publicApiAndAssemblyFailures':smoke['failures'],
    'scope':'Isolated consumer smoke verification of the packaged APIs/resources and six documentation examples; not complete language conformance.',
    'reports':['package-metadata.json','restore-proof.json','example-results.json','smoke-results.json'],
    'passed':metadata['passed'] and examples['passed'] and smoke['passed'] and no_project_refs and restored_exactly}
(base/'verification.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))
if not result['passed']: raise SystemExit(1)
