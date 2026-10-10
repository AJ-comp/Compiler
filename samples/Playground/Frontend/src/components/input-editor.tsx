import { SourceTextarea, type SourceRange } from "./source-textarea";
import { useI18n } from "../i18n";

export type InputEditorProps = {
  value: string;
  onChange(value: string): void;
  onRun(): void;
  highlightRange?: SourceRange | null;
  selectedSymbol?: string;
};

export function InputEditor({
  value,
  onChange,
  onRun,
  highlightRange,
  selectedSymbol,
}: InputEditorProps) {
  const { t } = useI18n();
  return (
    <SourceTextarea
      id="parser-input"
      ariaLabel={t("Input string")}
      value={value}
      onChange={onChange}
      ranges={highlightRange ? [highlightRange] : []}
      selectedSymbol={selectedSymbol}
      rows={3}
      placeholder={t("Enter a string to parse…")}
      onKeyDown={(event) => {
        if (
          !event.defaultPrevented &&
          event.key === "Enter" &&
          !event.shiftKey &&
          !event.nativeEvent.isComposing &&
          event.nativeEvent.keyCode !== 229
        ) {
          event.preventDefault();
          onRun();
        }
      }}
    />
  );
}
