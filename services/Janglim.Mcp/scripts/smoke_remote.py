#!/usr/bin/env python3
"""Verify a deployed MCP endpoint without printing credentials or response bodies.

Usage: python smoke_remote.py https://your-host/mcp
Authentication is read exclusively from JANGLIM_MCP_API_KEY.
"""

import hashlib
import json
import os
from pathlib import Path
import sys
import time
import urllib.error
import urllib.parse
import urllib.request


PROTOCOL_VERSION = "2025-06-18"
ARITHMETIC = """Expr : Expr '+' Term | Term ;
Term : Term '*' Factor | Factor ;
Factor : '(' Expr ')' | id ;
id := "[a-zA-Z]+" ;"""
PRESET_ID = "c11-core"
PRESET_VERSION = "1.0.0"
IF_HEADER = "if_header : 'if' '(' expression ')' ;"
UNLESS_HEADER = "if_header : 'unless' '(' expression ')' ;"


class SmokeFailure(Exception):
    """Contains only fixed check descriptions or numeric HTTP status codes."""


class NoRedirects(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, request, fp, code, message, headers, new_url):
        # Never forward the Bearer credential to a redirected endpoint.
        return None


def require(condition, description):
    if not condition:
        raise SmokeFailure(description)


def verify_preset_metadata(preset):
    require(isinstance(preset, dict)
            and preset.get("id") == PRESET_ID
            and preset.get("version") == PRESET_VERSION
            and preset.get("language") == "C"
            and preset.get("standard") == "C11"
            and bool(preset.get("name")) and bool(preset.get("description")),
            "C11 Core metadata was missing or incorrect.")
    features = preset.get("supportedFeatures")
    limitations = preset.get("limitations")
    require(isinstance(features, list) and bool(features)
            and all(isinstance(item, str) and bool(item) for item in features)
            and isinstance(limitations, list) and bool(limitations)
            and all(isinstance(item, str) and bool(item) for item in limitations)
            and any("typedef" in item.lower() for item in limitations)
            and any("preprocess" in item.lower() for item in limitations),
            "C11 Core support scope or exclusions were missing.")
    digest = preset.get("sha256")
    require(isinstance(digest, str) and len(digest) == 64
            and all(character in "0123456789abcdef" for character in digest),
            "C11 Core SHA-256 metadata was invalid.")


def verify_test_run(result, expected):
    tests = result.get("tests", {})
    cases = tests.get("cases", [])
    require(result.get("status") == "ok" and tests.get("total") == len(expected)
            and tests.get("passed") == len(expected) and tests.get("failed") == 0
            and len(cases) == len(expected),
            "C11 Core examples did not all match their expectations.")
    require(all(case.get("actualSuccess") is success and case.get("passed") is True
                for case, success in zip(cases, expected)),
            "C11 Core example results did not distinguish acceptance from rejection.")


class McpSmoke:
    def __init__(self, url, key):
        parsed = urllib.parse.urlsplit(url)
        require(
            parsed.scheme == "https"
            and bool(parsed.hostname)
            and parsed.username is None
            and parsed.password is None
            and not parsed.query
            and not parsed.fragment
            and parsed.path.rstrip("/") in ("", "/mcp"),
            "Use an HTTPS service URL or /mcp URL without credentials, query, or fragment.",
        )
        require(32 <= len(key) <= 256 and "\r" not in key and "\n" not in key,
                "JANGLIM_MCP_API_KEY must contain 32–256 characters.")
        self.endpoint = urllib.parse.urlunsplit(("https", parsed.netloc, "/mcp", "", ""))
        self.health_url = urllib.parse.urlunsplit(("https", parsed.netloc, "/health", "", ""))
        self.key = key
        self.opener = urllib.request.build_opener(NoRedirects())
        self.request_id = 0
        self.stage = "configuration"

    def send(self, url, payload=None, authenticated=True, origin=None, expected_status=None):
        headers = {"Accept": "application/json, text/event-stream"}
        if authenticated:
            headers["Authorization"] = "Bearer " + self.key
        if origin is not None:
            headers["Origin"] = origin
        data = None
        if payload is not None:
            headers["Content-Type"] = "application/json"
            headers["MCP-Protocol-Version"] = PROTOCOL_VERSION
            data = json.dumps(payload).encode("utf-8")
        request = urllib.request.Request(url, data=data, headers=headers)
        try:
            with self.opener.open(request, timeout=60) as response:
                if expected_status is not None:
                    require(response.status == expected_status, "Unexpected HTTP status.")
                    return None
                body = response.read(2 * 1024 * 1024 + 1)
                require(len(body) <= 2 * 1024 * 1024, "Response exceeded the smoke-test size limit.")
                if not body:
                    return None
                text = body.decode("utf-8")
                if response.headers.get_content_type() == "text/event-stream":
                    for frame in text.replace("\r\n", "\n").split("\n\n"):
                        lines = [line[5:].lstrip() for line in frame.split("\n") if line.startswith("data:")]
                        if lines:
                            event = json.loads("\n".join(lines))
                            if isinstance(event, dict) and "id" in event:
                                return event
                    raise SmokeFailure("SSE contained no JSON-RPC response.")
                return json.loads(text)
        except urllib.error.HTTPError as error:
            status = error.code
            error.close()
            if expected_status == status:
                return None
            raise SmokeFailure("Unexpected HTTP status " + str(status) + ".") from None

    def envelope(self, method, parameters, notification=False):
        result = {"jsonrpc": "2.0", "method": method, "params": parameters}
        if not notification:
            self.request_id += 1
            result["id"] = self.request_id
        return result

    def rpc(self, method, parameters, notification=False):
        payload = self.envelope(method, parameters, notification)
        response = self.send(self.endpoint, payload)
        if notification:
            return None
        require(isinstance(response, dict) and response.get("id") == payload["id"],
                "Missing or mismatched JSON-RPC response ID.")
        require("error" not in response, "Server returned a JSON-RPC error.")
        return response["result"]

    def tool(self, name, arguments):
        result = self.rpc("tools/call", {"name": name, "arguments": arguments})
        require(not result.get("isError", False), "Server returned a tool error.")
        structured = result.get("structuredContent")
        require(isinstance(structured, dict), "Missing structuredContent object.")
        return structured

    def run(self):
        self.stage = "health"
        health = self.send(self.health_url, authenticated=False)
        require(health.get("status") == "ok" and health.get("service") == "janglim-mcp",
                "Health response did not identify a healthy Janglim MCP service.")

        self.stage = "unauthorized access"
        payload = self.envelope("tools/list", {})
        self.send(self.endpoint, payload, authenticated=False, expected_status=401)

        self.stage = "untrusted Origin"
        self.send(self.endpoint, payload, origin="https://untrusted.example", expected_status=403)

        self.stage = "initialization"
        initialized = self.rpc("initialize", {
            "protocolVersion": PROTOCOL_VERSION,
            "capabilities": {},
            "clientInfo": {"name": "janglim-remote-smoke", "version": "1.0"},
        })
        require(initialized.get("protocolVersion") == PROTOCOL_VERSION, "Unexpected negotiated protocol version.")
        self.rpc("notifications/initialized", {}, notification=True)

        self.stage = "tool discovery"
        tools = self.rpc("tools/list", {})["tools"]
        require(len(tools) == 5 and {tool["name"] for tool in tools} == {
            "list_grammars", "get_grammar", "validate_grammar", "parse_input", "run_tests"
        }, "Expected exactly the five Janglim tools.")
        require(all(tool.get("annotations", {}).get("readOnlyHint") is True for tool in tools),
                "Tools are missing read-only annotations.")
        require(all(tool.get("annotations", {}).get("destructiveHint") is False
                    and tool.get("annotations", {}).get("idempotentHint") is True
                    and tool.get("annotations", {}).get("openWorldHint") is False
                    for tool in tools if tool["name"] in ("list_grammars", "get_grammar")),
                "Preset tools are missing immutable read-only annotations.")

        self.stage = "C11 Core catalog"
        listed = self.tool("list_grammars", {})
        grammars = listed.get("grammars", [])
        require(listed.get("status") == "ok" and len(grammars) == 1,
                "Expected the C11 Core grammar in the built-in catalog.")
        metadata = grammars[0]
        verify_preset_metadata(metadata)
        require("grammar" not in metadata, "The catalog unexpectedly included grammar text.")

        self.stage = "C11 Core retrieval and integrity"
        fetched = self.tool("get_grammar", {"grammarId": PRESET_ID})
        require(fetched.get("status") == "ok", "C11 Core retrieval failed.")
        baseline = fetched.get("preset")
        verify_preset_metadata(baseline)
        source = baseline.get("grammar")
        require(isinstance(source, str) and bool(source) and "\r" not in source,
                "C11 Core text was missing or not normalized to LF.")
        require(hashlib.sha256(source.encode("utf-8")).hexdigest() == baseline["sha256"],
                "C11 Core text did not match its SHA-256.")
        require(all(baseline.get(name) == value for name, value in metadata.items()),
                "Catalog and retrieved C11 Core metadata differed.")
        canonical = Path(__file__).resolve().parents[3] / "grammars" / "c" / "c11-core.ebnf"
        if canonical.is_file():
            local_source = canonical.read_text(encoding="utf-8-sig").replace("\r\n", "\n").replace("\r", "\n")
            require(source == local_source, "Deployed C11 Core text differed from the local canonical grammar.")

        self.stage = "unknown preset refusal"
        unknown = self.tool("get_grammar", {"grammarId": "unregistered-preset"})
        require(unknown.get("status") == "unknown_grammar" and unknown.get("preset") is None,
                "An unknown grammar ID was not refused.")

        self.stage = "modified C11 Core validation"
        require(source.count(IF_HEADER) == 1, "Expected exactly one C11 Core if_header rule.")
        edited = source.replace(IF_HEADER, UNLESS_HEADER)
        result = self.tool("validate_grammar", {"grammar": edited})
        grammar = result.get("grammar", {})
        require(result.get("status") == "ok" and grammar.get("isValid") is True
                and grammar.get("hasConflicts") is False and grammar.get("stateCount", 0) > 0
                and not grammar.get("diagnostics") and not grammar.get("conflicts"),
                "The modified C11 Core grammar failed conflict-free validation.")

        self.stage = "modified C11 Core examples"
        result = self.tool("run_tests", {
            "grammar": edited,
            "cases": [
                {"name": "new unless syntax", "input": "int f(int x) { unless (x) return 1; else return 0; }", "expectSuccess": True},
                {"name": "old if syntax", "input": "int f(int x) { if (x) return 1; else return 0; }", "expectSuccess": False},
                {"name": "unchanged while syntax", "input": "int f(int x) { while (x) { x--; } return x; }", "expectSuccess": True},
            ],
        })
        verify_test_run(result, [True, False, True])

        self.stage = "C11 Core baseline immutability"
        fetched_again = self.tool("get_grammar", {"grammarId": PRESET_ID})
        require(fetched_again.get("status") == "ok" and fetched_again.get("preset") == baseline,
                "User modifications changed the built-in C11 Core baseline.")

        self.stage = "grammar validation"
        result = self.tool("validate_grammar", {"grammar": ARITHMETIC})
        grammar = result["grammar"]
        require(result["status"] == "ok" and grammar["isValid"] is True
                and grammar["hasConflicts"] is False and grammar["stateCount"] > 0,
                "Arithmetic grammar failed validation.")

        self.stage = "parsing and tree positions"
        result = self.tool("parse_input", {"grammar": ARITHMETIC, "input": "a + b * c", "includeTree": True})
        parse = result["parse"]
        tree = parse.get("tree", [])
        require(result["status"] == "ok" and parse["success"] is True and bool(tree)
                and parse["treeTruncated"] is False and tree[0]["id"] == 0,
                "Expression did not produce a complete syntax tree.")
        ids = {node["id"] for node in tree}
        require(all(child in ids for node in tree for child in node["children"]),
                "Syntax tree contained a dangling child reference.")
        require(any(node.get("text") == "b" and node.get("start") == 4 and node.get("end") == 5 for node in tree),
                "Syntax tree token offsets were incorrect.")

        self.stage = "positive and negative examples"
        result = self.tool("run_tests", {
            "grammar": ARITHMETIC,
            "cases": [
                {"name": "valid", "input": "a + b * c", "expectSuccess": True},
                {"name": "missing operand", "input": "a +", "expectSuccess": False},
            ],
        })
        tests = result["tests"]
        require(result["status"] == "ok" and tests["total"] == 2 and tests["passed"] == 2 and tests["failed"] == 0,
                "Positive/negative examples did not both pass.")
        require(len(tests["cases"]) == 2 and tests["cases"][0]["actualSuccess"] is True
                and tests["cases"][1]["actualSuccess"] is False,
                "Example results did not distinguish acceptance from rejection.")

        self.stage = "conflict refusal"
        result = self.tool("parse_input", {
            "grammar": "Expr : Expr '+' Expr | id ; id := \"[a-zA-Z]+\" ;",
            "input": "a + b + c",
        })
        require(result["status"] == "grammar_conflict" and result["grammar"]["hasConflicts"] is True
                and bool(result["grammar"]["conflicts"]) and result.get("parse") is None,
                "Conflicted grammar was not refused.")


def main():
    started = time.monotonic()
    smoke = None
    try:
        require(len(sys.argv) == 2, "Usage: python smoke_remote.py https://your-host/mcp")
        key = os.environ.get("JANGLIM_MCP_API_KEY", "")
        smoke = McpSmoke(sys.argv[1], key)
        smoke.run()
    except SmokeFailure as error:
        stage = smoke.stage if smoke is not None else "configuration"
        print("FAIL: " + stage + " - " + str(error) + " (%.2fs)" % (time.monotonic() - started), file=sys.stderr)
        return 1
    except Exception as error:
        # Network/JSON exceptions can include URLs, headers, or body fragments. Never print them.
        stage = smoke.stage if smoke is not None else "configuration"
        print("FAIL: " + stage + " (" + type(error).__name__ + "; %.2fs)" % (time.monotonic() - started), file=sys.stderr)
        return 1
    print("PASS: 15 checks (health, auth, Origin, initialize, tools, C11 catalog, integrity, unknown ID, "
          "modified grammar, modified examples, immutable baseline, validate, parse/tree, examples, conflicts); %.2fs"
          % (time.monotonic() - started))
    return 0


if __name__ == "__main__":
    sys.exit(main())
