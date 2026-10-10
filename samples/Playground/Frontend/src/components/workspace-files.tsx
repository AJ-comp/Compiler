import { useEffect, useRef, useState, type ChangeEvent } from "react";
import { AlertDialog } from "radix-ui";
import { ChevronDown, Download, FileJson2, FolderOpen } from "lucide-react";
import { Button } from "./ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "./ui/dropdown-menu";
import { useI18n } from "../i18n";
import {
  DEFAULT_WORKSPACE_FILENAME,
  MAX_WORKSPACE_BYTES,
  WorkspaceFileError,
  parseWorkspace,
  serializeWorkspace,
  workspaceDownloadName,
  type WorkspaceSource,
} from "../workspace-file";

export type FileNotice = { key: string; name?: string; error?: boolean };
type PendingFile = { source: WorkspaceSource; name: string };

function fileErrorKey(error: unknown, operation: "read" | "save") {
  if (error instanceof WorkspaceFileError) {
    if (error.reason === "too-large")
      return "Choose a workspace file smaller than 2 MB.";
    if (error.reason === "version")
      return "This file uses an unsupported workspace version.";
    return "This is not a valid Janglim workspace file.";
  }
  return operation === "read"
    ? "Unable to read this file. Please try again."
    : "Unable to save the file. Please try again.";
}

export function WorkspaceFiles({
  source,
  disabled,
  onOpen,
  onNotice,
  onBusyChange,
  onFocusSource,
}: {
  source: WorkspaceSource;
  disabled: boolean;
  onOpen: (source: WorkspaceSource) => void;
  onNotice: (notice: FileNotice | null) => void;
  onBusyChange: (busy: boolean) => void;
  onFocusSource: () => void;
}) {
  const { t } = useI18n();
  const [checkpoint, setCheckpoint] = useState(source);
  const [filename, setFilename] = useState(DEFAULT_WORKSPACE_FILENAME);
  const [pending, setPending] = useState<PendingFile | null>(null);
  const [busy, setBusy] = useState(false);
  const fileInput = useRef<HTMLInputElement>(null);
  const trigger = useRef<HTMLButtonElement>(null);
  const latestSource = useRef(source);
  useEffect(() => {
    latestSource.current = source;
  }, [source]);

  const finish = () => {
    setBusy(false);
    onBusyChange(false);
  };
  const open = (file: PendingFile) => {
    setCheckpoint(file.source);
    setFilename(workspaceDownloadName(file.name));
    setPending(null);
    finish();
    onOpen(file.source);
    onNotice({ key: "Opened {name}.", name: file.name });
  };
  const readFile = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file || disabled || busy) return;
    setBusy(true);
    onBusyChange(true);
    onNotice(null);
    try {
      if (file.size >= MAX_WORKSPACE_BYTES)
        throw new WorkspaceFileError("too-large");
      const next = {
        source: parseWorkspace(await file.text()),
        name: file.name,
      };
      const current = latestSource.current;
      if (
        current.grammar !== checkpoint.grammar ||
        current.input !== checkpoint.input
      ) {
        setPending(next);
      } else {
        open(next);
      }
    } catch (error) {
      onNotice({ key: fileErrorKey(error, "read"), error: true });
      finish();
    }
  };
  const save = () => {
    if (disabled || busy) return;
    let url: string | undefined;
    try {
      const text = serializeWorkspace(source);
      url = URL.createObjectURL(new Blob([text], { type: "application/json" }));
      const link = document.createElement("a");
      link.href = url;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      link.remove();
      // Keep the URL alive until the browser has started consuming the download.
      window.setTimeout(() => URL.revokeObjectURL(url!), 1000);
      setCheckpoint(source);
      onNotice({ key: "Saved {name}.", name: filename });
    } catch (error) {
      if (url) URL.revokeObjectURL(url);
      onNotice({ key: fileErrorKey(error, "save"), error: true });
    }
  };

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            ref={trigger}
            variant="outline"
            className="examples-trigger"
            disabled={disabled || busy}
          >
            <FileJson2 size={16} />
            <span>{t("File")}</span>
            <ChevronDown size={14} />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="workspace-file-menu">
          <DropdownMenuLabel>
            {t("Grammar and input in one file")}
          </DropdownMenuLabel>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            disabled={disabled || busy}
            onSelect={() => fileInput.current?.click()}
          >
            <FolderOpen size={16} /> {t("Open file…")}
          </DropdownMenuItem>
          <DropdownMenuItem disabled={disabled || busy} onSelect={save}>
            <Download size={16} /> {t("Save file")}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      <input
        ref={fileInput}
        type="file"
        accept=".json,application/json"
        hidden
        aria-label={t("Open file…")}
        onChange={(event) => void readFile(event)}
      />
      <AlertDialog.Root
        open={!!pending}
        onOpenChange={(opened) => {
          if (!opened) {
            setPending(null);
            finish();
          }
        }}
      >
        <AlertDialog.Portal>
          <AlertDialog.Overlay className="workspace-dialog-overlay" />
          <AlertDialog.Content
            className="workspace-dialog"
            onCloseAutoFocus={(event) => {
              event.preventDefault();
              if (trigger.current?.disabled) onFocusSource();
              else trigger.current?.focus();
            }}
          >
            <AlertDialog.Title>
              {t("Replace your current work?")}
            </AlertDialog.Title>
            <AlertDialog.Description>
              {t(
                "You have unsaved changes. Save your file before opening another workspace.",
              )}
            </AlertDialog.Description>
            <div className="workspace-dialog-actions">
              <AlertDialog.Cancel asChild>
                <Button variant="outline">{t("Cancel")}</Button>
              </AlertDialog.Cancel>
              <AlertDialog.Action asChild>
                <Button onClick={() => pending && open(pending)}>
                  {t("Replace workspace")}
                </Button>
              </AlertDialog.Action>
            </div>
          </AlertDialog.Content>
        </AlertDialog.Portal>
      </AlertDialog.Root>
    </>
  );
}
