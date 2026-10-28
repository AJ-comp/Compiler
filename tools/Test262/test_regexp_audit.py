"""Verify exact static-string decoding and scoped constructor extraction."""
import importlib.util
from pathlib import Path
import tempfile
import unittest

SPEC = importlib.util.spec_from_file_location("regexp_audit", Path(__file__).with_name("audit_regexp_patterns.py"))
audit = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(audit)


class StringTransportTests(unittest.TestCase):
    def test_legacy_escape_lengths(self):
        self.assertEqual(audit.decode_string(r"'\141\400\08\9'"), "a 0\x0089")

    def test_continuations_do_not_add_characters(self):
        self.assertEqual(audit.decode_string("'a\\\r\nb\\\u2028c'"), "abc")

    def test_fixed_and_braced_surrogate_pairs_preserve_same_code_units(self):
        fixed = audit.decode_string(r"'\uD83D\uDE00'")
        scalar = audit.decode_string(r"'\u{1F600}'")
        self.assertEqual(fixed.encode("utf-16-le", "surrogatepass"), scalar.encode("utf-16-le"))

    def test_unmatched_surrogate_is_not_replaced(self):
        self.assertEqual(audit.decode_string(r"'\uD800'"), "\ud800")
        self.assertEqual(audit.decode_string(r"'\uD800'").encode("utf-16-le", "surrogatepass"), b"\x00\xd8")

    def test_incomplete_hex_escape_is_not_invented(self):
        for source in (r"'\x0'", r"'\u000'", r"'\u{}'"):
            with self.assertRaises(ValueError):
                audit.decode_string(source)

    def test_constructor_scope_flags_and_exact_origin(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            directory = root / "test/built-ins/RegExp"
            directory.mkdir(parents=True)
            source = r'''new RegExp("\uD800", "u"); RegExp('a', 'gg'); obj.RegExp('b'); RegExp(computed);'''
            (directory / "case.js").write_text(source, encoding="utf-8")
            cases, strings, skipped = audit.corpus_probes(root, False)
            self.assertEqual(len(cases), 1)
            self.assertEqual(cases[0]["body"], "\ud800")
            self.assertEqual(cases[0]["flags"], "u")
            self.assertEqual(cases[0]["path"], "built-ins/RegExp/case.js")
            self.assertEqual(cases[0]["offset"], 0)
            self.assertFalse(strings)
            self.assertFalse(skipped)


if __name__ == "__main__":
    unittest.main()
