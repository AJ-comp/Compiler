export type WorkspaceSource = { grammar: string; input: string };

export const WORKSPACE_FORMAT = "janglim-workspace";
export const WORKSPACE_VERSION = 1;
export const MAX_WORKSPACE_BYTES = 2 * 1024 * 1024;
export const DEFAULT_WORKSPACE_FILENAME = "janglim-workspace.janglim.json";

export class WorkspaceFileError extends Error {
  readonly reason: "invalid" | "version" | "too-large";
  constructor(reason: "invalid" | "version" | "too-large") {
    super(reason);
    this.reason = reason;
  }
}

function checkSize(text: string) {
  if (new TextEncoder().encode(text).byteLength >= MAX_WORKSPACE_BYTES)
    throw new WorkspaceFileError("too-large");
}

export function serializeWorkspace(source: WorkspaceSource): string {
  const text = `${JSON.stringify(
    {
      format: WORKSPACE_FORMAT,
      version: WORKSPACE_VERSION,
      grammar: source.grammar,
      input: source.input,
    },
    null,
    2,
  )}\n`;
  checkSize(text);
  return text;
}

export function parseWorkspace(text: string): WorkspaceSource {
  checkSize(text);
  let value: unknown;
  try {
    value = JSON.parse(text.replace(/^\uFEFF/, ""));
  } catch {
    throw new WorkspaceFileError("invalid");
  }
  if (
    !value ||
    typeof value !== "object" ||
    !("format" in value) ||
    value.format !== WORKSPACE_FORMAT ||
    !("version" in value) ||
    typeof value.version !== "number" ||
    !("grammar" in value) ||
    typeof value.grammar !== "string" ||
    !("input" in value) ||
    typeof value.input !== "string"
  )
    throw new WorkspaceFileError("invalid");
  if (value.version !== WORKSPACE_VERSION)
    throw new WorkspaceFileError("version");
  return { grammar: value.grammar, input: value.input };
}

export function workspaceDownloadName(name: string): string {
  const basename = name.split(/[\\/]/).pop() || DEFAULT_WORKSPACE_FILENAME;
  const safe = basename.replace(/[<>:"|?*\u0000-\u001F]/g, "_");
  return safe.endsWith(".janglim.json")
    ? safe
    : `${safe.replace(/\.json$/i, "")}.janglim.json`;
}
