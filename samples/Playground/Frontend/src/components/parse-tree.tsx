import { useId, useMemo, useLayoutEffect, useRef, useState } from "react";
import { GitBranch } from "lucide-react";
import { useI18n } from "../i18n";

export interface TreeNode {
  id: string;
  label: string;
  symbolName: string;
  isTerminal: boolean;
  inputRange: SourceRange | null;
  grammarRanges: SourceRange[];
  children: TreeNode[];
}

/** UTF-16 offsets; end is exclusive, matching Monaco and textarea selections. */
export interface SourceRange {
  start: number;
  end: number;
}

export interface StackFrame {
  state: number;
  symbol: TreeNode | null;
}

interface ParseTreeProps {
  stack: StackFrame[];
  zoom: number;
  selectedNodeId?: string | null;
  onSelect?: (node: TreeNode) => void;
  onClearSelection?: () => void;
}

interface PlacedNode {
  node: TreeNode;
  key: string;
  label: string;
  x: number;
  y: number;
  width: number;
  height: number;
}

interface TreeEdge {
  key: string;
  from: PlacedNode;
  to: PlacedNode;
}

const SIBLING_GAP = 26;
const ROOT_GAP = 62;
const LEVEL_GAP = 88;
const SIDE_PADDING = 40;
const ROOT_Y = 72;

function visibleLabel(label: string) {
  return label.replace(/\r\n|\r|\n/g, "\\n").replace(/\t/g, "\\t");
}

type Translate = (
  message: string,
  values?: Record<string, string | number>,
) => string;

function nodeDescription(node: TreeNode, t: Translate) {
  const kind = t(node.isTerminal ? "Token" : "Nonterminal");
  const symbol =
    node.symbolName && node.symbolName !== node.label
      ? ` ${node.symbolName}`
      : "";
  const values = { kind, symbol, label: node.label || "ε", id: node.id };
  return node.inputRange
    ? t(
        "{kind}{symbol}: {label}; input offsets {start} to {end}, end exclusive; node {id}",
        {
          ...values,
          start: node.inputRange.start,
          end: node.inputRange.end,
        },
      )
    : t("{kind}{symbol}: {label}; node {id}", values);
}

function labelWidth(label: string) {
  // Latin labels use the same monospace font as the editor. Allow extra space
  // for Korean, Japanese, Chinese and emoji so token text remains untruncated.
  return Array.from(label).reduce((width, character) => {
    const point = character.codePointAt(0) ?? 0;
    return width + (point > 0x2ff ? 14 : 8);
  }, 0);
}

function makeLayout(roots: { node: TreeNode; state: number }[]) {
  const subtreeWidths = new WeakMap<TreeNode, number>();
  const nodeWidths = new WeakMap<TreeNode, number>();
  const nodes: PlacedNode[] = [];
  const edges: TreeEdge[] = [];
  const rootPositions: { x: number; state: number; key: string }[] = [];
  let deepestLevel = 0;

  function measure(node: TreeNode): number {
    const ownWidth = Math.max(88, labelWidth(visibleLabel(node.label)) + 32);
    nodeWidths.set(node, ownWidth);
    const childrenWidth =
      node.children.reduce((sum, child) => sum + measure(child), 0) +
      Math.max(0, node.children.length - 1) * SIBLING_GAP;
    const width = Math.max(ownWidth, childrenWidth);
    subtreeWidths.set(node, width);
    return width;
  }

  function place(
    node: TreeNode,
    left: number,
    depth: number,
    key: string,
  ): PlacedNode {
    const subtreeWidth = subtreeWidths.get(node)!;
    const placed: PlacedNode = {
      node,
      key,
      label: visibleLabel(node.label),
      x: left + subtreeWidth / 2,
      y: ROOT_Y + depth * LEVEL_GAP,
      width: nodeWidths.get(node)!,
      height: 48,
    };
    nodes.push(placed);
    deepestLevel = Math.max(deepestLevel, depth);

    const childrenWidth =
      node.children.reduce((sum, child) => sum + subtreeWidths.get(child)!, 0) +
      Math.max(0, node.children.length - 1) * SIBLING_GAP;
    let childLeft = left + (subtreeWidth - childrenWidth) / 2;
    node.children.forEach((child, index) => {
      const childKey = `${key}.${index}`;
      const childNode = place(child, childLeft, depth + 1, childKey);
      edges.push({ key: childKey, from: placed, to: childNode });
      childLeft += subtreeWidths.get(child)! + SIBLING_GAP;
    });
    return placed;
  }

  let cursor = SIDE_PADDING;
  roots.forEach(({ node, state }, index) => {
    const width = measure(node);
    const key = `root-${index}`;
    const placed = place(node, cursor, 0, key);
    rootPositions.push({ x: placed.x, state, key });
    cursor += width + ROOT_GAP;
  });

  return {
    nodes,
    edges,
    rootPositions,
    width: Math.max(360, cursor - ROOT_GAP + SIDE_PADDING),
    height: ROOT_Y + deepestLevel * LEVEL_GAP + 64,
  };
}

function AccessibleTreeNode({ node, t }: { node: TreeNode; t: Translate }) {
  return (
    <li>
      <span>{nodeDescription(node, t)}</span>
      {node.children.length > 0 && (
        <ul>
          {node.children.map((child, index) => (
            <AccessibleTreeNode
              key={`${child.id}-${index}`}
              node={child}
              t={t}
            />
          ))}
        </ul>
      )}
    </li>
  );
}

export function ParseTree({
  stack,
  zoom,
  selectedNodeId,
  onSelect,
  onClearSelection,
}: ParseTreeProps) {
  const { t } = useI18n();
  const shadowId = `tree-shadow-${useId().replace(/:/g, "")}`;
  const instructionsId = `tree-instructions-${useId().replace(/:/g, "")}`;
  const viewport = useRef<HTMLDivElement>(null);
  const [available, setAvailable] = useState({ width: 800, height: 600 });
  useLayoutEffect(() => {
    if (!viewport.current) return;
    const observer = new ResizeObserver((entries) => {
      const box = entries[0].contentRect;
      setAvailable({ width: box.width, height: box.height });
    });
    observer.observe(viewport.current);
    return () => observer.disconnect();
  }, [stack.length]);
  const roots = useMemo(
    () =>
      stack.flatMap((frame) =>
        frame.symbol ? [{ node: frame.symbol, state: frame.state }] : [],
      ),
    [stack],
  );
  const layout = useMemo(() => makeLayout(roots), [roots]);
  const fitted = Math.max(
    0.35,
    Math.min(1, (available.width - 20) / layout.width),
  );
  const scale = fitted * (Number.isFinite(zoom) && zoom > 0 ? zoom : 1);

  if (!roots.length) {
    return (
      <div className="tree-viewport tree-empty" ref={viewport}>
        <div className="tree-empty-icon">
          <GitBranch size={26} strokeWidth={1.5} aria-hidden="true" />
        </div>
        <h3>{t("Your parse tree starts here")}</h3>
        <p>{t("Run a grammar to explore each shift and reduction.")}</p>
      </div>
    );
  }

  return (
    <div className="tree-viewport" ref={viewport}>
      <div className="tree-forest-count" aria-live="polite">
        {t(roots.length === 1 ? "{count} tree" : "{count} trees", {
          count: roots.length,
        })}{" "}
        <span>· {t("{count} nodes", { count: layout.nodes.length })}</span>
      </div>
      {!onSelect && (
        <div className="sr-only">
          <h3>{t("Concrete syntax tree forest")}</h3>
          <ul aria-label={t("Parse tree roots and their children")}>
            {roots.map(({ node, state }, index) => (
              <li key={`${node.id}-${index}`}>
                <span>{t("Stack state I{state}", { state })}</span>
                <ul>
                  <AccessibleTreeNode node={node} t={t} />
                </ul>
              </li>
            ))}
          </ul>
        </div>
      )}
      {onSelect && (
        <p id={instructionsId} className="sr-only">
          {t(
            "Select a tree node to highlight its grammar and input source. Use Enter or Space to select a focused node, and Escape to clear selection. Token offsets identify repeated input tokens; the end offset is exclusive.",
          )}
        </p>
      )}
      <svg
        className="parse-tree"
        width={layout.width * scale}
        height={layout.height * scale}
        viewBox={`0 0 ${layout.width} ${layout.height}`}
        role={onSelect ? "group" : undefined}
        aria-hidden={onSelect ? undefined : true}
        aria-label={
          onSelect
            ? t(
                roots.length === 1
                  ? "Concrete syntax tree forest with {count} root and {nodes} nodes"
                  : "Concrete syntax tree forest with {count} roots and {nodes} nodes",
                { count: roots.length, nodes: layout.nodes.length },
              )
            : undefined
        }
        aria-describedby={onSelect ? instructionsId : undefined}
        onKeyDown={
          onClearSelection
            ? (event) => {
                if (event.key === "Escape" && selectedNodeId) {
                  event.preventDefault();
                  event.stopPropagation();
                  onClearSelection();
                }
              }
            : undefined
        }
      >
        <defs>
          <filter id={shadowId} x="-20%" y="-30%" width="140%" height="170%">
            <feDropShadow
              dx="0"
              dy="2"
              stdDeviation="2"
              floodColor="#111827"
              floodOpacity="0.04"
            />
          </filter>
        </defs>
        {layout.edges.map(({ key, from, to }) => {
          const startY = from.y + from.height / 2;
          const endY = to.y - to.height / 2;
          const middle = (startY + endY) / 2;
          return (
            <path
              key={key}
              className="tree-edge"
              aria-hidden="true"
              d={`M ${from.x} ${startY} C ${from.x} ${middle}, ${to.x} ${middle}, ${to.x} ${endY}`}
              fill="none"
              stroke="var(--tree-edge-color, #d3d6de)"
              strokeWidth="1.5"
            />
          );
        })}
        {layout.rootPositions.map((root) => (
          <text
            key={root.key}
            className="tree-root-state"
            aria-hidden={onSelect ? true : undefined}
            x={root.x}
            y={29}
            textAnchor="middle"
            fill="var(--muted-foreground, #818898)"
            fontSize="10"
            fontFamily="inherit"
          >
            {t("State {state}", { state: root.state })}
          </text>
        ))}
        {layout.nodes.map(({ node, key, label, x, y, width, height }) => (
          <g
            key={key}
            className={[
              node.isTerminal ? "tree-node-terminal" : "tree-node-nonterminal",
              onSelect ? "tree-node-interactive" : "",
              selectedNodeId === node.id ? "tree-node-selected" : "",
            ]
              .filter(Boolean)
              .join(" ")}
            data-node-id={node.id}
            data-symbol-name={node.symbolName}
            data-input-start={node.inputRange?.start}
            data-input-end={node.inputRange?.end}
            transform={`translate(${x - width / 2}, ${y - height / 2})`}
            role={onSelect ? "button" : undefined}
            tabIndex={onSelect ? 0 : undefined}
            aria-pressed={onSelect ? selectedNodeId === node.id : undefined}
            aria-label={onSelect ? nodeDescription(node, t) : undefined}
            onClick={onSelect ? () => onSelect(node) : undefined}
            onKeyDown={
              onSelect
                ? (event) => {
                    if (event.key === "Enter" || event.key === " ") {
                      event.preventDefault();
                      onSelect(node);
                    }
                  }
                : undefined
            }
          >
            <title>{nodeDescription(node, t)}</title>
            <rect
              width={width}
              height={height}
              rx="9"
              fill={
                node.isTerminal
                  ? "var(--tree-terminal-fill, #f1edff)"
                  : "var(--panel, #ffffff)"
              }
              stroke={
                node.isTerminal
                  ? "var(--tree-terminal-border, #dfd5fc)"
                  : "var(--border, #e4e7ec)"
              }
              filter={`url(#${shadowId})`}
            />
            <text
              x={width / 2}
              y={14}
              textAnchor="middle"
              fontSize="8"
              fontFamily="inherit"
              fontWeight="600"
              letterSpacing="0.7"
              fill={
                node.isTerminal
                  ? "var(--tree-terminal-text)"
                  : "var(--soft-text)"
              }
            >
              {t(node.isTerminal ? "TOKEN" : "RULE")}
            </text>
            <text
              x={width / 2}
              y={32}
              textAnchor="middle"
              fontSize="13"
              fontFamily="'JetBrains Mono', 'Cascadia Code', ui-monospace, monospace"
              fontWeight={node.isTerminal ? "500" : "600"}
              fill={
                node.isTerminal
                  ? "var(--tree-terminal-text, #6d45bd)"
                  : "var(--foreground, #344054)"
              }
            >
              {label || "ε"}
            </text>
          </g>
        ))}
      </svg>
    </div>
  );
}
