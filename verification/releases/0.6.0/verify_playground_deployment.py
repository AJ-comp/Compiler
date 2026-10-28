"""Compare production bytes with the exact tested artifact, after successful deployment."""
from __future__ import annotations

import base64
import gzip
import hashlib
import json
from pathlib import Path
import re
import subprocess
import urllib.request
from datetime import datetime, timezone

ROOT = Path(__file__).resolve().parent
ARTIFACT = ROOT / "playground-site-40664356048-1"
SERVED = ROOT / "playground-served-40664356048"
RUN = "40664356048"
COMMIT = "efe182f49d3ff265e42e93a33bc144a1811c4c9c"
BASE = "https://janglim.mythosia.net/"


def gh_json(*args: str):
    completed = subprocess.run(["gh", *args], check=True, capture_output=True, text=True, encoding="utf-8")
    return json.loads(completed.stdout)


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def run() -> int:
    workflow = gh_json("run", "view", RUN, "--repo", "AJ-comp/Compiler", "--json", "headSha,status,conclusion,jobs,url")
    deployment = [job for job in workflow["jobs"] if "Deploy verified static files" in job["name"]]
    if workflow["headSha"] != COMMIT:
        raise RuntimeError("Workflow commit differs from the approved release commit.")
    if not deployment or any(job["conclusion"] != "success" for job in deployment):
        raise RuntimeError("Deployment must succeed before checking served files.")
    if not (ARTIFACT / "index.html").is_file():
        raise RuntimeError("Download the exact playground-site-40664356048-1 artifact first.")

    index = (ARTIFACT / "index.html").read_text(encoding="utf-8")
    paths = {"index.html", "ui/playground.js", "ui/style.css", "ui/locale.js", "css/app.css"}
    paths.update(re.findall(r'(?:src|href)="(_framework/[^"?]+\.js)"', index))
    dotnet_files = [p for p in (ARTIFACT / "_framework").glob("dotnet.*.js")
                    if not p.name.startswith(("dotnet.native.", "dotnet.runtime."))]
    if len(dotnet_files) != 1:
        raise RuntimeError(f"Expected one fingerprinted dotnet loader; found {len(dotnet_files)}.")
    loader = dotnet_files[0]
    paths.add(loader.relative_to(ARTIFACT).as_posix())
    loader_text = loader.read_text(encoding="utf-8")
    match = re.search(r"/\*json-start\*/\s*(\{.*?\})\s*/\*json-end\*/", loader_text, re.S)
    if not match:
        raise RuntimeError("Could not read the embedded .NET boot manifest.")
    boot = json.loads(match.group(1))
    engine_resources = []

    def visit(value):
        if isinstance(value, list):
            for item in value:
                visit(item)
        elif isinstance(value, dict):
            name = value.get("name", "")
            virtual = value.get("virtualPath", "")
            if name.endswith(".wasm") and ".resources." not in virtual and virtual.startswith(("AJ.Common", "Parse.", "Parse.wasm", "Janglim.Playground")):
                path = "_framework/" + name
                paths.add(path)
                expected_integrity = value.get("hash")
                local_integrity = "sha256-" + base64.b64encode(hashlib.sha256((ARTIFACT / path).read_bytes()).digest()).decode("ascii")
                if expected_integrity != local_integrity:
                    raise RuntimeError(f"Artifact boot integrity does not match {path}.")
                engine_resources.append({"virtualPath": virtual, "path": path, "integrity": expected_integrity})
            for key, item in value.items():
                if isinstance(item, (dict, list)):
                    visit(item)

    visit(boot["resources"])
    if not any(r["virtualPath"] == "Parse.FrontEnd.Parsers.wasm" for r in engine_resources):
        raise RuntimeError("Boot manifest omitted the parser engine resource.")
    if not any(r["virtualPath"] == "Parse.FrontEnd.Grammars.Ebnf.wasm" for r in engine_resources):
        raise RuntimeError("Boot manifest omitted the EBNF/JavaScript/TypeScript assembly resource.")
    for group in ("jsModuleNative", "jsModuleRuntime"):
        for item in boot["resources"].get(group, []):
            paths.add("_framework/" + item["name"])

    SERVED.mkdir(parents=True, exist_ok=True)
    checks = []
    for relative in sorted(paths):
        request_path = "" if relative == "index.html" else relative
        url = BASE + request_path
        request = urllib.request.Request(url, headers={"Accept-Encoding": "identity", "Cache-Control": "no-cache", "User-Agent": "Janglim-release-verification/0.6.0"})
        try:
            with urllib.request.urlopen(request, timeout=60) as response:
                content = response.read()
                encoding = response.headers.get("Content-Encoding", "").lower()
                if encoding == "gzip":
                    content = gzip.decompress(content)
                elif encoding not in ("", "identity"):
                    raise RuntimeError(f"Unexpected content encoding: {encoding}")
                headers = {key: response.headers.get(key) for key in ("Content-Type", "Content-Encoding", "ETag", "Last-Modified", "Cache-Control", "Age")}
                local = (ARTIFACT / relative).read_bytes()
                target = SERVED / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(content)
                check = {"path": relative, "url": response.url, "status": response.status,
                         "artifactSha256": sha256(local), "servedSha256": sha256(content),
                         "bytes": len(content), "match": content == local, "headers": headers}
        except Exception as error:
            check = {"path": relative, "url": url, "match": False, "error": str(error)}
        checks.append(check)
        print(json.dumps({key: check[key] for key in ("path", "match")}, ensure_ascii=False), flush=True)

    report = {"checkedUtc": datetime.now(timezone.utc).isoformat(), "baseUrl": BASE,
              "releaseVersion": "0.6.0", "playgroundVersion": "1.1.0", "commit": COMMIT,
              "workflow": workflow, "artifactName": ARTIFACT.name,
              "bootResourcesHash": boot["resources"].get("hash"), "engineResources": engine_resources,
              "checks": checks, "allMatch": all(check["match"] for check in checks)}
    report_path = ROOT / "playground-deployment-verification.json"
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"report": str(report_path), "allMatch": report["allMatch"], "count": len(checks)}, ensure_ascii=False))
    return 0 if report["allMatch"] else 1


if __name__ == "__main__":
    raise SystemExit(run())
