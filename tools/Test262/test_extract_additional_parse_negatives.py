"""Fixture ZIP coverage for the audit outside the main Test262 path selection."""

import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
import warnings
import zipfile


TOOL_DIRECTORY = Path(__file__).resolve().parent
sys.path.insert(0, str(TOOL_DIRECTORY))
try:
    SPEC = importlib.util.spec_from_file_location(
        "extract_additional_parse_negatives", TOOL_DIRECTORY / "extract_additional_parse_negatives.py")
    audit = importlib.util.module_from_spec(SPEC)
    SPEC.loader.exec_module(audit)
finally:
    sys.path.pop(0)


COMMIT = "a" * 40
ZIP_ROOT = "test262-" + COMMIT
NEGATIVE = {"phase": "parse", "type": "SyntaxError"}


def digest(value):
    return hashlib.sha256(value).hexdigest()


def source(metadata=None, body="let = ;\n", newline="\n"):
    value = {"description": "An additional parse-negative fixture", "esid": "sec-test",
             "negative": NEGATIVE}
    if metadata:
        value.update(metadata)
    text = "// Copyright fixture author. All rights reserved.\n/*---\n" + json.dumps(value) + "\n---*/\n" + body
    return text.replace("\n", newline).encode("utf-8")


class AdditionalParseNegativeTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.directory = Path(self.temporary.name)
        self.root = self.directory / "snapshot"
        self.root.mkdir()
        self.archive = self.directory / "snapshot.zip"
        self.output = self.directory / "additional.jsonl"
        self.filter = self.directory / "features.json"
        self.filter.write_text(json.dumps({"excludedFeatures": {"future-syntax": "Not in the frozen edition"}}), encoding="utf-8")

    def tearDown(self):
        self.temporary.cleanup()

    def bundle(self, files, *, root=ZIP_ROOT, extra=()):
        with warnings.catch_warnings():
            warnings.simplefilter("ignore", UserWarning)
            with zipfile.ZipFile(self.archive, "w") as archive:
                archive.writestr(root + "/LICENSE", "Fixture license; original tests retain their notices.\n")
                for path, raw in files:
                    archive.writestr(root + "/test/" + path, raw)
                for path, raw in extra:
                    # zipfile normally rewrites Windows path separators. Keep
                    # the literal member name when exercising unsafe ZIPs.
                    entry = zipfile.ZipInfo(path)
                    entry.filename = path
                    archive.writestr(entry, raw)
        snapshot = {"repository": audit.preparation.REPOSITORY, "commit": COMMIT,
                    "commitDate": "2026-10-13T22:44:04Z", "archiveSha256": digest(self.archive.read_bytes())}
        self.snapshot_file = self.root / "test262.snapshot.json"
        self.snapshot_file.write_text(json.dumps(snapshot), encoding="utf-8")
        return snapshot

    def extract(self, files=None):
        if files is not None:
            self.bundle(files)
        report = audit.extract(self.root, self.archive, self.output, self.filter)
        rows = [json.loads(line) for line in self.output.read_text(encoding="utf-8").splitlines()]
        return report, rows

    def manifest(self):
        return json.loads(self.output.with_suffix(".manifest.json").read_text(encoding="utf-8"))

    def cli(self):
        return subprocess.run([sys.executable, "-X", "utf8", str(TOOL_DIRECTORY / "extract_additional_parse_negatives.py"),
                               "--root", str(self.root), "--archive", str(self.archive),
                               "--output", str(self.output), "--feature-filter", str(self.filter)],
                              capture_output=True, text=True, encoding="utf-8", check=False)

    def test_default_variants_keep_parse_rejection_and_metadata(self):
        raw = source({"features": ["existing-feature"], "includes": ["must-not-be-injected.js"]})
        report, rows = self.extract([("built-ins/Object/example.js", raw)])
        self.assertTrue(report["complete"])
        self.assertEqual([(r["variant"], r["inputKind"], r["mode"]) for r in rows],
                         [("non-strict", "Script", "NonStrict"), ("strict", "Script", "Strict")])
        for row in rows:
            self.assertFalse(row["expectAccept"])
            self.assertTrue(row["webCompatibility"])
            self.assertEqual(row["classification"], "parse-negative")
            self.assertEqual((row["negativePhase"], row["negativeType"]), ("parse", "SyntaxError"))
            self.assertEqual(row["esid"], "sec-test")
            self.assertEqual(row["features"], ["existing-feature"])
            self.assertEqual(row["includes"], ["must-not-be-injected.js"])
            self.assertEqual(row["id"], row["path"] + "#" + row["variant"])
        self.assertEqual(rows[0]["source"], raw.decode("utf-8"))
        self.assertEqual(rows[1]["source"], audit.preparation.STRICT_PREFIX + raw.decode("utf-8"))
        self.assertEqual(report["selectedFiles"][0]["variantCount"], 2)

    def test_no_strict_emits_only_original_non_strict_source(self):
        raw = source({"flags": ["noStrict"]})
        _, rows = self.extract([("built-ins/Object/example.js", raw)])
        self.assertEqual(len(rows), 1)
        self.assertEqual((rows[0]["variant"], rows[0]["mode"], rows[0]["source"]),
                         ("non-strict", "NonStrict", raw.decode("utf-8")))
        self.assertEqual(rows[0]["strictPrefixLength"], 0)

    def test_only_strict_emits_directive_and_updated_source_hash(self):
        raw = source({"flags": ["onlyStrict"]})
        _, rows = self.extract([("built-ins/Object/example.js", raw)])
        self.assertEqual(len(rows), 1)
        row = rows[0]
        expected = audit.preparation.STRICT_PREFIX.encode() + raw
        self.assertEqual((row["variant"], row["mode"], row["inputKind"]), ("strict", "Strict", "Script"))
        self.assertEqual(row["strictPrefixLength"], len(audit.preparation.STRICT_PREFIX))
        self.assertEqual(row["sourceSha256"], digest(expected))
        self.assertEqual(row["originalSha256"], digest(raw))
        self.assertNotEqual(row["sourceSha256"], row["originalSha256"])

    def test_module_is_strict_and_keeps_original_source(self):
        raw = source({"flags": ["module"]}, "export const = ;\n")
        _, rows = self.extract([("built-ins/Object/example.js", raw)])
        self.assertEqual(len(rows), 1)
        self.assertEqual((rows[0]["variant"], rows[0]["inputKind"], rows[0]["mode"]),
                         ("module", "Module", "Strict"))
        self.assertEqual(rows[0]["source"], raw.decode("utf-8"))
        self.assertEqual(rows[0]["strictPrefixLength"], 0)

    def test_raw_is_non_strict_and_preserves_hashbang(self):
        raw = b"#! raw fixture\n" + source({"flags": ["raw"]})
        _, rows = self.extract([("built-ins/Object/example.js", raw)])
        self.assertEqual(len(rows), 1)
        self.assertEqual((rows[0]["variant"], rows[0]["inputKind"], rows[0]["mode"]),
                         ("raw", "Script", "NonStrict"))
        self.assertEqual(rows[0]["source"].encode(), raw)

    def test_module_raw_combination_is_preserved_strict_module(self):
        raw = b"#! module fixture\n" + source({"flags": ["module", "raw"]})
        _, rows = self.extract([("built-ins/Object/example.js", raw)])
        self.assertEqual(len(rows), 1)
        self.assertEqual((rows[0]["variant"], rows[0]["inputKind"], rows[0]["mode"]),
                         ("module", "Module", "Strict"))
        self.assertEqual(rows[0]["source"].encode(), raw)

    def test_async_and_generated_flags_do_not_change_variant_semantics(self):
        _, rows = self.extract([("built-ins/Object/example.js", source({"flags": ["async", "generated"]}))])
        self.assertEqual([r["variant"] for r in rows], ["non-strict", "strict"])
        self.assertEqual(rows[0]["flags"], ["async", "generated"])

    def test_staging_cases_are_explicitly_separate_supplementary_scope(self):
        report, rows = self.extract([
            ("staging/sm/a.js", source({"flags": ["noStrict"]})),
            ("built-ins/Object/staging-like/a.js", source({"flags": ["noStrict"]}))])
        self.assertEqual(report["scopeFileCounts"], {"additional-standard-path": 1, "supplementary-staging": 1})
        self.assertEqual([r["auditScope"] for r in rows], ["additional-standard-path", "supplementary-staging"])
        self.assertIn("not included in the main Test262 success total", report["scopePolicy"])

    def test_main_selection_paths_are_skipped_with_directory_boundaries(self):
        files = [(path, source()) for path in [
            "language/statements/a.js", "annexB/a.js", "built-ins/RegExp/a.js",
            "language-other/a.js", "annexB-other/a.js", "built-ins/RegExp-other/a.js"]]
        report, rows = self.extract(files)
        self.assertEqual(report["counts"]["alreadyWithinMainSelectionPaths"], 3)
        self.assertEqual(report["counts"]["outsideMainSelectionPaths"], 3)
        self.assertEqual({r["path"] for r in rows}, {path for path, _ in files[3:]})

    def test_positive_runtime_and_resolution_sources_are_not_parse_negatives(self):
        files = [("built-ins/Object/" + name + ".js", source({"negative": negative})) for name, negative in [
            ("positive", None), ("runtime", {"phase": "runtime", "type": "SyntaxError"}),
            ("resolution", {"phase": "resolution", "type": "SyntaxError"})]]
        report, rows = self.extract(files)
        self.assertEqual(rows, [])
        self.assertEqual(report["counts"]["outsideFilesWithoutParseSyntaxErrorExpectation"], 3)
        self.assertEqual(report["unexpectedMetadataErrorCount"], 0)

    def test_fixture_with_valid_metadata_is_excluded(self):
        report, rows = self.extract([("built-ins/Object/import_FIXTURE.js", source())])
        self.assertEqual(rows, [])
        self.assertEqual(report["excludedParseNegativeFiles"][0]["reason"], "fixture_not_standalone")
        self.assertTrue(report["complete"])

    def test_future_feature_is_excluded_in_standard_and_staging_paths(self):
        report, rows = self.extract([
            ("built-ins/Object/future.js", source({"features": ["future-syntax", "ordinary"]})),
            ("staging/future.js", source({"features": ["future-syntax"]}))])
        self.assertEqual(rows, [])
        self.assertEqual(len(report["excludedParseNegativeFiles"]), 2)
        for item in report["excludedParseNegativeFiles"]:
            self.assertEqual(item["reason"], "future_syntax_feature")
            self.assertEqual(item["excludedFeatures"], ["future-syntax"])

    def test_source_order_ordinals_and_feature_file_counts_are_deterministic(self):
        metadata = {"features": ["one", "one", "two"]}
        report, rows = self.extract([
            ("built-ins/Object/z.js", source(metadata)),
            ("built-ins/Object/a.js", source(metadata))])
        self.assertEqual([r["ordinal"] for r in rows], list(range(4)))
        self.assertEqual([r["path"] for r in rows], ["built-ins/Object/a.js"] * 2 + ["built-ins/Object/z.js"] * 2)
        self.assertEqual(report["featureFileCounts"], {"one": 2, "two": 2})
        self.assertEqual(report["variantCounts"], {"non-strict": 2, "strict": 2})

    def test_utf8_bom_crlf_copyright_and_body_are_preserved(self):
        raw = b"\xef\xbb\xbf" + source({"description": "Unicode text \uac00", "includes": ["harness.js"]},
                                     "const \uac00 = ;\n", newline="\r\n")
        report, rows = self.extract([("built-ins/Object/crlf.js", raw)])
        self.assertEqual(rows[0]["source"].encode("utf-8"), raw)
        self.assertEqual(rows[1]["source"].encode("utf-8"), audit.preparation.STRICT_PREFIX.encode() + raw)
        for row in rows:
            self.assertEqual(row["originalSha256"], digest(raw))
            self.assertEqual(row["sourceSha256"], digest(row["source"].encode("utf-8")))
        self.assertEqual(report["selectedFiles"][0]["byteLength"], len(raw))

    def test_non_test_and_non_javascript_members_are_not_counted(self):
        self.bundle([("built-ins/Object/a.txt", b"not JavaScript")], extra=[
            (ZIP_ROOT + "/harness/a.js", b"not a standalone test"),
            (ZIP_ROOT + "/docs/file.js", b"not a standalone test")])
        report, rows = self.extract()
        self.assertEqual(rows, [])
        self.assertEqual(report["counts"].get("allJavaScriptFiles", 0), 0)
        self.assertTrue(report["complete"])

    def test_manifest_binds_snapshot_archive_filter_and_emitted_bytes(self):
        snapshot = self.bundle([("built-ins/Object/a.js", source())])
        report, _ = self.extract()
        self.assertEqual(self.manifest(), report)
        self.assertEqual(report["snapshot"], snapshot)
        self.assertEqual(report["archive"]["sha256"], digest(self.archive.read_bytes()))
        self.assertEqual(report["featureFilter"]["sha256"], digest(self.filter.read_bytes()))
        self.assertEqual(report["casesSha256"], digest(self.output.read_bytes()))

    def test_missing_provenance_fails_without_overwriting_previous_output(self):
        self.bundle([("built-ins/Object/a.js", source())])
        self.snapshot_file.unlink()
        self.output.write_text("previous output", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "snapshot provenance"):
            self.extract()
        self.assertEqual(self.output.read_text(), "previous output")

    def test_malformed_or_nonofficial_provenance_fails_closed(self):
        snapshot = self.bundle([("built-ins/Object/a.js", source())])
        values = [None, [], {}, {**snapshot, "repository": "https://example.invalid/test262"},
                  {**snapshot, "commit": "main"}, {**snapshot, "commit": None},
                  {**snapshot, "archiveSha256": None}, {**snapshot, "archiveSha256": "bad"}]
        for value in values:
            with self.subTest(provenance=value):
                self.snapshot_file.write_text(json.dumps(value), encoding="utf-8")
                with self.assertRaisesRegex(ValueError, "snapshot provenance"):
                    self.extract()
        self.assertFalse(self.output.exists())

    def test_archive_digest_mismatch_fails_without_overwriting_output(self):
        snapshot = self.bundle([("built-ins/Object/a.js", source())])
        snapshot["archiveSha256"] = "0" * 64
        self.snapshot_file.write_text(json.dumps(snapshot), encoding="utf-8")
        self.output.write_text("previous output", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "archive hash differs"):
            self.extract()
        self.assertEqual(self.output.read_text(), "previous output")

    def test_wrong_archive_commit_root_fails_before_output_open(self):
        self.bundle([("built-ins/Object/a.js", source())], root="test262-" + "b" * 40)
        self.output.write_text("previous output", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "archive root differs"):
            self.extract()
        self.assertEqual(self.output.read_text(), "previous output")

    def test_unsafe_archive_paths_fail_closed(self):
        for path in [ZIP_ROOT + "/test/../bad.js", ZIP_ROOT + "\\test\\bad.js", "/" + ZIP_ROOT + "/test/bad.js"]:
            with self.subTest(path=path):
                self.bundle([], extra=[(path, source())])
                with self.assertRaisesRegex(ValueError, "unsafe path"):
                    self.extract()
        self.assertFalse(self.output.exists())

    def test_duplicate_archive_source_path_fails_before_output_open(self):
        self.bundle([("built-ins/Object/a.js", source()), ("built-ins/Object/a.js", source(body="var = ;\n"))])
        with self.assertRaisesRegex(ValueError, "duplicate JavaScript path"):
            self.extract()
        self.assertFalse(self.output.exists())

    def test_frontmatter_and_utf8_errors_are_reported_and_later_files_continue(self):
        files = [("built-ins/Object/a-missing.js", b"let = ;"),
                 ("built-ins/Object/b-yaml.js", b"/*---\nflags: [\n---*/"),
                 ("built-ins/Object/c-list.js", b"/*---\n- mapping required\n---*/"),
                 ("built-ins/Object/d-utf8.js", b"\xff"),
                 ("built-ins/Object/z-valid.js", source({"flags": ["noStrict"]}))]
        report, rows = self.extract(files)
        self.assertEqual([r["path"] for r in rows], ["built-ins/Object/z-valid.js"])
        self.assertEqual([e["reason"] for e in report["metadataErrors"]],
                         ["missing_frontmatter", "invalid_frontmatter_yaml", "frontmatter_is_not_mapping", "invalid_utf8"])
        self.assertEqual(report["unexpectedMetadataErrorCount"], 4)
        self.assertFalse(report["complete"])
        self.assertEqual(self.manifest()["unexpectedMetadataErrorCount"], 4)

    def test_invalid_list_metadata_is_audited_instead_of_aborting_scan(self):
        for field in ["flags", "features", "includes"]:
            for value in ["not-a-list", [1]]:
                with self.subTest(field=field, value=value):
                    report, rows = self.extract([
                        ("built-ins/Object/a-invalid.js", source({field: value})),
                        ("built-ins/Object/z-valid.js", source({"flags": ["noStrict"]}))])
                    self.assertEqual(len(rows), 1)
                    self.assertEqual(report["metadataErrors"][0]["reason"], "invalid_" + field)
                    self.assertEqual(report["unexpectedMetadataErrorCount"], 1)

    def test_conflicting_mode_flags_are_metadata_errors(self):
        for flags in [["module", "noStrict"], ["raw", "onlyStrict"], ["noStrict", "onlyStrict"]]:
            with self.subTest(flags=flags):
                report, rows = self.extract([("built-ins/Object/a.js", source({"flags": flags}))])
                self.assertEqual(rows, [])
                self.assertEqual(report["metadataErrors"][0]["reason"], "conflicting_variant_flags")
                self.assertFalse(report["complete"])

    def test_invalid_negative_metadata_is_not_silently_counted_as_positive(self):
        cases = [("not a mapping", "invalid_negative_metadata"),
                 ({"phase": "parse"}, "invalid_negative_metadata"),
                 ({"phase": "parse", "type": "TypeError"}, "parse_negative_is_not_syntax_error"),
                 ({"phase": "unknown", "type": "SyntaxError"}, "unknown_negative_phase")]
        for negative, reason in cases:
            with self.subTest(negative=negative):
                report, rows = self.extract([("built-ins/Object/a.js", source({"negative": negative}))])
                self.assertEqual(rows, [])
                self.assertEqual(report["metadataErrors"][0]["reason"], reason)
                self.assertEqual(report["unexpectedMetadataErrorCount"], 1)

    def test_unknown_flag_is_reported_instead_of_assuming_default_modes(self):
        report, rows = self.extract([("built-ins/Object/a.js", source({"flags": ["new-mode"]}))])
        self.assertEqual(rows, [])
        self.assertEqual(report["metadataErrors"][0]["reason"], "unknown_flags: new-mode")
        self.assertFalse(report["complete"])

    def test_missing_metadata_in_fixture_is_recorded_but_not_unexpected(self):
        report, rows = self.extract([("built-ins/Object/aux_FIXTURE.js", b"export {};")])
        self.assertEqual(rows, [])
        self.assertEqual(report["metadataErrors"], [{"path": "built-ins/Object/aux_FIXTURE.js",
                                                   "reason": "missing_frontmatter", "fixture": True}])
        self.assertEqual(report["unexpectedMetadataErrorCount"], 0)
        self.assertTrue(report["complete"])

    def test_cli_succeeds_when_only_fixture_metadata_is_missing(self):
        self.bundle([("built-ins/Object/aux_FIXTURE.js", b"export {};")])
        result = self.cli()
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(json.loads(result.stdout)["unexpectedMetadataErrorCount"], 0)
        self.assertTrue(self.manifest()["complete"])

    def test_cli_fails_after_recording_real_metadata_errors_and_valid_cases(self):
        self.bundle([("built-ins/Object/a-invalid.js", source({"flags": "invalid"})),
                     ("built-ins/Object/z-valid.js", source({"flags": ["noStrict"]}))])
        result = self.cli()
        self.assertNotEqual(result.returncode, 0)
        self.assertEqual(json.loads(result.stdout)["unexpectedMetadataErrorCount"], 1)
        self.assertFalse(self.manifest()["complete"])
        self.assertEqual(self.manifest()["counts"]["emittedVariants"], 1)
        self.assertIn("z-valid.js", self.output.read_text())

    def test_cli_rejects_missing_provenance_before_creating_output(self):
        self.bundle([("built-ins/Object/a.js", source())])
        self.snapshot_file.unlink()
        result = self.cli()
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("snapshot provenance", result.stderr)
        self.assertFalse(self.output.exists())

    def test_cli_rejects_archive_hash_mismatch_before_creating_output(self):
        snapshot = self.bundle([("built-ins/Object/a.js", source())])
        snapshot["archiveSha256"] = "0" * 64
        self.snapshot_file.write_text(json.dumps(snapshot), encoding="utf-8")
        result = self.cli()
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("archive hash differs", result.stderr)
        self.assertFalse(self.output.exists())

    def test_invalid_feature_exclusion_reason_fails_without_output(self):
        self.bundle([("built-ins/Object/a.js", source())])
        self.filter.write_text(json.dumps({"excludedFeatures": {"future-syntax": ""}}), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "nonempty exclusion reasons"):
            self.extract()
        self.assertFalse(self.output.exists())

    def test_unknown_feature_tag_is_retained_and_counted(self):
        report, rows = self.extract([("built-ins/Object/a.js", source({"features": ["unlisted-feature"]}))])
        self.assertEqual(len(rows), 2)
        self.assertEqual(report["featureFileCounts"], {"unlisted-feature": 1})
        self.assertEqual(rows[0]["features"], ["unlisted-feature"])


if __name__ == "__main__":
    unittest.main()
