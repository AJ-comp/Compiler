#!/usr/bin/env python3
"""Prepare pinned development oracles; no TypeScript dependency enters Janglim."""
from __future__ import annotations

import argparse
import hashlib
import json
import pathlib
import shutil
import subprocess
import tarfile
import urllib.request

HERE = pathlib.Path(__file__).resolve().parent


def sha256(path: pathlib.Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def ensure_archive(output: pathlib.Path, name: str, metadata: dict) -> pathlib.Path:
    path = output / name
    if not path.exists():
        request = urllib.request.Request(metadata["url"], headers={"User-Agent": "Janglim-TypeScript-audit"})
        data = urllib.request.urlopen(request, timeout=180).read()
        # Check the downloaded bytes before they are extracted or retained.
        if len(data) != metadata["size"] or hashlib.sha256(data).hexdigest() != metadata["sha256"]:
            raise ValueError(f"Downloaded archive does not match baseline: {name}")
        path.write_bytes(data)
    if path.stat().st_size != metadata["size"] or sha256(path) != metadata["sha256"]:
        raise ValueError(f"Cached archive does not match baseline: {name}")
    return path


def extract_source(archive: pathlib.Path, destination: pathlib.Path) -> pathlib.Path:
    with tarfile.open(archive) as tar:
        members = [member for member in tar if "/testdata/" not in member.name]
        tar.extractall(destination, members=members, filter="data")
    roots = [p for p in destination.iterdir() if p.is_dir() and (p / "go.mod").is_file()]
    if len(roots) != 1:
        raise ValueError("Expected exactly one pinned Go module")
    return roots[0]


def main() -> int:
    options = argparse.ArgumentParser(description=__doc__)
    options.add_argument("--output", type=pathlib.Path, default=HERE.parents[1] / "artifacts" / "typescript")
    options.add_argument("--go", type=pathlib.Path, help="Go 1.26.5 executable; builds the native parse adapter")
    options.add_argument("--include-corpus", action="store_true", help="Download the official pinned corpus archive without expanding baselines")
    args = options.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    baseline = json.loads((HERE / "baseline.json").read_text(encoding="utf-8"))
    required = ["native-source.tar.gz", "typescript-6.0.3.tgz", "typescript.tgz"]
    if __import__("sys").platform == "win32":
        required.append("typescript-win32-x64.tgz")
    if args.include_corpus:
        required.append("corpus-source.tar.gz")
    archives = {name: ensure_archive(args.output, name, baseline["archives"][name]) for name in required}
    source = extract_source(archives["native-source.tar.gz"], args.output / "source-7.0.2")
    for name, subdir in [("typescript-6.0.3.tgz", "typescript6"), ("typescript.tgz", "native-package"), ("typescript-win32-x64.tgz", "native-win32-x64")]:
        if name in archives:
            with tarfile.open(archives[name]) as tar:
                tar.extractall(args.output / subdir, filter="data")
    report = {
        "stableVersion": baseline["stableVersion"], "nativeRevision": baseline["nativeRevision"],
        "archives": {name: {"sha256": sha256(path), "size": path.stat().st_size} for name, path in archives.items()},
        "adapterSha256": sha256(HERE / "native-parse-oracle.go"),
        "nativeOraclePhase": "parse-only; checker grammar and semantic diagnostics are not combined",
    }
    if args.go:
        version = subprocess.run([str(args.go.resolve()), "version"], check=True, capture_output=True, text=True).stdout.strip()
        if version.split()[2] != "go" + baseline["goVersion"]:
            raise ValueError(f"Expected Go {baseline['goVersion']}; received {version}")
        adapter = source / "cmd" / "janglim-parse-oracle"
        adapter.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(HERE / "native-parse-oracle.go", adapter / "main.go")
        binary = args.output.resolve() / ("typescript-7.0.2-parse-oracle.exe" if __import__("sys").platform == "win32" else "typescript-7.0.2-parse-oracle")
        subprocess.run([str(args.go.resolve()), "-C", str(source.resolve()), "build", "-o", str(binary), "./cmd/janglim-parse-oracle"], check=True)
        report.update({"goVersion": version, "binarySha256": sha256(binary)})
    (args.output / "oracle-provenance.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
