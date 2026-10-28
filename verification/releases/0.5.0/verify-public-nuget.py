"""Read-only remote NuGet checks against the merge commit and verified CI artifact."""
from pathlib import Path
from urllib.request import Request, urlopen
import datetime
import hashlib
import json
import subprocess
import sys
import zipfile
import io
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
COMMIT = "1b8f85f13c063a855358a1d5bac3769fec2f84e0"
VERSION = "0.5.0"
BASE = "https://api.nuget.org/v3-flatcontainer/janglim/"


def fetch(url):
    request = Request(url, headers={"User-Agent": "Janglim-release-verification", "Cache-Control": "no-cache", "Accept-Encoding": "identity"})
    with urlopen(request, timeout=40) as response:
        assert response.status == 200
        return response.read()


def sha(data):
    return hashlib.sha256(data).hexdigest()


def blob(path):
    return subprocess.check_output(["git", "show", COMMIT + ":" + path], cwd=ROOT)


def metadata(data):
    node = next(item for item in ET.fromstring(data) if item.tag.endswith("metadata"))
    return {item.tag.rsplit("}", 1)[-1]: {"text": item.text, "attributes": item.attrib} for item in node}


index_bytes = fetch(BASE + "index.json")
index = json.loads(index_bytes)
assert VERSION in index["versions"], "Published version missing from the official NuGet index."
nuspec_bytes = fetch(BASE + VERSION + "/janglim.nuspec")
package_bytes = fetch(BASE + VERSION + "/janglim." + VERSION + ".nupkg")
download = OUT / "public-nuget"
download.mkdir(exist_ok=True)
(download / "index.json").write_bytes(index_bytes)
(download / "Janglim.0.5.0.nuspec").write_bytes(nuspec_bytes)
(download / "Janglim.0.5.0.nupkg").write_bytes(package_bytes)
spec = metadata(nuspec_bytes)
assert spec["id"]["text"] == "Janglim"
assert spec["version"]["text"] == VERSION
assert spec["repository"]["attributes"]["commit"] == COMMIT
expected_dlls = {
    "AJ.Common.dll", "Janglim.dll", "Parse.dll", "Parse.FrontEnd.dll",
    "Parse.FrontEnd.ErrorHandler.dll", "Parse.FrontEnd.Grammars.dll",
    "Parse.FrontEnd.Grammars.Ebnf.dll", "Parse.FrontEnd.Parsers.dll", "Parse.FrontEnd.Support.dll",
}
ci_package = Path(sys.argv[1]) if len(sys.argv) > 1 else OUT / "ci-nuget" / "Janglim.0.5.0.nupkg"
ci_digest = ci_package.with_name("SHA256SUMS.txt")
assert ci_digest.exists(), "The CI package checksum manifest is missing."
expected_ci_sha = next(line.split()[0] for line in ci_digest.read_text().splitlines() if "Janglim.0.5.0.nupkg" in line)
assert sha(ci_package.read_bytes()) == expected_ci_sha, "Downloaded CI artifact checksum mismatch."

with zipfile.ZipFile(io.BytesIO(package_bytes)) as remote, zipfile.ZipFile(ci_package) as ci:
    remote_entries = {item.filename: remote.read(item.filename) for item in remote.infolist() if not item.is_dir()}
    ci_entries = {item.filename: ci.read(item.filename) for item in ci.infolist() if not item.is_dir()}
    embedded_spec = metadata(remote_entries["Janglim.nuspec"])
    assert embedded_spec == spec, "NuGet nuspec endpoint and embedded package nuspec disagree."
    readme = remote_entries["README.md"]
    notice = remote_entries["licenses/Unicode-LICENSE.txt"]
    assert readme == blob("src/Janglim/README.md"), "Published README differs from the merge commit Git blob."
    assert notice == blob("grammars/javascript/unicode/LICENSE.txt"), "Published Unicode notice differs from the merge commit Git blob."
    main_dlls = sorted(name.rsplit("/", 1)[-1] for name in remote_entries if name.startswith("lib/netstandard2.1/") and name.endswith(".dll") and name.count("/") == 2)
    assert set(main_dlls) == expected_dlls
    satellites = sorted(name for name in remote_entries if name.endswith(".resources.dll"))
    ci_satellites = sorted(name for name in ci_entries if name.endswith(".resources.dll"))
    assert satellites == ci_satellites and len(satellites) == 40
    missing = sorted(set(ci_entries) - set(remote_entries))
    added = sorted(set(remote_entries) - set(ci_entries))
    changed = sorted(name for name in set(ci_entries) & set(remote_entries) if ci_entries[name] != remote_entries[name])
    assert not missing and not changed and set(added) <= {".signature.p7s"}, {"missing": missing, "added": added, "changed": changed}
    payloads = [{"entry": name, "bytes": len(value), "sha256": sha(value)} for name, value in sorted(ci_entries.items())]

report = {
    "verifiedAtUtc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
    "sourceCommit": COMMIT,
    "nugetIndexContainsVersion": True,
    "packageId": spec["id"]["text"], "version": spec["version"]["text"],
    "repositoryCommit": spec["repository"]["attributes"]["commit"],
    "nuspecEndpointMatchesEmbeddedNuspec": True,
    "readmeMatchesMergeCommitBlob": True,
    "readmeBytes": len(readme), "readmeSha256": sha(readme),
    "unicodeNoticeMatchesMergeCommitBlob": True,
    "unicodeNoticeBytes": len(notice), "unicodeNoticeSha256": sha(notice),
    "engineDllNames": main_dlls, "satelliteDllCount": len(satellites), "satelliteDllEntries": satellites,
    "ciPackageSha256": sha(ci_package.read_bytes()), "ciChecksumManifestVerified": True,
    "remotePackageSha256": sha(package_bytes),
    "payloadComparison": {"ciEntryCount": len(ci_entries), "remoteEntryCount": len(remote_entries), "missingEntries": missing, "addedEntries": added, "changedPayloadEntries": changed, "allOriginalPayloadsIdentical": True},
    "comparedPayloads": payloads,
    "allPassed": True,
}
(OUT / "public-nuget-verification.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps({key: value for key, value in report.items() if key not in {"comparedPayloads", "satelliteDllEntries"}}, ensure_ascii=False, indent=2))
