"""Regression tests for source preservation and Test262 expectation semantics."""

from __future__ import annotations

import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import io
import zipfile


SPEC = importlib.util.spec_from_file_location("prepare_test262", Path(__file__).with_name("prepare_test262.py"))
prepare = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(prepare)


class MetadataTests(unittest.TestCase):
    def test_default_variants_have_exact_official_prefix(self):
        text = "// Copyright\r\n/*---\r\ndescription: case\r\n---*/\r\nlet x = 1;\r\n"
        result = prepare.variants(text, [])
        self.assertEqual(result, [
            ("non-strict", "Script", "NonStrict", text),
            ("strict", "Script", "Strict", '"use strict";\n' + text),
        ])

    def test_module_is_strict_without_inserting_directive(self):
        text = "#!/usr/bin/env node\nexport {}"
        self.assertEqual(prepare.variants(text, ["module", "async"]), [("module", "Module", "Strict", text)])

    def test_raw_module_retains_hashbang_with_module_strictness(self):
        text = "#!\r\n/*---\r\nflags: [module, raw]\r\n---*/\r\nexport {};"
        self.assertEqual(prepare.variants(text, ["module", "raw"]), [("module", "Module", "Strict", text)])

    def test_raw_retains_bom_hashbang_and_line_endings(self):
        text = "\ufeff#!x\r\ntext\rtext\n"
        self.assertEqual(prepare.variants(text, ["raw"]), [("raw", "Script", "NonStrict", text)])

    def test_no_strict_only_emits_original(self):
        self.assertEqual(prepare.variants("with(x){}", ["noStrict"])[0], ("non-strict", "Script", "NonStrict", "with(x){}"))

    def test_only_strict_prepends_directive(self):
        self.assertEqual(prepare.variants("x;", ["onlyStrict"]), [("strict", "Script", "Strict", '"use strict";\nx;')])

    def test_conflicting_variant_flags_are_reported(self):
        for flags in (["noStrict", "onlyStrict"], ["module", "onlyStrict"], ["raw", "noStrict"]):
            with self.subTest(flags=flags), self.assertRaisesRegex(ValueError, "conflicting_variant_flags"):
                prepare.variants("x", flags)

    def test_parse_syntax_error_requires_rejection(self):
        self.assertEqual(prepare.expected_acceptance({"negative": {"phase": "parse", "type": "SyntaxError"}}), (False, "parse-negative", "parse", "SyntaxError"))

    def test_runtime_syntax_error_is_not_a_source_rejection(self):
        # E.g. eval("invalid syntax") is a well-formed source program.
        self.assertEqual(prepare.expected_acceptance({"negative": {"phase": "runtime", "type": "SyntaxError"}}), (True, "runtime-negative-source-only", "runtime", "SyntaxError"))

    def test_resolution_syntax_error_is_not_a_source_rejection(self):
        self.assertTrue(prepare.expected_acceptance({"negative": {"phase": "resolution", "type": "SyntaxError"}})[0])

    def test_runtime_type_error_is_accepted_syntax(self):
        self.assertTrue(prepare.expected_acceptance({"negative": {"phase": "runtime", "type": "TypeError"}})[0])

    def test_positive_is_accepted_syntax(self):
        self.assertEqual(prepare.expected_acceptance({}), (True, "positive", None, None))

    def test_non_syntax_parse_negative_is_reported(self):
        with self.assertRaisesRegex(ValueError, "parse_negative_is_not_syntax_error"):
            prepare.expected_acceptance({"negative": {"phase": "parse", "type": "ReferenceError"}})

    def test_unknown_phase_is_reported(self):
        with self.assertRaisesRegex(ValueError, "unknown_negative_phase"):
            prepare.expected_acceptance({"negative": {"phase": "early", "type": "SyntaxError"}})

    def test_invalid_negative_metadata_is_reported(self):
        for negative in ("SyntaxError", {}, {"phase": "parse"}, {"phase": [], "type": "SyntaxError"}):
            with self.subTest(negative=negative), self.assertRaisesRegex(ValueError, "invalid_negative_metadata"):
                prepare.expected_acceptance({"negative": negative})

    def test_yaml_multiline_and_sequences_are_parsed(self):
        metadata = prepare.frontmatter("// Copyright\n/*---\ndescription: >\n  First line\n  second line\nfeatures: [async-functions, generators]\nnegative:\n  phase: parse\n  type: SyntaxError\n---*/\ninvalid code")
        self.assertEqual(metadata["description"], "First line second line\n")
        self.assertEqual(prepare.string_list(metadata, "features"), ["async-functions", "generators"])
        self.assertFalse(prepare.expected_acceptance(metadata)[0])

    def test_missing_frontmatter_is_reported(self):
        with self.assertRaisesRegex(ValueError, "missing_frontmatter"):
            prepare.frontmatter("// fixture\nx();")

    def test_invalid_yaml_is_reported(self):
        with self.assertRaisesRegex(ValueError, "invalid_frontmatter_yaml"):
            prepare.frontmatter("/*---\nflags: [\n---*/")

    def test_scalar_frontmatter_is_reported(self):
        with self.assertRaisesRegex(ValueError, "frontmatter_is_not_mapping"):
            prepare.frontmatter("/*---\ntext\n---*/")

    def test_bad_list_metadata_is_reported(self):
        for value in ("raw", {"raw": True}, [1]):
            with self.subTest(value=value), self.assertRaisesRegex(ValueError, "invalid_flags"):
                prepare.string_list({"flags": value}, "flags")

    def test_prefix_boundary_does_not_match_similar_names(self):
        self.assertTrue(prepare.has_prefix("language/module-code/x.js", "language/module-code"))
        self.assertFalse(prepare.has_prefix("language/module-code-extra/x.js", "language/module-code"))

    def test_prefix_rejects_traversal(self):
        for value in ("..", "language/../staging", "", "language//x", "."):
            with self.subTest(value=value), self.assertRaises(ValueError):
                prepare.normalized_prefix(value)

    def test_prefix_accepts_test_root_and_windows_separators(self):
        self.assertEqual(prepare.normalized_prefix("test\\language\\module-code/"), "language/module-code")


class PreparationTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name) / "test262"
        (self.root / "test").mkdir(parents=True)
        (self.root / "LICENSE").write_bytes(b"Test262 original BSD license\n")
        self.output = Path(self.temporary.name) / "cases.jsonl"

    def tearDown(self):
        self.temporary.cleanup()

    def case(self, path="language/a.js", metadata="description: sample\n", body="x();\n", ending="\n"):
        source = "// Copyright original" + ending + "/*---" + ending + metadata.replace("\n", ending) + "---*/" + ending + body
        target = self.root / "test" / path
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(source.encode("utf-8"))
        return source

    def records(self, path=None):
        return [json.loads(line) for line in (path or self.output).read_text(encoding="utf-8").splitlines()]

    def test_end_to_end_preserves_source_copyright_and_original_hash(self):
        source = self.case(metadata="description: sample\nflags: [raw]\n", body="a\rb\r\n", ending="\r\n")
        manifest = prepare.prepare(self.root, self.output)
        record = self.records()[0]
        self.assertEqual(record["source"], source)
        self.assertEqual(record["originalSha256"], hashlib.sha256(source.encode("utf-8")).hexdigest())
        self.assertEqual(record["sourceSha256"], record["originalSha256"])
        self.assertEqual(record["strictPrefixLength"], 0)
        self.assertEqual(manifest["counts"]["emittedVariants"], 1)
        self.assertEqual(manifest["license"]["sha256"], hashlib.sha256((self.root / "LICENSE").read_bytes()).hexdigest())

    def test_includes_are_retained_as_metadata_without_injection(self):
        source = self.case(metadata="description: sample\nincludes: [assert.js, sta.js]\nflags: [noStrict]\n")
        prepare.prepare(self.root, self.output)
        record = self.records()[0]
        self.assertEqual(record["includes"], ["assert.js", "sta.js"])
        self.assertEqual(record["source"], source)

    def test_all_negative_phases_have_source_only_expectation(self):
        for index, phase in enumerate(("parse", "resolution", "runtime")):
            self.case(f"language/{index}.js", metadata=f"description: sample\nflags: [noStrict]\nnegative:\n  phase: {phase}\n  type: SyntaxError\n")
        manifest = prepare.prepare(self.root, self.output)
        self.assertEqual([record["expectAccept"] for record in self.records()], [False, True, True])
        self.assertEqual(manifest["negativePhaseFileCounts"], {"parse": 1, "resolution": 1, "runtime": 1})

    def test_excluded_categories_are_visible_in_manifest_and_audit(self):
        self.case("language/a_FIXTURE.js")
        self.case("staging/a.js")
        self.case("intl402/a.js")
        self.case("built-ins/Array/a.js")
        self.case("language/missing.js")
        (self.root / "test/language/missing.js").write_bytes(b"x();")
        self.case("language/bad-utf8.js")
        (self.root / "test/language/bad-utf8.js").write_bytes(b"\xff")
        manifest = prepare.prepare(self.root, self.output)
        counts = manifest["counts"]
        self.assertEqual(counts["discoveredFiles"], 6)
        for reason in ("fixture_not_standalone", "staging_not_standard_corpus", "intl402_outside_ecma262", "outside_selected_prefixes", "missing_frontmatter", "invalid_utf8"):
            self.assertEqual(counts["excluded:" + reason], 1)
        audit = self.records(self.output.with_suffix(".selection.jsonl"))
        self.assertEqual(len(audit), 6)
        self.assertTrue(all(record["status"] == "excluded" for record in audit))

    def test_feature_filter_records_reason_and_variant_counts(self):
        self.case(metadata="description: sample\nfeatures: [future-feature, async-functions]\n")
        manifest = prepare.prepare(self.root, self.output, exclusion_reasons={"future-feature": "not in the 2026 edition"})
        self.assertEqual(self.records(), [])
        self.assertEqual(manifest["featureFileCounts"], {"async-functions": 1, "future-feature": 1})
        self.assertEqual(manifest["excludedFeatureVariantCounts"], {"future-feature": 2})
        self.assertEqual(manifest["counts"]["excludedVariants:feature_filter"], 2)
        audit = self.records(self.output.with_suffix(".selection.jsonl"))[0]
        self.assertEqual(audit["exclusionReasons"], {"future-feature": "not in the 2026 edition"})

    def test_default_prefixes_include_regexp_and_annex_b(self):
        self.case("built-ins/RegExp/a.js", metadata="description: regex\nflags: [noStrict]\n")
        self.case("annexB/a.js", metadata="description: annex\nflags: [noStrict]\n")
        prepare.prepare(self.root, self.output)
        self.assertEqual({record["path"] for record in self.records()}, {"built-ins/RegExp/a.js", "annexB/a.js"})

    def test_limit_and_offset_do_not_hide_planned_cases(self):
        self.case("language/a.js")
        self.case("language/b.js")
        manifest = prepare.prepare(self.root, self.output, offset=1, limit=1)
        self.assertEqual([record["id"] for record in self.records()], ["language/a.js#strict"])
        self.assertEqual(manifest["counts"]["plannedVariants"], 4)
        self.assertEqual(manifest["counts"]["omittedVariants:offset"], 1)
        self.assertEqual(manifest["counts"]["omittedVariants:limit"], 2)

    def test_disjoint_shards_cover_all_variants(self):
        for name in ("a", "b", "c"):
            self.case(f"language/{name}.js")
        prepare.prepare(self.root, self.output)
        full = {record["id"] for record in self.records()}
        shards = []
        for index in range(3):
            target = self.output.with_name(f"shard{index}.jsonl")
            prepare.prepare(self.root, target, shard_count=3, shard_index=index)
            shards.append({record["id"] for record in self.records(target)})
        self.assertEqual(set.union(*shards), full)
        self.assertTrue(all(not (shards[a] & shards[b]) for a in range(3) for b in range(a + 1, 3)))

    def test_unknown_flag_is_counted_without_silently_dropping_case(self):
        self.case(metadata="description: sample\nflags: [futureFlag, noStrict]\n")
        manifest = prepare.prepare(self.root, self.output)
        self.assertEqual(manifest["unknownFlagFileCounts"], {"futureFlag": 1})
        self.assertEqual(len(self.records()), 1)

    def test_bad_parse_phase_is_audited_not_accepted(self):
        self.case(metadata="description: sample\nnegative:\n  phase: parse\n  type: TypeError\n")
        manifest = prepare.prepare(self.root, self.output)
        self.assertEqual(manifest["counts"]["excluded:parse_negative_is_not_syntax_error"], 1)
        self.assertEqual(self.records(), [])

    def test_snapshot_provenance_is_included(self):
        self.case()
        value = {"commit": prepare.PINNED_COMMIT, "archiveSha256": "original_archive_hash"}
        (self.root / "test262.snapshot.json").write_text(json.dumps(value), encoding="utf-8")
        self.assertEqual(prepare.prepare(self.root, self.output)["snapshot"], value)

    def test_feature_file_format_and_hash_are_preserved(self):
        self.case()
        feature_file = Path(self.temporary.name) / "edition.json"
        feature_file.write_text(json.dumps({"excludedFeatures": {"future": "2027 proposal"}}), encoding="utf-8")
        reasons = prepare.feature_exclusions(feature_file)
        self.assertEqual(reasons, {"future": "2027 proposal"})
        manifest = prepare.prepare(self.root, self.output, exclusion_reasons=reasons, feature_filter_path=feature_file)
        self.assertEqual(manifest["featureFilter"]["sha256"], hashlib.sha256(feature_file.read_bytes()).hexdigest())

    def test_feature_file_requires_explicit_nonempty_reasons(self):
        feature_file = Path(self.temporary.name) / "edition.json"
        for value in (["future"], {"future": ""}, {"future": True}, {"": "reason"}):
            feature_file.write_text(json.dumps(value), encoding="utf-8")
            with self.subTest(value=value), self.assertRaises(ValueError):
                prepare.feature_exclusions(feature_file)

    def test_invalid_shard_and_limit_are_rejected(self):
        for options in ({"shard_count": 0}, {"shard_count": 2, "shard_index": 2}, {"offset": -1}, {"limit": -1}):
            with self.subTest(options=options), self.assertRaises(ValueError):
                prepare.prepare(self.root, self.output, **options)

    def test_download_refuses_to_overwrite_existing_files(self):
        with self.assertRaisesRegex(ValueError, "existing files are preserved"):
            prepare.download_snapshot(self.root)
        self.assertTrue((self.root / "LICENSE").exists())

    def test_download_rejects_unpinned_ref_names(self):
        with self.assertRaisesRegex(ValueError, "full lowercase Git commit SHA"):
            prepare.download_snapshot(Path(self.temporary.name) / "new", "main")

    def test_archive_and_extracted_sources_emit_identical_cases_and_hashes(self):
        self.case("language/b.js", metadata="description: raw\nflags: [raw]\n", body="a\rb\r\n", ending="\r\n")
        self.case("language/A.js", metadata="description: normal\nfeatures: [generators]\n")
        archive = Path(self.temporary.name) / "source.zip"
        with zipfile.ZipFile(archive, "w") as bundle:
            for file in (self.root / "test").rglob("*.js"):
                bundle.writestr("test262-" + prepare.PINNED_COMMIT + "/test/" + file.relative_to(self.root / "test").as_posix(), file.read_bytes())
        (self.root / "test262.snapshot.json").write_text(json.dumps({"commit": prepare.PINNED_COMMIT, "archiveSha256": prepare.file_sha256(archive)}), encoding="utf-8")
        extracted = prepare.prepare(self.root, self.output)
        extracted_bytes = self.output.read_bytes()
        other_output = self.output.with_name("archive-cases.jsonl")
        archived = prepare.prepare(self.root, other_output, source_archive=archive)
        self.assertEqual(other_output.read_bytes(), extracted_bytes)
        self.assertEqual(archived["casesSha256"], extracted["casesSha256"])
        self.assertEqual(archived["candidateSourceSetSha256"], extracted["candidateSourceSetSha256"])
        self.assertEqual(archived["selectionAuditSha256"], extracted["selectionAuditSha256"])
        self.assertEqual(archived["counts"], extracted["counts"])
        self.assertEqual(archived["sourceArchive"]["sha256"], prepare.file_sha256(archive))

    def test_archive_checksum_must_match_recorded_snapshot(self):
        self.case()
        archive = Path(self.temporary.name) / "source.zip"
        with zipfile.ZipFile(archive, "w") as bundle:
            bundle.writestr("test262-main/test/language/a.js", b"x")
        (self.root / "test262.snapshot.json").write_text(json.dumps({"commit": prepare.PINNED_COMMIT, "archiveSha256": "wrong-checksum"}), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "archive hash does not match"):
            prepare.prepare(self.root, self.output, source_archive=archive)

    def test_archive_root_must_match_recorded_snapshot(self):
        self.case()
        archive = Path(self.temporary.name) / "source.zip"
        with zipfile.ZipFile(archive, "w") as bundle:
            bundle.writestr("test262-main/test/language/a.js", b"x")
        (self.root / "test262.snapshot.json").write_text(json.dumps({"commit": prepare.PINNED_COMMIT}), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "archive root does not match"):
            prepare.prepare(self.root, self.output, source_archive=archive)


class SnapshotTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name) / "snapshot"
        self.archive = self.root.parent / ("test262-" + prepare.PINNED_COMMIT + ".zip")

    def tearDown(self):
        self.temporary.cleanup()

    def bundle(self, path: str, content: bytes):
        with zipfile.ZipFile(self.archive, "w") as bundle:
            bundle.writestr(path, content)

    def test_archive_extraction_preserves_all_source_bytes_and_license(self):
        prefix = "test262-" + prepare.PINNED_COMMIT + "/"
        raw = b"\xef\xbb\xbf// copyright\r\n/*---\r\nflags: [raw]\r\n---*/\r\nx\r"
        with zipfile.ZipFile(self.archive, "w") as bundle:
            bundle.writestr(prefix + "test/language/raw.js", raw)
            bundle.writestr(prefix + "LICENSE", b"original license\r\n")
        snapshot = prepare.download_snapshot(self.root)
        self.assertEqual((self.root / "test/language/raw.js").read_bytes(), raw)
        self.assertEqual((self.root / "LICENSE").read_bytes(), b"original license\r\n")
        self.assertEqual(snapshot["commit"], prepare.PINNED_COMMIT)
        self.assertEqual(snapshot["archiveSha256"], hashlib.sha256(self.archive.read_bytes()).hexdigest())

    def test_archive_cannot_escape_snapshot_directory(self):
        self.bundle("test262-" + prepare.PINNED_COMMIT + "/../outside.js", b"x")
        with self.assertRaisesRegex(ValueError, "unsafe archive path"):
            prepare.download_snapshot(self.root)
        self.assertFalse((self.root.parent / "outside.js").exists())

    def test_archive_rejects_windows_separator_paths(self):
        self.bundle("test262-" + prepare.PINNED_COMMIT + "/..\\outside.js", b"x")
        with self.assertRaisesRegex(ValueError, "unsafe archive path"):
            prepare.download_snapshot(self.root)

    def test_archive_root_must_match_requested_commit(self):
        self.bundle("test262-main/test/language/a.js", b"x")
        with self.assertRaisesRegex(ValueError, "archive root does not match"):
            prepare.download_snapshot(self.root)

    def test_custom_commit_date_is_read_from_matching_official_metadata(self):
        commit = "a" * 40
        payload = {"sha": commit, "commit": {"committer": {"date": "2026-10-13T22:44:04Z"}}}
        with patch.object(prepare.urllib.request, "urlopen", return_value=io.BytesIO(json.dumps(payload).encode())) as request:
            self.assertEqual(prepare.official_commit_date(commit), "2026-10-13T22:44:04Z")
        self.assertEqual(request.call_args.args[0].full_url, "https://api.github.com/repos/tc39/test262/commits/" + commit)

    def test_custom_commit_date_rejects_mismatched_revision(self):
        payload = {"sha": "b" * 40, "commit": {"committer": {"date": "2026-10-13T22:44:04Z"}}}
        with patch.object(prepare.urllib.request, "urlopen", return_value=io.BytesIO(json.dumps(payload).encode())):
            with self.assertRaisesRegex(ValueError, "does not match requested revision"):
                prepare.official_commit_date("a" * 40)

    def test_custom_commit_date_requires_an_official_timestamp(self):
        payload = {"sha": "a" * 40, "commit": {}}
        with patch.object(prepare.urllib.request, "urlopen", return_value=io.BytesIO(json.dumps(payload).encode())):
            with self.assertRaisesRegex(ValueError, "missing its committer date"):
                prepare.official_commit_date("a" * 40)


if __name__ == "__main__":
    unittest.main()
