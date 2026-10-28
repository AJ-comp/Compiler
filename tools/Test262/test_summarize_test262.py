"""A compact conformance report must be justified by every raw result."""

import copy
from collections import Counter
import contextlib
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch


SPEC = importlib.util.spec_from_file_location("summarize_test262", Path(__file__).with_name("summarize_test262.py"))
summarize = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(summarize)


def test_case(name, *, mode="Strict", kind="Script", phase=None):
    source = ("// " + name + "\r\n") + ("let = ;" if phase == "parse" else "x();")
    classification = {None: "positive", "parse": "parse-negative", "runtime": "runtime-negative-source-only", "resolution": "resolution-negative-source-only"}[phase]
    variant = "module" if kind == "Module" else "strict" if mode == "Strict" else "non-strict"
    return {"id": "language/" + name + ".js#" + variant, "path": "language/" + name + ".js",
            "source": source, "sourceSha256": hashlib.sha256(source.encode()).hexdigest(),
            "inputKind": kind, "mode": mode, "variant": variant, "webCompatibility": True,
            "expectAccept": phase != "parse", "negativePhase": phase,
            "negativeType": "SyntaxError" if phase in {"parse", "resolution"} else "TypeError" if phase == "runtime" else None,
            "classification": classification}


def result(case):
    return {key: case[key] for key in ("id", "path", "sourceSha256", "inputKind", "mode", "variant")} | {
        "expectedAccept": case["expectAccept"], "accepted": case["expectAccept"],
        "outcome": "pass", "stage": "accepted" if case["expectAccept"] else "early-error",
        "diagnostics": [] if case["expectAccept"] else ["syntax_error@0+1: invalid source"], "elapsedMs": 1.0}


class EvidenceTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
        self.input = self.root / "cases.jsonl"
        self.input_manifest = self.input.with_suffix(".manifest.json")
        self.run_dir = self.root / "results"
        self.run_dir.mkdir()
        self.output = self.root / "report.json"
        self.binary_dir = self.root / "bin"
        self.binary_dir.mkdir()
        (self.binary_dir / "runner.dll").write_bytes(b"exact runner binary")
        (self.binary_dir / "product.dll").write_bytes(b"exact JavaScript implementation")
        (self.binary_dir / "runner.deps.json").write_bytes(b"{}\n")
        self.cases = [test_case("positive"), test_case("invalid", phase="parse"),
                      test_case("sloppy", mode="NonStrict"),
                      test_case("runtime", mode="Strict", kind="Module", phase="runtime"),
                      test_case("resolution", mode="Strict", kind="Module", phase="resolution")]
        self.results = {profile: [result(case) for case in self.cases if summarize.profile_for(case) == profile] for profile in summarize.PROFILES}
        self.save()

    def tearDown(self):
        self.temporary.cleanup()

    def write_json(self, path, value):
        path.write_text(json.dumps(value) + "\n", encoding="utf-8")

    def write_rows(self, path, records):
        path.write_text("".join(json.dumps(row) + "\n" for row in records), encoding="utf-8")

    def change_json(self, path, change):
        value = json.loads(path.read_text(encoding="utf-8"))
        change(value)
        self.write_json(path, value)

    def save(self):
        self.write_rows(self.input, self.cases)
        source_hash = summarize.file_sha256(self.input)
        self.write_json(self.input_manifest, {
            "schemaVersion": 1, "casesSha256": source_hash,
            "snapshot": {"commit": "d" * 40, "commitDate": "2026-10-13T22:44:04Z", "archiveSha256": "a" * 64},
            "includePrefixes": ["language", "annexB", "built-ins/RegExp"],
            "counts": {"emittedVariants": len(self.cases), "plannedVariants": len(self.cases),
                       "selectedFiles": len({case["path"] for case in self.cases}),
                       "expectedAccept": sum(case["expectAccept"] for case in self.cases),
                       "expectedReject": sum(not case["expectAccept"] for case in self.cases)},
            "variantCounts": dict(Counter(case["variant"] for case in self.cases)),
        })
        binaries = {path.relative_to(self.binary_dir).as_posix(): summarize.file_sha256(path) for path in self.binary_dir.rglob("*") if path.is_file()}
        self.write_json(self.run_dir / "run-manifest.json", {
            "schemaVersion": 1, "inputSha256": source_hash, "runner": "runner.dll",
            "binarySha256": binaries, "profiles": sorted(summarize.PROFILES), "timeoutMs": 1000})
        summaries = []
        total = Counter()
        for profile in summarize.PROFILES:
            self.write_rows(self.run_dir / (profile + ".jsonl"), self.results[profile])
            expected = sum(summarize.profile_for(case) == profile for case in self.cases)
            outcomes = dict(Counter(row["outcome"] for row in self.results[profile]))
            total.update(outcomes)
            summaries.append({"profile": profile, "expected": expected, "processed": len(self.results[profile]),
                              "outcomes": outcomes, "complete": True if expected else None,
                              "status": "complete" if expected else "skipped",
                              **({"reason": "no-selected-cases"} if not expected else {})})
        self.write_json(self.run_dir / "summary.json", {
            "schemaVersion": 1, "syntaxOnly": True, "complete": True, "input": str(self.input),
            "inputSha256": source_hash, "runnerSha256": binaries["runner.dll"], "binarySha256": binaries,
            "profiles": summaries, "processed": sum(total.values()), "outcomes": dict(total), "elapsedSeconds": 0.25})

    def run_report(self, **options):
        return summarize.summarize(self.input, self.run_dir, self.output, **options)

    def test_complete_matching_results_produce_source_only_pass(self):
        report = self.run_report()
        self.assertTrue(report["passed"])
        self.assertTrue(report["complete"])
        self.assertEqual(report["counts"]["variants"], 5)
        self.assertEqual(report["counts"]["expectedAccept"], 4)
        self.assertEqual(report["counts"]["expectedReject"], 1)
        self.assertEqual(report["counts"]["classifications"], {"positive": 2, "parse-negative": 1, "runtime-negative-source-only": 1, "resolution-negative-source-only": 1})
        self.assertIn("no JavaScript evaluation", report["scope"])
        self.assertEqual(report["input"]["snapshot"]["commit"], "d" * 40)
        self.assertEqual(report["checks"]["unexpectedExceptions"], 0)
        self.assertFalse(report["run"]["binaryFilesVerified"])

    def test_raw_evidence_and_binary_hashes_are_retained(self):
        report = self.run_report(binary_dir=self.binary_dir)
        self.assertEqual(report["input"]["sha256"], summarize.file_sha256(self.input))
        self.assertEqual(report["input"]["manifestSha256"], summarize.file_sha256(self.input_manifest))
        self.assertEqual(report["run"]["manifestSha256"], summarize.file_sha256(self.run_dir / "run-manifest.json"))
        self.assertEqual(report["run"]["summarySha256"], summarize.file_sha256(self.run_dir / "summary.json"))
        self.assertEqual(report["rawResults"]["module"]["sha256"], summarize.file_sha256(self.run_dir / "module.jsonl"))
        self.assertTrue(report["run"]["binaryFilesVerified"])

    def test_duplicate_input_id_is_rejected_even_with_different_source_hash(self):
        duplicate = copy.deepcopy(self.cases[0])
        duplicate["source"] = "different source"
        duplicate["sourceSha256"] = hashlib.sha256(duplicate["source"].encode()).hexdigest()
        self.cases.append(duplicate)
        self.save()
        with self.assertRaisesRegex(ValueError, "duplicate input ID"):
            self.run_report()

    def test_duplicate_result_id_is_rejected_even_with_different_hash(self):
        duplicate = copy.deepcopy(self.results["script-strict"][0])
        duplicate["sourceSha256"] = "b" * 64
        self.results["script-strict"].append(duplicate)
        self.save()
        with self.assertRaisesRegex(ValueError, "duplicate result ID"):
            self.run_report()

    def test_missing_result_is_rejected_despite_complete_summary(self):
        self.results["module"].pop()
        self.save()
        with self.assertRaisesRegex(ValueError, "missing result IDs"):
            self.run_report()

    def test_unknown_result_id_is_rejected(self):
        self.results["module"][0]["id"] = "unknown-id"
        self.save()
        with self.assertRaisesRegex(ValueError, "unknown result ID"):
            self.run_report()

    def test_result_identity_expectation_and_path_are_cross_checked(self):
        original = copy.deepcopy(self.results)
        changes = {"sourceSha256": "b" * 64, "variant": "raw", "inputKind": "Module", "mode": "NonStrict", "path": "wrong/path.js", "expectedAccept": False, "webCompatibility": False}
        for field, value in changes.items():
            with self.subTest(field=field):
                self.results = copy.deepcopy(original)
                self.results["script-strict"][0][field] = value
                self.save()
                with self.assertRaises(ValueError):
                    self.run_report()

    def test_result_cannot_be_moved_to_a_different_profile(self):
        self.results["module"].append(self.results["script-strict"].pop())
        self.save()
        with self.assertRaisesRegex(ValueError, "missing result IDs|wrong profile"):
            self.run_report()

    def test_reordered_complete_results_remain_verifiable_by_id(self):
        self.results["script-strict"].reverse()
        self.save()
        self.assertTrue(self.run_report()["passed"])

    def test_input_digest_must_match_all_three_manifests(self):
        for path, key in ((self.input_manifest, "casesSha256"), (self.run_dir / "run-manifest.json", "inputSha256"), (self.run_dir / "summary.json", "inputSha256")):
            with self.subTest(path=path):
                self.save()
                self.change_json(path, lambda value: value.update({key: "b" * 64}))
                with self.assertRaisesRegex(ValueError, "input digest mismatch"):
                    self.run_report()

    def test_input_source_checksum_is_independently_verified(self):
        self.cases[0]["source"] = "modified but stale source checksum"
        self.save()
        with self.assertRaisesRegex(ValueError, "input source hash mismatch"):
            self.run_report()

    def test_source_expectation_must_follow_metadata_phase(self):
        self.cases[0]["classification"] = "parse-negative"
        self.save()
        with self.assertRaisesRegex(ValueError, "classification mismatch"):
            self.run_report()

    def test_parse_negative_requires_syntax_error(self):
        self.cases[1]["negativeType"] = "TypeError"
        self.save()
        with self.assertRaisesRegex(ValueError, "parse-negative requires SyntaxError"):
            self.run_report()

    def test_runtime_syntax_error_remains_an_expected_source_accept(self):
        self.cases[3]["negativeType"] = "SyntaxError"
        self.save()
        self.assertTrue(self.run_report()["passed"])

    def test_source_manifest_counts_and_variants_must_match_actual_input(self):
        changes = [("counts", "emittedVariants", 0), ("counts", "plannedVariants", 6), ("counts", "selectedFiles", 100),
                   ("counts", "expectedAccept", 0), ("counts", "expectedReject", 0), ("counts", "omittedVariants:limit", 1),
                   ("variantCounts", "module", 0)]
        for section, key, value in changes:
            with self.subTest(key=key):
                self.save()
                self.change_json(self.input_manifest, lambda doc: doc[section].update({key: value}))
                with self.assertRaises(ValueError):
                    self.run_report()

    def test_unpinned_or_modified_source_snapshot_is_rejected(self):
        for snapshot in ({"commit": None}, {"commit": "main"}, {"commit": "d" * 40, "trackedFilesModified": True}):
            with self.subTest(snapshot=snapshot):
                self.save()
                self.change_json(self.input_manifest, lambda doc: doc.update(snapshot=snapshot))
                with self.assertRaises(ValueError):
                    self.run_report()

    def test_all_three_profiles_must_be_declared(self):
        self.change_json(self.run_dir / "run-manifest.json", lambda doc: doc.update(profiles=["script-strict", "module"]))
        with self.assertRaisesRegex(ValueError, "all three profiles"):
            self.run_report()

    def test_summary_requires_distinct_complete_profile_records(self):
        for mutate in (lambda doc: doc["profiles"].pop(), lambda doc: doc["profiles"].__setitem__(1, doc["profiles"][0]),
                       lambda doc: doc["profiles"][0].update(complete=False), lambda doc: doc["profiles"][0].update(status="skipped")):
            self.save()
            self.change_json(self.run_dir / "summary.json", mutate)
            with self.assertRaises(ValueError):
                self.run_report()

    def test_global_summary_must_be_complete_and_source_only(self):
        for field, value in (("complete", False), ("syntaxOnly", False), ("complete", 1)):
            with self.subTest(field=field, value=value):
                self.save()
                self.change_json(self.run_dir / "summary.json", lambda doc: doc.update({field: value}))
                with self.assertRaisesRegex(ValueError, "complete and source-only"):
                    self.run_report()

    def test_summary_outcomes_and_counts_cannot_hide_raw_failures(self):
        for mutate in (lambda doc: doc.update(processed=0), lambda doc: doc.update(outcomes={"pass": 100}),
                       lambda doc: doc["profiles"][0].update(processed=0), lambda doc: doc["profiles"][0].update(outcomes={"pass": 100})):
            self.save()
            self.change_json(self.run_dir / "summary.json", mutate)
            with self.assertRaises(ValueError):
                self.run_report()

    def test_genuine_failures_produce_failed_reports_instead_of_passes(self):
        original = copy.deepcopy(self.results)
        for outcome, stage, accepted, expected_count in (("unexpected-reject", "parse", False, "unexpectedReject"),
                                                         ("error", "error", False, "unexpectedExceptions"),
                                                         ("timeout", "timeout", False, "timeouts"),
                                                         ("crash", "crash", False, "crashes")):
            with self.subTest(outcome=outcome):
                self.results = copy.deepcopy(original)
                self.results["script-strict"][0].update(outcome=outcome, stage=stage, accepted=accepted)
                self.save()
                report = self.run_report()
                self.assertTrue(report["complete"])
                self.assertFalse(report["passed"])
                self.assertEqual(report["checks"][expected_count], 1)

    def test_unexpected_accept_is_counted(self):
        self.results["script-strict"][1].update(outcome="unexpected-accept", stage="accepted", accepted=True, diagnostics=[])
        self.save()
        self.assertEqual(self.run_report()["checks"]["unexpectedAccept"], 1)

    def test_error_timeout_or_crash_cannot_be_disguised_as_pass(self):
        for stage in ("error", "timeout", "crash"):
            with self.subTest(stage=stage):
                self.results["script-strict"][1].update(outcome="pass", stage=stage, accepted=False)
                self.save()
                with self.assertRaisesRegex(ValueError, "cannot be a pass"):
                    self.run_report()

    def test_accepted_value_and_pass_label_must_agree(self):
        self.results["script-strict"][0].update(accepted=False, stage="parse", outcome="pass")
        self.save()
        with self.assertRaisesRegex(ValueError, "contradicts accepted/expected"):
            self.run_report()

    def test_accepted_result_cannot_have_diagnostics_or_exceptions(self):
        for field, value in (("diagnostics", ["early error"]), ("exceptionChain", ["System.Exception: failure"])):
            with self.subTest(field=field):
                self.results["script-strict"][0] = result(self.cases[0]) | {field: value}
                self.save()
                with self.assertRaisesRegex(ValueError, "contradictory accepted"):
                    self.run_report()

    def test_early_error_requires_diagnostics(self):
        self.results["script-strict"][1]["diagnostics"] = []
        self.save()
        with self.assertRaisesRegex(ValueError, "contradictory early-error"):
            self.run_report()

    def test_expected_parser_syntax_exception_is_counted_separately(self):
        self.results["script-strict"][1].update(stage="parse", diagnostics=None,
            exceptionChain=["System.Reflection.TargetInvocationException: wrapper", "Janglim.ParsingException: invalid input"])
        self.save()
        report = self.run_report()
        self.assertTrue(report["passed"])
        self.assertEqual(report["checks"]["syntaxExceptionRejections"], 1)
        self.assertEqual(report["checks"]["unexpectedExceptions"], 0)

    def test_unexpected_exception_cannot_be_marked_as_syntax_rejection(self):
        self.results["script-strict"][1].update(stage="parse", diagnostics=None, exceptionChain=["System.NullReferenceException: broken parser"])
        self.save()
        with self.assertRaisesRegex(ValueError, "unexpected exception marked as syntax"):
            self.run_report()

    def test_unknown_stage_or_outcome_is_rejected(self):
        for field, value in (("stage", "skipped"), ("outcome", "success"), ("outcome", []), ("stage", {})):
            with self.subTest(field=field, value=value):
                self.results["script-strict"][0] = result(self.cases[0])
                self.save()
                self.results["script-strict"][0][field] = value
                self.write_rows(self.run_dir / "script-strict.jsonl", self.results["script-strict"])
                with self.assertRaises(ValueError):
                    self.run_report()

    def test_booleans_cannot_be_replaced_with_truthy_values(self):
        for field in ("accepted", "expectedAccept"):
            with self.subTest(field=field):
                self.results["script-strict"][0] = result(self.cases[0]) | {field: 1}
                self.save()
                with self.assertRaises(ValueError):
                    self.run_report()

    def test_missing_profile_file_is_rejected(self):
        (self.run_dir / "module.jsonl").unlink()
        with self.assertRaisesRegex(ValueError, "missing profile results"):
            self.run_report()

    def test_empty_profile_is_explicitly_skipped(self):
        self.cases = [self.cases[0]]
        self.results = {profile: [result(self.cases[0])] if profile == "script-strict" else [] for profile in summarize.PROFILES}
        self.save()
        (self.run_dir / "module.jsonl").unlink()
        report = self.run_report()
        self.assertTrue(report["passed"])
        self.assertEqual(report["profiles"]["module"]["status"], "skipped")
        self.assertEqual(report["rawResults"]["module"]["reason"], "no-selected-cases")

    def test_nonempty_skipped_profile_is_rejected(self):
        self.cases = [self.cases[0]]
        self.results = {profile: [result(self.cases[0])] if profile == "script-strict" else [] for profile in summarize.PROFILES}
        self.results["module"] = [result(test_case("unselected", kind="Module"))]
        self.save()
        with self.assertRaisesRegex(ValueError, "unknown result ID"):
            self.run_report()

    def test_empty_total_corpus_cannot_pass(self):
        self.cases = []
        self.results = {profile: [] for profile in summarize.PROFILES}
        self.save()
        with self.assertRaisesRegex(ValueError, "input corpus is empty"):
            self.run_report()

    def test_summary_binary_hashes_must_match_run_manifest(self):
        for field, value in (("binarySha256", {}), ("runnerSha256", "b" * 64)):
            with self.subTest(field=field):
                self.save()
                self.change_json(self.run_dir / "summary.json", lambda doc: doc.update({field: value}))
                with self.assertRaisesRegex(ValueError, "binary hashes differ"):
                    self.run_report()

    def test_missing_or_malformed_binary_hashes_are_rejected(self):
        for value in ({}, {"runner.dll": "not-a-sha"}, {"product.dll": "a" * 64}):
            with self.subTest(value=value):
                self.save()
                self.change_json(self.run_dir / "run-manifest.json", lambda doc: doc.update(binarySha256=value))
                with self.assertRaises(ValueError):
                    self.run_report()

    def test_optional_actual_binary_verification_detects_modified_files(self):
        (self.binary_dir / "product.dll").write_bytes(b"different implementation")
        with self.assertRaisesRegex(ValueError, "recorded binary hash mismatch"):
            self.run_report(binary_dir=self.binary_dir)

    def test_binary_verification_handles_windows_resource_paths(self):
        folder = self.binary_dir / "ko-KR"
        folder.mkdir()
        (folder / "resource.dll").write_bytes(b"localized resource")
        self.save()
        for path in (self.run_dir / "run-manifest.json", self.run_dir / "summary.json"):
            self.change_json(path, lambda doc: doc["binarySha256"].update({"ko-KR\\resource.dll": doc["binarySha256"].pop("ko-KR/resource.dll")}))
        self.assertTrue(self.run_report(binary_dir=self.binary_dir)["passed"])

    def test_binary_verification_rejects_escaping_paths(self):
        for name in ("../outside.dll", "C:/outside.dll", "/outside.dll", "..\\outside.dll"):
            with self.subTest(name=name), self.assertRaisesRegex(ValueError, "unsafe binary manifest path"):
                summarize.verify_binaries(self.binary_dir, {name: "a" * 64})

    def test_empty_failure_sidecars_are_hashed(self):
        path = self.run_dir / "script-strict.jsonl.failures.jsonl"
        path.write_bytes(b"")
        report = self.run_report()
        self.assertEqual(report["rawResults"]["script-strict"]["failureSidecar"]["records"], 0)
        self.assertEqual(report["rawResults"]["script-strict"]["failureSidecar"]["sha256"], hashlib.sha256(b"").hexdigest())

    def test_failure_sidecar_cannot_hide_stale_failure_evidence(self):
        failure = {"test": self.cases[0], "result": self.results["script-strict"][0]}
        self.write_rows(self.run_dir / "script-strict.jsonl.failures.jsonl", [failure])
        with self.assertRaisesRegex(ValueError, "stale or duplicate failure"):
            self.run_report()

    def test_consistent_failure_sidecar_is_verified(self):
        self.results["script-strict"][0].update(accepted=False, stage="parse", outcome="unexpected-reject")
        self.save()
        failure = {"test": self.cases[0], "result": self.results["script-strict"][0]}
        self.write_rows(self.run_dir / "script-strict.jsonl.failures.jsonl", [failure])
        self.assertFalse(self.run_report()["passed"])

    def test_missing_failure_sidecar_entries_are_rejected(self):
        self.results["script-strict"][0].update(accepted=False, stage="parse", outcome="unexpected-reject")
        self.save()
        self.write_rows(self.run_dir / "script-strict.jsonl.failures.jsonl", [])
        with self.assertRaisesRegex(ValueError, "sidecar is missing failed results"):
            self.run_report()

    def test_failure_sidecar_source_text_is_independently_hashed(self):
        self.results["script-strict"][0].update(accepted=False, stage="parse", outcome="unexpected-reject")
        self.save()
        original = copy.deepcopy(self.cases[0])
        original["source"] = "forged source with the old sourceSha256"
        self.write_rows(self.run_dir / "script-strict.jsonl.failures.jsonl", [{"test": original, "result": self.results["script-strict"][0]}])
        with self.assertRaisesRegex(ValueError, "sidecar source hash mismatch"):
            self.run_report()

    def test_report_cannot_overwrite_original_evidence_or_binaries(self):
        for output in (self.input, self.input_manifest, self.run_dir / "summary.json", self.run_dir / "module.jsonl", self.binary_dir / "product.dll"):
            with self.subTest(output=output), self.assertRaisesRegex(ValueError, "must not overwrite"):
                summarize.summarize(self.input, self.run_dir, output, binary_dir=self.binary_dir)

    def test_invalid_evidence_does_not_replace_a_previous_report(self):
        self.output.write_bytes(b"previous verified report")
        self.change_json(self.run_dir / "summary.json", lambda doc: doc.update(complete=False))
        with self.assertRaises(ValueError):
            self.run_report()
        self.assertEqual(self.output.read_bytes(), b"previous verified report")

    def test_unsupported_schema_is_rejected(self):
        for path in (self.input_manifest, self.run_dir / "run-manifest.json", self.run_dir / "summary.json"):
            with self.subTest(path=path):
                self.save()
                self.change_json(path, lambda doc: doc.update(schemaVersion=2))
                with self.assertRaisesRegex(ValueError, "unsupported schema"):
                    self.run_report()

    def test_result_evidence_must_remain_unchanged_during_verification(self):
        original_rows = summarize.rows

        def changing_rows(path):
            yield from original_rows(path)
            if path == self.run_dir / "module.jsonl":
                with path.open("a", encoding="utf-8") as stream:
                    stream.write("\n")

        with patch.object(summarize, "rows", side_effect=changing_rows):
            with self.assertRaisesRegex(ValueError, "evidence changed during verification"):
                self.run_report()

    def test_cli_success_returns_zero_and_writes_verified_report(self):
        with contextlib.redirect_stdout(io.StringIO()):
            status = summarize.main(["--input", str(self.input), "--run-dir", str(self.run_dir),
                                     "--output", str(self.output), "--binary-dir", str(self.binary_dir)])
        self.assertEqual(status, 0)
        self.assertTrue(json.loads(self.output.read_text())["passed"])

    def test_cli_complete_failed_run_returns_one_and_writes_failed_report(self):
        self.results["script-strict"][0].update(accepted=False, stage="timeout", outcome="timeout")
        self.save()
        with contextlib.redirect_stdout(io.StringIO()):
            status = summarize.main(["--input", str(self.input), "--results-dir", str(self.run_dir), "--output", str(self.output)])
        self.assertEqual(status, 1)
        report = json.loads(self.output.read_text())
        self.assertFalse(report["passed"])
        self.assertEqual(report["checks"]["timeouts"], 1)

    def test_cli_incomplete_evidence_returns_two_without_publishing_report(self):
        self.change_json(self.run_dir / "summary.json", lambda doc: doc.update(complete=False))
        with contextlib.redirect_stderr(io.StringIO()), self.assertRaises(SystemExit) as stopped:
            summarize.main(["--input", str(self.input), "--results-dir", str(self.run_dir), "--output", str(self.output)])
        self.assertEqual(stopped.exception.code, 2)
        self.assertFalse(self.output.exists())


if __name__ == "__main__":
    unittest.main()
