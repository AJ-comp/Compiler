"use strict";

// Development comparison oracle only. The supported baseline is TypeScript 7.0.2;
// this older JavaScript API cannot substitute for the pinned native parser.
const path = require("node:path");
const readline = require("node:readline");
const crypto = require("node:crypto");

const modulePath = process.argv[2];
if (!modulePath) {
  process.stderr.write("Usage: node typescript6-parse-oracle.cjs <path/to/typescript.js>\n");
  process.exit(2);
}
const ts = require(path.resolve(modulePath));
if (ts.version !== "6.0.3") {
  process.stderr.write(`Expected pinned TypeScript 6.0.3, received ${ts.version}\n`);
  process.exit(2);
}

function evaluate(request) {
  const extension = { ts: "ts", tsx: "tsx", dts: "d.ts", declaration: "d.ts", mts: "mts", cts: "cts" };
  const kind = (request.kind || "ts").toLowerCase();
  if (!Object.hasOwn(extension, kind)) throw new Error(`Unsupported input kind: ${kind}`);
  if (typeof request.source !== "string") throw new Error("source must be a string");
  const fileName = request.fileName || `/input.${extension[kind]}`;
  const sourceFile = ts.createSourceFile(fileName, request.source, ts.ScriptTarget.Latest, true);
  return {
    id: request.id,
    version: ts.version,
    revision: "050880ce59e30b356b686bd3144efe24f875ebc8",
    phase: "parse",
    offsetUnit: "UTF-16 code units",
    fileName,
    sourceSha256: crypto.createHash("sha256").update(request.source, "utf8").digest("hex"),
    accept: sourceFile.parseDiagnostics.length === 0,
    diagnostics: sourceFile.parseDiagnostics.map(d => ({
      code: d.code, start: d.start, length: d.length,
      message: ts.flattenDiagnosticMessageText(d.messageText, "\n")
    }))
  };
}

(async () => {
  for await (const line of readline.createInterface({ input: process.stdin, crlfDelay: Infinity })) {
    process.stdout.write(`${JSON.stringify(evaluate(JSON.parse(line)))}\n`);
  }
})().catch(error => {
  process.stderr.write(`${error.stack}\n`);
  process.exitCode = 2;
});
