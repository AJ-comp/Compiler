"""Regression coverage for exact Test262 input/result reuse boundaries."""

import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest


SPEC = importlib.util.spec_from_file_location("compare_test262", Path(__file__).with_name("compare_test262.py"))
compare = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(compare)


def case(name="a", source="x();\n"):
    return {"id": name + ".js#non-strict", "path": "language/" + name + ".js", "variant": "non-strict",
            "source": source, "sourceSha256": hashlib.sha256(source.encode("utf-8")).hexdigest(),
            "inputKind": "Script", "mode": "NonStrict", "expectAccept": True, "webCompatibility": True,
            "features": [], "description": "original metadata"}


class ComparisonTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
        self.baseline = self.root / "baseline.jsonl"
        self.latest = self.root / "latest.jsonl"
        self.output = self.root / "delta.jsonl"

    def tearDown(self):
        self.temporary.cleanup()

    def write(self, path, rows):
        path.write_text("".join(json.dumps(row) + "\n" for row in rows), encoding="utf-8")

    def read(self, path):
        return [json.loads(text) for text in path.read_text(encoding="utf-8").splitlines()]

    def run_comparison(self, old, new):
        self.write(self.baseline, old)
        self.write(self.latest, new)
        return compare.compare(self.baseline, self.latest, self.output)

    def test_identical_cases_are_reusable_and_delta_is_empty(self):
        row = case()
        result = self.run_comparison([row], [row])
        self.assertEqual(result["counts"], {"baselineVariants": 1, "latestVariants": 1, "reusableVariants": 1,
                                            "addedVariants": 0, "changedVariants": 0, "removedVariants": 0, "deltaVariants": 0})
        self.assertEqual(self.output.read_bytes(), b"")

    def test_new_id_is_added_and_old_id_is_reported_as_removed(self):
        result = self.run_comparison([case("old")], [case("new")])
        self.assertEqual(result["counts"]["addedVariants"], 1)
        self.assertEqual(result["counts"]["removedVariants"], 1)
        self.assertEqual(self.read(self.output)[0]["id"], "new.js#non-strict")
        statuses = {row["id"]: row["status"] for row in self.read(self.output.with_suffix(".comparison.jsonl"))}
        self.assertEqual(statuses, {"old.js#non-strict": "removed", "new.js#non-strict": "added"})

    def test_source_change_requires_delta_even_for_comment_only_change(self):
        result = self.run_comparison([case(source="// old metadata\nx();")], [case(source="// new metadata\nx();")])
        self.assertEqual(result["counts"]["changedVariants"], 1)
        self.assertEqual(result["changedFieldCounts"], {"sourceSha256": 1})

    def test_line_ending_change_requires_delta(self):
        result = self.run_comparison([case(source="x();\n")], [case(source="x();\r\n")])
        self.assertEqual(result["counts"]["changedVariants"], 1)

    def test_each_execution_signature_field_prevents_reuse(self):
        changes = {"expectAccept": False, "inputKind": "Module", "mode": "Strict", "webCompatibility": False, "variant": "raw"}
        for field, value in changes.items():
            with self.subTest(field=field):
                original = case()
                changed = copy.deepcopy(original)
                changed[field] = value
                result = self.run_comparison([original], [changed])
                self.assertEqual(result["counts"]["changedVariants"], 1)
                self.assertEqual(result["counts"]["reusableVariants"], 0)
                self.assertIn(field, result["changedFieldCounts"])

    def test_non_signature_metadata_alone_does_not_force_reparse(self):
        original = case()
        changed = copy.deepcopy(original)
        changed.update(description="metadata change outside source", esid="sec-example", features=["class"])
        result = self.run_comparison([original], [changed])
        self.assertEqual(result["counts"]["reusableVariants"], 1)

    def test_delta_retains_latest_order_and_entire_case_payload(self):
        rows = [case("c"), case("b"), case("a")]
        result = self.run_comparison([rows[1]], rows)
        self.assertEqual(self.read(self.output), [rows[0], rows[2]])
        self.assertEqual(result["counts"]["deltaVariants"], 2)

    def test_latest_order_changes_do_not_invalidate_matching_cases(self):
        a, b = case("a"), case("b")
        result = self.run_comparison([a, b], [b, a])
        self.assertEqual(result["counts"]["reusableVariants"], 2)

    def test_duplicate_baseline_ids_are_rejected(self):
        with self.assertRaisesRegex(ValueError, "duplicate baseline"):
            self.run_comparison([case(), case()], [case()])

    def test_duplicate_latest_ids_are_rejected(self):
        with self.assertRaisesRegex(ValueError, "duplicate latest"):
            self.run_comparison([case()], [case(), case()])

    def test_latest_validation_failure_preserves_previous_complete_output(self):
        previous = b"previous completed comparison\n"
        self.output.write_bytes(previous)
        invalid = case("b")
        invalid["source"] = "changed without updating checksum"
        with self.assertRaisesRegex(ValueError, "source hash mismatch"):
            self.run_comparison([case()], [case(), invalid])
        self.assertEqual(self.output.read_bytes(), previous)
        self.assertFalse(self.output.with_name("delta.jsonl.tmp").exists())
        self.assertFalse(self.output.with_name("delta.comparison.jsonl.tmp").exists())

    def test_baseline_source_checksum_is_validated(self):
        invalid = case()
        invalid["sourceSha256"] = "invalid"
        with self.assertRaisesRegex(ValueError, "source hash mismatch"):
            self.run_comparison([invalid], [case()])

    def test_missing_signature_fields_are_rejected(self):
        for field in compare.SIGNATURE_FIELDS:
            with self.subTest(field=field):
                invalid = case()
                del invalid[field]
                with self.assertRaises(ValueError):
                    self.run_comparison([case()], [invalid])

    def test_boolean_fields_require_booleans_instead_of_truthy_values(self):
        for field in ("expectAccept", "webCompatibility"):
            for value in (1, "true", None):
                with self.subTest(field=field, value=value):
                    invalid = case()
                    invalid[field] = value
                    with self.assertRaisesRegex(ValueError, "boolean field"):
                        self.run_comparison([case()], [invalid])

    def test_unknown_mode_and_input_kind_are_rejected(self):
        for field, value in (("mode", "strict"), ("inputKind", "Expression")):
            invalid = case()
            invalid[field] = value
            with self.subTest(field=field), self.assertRaisesRegex(ValueError, "unknown inputKind or mode"):
                self.run_comparison([case()], [invalid])

    def test_empty_inputs_are_reported_with_zero_counts(self):
        result = self.run_comparison([], [])
        self.assertEqual(result["counts"]["baselineVariants"], 0)
        self.assertEqual(result["counts"]["latestVariants"], 0)
        self.assertEqual(result["counts"]["deltaVariants"], 0)

    def test_output_cannot_replace_either_input_or_input_manifest(self):
        self.write(self.baseline, [case()])
        self.write(self.latest, [case()])
        original = self.baseline.read_bytes(), self.latest.read_bytes()
        for output in (self.baseline, self.latest, self.baseline.with_suffix(".manifest.json"), self.latest.with_suffix(".manifest.json")):
            with self.subTest(output=output), self.assertRaisesRegex(ValueError, "must not overwrite"):
                compare.compare(self.baseline, self.latest, output)
        self.assertEqual((self.baseline.read_bytes(), self.latest.read_bytes()), original)

    def test_preparation_manifests_are_checked_and_provenance_retained(self):
        self.write(self.baseline, [case()])
        self.write(self.latest, [case(), case("new")])
        for path, commit in ((self.baseline, "base-commit"), (self.latest, "head-commit")):
            path.with_suffix(".manifest.json").write_text(json.dumps({"casesSha256": compare.file_sha256(path), "snapshot": {"commit": commit}}), encoding="utf-8")
        result = compare.compare(self.baseline, self.latest, self.output)
        self.assertEqual(result["baseline"]["snapshot"]["commit"], "base-commit")
        self.assertEqual(result["latest"]["snapshot"]["commit"], "head-commit")
        self.assertEqual(result["casesSha256"], compare.file_sha256(self.output))
        self.assertEqual(result["comparisonAuditSha256"], compare.file_sha256(self.output.with_suffix(".comparison.jsonl")))

    def test_stale_preparation_manifest_is_rejected(self):
        self.write(self.baseline, [case()])
        self.write(self.latest, [case()])
        self.latest.with_suffix(".manifest.json").write_text(json.dumps({"casesSha256": "stale-hash"}), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "corpus manifest hash mismatch"):
            compare.compare(self.baseline, self.latest, self.output)

    def test_delta_is_reproducible(self):
        a, b = case("a"), case("b")
        self.run_comparison([a], [a, b])
        first = self.output.read_bytes()
        other = self.root / "second.jsonl"
        compare.compare(self.baseline, self.latest, other)
        self.assertEqual(other.read_bytes(), first)

    def test_utf8_bom_in_jsonl_is_supported(self):
        self.write(self.baseline, [case()])
        self.latest.write_text(json.dumps(case()) + "\n", encoding="utf-8-sig")
        result = compare.compare(self.baseline, self.latest, self.output)
        self.assertEqual(result["counts"]["reusableVariants"], 1)

    def test_json_errors_include_file_and_line(self):
        self.write(self.baseline, [])
        self.latest.write_text("\nnot-json\n", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, r"latest.jsonl:2"):
            compare.compare(self.baseline, self.latest, self.output)

    def test_profile_counts_distinguish_reuse_from_required_delta(self):
        a = case("a")
        b = case("b")
        b.update(mode="Strict", variant="strict")
        c = case("c")
        c.update(inputKind="Module", mode="Strict", variant="module")
        result = self.run_comparison([a], [a, b, c])
        self.assertEqual(result["profileCounts"]["reusable"], {"script-nonstrict": 1})
        self.assertEqual(result["profileCounts"]["added"], {"module": 1, "script-strict": 1})


if __name__ == "__main__":
    unittest.main()
