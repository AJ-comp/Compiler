import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  ArrowDown,
  ArrowRight,
  BookOpen,
  Check,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  Code2,
  Command,
  Copy,
  GitFork,
  GitBranch,
  Layers3,
  List,
  LoaderCircle,
  Maximize2,
  Minus,
  Moon,
  Pause,
  Play,
  Plus,
  RotateCcw,
  Search,
  ShieldCheck,
  SkipBack,
  SkipForward,
  StepBack,
  StepForward,
  Sun,
  Table2,
  TriangleAlert,
  X,
  XCircle,
} from "lucide-react";
import { Button } from "./components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "./components/ui/tooltip";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "./components/ui/dropdown-menu";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./components/ui/tabs";
import { GrammarEditor } from "./components/grammar-editor";
import { InputEditor } from "./components/input-editor";
import { LanguagePicker } from "./components/language-picker";
import { WorkspaceFiles, type FileNotice } from "./components/workspace-files";
import type { WorkspaceSource } from "./workspace-file";
import {
  ParseTree,
  type StackFrame,
  type TreeNode,
} from "./components/parse-tree";
import Preloader from "@licensed/preloader";
import { WorkspaceShell } from "./components/workspace-shell";
import type { ParserBridge } from "./main";
import { useI18n } from "./i18n";
import { version as playgroundVersion } from "../package.json";

const DEFAULT_GRAMMAR = `Expr   : Expr '+' Term | Term ;\nTerm   : Term '*' Factor | Factor ;\nFactor : '(' Expr ')' | id ;\nid     := "[a-zA-Z]+" ;`;
const EXAMPLES = [
  {
    id: "arithmetic",
    name: "Arithmetic expressions",
    description: "Precedence & nested expressions",
    grammar: DEFAULT_GRAMMAR,
    input: "a + a * a",
  },
  {
    id: "parenthesized",
    name: "Parenthesized expression",
    description: "Explore a deeper syntax tree",
    grammar: DEFAULT_GRAMMAR,
    input: "(a + b) * c",
  },
  {
    id: "ambiguous",
    name: "Ambiguous grammar",
    description: "Inspect shift/reduce conflicts",
    grammar: `Expr : Expr '+' Expr | id ;\nid := "[a-zA-Z]+" ;`,
    input: "a + b + c",
  },
  {
    id: "rejected",
    name: "Rejected input",
    description: "Follow the parser to an error",
    grammar: DEFAULT_GRAMMAR,
    input: "a + * a",
  },
];
type Action = { kind: string; target: string };
type Step = {
  index: number;
  action: Action | null;
  lookahead: string | null;
  stackBefore: StackFrame[];
  stackAfter: StackFrame[];
  isError: boolean;
  errorMessage: string | null;
  recoveryMessage: string | null;
};
type Entry = Action & { state: number; symbol: string };
type Session = {
  success: boolean;
  grammarErrors: string[];
  conflicts: { state: number; kind: string }[];
  steps: Step[];
  entries: Entry[];
  tokenCount: number;
  stateCount: number;
  elapsedMs: number;
};
type View = "tree" | "table" | "trace";

function findTreeNode(stack: StackFrame[], id: string): TreeNode | null {
  const pending = stack.flatMap((frame) =>
    frame.symbol ? [frame.symbol] : [],
  );
  while (pending.length) {
    const node = pending.pop()!;
    if (node.id === id) return node;
    pending.push(...node.children);
  }
  return null;
}

function IconButton({
  label,
  children,
  ...props
}: React.ComponentProps<typeof Button> & { label: string }) {
  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <Button variant="ghost" size="icon" aria-label={label} {...props}>
          {children}
        </Button>
      </TooltipTrigger>
      <TooltipContent>{label}</TooltipContent>
    </Tooltip>
  );
}
function ActionBadge({ kind }: { kind?: string }) {
  return (
    <span
      className={`action-badge action-${kind?.replace("ε-", "") ?? "idle"}`}
    >
      {kind || "—"}
    </span>
  );
}

export function Playground({ bridge }: { bridge: ParserBridge }) {
  const { t } = useI18n();
  const [grammar, setGrammar] = useState(DEFAULT_GRAMMAR);
  const [input, setInput] = useState("a + a * a");
  const [example, setExample] = useState(EXAMPLES[0].id);
  const [session, setSession] = useState<Session | null>(null);
  const [lastSource, setLastSource] = useState({
    grammar: DEFAULT_GRAMMAR,
    input: "a + a * a",
  });
  const [position, setPosition] = useState(0);
  const [playing, setPlaying] = useState(false);
  const [speed, setSpeed] = useState(600);
  const [running, setRunning] = useState(false);
  const [runError, setRunError] = useState<{
    message: string;
    authored: boolean;
  } | null>(null);
  const [view, setView] = useState<View>("tree");
  const [zoom, setZoom] = useState(1);
  const [filter, setFilter] = useState("");
  const [expanded, setExpanded] = useState(false);
  const [copied, setCopied] = useState(false);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [fileBusy, setFileBusy] = useState(false);
  const [fileNotice, setFileNotice] = useState<FileNotice | null>(null);
  const [dark, setDark] = useState(() => {
    try {
      return localStorage.getItem("janglim-theme") === "dark";
    } catch {
      return false;
    }
  });
  const runningRef = useRef(false);
  const fileBusyRef = useRef(false);
  const changeFileBusy = (busy: boolean) => {
    fileBusyRef.current = busy;
    setFileBusy(busy);
  };
  const steps = session?.steps ?? [];
  const current = steps[position];
  const dirty =
    !!session && (lastSource.grammar !== grammar || lastSource.input !== input);
  const selectedNode = useMemo(
    () =>
      selectedNodeId && current && !dirty && !running
        ? findTreeNode(current.stackAfter, selectedNodeId)
        : null,
    [current, selectedNodeId, dirty, running],
  );

  useEffect(() => {
    if (selectedNodeId && !selectedNode) setSelectedNodeId(null);
  }, [selectedNodeId, selectedNode]);

  const clearSelection = useCallback(() => setSelectedNodeId(null), []);
  const selectNode = (node: TreeNode) => {
    setPlaying(false);
    setSelectedNodeId((previous) => (previous === node.id ? null : node.id));
  };

  const run = useCallback(
    async (nextGrammar = grammar, nextInput = input) => {
      if (runningRef.current || fileBusyRef.current) return;
      runningRef.current = true;
      setRunning(true);
      setPlaying(false);
      setRunError(null);
      setSelectedNodeId(null);
      setSession(null);
      setPosition(0);
      try {
        await new Promise<void>((resolve) =>
          requestAnimationFrame(() => resolve()),
        );
        const response = await bridge.invokeMethodAsync(
          "Parse",
          nextGrammar,
          nextInput,
        );
        const result: Session =
          typeof response === "string" ? JSON.parse(response) : response;
        setSession(result);
        setPosition(Math.max(0, result.steps.length - 1));
        setLastSource({ grammar: nextGrammar, input: nextInput });
      } catch (error) {
        setRunError(
          error instanceof Error
            ? { message: error.message, authored: false }
            : {
                message: "The parser could not complete this run.",
                authored: true,
              },
        );
      } finally {
        setRunning(false);
        runningRef.current = false;
      }
    },
    [bridge, grammar, input],
  );

  useEffect(() => {
    void run(DEFAULT_GRAMMAR, "a + a * a");
  }, [bridge]);
  useEffect(() => {
    const handler = (event: KeyboardEvent) => {
      if (
        !event.defaultPrevented &&
        !event.isComposing &&
        event.keyCode !== 229 &&
        (event.ctrlKey || event.metaKey) &&
        event.key === "Enter"
      ) {
        event.preventDefault();
        if (!fileBusy) void run();
      }
      if (event.key === "Escape") {
        setExpanded(false);
        setSelectedNodeId(null);
      }
    };
    document.addEventListener("keydown", handler, true);
    return () => document.removeEventListener("keydown", handler, true);
  }, [run, fileBusy]);
  useEffect(() => {
    if (!playing || !steps.length) return;
    const timer = window.setInterval(
      () =>
        setPosition((previous) => {
          if (previous >= steps.length - 1) {
            setPlaying(false);
            return previous;
          }
          return previous + 1;
        }),
      speed,
    );
    return () => window.clearInterval(timer);
  }, [playing, speed, steps.length]);
  useEffect(() => {
    document.documentElement.classList.toggle("dark", dark);
    try {
      localStorage.setItem("janglim-theme", dark ? "dark" : "light");
    } catch {
      /* Storage is optional. */
    }
  }, [dark]);

  const navigate = (next: number) => {
    setPlaying(false);
    setPosition(Math.min(steps.length - 1, Math.max(0, next)));
  };
  const togglePlayback = () => {
    if (position === steps.length - 1) setPosition(0);
    setPlaying(!playing);
  };
  const entries = useMemo(
    () =>
      (session?.entries ?? []).filter((entry) =>
        `${entry.state} I${entry.state} ${entry.symbol} ${entry.kind} ${entry.target}`
          .toLowerCase()
          .includes(filter.toLowerCase()),
      ),
    [session, filter],
  );
  const copyGrammar = async () => {
    try {
      await navigator.clipboard.writeText(grammar);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1800);
    } catch {
      setCopied(false);
    }
  };
  const reset = () => {
    setGrammar(DEFAULT_GRAMMAR);
    setInput("a + a * a");
    setExample(EXAMPLES[0].id);
    void run(DEFAULT_GRAMMAR, "a + a * a");
  };
  const selectExample = (selected: (typeof EXAMPLES)[number]) => {
    setExample(selected.id);
    setGrammar(selected.grammar);
    setInput(selected.input);
    setView("tree");
    setZoom(1);
    void run(selected.grammar, selected.input);
  };
  const openWorkspace = (source: WorkspaceSource) => {
    setGrammar(source.grammar);
    setInput(source.input);
    setExample("");
    setView("tree");
    setZoom(1);
    setFilter("");
    setCopied(false);
    void run(source.grammar, source.input);
  };

  return (
    <TooltipProvider delayDuration={200}>
      <WorkspaceShell
        rail={
          <aside className="nav-rail" aria-label={t("Workspace navigation")}>
            <a
              className="brand-mark"
              href="/"
              aria-label={t("Janglim Playground")}
            >
              <GitBranch size={24} strokeWidth={2.1} />
            </a>
            <div className="rail-divider" />
            {(
              [
                { value: "tree", label: "Parse tree", icon: GitBranch },
                { value: "table", label: "Parsing table", icon: Table2 },
                { value: "trace", label: "Execution trace", icon: List },
              ] as const
            ).map((item) => (
              <Tooltip key={item.value}>
                <TooltipTrigger asChild>
                  <button
                    className={`rail-item ${view === item.value ? "active" : ""}`}
                    onClick={() => setView(item.value)}
                    aria-label={t(item.label)}
                    aria-pressed={view === item.value}
                  >
                    <item.icon size={20} />
                  </button>
                </TooltipTrigger>
                <TooltipContent side="right">{t(item.label)}</TooltipContent>
              </Tooltip>
            ))}
            <div className="rail-bottom">
              <a
                className="rail-item"
                href="https://aj-comp.github.io/Compiler/"
                target="_blank"
                rel="noreferrer"
                aria-label={t("Janglim documentation")}
              >
                <BookOpen size={20} />
              </a>
              <IconButton
                label={t(
                  dark ? "Switch to light theme" : "Switch to dark theme",
                )}
                onClick={() => setDark(!dark)}
              >
                {dark ? <Sun /> : <Moon />}
              </IconButton>
            </div>
          </aside>
        }
      >
        <div className="shell-main">
          <header className="app-header">
            <div className="wordmark">
              Janglim<span className="header-slash">/</span>
              <span className="header-location">{t("Playground")}</span>
              <span className="version-tag">v{playgroundVersion}</span>
            </div>
            <div className="header-actions">
              <div className="header-links">
                <span className="engine-pill">
                  <span />
                  {t("LALR(1) engine")}
                </span>
                <a
                  href="https://aj-comp.github.io/Compiler/"
                  target="_blank"
                  rel="noreferrer"
                >
                  {t("Documentation")} <ArrowRight size={14} />
                </a>
                <a
                  href="https://github.com/AJ-comp/Compiler"
                  target="_blank"
                  rel="noreferrer"
                  aria-label={t("Source on GitHub")}
                >
                  <GitFork size={19} />
                </a>
              </div>
              <LanguagePicker />
            </div>
          </header>
          <main className="page-content">
            <section className="page-heading">
              <div>
                <div className="eyebrow">
                  <span className="tiny-square" /> {t("GRAMMAR WORKSPACE")}
                </div>
                <h1>
                  {t("See your grammar take shape")}
                  <span>.</span>
                </h1>
                <p>
                  {t(
                    "Write a grammar. Run an input. Explore every parsing decision.",
                  )}
                </p>
              </div>
              <div className="heading-actions">
                <WorkspaceFiles
                  source={{ grammar, input }}
                  disabled={running}
                  onOpen={openWorkspace}
                  onNotice={setFileNotice}
                  onBusyChange={changeFileBusy}
                  onFocusSource={() =>
                    document.getElementById("parser-input")?.focus()
                  }
                />
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="outline" className="examples-trigger">
                      <Code2 size={16} />
                      <span>{t("Examples")}</span>
                      <ChevronDown size={14} />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="examples-menu">
                    <DropdownMenuLabel>
                      {t("Explore a grammar")}
                    </DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    {EXAMPLES.map((item) => (
                      <DropdownMenuItem
                        key={item.id}
                        onSelect={() => selectExample(item)}
                        disabled={running || fileBusy}
                      >
                        <div>
                          <strong>{t(item.name)}</strong>
                          <small>{t(item.description)}</small>
                        </div>
                        {example === item.id && <Check size={15} />}
                      </DropdownMenuItem>
                    ))}
                  </DropdownMenuContent>
                </DropdownMenu>
                <Button
                  className="run-button"
                  onClick={() => void run()}
                  disabled={running || fileBusy}
                >
                  {running ? (
                    <LoaderCircle className="spin" size={16} />
                  ) : (
                    <Play size={15} fill="currentColor" />
                  )}
                  {t(running ? "Parsing…" : "Run parser")}
                  <kbd>Ctrl ↵</kbd>
                </Button>
              </div>
            </section>

            {fileNotice && (
              <div
                className={`workspace-file-notice ${fileNotice.error ? "error" : ""}`}
                role={fileNotice.error ? "alert" : "status"}
              >
                {fileNotice.error ? (
                  <TriangleAlert size={16} />
                ) : (
                  <CheckCircle2 size={16} />
                )}
                <span>
                  {t(fileNotice.key, { name: fileNotice.name ?? "" })}
                </span>
                <Button
                  variant="ghost"
                  size="icon-sm"
                  aria-label={t("Dismiss notification")}
                  onClick={() => setFileNotice(null)}
                >
                  <X size={15} />
                </Button>
              </div>
            )}

            <div className="session-strip" role="status" aria-live="polite">
              <div
                className={`session-status ${session?.success && !dirty ? "success" : session && !dirty ? "failure" : ""}`}
              >
                {running ? (
                  <LoaderCircle size={16} className="spin" />
                ) : dirty ? (
                  <span className="status-dot" />
                ) : session?.grammarErrors.length ? (
                  <TriangleAlert size={16} />
                ) : session?.success ? (
                  <CheckCircle2 size={16} />
                ) : session ? (
                  <XCircle size={16} />
                ) : (
                  <span className="status-dot" />
                )}
                <strong>
                  {t(
                    running
                      ? "Building your parse"
                      : dirty
                        ? "Edited · run to update"
                        : session?.grammarErrors.length
                          ? "Grammar needs attention"
                          : session?.success
                            ? "Input accepted"
                            : session
                              ? "Input rejected"
                              : "Ready to parse",
                  )}
                </strong>
                {session && !dirty && !session.grammarErrors.length && (
                  <span className="status-description">
                    {t(
                      session.success
                        ? "The input matches your grammar."
                        : "Explore the trace to find the error.",
                    )}
                  </span>
                )}
              </div>
              <div className="session-metrics">
                <span>
                  {t("{count} tokens", { count: session?.tokenCount ?? "—" })}
                </span>
                <span>
                  {t("{count} states", { count: session?.stateCount ?? "—" })}
                </span>
                <span>
                  {t("{count} steps", { count: steps.length || "—" })}
                </span>
                <span className="metric-time">
                  {session ? `${session.elapsedMs.toFixed(1)} ms` : "— ms"}
                </span>
              </div>
            </div>

            {(runError || !!session?.grammarErrors.length) && (
              <div className="diagnostics" role="alert">
                <TriangleAlert size={18} />
                <div>
                  <strong>{t("Unable to run this grammar")}</strong>
                  {(runError
                    ? [
                        runError.authored
                          ? t(runError.message)
                          : runError.message,
                      ]
                    : session!.grammarErrors
                  ).map((error, index) => (
                    <p key={index}>{error}</p>
                  ))}
                </div>
              </div>
            )}
            {!!session?.conflicts.length && (
              <details className="conflict-panel">
                <summary>
                  <TriangleAlert size={15} />
                  <strong>
                    {t("Grammar conflicts: {count}", {
                      count: session.conflicts.length,
                    })}
                  </strong>
                  <span>{t("Inspect ambiguity")}</span>
                  <ChevronDown size={14} />
                </summary>
                <div>
                  <p>
                    {t(
                      "This grammar is not LALR(1). The engine continues with its default action at each conflict.",
                    )}
                  </p>
                  {session.conflicts.map((conflict, index) => (
                    <p key={index}>
                      <code>I{conflict.state}</code> {conflict.kind}
                    </p>
                  ))}
                </div>
              </details>
            )}

            <div
              className={`workspace ${expanded ? "workspace-expanded" : ""}`}
            >
              <section
                className="editor-panel panel"
                aria-labelledby="grammar-label"
              >
                <div className="panel-heading">
                  <div className="panel-title">
                    <Code2 size={16} />
                    <h2 id="grammar-label">{t("Grammar")}</h2>
                    <span className="language-label">EBNF</span>
                  </div>
                  <div>
                    <IconButton
                      label={t(copied ? "Grammar copied" : "Copy grammar")}
                      onClick={() => void copyGrammar()}
                    >
                      {copied ? <Check /> : <Copy />}
                    </IconButton>
                    <IconButton
                      label={t("Reset default example")}
                      disabled={running || fileBusy}
                      onClick={reset}
                    >
                      <RotateCcw />
                    </IconButton>
                  </div>
                </div>
                <div className="editor-file">
                  <span className="file-dot" />
                  <span>grammar.ebnf</span>
                  <small>
                    {t("{count} lines", { count: grammar.split("\n").length })}
                  </small>
                </div>
                <GrammarEditor
                  value={grammar}
                  onChange={setGrammar}
                  dark={dark}
                  highlightRanges={selectedNode?.grammarRanges}
                  selectedSymbol={selectedNode?.symbolName}
                />
                <div className="editor-hint">
                  <span>
                    <span className="syntax-dot" />
                    {t("Literals in single quotes")}
                  </span>
                  <span>{t("Regex in double quotes")}</span>
                </div>
                <div className="input-section">
                  <div className="input-heading">
                    <label htmlFor="parser-input">{t("Input string")}</label>
                    <span>{t("Plain text")}</span>
                  </div>
                  <InputEditor
                    value={input}
                    onChange={setInput}
                    onRun={() => {
                      if (!fileBusy) void run();
                    }}
                    highlightRange={selectedNode?.inputRange}
                    selectedSymbol={selectedNode?.symbolName}
                  />
                  <div className="input-footer">
                    <span>
                      {t("{count} characters", { count: input.length })}
                    </span>
                    <span>
                      <Command size={11} /> {t("Enter to run")}
                    </span>
                  </div>
                </div>
                <div className="editor-note">
                  <ShieldCheck size={15} />
                  <span>{t("Your grammar runs locally in your browser.")}</span>
                </div>
              </section>

              <section
                className="results-panel panel"
                aria-label={t("Parser results")}
              >
                <Preloader
                  loading={running}
                  variant="circle"
                  position="absolute"
                  duration={250}
                  respectReducedMotion
                  bgColor="var(--panel)"
                  loadingText={t("Building your parse…")}
                  ariaLabel={t("Building your parse…")}
                  customContent={() => (
                    <div className="parser-loading">
                      <LoaderCircle size={24} className="spin" />
                      <span>{t("Building your parse…")}</span>
                    </div>
                  )}
                />
                <Tabs
                  value={view}
                  onValueChange={(value) => setView(value as View)}
                  className="results-tabs"
                >
                  <div className="results-heading">
                    <TabsList className="view-tabs">
                      <TabsTrigger value="tree">
                        <GitBranch size={15} />
                        {t("Parse tree")}
                      </TabsTrigger>
                      <TabsTrigger value="table">
                        <Table2 size={15} />
                        {t("Parsing table")}
                        <span className="tab-count">
                          {session?.entries.length ?? 0}
                        </span>
                      </TabsTrigger>
                      <TabsTrigger value="trace">
                        <List size={15} />
                        {t("Trace")}
                        <span className="tab-count">{steps.length}</span>
                      </TabsTrigger>
                    </TabsList>
                    <IconButton
                      label={t(
                        expanded ? "Exit expanded view" : "Expand results",
                      )}
                      onClick={() => setExpanded(!expanded)}
                    >
                      <Maximize2 />
                    </IconButton>
                  </div>
                  <TabsContent value="tree" className="tree-content">
                    <div className="tree-stage">
                      <div className="stage-topline">
                        <span className="stage-label">
                          {t("CONCRETE SYNTAX TREE")}
                        </span>
                        <div className="tree-legend">
                          <span>
                            <i />
                            {t("Nonterminal")}
                          </span>
                          <span>
                            <i />
                            {t("Terminal")}
                          </span>
                        </div>
                      </div>
                      <ParseTree
                        stack={current?.stackAfter ?? []}
                        zoom={zoom}
                        selectedNodeId={selectedNode?.id}
                        onSelect={dirty || running ? undefined : selectNode}
                        onClearSelection={clearSelection}
                      />
                      <div className="tree-zoom">
                        <IconButton
                          label={t("Zoom out")}
                          onClick={() => setZoom(Math.max(0.4, zoom - 0.1))}
                          disabled={zoom <= 0.4}
                        >
                          <Minus />
                        </IconButton>
                        <button
                          onClick={() => setZoom(1)}
                          aria-label={t("Fit tree width")}
                        >
                          {Math.round(zoom * 100)}%
                        </button>
                        <IconButton
                          label={t("Zoom in")}
                          onClick={() => setZoom(Math.min(2, zoom + 0.1))}
                          disabled={zoom >= 2}
                        >
                          <Plus />
                        </IconButton>
                      </div>
                      <div className="stage-caption">
                        {current?.action?.kind === "accept" ? (
                          <>
                            <Check size={13} /> {t("Complete parse tree")}
                          </>
                        ) : (
                          <>
                            <GitBranch size={13} />{" "}
                            {t("Stack forest after the current action")}
                          </>
                        )}
                      </div>
                    </div>
                    <aside className="step-inspector">
                      <div className="inspector-heading">
                        <h3>{t("Current step")}</h3>
                        <span>
                          {steps.length
                            ? String(position + 1).padStart(2, "0")
                            : "—"}
                          <small> / {steps.length || "—"}</small>
                        </span>
                      </div>
                      <div className="current-action">
                        <ActionBadge kind={current?.action?.kind} />
                        <code>
                          {current?.action?.target ||
                            (current?.action?.kind === "accept"
                              ? t("Parsing complete")
                              : t("Run to begin"))}
                        </code>
                      </div>
                      <dl className="step-details">
                        <div>
                          <dt>{t("Lookahead")}</dt>
                          <dd>
                            <code>{current?.lookahead ?? "—"}</code>
                          </dd>
                        </div>
                        <div>
                          <dt>{t("Stack depth")}</dt>
                          <dd>{current?.stackAfter.length ?? "—"}</dd>
                        </div>
                      </dl>
                      {steps.length > 0 && (
                        <div
                          className={`symbol-selection ${selectedNode ? "has-selection" : ""}`}
                          aria-label={t("Selected symbol")}
                        >
                          {selectedNode ? (
                            <>
                              <div className="selection-heading">
                                <h3>{t("Selected symbol")}</h3>
                                <IconButton
                                  label={t("Clear symbol selection")}
                                  onClick={clearSelection}
                                >
                                  <X />
                                </IconButton>
                              </div>
                              <div className="selection-name">
                                <span>
                                  {t(
                                    selectedNode.isTerminal ? "Token" : "Rule",
                                  )}
                                </span>
                                <code>
                                  {selectedNode.symbolName ||
                                    selectedNode.label ||
                                    "ε"}
                                </code>
                              </div>
                              {selectedNode.isTerminal &&
                                selectedNode.label !==
                                  selectedNode.symbolName && (
                                  <code className="selection-lexeme">
                                    {JSON.stringify(selectedNode.label)}
                                  </code>
                                )}
                              <p aria-live="polite">
                                {selectedNode.inputRange
                                  ? t("Input · characters {start}–{end}", {
                                      start: selectedNode.inputRange.start + 1,
                                      end: selectedNode.inputRange.end,
                                    })
                                  : t("No consumed input")}
                              </p>
                              <p>
                                {selectedNode.grammarRanges.length
                                  ? t("Grammar locations · {count}", {
                                      count: selectedNode.grammarRanges.length,
                                    })
                                  : t(
                                      "Generated symbol · no direct grammar location",
                                    )}
                              </p>
                            </>
                          ) : (
                            <p>
                              {t(
                                dirty
                                  ? "Run again to link the tree to your edits."
                                  : "Select a tree node to highlight its grammar and input.",
                              )}
                            </p>
                          )}
                        </div>
                      )}
                      {current?.isError && (
                        <div className="step-error">
                          <TriangleAlert size={14} />
                          {current.errorMessage}
                        </div>
                      )}
                      {current?.recoveryMessage && (
                        <p className="recovery-message">
                          {current.recoveryMessage}
                        </p>
                      )}
                      <div className="stack-heading">
                        <Layers3 size={14} />
                        <h3>{t("Parser stack")}</h3>
                        <span>
                          {t("TOP")} <ArrowDown size={10} />
                        </span>
                      </div>
                      <div className="stack-list">
                        {current?.stackAfter.length ? (
                          [...current.stackAfter]
                            .reverse()
                            .map((frame, index) => (
                              <div className="stack-frame" key={index}>
                                <code className="state-id">I{frame.state}</code>
                                <code
                                  className={
                                    frame.symbol?.isTerminal
                                      ? "terminal-label"
                                      : ""
                                  }
                                >
                                  {frame.symbol?.label || "∅"}
                                </code>
                                {index === 0 && (
                                  <span className="stack-top-dot" />
                                )}
                              </div>
                            ))
                        ) : (
                          <div className="stack-empty">
                            {t("The stack appears after parsing.")}
                          </div>
                        )}
                      </div>
                      <div className="inspector-footer">
                        <span className="tiny-square" />{" "}
                        {t("LALR(1) · after action")}
                      </div>
                    </aside>
                  </TabsContent>
                  <TabsContent value="table" className="data-content">
                    <div className="data-toolbar">
                      <div>
                        <h3>ACTION / GOTO</h3>
                        <p>
                          {t(
                            "Every transition in the generated parsing table.",
                          )}
                        </p>
                      </div>
                      <div className="search-field">
                        <Search size={15} />
                        <input
                          aria-label={t("Filter parsing table")}
                          placeholder={t("Filter state, symbol or action…")}
                          value={filter}
                          onChange={(event) => setFilter(event.target.value)}
                        />
                      </div>
                    </div>
                    <div className="table-scroll">
                      <table>
                        <thead>
                          <tr>
                            <th>{t("State")}</th>
                            <th>{t("Symbol")}</th>
                            <th>{t("Action")}</th>
                            <th>{t("Target / production")}</th>
                          </tr>
                        </thead>
                        <tbody>
                          {entries.map((entry, index) => (
                            <tr key={index}>
                              <td>
                                <code className="state-id">I{entry.state}</code>
                              </td>
                              <td>
                                <code>{entry.symbol}</code>
                              </td>
                              <td>
                                <ActionBadge kind={entry.kind} />
                              </td>
                              <td>
                                <code>{entry.target || "—"}</code>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                      {!entries.length && (
                        <div className="table-empty">
                          {t(
                            session?.entries.length
                              ? "No entries match your filter."
                              : "Run a valid grammar to generate its parsing table.",
                          )}
                        </div>
                      )}
                    </div>
                    <div className="data-footer">
                      {t("{count} of {total} entries", {
                        count: entries.length,
                        total: session?.entries.length ?? 0,
                      })}
                      <span>
                        {t("{count} states", {
                          count: session?.stateCount ?? 0,
                        })}
                      </span>
                    </div>
                  </TabsContent>
                  <TabsContent value="trace" className="data-content">
                    <div className="data-toolbar">
                      <div>
                        <h3>{t("Execution trace")}</h3>
                        <p>
                          {t("Select a step to inspect its tree and stack.")}
                        </p>
                      </div>
                      <span className="trace-total">
                        {t("{count} steps", { count: steps.length })}
                      </span>
                    </div>
                    <div className="table-scroll">
                      <table className="trace-table">
                        <thead>
                          <tr>
                            <th>{t("Step")}</th>
                            <th>{t("Action")}</th>
                            <th>{t("Lookahead")}</th>
                            <th>{t("Target / production")}</th>
                            <th>{t("Stack")}</th>
                          </tr>
                        </thead>
                        <tbody>
                          {steps.map((step, index) => (
                            <tr
                              key={index}
                              className={`${position === index ? "selected-row" : ""} ${step.isError ? "error-row" : ""}`}
                            >
                              <td>
                                <button
                                  aria-label={t("Go to step {step}", {
                                    step: index + 1,
                                  })}
                                  onClick={() => navigate(index)}
                                  className="trace-step-button"
                                >
                                  {position === index && (
                                    <ChevronRight size={12} />
                                  )}
                                  <span>
                                    {String(index + 1).padStart(2, "0")}
                                  </span>
                                </button>
                              </td>
                              <td>
                                <ActionBadge kind={step.action?.kind} />
                              </td>
                              <td>
                                <code>{step.lookahead ?? "—"}</code>
                              </td>
                              <td>
                                <button
                                  className="trace-target-button"
                                  onClick={() => {
                                    navigate(index);
                                    setView("tree");
                                  }}
                                  aria-label={t("Inspect tree at step {step}", {
                                    step: index + 1,
                                  })}
                                >
                                  <code>
                                    {step.action?.target ||
                                      (step.isError
                                        ? step.errorMessage
                                        : step.action?.kind === "accept"
                                          ? t("Parsing complete")
                                          : "—")}
                                  </code>
                                </button>
                                {step.recoveryMessage && (
                                  <small className="recovery-message">
                                    {step.recoveryMessage}
                                  </small>
                                )}
                              </td>
                              <td>
                                <code>
                                  {step.stackAfter
                                    .map((frame) => `I${frame.state}`)
                                    .join(" · ")}
                                </code>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                      {!steps.length && (
                        <div className="table-empty">
                          {t("Run a grammar to explore the execution trace.")}
                        </div>
                      )}
                    </div>
                    <div className="data-footer">
                      {t("{count} decisions", { count: steps.length })}
                      <span>
                        {t("Step {step} selected", {
                          step: steps.length ? position + 1 : 0,
                        })}
                      </span>
                    </div>
                  </TabsContent>
                </Tabs>
                <div className="playback-bar">
                  <div className="transport">
                    <IconButton
                      label={t("First step")}
                      onClick={() => navigate(0)}
                      disabled={!steps.length || position === 0}
                    >
                      <SkipBack />
                    </IconButton>
                    <IconButton
                      label={t("Previous step")}
                      onClick={() => navigate(position - 1)}
                      disabled={!steps.length || position === 0}
                    >
                      <StepBack />
                    </IconButton>
                    <Button
                      className="play-toggle"
                      size="icon"
                      aria-label={t(playing ? "Pause playback" : "Play trace")}
                      onClick={togglePlayback}
                      disabled={!steps.length}
                    >
                      {playing ? (
                        <Pause size={15} fill="currentColor" />
                      ) : (
                        <Play size={15} fill="currentColor" />
                      )}
                    </Button>
                    <IconButton
                      label={t("Next step")}
                      onClick={() => navigate(position + 1)}
                      disabled={!steps.length || position === steps.length - 1}
                    >
                      <StepForward />
                    </IconButton>
                    <IconButton
                      label={t("Last step")}
                      onClick={() => navigate(steps.length - 1)}
                      disabled={!steps.length || position === steps.length - 1}
                    >
                      <SkipForward />
                    </IconButton>
                  </div>
                  <div className="timeline">
                    <input
                      type="range"
                      aria-label={t("Parsing step")}
                      min={0}
                      max={Math.max(0, steps.length - 1)}
                      value={position}
                      disabled={!steps.length}
                      onChange={(event) => navigate(Number(event.target.value))}
                      style={
                        {
                          "--progress": `${steps.length > 1 ? (position / (steps.length - 1)) * 100 : 0}%`,
                        } as React.CSSProperties
                      }
                    />
                    <span>
                      <b>{steps.length ? position + 1 : 0}</b> / {steps.length}
                    </span>
                  </div>
                  <select
                    aria-label={t("Playback speed")}
                    value={speed}
                    onChange={(event) => setSpeed(Number(event.target.value))}
                  >
                    <option value={1200}>0.5×</option>
                    <option value={600}>1×</option>
                    <option value={250}>2.4×</option>
                  </select>
                </div>
              </section>
            </div>
            <footer className="page-footer">
              <span>
                <span className="status-dot" />{" "}
                {t("Janglim · an open source parser engine")}
              </span>
              <span>
                EBNF <ChevronRight size={11} /> {t("Lexer")}{" "}
                <ChevronRight size={11} /> {t("LALR parser")}{" "}
                <ChevronRight size={11} /> {t("Syntax tree")}
              </span>
            </footer>
          </main>
        </div>
      </WorkspaceShell>
    </TooltipProvider>
  );
}
