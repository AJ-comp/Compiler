from pathlib import Path
import concurrent.futures, subprocess, json, hashlib, re
base=Path(__file__).resolve().parent
dotnet=Path.home()/'.dotnet'/'dotnet.exe'
examples=json.loads((base/'examples.json').read_text())
def run(item):
    project=base/item['id']/(item['id']+'.csproj')
    output=subprocess.run([str(dotnet),'run','--project',str(project),'-c','Release'],capture_output=True,text=True,encoding='utf-8',timeout=480)
    (base/(item['id']+'.log')).write_text(output.stdout+output.stderr,encoding='utf-8')
    lines=output.stdout.strip().splitlines()
    final=lines[-1] if lines else ''
    expected=final=='True' if item['kind']!='javascript' else 'async' in final and 'load' in final and 'return' in final
    actual=(project.parent/'Program.cs').read_text(encoding='utf-8').removesuffix('\n')
    unchanged=hashlib.sha256(actual.encode()).hexdigest()==item['sha256']
    result={**item,'exitCode':output.returncode,'unchanged':unchanged,'lastOutput':final,'passed':output.returncode==0 and expected and unchanged}
    print(json.dumps(result),flush=True)
    return result
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as executor:
    results=list(executor.map(run,examples))
(base/'example-results.json').write_text(json.dumps({'passed':all(r['passed'] for r in results),'count':len(results),'results':results},indent=2),encoding='utf-8')
