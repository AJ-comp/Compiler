import assert from "node:assert/strict";
import test from "node:test";
import {
  MAX_WORKSPACE_BYTES,
  WorkspaceFileError,
  parseWorkspace,
  serializeWorkspace,
  workspaceDownloadName,
} from "../src/workspace-file.ts";

const source = {
  grammar: `식 : 이름 '+' 이름 ;\r\n이름 := "[가-힣]+" ;\n`,
  input: "  하나 + 둘\t\n🙂 $& {count}",
};
const document = { format: "janglim-workspace", version: 1, ...source };
const rejectsWith = (reason) => (error) =>
  error instanceof WorkspaceFileError && error.reason === reason;

test("workspace round trips preserve Unicode, whitespace, line endings, and empty input", () => {
  for (const original of [source, { grammar: "", input: "" }]) {
    const saved = serializeWorkspace(original);
    assert.deepEqual(parseWorkspace(saved), original);
    assert.deepEqual(Object.keys(JSON.parse(saved)), [
      "format",
      "version",
      "grammar",
      "input",
    ]);
  }
});

test("a UTF-8 BOM from an external editor is accepted", () => {
  assert.deepEqual(parseWorkspace(`\uFEFF${JSON.stringify(document)}`), source);
});

test("malformed JSON, unrelated JSON, and non-string source fields are rejected", () => {
  for (const text of [
    "",
    "{",
    "null",
    "[]",
    '"workspace"',
    "true",
    JSON.stringify({ ...document, format: "other" }),
    JSON.stringify({ grammar: source.grammar, input: source.input }),
    JSON.stringify({ ...document, version: "1" }),
    JSON.stringify({ ...document, grammar: null }),
    JSON.stringify({ ...document, input: [] }),
  ])
    assert.throws(() => parseWorkspace(text), rejectsWith("invalid"));
});

test("future and legacy schema versions are explicitly rejected", () => {
  for (const version of [0, 2, 1.5])
    assert.throws(
      () => parseWorkspace(JSON.stringify({ ...document, version })),
      rejectsWith("version"),
    );
});

test("unknown metadata is ignored and cannot be exported as application state", () => {
  const original = { ...document, theme: "dark", language: "ja", results: [1] };
  const imported = parseWorkspace(JSON.stringify(original));
  assert.deepEqual(imported, source);
  assert.deepEqual(JSON.parse(serializeWorkspace(original)), document);
});

test("the same UTF-8 byte limit protects both import and export", () => {
  assert.throws(
    () => parseWorkspace(" ".repeat(MAX_WORKSPACE_BYTES)),
    rejectsWith("too-large"),
  );
  assert.throws(
    () =>
      serializeWorkspace({
        grammar: "가".repeat(MAX_WORKSPACE_BYTES / 2),
        input: "",
      }),
    rejectsWith("too-large"),
  );
  const prefixBytes = new TextEncoder().encode(
    JSON.stringify({
      format: "janglim-workspace",
      version: 1,
      grammar: "",
      input: "",
    }),
  ).length;
  const justUnder = JSON.stringify({
    format: "janglim-workspace",
    version: 1,
    grammar: "a".repeat(MAX_WORKSPACE_BYTES - prefixBytes - 1),
    input: "",
  });
  assert.equal(
    new TextEncoder().encode(justUnder).length,
    MAX_WORKSPACE_BYTES - 1,
  );
  assert.equal(parseWorkspace(justUnder).input, "");
});

test("download names keep the workspace suffix and discard path and control characters", () => {
  assert.equal(
    workspaceDownloadName("計算機.janglim.json"),
    "計算機.janglim.json",
  );
  assert.equal(
    workspaceDownloadName("C:\\fakepath\\calculator.json"),
    "calculator.janglim.json",
  );
  assert.equal(workspaceDownloadName("/tmp/a\n*.json"), "a__.janglim.json");
});
