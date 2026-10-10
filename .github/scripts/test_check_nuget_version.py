"""Network-free checks for the release pipeline's NuGet availability gate."""

import importlib.util
import io
import json
from pathlib import Path
import socket
import ssl
import unittest
from unittest.mock import patch
from urllib.error import HTTPError, URLError


spec = importlib.util.spec_from_file_location(
    "check_nuget_version", Path(__file__).with_name("check-nuget-version.py")
)
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)


def index(versions):
    return io.BytesIO(json.dumps({"versions": versions}).encode())


def http_error(status):
    return HTTPError("https://api.nuget.org/", status, "test response", {}, None)


class NuGetVersionTests(unittest.TestCase):
    def setUp(self):
        self.open = patch.object(check, "urlopen").start()
        self.sleep = patch.object(check.time, "sleep").start()
        self.addCleanup(patch.stopall)

    def test_existing_version_is_case_insensitive(self):
        self.open.return_value = index(["0.3.0-PREVIEW.2", "0.2.0"])
        self.assertTrue(check.version_exists("Janglim", "0.3.0-preview.2"))
        request = self.open.call_args.args[0]
        self.assertEqual(request.full_url, "https://api.nuget.org/v3-flatcontainer/janglim/index.json")
        self.assertEqual(self.open.call_args.kwargs["timeout"], check.TIMEOUT_SECONDS)

    def test_missing_version_is_false(self):
        self.open.return_value = index(["0.3.0-preview.2"])
        self.assertFalse(check.version_exists("Janglim", "0.3.0"))

    def test_package_not_found_is_false_without_retry(self):
        self.open.side_effect = http_error(404)
        self.assertFalse(check.version_exists("Janglim", "0.3.0"))
        self.assertEqual(self.open.call_count, 1)
        self.sleep.assert_not_called()

    def test_server_error_and_rate_limit_are_retried(self):
        for status in (500, 503, 429):
            with self.subTest(status=status):
                self.open.reset_mock()
                self.sleep.reset_mock()
                self.open.side_effect = [http_error(status), index(["0.3.0"])]
                self.assertTrue(check.version_exists("Janglim", "0.3.0"))
                self.assertEqual(self.open.call_count, 2)
                self.sleep.assert_called_once_with(1)

    def test_temporary_network_errors_are_retried(self):
        for reason in (TimeoutError(), ConnectionResetError(), socket.gaierror(socket.EAI_AGAIN, "temporary")):
            with self.subTest(reason=type(reason).__name__):
                self.open.reset_mock()
                self.open.side_effect = [URLError(reason), index([])]
                self.assertFalse(check.version_exists("Janglim", "0.3.0"))
                self.assertEqual(self.open.call_count, 2)

    def test_exhausted_retries_fail_closed(self):
        for failure in (http_error(500), URLError(TimeoutError())):
            with self.subTest(failure=type(failure).__name__):
                self.open.reset_mock()
                self.sleep.reset_mock()
                self.open.side_effect = failure
                with self.assertRaisesRegex(RuntimeError, "failed after 3 attempts"):
                    check.version_exists("Janglim", "0.3.0")
                self.assertEqual(self.open.call_count, 3)
                self.assertEqual([call.args[0] for call in self.sleep.call_args_list], [1, 2])

    def test_other_http_errors_fail_without_retry(self):
        for status in (400, 401, 403):
            with self.subTest(status=status):
                self.open.reset_mock()
                self.open.side_effect = http_error(status)
                with self.assertRaisesRegex(RuntimeError, f"HTTP {status}"):
                    check.version_exists("Janglim", "0.3.0")
                self.assertEqual(self.open.call_count, 1)

    def test_permanent_network_errors_fail_without_retry(self):
        for reason in (ssl.SSLCertVerificationError(), socket.gaierror(socket.EAI_NONAME, "not found")):
            with self.subTest(reason=type(reason).__name__):
                self.open.reset_mock()
                self.open.side_effect = URLError(reason)
                with self.assertRaisesRegex(RuntimeError, "non-retryable"):
                    check.version_exists("Janglim", "0.3.0")
                self.assertEqual(self.open.call_count, 1)

    def test_malformed_json_and_schema_fail_without_retry(self):
        for payload in (b"not JSON", b"\xff", b"[]", b"{}", b'{"versions":"0.3.0"}', b'{"versions":[3]}'):
            with self.subTest(payload=payload):
                self.open.reset_mock()
                self.open.side_effect = None
                self.open.return_value = io.BytesIO(payload)
                with self.assertRaises(RuntimeError):
                    check.version_exists("Janglim", "0.3.0")
                self.assertEqual(self.open.call_count, 1)

    def test_invalid_arguments_never_request_network(self):
        for package, version in (("../Janglim", "0.3.0"), ("", "0.3.0"), ("x" * 101, "0.3.0"), ("Janglim", "0.3.0\n"), ("Janglim", "release"), ("Janglim", "0.3.0;echo")):
            with self.subTest(package=package, version=version):
                with self.assertRaises(ValueError):
                    check.version_exists(package, version)
        self.open.assert_not_called()

    def test_cli_outputs_only_github_output_assignment(self):
        for versions, expected in ((["0.3.0"], "exists=true\n"), ([], "exists=false\n")):
            with self.subTest(expected=expected):
                self.open.return_value = index(versions)
                output = io.StringIO()
                with patch.object(check.sys, "stdout", output):
                    self.assertEqual(check.main(["Janglim", "0.3.0"]), 0)
                self.assertEqual(output.getvalue(), expected)

    def test_cli_failure_has_no_exists_output(self):
        self.open.side_effect = http_error(403)
        output, errors = io.StringIO(), io.StringIO()
        with patch.object(check.sys, "stdout", output), patch.object(check.sys, "stderr", errors):
            self.assertEqual(check.main(["Janglim", "0.3.0"]), 1)
        self.assertEqual(output.getvalue(), "")
        self.assertIn("HTTP 403", errors.getvalue())


if __name__ == "__main__":
    unittest.main()
