"""The coordinator must never turn missing, stale or crashed work into a pass."""
from __future__ import annotations

import argparse
import contextlib
import importlib.util
import io
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch


SPEC = importlib.util.spec_from_file_location("run_test262", Path(__file__).with_name("run_test262.py"))
run = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(run)


def case(number=1):
    return {"id": str(number), "path": "language/case.js", "variant": "strict",
            "inputKind": "Script", "mode": "Strict", "webCompatibility": True,
            "expectAccept": True, "source": "x;", "sourceSha256": "hash-" + str(number)}


def result(item, outcome="pass"):
    return {key: item[key] for key in ("id", "path", "variant", "inputKind", "mode", "sourceSha256")} | {
        "expectedAccept": item["expectAccept"], "accepted": outcome == "pass",
        "outcome": outcome, "stage": "accepted" if outcome == "pass" else outcome}


class ResumeTests(unittest.TestCase):
    def test_identical_cases_are_resumable(self):
        item = case()
        run.validate_resume([item], [result(item)])

    def test_results_cannot_exceed_input_count(self):
        with self.assertRaisesRegex(ValueError, "exceeds"):
            run.validate_resume([], [result(case())])

    def test_changed_case_identity_is_rejected(self):
        for field, value in (("id", "other"), ("sourceSha256", "other"),
                             ("expectAccept", False), ("inputKind", "Module"),
                             ("mode", "NonStrict"), ("variant", "raw")):
            with self.subTest(field=field), self.assertRaisesRegex(ValueError, "differs"):
                run.validate_resume([case() | {field: value}], [result(case())])

    def test_hashless_results_cannot_authorize_resume(self):
        for left, right in ((case() | {"sourceSha256": None}, result(case())),
                            (case(), result(case()) | {"sourceSha256": None})):
            with self.subTest(left=left), self.assertRaisesRegex(ValueError, "source hash"):
                run.validate_resume([left], [right])

    def test_profile_mapping(self):
        self.assertEqual(run.profile_for(case()), "script-strict")
        self.assertEqual(run.profile_for(case() | {"mode": "NonStrict"}), "script-nonstrict")
        self.assertEqual(run.profile_for(case() | {"inputKind": "Module"}), "module")


class FileTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.output = self.root / "results"
        self.output.mkdir()
        self.binary = self.root / "bin"
        self.binary.mkdir()
        self.runner = self.binary / "runner.dll"
        self.runner.write_bytes(b"runner v1")
        (self.binary / "Janglim.dll").write_bytes(b"product v1")
        (self.binary / "runner.deps.json").write_text("{}", encoding="utf-8")
        self.input = self.root / "cases.jsonl"
        self.input.write_text(json.dumps(case()) + "\n", encoding="utf-8")
        self.options = argparse.Namespace(input=self.input, output_dir=self.output,
            dotnet=Path("dotnet"), runner=self.runner, timeout_ms=1000, max_restarts=2, resume=False)

    def prepare_manifest(self):
        return run.validate_manifest(self.options, ["script-strict"])

    def write_results(self, rows, append=False):
        with (self.output / "script-strict.jsonl").open("a" if append else "w", encoding="utf-8") as stream:
            for row in rows:
                stream.write(json.dumps(row) + "\n")

    def test_manifest_tracks_runner_and_all_product_dependencies(self):
        manifest = self.prepare_manifest()
        self.assertEqual(set(manifest["binarySha256"]), {"runner.dll", "Janglim.dll", "runner.deps.json"})
        self.options.resume = True
        self.assertEqual(self.prepare_manifest(), manifest)

    def test_resume_rejects_changed_product_dependency(self):
        self.prepare_manifest()
        self.options.resume = True
        (self.binary / "Janglim.dll").write_bytes(b"product v2")
        with self.assertRaisesRegex(ValueError, "changed"):
            self.prepare_manifest()

    def test_resume_rejects_added_dependency(self):
        self.prepare_manifest()
        self.options.resume = True
        (self.binary / "new.dll").write_bytes(b"dependency")
        with self.assertRaisesRegex(ValueError, "changed"):
            self.prepare_manifest()

    def test_resume_rejects_changed_runner(self):
        self.prepare_manifest()
        self.options.resume = True
        self.runner.write_bytes(b"runner v2")
        with self.assertRaisesRegex(ValueError, "changed"):
            self.prepare_manifest()

    def test_resume_rejects_changed_input_options_or_profile_selection(self):
        self.prepare_manifest()
        self.options.resume = True
        self.input.write_text(json.dumps(case() | {"webCompatibility": False}), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "changed"):
            self.prepare_manifest()
        self.input.write_text(json.dumps(case()) + "\n", encoding="utf-8")
        self.options.timeout_ms += 1
        with self.assertRaisesRegex(ValueError, "changed"):
            self.prepare_manifest()
        self.options.timeout_ms -= 1
        with self.assertRaisesRegex(ValueError, "changed"):
            run.validate_manifest(self.options, ["module"])

    def test_missing_manifest_cannot_authorize_resume(self):
        self.options.resume = True
        with self.assertRaisesRegex(ValueError, "original"):
            self.prepare_manifest()

    def test_existing_results_are_preserved_without_resume(self):
        self.write_results([result(case())])
        with self.assertRaisesRegex(ValueError, "Existing results"):
            self.prepare_manifest()
        self.assertEqual(len(run.read_rows(self.output / "script-strict.jsonl")), 1)

    def test_empty_profile_is_explicitly_skipped_without_running_child(self):
        with patch.object(run.subprocess, "run") as child:
            summary = run.run_profile("module", [], self.options)
        self.assertEqual(summary["status"], "skipped")
        self.assertEqual(summary["reason"], "no-selected-cases")
        self.assertIsNone(summary["complete"])
        child.assert_not_called()

    def test_completed_profile_has_exact_case_counts(self):
        def child(*args, **kwargs):
            self.write_results([result(case())])
            return subprocess.CompletedProcess(args[0], 0)
        with patch.object(run.subprocess, "run", side_effect=child):
            summary = run.run_profile("script-strict", [case()], self.options)
        self.assertEqual(summary["outcomes"], {"pass": 1})
        self.assertTrue(summary["complete"])

    def test_crash_after_last_result_cannot_pass(self):
        def child(*args, **kwargs):
            self.write_results([result(case())])
            return subprocess.CompletedProcess(args[0], 137)
        with patch.object(run.subprocess, "run", side_effect=child), self.assertRaisesRegex(RuntimeError, "crashed"):
            run.run_profile("script-strict", [case()], self.options)

    def test_failure_exit_with_all_pass_results_cannot_pass(self):
        def child(*args, **kwargs):
            self.write_results([result(case())])
            return subprocess.CompletedProcess(args[0], 1)
        with patch.object(run.subprocess, "run", side_effect=child), self.assertRaisesRegex(RuntimeError, "all-pass"):
            run.run_profile("script-strict", [case()], self.options)

    def test_recorded_negative_mismatch_retains_failure_outcome(self):
        def child(*args, **kwargs):
            self.write_results([result(case(), "unexpected-reject")])
            return subprocess.CompletedProcess(args[0], 1)
        with patch.object(run.subprocess, "run", side_effect=child):
            summary = run.run_profile("script-strict", [case()], self.options)
        self.assertEqual(summary["outcomes"], {"unexpected-reject": 1})

    def test_timeout_is_retained_and_next_case_runs_in_fresh_child(self):
        calls = []
        def child(command, **kwargs):
            calls.append(command)
            if len(calls) == 1:
                self.write_results([result(case(), "timeout")])
                return subprocess.CompletedProcess(command, 124)
            self.write_results([result(case(2))], append=True)
            return subprocess.CompletedProcess(command, 0)
        with patch.object(run.subprocess, "run", side_effect=child):
            summary = run.run_profile("script-strict", [case(), case(2)], self.options)
        self.assertEqual(summary["outcomes"], {"timeout": 1, "pass": 1})
        self.assertEqual(summary["restarts"], 1)
        self.assertIn("--append", calls[1])
        self.assertEqual(calls[1][calls[1].index("--skip") + 1], "1")

    def test_process_crash_records_case_as_failure_and_continues(self):
        calls = []
        def child(command, **kwargs):
            calls.append(command)
            if len(calls) == 1:
                return subprocess.CompletedProcess(command, 137)
            self.write_results([result(case(2))], append=True)
            return subprocess.CompletedProcess(command, 0)
        with patch.object(run.subprocess, "run", side_effect=child):
            summary = run.run_profile("script-strict", [case(), case(2)], self.options)
        self.assertEqual(summary["outcomes"], {"crash": 1, "pass": 1})

    def test_unrecorded_timeout_cannot_skip_a_case(self):
        def child(command, **kwargs):
            self.write_results([result(case())])
            return subprocess.CompletedProcess(command, 124)
        with patch.object(run.subprocess, "run", side_effect=child), self.assertRaisesRegex(RuntimeError, "no recorded timeout"):
            run.run_profile("script-strict", [case(), case(2)], self.options)

    def test_premature_success_exit_remains_incomplete(self):
        def child(command, **kwargs):
            self.write_results([result(case())])
            return subprocess.CompletedProcess(command, 0)
        with patch.object(run.subprocess, "run", side_effect=child), self.assertRaisesRegex(RuntimeError, "before all cases"):
            run.run_profile("script-strict", [case(), case(2)], self.options)

    def test_restart_limit_prevents_unbounded_restarts(self):
        self.options.max_restarts = 0
        def child(command, **kwargs):
            self.write_results([result(case(), "timeout")])
            return subprocess.CompletedProcess(command, 124)
        with patch.object(run.subprocess, "run", side_effect=child), self.assertRaisesRegex(RuntimeError, "restart limit"):
            run.run_profile("script-strict", [case(), case(2)], self.options)

    def test_duplicate_cli_profiles_are_rejected(self):
        argv = ["run", "--input", str(self.input), "--output-dir", str(self.output),
                "--runner", str(self.runner), "--profile", "module", "--profile", "module"]
        with patch.object(sys, "argv", argv), self.assertRaisesRegex(ValueError, "only once"):
            run.main()

    def test_all_empty_profiles_produce_nonzero_exit(self):
        self.input.write_text("", encoding="utf-8")
        argv = ["run", "--input", str(self.input), "--output-dir", str(self.output), "--runner", str(self.runner)]
        with patch.object(sys, "argv", argv), contextlib.redirect_stdout(io.StringIO()):
            status = run.main()
        self.assertEqual(status, 1)
        summary = json.loads((self.output / "summary.json").read_text(encoding="utf-8"))
        self.assertFalse(summary["complete"])
        self.assertTrue(all(p["status"] == "skipped" for p in summary["profiles"]))


if __name__ == "__main__":
    unittest.main()
