import {
  useId,
  useLayoutEffect,
  useRef,
  type KeyboardEventHandler,
  type ReactNode,
} from "react";
import { useI18n } from "../i18n";

/** UTF-16 source offsets; end is exclusive, as in String.prototype.slice. */
export type SourceRange = { start: number; end: number };

export function validSourceRanges(value: string, ranges: SourceRange[] = []) {
  const sorted = ranges
    .filter(
      ({ start, end }) =>
        Number.isInteger(start) &&
        Number.isInteger(end) &&
        start >= 0 &&
        end > start &&
        end <= value.length,
    )
    .map((range) => ({ ...range }))
    .sort((a, b) => a.start - b.start || a.end - b.end);
  const merged: SourceRange[] = [];
  for (const range of sorted) {
    const previous = merged[merged.length - 1];
    if (previous && range.start <= previous.end)
      previous.end = Math.max(previous.end, range.end);
    else merged.push(range);
  }
  return merged;
}

function highlightedSource(value: string, ranges: SourceRange[]) {
  const fragments: ReactNode[] = [];
  let cursor = 0;
  for (const range of ranges) {
    if (range.start > cursor) fragments.push(value.slice(cursor, range.start));
    fragments.push(
      <mark
        className="source-range-highlight"
        key={`${range.start}:${range.end}`}
      >
        {value.slice(range.start, range.end)}
      </mark>,
    );
    cursor = range.end;
  }
  fragments.push(value.slice(cursor));
  return fragments;
}

type SourceTextareaProps = {
  value: string;
  onChange(value: string): void;
  ranges?: SourceRange[];
  selectedSymbol?: string;
  id?: string;
  ariaLabel: string;
  rows?: number;
  placeholder?: string;
  className?: string;
  textareaClassName?: string;
  onKeyDown?: KeyboardEventHandler<HTMLTextAreaElement>;
  announce?: boolean;
};

/** The native textarea keeps focus, caret, selection, scrolling, and resizing. */
export function SourceTextarea({
  value,
  onChange,
  ranges = [],
  selectedSymbol,
  id,
  ariaLabel,
  rows = 3,
  placeholder,
  className = "",
  textareaClassName = "",
  onKeyDown,
  announce = true,
}: SourceTextareaProps) {
  const { t } = useI18n();
  const textarea = useRef<HTMLTextAreaElement>(null);
  const overlay = useRef<HTMLDivElement>(null);
  const content = useRef<HTMLDivElement>(null);
  const statusId = useId();
  const validRanges = validSourceRanges(value, ranges);
  const rangeKey = validRanges
    .map(({ start, end }) => `${start}:${end}`)
    .join(",");

  const syncScroll = () => {
    if (textarea.current && content.current)
      content.current.style.transform = `translate(${-textarea.current.scrollLeft}px, ${-textarea.current.scrollTop}px)`;
  };
  const measure = () => {
    const field = textarea.current;
    const viewport = overlay.current;
    const text = content.current;
    if (!field || !viewport || !text) return;
    const style = window.getComputedStyle(field);
    // clientWidth excludes the scrollbar; the overlay therefore wraps at the
    // same column as the native textarea, including after manual resizing.
    viewport.style.width = `${field.clientWidth}px`;
    viewport.style.height = `${field.clientHeight}px`;
    viewport.style.left = `${field.offsetLeft + field.clientLeft}px`;
    viewport.style.top = `${field.offsetTop + field.clientTop}px`;
    for (const property of [
      "font-family",
      "font-size",
      "font-weight",
      "font-style",
      "font-variant",
      "line-height",
      "letter-spacing",
      "text-transform",
      "text-indent",
      "text-align",
      "direction",
      "tab-size",
      "padding-top",
      "padding-right",
      "padding-bottom",
      "padding-left",
    ])
      text.style.setProperty(property, style.getPropertyValue(property));
    syncScroll();
  };

  useLayoutEffect(() => {
    measure();
  });
  useLayoutEffect(() => {
    const field = textarea.current;
    if (!field) return;
    const observer =
      typeof ResizeObserver === "undefined"
        ? null
        : new ResizeObserver(measure);
    observer?.observe(field);
    window.addEventListener("resize", measure);
    return () => {
      observer?.disconnect();
      window.removeEventListener("resize", measure);
    };
  }, []);
  useLayoutEffect(() => {
    const field = textarea.current;
    const viewport = overlay.current;
    const firstMark = content.current?.querySelector("mark");
    if (!field || !viewport || !firstMark) return;
    const lines = Array.from(firstMark.getClientRects()).filter(
      (rect) => rect.height > 0,
    );
    if (lines.length === 0) return;
    const bounds = viewport.getBoundingClientRect();
    const inset = Math.min(4, bounds.height / 4, bounds.width / 4);
    const fullRange = firstMark.getBoundingClientRect();
    // Show the whole marked span when it fits. For a wrapped/multiline span
    // taller than the viewport, reveal its first line instead of its last line.
    const target =
      fullRange.height <= bounds.height - inset * 2 ? fullRange : lines[0];
    const top = bounds.top + inset;
    const bottom = bounds.bottom - inset;
    const left = bounds.left + inset;
    const right = bounds.right - inset;
    if (target.top < top) field.scrollTop += target.top - top;
    else if (target.bottom > bottom) field.scrollTop += target.bottom - bottom;
    if (target.left < left) field.scrollLeft += target.left - left;
    else if (target.right > right) field.scrollLeft += target.right - right;
    syncScroll();
    // Scroll only for a new source/selection, so other renders and manual
    // scrolling do not keep pulling the reader back to the highlighted span.
  }, [value, rangeKey, selectedSymbol]);

  return (
    <div
      className={`source-textarea ${className}`.trim()}
      data-highlighted={validRanges.length > 0 || undefined}
    >
      <div className="source-textarea-overlay" ref={overlay} aria-hidden="true">
        <div className="source-textarea-content" ref={content}>
          {highlightedSource(value, validRanges)}
          {"\n"}
        </div>
      </div>
      <textarea
        ref={textarea}
        id={id}
        aria-label={ariaLabel}
        aria-describedby={announce ? statusId : undefined}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        onScroll={syncScroll}
        onKeyDown={onKeyDown}
        rows={rows}
        wrap="soft"
        spellCheck={false}
        placeholder={placeholder}
        className={`source-textarea-control ${textareaClassName}`.trim()}
      />
      {announce && (
        <span
          className="source-highlight-status"
          id={statusId}
          role="status"
          aria-live="polite"
          aria-atomic="true"
        >
          {selectedSymbol && validRanges.length > 0
            ? t("Highlighted source for {symbol}.", { symbol: selectedSymbol })
            : ""}
        </span>
      )}
    </div>
  );
}
