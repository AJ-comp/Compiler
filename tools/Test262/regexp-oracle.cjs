// Compile Pattern grammar only. No RegExp matching or JavaScript source is run.
const readline = require("node:readline");
const { createHash } = require("node:crypto");
readline.createInterface({ input: process.stdin }).on("line", line => {
  const test = JSON.parse(line);
  const body = Buffer.from(test.bodyUtf16, "base64").toString("utf16le");
  const bodySha256 = createHash("sha256").update(Buffer.from(body, "utf16le")).digest("hex");
  let accepted = false, error = null, unexpectedException = null;
  try {
    new RegExp(body, test.flags);
    accepted = true;
  } catch (exception) {
    error = exception.message;
    if (!(exception instanceof SyntaxError)) unexpectedException = exception.name;
  }
  process.stdout.write(JSON.stringify({ id: test.id, accepted, error, unexpectedException, bodySha256 }) + "\n");
});
