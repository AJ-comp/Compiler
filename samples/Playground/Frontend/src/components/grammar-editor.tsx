import { useEffect, useRef, useState } from "react";
import { useI18n } from "../i18n";
import {
  SourceTextarea,
  validSourceRanges,
  type SourceRange,
} from "./source-textarea";

type EditorRange = {
  startLineNumber: number;
  startColumn: number;
  endLineNumber: number;
  endColumn: number;
};
type Decoration = {
  range: EditorRange;
  options: { inlineClassName: string; stickiness: number };
};
type DecorationCollection = {
  set(decorations: Decoration[]): void;
  clear(): void;
};

type Editor = {
  getValue(): string;
  setValue(value: string): void;
  onDidChangeModelContent(callback: () => void): { dispose(): void };
  getModel(): {
    getPositionAt(offset: number): { lineNumber: number; column: number };
  } | null;
  createDecorationsCollection(decorations?: Decoration[]): DecorationCollection;
  revealRangeInCenterIfOutsideViewport(range: EditorRange): void;
  updateOptions(options: { ariaLabel: string }): void;
  dispose(): void;
};
declare global {
  interface Window {
    monaco?: {
      editor: {
        create(element: HTMLElement, options: Record<string, unknown>): Editor;
        setTheme(theme: string): void;
        defineTheme(name: string, options: Record<string, unknown>): void;
      };
    };
  }
}

export function GrammarEditor({
  value,
  onChange,
  dark,
  highlightRanges = [],
  selectedSymbol,
}: {
  value: string;
  onChange(value: string): void;
  dark: boolean;
  highlightRanges?: SourceRange[];
  selectedSymbol?: string;
}) {
  const { t } = useI18n();
  const ariaLabel = t("EBNF grammar editor");
  const container = useRef<HTMLDivElement>(null);
  const editor = useRef<Editor | null>(null);
  const decorations = useRef<DecorationCollection | null>(null);
  const latest = useRef({ value, onChange, ariaLabel });
  latest.current = { value, onChange, ariaLabel };
  const [ready, setReady] = useState(false);
  const ranges = validSourceRanges(value, highlightRanges);
  const highlightKey = ranges
    .map(({ start, end }) => `${start}:${end}`)
    .join(",");

  useEffect(() => {
    let subscription: { dispose(): void } | undefined;
    const initialize = () => {
      if (!window.monaco?.editor || !container.current || editor.current)
        return;
      for (const isDark of [false, true]) {
        window.monaco.editor.defineTheme(
          isDark ? "janglim-dark" : "janglim-light",
          {
            base: isDark ? "vs-dark" : "vs",
            inherit: true,
            rules: [
              { token: "type", foreground: isDark ? "B9A3FF" : "6941C6" },
              { token: "string", foreground: isDark ? "86D3BB" : "087F5B" },
              { token: "regexp", foreground: isDark ? "ECC180" : "B45F16" },
              { token: "comment", foreground: "8A8F98" },
              { token: "operators", foreground: isDark ? "CFD3DB" : "697386" },
            ],
            colors: {
              "editor.background": isDark ? "#1B1D25" : "#FFFFFF",
              "editorLineNumber.foreground": "#8A8F98",
              "editor.lineHighlightBackground": isDark ? "#242630" : "#F8F7FC",
              "editor.selectionBackground": isDark ? "#43375E" : "#E8DFFF",
            },
          },
        );
      }
      editor.current = window.monaco.editor.create(container.current, {
        value: latest.current.value,
        language: "ebnf",
        theme: "janglim-light",
        automaticLayout: true,
        fontFamily: "'JetBrains Mono', 'Cascadia Code', Consolas, monospace",
        fontSize: 13,
        lineHeight: 25,
        minimap: { enabled: false },
        scrollBeyondLastLine: false,
        wordWrap: "on",
        lineNumbersMinChars: 2,
        folding: false,
        glyphMargin: false,
        overviewRulerLanes: 0,
        padding: { top: 20, bottom: 20 },
        renderLineHighlight: "none",
        tabSize: 2,
        ariaLabel: latest.current.ariaLabel,
        scrollbar: { verticalScrollbarSize: 5, horizontalScrollbarSize: 5 },
      });
      decorations.current = editor.current.createDecorationsCollection();
      subscription = editor.current.onDidChangeModelContent(() => {
        decorations.current?.clear();
        latest.current.onChange(editor.current!.getValue());
      });
      setReady(true);
    };
    initialize();
    const poll = window.setInterval(initialize, 100);
    return () => {
      window.clearInterval(poll);
      subscription?.dispose();
      decorations.current?.clear();
      decorations.current = null;
      editor.current?.dispose();
      editor.current = null;
    };
  }, []);
  useEffect(() => {
    if (editor.current && editor.current.getValue() !== value)
      editor.current.setValue(value);
  }, [value]);
  useEffect(() => {
    if (ready)
      window.monaco?.editor.setTheme(dark ? "janglim-dark" : "janglim-light");
  }, [dark, ready]);
  useEffect(() => {
    editor.current?.updateOptions({ ariaLabel });
  }, [ariaLabel, ready]);
  useEffect(() => {
    const instance = editor.current;
    const model = instance?.getModel();
    if (!instance || !model || !decorations.current) return;
    const nextDecorations: Decoration[] = ranges.map(({ start, end }) => {
      const first = model.getPositionAt(start);
      const last = model.getPositionAt(end);
      return {
        range: {
          startLineNumber: first.lineNumber,
          startColumn: first.column,
          endLineNumber: last.lineNumber,
          endColumn: last.column,
        },
        options: {
          inlineClassName: "grammar-source-highlight",
          // Never grow a source highlight when text is inserted at either edge.
          stickiness: 1,
        },
      };
    });
    decorations.current.set(nextDecorations);
    if (nextDecorations.length > 0)
      instance.revealRangeInCenterIfOutsideViewport(nextDecorations[0].range);
  }, [ready, value, highlightKey]);

  return (
    <div className="grammar-editor">
      <div className="monaco-host" ref={container} />
      {!ready && (
        <SourceTextarea
          ariaLabel={ariaLabel}
          value={value}
          onChange={onChange}
          ranges={ranges}
          selectedSymbol={selectedSymbol}
          className="source-textarea--grammar"
          textareaClassName="editor-fallback"
          announce={false}
        />
      )}
      <span
        className="source-highlight-status"
        role="status"
        aria-live="polite"
        aria-atomic="true"
      >
        {selectedSymbol && ranges.length > 0
          ? t("Highlighted grammar source for {symbol}.", {
              symbol: selectedSymbol,
            })
          : ""}
      </span>
    </div>
  );
}
