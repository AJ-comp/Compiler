import { createRoot, type Root } from "react-dom/client";
import { Playground } from "./playground";
import { I18nProvider } from "./i18n";
import "./styles.css";

export type ParserBridge = {
  invokeMethodAsync(
    method: "Parse",
    grammar: string,
    input: string,
  ): Promise<string>;
};
const roots = new Map<HTMLElement, Root>();
export function mountPlayground(element: HTMLElement, bridge: ParserBridge) {
  roots.get(element)?.unmount();
  const root = createRoot(element);
  roots.set(element, root);
  root.render(
    <I18nProvider>
      <Playground bridge={bridge} />
    </I18nProvider>,
  );
}
export function unmountPlayground(element: HTMLElement) {
  roots.get(element)?.unmount();
  roots.delete(element);
}
