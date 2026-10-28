import hashlib
import io
import json
import pathlib
import shutil
import subprocess
import tarfile
import tempfile
import unittest
from unittest import mock

import prepare_oracles


class PreparationTests(unittest.TestCase):
    def test_valid_cached_archive_is_reused_without_network(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = pathlib.Path(directory)
            (folder / "archive").write_bytes(b"pinned")
            metadata = {"size": 6, "sha256": hashlib.sha256(b"pinned").hexdigest(), "url": "https://invalid"}
            with mock.patch("urllib.request.urlopen", side_effect=AssertionError("network used")):
                self.assertEqual(prepare_oracles.ensure_archive(folder, "archive", metadata).read_bytes(), b"pinned")

    def test_mismatched_cached_archive_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = pathlib.Path(directory)
            (folder / "archive").write_bytes(b"changed")
            with self.assertRaisesRegex(ValueError, "Cached archive"):
                prepare_oracles.ensure_archive(folder, "archive", {"size": 7, "sha256": "wrong"})

    def test_mismatched_download_is_not_retained(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = pathlib.Path(directory)
            with mock.patch("urllib.request.urlopen", return_value=io.BytesIO(b"changed")):
                with self.assertRaisesRegex(ValueError, "Downloaded archive"):
                    prepare_oracles.ensure_archive(folder, "archive", {"size": 7, "sha256": "wrong", "url": "https://example.invalid"})
            self.assertFalse((folder / "archive").exists())

    def make_tar(self, path, files):
        with tarfile.open(path, "w:gz") as archive:
            for name, data in files.items():
                info = tarfile.TarInfo(name)
                info.size = len(data)
                archive.addfile(info, io.BytesIO(data))

    def test_source_extraction_keeps_license_and_skips_testdata(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = pathlib.Path(directory)
            self.make_tar(folder / "source.tgz", {
                "source/go.mod": b"module example", "source/LICENSE": b"upstream",
                "source/internal/parser/parser.go": b"parser", "source/testdata/baselines/reference/huge": b"baseline"
            })
            root = prepare_oracles.extract_source(folder / "source.tgz", folder / "out")
            self.assertEqual((root / "LICENSE").read_bytes(), b"upstream")
            self.assertTrue((root / "internal/parser/parser.go").exists())
            self.assertFalse((root / "testdata").exists())

    def test_source_extraction_blocks_parent_traversal(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = pathlib.Path(directory)
            self.make_tar(folder / "source.tgz", {"../../escape": b"bad"})
            with self.assertRaises(tarfile.FilterError):
                prepare_oracles.extract_source(folder / "source.tgz", folder / "out")

    def test_source_extraction_rejects_multiple_module_roots(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = pathlib.Path(directory)
            self.make_tar(folder / "source.tgz", {"one/go.mod": b"one", "two/go.mod": b"two"})
            with self.assertRaisesRegex(ValueError, "exactly one"):
                prepare_oracles.extract_source(folder / "source.tgz", folder / "out")


ARTIFACTS = pathlib.Path(__file__).resolve().parents[2] / "artifacts/typescript"
NATIVE = ARTIFACTS / "typescript-7.0.2-parse-oracle.exe"
API6 = ARTIFACTS / "typescript6/package/lib/typescript.js"


@unittest.skipUnless(NATIVE.exists(), "prepare the pinned native oracle to run integration checks")
class NativeOracleTests(unittest.TestCase):
    def evaluate(self, cases, command=None):
        process = subprocess.run(command or [str(NATIVE)], input="".join(json.dumps(c) + "\n" for c in cases),
                                 text=True, capture_output=True, check=True)
        return [json.loads(line) for line in process.stdout.splitlines()]

    def test_parse_failure_is_separate_from_grammar_and_type_errors(self):
        results = self.evaluate([
            {"source": "let count: = 1;"},
            {"source": "function f(x?: string, y: number) {}"},
            {"source": "let name: string = 1;"},
        ])
        self.assertEqual([r["accept"] for r in results], [False, True, True])
        self.assertTrue(all(r["phase"] == "parse" for r in results))

    def test_tsx_and_ambient_input_contexts_are_preserved(self):
        results = self.evaluate([
            {"kind": "tsx", "source": "const e = <div />;"},
            {"kind": "ts", "source": "const e = <div />;"},
            {"kind": "dts", "source": "declare namespace N { const x: number; }"},
        ])
        self.assertEqual([r["accept"] for r in results], [True, False, True])
        self.assertEqual(results[-1]["fileName"], "/input.d.ts")

    def test_utf8_source_identity_and_diagnostic_offsets(self):
        source = "let 이름: = 1;"
        result = self.evaluate([{"source": source}])[0]
        self.assertEqual(result["sourceSha256"], hashlib.sha256(source.encode()).hexdigest())
        self.assertEqual(result["offsetUnit"], "UTF-8 bytes")
        self.assertEqual(result["diagnostics"][0]["start"], len("let 이름: ".encode()))

    def test_prerelease_source_phase_import_is_not_accepted(self):
        result = self.evaluate([{"source": 'import source M from "./m.wasm";'}])[0]
        self.assertFalse(result["accept"])

    @unittest.skipUnless(API6.exists() and shutil.which("node"), "prepare the comparison API")
    def test_older_api_cannot_substitute_for_native_baseline(self):
        cases = [{"source": "let f: function(string): void;"}]
        native = self.evaluate(cases)[0]
        older = self.evaluate(cases, ["node", str(pathlib.Path(__file__).with_name("typescript6-parse-oracle.cjs")), str(API6)])[0]
        self.assertFalse(native["accept"])
        self.assertTrue(older["accept"])


if __name__ == "__main__":
    unittest.main()
